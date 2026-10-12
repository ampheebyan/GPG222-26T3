using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CarStateUI : NetworkBehaviour
{
    [SerializeField]
    public PublicCarStateData carState;
    public TMP_Text text;
    public SteeringWheelAlignment steeringWheel;
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateCarState_Rpc(PublicCarStateData data)
    {
        carState = data;
        steeringWheel.SetAngle(carState.SteeringAngle);
    }

    private void FixedUpdate()
    {
        if(text) text.SetText($"{carState.Gear}\n{carState.SpeedKmh}m/s");
    }
}
