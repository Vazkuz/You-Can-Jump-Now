using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mineral : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>(3);
    public static event Action<ulong> OnFinishedMine;
    private NetworkVariable<bool> canBeMined = new NetworkVariable<bool>(true);
    NetworkObject thisNGO;

    //Start, on in-scene objects, occurs BEFORE OnNetworkSpawn.
    protected void Start()
    {
        thisNGO = GetComponent<NetworkObject>();
    }

    // Esto ocurre despues de Start
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerNetwork.OnMining += OnMined;
    }

    private void OnMined(ulong player)
    {
        if (!canBeMined.Value) return;

        health.Value--;
        if (health.Value <= 0)
        {
            print("Mineral destruido por el cliente " + player);
            if(IsServer)
            {
                //FinishMineOnServer(player);
                canBeMined.Value = false;
                FinishMineRpc(player);
                thisNGO.Despawn(gameObject);
            }
        }
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

    private void FinishMineOnServer(ulong clientId)
    {
        canBeMined.Value = false;
        OnFinishedMine?.Invoke(clientId);
    }


}
