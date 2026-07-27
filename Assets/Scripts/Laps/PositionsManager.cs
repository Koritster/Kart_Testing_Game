using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PositionsManager : NetworkBehaviour
{
    public static PositionsManager instance;

    public bool started;

    [SerializeField] private List<RaceCheckpoint> checkpoints;

    List<Kart> karts = new List<Kart>();
    bool tie;

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

    private void Start()
    {
        
    }

    void Update()
    {
        if(!started) return;

        //Calcular posiciones
        if (tie)
        {
            CalculatePositions();
        }
        //karts.Sort((a, b) => b.trackProgress.Value.CompareTo(a.trackProgress.Value));

    }

    public void CalculatePositions()
    {
        karts.Sort((a, b) =>
        {
            // 1. Vueltas
            int result = b.laps.Value.CompareTo(a.laps.Value);
            if (result != 0)
            {
                if (tie)
                {
                    tie = false;
                }

                return result;
            }

            int aCheckpoint = a.actualCheckpoint.Value;
            // 2. Checkpoint
            result = b.actualCheckpoint.Value.CompareTo(aCheckpoint);
            if (result != 0)
            {
                if (tie)
                {
                    tie = false;
                }

                return result;
            }

            tie = true;

            // 3. Distancia al siguiente checkpoint
            float distA = GetDistanceToNextCheckpoint(a, aCheckpoint);
            float distB = GetDistanceToNextCheckpoint(b, aCheckpoint);

            return distA.CompareTo(distB);
        });
    }

    float GetDistanceToNextCheckpoint(Kart _kart, int checkpoint)
    {
        float distancia = Vector3.Distance(_kart.transform.position, checkpoints[checkpoint + 1].transform.position);
        _kart.distanceToNextCheckpoint.Value = distancia;
        return distancia;
    }

    public void PlayerFinishedLap(Kart _kart)
    {
        _kart.laps.Value += 1;

        //Calcular si es la vuelta final

        _kart.actualCheckpoint.Value = 0;
        _kart.distanceToNextCheckpoint.Value = 0;

        //Checar los puntajes de todos los jugadores, si es que se ha ganado, llamar un RPC que actualice una lista añadiendo al jugador que haya terminado ya la carrera
        //Al final de la partida mostrar esa lista en orden para saber quién llegó después de quién
    }
    
    public int GetCheckpointCount()
    {
        return checkpoints.Count;
    }

    public void RegisterKart(Kart kart)
    {
        Debug.Log("Carro " + kart + " registrado");
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
