using System;
using UnityEngine;

public class SpeedometerVisual : MonoBehaviour
{
    public float rotationZMin;
    public float rotationZMax;
    public CarState state;
    public Transform speedometer;
    private void Update()
    {
        if (speedometer && state)
        {
            float value = state.car.SpeedMS * 3.6f;
            float inverse = Mathf.InverseLerp(0, 120, value);
            var angles = speedometer.eulerAngles;
            angles.z = Mathf.Lerp(rotationZMin, rotationZMax, inverse);
            speedometer.eulerAngles = angles;
        }
    }
}
