using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerChatManager : NetworkBehaviour
{
    public TMP_InputField inputField;
    public TMP_InputField log;
    public int carIndex;
    public NetworkedCarHandler carController;
    public GameManager instancedGameManager;

    private void Awake()
    {
        inputField.onSubmit.AddListener(SubmitInput);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(WaitForGameManager());
        }
    }
    private IEnumerator WaitForGameManager()
    {
        while (!GameManager.Instance) yield return null;
        instancedGameManager = GameManager.Instance;
        carController = instancedGameManager.networkedCars[carIndex];
    }


    private void SubmitInput(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        ulong id = OwnerClientId;
        SendChatToServerRpc(text, id);
        inputField.text = "";
    }
    
    [Rpc(SendTo.Server)]
    private void SendChatToServerRpc(string message, ulong id)
    {
        Debug.Log($"{id} sent chat message: {message}");
        NetPlayData player = carController.players[id];
        string formatted = $"{player.Nickname}: {message}";
        ReceiveChatFromServerRpc(formatted);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReceiveChatFromServerRpc(string text)
    {
        log.text += $"{text}\n";
    }
}
