using UnityEngine;

public class ThrottleKartState : DriveKartState
{
    public override void ApplyThrottle(Rigidbody kartRB, bool isThrottling, bool isGrounded, bool isDrifting, bool isReversing, float maxForce, float maxTurnForce, float reverseForce, float airMultiplier, float targetSpeed, float maxBoosterMultiplier, float accelerationRate, float maxTurnCounterForce)
    {
        base.ApplyThrottle(kartRB, isThrottling, isGrounded, isDrifting, isReversing, maxForce, maxTurnForce, reverseForce, airMultiplier, targetSpeed, maxBoosterMultiplier, accelerationRate, maxTurnCounterForce);

        Vector3 velocity = Vector3.zero;

        if (isThrottling)
        {
            velocity = kartRB.transform.forward * maxForce;
        }

        if (isDrifting)
        {
            velocity = kartRB.transform.forward * maxTurnForce;
            kartRB.AddForce(-kartRB.linearVelocity * maxTurnCounterForce, ForceMode.Force);
        }

        if (isReversing)
        {
            velocity = -kartRB.transform.forward * reverseForce;
        }

        if (!isGrounded)
        {
            velocity *= airMultiplier;
        }

        if (kartRB.linearVelocity.magnitude < targetSpeed)
            kartRB.AddForce(velocity * maxBoosterMultiplier * accelerationRate, ForceMode.Force);
    }

    //public override void ApplyDrift(Rigidbody kartRB, Vector2 move, bool isDrifting, bool isGrounded, bool boostActive, bool driftBoostActive, out bool exhaustVFXActive, bool exhaustVFX, out bool rightParticlesActive, bool rightParticles, out bool leftParticlesActive, bool leftParticles, out bool driftInitiated, float boostImmediateForce, float driftThrottleUpperThreshold, float driftThrottleLowerThreshold, out float maxRotationAngle, out float maxDriftingTime, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier, float rotationAngle, float driftingRotationAngle, float driftingTime)
    //{
    //    base.ApplyDrift(kartRB, move, isDrifting, isGrounded, boostActive, driftBoostActive, out exhaustVFXActive, exhaustVFX, out rightParticlesActive, rightParticles, out leftParticlesActive, leftParticles, out driftInitiated, boostImmediateForce, driftThrottleUpperThreshold, driftThrottleLowerThreshold, out maxRotationAngle, out maxDriftingTime, out maxBoosterTime, out maxBoosterMultiplier, boosterTime, boosterMultiplier, rotationAngle, driftingRotationAngle, driftingTime);
    //    if (isDrifting && isGrounded && kartRB.linearVelocity.magnitude > driftThrottleUpperThreshold)
    //    {
    //        driftInitiated = true;
    //    }

    //    if (!isGrounded || kartRB.linearVelocity.magnitude < driftThrottleLowerThreshold)
    //    {
    //        maxRotationAngle = rotationAngle;
    //        maxDriftingTime = driftingTime;

    //        driftInitiated = false;

    //        rightParticlesActive = false;
    //        leftParticlesActive = false;
    //    }

    //    if (driftBoostActive && !isDrifting)
    //    {
    //        ApplyDriftBoost(kartRB, boostImmediateForce * 0.8f, 0.8f, out boostActive, out driftBoostActive, out maxBoosterTime, out maxBoosterMultiplier, boosterTime, boosterMultiplier);
    //        exhaustVFXActive = boostActive;

    //        maxRotationAngle = rotationAngle;
    //        maxDriftingTime = driftingTime;

    //        driftInitiated = false;

    //        rightParticlesActive = false;
    //        leftParticlesActive = false;
    //    }

    //    if (driftInitiated)
    //    {
    //        maxRotationAngle = driftingRotationAngle;

    //        if (move.x == 0f)
    //        {
    //            maxRotationAngle = rotationAngle;
    //            maxDriftingTime = driftingTime;

    //            driftInitiated = false;

    //            rightParticlesActive = false;
    //            leftParticlesActive = false;
    //        }
    //        else if (move.x > 0f)
    //        {
    //            rightParticlesActive = true;
    //            leftParticlesActive = false;
    //        }
    //        else if (move.x < 0f)
    //        {
    //            rightParticlesActive = false;
    //            leftParticlesActive = true;
    //        }
    //    }
    //}

    public override void ApplyDriftBoost(Rigidbody kartRB, float boostImmediateForce, float boostTimeMultiplier, out bool boostActive, out bool driftBoostActive, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier)
    {
        base.ApplyDriftBoost(kartRB, boostImmediateForce, boostTimeMultiplier, out boostActive, out driftBoostActive, out maxBoosterTime, out maxBoosterMultiplier, boosterTime, boosterMultiplier);
        driftBoostActive = false;
    }
}
