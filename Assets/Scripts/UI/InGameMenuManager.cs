using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuManager : MonoBehaviour
{
    public GameObject menu;
    public Slider volSlider;
    public GameManager instancedGameManager;
    public NetworkedCarHandler carController;
    public int carIndex;
    public TMP_Text playerList;
    private IEnumerator WaitForGameManager()
    {
        while (!GameManager.Instance) yield return null;
        instancedGameManager = GameManager.Instance;
        carController = instancedGameManager.networkedCars[carIndex];
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private VolumeManager _volumeManager;
    private void Awake()
    {
        _volumeManager = VolumeManager.Instance;
        volSlider.value = _volumeManager.volume;
        StartCoroutine(WaitForGameManager());
    }

    public void SetVol()
    {
        _volumeManager.SetVolume(volSlider.value);
    }
    
    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                menu.SetActive(!menu.activeSelf);
            }
        }
        if (menu.activeSelf)
        {
            if (playerList)
            {
                string players = "";
                foreach (NetPlayData data in carController.cachedPlayers)
                {
                    players += $"{data.Nickname}\n";
                }
                playerList.SetText(players);
            }
        }
    }
}
