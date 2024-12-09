using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Pickaxe : NetworkBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private List<Collider2D> colliders;
    private NetworkObject networkObject;
    private bool justSpawned = true;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkObject = GetComponent<NetworkObject>();
        colliders = GetComponents<Collider2D>().ToList();
        print("El pico tiene " + colliders.Count + " colliders.");
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
            foreach (Collider2D collider in colliders)
            {
                collider.enabled = true;
            }

            // Giving the pickaxe back to the server.
            if (!IsServer) RequestChangeOwnershipRpc(NetworkManager.ServerClientId);
            else ChangePickaxeOwnership(NetworkManager.ServerClientId);
            return;
        }

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;

        rb.isKinematic = true;
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        transform.localPosition = new Vector3(0, 0 , 0);
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
        networkObject.ChangeOwnership(newClientId);
    }
}
