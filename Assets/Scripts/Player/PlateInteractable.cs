using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateInteractable : NetworkBehaviour
{
    //public float weight { get { return _weight; } private set { _weight = value; } }
    //[SerializeField] private float _weight;

    public NetworkVariable<float> weight = new NetworkVariable<float>(1, default, NetworkVariableWritePermission.Owner);
    [SerializeField] private float ownWeight;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        weight.Value = ownWeight;
    }

    public void AddWeight(float weight)
    {
        this.weight.Value += weight;
    }
}
