using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    private NetworkVariable<int> nLevel =  new NetworkVariable<int>(0);
    private NetworkVariable<int> playersSetUp = new NetworkVariable<int>(0);
    [SerializeField] private List<Level> levelList;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform pickaxe;
    // Start is called before the first frame update
    protected void Start()
    {
        LoadLevel(nLevel.Value);
        PlayerNetwork.OnPlayerPrefabSpawn += SetUpPlayerPos;
    }

    private async Task WaitUntilSpawnedAsync()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        while (!networkObject.IsSpawned)
        {
            await Task.Yield(); // Yield control back to the main thread
        }
    }

    private void LoadLevel(int nLevel)
    {
        if (!IsServer) return;
        //LOADSCREEN FUNCTIONALITY GOES HERE (WHEN WE HAVE IT)
        playersSetUp.Value = 0;
        SetUpLevel(levelList[nLevel]);
    }

    private void SetUpLevel(Level level)
    {
        if (!IsServer) return;
        mainCamera.position = level.cameraPos.position;
        //pickaxe.position = level.pickaxePos.position;
        // Añadir GOLD POSITION aquí
    }

    private async void SetUpPlayerPos(ulong playerId)
    {
        await WaitUntilSpawnedAsync();

        if(!IsServer) return;

        Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
        player.position = levelList[nLevel.Value].playersPos[playersSetUp.Value].position;
        playersSetUp.Value++;
    }
}
