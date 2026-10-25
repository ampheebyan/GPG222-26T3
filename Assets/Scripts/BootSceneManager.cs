using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    // This exists just to load the main menu scene.
    public string sceneToLoad = "";
    
    private void Start()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
