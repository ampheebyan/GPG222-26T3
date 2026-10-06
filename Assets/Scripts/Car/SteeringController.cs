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
    [Header("VVVV DO NOT TOUCH VVVV")]
    public float accelerateValue;
    public float brakeValue;
    public float steeringValue;
    [Header("Car Information")]
    public float speedKmh;
    public GearShift gear = GearShift.Parked;
    [Header("Car Variables")]
    public CarValues cVars;
    [SerializeField] private WheelCollider frontLeftWheel, frontRightWheel, backLeftWheel, backRightWheel;
    [Header("Extra Assignables")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    private bool _bLock = false;
    private void Start()
    {
        if (!cVars)
        {
            throw new Exception("How did you even get to this point?");
        }
        if(centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
    }

    private void FixedUpdate()
    {
        speedKmh = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);
        float fSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float sFactor = Mathf.InverseLerp(0, 120f, Mathf.Abs(fSpeed));

        float motorTorque = Mathf.Lerp(cVars.power, 0, sFactor);
        float steerRange = Mathf.Lerp(cVars.angle.x, cVars.angle.y, sFactor);

        frontLeftWheel.steerAngle = steeringValue * steerRange;
        frontRightWheel.steerAngle = steeringValue * steerRange;
        if (gear == GearShift.Parked)
        {
            backLeftWheel.motorTorque = 0f;
            backRightWheel.motorTorque = 0f;
            rb.linearVelocity /= 4;
            backLeftWheel.brakeTorque = 15000f;
            backRightWheel.brakeTorque = 15000f;
            return;
        }

        
        if (brakeValue > 0)
        {
            Debug.Log("Brake");

            if (speedKmh > 0f && _bLock == false)
            {
                backLeftWheel.motorTorque = 0f;
                backRightWheel.motorTorque = 0f;
                backLeftWheel.brakeTorque = brakeValue * cVars.brakePower;
                backRightWheel.brakeTorque = brakeValue * cVars.brakePower;
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
