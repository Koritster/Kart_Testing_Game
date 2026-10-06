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
    bool boostActive, isGrounded, groundBoostActive, driftBoostActive, driftInitiated, exhaustVFXActive, rightParticlesActive, leftParticlesActive;

    protected Rigidbody m_Rigidbody;
    protected Vector2 move;
    protected Vector3 m_Input;
    protected bool throttle, reverse, drift;
    protected float m_MaxForce, m_MaxTurnForce, m_MaxTurnCounterForce, m_MaxDriftingTime, m_MaxBoosterTime, m_MaxBoosterMultiplier, m_MaxRotationAngle;

    protected KartStateMachine stateMachine;
    protected InitialKartState initialKartState;
    protected DriveKartState driveKartState;
    protected ThrottleKartState throttleKartState;
    protected ReverseKartState reverseKartState;

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

        initialKartState = new InitialKartState();
        driveKartState = new DriveKartState();
        throttleKartState = new ThrottleKartState();
        reverseKartState = new ReverseKartState();

        InitializeKart();
    }

    public virtual void Update()
    {
        CheckIfGrounded();
        CalculateMoveInput();

        if (boostActive)
            ReduceBoosterTimer();
        if (driftInitiated)
            ReduceDriftingTimer();
    }

    public virtual void FixedUpdate()
    {
        stateMachine.currentState.ApplyThrottle(m_Rigidbody, throttle, isGrounded, driftInitiated, reverse, m_MaxForce, m_MaxTurnForce, m_ReverseForce, m_AirMultiplier, m_TargetSpeed, m_MaxBoosterMultiplier, m_AccelerationRate, m_MaxTurnCounterForce);
        stateMachine.currentState.ApplyReverse(m_Rigidbody, reverse, isGrounded, m_ReverseForce, m_AirMultiplier, m_TargetSpeed, m_MaxBoosterMultiplier, m_AccelerationRate);
        stateMachine.currentState.ApplyRotation(m_Rigidbody, centerOfMass.position, m_Input, m_RaycastDistance, m_RotationForce, raycastLayers);
        stateMachine.currentState.ApplyTrackGravity(m_Rigidbody, centerOfMass.position, m_RaycastDistance, m_GravityConstant, raycastLayers);
        //stateMachine.currentState.ApplyDrift(m_Rigidbody, move, drift, isGrounded, boostActive, driftBoostActive, out exhaustVFXActive, exhaustVFXActive, out rightParticlesActive, rightParticlesActive, out leftParticlesActive, leftParticlesActive, out driftInitiated, m_BoostImmediateForce, m_DriftThrottleUpperThreshold, m_DriftThrottleLowerThreshold, out m_MaxRotationAngle, out m_MaxDriftingTime, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier, m_RotationAngle, m_DriftingRotationAngle, m_DriftingTime);
        if(stateMachine.currentState == throttleKartState)
        ApplyDrift();
        ShowParticleEffects(exhaustVFXActive, rightParticlesActive, leftParticlesActive);
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
        exhaustVFXActive = false;
        rightParticlesActive = false;
        leftParticlesActive = false;
        m_MaxBoosterTime = 0;
        m_MaxDriftingTime = m_DriftingTime;
        m_MaxBoosterMultiplier = 1;

        stateMachine = new KartStateMachine(initialKartState);
        stateMachine.ChangeState(throttleKartState);
    }

    protected virtual void CalculateMoveInput() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("NitroPad"))
        {
            //APPLY NITRO PAD BOOST
            stateMachine.currentState.ApplyBoost(m_Rigidbody, m_BoostImmediateForce, 1f, out boostActive, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier);
            exhaustVFXActive = boostActive;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (groundBoostActive && isGrounded && stateMachine.currentState == throttleKartState)
        {
            //APPLY GROUND LANDING BOOST
            stateMachine.currentState.ApplyBoost(m_Rigidbody, m_BoostImmediateForce * 0.5f, 0.5f, out boostActive, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier);
            exhaustVFXActive = boostActive;
            groundBoostActive = false;
        }
    }

    private void ShowParticleEffects(bool exhaustVFXActive, bool rightParticlesActive, bool leftParticlesActive)
    {
        exhaustVFX.gameObject.SetActive(exhaustVFXActive);
        rightParticles.gameObject.SetActive(rightParticlesActive);
        leftParticles.gameObject.SetActive(leftParticlesActive);
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
            exhaustVFXActive = false;
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

    void ApplyDrift()
    {
        if (drift && isGrounded && m_Rigidbody.linearVelocity.magnitude > m_DriftThrottleUpperThreshold)
        {
            driftInitiated = true;
        }

        if (!isGrounded || m_Rigidbody.linearVelocity.magnitude < m_DriftThrottleLowerThreshold)
        {
            m_MaxRotationAngle = m_RotationAngle;
            m_MaxDriftingTime = m_DriftingTime;

            driftInitiated = false;

            rightParticlesActive = false;
            leftParticlesActive = false;
        }

        if (driftBoostActive && !drift)
        {
            //APPLY DRIFT BOOST
            stateMachine.currentState.ApplyDriftBoost(m_Rigidbody, m_BoostImmediateForce * 0.8f, 0.8f, out boostActive, out driftBoostActive, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier);
            exhaustVFXActive = boostActive;

            m_MaxRotationAngle = m_RotationAngle;
            m_MaxDriftingTime = m_DriftingTime;

            driftInitiated = false;

            rightParticlesActive = false;
            leftParticlesActive = false;
        }

        if (driftInitiated)
        {
            m_MaxRotationAngle = m_DriftingRotationAngle;

            if (move.x == 0f)
            {
                m_MaxRotationAngle = m_RotationAngle;
                m_MaxDriftingTime = m_DriftingTime;

                driftInitiated = false;

                rightParticlesActive = false;
                leftParticlesActive = false;
            }
            else if (move.x > 0f)
            {
                rightParticlesActive = true;
                leftParticlesActive = false;
            }
            else if(move.x < 0f)
            {
                rightParticlesActive = false;
                leftParticlesActive = true;
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
