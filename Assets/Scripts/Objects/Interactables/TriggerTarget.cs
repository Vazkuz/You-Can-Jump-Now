using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TriggerTarget : NetworkBehaviour
{
    public NetworkVariable<bool> isActive { get; private set; } = new NetworkVariable<bool>(false);
    public virtual void Activate()
    {
        isActive.Value = true;
        print($"Target {name} activated.");
    }
    public virtual void Deactivate()
    {
        isActive.Value = false;
        print($"Target {name} deactivated.");
    }
}
