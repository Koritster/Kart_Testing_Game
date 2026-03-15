using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        LapManager lapManager = other.GetComponent<LapManager>();
        if (lapManager != null)
        {
            lapManager.PassCheckpoint(checkpointIndex);
        }
    }
}