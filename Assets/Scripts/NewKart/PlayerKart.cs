#define DEBUG_FEATURE
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKart : NewKart
{
    PlayerInput kartInput;
    InputAction moveAction, throttleAction, reverseAction, driftAction;

    protected virtual void OnEnable()
    {
        kartInput = GetComponent<PlayerInput>();
        moveAction = kartInput.actions["Move"];
        throttleAction = kartInput.actions["Throttle"];
        reverseAction = kartInput.actions["Reverse"];
        driftAction = kartInput.actions["Drift"];
        moveAction.performed += OnMove;
        throttleAction.performed += OnThrottle;
        throttleAction.canceled += OnStopThrottle;
        reverseAction.performed += OnReverse;
        reverseAction.canceled += OnStopReverse;
        driftAction.performed += OnDrift;
        driftAction.canceled += OnStopDrift;
    }

    protected virtual void OnDisable()
    {
        moveAction.performed -= OnMove;
        throttleAction.performed -= OnThrottle;
        throttleAction.canceled -= OnStopThrottle;
        reverseAction.performed -= OnReverse;
        reverseAction.canceled -= OnStopReverse;
        driftAction.performed -= OnDrift;
        driftAction.canceled -= OnStopDrift;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void CalculateMoveInput()
    {
#if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        base.CalculateMoveInput();

        float angleLimit = Mathf.Clamp(Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg, -m_MaxRotationAngle, m_MaxRotationAngle);
        float targetAngle = angleLimit + Camera.main.transform.eulerAngles.y;
        m_Input = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        
        Debug.Log($"m_Input {m_Input}");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        move = context.ReadValue<Vector2>();

        Debug.Log($"Move {move}");

        SendInputServerRpc(move, throttle, reverse, drift);
    }

    public void OnReverse(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (stateMachine.currentState == initialKartState)
            return;

        if (context.performed)
        {
            throttle = false;
            reverse = true;

            //stateMachine.ChangeState(reverseKartState);
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    public void OnStopReverse(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (stateMachine.currentState == initialKartState)
            return;

        if (context.canceled)
        {
            reverse = false;

            //stateMachine.ChangeState(driveKartState);
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    public void OnThrottle(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (stateMachine.currentState == initialKartState)
            return;

        if (context.performed)
        {
            throttle = true;
            reverse = false;

            //stateMachine.ChangeState(throttleKartState);
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    public void OnStopThrottle(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (stateMachine.currentState == initialKartState)
            return;

        if (context.canceled)
        {
            throttle = false;

            //stateMachine.ChangeState(driveKartState);
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (context.performed)
        {
            drift = true;
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    public void OnStopDrift(InputAction.CallbackContext context)
    {
#if DEBUG_FEATURE
        if (!IsOwner) return;
#endif

        if (context.canceled)
        {
            drift = false;
            SendInputServerRpc(move, throttle, reverse, drift);
        }
    }

    [Rpc(SendTo.Server)]
    protected void SendInputServerRpc(Vector2 moveInput, bool throttleInput, bool reverseInput, bool driftInput)
    {
        move = moveInput;
        throttle = throttleInput;
        reverse = reverseInput;
        drift = driftInput;

        if (reverse)
        {
            stateMachine.ChangeState(reverseKartState);
        }
        else if(throttle) 
        {
            stateMachine.ChangeState(throttleKartState);
        }
        else
        {
            stateMachine.ChangeState(driveKartState);
        }
    }
}
