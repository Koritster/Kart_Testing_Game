using UnityEngine;

public class KartState
{
    protected float gravityVariable;
    protected float accelerationVariable;

    public virtual void InitializeState()
    {
        gravityVariable = 0;
        accelerationVariable = 0;
    }

    public virtual void ApplyZeroVelocity(Rigidbody kartRB)
    {
        kartRB.linearVelocity = Vector3.zero;
    }

    public virtual void ApplyThrottle(Rigidbody kartRB, Vector3 centerOfMass, bool isThrottling, bool isGrounded, bool isDrifting, bool isReversing, float raycastDistance, float maxForce, float maxTurnForce, float reverseForce, float airMultiplier, float targetSpeed, float maxBoosterMultiplier, float accelerationRate, float maxTurnCounterForce, LayerMask rotationLayers, LayerMask gravityLayers) { }

    public virtual void ApplyReverse(Rigidbody kartRB, bool isReversing, bool isGrounded, float reverseForce, float airMultiplier, float targetSpeed, float maxBoosterMultiplier, float accelerationRate) { }

    public virtual void ApplyRotation(Rigidbody kartRB, Vector3 centerOfMass, Vector3 input, bool isThrottling, float raycastDistance, float rotationForce, float targetSpeed, LayerMask rotationLayers, LayerMask gravityLayers) { }

    public virtual void ApplyDrift(Rigidbody kartRB, Vector2 move, bool isDrifting, bool isGrounded, bool boostActive, bool driftBoostActive, out bool exhaustVFXActive, bool exhaustVFX, out bool rightParticlesActive, bool rightParticles, out bool leftParticlesActive, bool leftParticles, out bool driftInitiated, float boostImmediateForce, float driftThrottleUpperThreshold, float driftThrottleLowerThreshold, out float maxRotationAngle, out float maxDriftingTime, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier, float rotationAngle, float driftingRotationAngle, float driftingTime)
    {
        maxRotationAngle = rotationAngle;
        maxDriftingTime = driftingTime;
        driftBoostActive = false;
        isGrounded = false;
        driftInitiated = false;
        exhaustVFXActive = exhaustVFX;
        rightParticlesActive = rightParticles;
        leftParticlesActive = leftParticles;
        maxBoosterTime = boosterTime;
        maxBoosterMultiplier = boosterMultiplier;
        boostActive = false;
    }

    public virtual void ApplyTrackGravity(Rigidbody kartRB, Vector3 centerOfMass, float raycastDistance, float gravityConstant, LayerMask rotationLayers, LayerMask gravityLayers)
    {
        //RaycastHit gravityHit, rotationHit;
        //bool gravity = Physics.Raycast(centerOfMass, -kartRB.transform.up, out gravityHit, raycastDistance, gravityLayers);
        //bool rotation = Physics.Raycast(centerOfMass, -kartRB.transform.up, out rotationHit, raycastDistance, rotationLayers);

        //if (rotation && !gravity)
        //{
        //    gravityVariable = Mathf.Lerp(gravityVariable, gravityConstant * 5f, Time.fixedDeltaTime);
        //    kartRB.AddForce(Vector3.down * gravityVariable, ForceMode.Acceleration);
        //    return;
        //}
        //if(gravity)
        //{
        //    gravityVariable = gravityConstant;
        //}

        RaycastHit hit;
        if (Physics.Raycast(centerOfMass, kartRB.transform.forward, out hit, raycastDistance, gravityLayers) || Physics.Raycast(centerOfMass, -kartRB.transform.up, out hit, raycastDistance, gravityLayers))
        {
            kartRB.AddForce(-hit.normal * gravityConstant, ForceMode.Acceleration);
        }
        else
        {
            kartRB.AddForce(Vector3.down * gravityConstant, ForceMode.Acceleration);
        }
    }

    public virtual void ApplyBoost(Rigidbody kartRB, float boostImmediateForce, float boosterTimeMultiplier, out bool boostActive, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier)
    {
        maxBoosterTime = boosterMultiplier;
        maxBoosterMultiplier = boosterMultiplier;
        boostActive = false;
    }

    public virtual void ApplyDriftBoost(Rigidbody kartRB, float boostImmediateForce, float boostTimeMultiplier, out bool boostActive, out bool driftBoostActive, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier)
    {
        ApplyBoost(kartRB, boostImmediateForce, boostTimeMultiplier, out boostActive, out maxBoosterTime, out maxBoosterMultiplier, boosterTime, boosterMultiplier);
        driftBoostActive = true;
    }

    public virtual bool CheckIfKartIsStuck(Rigidbody kartRB, bool isThrottling, float stuckTime, out float maxStuckTime, float currentStuckTime, float targetSpeed)
    {
        maxStuckTime = currentStuckTime;
        return false;
    }
}
