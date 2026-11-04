//#define DEBUG_FEATURE
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class NewKart : MonoBehaviour
{
#if DEBUG_FEATURE
    Variables network
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
#endif

    public int laps {  get; private set; }
    public int actualCheckpoint {  get; private set; }
    public int position {  get; private set; }

    public KartStateMachine stateMachine {  get; private set; }
    public Rigidbody m_Rigidbody {  get; private set; }

    RaycastHit hit;
    bool boostActive, isGrounded, groundBoostActive, driftBoostActive, driftInitiated, exhaustVFXActive, rightParticlesActive, leftParticlesActive, checkForStuckKart;

    protected Vector2 move;
    protected Vector3 m_Input, m_CollisionNormal;
    protected bool throttle, reverse, drift, kartStuck, kartCollision;
    protected float m_MaxForce, m_MaxTurnForce, m_MaxTurnCounterForce, m_MaxDriftingTime, m_MaxBoosterTime, m_MaxBoosterMultiplier, m_MaxRotationAngle, m_MaxStuckTime;
    protected int m_CollisionLayer;

    protected InitialKartState initialKartState;
    protected DriveKartState driveKartState;
    protected ThrottleKartState throttleKartState;
    protected ReverseKartState reverseKartState;

    public Transform centerOfMass;
    public Transform Nozzle;
    public Transform RightWheel;
    public Transform LeftWheel;
    public LayerMask gravityLayers;
    public LayerMask rotationLayers;
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
    public float m_StuckTime = 3f;

    public virtual void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();

        initialKartState = new InitialKartState();
        driveKartState = new DriveKartState();
        throttleKartState = new ThrottleKartState();
        reverseKartState = new ReverseKartState();

        InitializeKart();

        //ONLY FOR TESTING SINGLE PLAYER, WILL REMOVE LATER
#if DEBUG_FEATURE
        if (!IsServer)
#endif
            //PositionsManager.instance.RegisterKart(this);
    }

    public virtual void Update()
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif
        
        CalculateMoveInput();
     
        CheckIfGrounded();
        
        if (boostActive)
            ReduceBoosterTimer();
        if (driftInitiated)
            ReduceDriftingTimer();
        if (kartStuck)
            ReduceStuckTime();
    }

    public virtual void FixedUpdate()
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        stateMachine.currentState.ApplyThrottle(m_Rigidbody, centerOfMass.position, m_Input, throttle, isGrounded, driftInitiated, reverse, m_RaycastDistance, m_MaxForce, m_MaxTurnForce, m_ReverseForce, m_AirMultiplier, m_TargetSpeed, m_MaxBoosterMultiplier, m_AccelerationRate, m_MaxTurnCounterForce, rotationLayers, gravityLayers);
        stateMachine.currentState.ApplyReverse(m_Rigidbody, reverse, isGrounded, m_ReverseForce, m_AirMultiplier, m_TargetSpeed, m_MaxBoosterMultiplier, m_AccelerationRate);
        stateMachine.currentState.ApplyRotation(m_Rigidbody, RightWheel, LeftWheel, centerOfMass.position, m_Input, m_CollisionNormal, kartCollision, throttle, isGrounded, m_CollisionLayer, m_RaycastDistance, m_RotationForce, m_TargetSpeed, rotationLayers, gravityLayers);
        stateMachine.currentState.ApplyTrackGravity(m_Rigidbody, centerOfMass.position, m_RaycastDistance, m_GravityConstant, rotationLayers, gravityLayers);
        kartStuck = stateMachine.currentState.CheckIfKartIsStuck(m_Rigidbody, throttle, m_StuckTime, out m_MaxStuckTime, m_MaxStuckTime, m_TargetSpeed);
        //stateMachine.currentState.ApplyDrift(m_Rigidbody, move, drift, isGrounded, boostActive, driftBoostActive, out exhaustVFXActive, exhaustVFXActive, out rightParticlesActive, rightParticlesActive, out leftParticlesActive, leftParticlesActive, out driftInitiated, m_BoostImmediateForce, m_DriftThrottleUpperThreshold, m_DriftThrottleLowerThreshold, out m_MaxRotationAngle, out m_MaxDriftingTime, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier, m_RotationAngle, m_DriftingRotationAngle, m_DriftingTime);
        if (stateMachine.currentState != reverseKartState)
        {
            ApplyDrift();
            ShowParticleEffects(exhaustVFXActive, rightParticlesActive, leftParticlesActive);
        }
        else
        {
            boostActive = false;
            groundBoostActive = false;
            driftBoostActive = false;
            drift = false;
        }
        if(stateMachine.currentState == driveKartState)
        {
            ShowParticleEffects(exhaustVFXActive, false, false);
        }
    }

    public virtual void SetActualCheckpoint(int checkpoint) { actualCheckpoint = checkpoint; }
    public virtual void SetLaps(int newLaps) { laps = newLaps; }
    public virtual void AddLaps(int newLaps) { laps += newLaps; }
    public virtual void SetPosition(int newPosition) { position = newPosition; }
    public virtual void AddPosition(int newPosition) { position += newPosition; }

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
        checkForStuckKart = false;
        kartStuck = false;
        isGrounded = false;
        driftInitiated = false;
        exhaustVFXActive = false;
        rightParticlesActive = false;
        leftParticlesActive = false;
        kartCollision = false;
        m_MaxBoosterTime = 0;
        m_MaxDriftingTime = m_DriftingTime;
        m_MaxBoosterMultiplier = 1;
        m_MaxStuckTime = m_StuckTime;

        SetActualCheckpoint(0);
        SetLaps(0);
        SetPosition(0);

        stateMachine = new KartStateMachine(initialKartState);
        //vv THIS WILL BE HANDLED BY A MANAGER OR EVENT HANDLER LATER vv
        stateMachine.ChangeState(driveKartState);
    }

    protected virtual void CalculateMoveInput() { }

    private void OnTriggerEnter(Collider other)
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        if (other.gameObject.layer == LayerMask.NameToLayer("NitroPad"))
        {
            //APPLY NITRO PAD BOOST
            stateMachine.currentState.ApplyBoost(m_Rigidbody, m_BoostImmediateForce, 1f, out boostActive, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier);
            exhaustVFXActive = boostActive;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
#if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        if (groundBoostActive && stateMachine.currentState != initialKartState)
        {
            //APPLY GROUND LANDING BOOST
            stateMachine.currentState.ApplyBoost(m_Rigidbody, m_BoostImmediateForce * 0.5f, 0.5f, out boostActive, out m_MaxBoosterTime, out m_MaxBoosterMultiplier, m_BoosterTime, m_BoosterMultiplier);
            exhaustVFXActive = boostActive;
            groundBoostActive = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
#if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        kartCollision = true;
        m_CollisionNormal = collision.GetContact(0).normal;
        m_CollisionLayer = collision.GetContact(0).otherCollider.gameObject.layer;
    }

    private void OnCollisionExit(Collision collision)
    {
#if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        kartCollision = false;
    }

    private void ShowParticleEffects(bool exhaustVFXActive, bool rightParticlesActive, bool leftParticlesActive)
    {
        exhaustVFX.gameObject.SetActive(exhaustVFXActive);
        rightParticles.gameObject.SetActive(rightParticlesActive);
        leftParticles.gameObject.SetActive(leftParticlesActive);
    }

    void ReduceStuckTime()
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        if (m_MaxStuckTime > 0)
        {
            m_MaxStuckTime -= Time.deltaTime;
        }
        else
        {
            m_MaxStuckTime = 0;
        }
    }    

    void ReduceBoosterTimer()
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

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
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif
     
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
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif
 
        if (!isGrounded || !throttle || m_Rigidbody.linearVelocity.magnitude < m_DriftThrottleLowerThreshold)
        {
            m_MaxRotationAngle = m_RotationAngle;
            m_MaxDriftingTime = m_DriftingTime;

            driftInitiated = false;
            driftBoostActive = false;

            rightParticlesActive = false;
            leftParticlesActive = false;

            return;
        }

        if (drift && isGrounded && m_Rigidbody.linearVelocity.magnitude > m_DriftThrottleUpperThreshold)
        {
            driftInitiated = true;
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
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

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
