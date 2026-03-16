using UnityEngine;

[CreateAssetMenu(fileName = "New Car Model", menuName = "Cars/New Car")]
public class CarModel : ScriptableObject
{
    public GameObject carVisual;
    public Sprite carIcon;
}
