using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkedPlayer : NetworkBehaviour
{
    public NetworkedCarHandler CarController;
    public NetPlayData localData = new NetPlayData();
    public override void OnNetworkSpawn()
    {
        Debug.Log($"{OwnerClientId}: Spawned NetworkedPlayer");

        CarController = GameObject.FindGameObjectWithTag("Car").GetComponent<NetworkedCarHandler>();
        if (!IsLocalPlayer)
        {
            if (TryGetComponent(out PlayerInput playerInput))
            {
                // Disable everyone else's PlayerInput so they don't interfere. This is not the best way to handle this, but this does what I want.
                playerInput.enabled = false;
            }
        }
    }
    
    public void OnSteering(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Steering = temp;
        UpdateValues_Rpc();

    }

    public void OnBrake(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Brake = temp;
        UpdateValues_Rpc();

    }

    public void OnAccelerate(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Accelerator = temp;
        UpdateValues_Rpc();

    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateValues_Rpc()
    {
        Debug.Log($"{localData.Steering}, {localData.Brake}, {localData.Accelerator}");
        if (CarController)
        {
            if (CarController.players.ContainsKey(OwnerClientId))
            {
                CarController.players[OwnerClientId] = localData;
            }
            else
            {
                CarController.players.TryAdd(OwnerClientId, localData);
            }
        }
    }
    
    private void Update()
    {
    }

    public void OnGearShiftUp()
    {
        if(CarController) CarController.car.GearShiftUp_Rpc();
    }

    public void OnGearShiftDown()
    {
        if(CarController) CarController.car.GearShiftDown_Rpc();
    }
}
