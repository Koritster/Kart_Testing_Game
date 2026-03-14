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
