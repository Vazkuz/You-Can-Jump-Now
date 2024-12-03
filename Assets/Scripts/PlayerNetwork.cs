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

    [Header("Player Movement Vars")]
    [SerializeField] private float moveSpeed = 3f;
    private PlayerInputActions inputActions;
    [SerializeField] float jumpForce = 50f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    InputAction moveAction;
    InputAction jumpAction;
    private Vector2 moveInput;
    private new Rigidbody2D rb;

    [Header("Ground Variables")]
    //[SerializeField] bool isGrounded = false; //BORRAR LUEGO, SOLO PARA DEBUG
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;
    [SerializeField] private LayerMask groundLayer;

    protected void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    protected void OnEnable()
    {
        inputActions.Enable();
        moveAction = inputActions.PlayerControls.move;

        jumpAction = inputActions.PlayerControls.jump;

        inputActions.PlayerControls.jump.performed += OnJump;
    }

    protected void OnDisable()
    {
        inputActions.PlayerControls.jump.performed -= OnJump;
        inputActions.Disable();
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected void Update()
    {
        HandleNetworkMovement();

    }

    private void HandleNetworkMovement()
    {
        //Check if client is owner. If it's not, it can't move the player
        if (!IsOwner && !isDebugScene) return;
        moveInput = moveAction.ReadValue<Vector2>();

        rb.velocity = Vector2.right * moveInput.x * moveSpeed + Vector2.up * rb.velocity.y;

        HandleJump();
    }

    private void HandleJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity +=  Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if(rb.velocity.y > 0 && !jumpAction.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner && !isDebugScene) return;
        if (!IsGrounded()) return;
        rb.velocity = Vector2.up * jumpForce;

    }

    public bool IsGrounded()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    //protected void OnCollisionEnter2D(Collision2D other)
    //{
    //    //se puede usar tag, pero por ahora no lo hare. Revisitar esta decision mas adelante.
    //    Vector3 normal = other.GetContact(0).normal;
    //    if (normal == Vector3.up) isGrounded = true;
    //}

    //protected void OnCollisionExit2D(Collision2D other)
    //{
    //    //se puede usar tag, pero por ahora no lo hare. Revisitar esta decision mas adelante.
    //    isGrounded = false;
    //}

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
