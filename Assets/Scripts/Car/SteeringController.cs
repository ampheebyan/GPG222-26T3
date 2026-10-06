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
    // All movement is handled on server so disable this
    public override void OnNetworkSpawn()
    {
        if (!IsServer) enabled = false;
    }
    // https://docs.unity3d.com/6000.0/Documentation/Manual/WheelColliderTutorial.html using this btw, never used WheelColliders before tbh
    [Header("VVVV DO NOT TOUCH VVVV")]
    public float accelerateValue;
    public float brakeValue;
    public float steeringValue;
    [SerializeField] private float fSpeed;
    [SerializeField] private float sFactor;
    [Header("Car Information")]
    public float speedMS;
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
        speedMS = Mathf.RoundToInt(rb.linearVelocity.magnitude);
        fSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        sFactor = Mathf.InverseLerp(0, cVars.maxSpeed, Mathf.Abs(fSpeed));
        float motorTorque = Mathf.Lerp(cVars.power, 0, sFactor);
        float steerRange = Mathf.Lerp(cVars.angle.x, cVars.angle.y, sFactor);
        frontLeftWheel.steerAngle = steeringValue * steerRange;
        frontRightWheel.steerAngle = steeringValue * steerRange;
        if (gear == GearShift.Parked)
        {
            backLeftWheel.motorTorque = 0f;
            backRightWheel.motorTorque = 0f;
            backLeftWheel.brakeTorque = cVars.brakePower;
            backRightWheel.brakeTorque = cVars.brakePower;
            return;
        }
        if (brakeValue > 0)
        {
            if (speedMS > 0f && _bLock == false)
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
            if (Mathf.Approximately(accelerateValue, 0f) && Mathf.Approximately(speedMS, 3f))
            {
                backLeftWheel.brakeTorque = cVars.brakePower / 2;
                backRightWheel.brakeTorque = cVars.brakePower / 2;
            } else if (gear == GearShift.Drive)
            {
                _bLock = false;

                if (Mathf.Abs(fSpeed) < cVars.maxSpeed)
                {
                    backLeftWheel.motorTorque = accelerateValue * motorTorque;
                    backRightWheel.motorTorque = accelerateValue * motorTorque;
                }
                else
                {
                    backLeftWheel.motorTorque = 0f;
                    backRightWheel.motorTorque = 0f;
                }
                backLeftWheel.brakeTorque = 0f;
                backRightWheel.brakeTorque = 0f;

            } else if (gear == GearShift.Reverse)
            {
                _bLock = false;
                if (Mathf.Abs(fSpeed) < cVars.maxSpeed)
                {
                    backLeftWheel.motorTorque = -accelerateValue * motorTorque;
                    backRightWheel.motorTorque = -accelerateValue * motorTorque;
                }
                else
                {
                    backLeftWheel.motorTorque = 0f;
                    backRightWheel.motorTorque = 0f;
                }
                backLeftWheel.brakeTorque = 0f;
                backRightWheel.brakeTorque = 0f;
            } 
        }
    }
}
