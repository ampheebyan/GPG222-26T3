using UnityEngine;

[CreateAssetMenu(fileName = "CarValue", menuName = "CarValues", order = 0)]
public class CarValues : ScriptableObject
{
    [Header("REMINDER THAT SPEED IS HANDLED USING M/S")]
    public float power = 1500f;
    public float brakePower = 12000f;
    public Vector2 angle = new (30, 60);
    public float maxSpeed = 33f;
}
