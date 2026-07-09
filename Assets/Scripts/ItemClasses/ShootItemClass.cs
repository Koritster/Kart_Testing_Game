using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Item", menuName = "Item/Proyectil")]
public class ShootItemClass : ItemClass
{
    [Header("Projectile Properties")]
    public float velocity;
    public int amount;
    public bool isHoming;
    public GameObject projectilePrefab;

    public override void UseItem(CarController caller)
    {
        base.UseItem(caller);
    }

    public override ShootItemClass GetShootItem() { return this; }
}
