using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
public struct PublicCarData : INetworkSerializable
{
    public GearShift Gear;
    public float SpeedKmh;
    public float MaxSpeed;
    public bool Accelerating;
    public float SteeringAngle;
    public Vector2 CarHealth;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Gear);
        serializer.SerializeValue(ref SpeedKmh);
        serializer.SerializeValue(ref MaxSpeed);
        serializer.SerializeValue(ref Accelerating);
        serializer.SerializeValue(ref SteeringAngle);
        serializer.SerializeValue(ref CarHealth);
    }
}
public class NetworkedCarHandler : NetworkBehaviour
{
    public bool stopped = true;
    public CarModel car;
    [FormerlySerializedAs("carStateUI")] [FormerlySerializedAs("CarStateVisual")] public CarState carState;
    private Dictionary<ulong, NetPlayData> players = new();
    private bool _isAccelerating = false;
    private Vector2 _carHealth = new ();
    public Vector3 originPosition;
    public Quaternion originRotation;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _carHealth = new(car.cVars.carHealth, car.cVars.carHealth);
            originPosition = transform.position;
            originRotation = transform.rotation;
        }
    }

    private void CarReset()
    {
        if (!IsServer) return; // This should never be executed by the players.
        stopped = true;
        _carHealth = new(car.cVars.carHealth, car.cVars.carHealth);
        transform.position = originPosition;
        transform.rotation = originRotation;
        stopped = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsServer)
        {
            if (other.gameObject.TryGetComponent(out ObstacleTag tag))
            {
                if (car.speedMS >= tag.speedRequirement)
                {
                    _carHealth.x -= Random.Range(tag.damageRange.x, tag.damageRange.y);
                }
            } else _carHealth.x -= 1;
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if(_carHealth.x <= 0) CarReset();
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
        if (stopped) return;
        
        float steering = 0;
        float accelerator = 0;
        float brake = 0;
        int pCount = players.Count; // used for division of control, on the off chance that these values are off at all, it will just get set to 1.
        if (pCount == 0) pCount = 1;
        /*  WHY DIVIDE YOU ASK?
         *  previously, everyone had equal control over everything. this was fine, because it meant that each person could just willingly choose to ignore some parts of the car. i don't want that.
         *  now, each person has n% control over the car, n being 1 / pCount. 4 players means 25% control per person, requiring everyone to cooperate properly.
         *  my testing with 2 people (me, and one other person) was infinitely more fun with 50% control per input, simply because to turn properly, we had to actually do it together. */
        int playersSteering = 0;
        int playersAccelerating = 0;
        int playersBraking = 0;
        foreach (var (id, data) in players)
        {
            if(data.Steering != 0) playersSteering += 1;
            if(data.Accelerator != 0) playersAccelerating += 1;
            if(data.Brake != 0) playersBraking += 1;
            
            Debug.Log($"steering: {playersSteering} / accel: {playersAccelerating} / braking: {playersBraking}");
            Debug.Log($"{id} [STEERING] [DATA]: {data.Steering}");
            Debug.Log($"{id} [ACCELERATOR] [DATA]: {data.Accelerator}");
            Debug.Log($"{id} [BRAKE] [DATA]: {data.Brake}");

            if(playersSteering > 0) steering = Mathf.Clamp(steering + data.Steering / playersSteering, -1, 1);    
            if(playersAccelerating > 0) accelerator = Mathf.Clamp(accelerator + data.Accelerator / playersAccelerating, -1, 1);    
            if(playersBraking > 0) brake = Mathf.Clamp(brake + data.Brake / playersBraking, -1, 1);    
        }

        _isAccelerating = playersAccelerating > 0;
        if(!Mathf.Approximately(car.steeringValue, steering)) car.steeringValue = steering;
        if(!Mathf.Approximately(car.accelerateValue, accelerator)) car.accelerateValue = accelerator;
        if(!Mathf.Approximately(car.brakeValue, brake)) car.brakeValue = brake;

        CarStateBroadcast();

    }
    [Rpc(SendTo.Server)]
    public void GearShiftUp_Rpc()
    {
        if (stopped) return;

        car.gear = (GearShift)Mathf.Clamp((int)(car.gear + 1), 0, 3);
        CarStateBroadcast();
        carState.GearShiftSound_Rpc();
    }
    
    [Rpc(SendTo.Server)]
    public void GearShiftDown_Rpc()
    {
        if (stopped) return;

        car.gear = (GearShift)Mathf.Clamp((int)(car.gear - 1), 0, 3);
        CarStateBroadcast();
        carState.GearShiftSound_Rpc();
    }

    private void CarStateBroadcast()
    {
        if (!IsServer) return; // should ideally never be the case, but as a preventative measure.
        
        PublicCarData carData = new PublicCarData()
        {
            SpeedKmh = car.speedMS,
            Gear = car.gear,
            SteeringAngle = car.steeringValue,
            MaxSpeed = car.cVars.maxSpeed,
            Accelerating = _isAccelerating,
            CarHealth = _carHealth
        };

        try
        {
            carState.UpdateCarState_Rpc(carData); // send out new updated data to everyone
        }
        catch (Exception ex)
        {
            // ignore, but log error just in case. should only cause an issue if the server is shutting anyway
            Debug.LogException(ex);
        }
    }
}
