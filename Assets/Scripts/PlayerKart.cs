using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKart : NewKart
{
    PlayerInput kartInput;
    InputAction moveAction, throttleAction, reverseAction, driftAction;

    private void OnEnable()
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

    private void OnDisable()
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

        float angleLimit = Mathf.Clamp(Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg, -m_MaxRotationAngle, m_MaxRotationAngle);
        float targetAngle = angleLimit + Camera.main.transform.eulerAngles.y;
        m_Input = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnReverse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            throttle = false;
            reverse = true;
        }
    }

    public void OnStopReverse(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            reverse = false;
        }
    }

    public void OnThrottle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            throttle = true;
            reverse = false;
        }
    }

    public void OnStopThrottle(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            throttle = false;
        }
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            drift = true;
        }
    }

    public void OnStopDrift(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            drift = false;
        }
    }
}
