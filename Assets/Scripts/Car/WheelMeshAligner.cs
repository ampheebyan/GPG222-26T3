using System;
using UnityEngine;

public class WheelMeshAligner : MonoBehaviour
{
    // This is kind of 'taken'-ish from the Unity documentation on the WheelCollider component.
    [SerializeField] private WheelCollider wheel;

    private void FixedUpdate()
    {
        if (wheel)
        {
            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
