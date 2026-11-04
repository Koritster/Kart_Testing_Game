using UnityEngine;
using UnityEngine.Windows;

public class DriveKartState : KartState
{
    public override void ApplyRotation(Rigidbody kartRB, Transform rightWheel, Transform leftWheel, Vector3 centerOfMass, Vector3 input, Vector3 normal, bool contact, bool isThrottling, bool isGrounded, int contactLayer, float raycastDistance, float rotationForce, float targetSpeed, LayerMask rotationLayers, LayerMask gravityLayers)
    {
        base.ApplyRotation(kartRB, rightWheel, leftWheel, centerOfMass, input, normal, contact, isThrottling, isGrounded, contactLayer, raycastDistance, rotationForce, targetSpeed, rotationLayers, gravityLayers);

        //RaycastHit hit;
        rightWheel.transform.rotation = Quaternion.LookRotation(input);
        leftWheel.transform.rotation = Quaternion.LookRotation(input);

        //if (contact && kartRB.linearVelocity.magnitude < targetSpeed * 0.05f && (gravityLayers & (1 << contactLayer)) == 0)
        //{
        //    kartRB.rotation = Quaternion.Slerp(kartRB.rotation, Quaternion.LookRotation(input), Time.fixedDeltaTime * rotationForce * 0.25f);
        //    return;
        //}

        //if (isGrounded && Physics.Raycast(centerOfMass, -kartRB.transform.up, out hit, raycastDistance, rotationLayers))
        //{
        //    Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        //    Quaternion inputRotation = Quaternion.LookRotation(input);
        //    Quaternion combinedRotation = gravityRotation * inputRotation;
        //    kartRB.rotation = Quaternion.Slerp(kartRB.rotation, combinedRotation, Time.fixedDeltaTime * rotationForce);
        //    return;
        //}

        if (contact && (rotationLayers & (1 << contactLayer)) != 0)
        {
            //Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, normal);
            Quaternion inputRotation = Quaternion.LookRotation(input, normal);
            //Quaternion combinedRotation = gravityRotation * inputRotation;
            kartRB.transform.localRotation = Quaternion.Lerp(kartRB.transform.localRotation, inputRotation, Time.fixedDeltaTime * rotationForce * 1.5f);
            return;
        }

        kartRB.rotation = Quaternion.Lerp(kartRB.rotation, Quaternion.LookRotation(input), Time.fixedDeltaTime * rotationForce);
    }

    public override void ApplyBoost(Rigidbody kartRB, float boostImmediateForce, float boostTimeMultiplier, out bool boostActive, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier)
    {
        kartRB.AddForce(kartRB.linearVelocity.normalized * boostImmediateForce, ForceMode.VelocityChange);
        maxBoosterTime = boosterTime * boostTimeMultiplier;
        maxBoosterMultiplier = boosterMultiplier;
        boostActive = true;
    }
}
