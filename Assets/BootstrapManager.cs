﻿using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;


namespace Unity.Netcode.Samples
{
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

		string ipAddress = "127.0.0.1";


		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10, 10, 200, 400));


			NetworkManager networkManager = NetworkManager.Singleton;
			if (!networkManager.IsClient && !networkManager.IsServer)
			{
				GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
				myButtonStyle.fontSize    = 12;
				myButtonStyle.fixedWidth  = buttonWidth * 1.5f;
				myButtonStyle.fixedHeight = buttonHeight / 2f;


				if (GUILayout.Button("Host", myButtonStyle))
				{
					networkManager.StartHost();
				}

				GUILayout.Space(20);


				GUILayout.Label("Manual IP");
				ipAddress = GUILayout.TextField(ipAddress, 15);
				if (GUILayout.Button("Client", myButtonStyle))
				{
					NewIPAddress(ipAddress);
					networkManager.StartClient();
				}


				GUILayout.Space(20);


				if (GUILayout.Button("Server", myButtonStyle))
				{
					networkManager.StartServer();
				}
			}

			GUILayout.EndArea();
		}


		public void NewIPAddress(string _ipAddress)
		{
			Debug.Log("New IP Address: " + _ipAddress);
			if (transport != null) transport.SetConnectionData(_ipAddress, 7777);
		}


		public void Ping()
		{
			Ping      ping      = new Ping();
			PingReply pingReply = ping.Send("134.14.134.1"); // TODO put in the relay server actual IP
			Debug.Log("Ping time = " + pingReply.RoundtripTime);
			Debug.Log("                - Status = " + pingReply.Status);

			switch (pingReply.Status)
			{
				case IPStatus.BadDestination:
					break;
				case IPStatus.BadHeader:
					break;
				case IPStatus.BadOption:
					break;
				case IPStatus.BadRoute:
					break;
				case IPStatus.DestinationHostUnreachable:
					break;
				case IPStatus.DestinationNetworkUnreachable:
					break;
				case IPStatus.DestinationPortUnreachable:
					break;
				case IPStatus.DestinationProhibited:
					break;
				case IPStatus.DestinationScopeMismatch:
					break;
				case IPStatus.DestinationUnreachable:
					break;
				case IPStatus.HardwareError:
					break;
				case IPStatus.IcmpError:
					break;
				case IPStatus.NoResources:
					break;
				case IPStatus.PacketTooBig:
					break;
				case IPStatus.ParameterProblem:
					break;
				case IPStatus.SourceQuench:
					break;
				case IPStatus.Success:
					break;
				case IPStatus.TimedOut:
					break;
				case IPStatus.TimeExceeded:
					break;
				case IPStatus.TtlExpired:
					break;
				case IPStatus.TtlReassemblyTimeExceeded:
					break;
				case IPStatus.Unknown:
					break;
				case IPStatus.UnrecognizedNextHeader:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}