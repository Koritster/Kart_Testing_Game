using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MoveToWaypoints : NetworkBehaviour
{
    public float speed;

    private Rigidbody rb;
    private Transform target;
    private CarController car;
    bool canMove;

    public int currentLap = 0;
    public int totalLaps = 3;
    float itemTimer;
    float itemUseDelay;
    private HitteableBehaviour currentTarget;

    void Start()
    {
        //GetComponents
        rb = GetComponent<Rigidbody>();

        //Waypoints
        target = Waypoints.waypoints[0];
        Debug.Log("Inicializando waypoints");
        car = GetComponent<CarController>();
        //Tiempo para que use un item
        itemUseDelay = Random.Range(3f, 8f);
    }

    public void ActivateMovement()
    {
        canMove = true;
        Debug.Log("Comenzando a moverse");
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!canMove) return;

        Movement();
        HandleItemUse();
    }


    Vector3 dir;

    private void Movement()
    {
        //Move Caracter
        dir = target.position - transform.position;
        rb.MovePosition(transform.position + dir.normalized * (speed / 15) * Time.deltaTime);

        transform.forward = Vector3.Slerp(transform.forward, dir, Time.deltaTime * 20f);

        if (Vector3.Distance(transform.position, target.position) < 5f)
        {
            NextWaypoint();
        }
    }

    HitteableBehaviour GetNearestTarget()
    {
        float minDistance = Mathf.Infinity;
        HitteableBehaviour nearest = null;

        foreach (HitteableBehaviour h in HitteableBehaviour.GetAllExcept(car))
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

    bool TargetIsInFront(HitteableBehaviour target)
    {
        Vector3 forward = transform.forward;
        Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

        float dot = Vector3.Dot(forward, dirToTarget);

        return dot > 0.5f;
    }

    void HandleItemUse()
    {
        if (car == null) return;

        itemTimer += Time.deltaTime;

        if (itemTimer >= itemUseDelay && car.HasItem())
        {
            currentTarget = GetNearestTarget();

            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (dist < 25f)
                {
                    Debug.Log("IA disparando a objetivo");

                    car.AimObjective();
                    car.UseItem();

                    itemTimer = 0;
                    itemUseDelay = Random.Range(4f, 8f);
                }
            }
        }
    }

    private int wavepointIndex = 0;

    //Waypoints de la pista y contador de vueltas
    private void NextWaypoint()
    {
        if (wavepointIndex >= Waypoints.waypoints.Length - 1)
        {
            wavepointIndex = 0;
            currentLap++;

            Debug.Log("IA completó vuelta: " + currentLap);

            if (currentLap >= totalLaps)
            {
                FinishRace();
                return;
            }

            target = Waypoints.waypoints[wavepointIndex];
        }
        else
        {
            wavepointIndex++;
            target = Waypoints.waypoints[wavepointIndex];
        }
    }
    // Detiene el movimiento de la IA cuando termina la carrera
    void FinishRace()
    {
        canMove = false;
        Debug.Log("Terminó la carrera");
    }
}
