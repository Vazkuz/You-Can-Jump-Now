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
    [SerializeField] private Transform pickaxePrefab;
    private Transform pickaxeObjectTransform; // Just to check if there's already a pickaxe in the scene.
    private NetworkVariable<bool> isTherePickaxe = new NetworkVariable<bool>(false);
    private bool justConnecting = true;
    // Start is called before the first frame update
    protected void Start()
    {
        LoadLevel(nLevel.Value);
        PlayerNetwork.OnPlayerPrefabSpawn += SetUpPlayerPos;
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

    private void LoadLevel(int nLevel)
    {
        if (!IsServer) return;
        //LOADSCREEN FUNCTIONALITY GOES HERE (WHEN WE HAVE IT)
        playersSetUp.Value = 0;
        SetUpLevel(levelList[nLevel]);

        if (justConnecting) return;

        SetUpPlayersPos();
        SetUpPickaxe();
    }

    private void SetUpPlayersPos()
    {
        //configurar la posicion de ambos jugadores
    }

    private void SetUpLevel(Level level)
    {
        if (!IsServer) return;
        mainCamera.position = level.cameraPos.position;
    }

    private async void SetUpPlayerPos(ulong playerId)
    {
        await WaitUntilSpawnedAsync();

        if(!IsServer) return;

        Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
        player.position = levelList[nLevel.Value].playersPos[playersSetUp.Value].position;
        playersSetUp.Value++;

        if (playersSetUp.Value <= 1)
        {
            SetUpPickaxe();
            // Añadir GOLD POSITION aquí
        }

        if (playersSetUp.Value >= 2)
        {
            justConnecting = false;
        }
    }

    private void SetUpPickaxe()
    {
        //First we check if the pickaxe has already been spawned. If not, we have to spawn it.
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
