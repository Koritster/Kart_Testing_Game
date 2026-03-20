using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class RaceCheckpoint : NetworkBehaviour
{
    [HideInInspector] public int index;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if(other.TryGetComponent<Kart>(out Kart _kart))
        {
            if (_kart.checkpointIndex == index)
            {
                _kart.checkpointIndex++;

                _kart.checkpointIndex = (_kart.checkpointIndex + 1) % PositionsManager.instance.GetCheckpointCount();

                if (_kart.checkpointIndex == 0)
                {
                    _kart.lap++;
                }
            }
        }
    }
}
