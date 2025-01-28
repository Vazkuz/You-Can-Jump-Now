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
    }
    protected void OnEnable()
    {
        PlayerNetwork.OnExit += OnPlayerGoThroughDoor;
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
        if (finishPlayers.Count >= 2)
        {
            AllPlayersWentThroughRpc();
        }

    }

    [Rpc(SendTo.Server)]
    private void AllPlayersWentThroughRpc()
    {
        OnAllPlayersFinish?.Invoke();
    }

    public void CleanFinishPlayers()
    {
        finishPlayers.Clear();
    }

}
