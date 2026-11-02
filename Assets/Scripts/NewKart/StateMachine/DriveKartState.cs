using UnityEngine;
using UnityEngine.Windows;

public class DriveKartState : KartState
{
    public override void ApplyRotation(Rigidbody kartRB, Vector3 centerOfMass, Vector3 input, bool isThrottling, float raycastDistance, float rotationForce, float targetSpeed, LayerMask rotationLayers, LayerMask gravityLayers)
    {
        base.ApplyRotation(kartRB, centerOfMass, input, isThrottling, raycastDistance, rotationForce, targetSpeed, rotationLayers, gravityLayers);

        RaycastHit hit;

        if (kartRB.linearVelocity.magnitude < targetSpeed * 0.15f)
        {
            kartRB.rotation = Quaternion.Lerp(kartRB.rotation, Quaternion.LookRotation(input), Time.fixedDeltaTime * rotationForce);
            return;
        }

        if (Physics.Raycast(centerOfMass, kartRB.transform.forward, out hit, raycastDistance * 1.5f, rotationLayers))
        {
            Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion inputRotation = Quaternion.LookRotation(input);
            Quaternion combinedRotation = gravityRotation * inputRotation;
            kartRB.rotation = Quaternion.Lerp(kartRB.rotation, combinedRotation, Time.fixedDeltaTime * rotationForce * 1.25f);
            return;
        }

        if (Physics.Raycast(centerOfMass, -kartRB.transform.up, out hit, raycastDistance, rotationLayers))
        {
            Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion inputRotation = Quaternion.LookRotation(input);
            Quaternion combinedRotation = gravityRotation * inputRotation;
            kartRB.rotation = Quaternion.Lerp(kartRB.rotation, combinedRotation, Time.fixedDeltaTime * rotationForce);
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
