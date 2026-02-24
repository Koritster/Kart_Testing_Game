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
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class CarController : NetworkBehaviour
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

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private float turnInput;
    private bool accelerationInput;
    private bool driftingInput;

    private Rigidbody rb;

    private ItemClass m_ItemObtained;

    private void OnEnable()
    {
        playerName.OnValueChanged += OnNameChanged;
    }

    private void OnDisable()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(m_GroundCheck.position, groundChkRadius);
    }

    #region Items

    public void ReceiveItem(ItemClass item)
    {

    }

    public void ReceiveRandomItem()
    {

    }

    private void UseItem(ItemClass item)
    {
        item.UseItem(this);
    }

    #endregion

    #region Inputs

    public void TurnInput(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

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

        accelerationInput = ctx.performed;
    }

    #endregion

    #region Networking

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        m_PlayerNameTxt.text = newName.ToString();
    }

    #endregion
}
