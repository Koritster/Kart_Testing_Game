using Unity.Netcode;
using UnityEngine;

public class ItemBlock : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<CarController>(out CarController m_Car))
        {

        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetItemBlockServerRpc()
    {

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GetItemBlockClientRpc()
    {

    }
}
