using Unity.Netcode;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    private Rigidbody rb;
    private HitteableBehaviour m_Objective;
    private float speed;
    private bool isHoming;

    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    private int currentBounces = 0;
    private float lastBounceTime;

    private int roadLayer;
    private int defaultLayer;
    private int wallLayer;

    private void Awake()
    {
        roadLayer = LayerMask.NameToLayer("RGSK_Road");
        defaultLayer = LayerMask.NameToLayer("Default");
        wallLayer = LayerMask.NameToLayer("InviibleWall");
    }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        currentBounces = 0;
        lastBounceTime = 0f;
    }

    public void SetProperties(HitteableBehaviour m_Objective, Vector3 m_Direction, float speed, bool isHoming)
    {
        this.m_Objective = m_Objective;
        this.speed = speed;
        this.isHoming = isHoming;

        Debug.Log($"Proyectil creado. ¿Es dirigido (Homing)?: {this.isHoming}");

        transform.forward = m_Direction.normalized;
        Vector3 initialVelocity = transform.forward * speed;
        initialVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = initialVelocity;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (isHoming && m_Objective != null)
        {
            Vector3 dir = (m_Objective.GetComponent<Collider>().bounds.center - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, speed * Time.fixedDeltaTime, 0f);
            Vector3 targetVelocity = newDir * speed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
            transform.forward = newDir;
        }
        else
        {
            // Solo forzamos la velocidad si no acabamos de rebotar
            if (Time.time - lastBounceTime > 0.1f)
            {
                Vector3 targetVelocity = transform.forward * speed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        GameObject other = collision.gameObject;
        Debug.Log($"¡Colisión detectada con: {other.name} | Capa: {LayerMask.LayerToName(other.layer)}!");

        if (other.TryGetComponent<HitteableBehaviour>(out HitteableBehaviour m_HittedObject))
        {
            m_HittedObject.OnHit();
            DespawnProjectile();
            return;
        }

        if (other.layer == wallLayer || other.layer == defaultLayer)
        {
            if (isHoming)
            {
                Debug.Log("Proyectil DIRIGIDO chocó con pared");
                DespawnProjectile();
                return;
            }

            Debug.Log("Proyectil NORMAL chocó con pared.");

            if (Time.time - lastBounceTime < 0.1f) return;
            lastBounceTime = Time.time;

            currentBounces++;
            Debug.Log($"Rebote número: {currentBounces} de {maxBounces}");

            if (currentBounces > maxBounces)
            {
                DespawnProjectile();
            }
            else
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 reflectDir = Vector3.Reflect(transform.forward, contact.normal);
                reflectDir.y = 0;
                reflectDir.Normalize();

                if (reflectDir != Vector3.zero)
                {
                    transform.forward = reflectDir;
                    rb.position += contact.normal * 0.2f;
                    Vector3 newVel = reflectDir * speed;
                    newVel.y = rb.linearVelocity.y;
                    rb.linearVelocity = newVel;
                }
            }
            return;
        }

        if (other.layer == roadLayer) return;

        if (!isHoming)
        {
            DespawnProjectile();
        }
    }

    private void DespawnProjectile()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}