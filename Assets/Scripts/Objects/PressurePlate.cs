using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PressurePlate : NetworkBehaviour
{
    [SerializeField] private Transform unpressedPos;
    [SerializeField] private Transform pressedPos;
    [SerializeField] private float minimumWeight;

    private float currentWeight;

    private NetworkVariable<bool> isPressed = new NetworkVariable<bool>(false);

    protected void Start()
    {
        transform.position = unpressedPos.position;
        currentWeight = 0;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlateInteractable>() == null) return;
        if (!collision.enabled) return;

        OnWeightAdded(collision.gameObject.GetComponent<PlateInteractable>().weight.Value);

        if (collision.gameObject.GetComponent<PlayerNetwork>() == null) return;
        PlayerNetwork.OnWeightAdded += OnWeightAdded;
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlateInteractable>() == null) return;
        if (!collision.enabled) return;

        OnWeightAdded(-collision.gameObject.GetComponent<PlateInteractable>().weight.Value);

        if (collision.gameObject.GetComponent<PlayerNetwork>() == null) return;
        PlayerNetwork.OnWeightAdded -= OnWeightAdded;
    }

    private void OnWeightAdded(float releasedWeight)
    {
        currentWeight += releasedWeight;

        if (!IsServer) return;

        if(currentWeight >= minimumWeight)
        {
            isPressed.Value = true;
        }
        else
        {
            isPressed.Value = false;
        }
    }

    protected void OnDisable()
    {
        PlayerNetwork.OnWeightAdded -= OnWeightAdded;
    }
}
