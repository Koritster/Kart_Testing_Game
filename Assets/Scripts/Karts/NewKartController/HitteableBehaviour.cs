using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HitteableBehaviour : NetworkBehaviour
{
    public static List<HitteableBehaviour> m_AllHitteables = new List<HitteableBehaviour>();

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
        //Logica de ser golpeado
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
