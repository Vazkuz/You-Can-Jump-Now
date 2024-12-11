using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Breakable : NetworkBehaviour
{
    [SerializeField] protected NetworkVariable<int> health = new NetworkVariable<int>(3);
    private NetworkVariable<bool> itsBroken = new NetworkVariable<bool>(false);
    protected NetworkObject networkObject;

    public static event Action<ulong> OnFinishedMine;

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
        FinishMineRpc(player);
    }

    /// <summary>
    /// Once the mineral's health is 0, it has been mined completely. All clients get this information with this Rpc.
    /// </summary>
    /// <param name="clientId">ID of the last player that mined the mineral. In other words, id of the player who destroyed the mineral.</param>
    [Rpc(SendTo.Everyone)]
    private void FinishMineRpc(ulong clientId)
    {
        OnFinishedMine?.Invoke(clientId);
    }
}
