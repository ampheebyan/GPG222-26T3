using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct NetPlayData
{
    public float Steering;
    public float Accelerator;
    public float Brake;
}
public class NetworkedCarHandler : NetworkBehaviour
{
    public SteeringController car;

    public Dictionary<ulong, NetPlayData> players = new();

    public override void OnNetworkSpawn()
    {
    }
    
    private void Update()
    {
        if (!IsServer) return;
        float steering = 0;
        float accelerator = 0;
        float brake = 0;

        Debug.Log(players.Count);
        foreach (var (id, data) in players)
        {
            Debug.Log($"{id} [STEERING] [DATA]: {data.Steering}");
            Debug.Log($"{id} [ACCELERATOR] [DATA]: {data.Accelerator}");
            Debug.Log($"{id} [BRAKE] [DATA]: {data.Brake}");
            steering = Mathf.Clamp(steering + data.Steering, -1, 1);    
            accelerator = Mathf.Clamp(accelerator + data.Accelerator, -1, 1);    
            brake = Mathf.Clamp(brake + data.Brake, -1, 1);    
        }
        
        UpdateValues_Rpc(accelerator, brake, steering);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateValues_Rpc(float accelerator, float brake, float steering)
    {
        if(!Mathf.Approximately(car.steeringValue, steering)) car.steeringValue = steering;
        if(!Mathf.Approximately(car.accelerateValue, accelerator)) car.accelerateValue = accelerator;
        if(!Mathf.Approximately(car.brakeValue, brake)) car.brakeValue = brake;
    }
}
