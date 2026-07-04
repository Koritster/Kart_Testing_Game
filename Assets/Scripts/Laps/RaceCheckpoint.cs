using System.Globalization;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RaceCheckpoint : NetworkBehaviour
{
    public int index;

    private bool hasBeenPassed;

    private void Awake()
    {
        if(index == 0)
        {
            hasBeenPassed = true;
        }
    }

    private void Start()
    {
        Debug.Log(PositionsManager.instance.GetCheckpointCount());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (hasBeenPassed) return;

        if(other.TryGetComponent<PlayerNetworkKart>(out PlayerNetworkKart _kart))
        {
            //Si no ha pasado por el anterior checkpoint no cuenta
            if ((_kart.actualCheckpoint + 1) != index) return;

            hasBeenPassed = true;
            Debug.Log("Cruzaste el checpoint " + index);

            //Si es el ultimo, habilitar la meta
            if (index == PositionsManager.instance.GetCheckpointCount() - 1)
            {
                Debug.Log("Ultimo checkpoint");
                PositionsManager.instance.EnableLapCheckpoint();
            }

            //Si es la meta
            if (index == 0)
            {
                Debug.Log("Cruzaste la meta");
                _kart.trackProgress.Value += 1000f;
                PositionsManager.instance.LocalPlayerPassedLap();
            }

            _kart.actualCheckpoint = index;
        }
    }

    /*public float CalculateDistanceToNextCheckpoint()
    {
        if()
    }*/

    public void RestartCheckpoint()
    {
        hasBeenPassed = false;
        
        if (index == 0)
        {
            hasBeenPassed = true;
        }
    }
}
