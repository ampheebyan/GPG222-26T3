using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class StupidSimpleNetworkedTransform : NetworkBehaviour
{
    // I basically just ended up reimplementing the normal NetworkTransform because I wanted half of the functions it provided.
    // guess that's cool aye
    
    public float updateThreshold = 0.001f;
    public float interpolationSpeed = 10f;
    
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;

    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    private void Awake()
    {
        if (IsServer)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            lastScale = transform.localScale;
        }
    }

    private bool VectorApproximately(Vector3 a, Vector3 b, float t)
    {
        return (a-b).sqrMagnitude <= t * t;
    }

    private void FixedUpdate()
    {
        if (!IsSpawned) return;
        if (!IsServer) return;

        if (!VectorApproximately(transform.position, lastPosition, updateThreshold))
        {
            UpdatePosition_Rpc(transform.position);
            lastPosition = transform.position;
        }

        if (!(Quaternion.Angle(transform.rotation, lastRotation) < updateThreshold))
        {
            UpdateRotation_Rpc(transform.rotation);
            lastRotation = transform.rotation;
        }
        
        if (!VectorApproximately(transform.localScale, lastScale, updateThreshold))
        {
            UpdateScale_Rpc(transform.localScale);
            lastScale = transform.localScale;
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (IsServer) return;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, interpolationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);
    }

    // found out NotServer was an option, and realised it was perfect for this
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void UpdatePosition_Rpc(Vector3 position)
    {
        targetPosition = position;
    }
    
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void UpdateRotation_Rpc(Quaternion rotation)
    {
        targetRotation = rotation;
    }
    
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void UpdateScale_Rpc(Vector3 scale)
    {
        targetScale = scale;
    }
}
