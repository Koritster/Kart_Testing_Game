using UnityEngine;

public class ReverseKartState : DriveKartState
{
    public override void ApplyReverse(Rigidbody kartRB, bool isReversing, bool isGrounded, float reverseForce, float airMultiplier, float maxBoosterMultiplier, float accelerationRate)
    {
        base.ApplyReverse(kartRB, isReversing, isGrounded, reverseForce, airMultiplier, maxBoosterMultiplier, accelerationRate);

        Vector3 velocity = Vector3.zero;

        if (isReversing)
        {
            velocity = -kartRB.transform.forward * reverseForce;
        }

        if (!isGrounded)
        {
            velocity *= airMultiplier;
        }

        kartRB.AddForce(velocity * maxBoosterMultiplier * accelerationRate, ForceMode.Force);
    }
}
