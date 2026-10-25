using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Simple Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
/// </summary>
public class SoloBootstrapManager : MonoBehaviour
{
	public UnityTransport transport;
	private ConnectionHandler _connectionHandler;

	IEnumerator FindConnectionHandler()
	{
		yield return null;
		
		while (!ConnectionHandler.Instance) yield return null;
		_connectionHandler = ConnectionHandler.Instance;
		if(_connectionHandler)
			_connectionHandler.maxPlayers = 1;

		while (!NetworkManager.Singleton) yield return null;
		if (NetworkManager.Singleton)
		{
			NetworkManager.Singleton.StartHost();
			while (!NetworkManager.Singleton.IsServer) yield return null;
			NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
		}
	}

	private void Awake()
	{
		StartCoroutine(FindConnectionHandler());
	}

}
