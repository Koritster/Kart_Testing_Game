using System.Collections.Generic;
using UnityEngine;

public class PositionsManager : MonoBehaviour
{
    public static PositionsManager instance;

    public bool started;

    [SerializeField] private List<RaceCheckpoint> checkpoints;

    List<PlayerNetworkKart> karts = new List<PlayerNetworkKart>();

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

        foreach (RaceCheckpoint rc in checkpoints)
        {
            rc.RestartCheckpoint();
        }
    }

    private void Start()
    {
        
    }

    void Update()
    {
        if(!started) return;

        karts.Sort((a, b) => b.trackProgress.Value.CompareTo(a.trackProgress.Value));
    }

    public void LocalPlayerPassedLap()
    {
        foreach (RaceCheckpoint rc in checkpoints)
        {
            rc.RestartCheckpoint();
        }

        //Checar los puntajes de todos los jugadores, si es que se ha ganado, llamar un RPC que actualice una lista añadiendo al jugador que haya terminado ya la carrera
        //Al final de la partida mostrar esa lista en orden para saber quién llegó después de quién
    }

    public void EnableLapCheckpoint()
    {
        checkpoints[0].RestartCheckpoint();
    }

    public int GetCheckpointCount()
    {
        return checkpoints.Count;
    }

    public void RegisterKart(PlayerNetworkKart kart)
    {
        if (!karts.Contains(kart))
        {
            karts.Add(kart);
        }
    }

    public int GetPosition(PlayerNetworkKart kart)
    {
        return karts.IndexOf(kart) + 1;
    }
}
