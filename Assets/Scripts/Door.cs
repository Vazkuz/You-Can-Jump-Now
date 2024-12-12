using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using WebSocketSharp;

public class Door : Breakable
{
    [SerializeField] private SpriteRenderer rockSprite;
    [SerializeField] private GameObject exit;

    private NetworkVariable<bool> exitOpen = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<int> nPlayersFinished = new NetworkVariable<int>(0);

    public static event Action OnAllPlayersFinish;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        exit.SetActive(false);
    }

    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); // en principio si la puerta se destruye ya no se puede regenerar
        exitOpen.Value = true;
        DoorOpenRpc();
        //networkObject.Despawn(gameObject);
    }

    [Rpc(SendTo.Everyone)]
    private void DoorOpenRpc()
    {
        exit.SetActive(true);
        rockSprite.enabled = false;
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
    }

    private void OnPlayerGoThroughDoor(ulong player)
    {
        if (!IsServer)
        {
            RequestHandlePlayerGoThroughRpc(player);
        }
        else
        {
            HandlePlayerGoThrough(player);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestHandlePlayerGoThroughRpc(ulong player)
    {
        HandlePlayerGoThrough(player);
    }

    private void HandlePlayerGoThrough(ulong player)
    {
        nPlayersFinished.Value++;
        if (nPlayersFinished.Value >= 2)
        {
            AllPlayersWentThroughRpc();
        }

    }

    [Rpc(SendTo.Everyone)]
    private void AllPlayersWentThroughRpc()
    {
        print("Todos los jugadores pasaron yee");
        OnAllPlayersFinish?.Invoke();
    }

}
