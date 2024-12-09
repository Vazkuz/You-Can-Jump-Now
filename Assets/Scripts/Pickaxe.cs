using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pickaxe : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Collider2D _collider2D;
    private NetworkObject networkObject;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        networkObject = GetComponent<NetworkObject>();
    }
    // Start is called before the first frame update
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        base.OnNetworkObjectParentChanged(parentNetworkObject);

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;
        
        rb.isKinematic = true;
        _collider2D.enabled = false;
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
