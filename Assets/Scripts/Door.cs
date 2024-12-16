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

    [SerializeField] private NetworkVariable<bool> exitOpen = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<int> nPlayersFinished = new NetworkVariable<int>(0);

    public static event Action OnAllPlayersFinish;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (exitOpen.Value)
        {
            exitOpen.Value = true;
            DoorOpenRpc();
            return;
        }
        exit.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (!exitOpen.Value) return;
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (!exitOpen.Value) return;

        PlayerNetwork.OnExit -= OnPlayerGoThroughDoor;
    }

    protected void OnDisable()
    {
        PlayerNetwork.OnExit -= OnPlayerGoThroughDoor;
    }

    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); // en principio si la puerta se destruye ya no se puede regenerar
        exitOpen.Value = true;
        DoorOpenRpc();
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
        //networkObject.Despawn(gameObject);
    }

    [Rpc(SendTo.Everyone)]
    private void DoorOpenRpc()
    {
        exit.SetActive(true);
        rockSprite.enabled = false;
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
        print("todos pasaron");
        OnAllPlayersFinish?.Invoke();
    }

}
