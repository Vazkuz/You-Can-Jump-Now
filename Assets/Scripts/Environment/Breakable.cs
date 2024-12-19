using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Breakable : NetworkBehaviour
{
    [SerializeField] protected NetworkVariable<int> health = new NetworkVariable<int>(3);
    [SerializeField] protected LayerMask playerLayer;
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
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Mathf.Log(playerLayer, 2)) return;

        if (collision.GetComponent<PlayerNetwork>().hasPickaxe.Value)
        {
            PlayerNetwork.OnMining += OnHit;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Mathf.Log(playerLayer, 2)) return;

        if (collision.GetComponent<PlayerNetwork>().hasPickaxe.Value)
        {
            PlayerNetwork.OnMining -= OnHit;
        }

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

    /// <summary>
    /// OnBreak is called only on the server once the breakable's health reaches 0.
    /// </summary>
    /// <param name="player">The player who broke the breakable.</param>
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
