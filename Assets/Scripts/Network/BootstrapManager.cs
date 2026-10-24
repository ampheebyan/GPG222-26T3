using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

/// <summary>
/// Simple Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
	[SerializeField]
	private float buttonWidth = 70f;
	[SerializeField]
	private float buttonHeight = 40f;

	public UnityTransport transport;
	private ConnectionHandler _connectionHandler;
	string _ipAddress = "127.0.0.1";
	string _maxPlayers = "4";

	IEnumerator FindConnectionHandler()
	{
		while (!ConnectionHandler.Instance) yield return null;
		_connectionHandler = ConnectionHandler.Instance;
	}

	private void Awake()
	{
		StartCoroutine(FindConnectionHandler());
	}

	private void OnGUI()
	{
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
				if(_connectionHandler)
					_connectionHandler.maxPlayers = players;
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
			
			if (_connectionHandler)
			{
				if(!string.IsNullOrEmpty(_connectionHandler.disconnectReason))
				{
					GUILayout.Label($"Client disconnected: {_connectionHandler.disconnectReason}.");
					if (GUILayout.Button("Dismiss", myButtonStyle))
					{
						_connectionHandler.disconnectReason = "";
					}
				}
			}

		}

		GUILayout.EndArea();
	}


	private void NewIPAddress(string ipAddress)
	{
		Debug.Log("New IP Address: " + ipAddress);
		if (transport != null) transport.SetConnectionData(ipAddress, 7777);
	}
}
