using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public List<NetworkedCarHandler> networkedCars = new ();
    public Action OnDisconnect;
    private void SceneManagerOnOnSceneEvent(SceneEvent @event)
    {
        Debug.Log("SceneManager OnSceneEvent");
        if (@event.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            foreach (NetworkedCarHandler networkedCar in networkedCars) networkedCar.stopped = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            Debug.Log("NOT SERVER");
            return;
        }
        Debug.Log("IS SERVER");
        
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManagerOnOnSceneEvent;
        }
        else
        {
            Debug.Log("No Network Manager?");
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(this);
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientStopped += b =>
            {
                if(b) Debug.Log("Host client was disconnected? Assuming server stopped.");
                Debug.Log("Got disconnected, returning to main menu.");
                OnDisconnect?.Invoke();
                SceneManager.LoadScene("Menu");
            };
        }
    }


}
