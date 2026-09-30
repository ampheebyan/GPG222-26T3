using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetPlayer : NetworkBehaviour
{
    public void Update()
    {
        if (Keyboard.current == null) return;
        if(IsLocalPlayer)
        {
            if(Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                NetEvent_RTS_Rpc();
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void NetEvent_RTS_Rpc()
    {
        Color color = Random.ColorHSV();
        NetEvent_Rpc(color);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Owner)]
    private void NetEvent_Rpc(Color color)
    {
        if (transform.TryGetComponent(out MeshRenderer renderer))
        {
            renderer.material.color = color;
        }
    }
}
