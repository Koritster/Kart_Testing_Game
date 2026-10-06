using UnityEngine;
using UnityEngine.Windows;

public class DriveKartState : KartState
{
    public override void ApplyRotation(Rigidbody kartRB, Vector3 centerOfMass, Vector3 input, float raycastDistance, float rotationForce, LayerMask raycastLayers)
    {
        base.ApplyRotation(kartRB, centerOfMass, input, raycastDistance, rotationForce, raycastLayers);

        RaycastHit hit;
        if (Physics.Raycast(centerOfMass, kartRB.transform.forward, out hit, raycastDistance, raycastLayers) || Physics.Raycast(centerOfMass, -kartRB.transform.up, out hit, raycastDistance, raycastLayers))
        {
            Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion inputRotation = Quaternion.LookRotation(input);
            Quaternion combinedRotation = gravityRotation * inputRotation;
            kartRB.rotation = Quaternion.Lerp(kartRB.rotation, combinedRotation, Time.fixedDeltaTime * rotationForce);
        }
        else
        {
            kartRB.rotation = Quaternion.Lerp(kartRB.rotation, Quaternion.LookRotation(input), Time.fixedDeltaTime * rotationForce);
        }
    }

    public override void ApplyBoost(Rigidbody kartRB, float boostImmediateForce, float boostTimeMultiplier, out bool boostActive, out float maxBoosterTime, out float maxBoosterMultiplier, float boosterTime, float boosterMultiplier)
    {
        kartRB.AddForce(kartRB.linearVelocity.normalized * boostImmediateForce, ForceMode.VelocityChange);
        maxBoosterTime = boosterTime * boostTimeMultiplier;
        maxBoosterMultiplier = boosterMultiplier;
        boostActive = true;
    }
}
