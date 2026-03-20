using System.Collections.Generic;
using UnityEngine;

public class PositionsManager : MonoBehaviour
{
    public static PositionsManager instance;

    public bool started;

    [SerializeField] private List<RaceCheckpoint> checkpoints;

    List<Kart> karts = new List<Kart>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        for(int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].index = i;
        }
    }

    void Update()
    {
        if(!started) return;

        karts.Sort((a, b) => b.trackProgress.Value.CompareTo(a.trackProgress.Value));
    }

    public int GetCheckpointCount()
    {
        return checkpoints.Count;
    }

    public void RegisterKart(Kart kart)
    {
        if (!karts.Contains(kart))
        {
            karts.Add(kart);
        }
    }

    public int GetPosition(Kart kart)
    {
        return karts.IndexOf(kart) + 1;
    }
}
