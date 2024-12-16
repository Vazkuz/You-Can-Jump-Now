using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : NetworkBehaviour
{
    protected void Start()
    {
        Door.OnAllPlayersFinish += StartGame;
    }

    private void StartGame()
    {
        if (!IsServer) return;

        string nextScene = NameFromIndex(SceneManager.GetActiveScene().buildIndex + 1);

        NetworkManager.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

    private static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }
}
