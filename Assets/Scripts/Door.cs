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
    //[SerializeField] private NetworkVariable<int> nPlayersFinished = new NetworkVariable<int>(0);
    [SerializeField] private NetworkList<ulong> finishPlayers;

    public static event Action OnAllPlayersFinish;

    protected void Awake()
    {
        finishPlayers = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!exitOpen.Value) //puerta cerrada
        {
            exit.SetActive(false);
            return;
        }

        //puerta abierta (desde el comienzo)
        DoorOpenRpc();
        //If the exit is open when spawned, then subscribe to OnExit, because it won't be broken.
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
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

    //public override void OnDestroy()
    //{
    //    base.OnDestroy();
    //    finishPlayers.Dispose();
    //}

    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); // en principio si la puerta se destruye ya no se puede regenerar
        exitOpen.Value = true;
        DoorOpenRpc();
        if (!IsServer)
        {
            RequestSubscribeToPlayerRpc();
        }
        else
        {
            SubscribeToPlayer();
        }
        //networkObject.Despawn(gameObject);
    }

    [Rpc(SendTo.Server)]
    private void RequestSubscribeToPlayerRpc()
    {
        SubscribeToPlayer();
    }

    private void SubscribeToPlayer()
    {
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
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
        if (finishPlayers.Contains(player)) return;

        finishPlayers.Add(player);
        //nPlayersFinished.Value++;
        if (finishPlayers.Count >= 2)//nPlayersFinished.Value >= 2)
        {
            AllPlayersWentThroughRpc();
        }

    }

    [Rpc(SendTo.Server)]
    private void AllPlayersWentThroughRpc()
    {
        OnAllPlayersFinish?.Invoke();
    }

}
