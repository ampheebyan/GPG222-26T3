using UnityEngine;

public class ObstacleTag : MonoBehaviour
{
    [Header("This is a randomised value that will be dealt to the car health upon impact!")]
    public Vector2 damageRange = new(1, 4);

    [Header("This is the speed the car has to be going for reasonable damage to be dealt via this object!")]
    public float speedRequirement = 10f;
}
