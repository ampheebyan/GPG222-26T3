using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

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
    public CarModel car;
    [FormerlySerializedAs("CarStateVisual")] public CarStateUI carStateUI;
    private Dictionary<ulong, NetPlayData> players = new();
    
    public override void OnNetworkSpawn()
    {
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            try
            {
                CarStateBroadcast();
            }
            catch (Exception ex)
            {
                // ignore, assume server is shutting down, but log error incase
                Debug.LogException(ex);
            }

            Debug.Log($"Current Player Count: {players.Count}");
        }
    }

    [Rpc(SendTo.Server)]
    public void RemovePlayer_Rpc(ulong id)
    {
        if (!IsSpawned) return;
        try { players.Remove(id); }
        catch (Exception ex)
        {
            // mostly ignore, assume server is shutting down.
            Debug.LogException(ex);
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayer_Rpc(ulong id)
    {
        if (!IsSpawned) return;
        try
        {
            players.Add(id, new NetPlayData());
        }
        catch (Exception ex)
        {
            // mostly ignore, assume server is shutting down.
            Debug.LogException(ex);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void UpdateValues_Rpc(ulong id, NetPlayData data)
    {
        if (!IsSpawned) return;
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
        int pCount = players.Count; // used for division of control, on the off chance that these values are off at all, it will just get set to 1.
        if (pCount == 0) pCount = 1;
        foreach (var (id, data) in players)
        {
            Debug.Log($"{id} [STEERING] [DATA]: {data.Steering} (div: {data.Steering / pCount})");
            Debug.Log($"{id} [ACCELERATOR] [DATA]: {data.Accelerator} (div: {data.Accelerator / pCount})");
            Debug.Log($"{id} [BRAKE] [DATA]: {data.Brake} (div: {data.Brake / pCount})");
            steering = Mathf.Clamp(steering + data.Steering / pCount, -1, 1);    
            accelerator = Mathf.Clamp(accelerator + data.Accelerator / pCount, -1, 1);    
            brake = Mathf.Clamp(brake + data.Brake / pCount, -1, 1);    
        }

        if(!Mathf.Approximately(car.steeringValue, steering)) car.steeringValue = steering;
        if(!Mathf.Approximately(car.accelerateValue, accelerator)) car.accelerateValue = accelerator;
        if(!Mathf.Approximately(car.brakeValue, brake)) car.brakeValue = brake;

        CarStateBroadcast();

    }
    [Rpc(SendTo.Server)]
    public void GearShiftUp_Rpc()
    {
        car.gear = (GearShift)Mathf.Clamp((int)(car.gear + 1), 0, 3);
        CarStateBroadcast();
    }
    
    [Rpc(SendTo.Server)]
    public void GearShiftDown_Rpc()
    {
        car.gear = (GearShift)Mathf.Clamp((int)(car.gear - 1), 0, 3);
        CarStateBroadcast();
    }

    private void CarStateBroadcast()
    {
        if (!IsServer) return; // should ideally never be the case, but as a preventative measure.
        PublicCarStateData carData = new PublicCarStateData()
        {
            SpeedKmh = car.speedMS,
            Gear = car.gear
        };

        try
        {
            carStateUI.UpdateCarState_Rpc(carData);
        }
        catch (Exception ex)
        {
            // ignore, but log error just in case. should only cause an issue if the server is shutting anyway
            Debug.LogException(ex);
        }
    }
}
