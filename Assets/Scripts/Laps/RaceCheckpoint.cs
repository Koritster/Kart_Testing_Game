//#define DEBUG_FEATURE
using System.Globalization;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{
    public int index;

    private void Start()
    {
        Debug.Log(PositionsManager.instance.GetCheckpointCount());
    }

    private void OnTriggerEnter(Collider other)
    {
        #if DEBUG_FEATURE
        if (!IsServer) return;
#endif

        if (other.TryGetComponent<NewKart>(out NewKart _kart))
        {
            //Si no ha pasado por el anterior checkpoint no cuenta
            //Si es el último checkpoint, igual checa
            if ((_kart.actualCheckpoint + 1) != index && (_kart.actualCheckpoint != PositionsManager.instance.GetCheckpointCount() - 1)) return;

            Debug.Log("Cruzaste el checkpoint " + index);

            //Si es la meta
            if (index == 0 && _kart.actualCheckpoint == PositionsManager.instance.GetCheckpointCount() - 1)
            {
                Debug.Log("Cruzaste la meta");
                PositionsManager.instance.PlayerFinishedLap(_kart);
            }

            _kart.SetActualCheckpoint(index);

            PositionsManager.instance.CalculatePositionsServerRpc();
        }
    }
}
