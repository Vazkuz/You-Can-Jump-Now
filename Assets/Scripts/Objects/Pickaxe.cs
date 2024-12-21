using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Pickaxe : Grabbable
{
    private Rigidbody2D rb;
    [SerializeField] private Collider2D triggerCollider;
    private NetworkObject networkObject;
    private bool justSpawned = true;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkObject = GetComponent<NetworkObject>();
    }
    // Start is called before the first frame update
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        base.OnNetworkObjectParentChanged(parentNetworkObject);

        if (justSpawned)
        {
            justSpawned = false; 
            return;
        }
        else if (transform.parent == null)
        {
            rb.isKinematic = false;
            triggerCollider.enabled = true;

            // Giving the pickaxe back to the server.
            if (!IsServer) RequestChangeOwnershipRpc(NetworkManager.ServerClientId);
            else ChangePickaxeOwnership(NetworkManager.ServerClientId);
            return;
        }

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;

        rb.isKinematic = true;
        triggerCollider.enabled = false;

        transform.localPosition = parentNetworkObject.GetComponent<PlayerNetwork>().Hand.localPosition;
        if (!IsServer) RequestChangeOwnershipRpc(parentNetworkObject.OwnerClientId);
        else ChangePickaxeOwnership(parentNetworkObject.OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeOwnershipRpc(ulong newClientId)
    {
        ChangePickaxeOwnership(newClientId);
    }

    private void ChangePickaxeOwnership(ulong newClientId)
    {
        if(networkObject.OwnerClientId == newClientId) return;

        networkObject.ChangeOwnership(newClientId);
    }
}
