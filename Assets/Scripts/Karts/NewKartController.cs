using TMPro;
using Unity.Collections;
using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NewKartController : PlayerKart
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_PlayerNameTxt;
    [SerializeField] private TextMeshProUGUI m_PositionTxt;

    [SerializeField] private Image m_ItemIcon;

    /*[Header("Prefabs References")]
    [SerializeField] private GameObject m_AimMarkerPrefab;
    [SerializeField] private GameObject m_ShieldPrefab;

    [SerializeField] private Transform m_ShootingTransform;
    [SerializeField] private Transform m_ShieldTransform;
    [SerializeField] private Transform m_CameraOffset;*/
    [Header("Transform References")]
    //[SerializeField] private Transform m_CarModelVisualTransform;

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<FixedString32Bytes> carModel = new NetworkVariable<FixedString32Bytes>(default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject virtualCam;

    /*private GameObject m_AimMarker;
    private GameObject shieldVisual;

    private int shieldHits = 0;
    private bool isInvulnerable;*/

    //private ItemClass m_ItemObtained;

    //private List<HitteableBehaviour> m_Hitteables;

    //private HitteableBehaviour m_ActualObjective = null;

    //private HitteableBehaviour m_LocalHitteableBehaviour;

    #region Unity functions

    protected override void OnEnable()
    {
        base.OnEnable();

        //Suscripciones a cambios de variables network
        playerName.OnValueChanged += OnNameChanged;
        carModel.OnValueChanged += OnCarModelChanged;

        //Instanciacion de marcadores
        /*m_AimMarker = Instantiate(m_AimMarkerPrefab, Vector3.zero, Quaternion.identity);
        m_AimMarker.SetActive(false);
        shieldVisual = Instantiate(m_ShieldPrefab, m_ShieldTransform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.SetActive(false);*/
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        playerName.OnValueChanged -= OnNameChanged;
        carModel.OnValueChanged -= OnCarModelChanged;
    }

    public override void Update()
    {
        base.Update();

        if (!IsOwner || NetcodeLobby.instance.GameStarted.Value == false) return;

        UIPosition();
        
        if (!IsServer) return;
        
        //float progress = ;

        //trackProgress.Value = progress;

        //int pos = PositionsManager.instance.GetPosition(this);

        /*
        if (m_ItemObtained != null)
        {
            if (m_ItemObtained.GetShootItem() != null)
            {
                if (m_ItemObtained.GetShootItem().isHoming)
                {
                    AimObjective();
                }
            }
        }*/
    }

    #endregion

    #region Utility

    public void Teleport(Transform _NewPos)
    {
        GetComponent<NetworkTransform>().Teleport(
            _NewPos.position,
            _NewPos.rotation,
            transform.localScale
        );
    }

    #endregion
    
    /*
    
    #region Items

    public void ReceiveItem(ItemClass item)
    {
        if (!IsOwner) return;

        Debug.Log("Recibiendo item");

        if (m_ItemIcon == null)
            Debug.LogError("ItemIcon no asignado en el inspector");

        if (item == null)
            Debug.LogError("Item recibido es null");

        m_ItemObtained = item;

        if (m_ItemIcon != null && item != null)
            m_ItemIcon.sprite = item.itemIcon;
    }

    public void UseItem()
    {
        m_ItemObtained.UseItem(this);

        if (m_ItemObtained.GetShootItem() != null)
        {
            ShootItemClass projectile = m_ItemObtained.GetShootItem();

            Shoot(projectile, m_ActualObjective);
        }
        else if (m_ItemObtained.GetSpecialEffectItem() != null)
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
        if (item.type == EffectType.Shield)
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

        foreach (HitteableBehaviour item in HitteableBehaviour.GetAllExcept(m_LocalHitteableBehaviour))
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
        if (m_Hitteables.Count <= 0) return;

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
    
    */

    #region UI

    private void UIPosition()
    {
        m_PositionTxt.text = m_PositionTxt.text = Position.Value + "°";
    }

    #endregion

    #region Networking

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Registrar kart
        if (IsClient)
        {
            PositionsManager.instance.RegisterKart(this);
        }

        //Actualizar cambios 
        if (playerName.Value != default)
        {
            ChangeName(playerName.Value.ToString());
        }

        if (carModel.Value != default)
        {
            ChangeCarModel(carModel.Value.ToString());
        }

        if (IsOwner)
        {
            cam.gameObject.SetActive(true);
            virtualCam.gameObject.SetActive(true);

            if (Camera.main != null)
                Camera.main.gameObject.SetActive(false);
        }
    }

    /*[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
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
    */

    //Al cambiar el nombre
    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        ChangeName(newName.ToString());
    }

    private void ChangeName(string name)
    {
        m_PlayerNameTxt.text = name;
    }

    //Al cambiar el modelo de carro
    private void OnCarModelChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        ChangeCarModel(newName.ToString());
    }

    private void ChangeCarModel(string newName)
    {
        GameObject kartVisual = CarSelector.instance.SearchKartModelByName(newName);
        //GameObject kartInstantiated = Instantiate(kartVisual, m_CarModelVisualTransform);
        //kartInstantiated.transform.localPosition = Vector3.zero;
    }

    #endregion
}
