using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.Serialization;

public class BootstrapManager : MonoBehaviour
{
	[SerializeField]
	private float buttonWidth = 70f;
	[SerializeField]
	private float buttonHeight = 40f;

	public UnityTransport transport;
	public string joinCode;
	[FormerlySerializedAs("_connectionHandler")] public ConnectionHandler connectionHandler;
	string _ipAddress = "127.0.0.1";
	string _maxPlayers = "4";
	private bool _signedIn = false;
	IEnumerator FindConnectionHandler()
	{
		while (!ConnectionHandler.Instance) yield return null;
		connectionHandler = ConnectionHandler.Instance;
	}

	private void Awake()
	{
		StartCoroutine(FindConnectionHandler());
	}

	private void OnGUI()
	{
		/*
		GUILayout.BeginArea(new Rect(10, 10, 200, 400));
		GUILayout.Label("Roadtrippers Prototype");
		GUILayout.Label("GPG222 Phoebe (1033478)");

		NetworkManager networkManager = NetworkManager.Singleton;
		if (networkManager && !networkManager.IsClient && !networkManager.IsServer)
		{
			GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 16,
				fixedWidth = buttonWidth * 1.5f,
				fixedHeight = buttonHeight / 2f
			};
			GUILayout.Label("Max Players");
			string maxPlayers = GUILayout.TextField(_maxPlayers, 2);
			_maxPlayers = Regex.Replace(maxPlayers, @"[^0-9]", "");

			if (int.TryParse(_maxPlayers, out int players))
			{
				if(connectionHandler)
					connectionHandler.maxPlayers = players;
			}

			if (GUILayout.Button("Host", myButtonStyle))
			{
				networkManager.StartHost();
			}

			GUILayout.Space(20);

			GUILayout.Label("Server IP");
			_ipAddress = GUILayout.TextField(_ipAddress, 32);
			if (GUILayout.Button("Connect", myButtonStyle))
			{
				NewIPAddress(_ipAddress);
				networkManager.StartClient();
			}

			if (GUILayout.Button("Start Server through Relay", myButtonStyle))
			{
				StartHostWithRelay(connectionHandler.maxPlayers, "udp");
			}
			
			GUILayout.Label("Relay Code");
			joinCode = GUILayout.TextField(joinCode, 32);
			if (GUILayout.Button("Connect as Client", myButtonStyle))
			{
				StartClientWithRelay(joinCode, "udp");
			}

			
			if (connectionHandler)
			{
				if(!string.IsNullOrEmpty(connectionHandler.disconnectReason))
				{
					GUILayout.Label($"Client disconnected: {connectionHandler.disconnectReason}.");
					if (GUILayout.Button("Dismiss", myButtonStyle))
					{
						connectionHandler.disconnectReason = "";
					}
				}
			}

		}

		GUILayout.EndArea();
		*/
	}

	private async Task InitServices()
	{
		if (_signedIn) return;
		await UnityServices.InitializeAsync();
		if (!AuthenticationService.Instance.IsSignedIn)
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			_signedIn = true;
		}
	}
	public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
	{
		await InitServices();

		var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		return NetworkManager.Singleton.StartHost() ? joinCode : null;
	}

	public void StartHostingRelayGame()
	{
		StartHostWithRelay(connectionHandler.maxPlayers, "udp");
	}
	public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
	{
		await InitServices();

		var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
	}

	private void NewIPAddress(string ipAddress)
	{
		Debug.Log("New IP Address: " + ipAddress);
		if (transport != null) transport.SetConnectionData(ipAddress, 7777);
	}
}
