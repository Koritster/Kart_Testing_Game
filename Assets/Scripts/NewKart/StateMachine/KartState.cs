using UnityEngine;

public class KartState
{
    public virtual void ApplyThrottle(Rigidbody kartRB, bool isThrottling, bool isGrounded, bool isDrifting, bool isReversing, float maxForce, float maxTurnForce, float reverseForce, float airMultiplier, float targetSpeed, float maxBoosterMultiplier, float accelerationRate, float maxTurnCounterForce) { }

    public virtual void ApplyReverse(Rigidbody kartRB, bool isReversing, bool isGrounded, float reverseForce, float airMultiplier, float targetSpeed, float maxBoosterMultiplier, float accelerationRate) { }

    public virtual void ApplyRotation(Rigidbody kartRB, Vector3 centerOfMass, Vector3 input, float raycastDistance, float rotationForce, LayerMask raycastLayers) { }

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

    public virtual void ApplyTrackGravity(Rigidbody kartRB, Vector3 centerOfMass, float raycastDistance, float gravityConstant, LayerMask layerMask)
    {
        RaycastHit hit;
        if (Physics.Raycast(centerOfMass, kartRB.transform.forward, out hit, raycastDistance, layerMask) || Physics.Raycast(centerOfMass, -kartRB.transform.up, out hit, raycastDistance, layerMask))
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
}
