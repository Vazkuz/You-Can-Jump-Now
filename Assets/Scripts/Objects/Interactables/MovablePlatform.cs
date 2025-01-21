using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovablePlatform : TriggerTarget
{
    [SerializeField] private Transform inactivePos;
    [SerializeField] private Transform activePos;
    [SerializeField] private float speed = 5f;
    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    protected void Update()
    {
        if (isActive.Value && transform.position == activePos.position) return;
        if (!isActive.Value && transform.position == inactivePos.position) return;

        if (isActive.Value)
        {
            transform.position = Vector3.MoveTowards(transform.position, activePos.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, inactivePos.position, speed * Time.deltaTime);
        }
    }
}
