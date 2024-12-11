using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneManager : NetworkBehaviour
{
    [SerializeField] private Transform pickaxePrefab;
    private Transform pickaxeObjectTransform;

    // Start is called before the first frame update
    public void SpawnPickaxe()
    {
        if (!IsServer)
        {
            RequestSpawnPickaxeRpc();
        }
        else
        {
            SpawnPickaxeOnServer();
        }
        
    }

    [Rpc(SendTo.Server)]
    private void RequestSpawnPickaxeRpc()
    {
        SpawnPickaxeOnServer();
    }

    private void SpawnPickaxeOnServer()
    {
        if (pickaxeObjectTransform == null)
        {
            pickaxeObjectTransform = Instantiate(pickaxePrefab);
            pickaxeObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
