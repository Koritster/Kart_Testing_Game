using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public interface IHitteable
{
    public abstract void OnHit();
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


