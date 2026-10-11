using UnityEngine;

[CreateAssetMenu(fileName = "Car Settings", menuName = "Car Settings", order = 0)]
public class CarSettings : ScriptableObject
{
    [Header("REMINDER THAT SPEED IS HANDLED USING M/S!!!!")]
    public float power = 1500f;
    public float brakePower = 12000f;
    public Vector2 angle = new (30, 60);
    public float maxSpeed = 33f;
}
