using System;
using UnityEngine;

public class SpeedometerVisual : MonoBehaviour
{
    public float value;
    public float rotationZMin;
    public float rotationZMax;
    
    public Transform speedometer;
    private void Update()
    {
        if (speedometer)
        {
            var angles = speedometer.eulerAngles;
            angles.z = Mathf.Lerp(rotationZMin, rotationZMax, value * Time.deltaTime);
            speedometer.eulerAngles = angles;
        }
    }
}
