using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public interface IHitteable
{
    public abstract void OnHit();
}

public class Kart : HitteableBehaviour
{
    public NetworkVariable<float> trackProgress = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int lap;
    public int checkpointIndex;
    
    private List<Transform> checkpoints;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            PositionsManager.instance.RegisterKart(this);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        float progress = CalcularProgreso();

        trackProgress.Value = progress;

        int pos = PositionsManager.instance.GetPosition(this);
    }

    float CalcularProgreso()
    {
        Vector3 A = checkpoints[checkpointIndex].position;
        Vector3 B = checkpoints[(checkpointIndex + 1) % checkpoints.Count].position;
        Vector3 P = transform.position;

        Vector3 AB = B - A;
        Vector3 AP = P - A;

        float t = Vector3.Dot(AP, AB) / AB.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return lap * checkpoints.Count + checkpointIndex + t;
    }
}

public class HitteableBehaviour : NetworkBehaviour, IHitteable
{
    public static List<IHitteable> m_AllHitteables = new List<IHitteable>();

    public Transform m_MarkerPosition;

    public virtual void OnEnable()
    {
        m_AllHitteables.Add(this);
    }

    public virtual void OnDisable()
    {
        m_AllHitteables.Add(this);
    }

    public virtual void OnHit()
    {
        Debug.Log(gameObject.name + " ha sido golpeado");
    }

    public static List<HitteableBehaviour> GetAllExcept(HitteableBehaviour exception)
    {
        List<HitteableBehaviour> temp = new List<HitteableBehaviour>();

        foreach (HitteableBehaviour item in m_AllHitteables)
        {
            if (item == exception)
                continue;

            temp.Add(item);
        }

        return temp;
    }
}


