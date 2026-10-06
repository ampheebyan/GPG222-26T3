using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CarStateVisual : NetworkBehaviour
{
    [SerializeField]
    public PublicCarStateData carState;
    public TMP_Text text;
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateCarState_Rpc(PublicCarStateData data)
    {
        carState = data;
    }

    private void FixedUpdate()
    {
        if(text) text.SetText($"{carState.Gear}\n{carState.SpeedKmh}km/h");
    }
}
