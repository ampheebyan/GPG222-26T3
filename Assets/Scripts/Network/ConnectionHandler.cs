using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ConnectionHandler : MonoBehaviour
{
    public static ConnectionHandler Instance;
    [SerializeField] private NetworkManager networkManager;
    
    public int maxPlayers = 4;
    public string disconnectReason;
    private void Awake()
    {
        if(Instance == null) Instance = this; else Destroy(this);
        if (networkManager)
        {
            networkManager.ConnectionApprovalCallback += ConnectionApprovalCallback;
            networkManager.OnConnectionEvent += NetworkManagerOnOnConnectionEvent;
        }
    }

    private void NetworkManagerOnOnConnectionEvent(NetworkManager netMan, ConnectionEventData eventData)
    {
        switch (eventData.EventType)
        {
            case ConnectionEvent.ClientDisconnected:
                disconnectReason = netMan.DisconnectReason;
                Debug.Log($"Client was disconnected with reason: {netMan.DisconnectReason}");
                break;
        }
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
    {
        if (networkManager.ConnectedClientsIds.Count >= maxPlayers)
        {
            res.Approved = false;
            res.Reason = $"Server is at max capacity ({maxPlayers})";
        }
        else
        {
            res.Approved = true;
            res.CreatePlayerObject = true;
        }
    }
}
