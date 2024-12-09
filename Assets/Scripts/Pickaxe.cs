using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pickaxe : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Collider2D collider2D;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
    }
    // Start is called before the first frame update
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        base.OnNetworkObjectParentChanged(parentNetworkObject);

        if (parentNetworkObject == null) return;
        if (parentNetworkObject.GetComponent<PlayerNetwork>() == null) return;
        
        rb.isKinematic = true;
        collider2D.enabled = false;
        transform.localPosition = new Vector3(0, 0 , 0);

    }

}
