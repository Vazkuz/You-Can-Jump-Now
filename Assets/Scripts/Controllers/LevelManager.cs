using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public int currentStage { get { return _currentStage; } private set { _currentStage = value; } }
    [SerializeField] int _currentStage = 0;
    public int _nLevel { get { return nLevel.Value; } private set { nLevel.Value = value; } }
    private NetworkVariable<int> nLevel = new NetworkVariable<int>(0);
    private NetworkVariable<int> playersSetUp = new NetworkVariable<int>(0);
    [SerializeField] private List<Level> levelList;
    private Door currentDoor;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform pickaxePrefab;
    private Transform pickaxeObjectTransform; // Just to check if there's already a pickaxe in the scene.

    [SerializeField] private Transform goldPrefab;
    private Transform goldObjectTransform; // Just to check if there's already a pickaxe in the scene.

    public List<TriggerTarget> targets;

    [SerializeField] private TMP_Text DependencyMsg;
    [SerializeField] private float depMsgTime = 2f;

    public static event Action OnStageFinish;
    //private bool justConnecting = true;
    // Start is called before OnNetworkSpawn (on-scene object)
    protected void Start()
    {
        LoadLevel(nLevel.Value);
        //if(IsServer) nLevel.Value++;
        //PlayerNetwork.OnPlayerPrefabSpawn += AsyncSetupPlayerPos;
        Door.OnAllPlayersFinish += HandlePlayersFinishedLevel;
        DependencyMsg.gameObject.SetActive(false);
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
        if (nLevel.Value >= levelList.Count)
        {
            print("Se acabo este Stage");
            OnStageFinish?.Invoke();
            return;
        }

        // Check dependencias (pickaxe, gold). If everything is ok, continue.
        bool goldDependency = false;
        if (NetworkManager.ConnectedClients[0].PlayerObject.GetComponent<PlayerNetwork>().hasGold.Value ||
            NetworkManager.ConnectedClients[1].PlayerObject.GetComponent<PlayerNetwork>().hasGold.Value)
        {
            goldDependency = true;
        }

        // Check dependencias (pickaxe, gold). If everything is ok, continue.
        bool pickaxeDependency = false;
        if (NetworkManager.ConnectedClients[0].PlayerObject.GetComponent<PlayerNetwork>().hasPickaxe.Value ||
            NetworkManager.ConnectedClients[1].PlayerObject.GetComponent<PlayerNetwork>().hasPickaxe.Value)
        {
            pickaxeDependency = true;
        }

        // If all dependencies are correct, than the new level is loaded and nothing else happens.
        if(goldDependency && pickaxeDependency)
        {
            if(IsServer) nLevel.Value++;
            LoadLevel(nLevel.Value);
            return;
        }

        // Otherwise, we need to check which dependency is not correct, and we inform that to the players.
        HandleFailedDependencyRPC(goldDependency, pickaxeDependency);

        // Finally, we "respawn" the players and clean the door info. (This triggers IF there's no Gold or Pickaxe).
        SetUpPlayersPos(nLevel.Value, false);
        currentDoor.CleanFinishPlayers();
    }

    private void LoadLevel(int levelToLoad, bool isRetry = false)
    {
        if (!IsServer) return;
        //LOADSCREEN FUNCTIONALITY GOES HERE (WHEN WE HAVE IT)
        if (isRetry)
        {
            List<ulong> playersId = NetworkManager.Singleton.ConnectedClients.Keys.ToList();

            foreach (ulong playerId in playersId)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { playerId }
                    }
                };

                MakePlayersReleaseObjectsClientRpc(playerId, clientRpcParams);
            }
        }

        playersSetUp.Value = 0;
        SetUpLevelRpc(levelToLoad, isRetry);

        //if (justConnecting) return;

        SetUpPlayersPos(levelToLoad);

        print($"Setting up grabbables last saved pos. Pickaxe: {levelList[levelToLoad].pickaxePos.position}");
        if (FindObjectOfType<Pickaxe>())
        {
            FindObjectOfType<Pickaxe>().GetComponent<Grabbable>().lastSavedPos.Value = levelList[levelToLoad].pickaxePos.position;
        }
        else
        {
            SetUpObject(pickaxeObjectTransform, pickaxePrefab, levelList[levelToLoad].pickaxePos.position);
        }

        if (FindObjectOfType<Gold>())
        {
            FindObjectOfType<Gold>().GetComponent<Grabbable>().lastSavedPos.Value = levelList[levelToLoad].goldPos.position;
        }
        else
        {
            SetUpObject(goldObjectTransform, goldPrefab, levelList[levelToLoad].goldPos.position);
        }
    }

    [ClientRpc]
    private void MakePlayersReleaseObjectsClientRpc(ulong playerId, ClientRpcParams clientRpcParams = default)
    {
        Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
        player.GetComponent<PlayerNetwork>().CallReleaseObject();
    }

    private void SetUpPlayersPos(int level, bool newLevel = true)
    {
        List<ulong> playersId = NetworkManager.Singleton.ConnectedClients.Keys.ToList();

        foreach (ulong playerId in playersId)
        {
            Transform player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId).GetComponent<Transform>();
            if (newLevel)
            {
                if (player.GetComponent<PlayerNetwork>().hasGold.Value)
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[level].playersPos[0].position);
                    player.GetComponent<PlayerNetwork>().lastSavedPos.Value = levelList[level].playersPos[0].position;
                }
                else if (player.GetComponent<PlayerNetwork>().hasPickaxe.Value)
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[level].playersPos[1].position);
                    player.GetComponent<PlayerNetwork>().lastSavedPos.Value = levelList[level].playersPos[1].position;
                }
                else
                {
                    player.GetComponent<PlayerNetwork>().SetUpPlayer(levelList[level].playersPos[playersSetUp.Value].position);
                    player.GetComponent<PlayerNetwork>().lastSavedPos.Value = levelList[level].playersPos[playersSetUp.Value].position;
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
    private void SetUpLevelRpc(int levelToSetup, bool isRetry = false)
    {
        Level currentLevel = levelList[levelToSetup];
        currentDoor = currentLevel.door;
        if(IsServer && isRetry) currentDoor.ResetInitialConditions();
        currentLevel.gameObject.SetActive(true);
        foreach (Level level in levelList)
        {
            if(level != currentLevel) level.gameObject.SetActive(false);
        }

        foreach(Mineral mineral in currentLevel.minerals)
        {
            mineral.ResetInitialConditions();
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

    // OJO: TODO ESTO TENDRA QUE HACERSE CON EL TEMA DE LENGUAJE PARA LOCALIZACION. ESTO ES TEMPORAL
    [Rpc(SendTo.Everyone)]
    private void HandleFailedDependencyRPC(bool goldDependency, bool pickaxeDependency)
    {
        if(!goldDependency && !pickaxeDependency)
        {
            SetDepMessage("Faltan el oro y el pico");
        }
        else if (!goldDependency)
        {
            SetDepMessage("Falta el oro");
        }
        else if (!pickaxeDependency)
        {
            SetDepMessage("Falta el pico");
        }
    }

    private void SetDepMessage(string message)
    {
        DependencyMsg.text = message;
        CancelInvoke("TurnOffMessageVisibility");
        DependencyMsg.gameObject.SetActive(true);
        Invoke("TurnOffMessageVisibility", depMsgTime);
    }

    protected void TurnOffMessageVisibility()
    {
        DependencyMsg.gameObject.SetActive(false);
    }

    public void ResetLevel()
    {
        LoadLevel(nLevel.Value, true);
    }
}
