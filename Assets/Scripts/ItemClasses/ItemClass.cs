using UnityEngine;

public class ItemClass : ScriptableObject
{
    [Header("Item Properties")]
    public string name;
    public Sprite itemIcon;

    public virtual void UseItem(CarController caller)
    {
        Debug.Log("Used: " + name);
    }

    public virtual ItemClass GetItem() { return this; }
    public virtual ShootItemClass GetShootItem() { return null; }
    public virtual SpecialEffectItemClass GetSpecialEffectItem() { return null; }
}
