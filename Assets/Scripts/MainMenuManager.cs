using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public BootstrapManager bootstrapManager;
    public NetworkManager networkManager;
    
    public GameObject hostMenu;
    public GameObject clientMenu;
    
    IEnumerator HandleTransport()
    {
        while (!NetworkManager.Singleton) yield return null;
        if (NetworkManager.Singleton)
        {
            if (NetworkManager.Singleton.gameObject.TryGetComponent(out UnityTransport transport))
            {
                bootstrapManager.enabled = false;
                bootstrapManager.transport = transport;
                bootstrapManager.enabled = true;
                networkManager = NetworkManager.Singleton;
            }
        }
    }

    public void TriggerSceneChange()
    {
        if (networkManager)
        {
            if (networkManager.IsServer)
            {
                networkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            }
        }
    }

    public void StopHosting()
    {
        if (networkManager)
        {
            if (networkManager.IsServer)
            {
                foreach (var (id, client) in networkManager.ConnectedClients)
                    networkManager.DisconnectClient(id, "Server is closing");
                networkManager.Shutdown();
            }
        }
    }
    private void Awake()
    {
        if (bootstrapManager == null) return;
        StartCoroutine(HandleTransport());
    }
    public void Disconnect()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    public void ExitGame()
    {
#if !UNITY_EDITOR
        Application.Quit();
#else
        Debug.Log("This would exit the game, but like.. you're in the Editor. so just. press the stop button at the top?");
#endif
    }
    
    private void Update()
    {
        if (!networkManager) return;

        if (hostMenu) hostMenu.SetActive(networkManager.IsServer);

        if(clientMenu) clientMenu.SetActive(networkManager.IsClient && !networkManager.IsServer);
    }
}
