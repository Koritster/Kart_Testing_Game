using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ProjectileBehaviour : NetworkBehaviour
{
    private Rigidbody rb;

    private HitteableBehaviour m_Objective;
    private float speed;
    private bool isHoming;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetProperties(HitteableBehaviour m_Objective, Vector3 m_Direction, float speed, bool isHoming)
    {
        this.m_Objective = m_Objective;
        this.speed = speed;
        this.isHoming = isHoming;

        transform.forward = m_Direction;

        if (!isHoming || m_Objective == null)
        {
            rb.linearVelocity = m_Direction * speed;
        }
    }
    
    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!isHoming || m_Objective == null) return;

        //Vector3 dir = (m_Objective.transform.position - transform.position).normalized;
        Vector3 dir = (m_Objective.GetComponent<Collider>().bounds.center - transform.position).normalized;

        Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, speed * Time.fixedDeltaTime, 0f);

        rb.linearVelocity = newDir * speed;
        transform.forward = newDir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if(other.TryGetComponent<HitteableBehaviour>(out HitteableBehaviour m_HittedObject))
        {
            m_HittedObject.OnHit();
            Debug.Log("Golpeaste a algo!");
            NetworkObject.Despawn(true);
            return;
        }

        if(!isHoming)
        {
            NetworkObject.Despawn(true);
        }
    }
}
