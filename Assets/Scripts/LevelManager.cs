using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    private NetworkVariable<int> nLevel =  new NetworkVariable<int>(0);
    private NetworkVariable<int> playersSetUp = new NetworkVariable<int>(0);
    [SerializeField] private List<Level> levelList;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform pickaxePrefab;
    private Transform pickaxeObjectTransform; // Just to check if there's already a pickaxe in the scene.
    private NetworkVariable<bool> isTherePickaxe = new NetworkVariable<bool>(false);
    //private bool justConnecting = true;
    // Start is called before OnNetworkSpawn (on-scene object)
    protected void Start()
    {
        LoadLevel();
        //PlayerNetwork.OnPlayerPrefabSpawn += AsyncSetupPlayerPos;
        Door.OnAllPlayersFinish += HandlePlayersFinishedLevel;
    }
    protected void OnDisable()
    {
        Door.OnAllPlayersFinish -= HandlePlayersFinishedLevel;
    }

    private async Task WaitUntilSpawnedAsync()
    {
        //Wait until the Level Manager has spawned
        NetworkObject networkObject = GetComponent<NetworkObject>();
        while (!networkObject.IsSpawned)
        {
            await Task.Yield();
        }
    }

    private void HandlePlayersFinishedLevel()
    {
        if (!IsServer) return;
        // Check dependencias (pickaxe, gold). If everything is ok, continue.
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (!IsServer) return;
        //LOADSCREEN FUNCTIONALITY GOES HERE (WHEN WE HAVE IT)
        playersSetUp.Value = 0;
        SetUpLevelRpc();

        //if (justConnecting) return;

        SetUpPlayersPos();
        SetUpPickaxe();
        nLevel.Value++;
    }

    private void SetUpPlayersPos()
    {
        List<ulong> playersId = NetworkManager.Singleton.ConnectedClients.Keys.ToList();

        foreach (ulong playerId in playersId)
        {
            //SetupPlayerPosRpc(playerId);
            Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
            player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[nLevel.Value].playersPos[playersSetUp.Value].position);
            playersSetUp.Value++;
        }

    }

    [Rpc(SendTo.Everyone)]
    private void SetUpLevelRpc()
    {
        Level level = levelList[nLevel.Value];
        level.gameObject.SetActive(true);
        foreach (Level _level in levelList)
        {
            if(_level != level) _level.gameObject.SetActive(false);
        }
        mainCamera.position = level.cameraPos.position;
    }

    private async void AsyncSetupPlayerPos(ulong playerId)
    {
        await WaitUntilSpawnedAsync();

        if(!IsServer) return;

        Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
        player.position = levelList[nLevel.Value].playersPos[playersSetUp.Value].position;
        playersSetUp.Value++;

        //if (playersSetUp.Value <= 1)
        //{
        //    SetUpPickaxe();
        //    // Añadir GOLD POSITION aquí
        //}

        //if (playersSetUp.Value >= 2)
        //{
        //    justConnecting = false;
        //}
    }

    private void SetUpPickaxe()
    {
        //First we check if the pickaxe has already been spawned. If not, we spawn it.
        if (pickaxeObjectTransform == null)
        {
            pickaxeObjectTransform = Instantiate(pickaxePrefab);
            pickaxeObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            isTherePickaxe.Value = true;
        }

        //Then, we change its position. We do this so we can just move it if it's already there.
        pickaxeObjectTransform.position = levelList[nLevel.Value].pickaxePos.position;
    }
}
