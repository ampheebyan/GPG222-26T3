using UnityEngine;

[CreateAssetMenu(fileName = "CarValue", menuName = "CarValues", order = 0)]
public class CarValues : ScriptableObject
{
    public float power = 1500f;
    public float brakePower = 12000f;
    public Vector2 angle = new (30, 60);
}
