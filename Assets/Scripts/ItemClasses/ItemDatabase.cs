using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [SerializeField] private List<ItemClass> m_DatabaseItems = new List<ItemClass>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public ShootItemClass GetProjectileItemById(int id)
    {
        foreach(var item in m_DatabaseItems)
        {
            if (item.itemId == id)
                return item.GetShootItem();
        }

        return null;
    }

    public SpecialEffectItemClass GetSpecialEffectItemById(int id)
    {
        foreach (var item in m_DatabaseItems)
        {
            if (item.itemId == id)
                return item.GetSpecialEffectItem();
        }

        return null;
    }
}
