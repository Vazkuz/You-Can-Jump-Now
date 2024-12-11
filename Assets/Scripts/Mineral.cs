using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mineral : Breakable
{
    public static event Action<ulong> OnFinishedMine;

    protected override void OnBreak(ulong player)
    {
        FinishMineRpc(player);
        base.OnBreak(player);
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
