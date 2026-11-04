using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public BootstrapManager bootstrapManager;
    public NetworkManager networkManager;
    
    public GameObject hostMenu;
    public TMP_Text hostText;
    public TMP_Text statusText;
    public GameObject clientMenu;
    public TMP_InputField serverJoinCode;
    public Slider maxPlayers;
    public GameObject connectionMenu;
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
        maxPlayers.value = bootstrapManager.connectionHandler.maxPlayers;
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

    public void HostRelayGame()
    {
        if (maxPlayers) bootstrapManager.connectionHandler.maxPlayers = (int)maxPlayers.value;
        if (bootstrapManager) bootstrapManager.StartHostingRelayGame();
    }

    public void ConnectToRelayGame()
    {
        if (serverJoinCode)
        {
            if (bootstrapManager && !string.IsNullOrEmpty(serverJoinCode.text))
                bootstrapManager.StartClientWithRelay(serverJoinCode.text, "udp");
        }
    }

    public string CleanupDisconnectText(string text)
    {
        string returnText = text;
        if (text.ToLower().Contains("the transport was shutdown"))
        {
            returnText = "Server is closing";
        }
        return returnText;
    }
    
    private void Update()
    {
        if (!networkManager) return;
        
        if (hostMenu) hostMenu.SetActive(networkManager.IsServer);
        if (hostText)
        {
            hostText.text = $"You are hosting the server\n{bootstrapManager.joinCode}";
        }
        if (clientMenu) clientMenu.SetActive(networkManager.IsClient && !networkManager.IsServer);
        
        if (connectionMenu) connectionMenu.SetActive(!(networkManager.IsClient || networkManager.IsServer));
        if (statusText && !string.IsNullOrEmpty(bootstrapManager.connectionHandler.disconnectReason)) statusText.text = CleanupDisconnectText(bootstrapManager.connectionHandler.disconnectReason);
    }
}
