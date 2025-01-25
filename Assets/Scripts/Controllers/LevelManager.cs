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
    private Door currentDoor;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform pickaxePrefab;
    private Transform pickaxeObjectTransform; // Just to check if there's already a pickaxe in the scene.
    //private NetworkVariable<bool> isTherePickaxe = new NetworkVariable<bool>(false);

    [SerializeField] private Transform goldPrefab;
    private Transform goldObjectTransform; // Just to check if there's already a pickaxe in the scene.
    //private NetworkVariable<bool> isThereGold = new NetworkVariable<bool>(false);

    public List<TriggerTarget> targets;

    public static event Action OnStageFinish;
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

    //private async Task WaitUntilSpawnedAsync()
    //{
    //    //Wait until the Level Manager has spawned
    //    NetworkObject networkObject = GetComponent<NetworkObject>();
    //    while (!networkObject.IsSpawned)
    //    {
    //        await Task.Yield();
    //    }
    //}

    private void HandlePlayersFinishedLevel()
    {
        if (!IsServer) return;
        if(nLevel.Value >= levelList.Count)
        {
            print("Se acabo este Stage");
            OnStageFinish?.Invoke();
            return;
        }

        // Check dependencias (pickaxe, gold). If everything is ok, continue.
        bool goldDependency = false;
        if (NetworkManager.ConnectedClients[0].PlayerObject.GetComponent<PlayerNetwork>().hasGold.Value)
        {
            goldDependency = true;
        }

        if (NetworkManager.ConnectedClients[1].PlayerObject.GetComponent<PlayerNetwork>().hasGold.Value)
        {
            goldDependency = true;
        }

        // Check dependencias (pickaxe, gold). If everything is ok, continue.
        bool pickaxeDependency = false;
        if (NetworkManager.ConnectedClients[0].PlayerObject.GetComponent<PlayerNetwork>().hasPickaxe.Value)
        {
            pickaxeDependency = true;
        }

        if (NetworkManager.ConnectedClients[1].PlayerObject.GetComponent<PlayerNetwork>().hasPickaxe.Value)
        {
            pickaxeDependency = true;
        }

        // If all dependencies are correct, than the new level is loaded and nothing else happens.
        if(goldDependency && pickaxeDependency)
        {
            LoadLevel();
            return;
        }

        // Otherwise, we need to check which dependency is not correct, and we inform that to the players.

        if (!goldDependency && !pickaxeDependency)
        {
            // Avisar que no hay pico ni oro
        }

        if (!goldDependency)
        {
            // Avisar que no hay oro
        }

        if (!pickaxeDependency)
        {
            // Avisar que no hay pico
        }

        // Finally, we "respawn" the players and clean the door info.
        SetUpPlayersPos(false);
        currentDoor.CleanFinishPlayers();
    }

    private void LoadLevel()
    {
        if (!IsServer) return;
        //LOADSCREEN FUNCTIONALITY GOES HERE (WHEN WE HAVE IT)
        playersSetUp.Value = 0;
        SetUpLevelRpc();

        //if (justConnecting) return;

        SetUpPlayersPos();

        if(!FindObjectOfType<Pickaxe>()) 
            SetUpObject(pickaxeObjectTransform, pickaxePrefab, levelList[nLevel.Value].pickaxePos.position);

        if (!FindObjectOfType<Gold>())
            SetUpObject(goldObjectTransform, goldPrefab, levelList[nLevel.Value].goldPos.position);
        nLevel.Value++;
    }

    private void SetUpPlayersPos(bool newLevel = true)
    {
        List<ulong> playersId = NetworkManager.Singleton.ConnectedClients.Keys.ToList();

        foreach (ulong playerId in playersId)
        {
            //SetupPlayerPosRpc(playerId);
            Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
            if (newLevel)
            {
                if (player.GetComponent<PlayerNetwork>().hasGold.Value)
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[nLevel.Value].playersPos[0].position);
                }
                else if (player.GetComponent<PlayerNetwork>().hasPickaxe.Value)
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[nLevel.Value].playersPos[1].position);
                }
                else
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[nLevel.Value].playersPos[playersSetUp.Value].position);
                }
                playersSetUp.Value++;
            }
            else
            {
                player.GetComponent<PlayerNetwork>().SetUpPlayer(player.transform.position);
            }
        }

    }

    [Rpc(SendTo.Everyone)]
    private void SetUpLevelRpc()
    {
        Level currentLevel = levelList[nLevel.Value];
        currentDoor = currentLevel.door;
        currentLevel.gameObject.SetActive(true);
        foreach (Level level in levelList)
        {
            if(level != currentLevel) level.gameObject.SetActive(false);
        }
        mainCamera.position = currentLevel.cameraPos.position;
    }

    //private async void AsyncSetupPlayerPos(ulong playerId)
    //{
    //    await WaitUntilSpawnedAsync();

    //    if(!IsServer) return;

    //    Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
    //    player.position = levelList[nLevel.Value].playersPos[playersSetUp.Value].position;
    //    playersSetUp.Value++;

    //    //if (playersSetUp.Value <= 1)
    //    //{
    //    //    SetUpPickaxe();
    //    //    // Añadir GOLD POSITION aquí
    //    //}

    //    //if (playersSetUp.Value >= 2)
    //    //{
    //    //    justConnecting = false;
    //    //}
    //}

    private void SetUpObject(Transform objectTransform, Transform objectPrefab, Vector3 setupPos)
    {
        //First we check if the pickaxe has already been spawned. If not, we spawn it.
        if (objectTransform == null)
        {
            objectTransform = Instantiate(objectPrefab);
            objectTransform.GetComponent<NetworkObject>().Spawn(true);
            //isTherePickaxe.Value = true;
        }

        //Then, we change its position. We do this so we can just move it if it's already there.
        objectTransform.position = setupPos;
    }
}
