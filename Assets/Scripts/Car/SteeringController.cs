using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringController : MonoBehaviour
{
    [SerializeField] private InputActionReference pedals;
    private float pedalsValue;
    [SerializeField] private InputActionReference steering;
    private float steeringValue;
    [SerializeField] private WheelCollider frontLeftWheel, frontRightWheel, backLeftWheel, backRightWheel;
    public float power = 500f;
    public float angle = 30f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    public float SpeedKmh;
    private void Start()
    {
        if(centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        pedalsValue = pedals.action.ReadValue<float>();
        steeringValue = steering.action.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        if (Mathf.Approximately(pedalsValue, 0))
        {
            backLeftWheel.motorTorque = 0f;
            backRightWheel.motorTorque = 0f;
        }
        else
        {
            backLeftWheel.motorTorque = pedalsValue * power;
            backRightWheel.motorTorque = pedalsValue * power;
        }
        frontLeftWheel.steerAngle = steeringValue * angle;
        frontRightWheel.steerAngle = steeringValue * angle;
        SpeedKmh = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);
    }
}
