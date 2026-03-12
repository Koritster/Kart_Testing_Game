using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemBlock : MonoBehaviour
{
    public List<ItemClass> itemsPool = new List<ItemClass>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<CarController>(out CarController m_Car))
        {
            m_Car.ReceiveItem(GetRandomItem());

            GetItemBlockServerRpc();
        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetItemBlockServerRpc()
    {
        GetItemBlockClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GetItemBlockClientRpc()
    {
        gameObject.SetActive(false);
    }

    private ItemClass GetRandomItem()
    {
        int item = Random.Range(0, itemsPool.Count);
        return itemsPool[item];
    }
}
