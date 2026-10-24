using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class NetworkedPlayer : NetworkBehaviour
{
    [FormerlySerializedAs("CarController")] public NetworkedCarHandler carController;
    public GameManager instancedGameManager;
    public int carIndex = 0;
    public NetPlayData localData = new NetPlayData();
    public bool disconnected = false;
    private Coroutine _waitForGm;
    private void Awake()
    {
        _waitForGm = StartCoroutine(WaitForGameManager());
    }

    public override void OnDestroy()
    {
        if(_waitForGm != null) StopCoroutine(_waitForGm); // When destroying, if still looking for GameManager, stop that.
        if (IsServer || disconnected) return;
        if(carController) carController.RemovePlayer_Rpc(OwnerClientId);
    }

    protected override void OnNetworkPostSpawn()
    {
        if (!IsLocalPlayer) return;
        
        if (carController)
        {
            StartCoroutine(CallNextFrame()); // call AddPlayer_Rpc next frame, just to make sure things are in place
        }
    }

    private IEnumerator CallNextFrame()
    {
        yield return null;

        if (IsSpawned && carController)
        {
            carController.AddPlayer_Rpc(OwnerClientId);
        }
    }

    private IEnumerator WaitForGameManager()
    {
        while (!GameManager.Instance) yield return null;
        instancedGameManager = GameManager.Instance;
        carController = instancedGameManager.networkedCars[carIndex];
        instancedGameManager.OnDisconnect += OnDisconnect;
    }

    private void OnDisconnect()
    {
        disconnected = true;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"{OwnerClientId}: Spawned NetworkedPlayer");

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
        if(carController) carController.UpdateValues_Rpc(OwnerClientId, localData);
    }

    public void OnBrake(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Brake = temp;
        if(carController) carController.UpdateValues_Rpc(OwnerClientId, localData);

    }

    public void OnAccelerate(InputValue value)
    {
        float temp = value.Get<float>();
        localData.Accelerator = temp;
        if(carController) carController.UpdateValues_Rpc(OwnerClientId, localData);

    }
    
    public void OnGearShiftUp()
    {
        if(carController) carController.GearShiftUp_Rpc();
    }

    public void OnGearShiftDown()
    {
        if(carController) carController.GearShiftDown_Rpc();
    }
}
