using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode.Components;
using TMPro;
using static NetcodeLobby;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Services.Matchmaker.Models;
using Unity.VisualScripting;
using static SpecialEffectItemClass;

public class CarController : HitteableBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float maxAcel;
    [SerializeField] private float turnForce;
    [SerializeField] private float turnForceDrifting;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private float groundChkRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TextMeshProUGUI m_PlayerNameTxt;

    [SerializeField] private GameObject m_Cam;

    [SerializeField] private Image m_ItemIcon;

    [SerializeField] private GameObject m_AimMarkerPrefab;
    [SerializeField] private GameObject m_ShieldPrefab;
    [SerializeField] private Transform m_ShootingTransform;
    [SerializeField] private Transform m_ShieldTransform;

    [SerializeField] private float stunTime;

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private GameObject m_AimMarker;
    private GameObject shieldVisual;

    private float turnInput;
    private bool accelerationInput;
    private bool driftingInput;
    private bool canControl = true;

    private int shieldHits = 0;
    private bool isInvulnerable;

    private Rigidbody rb;

    private ItemClass m_ItemObtained;

    private List<HitteableBehaviour> m_Hitteables;

    private HitteableBehaviour m_ActualObjective = null;

    public override void OnEnable()
    {
        base.OnEnable();
        playerName.OnValueChanged += OnNameChanged;

        m_AimMarker = Instantiate(m_AimMarkerPrefab, Vector3.zero, Quaternion.identity);
        m_AimMarker.SetActive(false);
        shieldVisual = Instantiate(m_ShieldPrefab, m_ShieldTransform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.SetActive(false);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerName.OnValueChanged -= OnNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (!IsOwner) return;

        if (Camera.main != null)
            Camera.main.gameObject.SetActive(false);

        m_Cam.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!IsOwner || NetcodeLobby.instance.GameStarted.Value == false) return;

        Vector3 velLocal = transform.InverseTransformDirection(rb.linearVelocity);

        if (CheckGround())
        {
            //Limita velocidad hacia delante
            if (velLocal.z < maxAcel && accelerationInput)
            {
                rb.AddRelativeForce(Vector3.forward * speed);
            }

            float force = driftingInput ? turnForceDrifting : turnForce;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, force * turnInput * Time.deltaTime, 0f));
        }

        velLocal.x = 0;
        rb.linearVelocity = transform.TransformDirection(velLocal);

        if (m_ItemObtained != null)
        {
            if (m_ItemObtained.GetShootItem() != null)
            {
                if (m_ItemObtained.GetShootItem().isHoming)
                {
                    AimObjective();
                }
            }
        }
    }

    private bool CheckGround()
    {
        if (Physics.OverlapSphere(m_GroundCheck.position, groundChkRadius, groundLayer) != null)
        {
            return true;
        }

        return false;
    }

    public void Teleport(Transform _NewPos)
    {
        GetComponent<NetworkTransform>().Teleport(
            _NewPos.position,
            _NewPos.rotation,
            transform.localScale
        );
    }

    public void SetHitteables()
    {
        m_Hitteables = GetAllExcept(this);
    }

    public override void OnHit()
    {
        base.OnHit();

        OnHitCoroutine();
    }

    private IEnumerator OnHitCoroutine()
    {
        if (isInvulnerable) yield return null;

        if(shieldHits > 0)
        {
            shieldHits--;
            
            if(shieldHits <= 0)
            {
                ChangeShieldClientRpc(false);
            }

            yield return null;
        }

        canControl = false;
        yield return new WaitForSeconds(stunTime);
        canControl = true;
    }

    #region Items

    public void ReceiveItem(ItemClass item)
    {
        Debug.Log("Recibiendo item");

        if (m_ItemIcon == null)
            Debug.LogError("ItemIcon no asignado en el inspector");

        if (item == null)
            Debug.LogError("Item recibido es null");

        m_ItemObtained = item;

        if (m_ItemIcon != null && item != null)
            m_ItemIcon.sprite = item.itemIcon;
    }
    //IA tiene item
    public bool HasItem()
    {
        return m_ItemObtained != null;
    }

    public void UseItem()
    {
        m_ItemObtained.UseItem(this);

        if (m_ItemObtained.GetShootItem() != null)
        {
            ShootItemClass projectile = m_ItemObtained.GetShootItem();
            
            Shoot(projectile, m_ActualObjective);
        }
        else if(m_ItemObtained.GetSpecialEffectItem() != null)
        {
            ActivateSpecialEffectItem(m_ItemObtained.GetSpecialEffectItem());
        }
        else
        {
        }
        RemoveItem();
    }

    private void ActivateSpecialEffectItem(SpecialEffectItemClass item)
    {
        if(item.type == EffectType.Shield)
        {
            ActiveShieldServerRpc(item.duration, item.shieldHits);
        }
    }

    private IEnumerator ActivateShield(float duration)
    {
        yield return new WaitForSeconds(duration);
        shieldHits = 0;
        ChangeShieldClientRpc(false);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void ActiveShieldServerRpc(float duration, int hits)
    {
        shieldHits = hits;

        ChangeShieldClientRpc(true);

        StartCoroutine(ActivateShield(duration));
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeShieldClientRpc(bool activated)
    {
        shieldVisual.SetActive(activated);
    }

    private HitteableBehaviour GetNearestHitteableObject()
    {
        float max = float.PositiveInfinity;
        HitteableBehaviour nearestObjective = null;

        foreach (HitteableBehaviour item in GetAllExcept(this))
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if (distance < max)
            {
                max = distance;
                nearestObjective = item;
            }
        }

        return nearestObjective;
    }

    public void AimObjective()
    {
        HitteableBehaviour nearestObjective = GetNearestHitteableObject();

        Vector3 playerVector = transform.forward;
        playerVector.y = 0;
        Vector3 playerToNearestObjective = (nearestObjective.transform.position - transform.position).normalized;
        playerToNearestObjective.y = 0;
        float dotProduct = Vector3.Dot(playerVector, playerToNearestObjective);

        //Debug.Log(nearestObjective.name + " en la posicion " + nearestObjective.transform.position + " con punto " + dotProduct);
        if (dotProduct > 0.6f)
        {
            Debug.Log("Objetivo en la mira");
            m_AimMarker.SetActive(true);
            m_AimMarker.transform.position = nearestObjective.m_MarkerPosition.position;
            m_ActualObjective = nearestObjective;
        }
        else
        {
            if (m_AimMarker.activeSelf)
            {
                m_AimMarker.SetActive(false);
            }
            
            m_ActualObjective = null;
        }
    }

    public void Shoot(ShootItemClass m_Projectile, HitteableBehaviour m_Objective)
    {
        Debug.Log("Disparando objeto");

        NetworkObjectReference targetRef = default;

        if (m_Objective != null)
            targetRef = new NetworkObjectReference(m_Objective.NetworkObject);

        ShootServerRpc(m_Projectile.itemId, targetRef, transform.forward);
    }

    private void RemoveItem()
    {
        m_ItemIcon.sprite = null;
        m_ItemObtained = null;
        m_AimMarker.SetActive(false);
        m_ActualObjective = null;
    }

    #endregion

    #region Inputs

    public void TurnInput(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!canControl) return;

        turnInput = ctx.ReadValue<Vector2>().x;
    }

    public void DriftInput(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        driftingInput = ctx.performed;
    }

    public void AccelerationInput(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (!canControl) return;

        accelerationInput = ctx.performed;
    }

    public void UseItemInput(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if(ctx.started && m_ItemObtained != null)
        {
            UseItem();
        }
    }

    #endregion

    #region Networking

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShootServerRpc(int m_ProjectileId, NetworkObjectReference targetRef, Vector3 direction)
    {
        ShootItemClass m_Projectile = ItemDatabase.Instance.GetProjectileItemById(m_ProjectileId);

        GameObject projectile = Instantiate(m_Projectile.projectilePrefab, m_ShootingTransform.position, Quaternion.identity);
        
        projectile.GetComponent<NetworkObject>().Spawn();

        HitteableBehaviour objective = null;

        if (targetRef.TryGet(out NetworkObject targetNetObj))
        {
            objective = targetNetObj.GetComponent<HitteableBehaviour>();
        }

        projectile.GetComponent<ProjectileBehaviour>().SetProperties(objective, direction, m_Projectile.velocity, m_Projectile.isHoming);
    }

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        m_PlayerNameTxt.text = newName.ToString();
    }

    #endregion
}
