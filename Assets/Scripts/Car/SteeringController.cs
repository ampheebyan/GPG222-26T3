using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum GearShift
{
    Parked,
    Neutral,
    Reverse,
    Drive
}
public class SteeringController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) enabled = false;
    }
    // https://docs.unity3d.com/6000.0/Documentation/Manual/WheelColliderTutorial.html using this btw, never used WheelColliders before tbh
    
    public float accelerateValue;
    public float brakeValue;
    public float steeringValue;
    [SerializeField] private WheelCollider frontLeftWheel, frontRightWheel, backLeftWheel, backRightWheel;
    public GearShift gear = GearShift.Parked;
    public float power = 1500f;
    public Vector2 angle = new (30, 60);
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    public float speedKmh;
    private bool _bLock = false;
    private void Start()
    {
        if(centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        //accelerateValue = accelerate.action.ReadValue<float>();
        //brakeValue = brake.action.ReadValue<float>();
        //steeringValue = steering.action.ReadValue<float>();
    }


    private void FixedUpdate()
    {
        speedKmh = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);
        float fSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float sFactor = Mathf.InverseLerp(0, 120f, Mathf.Abs(fSpeed));

        float motorTorque = Mathf.Lerp(power, 0, sFactor);
        float steerRange = Mathf.Lerp(angle.x, angle.y, sFactor);

        frontLeftWheel.steerAngle = steeringValue * steerRange;
        frontRightWheel.steerAngle = steeringValue * steerRange;
        if (gear == GearShift.Parked)
        {
            backLeftWheel.motorTorque = 0f;
            backRightWheel.motorTorque = 0f;
            backLeftWheel.brakeTorque = 9001f;
            backRightWheel.brakeTorque = 9001f;
            return;
        }

        
        if (brakeValue > 0)
        {
            Debug.Log("Brake");

            if (speedKmh > 0f && _bLock == false)
            {
                backLeftWheel.motorTorque = 0f;
                backRightWheel.motorTorque = 0f;
                backLeftWheel.brakeTorque = brakeValue * motorTorque;
                backRightWheel.brakeTorque = brakeValue * motorTorque;
            }
            else
            {
                _bLock = true;
                backLeftWheel.motorTorque = -brakeValue * motorTorque;
                backRightWheel.motorTorque = -brakeValue * motorTorque;
                backLeftWheel.brakeTorque = 0f;
                backRightWheel.brakeTorque = 0f;
            }
        }
        else
        {
            if (Mathf.Approximately(accelerateValue, 0f))
            {
                backLeftWheel.motorTorque = 0f;
                backRightWheel.motorTorque = 0f;
            }
            if (gear == GearShift.Drive)
            {
                _bLock = false;
                Debug.Log("Drive");
                Debug.Log(accelerateValue);
                backLeftWheel.motorTorque = accelerateValue * motorTorque;
                backRightWheel.motorTorque = accelerateValue * motorTorque;
                backLeftWheel.brakeTorque = 0f;
                backRightWheel.brakeTorque = 0f;
            } else if (gear == GearShift.Reverse)
            {
                _bLock = false;
                Debug.Log("Reverse");
                Debug.Log(accelerateValue);
                backLeftWheel.motorTorque = -accelerateValue * motorTorque;
                backRightWheel.motorTorque = -accelerateValue * motorTorque;
                backLeftWheel.brakeTorque = 0f;
                backRightWheel.brakeTorque = 0f;
            } 
        }
    }
}
