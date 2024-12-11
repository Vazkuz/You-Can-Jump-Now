using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Breakable : NetworkBehaviour
{
    [SerializeField] protected NetworkVariable<int> health = new NetworkVariable<int>(3);
    private NetworkVariable<bool> itsBroken = new NetworkVariable<bool>(false);
    NetworkObject networkObject;

    //Start, on in-scene objects, occurs BEFORE OnNetworkSpawn.
    protected virtual void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    // Esto ocurre despues de Start
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerNetwork.OnMining += OnHit;
    }

    protected virtual void OnHit(ulong player)
    {
        if (itsBroken.Value) return;

        health.Value--;
        if (health.Value <= 0)
        {
            if (IsServer)
            {
                OnBreak(player);
            }
        }
    }

    protected virtual void OnBreak(ulong player)
    {
        itsBroken.Value = true;
        networkObject.Despawn(gameObject);
    }
}
