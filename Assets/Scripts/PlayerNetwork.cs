using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] bool isDebugScene = false;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 3f;
    private PlayerInputActions inputActions;
    [SerializeField] float jumpForce = 50f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    InputAction moveAction;
    InputAction jumpAction;
    private Vector2 moveInput;
    private new Rigidbody2D rb;

    protected void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected void Update()
    {
        //Check if client is owner. If it's not, it can't move the player
        if (!IsOwner || isDebugScene) return;
        HandleMovement();

    }

    private void HandleMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        HandleJump();
    }

    private void HandleJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if(rb.velocity.y > 0 && !jumpAction.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    protected void OnEnable()
    {
        inputActions.Enable();
        moveAction = inputActions.PlayerControls.move;

        jumpAction = inputActions.PlayerControls.jump;

        //inputActions.PlayerControls.jump.performed += OnJump;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner || isDebugScene) return;
        rb.velocity = Vector2.up * jumpForce;

    }

    protected void OnDisable()
    {
        //inputActions.PlayerControls.jump.performed -= OnJump;
        inputActions.Disable();
    }

    //[ServerRpc]
    //private void TestServerRpc()
    //{
    //    print("TestServerRpc " + OwnerClientId);
    //}

    //[ClientRpc]
    //private void TestClientRpc()
    //{
    //    print("TestClientRpc");
    //}
}
