using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct NetPlayData : INetworkSerializable
{
    public float Steering;
    public float Accelerator;
    public float Brake;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Steering);
        serializer.SerializeValue(ref Accelerator);
        serializer.SerializeValue(ref Brake);
    }
}

[Serializable]
public struct PublicCarStateData : INetworkSerializable
{
    public GearShift Gear;
    public float SpeedKmh;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Gear);
        serializer.SerializeValue(ref SpeedKmh);
    }
}
public class NetworkedCarHandler : NetworkBehaviour
{
    public SteeringController car;
    public CarStateVisual CarStateVisual;
    private Dictionary<ulong, NetPlayData> players = new();
    
    public override void OnNetworkSpawn()
    {
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            CarStateBroadcast_Rpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void UpdateValues_Rpc(ulong id, NetPlayData data)
    {
        if (players.ContainsKey(id))
        {
            players[id] = data;
        }
        else
        {
            players.TryAdd(id, data);
        }
        
        UpdateCarValues_Rpc();
    }

    [Rpc(SendTo.Server)]
    private void UpdateCarValues_Rpc()
    {
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

        if(!Mathf.Approximately(car.steeringValue, steering)) car.steeringValue = steering;
        if(!Mathf.Approximately(car.accelerateValue, accelerator)) car.accelerateValue = accelerator;
        if(!Mathf.Approximately(car.brakeValue, brake)) car.brakeValue = brake;

        CarStateBroadcast_Rpc();

    }
    [Rpc(SendTo.Server)]
    public void GearShiftUp_Rpc()
    {
        car.gear = (GearShift)Mathf.Clamp((int)(car.gear + 1), 0, 3);
        CarStateBroadcast_Rpc();
    }
    
    [Rpc(SendTo.Server)]
    public void GearShiftDown_Rpc()
    {
        car.gear = (GearShift)Mathf.Clamp((int)(car.gear - 1), 0, 3);
        CarStateBroadcast_Rpc();
    }

    [Rpc(SendTo.Server)]
    private void CarStateBroadcast_Rpc()
    {
        PublicCarStateData carData = new PublicCarStateData()
        {
            SpeedKmh = car.speedMS,
            Gear = car.gear
        };
     
        CarStateVisual.UpdateCarState_Rpc(carData);
    }
}
