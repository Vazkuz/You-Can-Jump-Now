using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void Update()
    {
        if (!IsOwner) return;
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.PlayerControls.move.performed += OnMove;
        inputActions.PlayerControls.move.canceled += OnMove;
    }

    private void OnDisable()
    {
        inputActions.PlayerControls.move.performed -= OnMove;
        inputActions.PlayerControls.move.canceled -= OnMove;
        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
    }
}
