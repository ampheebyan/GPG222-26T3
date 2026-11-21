using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuManager : MonoBehaviour
{
    public GameObject menu;
    public Slider volSlider;
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
    }
}
