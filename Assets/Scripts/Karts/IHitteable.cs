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
    public NetworkVariable<int> laps = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> actualCheckpoint = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> Position = new(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Debug.Log("Spawneando carro");
        if (IsClient)
        {
            PositionsManager.instance.RegisterKart(this);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        //float progress = ;

        //trackProgress.Value = progress;

        int pos = PositionsManager.instance.GetPosition(this);
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


