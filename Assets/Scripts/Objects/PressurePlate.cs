using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PressurePlate : NetworkBehaviour
{
    [SerializeField] private Transform unpressedPos;
    [SerializeField] private Transform pressedPos;
    [SerializeField] private float minimumWeight;

    private NetworkVariable<bool> isPressed = new NetworkVariable<bool>(false);

    protected void Start()
    {
        transform.position = unpressedPos.position;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlateInteractable>() == null) return;

        print("Alguien pisa la placa");
    }
}
