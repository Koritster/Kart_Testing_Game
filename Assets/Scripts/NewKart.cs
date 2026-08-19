using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewKart : NetworkBehaviour
{
    //Variables network
    [Header("Network Variables")]
    public NetworkVariable<int> laps = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> actualCheckpoint = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> Position = new(0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    RaycastHit hit;
    bool boostActive, isGrounded, groundBoostActive, driftBoostActive, driftInitiated;

    protected Rigidbody m_Rigidbody;
    protected Vector2 move;
    protected Vector3 m_Input;
    protected bool throttle, reverse, drift;
    protected float m_MaxForce, m_MaxTurnForce, m_MaxTurnCounterForce, m_MaxDriftingTime, m_MaxBoosterTime, m_MaxBoosterMultiplier, m_MaxRotationAngle;

    public Transform centerOfMass;
    public Transform Nozzle;
    public LayerMask raycastLayers;
    public ParticleSystem rightParticles, leftParticles, exhaustVFX;
    public float m_RaycastDistance = 1f;
    public float m_AccelerationRate = 8f;
    public float m_TargetSpeed = 10f;
    public float m_Force = 10f;
    public float m_ReverseForce = 8f;
    public float m_TurnForce = 10f;
    public float m_BoostImmediateForce = 10f;
    public float m_MagnitudeTurnLimit = 20f;
    public float m_TurnCounterForce = 0.5f;
    public float m_GravityConstant = 9.81f;
    public float m_RotationAngle = 45f;
    public float m_DriftingRotationAngle = 90f;
    public float m_DriftingTime = 1f;
    public float m_RotationForce = 10f;
    public float m_BoosterTime = 1f;
    public float m_BoosterMultiplier = 1.5f;
    public float m_AirMultiplier = 0.5f;
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

        if (driftInitiated)
            ReduceDriftingTimer();

        CalculateMoveInput();
    }

    public virtual void FixedUpdate()
    {
        ApplyThrottle();
        ApplyRotation();
        ApplyTrackGravity();
        ApplyDrift();
    }

    protected virtual void InitializeKart()
    {
        m_MaxForce = m_Force;
        m_MaxTurnForce = m_TurnForce;
        m_MaxTurnCounterForce = m_TurnCounterForce;
        m_MaxRotationAngle = m_RotationAngle;
        throttle = false;
        boostActive = false;
        groundBoostActive = false;
        driftBoostActive = false;
        drift = false;
        isGrounded = false;
        driftInitiated = false;
        m_MaxBoosterTime = 0;
        m_MaxDriftingTime = m_DriftingTime;
        m_MaxBoosterMultiplier = 1;
    }

    protected virtual void CalculateMoveInput() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("NitroPad"))
        {
            ApplyBoost();
        }

        Debug.Log(actualCheckpoint.Value);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (groundBoostActive && isGrounded)
        {
            ApplyGroundBoost();
        }
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
            m_MaxBoosterTime = 0;
            m_MaxBoosterMultiplier = 1;
            boostActive = false;
            exhaustVFX.gameObject.SetActive(false);
        }
    }

    void ReduceDriftingTimer()
    {
        if (m_MaxDriftingTime > 0)
        {
            m_MaxDriftingTime -= Time.deltaTime;
        }
        else
        {
            driftBoostActive = true;
        }
    }

    void ApplyThrottle()
    {
        Vector3 velocity = Vector3.zero;

        //NEW FORCE MOVEMENT
        if (throttle)
        {
            velocity = m_Rigidbody.transform.forward * m_MaxForce;
        }

        if (driftInitiated)
        {
            m_Rigidbody.AddForce(-m_Rigidbody.linearVelocity * m_MaxTurnCounterForce, ForceMode.Force);
            velocity = m_Rigidbody.transform.forward * m_MaxTurnForce;
        }

        if (reverse)
        {
            velocity = -m_Rigidbody.transform.forward * m_ReverseForce;
        }

        if (!isGrounded)
        {
            velocity *= m_AirMultiplier;
        }

        if (m_Rigidbody.linearVelocity.magnitude < m_TargetSpeed)
            m_Rigidbody.AddForce(velocity * m_MaxBoosterMultiplier * m_AccelerationRate, ForceMode.Force);
    }

    void ApplyBoost()
    {
        m_Rigidbody.AddForce(m_Rigidbody.linearVelocity.normalized * m_BoostImmediateForce, ForceMode.VelocityChange);
        m_MaxBoosterTime = m_BoosterTime;
        m_MaxBoosterMultiplier = m_BoosterMultiplier;
        boostActive = true;
        exhaustVFX.gameObject.SetActive(true);
    }

    void ApplyGroundBoost()
    {
        m_Rigidbody.AddForce(m_Rigidbody.transform.forward * m_BoostImmediateForce * 0.5f, ForceMode.VelocityChange);
        m_MaxBoosterTime = m_BoosterTime * 0.5f;
        m_MaxBoosterMultiplier = m_BoosterMultiplier;
        boostActive = true;
        groundBoostActive = false;
        exhaustVFX.gameObject.SetActive(true);
    }

    void ApplyDriftBoost()
    {
        m_Rigidbody.AddForce(m_Rigidbody.transform.forward * m_BoostImmediateForce * 0.8f, ForceMode.VelocityChange);
        m_MaxBoosterTime = m_BoosterTime;
        m_MaxBoosterMultiplier = m_BoosterMultiplier;
        boostActive = true;
        driftBoostActive = false;
        exhaustVFX.gameObject.SetActive(true);
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
            //Quaternion finalRotation = Quaternion.Lerp(m_Rigidbody.rotation, Quaternion.LookRotation(m_Input), Time.fixedDeltaTime * m_RotationForce);

            //m_Rigidbody.transform.up = Vector3.Lerp(m_Rigidbody.transform.up, hit.normal, Time.fixedDeltaTime * 8f);
            //m_Rigidbody.transform.Rotate(Vector3.up, finalRotation.eulerAngles.y, Space.Self);

            //float angle = Vector3.Angle(Vector3.up, hit.normal);
            //Quaternion gravityRotation = Quaternion.Euler(angle, 0f, 0f);

            Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
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
            //m_MaxRotationAngle = m_DriftingRotationAngle;
            driftInitiated = true;
        }

        if (/*!drift || */!isGrounded || m_Rigidbody.linearVelocity.magnitude < m_DriftThrottleLowerThreshold)
        {
            m_MaxRotationAngle = m_RotationAngle;
            m_MaxDriftingTime = m_DriftingTime;

            driftInitiated = false;

            rightParticles.gameObject.SetActive(false);
            leftParticles.gameObject.SetActive(false);
        }

        if (driftBoostActive && !drift /*&& move.x < 0.75f && move.x > -0.75f*/)
        {
            ApplyDriftBoost();
            m_MaxRotationAngle = m_RotationAngle;
            m_MaxDriftingTime = m_DriftingTime;

            driftInitiated = false;

            rightParticles.gameObject.SetActive(false);
            leftParticles.gameObject.SetActive(false);
        }

        if (driftInitiated)
        {
            m_MaxRotationAngle = m_DriftingRotationAngle;

            if (move.x == 0f)
            {
                m_MaxRotationAngle = m_RotationAngle;
                m_MaxDriftingTime = m_DriftingTime;

                driftInitiated = false;

                rightParticles.gameObject.SetActive(false);
                leftParticles.gameObject.SetActive(false);
            }
            else if (move.x > 0f)
            {
                rightParticles.gameObject.SetActive(true);
                leftParticles.gameObject.SetActive(false);
            }
            else if(move.x < 0f)
            {
                leftParticles.gameObject.SetActive(true);
                rightParticles.gameObject.SetActive(false);
            }    
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
            groundBoostActive = true;
        }
    }
}
