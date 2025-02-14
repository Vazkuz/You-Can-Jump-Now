using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PressurePlate : NetworkBehaviour
{
    [SerializeField] private Transform unpressedPos;
    [SerializeField] private TriggerTarget target;
    [Tooltip("Activator Types Required for this plate")] [SerializeField] private List<PlateActivator.ActivatorType> typesRq;
    private int typeRqsMet = 0;
    private NetworkVariable<bool> isPressed = new NetworkVariable<bool>(false);

    protected void Start()
    {
        transform.localPosition = unpressedPos.localPosition;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlateActivator>() == null) return;
        if (!collision.enabled) return;

        OnActivatorAdd(collision.gameObject.GetComponent<PlateActivator>().type, true);

        if (collision.gameObject.GetComponent<PlayerNetwork>() == null) return;
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlateActivator>() == null) return;
        if (!collision.enabled) return;

        OnActivatorAdd(collision.gameObject.GetComponent<PlateActivator>().type, false);

        if (collision.gameObject.GetComponent<PlayerNetwork>() == null) return;
    }


    private void OnActivatorAdd(PlateActivator.ActivatorType type, bool isAdded)
    {
        if (!IsServer) return;

        if(!typesRq.Contains(type)) return;

        if(isAdded) typeRqsMet++;
        else typeRqsMet--;

        if (typeRqsMet >= typesRq.Count)
        {
            isPressed.Value = true;
            target.Activate();
        }
        else
        {
            if (!isPressed.Value) return;

            isPressed.Value = false;

            if (!target.isActive.Value) return; //no need to deactivate what it was not active

            target.Deactivate();
        }
    }
}
