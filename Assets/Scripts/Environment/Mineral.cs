using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mineral : Breakable
{

    public static event Action<ulong> OnFinishedMine;
    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    // El mineral, eventualmente, tambien puede regenerarse
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); //eventualmente cambiar esto, no se destruye sino que se esconde mientras el jugador no salte o algo asi
        FinishMineRpc(player);
        ToggleSpriteRpc(false);
        //networkObject.Despawn(gameObject);
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

    [Rpc(SendTo.Everyone)]
    private void ToggleSpriteRpc(bool isActive)
    {
        GetComponent<SpriteRenderer>().enabled = isActive;
    }

    public override void ResetInitialConditions()
    {
        base.ResetInitialConditions();
        ToggleSpriteRpc(true);
    }
}
