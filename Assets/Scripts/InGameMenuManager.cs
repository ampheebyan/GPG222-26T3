using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public void Disconnect()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
