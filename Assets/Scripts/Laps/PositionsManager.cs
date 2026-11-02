//#define DEBUG_FEATURE
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PositionsManager : MonoBehaviour
{
    public static PositionsManager instance;

#if DEBUG_FEATURE
    public NetworkVariable<bool> started = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
#endif

    public bool started {  get; private set; }

    [SerializeField] private List<RaceCheckpoint> checkpoints;

    //HERI
    [SerializeField]
    private float fallLimitYValue = -100f;
    public StateMachineManager stateMachineManager { get; private set; }
    //HERI

    List<NewKart> karts = new List<NewKart>();
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
        //HERI
        stateMachineManager = new StateMachineManager(checkpoints.ToArray());

        Transform checkpointParent = GameObject.Find("Checkpoints").transform;
        //HERI
    }

    void Update()
    {
#if DEBUG_FEATURE
if (!IsServer) return;
        if (!started.Value) return;
#endif

        //Calcular posiciones
        if (tie)
        {
            CalculatePositionsServerRpc();
        }
        //karts.Sort((a, b) => b.trackProgress.Value.CompareTo(a.trackProgress.Value));

        //HERI
        stateMachineManager.CheckForFallenKarts(karts.ToArray(), fallLimitYValue);
        //HERI
    }

    public void SetStarted(bool started)
    {
        this.started = started;
    }    

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
    public void CalculatePositionsServerRpc()
    {
        karts.Sort((a, b) =>
        {
            // 1. Vueltas
            int result = b.laps.CompareTo(a.laps);
            if (result != 0)
            {
                if (tie)
                {
                    tie = false;
                }

                Debug.Log("Están en vueltas diferentes");
                return result;
            }

            int aCheckpoint = a.actualCheckpoint;
            // 2. Checkpoint
            result = b.actualCheckpoint.CompareTo(aCheckpoint);
            if (result != 0)
            {
                if (tie)
                {
                    tie = false;
                }

                Debug.Log("Están en checkpoints diferentes: " + aCheckpoint + " - " + b.actualCheckpoint);

                return result;
            }

            tie = true;
            Debug.Log("Hay empate");

            // 3. Distancia al siguiente checkpoint
            float distA = GetDistanceToNextCheckpoint(a, aCheckpoint);
            float distB = GetDistanceToNextCheckpoint(b, aCheckpoint);

            return distA.CompareTo(distB);
        });

        for (int i = 0; i < karts.Count; i++)
        {
            karts[i].AddPosition(1);
        }
    }

    float GetDistanceToNextCheckpoint(NewKart _kart, int checkpoint)
    {
        float distancia = Vector3.Distance(_kart.transform.position, checkpoints[checkpoint + 1].transform.position);
        return distancia;
    }

    public void PlayerFinishedLap(NewKart _kart)
    {
        _kart.AddLaps(1);

        //Calcular si es la vuelta final

        _kart.SetActualCheckpoint(0);

        //Checar los puntajes de todos los jugadores, si es que se ha ganado, llamar un RPC que actualice una lista añadiendo al jugador que haya terminado ya la carrera
        //Al final de la partida mostrar esa lista en orden para saber quién llegó después de quién
    }
    
    public int GetCheckpointCount()
    {
        return checkpoints.Count;
    }

    public void RegisterKart(NewKart kart)
    {
        Debug.Log("Carro " + kart + " registrado");
        if (!karts.Contains(kart))
        {
            karts.Add(kart);
        }
    }

    public int GetPosition(NewKart kart)
    {
        return kart.position;
    }
}
