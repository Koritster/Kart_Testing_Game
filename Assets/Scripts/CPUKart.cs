using Unity.VisualScripting;
using UnityEngine;

public class CPUKart : NewKart
{
    public int actualTrackTarget;
    public float targetDistanceThreshold = 1.0f;
    public float obstacleAvoidanceDistance = 5f;
    public float obstacleAvoidanceForce = 1f;
    public float maxDriftTimer = 1f;
    public float checkDriftAngle = 30f;

    private Transform targetsParent;
    private Transform trackTargetTransform;
    private Vector3 directionToTrackTarget;
    private float driftTimer, angleToDrift;

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    public override void Start()
    {
        base.Start();

        //DEBUG 
        //throttle = true;
    }

    public override void Update()
    {
        base.Update();

        Throttle();
        Break();
        CalculateTrackTargetDirection();
        CalculateTrackTargetDistance();

        if (driftTimer > 0)
            ReduceDriftCooldown();
        else
            CheckIfDrift();
    }

    protected override void InitializeKart()
    {
        base.InitializeKart();

        targetsParent = GameObject.Find("CPUTargets").transform;
        actualTrackTarget = 0;
        directionToTrackTarget = Vector3.zero;
        driftTimer = 0;
        FindNextTrackTargetTransform();
    }

    //DEBUGGING, MAY CHANGE LATER
    protected override void CalculateMoveInput()
    {
        base.CalculateMoveInput();

        Vector3 avoidance = ObstacleAvoidance();
        m_Input = new Vector3(directionToTrackTarget.x, 0f, directionToTrackTarget.z);
        m_Input += new Vector3(avoidance.x, 0f, avoidance.z);
        if(driftTimer <= 0)
        {
            move.x = Vector3.Angle(m_Input, m_Rigidbody.transform.right) < 90 ? 1 : -1;
        }
    }
    
    private void ReduceDriftCooldown()
    {
        driftTimer -= Time.deltaTime;
    }

    private void CheckIfDrift()
    {
        angleToDrift = Vector3.Angle(directionToTrackTarget, m_Rigidbody.transform.forward);
        if (angleToDrift > checkDriftAngle)
        {
            driftTimer = Time.deltaTime;
            drift = true;
        }
        else 
            drift = false;
    }

    private void Throttle()
    {
        if (m_Rigidbody.linearVelocity.magnitude > m_TargetSpeed && angleToDrift > checkDriftAngle)
            throttle = false;
        else
            throttle = true;
    }

    private void Break()
    {
        if (m_Rigidbody.linearVelocity.magnitude > m_TargetSpeed * 0.5f && angleToDrift > checkDriftAngle * 2f)
            reverse = true;

        if (angleToDrift < checkDriftAngle)
            reverse = false;
    }

    private Vector3 AvoidanceForce(Vector3 v)
    {
        return (m_Rigidbody.linearVelocity - v).normalized;
    }

    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        if (Physics.Raycast(m_Rigidbody.transform.position, m_Rigidbody.transform.forward, out hit, obstacleAvoidanceDistance))
            return AvoidanceForce(hit.transform.position) * obstacleAvoidanceForce;
        else
            return Vector3.zero;
    }

    private void CalculateTrackTargetDirection()
    {
        directionToTrackTarget = trackTargetTransform.position - m_Rigidbody.transform.position;
        directionToTrackTarget.Normalize();
    }

    private void CalculateTrackTargetDistance()
    {
        float distance = Vector3.Distance(m_Rigidbody.transform.position, trackTargetTransform.position);
        if(distance < targetDistanceThreshold)
        {
            FindNextTrackTargetIndex();
            FindNextTrackTargetTransform();
        }
    }

    private void FindNextTrackTargetIndex()
    {
        //Change this in the future when evaluating targets weights
        actualTrackTarget++;
    }

    private void FindNextTrackTargetTransform()
    {
        CPUTrackTarget[] targets = targetsParent.GetComponentsInChildren<CPUTrackTarget>();
        foreach (var target in targets)
        {
            int i = target.index;
            if(i == actualTrackTarget)
            {
                //DEBUG
                if(target.weight == 100)
                {
                    trackTargetTransform = target.transform;
                    return;
                }
            }
        }
    }
}
