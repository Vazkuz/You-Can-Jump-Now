using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    protected Rigidbody2D rb;
    [SerializeField] protected Collider2D triggerCollider;
    private NetworkObject networkObject;
    private bool justSpawned = true;
    public SpriteRenderer body;
    // Start is called before the first frame update
    protected virtual void Start()
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

            // Giving the grabbable back to the server.
            if (!IsServer) RequestChangeOwnershipRpc(NetworkManager.ServerClientId);
            else ChangeGrabbableOwnership(NetworkManager.ServerClientId);
            return;
        }

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;

        rb.isKinematic = true;
        triggerCollider.enabled = false;

        transform.localPosition = parentNetworkObject.GetComponent<PlayerNetwork>().Hand.localPosition;
        if (!IsServer) RequestChangeOwnershipRpc(parentNetworkObject.OwnerClientId);
        else ChangeGrabbableOwnership(parentNetworkObject.OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeOwnershipRpc(ulong newClientId)
    {
        ChangeGrabbableOwnership(newClientId);
    }

    private void ChangeGrabbableOwnership(ulong newClientId)
    {
        if (networkObject.OwnerClientId == newClientId) return;

        networkObject.ChangeOwnership(newClientId);
    }
}
