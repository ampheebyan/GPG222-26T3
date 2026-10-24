using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class CarState : NetworkBehaviour
{
    // This script handles all "responsive" information about the car shown to the player (eg. engine sounds, steering angle, speed text)
    [FormerlySerializedAs("carState")] [SerializeField]
    public PublicCarData car;
    public TMP_Text text;
    public SteeringWheelAlignment steeringWheel;
    public AudioSource engineSound;
    public float maxVolume = 1.0f; // max volume just set here for now. will add menus and move it later
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateCarState_Rpc(PublicCarData data)
    {
        car = data;
        steeringWheel.SetAngle(car.SteeringAngle);
        if (engineSound)
        {
            float vol = Mathf.InverseLerp(0.0f, car.MaxSpeed, car.SpeedKmh);
            engineSound.volume = Mathf.Clamp(vol, 0f, maxVolume);
        }
    }

    private void FixedUpdate()
    {
        if(text) text.SetText($"{car.Gear}\n{car.SpeedKmh}m/s");
    }
}
