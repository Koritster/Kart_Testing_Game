using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MoveToWaypoints : NetworkBehaviour
{
    public float speed;

    private Rigidbody rb;
    private Transform target;

    bool canMove;

    void Start()
    {
        //GetComponents
        rb = GetComponent<Rigidbody>();

        //Waypoints
        target = Waypoints.waypoints[0];
        Debug.Log("Inicializando waypoints");
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

    private void NextWaypoint()
    {
        if (wavepointIndex >= Waypoints.waypoints.Length - 1)
        {
            wavepointIndex = 0;
            target = Waypoints.waypoints[wavepointIndex];
        }
        else
        {
            wavepointIndex++;
            target = Waypoints.waypoints[wavepointIndex];
        }
    }
}
