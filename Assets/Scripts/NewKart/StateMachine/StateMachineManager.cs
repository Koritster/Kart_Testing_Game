using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class StateMachineManager
{
    RaceCheckpoint[] raceCheckpoints;

    public StateMachineManager(RaceCheckpoint[] checkpoints)
    {
        raceCheckpoints = checkpoints;
    }

    public void CheckForFallenKarts(NewKart[] karts, float fallLimitYValue)
    {
        for (int i = 0; i < karts.Length; i++)
        {
            if(karts[i].transform.position.y <= fallLimitYValue)
            {
                //Debug.Log("ENTERS HERE");
                karts[i].transform.position = raceCheckpoints[karts[i].actualCheckpoint.Value].transform.position;
                karts[i].transform.forward = raceCheckpoints[karts[i].actualCheckpoint.Value].transform.forward;
            }
        }
    }
}
