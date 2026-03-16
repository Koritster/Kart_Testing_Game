using UnityEngine;
using Unity.Netcode;

public class KartAIController : NetworkBehaviour
{
    private float itemTimer;
    private float itemUseDelay;
    private ItemClass currentItem;
    private HitteableBehaviour currentTarget;

    void Start()
    {
        itemUseDelay = Random.Range(3f, 8f);
    }

    void Update()
    {
        if (!IsServer) return;

        HandleItemUse();
    }

    public void ReceiveItem(ItemClass item)
    {
        Debug.Log("IA recibió item");

        currentItem = item;
    }

    bool HasItem()
    {
        return currentItem != null;
    }

    void UseItem()
    {
        if (currentItem == null) return;

        if (currentItem.GetShootItem() != null)
        {
            ShootItemClass projectile = currentItem.GetShootItem();

            Shoot(projectile, currentTarget);
        }

        currentItem = null;
    }

    //Disparo de la IA
    [SerializeField] Transform shootingTransform;
    void Shoot(ShootItemClass projectileData, HitteableBehaviour objective)
    {
        NetworkObjectReference targetRef = default;

        if (objective != null)
            targetRef = new NetworkObjectReference(objective.NetworkObject);

        ShootServerRpc(projectileData.itemId, targetRef, transform.forward);
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(int projectileId, NetworkObjectReference targetRef, Vector3 direction)
    {
        ShootItemClass projectileData = ItemDatabase.Instance.GetProjectileItemById(projectileId);

        GameObject projectile = Instantiate(
            projectileData.projectilePrefab,
            shootingTransform.position,
            Quaternion.identity
        );

        projectile.GetComponent<NetworkObject>().Spawn();

        HitteableBehaviour objective = null;

        if (targetRef.TryGet(out NetworkObject targetNetObj))
            objective = targetNetObj.GetComponent<HitteableBehaviour>();

        projectile.GetComponent<ProjectileBehaviour>()
            .SetProperties(objective, direction, projectileData.velocity, projectileData.isHoming);
    }

    //Decide cuándo usar ítems.
    void HandleItemUse()
    {
        itemTimer += Time.deltaTime;

        if (itemTimer >= itemUseDelay && HasItem())
        {
            currentTarget = GetNearestTarget();

            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (dist < 25f && TargetIsInFront(currentTarget))
                {
                    Debug.Log("IA disparando");

                    UseItem();

                    itemTimer = 0;
                    itemUseDelay = Random.Range(4f, 8f);
                }
            }
        }
    }

    //función que busca enemigos
    HitteableBehaviour GetNearestTarget()
    {
        float minDistance = Mathf.Infinity;
        HitteableBehaviour nearest = null;

        foreach (HitteableBehaviour h in HitteableBehaviour.GetAllExcept(GetComponent<HitteableBehaviour>()))
        {
            float dist = Vector3.Distance(transform.position, h.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = h;
            }
        }

        return nearest;
    }

    //permite que la IA dispare solo si el enemigo está enfrente
    bool TargetIsInFront(HitteableBehaviour target)
    {
        Vector3 forward = transform.forward;
        Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

        float dot = Vector3.Dot(forward, dirToTarget);

        return dot > 0.5f;
    }
}