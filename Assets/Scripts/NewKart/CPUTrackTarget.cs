using UnityEngine;

public class CPUTrackTarget : MonoBehaviour
{
    public int index;
    [Range(0, 100)]
    public int weight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 10.0f);
    }
}
