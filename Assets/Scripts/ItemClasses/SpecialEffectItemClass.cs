using UnityEngine;

[CreateAssetMenu(fileName = "New Special Effect Item", menuName = "Item/Efecto especial")]
public class SpecialEffectItemClass : ItemClass
{
    [Header("Special Effect Properties")]
    public float duration;
    public GameObject projectilePrefab;

    public override void UseItem(CarController caller)
    {
        base.UseItem(caller);
    }

    public override SpecialEffectItemClass GetSpecialEffectItem() { return this; }
}