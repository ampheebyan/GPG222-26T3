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
                // Disable non local PlayerInput so they don't interfere. (I think) This is not the best way to handle this, but this does what I want.
                playerInput.enabled = false;
            }
        }
    }
    
    public void OnSteering(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Steering = temp;
        if(CarController) CarController.UpdateValues_Rpc(OwnerClientId, localData);
    }

    public void OnBrake(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Brake = temp;
        if(CarController) CarController.UpdateValues_Rpc(OwnerClientId, localData);

    }

    public void OnAccelerate(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Accelerator = temp;
        if(CarController) CarController.UpdateValues_Rpc(OwnerClientId, localData);

    }
    
    public void OnGearShiftUp()
    {
        if(CarController) CarController.GearShiftUp_Rpc();
    }

    public void OnGearShiftDown()
    {
        if(CarController) CarController.GearShiftDown_Rpc();
    }
}
