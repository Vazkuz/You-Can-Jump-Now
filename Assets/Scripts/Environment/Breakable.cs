using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Breakable : NetworkBehaviour
{
    [SerializeField] protected NetworkVariable<int> health = new NetworkVariable<int>(3);
    protected int InitialHealth;
    [SerializeField] protected LayerMask playerLayer;
    private NetworkVariable<bool> itsBroken = new NetworkVariable<bool>(false);
    protected NetworkObject networkObject;

    [Tooltip("Break Sprites. Poner solo los de romper, el inicial no.")]
    [SerializeField] protected List<Sprite> breakSprites;

    [SerializeField] private Sprite initialSprite;

    //Start, on in-scene objects, occurs BEFORE OnNetworkSpawn.
    protected virtual void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        InitialHealth = health.Value;
        initialSprite = GetBreakableSpriteRenderer().sprite;
    }

    protected virtual SpriteRenderer GetBreakableSpriteRenderer()
    {
        return GetComponent<SpriteRenderer>();
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
            if (!IsServer) return;

            OnBreak(player);
            return;
        }
        UpdateBreakSpriteRpc(InitialHealth - health.Value - 1);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateBreakSpriteRpc(int phaseCount)
    {
        GetBreakableSpriteRenderer().sprite = breakSprites[phaseCount];
    }

    /// <summary>
    /// OnBreak is called only on the server once the breakable's health reaches 0.
    /// </summary>
    /// <param name="player">The player who broke the breakable.</param>
    protected virtual void OnBreak(ulong player)
    {
        itsBroken.Value = true;
    }

    public virtual void ResetInitialConditions()
    {
        if (!IsServer) return;
        ResetSpriteRpc();
        health.Value = InitialHealth;
        itsBroken.Value = false;
        PlayerNetwork.OnMining -= OnHit;
    }

    [Rpc(SendTo.Everyone)]
    private void ResetSpriteRpc()
    {
        GetBreakableSpriteRenderer().sprite = initialSprite;
        GetBreakableSpriteRenderer().enabled = true;
    }

}
