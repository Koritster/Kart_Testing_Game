using UnityEngine;

public class LapManager : MonoBehaviour
{
    public int totalLaps = 3;
    public int totalCheckpoints;

    public int currentLap = 0;

    int nextCheckpointIndex = 0;
    bool canFinishLap = false;

    void Start()
    {
        currentLap = 1;
    }

    public void PassCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex == nextCheckpointIndex)
        {
            nextCheckpointIndex++;

            if (nextCheckpointIndex >= totalCheckpoints)
            {
                canFinishLap = true;
            }
        }
    }

    public void TryCompleteLap()
    {
        if (!canFinishLap) return;

        currentLap++;
        nextCheckpointIndex = 0;
        canFinishLap = false;

        if (currentLap > totalLaps)
        {
            FinishRace();
        }
    }

    void FinishRace()
    {
        Debug.Log("Carrera terminada");
    }
}