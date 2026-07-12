using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewKart : NetworkBehaviour
{
    Rigidbody m_Rigidbody;
    RaycastHit hit;
    bool boostActive, isGrounded;

    protected Vector2 move;
    protected Vector3 m_Input;
    protected bool throttle, reverse, drift;
    protected float m_MaxForce, m_MaxTurnForce, m_MaxTurnCounterForce, m_MaxBoosterTime, m_MaxRotationAngle;

    public Transform centerOfMass;
    public Transform Nozzle;
    public LayerMask raycastLayers;
    public float m_RaycastDistance = 1f;
    public float m_Force = 10f;
    public float m_ReverseForce = 8f;
    public float m_TurnForce = 10f;
    public float m_BoostImmediateForce = 10f;
    public float m_MagnitudeTurnLimit = 20f;
    public float m_TurnCounterForce = 0.5f;
    public float m_GravityConstant = 9.81f;
    public float m_RotationAngle = 45f;
    public float m_DriftingRotationAngle = 90f;
    public float m_RotationForce = 10f;
    public float m_BoosterTime = 1f;
    public float m_BoosterMultiplier = 1.5f;
    public float m_DriftThrottleUpperThreshold = 10f;
    public float m_DriftThrottleLowerThreshold = 10f;

    public virtual void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();

        InitializeKart();
    }

    public virtual void Update()
    {
        CheckIfGrounded();

        if (boostActive)
            ReduceBoosterTimer();
    }

    public virtual void FixedUpdate()
    {
        ApplyThrottle();
        ApplyRotation();
        ApplyTrackGravity();
        ApplyDrift();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("NitroPad"))
        {
            ApplyBoost();
        }
    }

    void InitializeKart()
    {
        m_MaxForce = m_Force;
        m_MaxTurnForce = m_TurnForce;
        m_MaxTurnCounterForce = m_TurnCounterForce;
        m_MaxRotationAngle = m_RotationAngle;
        throttle = false;
        boostActive = false;
        drift = false;
        isGrounded = false;
        m_MaxBoosterTime = m_BoosterTime;
    }

    void ReduceBoosterTimer()
    {
        if (m_MaxBoosterTime > 0)
        {
            m_MaxBoosterTime -= Time.deltaTime;
        }
        else
        {
            m_MaxForce = m_Force;
            m_MaxTurnForce = m_TurnForce;
            m_MaxTurnCounterForce = m_TurnCounterForce;
            boostActive = false;
        }
    }

    void ApplyThrottle()
    {
        //NEW FORCE MOVEMENT
        if (throttle)
        {
            if (m_Rigidbody.linearVelocity.magnitude > m_MagnitudeTurnLimit && m_Input.x != 0)
            {
                m_Rigidbody.AddForce(-m_Rigidbody.linearVelocity * m_MaxTurnCounterForce, ForceMode.Force);
                m_Rigidbody.AddForce(m_Rigidbody.transform.forward * m_MaxTurnForce, ForceMode.Force);
            }
            else
                m_Rigidbody.AddForce(m_Rigidbody.transform.forward * m_MaxForce, ForceMode.Force);
        }

        if(reverse)
        {
            m_Rigidbody.AddForce(-m_Rigidbody.transform.forward * m_ReverseForce, ForceMode.Force);
        }
    }

    void ApplyBoost()
    {
        m_Rigidbody.AddForce(m_Rigidbody.linearVelocity.normalized * m_BoostImmediateForce, ForceMode.VelocityChange);
        //m_Rigidbody.velocity *= 2;
        boostActive = true;
        m_MaxBoosterTime = m_BoosterTime;
        m_MaxForce = m_Force * m_BoosterMultiplier;
        m_MaxTurnForce = m_TurnForce * m_BoosterMultiplier;
        m_MaxTurnCounterForce = m_TurnCounterForce * m_BoosterMultiplier;
    }

    void ApplyTrackGravity()
    {
        if (Physics.Raycast(centerOfMass.position, m_Rigidbody.transform.forward, out hit, m_RaycastDistance, raycastLayers) || Physics.Raycast(centerOfMass.position, -m_Rigidbody.transform.up, out hit, m_RaycastDistance, raycastLayers))
        {
            m_Rigidbody.AddForce(-hit.normal * m_GravityConstant, ForceMode.Acceleration);
        }
        else
        {
            m_Rigidbody.AddForce(Vector3.down * m_GravityConstant, ForceMode.Acceleration);
        }
    }

    void ApplyRotation()
    {
        if (Physics.Raycast(centerOfMass.position, m_Rigidbody.transform.forward, out hit, m_RaycastDistance, raycastLayers) || Physics.Raycast(centerOfMass.position, -m_Rigidbody.transform.up, out hit, m_RaycastDistance, raycastLayers))
        {
            float angle;
            if (hit.normal.z <= 0)
                angle = -Vector3.Angle(Vector3.up, hit.normal);
            else
                angle = Vector3.Angle(Vector3.up, hit.normal);
            Quaternion gravityRotation = Quaternion.Euler(angle, 0f, 0f);
            Quaternion inputRotation = Quaternion.LookRotation(m_Input);
            Quaternion combinedRotation = gravityRotation * inputRotation;
            m_Rigidbody.rotation = Quaternion.Lerp(m_Rigidbody.rotation, combinedRotation, Time.fixedDeltaTime * m_RotationForce);
        }
        else
        {
            m_Rigidbody.rotation = Quaternion.Lerp(m_Rigidbody.rotation, Quaternion.LookRotation(m_Input), Time.fixedDeltaTime * m_RotationForce);
        }
    }

    void ApplyDrift()
    {
        if (drift && isGrounded && m_Rigidbody.linearVelocity.magnitude > m_DriftThrottleUpperThreshold)
        {
            m_MaxRotationAngle = m_DriftingRotationAngle;
        }

        if(!drift || !isGrounded || m_Rigidbody.linearVelocity.magnitude < m_DriftThrottleLowerThreshold)
        {
            m_MaxRotationAngle = m_RotationAngle;
        }
    }

    void CheckIfGrounded()
    {
        if (Physics.Raycast(centerOfMass.position, -m_Rigidbody.transform.up, out hit, m_RaycastDistance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
