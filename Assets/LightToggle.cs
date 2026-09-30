using Unity.Netcode;
using UnityEngine;

public class LightToggle : NetworkBehaviour
{
    [SerializeField] private Light light;
    
    [Rpc(SendTo.ClientsAndHost)]
    public void ToggleRpc()
    {
        light.enabled = !light.enabled;
    }
}
