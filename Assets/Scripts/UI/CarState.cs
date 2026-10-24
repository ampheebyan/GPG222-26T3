using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CarState : NetworkBehaviour
{
    // This script handles all "responsive" information about the car shown to the player (eg. engine sounds, steering angle, speed text)
    [FormerlySerializedAs("carState")] [SerializeField]
    public PublicCarData car;
    public TMP_Text text;
    public SteeringWheelAlignment steeringWheel;
    public AudioSource engineSound;
    public float maxVolume = 1.0f; // max volume just set here for now. will add menus and move it later
    public Vector2 pitchRange = new Vector2(1.0f, 1.5f);
    public float time = 0f;
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateCarState_Rpc(PublicCarData data)
    {
        car = data;
        steeringWheel.SetAngle(car.SteeringAngle);
        if (engineSound)
        {
            float vol = Mathf.InverseLerp(0.0f, car.MaxSpeed, car.SpeedKmh);
            engineSound.volume = Mathf.Clamp(vol, 0f, maxVolume);
            time = Mathf.MoveTowards(time, car.Accelerating ? 1f: 0f, Time.deltaTime);
            engineSound.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, time) + Random.Range(0.01f, 0.075f);
        }
    }

    private void FixedUpdate()
    {
        if(text) text.SetText($"{car.Gear}\n{car.SpeedKmh}m/s");
    }
}
