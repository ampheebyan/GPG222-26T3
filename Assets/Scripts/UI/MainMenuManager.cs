using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[Serializable]
public struct MapChoice {
    public string internalMapName;
    public string mapName;
    public string mapDescription;
}

public class MainMenuManager : MonoBehaviour
{
    public BootstrapManager bootstrapManager;
    public NetworkManager networkManager;
    
    public GameObject hostMenu;
    public TMP_Text hostText;
    public TMP_Text hostButtonText;
    public TMP_Text statusText;
    public GameObject clientMenu;
    public TMP_InputField serverJoinCode;
    public Slider maxPlayers;
    public GameObject connectionMenu;
    private bool _waiting;

    public int mapChoiceIndex;
    public List<MapChoice> mapChoices = new ();
    public Slider mapChoiceSlider;
    public TMP_Text mapChoiceText;

    public TMP_InputField nicknameField;
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
                if (mapChoices.Count < 1)
                {
                    throw new Exception("No possible map choices! That's not good.");
                }
                
                string sceneName = mapChoices[mapChoiceIndex].internalMapName;
                networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
        mapChoiceSlider.value = mapChoiceIndex;
        if (NicknameManager.Instance)
        {
            nicknameField.text = NicknameManager.Instance.nickname;
            nicknameField.onSubmit.AddListener(NicknameFieldChange);
        }
        else
        {
            nicknameField.readOnly = true;
            nicknameField.text = "N/A";
        }
    }

    private void NicknameFieldChange(string text)
    {
        NicknameManager.Instance.SetNick(text);
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
        _waiting = true;
        if (maxPlayers) bootstrapManager.connectionHandler.maxPlayers = (int)maxPlayers.value;
        if (bootstrapManager) bootstrapManager.StartHostingRelayGame();
    }

    public void ConnectToRelayGame()
    {
        if (serverJoinCode)
        {
            _waiting = true;
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
        
        if (networkManager.IsServer || networkManager.IsClient) _waiting = false;
        
        if (hostMenu) hostMenu.SetActive(networkManager.IsServer);
        if (clientMenu) clientMenu.SetActive(networkManager.IsClient && !networkManager.IsServer);
        
        if (networkManager.IsServer)
        {
            mapChoiceSlider.maxValue = mapChoices.Count - 1;
            mapChoiceIndex = (int)mapChoiceSlider.value;
            
            MapChoice choice = mapChoices[mapChoiceIndex];
            mapChoiceText.text = $"{choice.mapName}\n<size=75%>{choice.mapDescription}</size>"; 
        }
        
        if (hostText)
        {
            hostText.text = $"You are hosting the server\n{bootstrapManager.joinCode}";
        }
        if (hostButtonText) hostButtonText.text = $"Host ({maxPlayers.value})";
        
        if (connectionMenu) connectionMenu.SetActive(!(networkManager.IsClient || networkManager.IsServer || _waiting));
        if (statusText && !string.IsNullOrEmpty(bootstrapManager.connectionHandler.disconnectReason)) statusText.text = CleanupDisconnectText(bootstrapManager.connectionHandler.disconnectReason);
    }
}
