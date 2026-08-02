using System.Globalization;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RaceCheckpoint : NetworkBehaviour
{
    public int index;

    private void Start()
    {
        Debug.Log(PositionsManager.instance.GetCheckpointCount());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<Kart>(out Kart _kart))
        {
            //Si no ha pasado por el anterior checkpoint no cuenta
            //Si es el último checkpoint, igual checa
            if ((_kart.actualCheckpoint.Value + 1) != index && (_kart.actualCheckpoint.Value != PositionsManager.instance.GetCheckpointCount() - 1)) return;

            Debug.Log("Cruzaste el checpoint " + index);

            //Si es la meta
            if (index == 0 && _kart.actualCheckpoint.Value == PositionsManager.instance.GetCheckpointCount() - 1)
            {
                Debug.Log("Cruzaste la meta");
                PositionsManager.instance.PlayerFinishedLap(_kart);
            }

            _kart.actualCheckpoint.Value = index;

            PositionsManager.instance.CalculatePositionsServerRpc();
        }
    }
}
