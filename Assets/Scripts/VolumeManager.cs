using System;
using Unity.VisualScripting;
using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;
    public float volume = 1;
    private void Awake()
    {
        if(Instance == null) { Instance = this; DontDestroyOnLoad(this); } else { Destroy(this); }
        volume = PlayerPrefs.GetFloat("Volume", 1);
        Debug.Log($"VolumeManager: awake, volume is {volume}");
    }

    public void SetVolume(float volume)
    {
        this.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
