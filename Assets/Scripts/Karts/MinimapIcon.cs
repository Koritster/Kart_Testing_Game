using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class MinimapIcon : NetworkBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject border;

    public void Setup(string _kart)
    {
        CarModel carModel = CarSelector.instance.SearchKartModelDataByName(_kart);

        icon.GetComponent<MeshRenderer>().material = carModel.carIconMinimap;
        border.SetActive(false);

        if (IsOwner)
        {
            border.SetActive(true);
        }
    }
}
