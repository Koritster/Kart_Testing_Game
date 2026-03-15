using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LapManager lapManager = other.GetComponent<LapManager>();
        if (lapManager != null)
        {
            lapManager.TryCompleteLap();
        }
    }
}