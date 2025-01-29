using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    protected Rigidbody2D rb;
    [SerializeField] protected Collider2D triggerCollider;
    private NetworkObject networkObject;
    private bool justSpawned = true;
    public SpriteRenderer body;
    [SerializeField] private GrabbedObject grabbedObject;
    bool isParented = false;
    CancellationTokenSource tokenSource;

    public NetworkVariable<Vector3> lastSavedPos;

    protected void OnValidate()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        // Ensure we have a SpriteRenderer and update it in the editor
        if (body == null)
        {
            body = GetComponent<SpriteRenderer>();
        }

        if (body != null && grabbedObject != null)
        {
            body.sprite = grabbedObject.objectImage;
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        PlayerNetwork.OnShowLocalGrabbable += OnHideNetworkGrabbable;
        PlayerNetwork.OnHideLocalGrabbable += OnShowNetworkGrabbable;
        rb = GetComponent<Rigidbody2D>();
        networkObject = GetComponent<NetworkObject>();
        tokenSource = new CancellationTokenSource();
    }

    private void OnHideNetworkGrabbable(ulong newOwnerId, string name)
    {
        if (this.name != name) return;
        HandleGrabbablePhysicsRpc(true);

        if (!IsServer) RequestChangeOwnershipRpc(newOwnerId);
        else ChangeGrabbableOwnership(newOwnerId);
    }

    private void OnShowNetworkGrabbable(string name)
    {
        if (this.name != name) return;

        HandleGrabbablePhysicsRpc(false);

        // Giving the grabbable back to the server.
        if (!IsServer) RequestChangeOwnershipRpc(NetworkManager.ServerClientId);
        else ChangeGrabbableOwnership(NetworkManager.ServerClientId);

        body.enabled = true;
    }

    [Rpc(SendTo.Everyone)]
    private void HandleGrabbablePhysicsRpc(bool isHidden)
    {
        rb.isKinematic = isHidden;
        triggerCollider.enabled = !isHidden;
        isParented = isHidden;
    }

    //This section will occur only when the host grabs a grabbable
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (justSpawned)
        {
            justSpawned = false;
            return;
        }
        else if (transform.parent == null) //This section occurs when a player releases the grabbable
        {
            OnShowNetworkGrabbable(name);
            return;
        }

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;

        HandleGrabbableGrabbed(parentNetworkObject);
    }

    private void HandleGrabbableGrabbed(NetworkObject parentNetworkObject)
    {
        rb.isKinematic = true;
        triggerCollider.enabled = false;
        isParented = true;
        if (!IsServer) RequestChangeOwnershipRpc(parentNetworkObject.OwnerClientId);
        else ChangeGrabbableOwnership(parentNetworkObject.OwnerClientId);

        transform.localPosition = transform.parent.GetComponent<PlayerNetwork>().Hand.localPosition;
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

    protected async Task HandleAsyncOperation()
    {
        var result = await Task.Run(() => 
        {
            while(!isParented)
            {
                if (tokenSource.IsCancellationRequested) return 0;
            }
            return 0;
        }, tokenSource.Token);

        if (tokenSource.IsCancellationRequested) return;
    }

    protected void OnDisable()
    {
        tokenSource.Cancel();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerNetwork.OnShowLocalGrabbable -= OnHideNetworkGrabbable;
        PlayerNetwork.OnHideLocalGrabbable -= OnShowNetworkGrabbable;
    }

    public void Death()
    {
        if (!IsOwner) return;
        //AÑADIR: PARTÍCULAS DE MUERTE O ALGO, Y LUEGO PARTÍCULAS PARA REGRESAR O ALGO ASÍ
        transform.position = lastSavedPos.Value;
    }
}
