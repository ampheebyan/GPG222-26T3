using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerChatManager : NetworkBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text inputFieldPlaceholder;
    public TMP_Text log;
    public ScrollRect logRect;
    public int carIndex;
    public NetworkedCarHandler carController;
    public int maxChar = 320;
    public GameManager instancedGameManager;
    private static readonly Regex RichTextFilterRegex = new(@"<(?!(/?(b|i|u|s)(?![a-zA-Z0-9-])\s*>))[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private void Awake()
    {
        inputField.onSubmit.AddListener(SubmitInput);
        inputField.characterLimit = maxChar;
        if (NicknameHandler.Instance)
        {
            inputFieldPlaceholder.text = $"Chatting as {NicknameHandler.Instance.nickname}..";
        }
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
        string filtered = RichTextFilterRegex.Replace(message, string.Empty);
        if(filtered.Length > maxChar) filtered = filtered.Substring(0, maxChar);
        NetPlayData player = carController.players[id];
        string formatted = $"{player.Nickname}: {filtered}";
        ReceiveChatFromServerRpc(formatted);
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        logRect.verticalNormalizedPosition = 0;
        
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ReceiveChatFromServerRpc(string text)
    {
        log.text += $"{text}\n";
        if (logRect.verticalNormalizedPosition < 0.1f || logRect.content.rect.height <= logRect.viewport.rect.height)
            StartCoroutine(ScrollToBottom());
    }
}
