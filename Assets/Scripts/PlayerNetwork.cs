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

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeedGround = 3f;
    [SerializeField] private float moveSpeedAir = 3f;
    private float moveSpeed;
    private PlayerInputActions inputActions;
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.1f;
    private float mayJumpTime;
    InputAction moveAction;
    InputAction jumpAction;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    [Header("Ground Variables")]
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;
    [SerializeField] private LayerMask groundLayer;
    private bool _isFlying = false;
    private bool justChangedD = false;

    [Header("Pickaxe Variables")]
    [SerializeField] private LayerMask pickaxeLayer;
    public Transform Hand { get { return hand; } private set { hand = value; } }
    [SerializeField] private Transform hand;

    [Header("Lobby Vars")]
    [SerializeField] SpriteRenderer hostSign;

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
        _isFlying = !IsGrounded();
        mayJumpTime = 0f;
        moveSpeed = moveSpeedGround;
        //if(!IsServer) hostSign.enabled = false; // MAS ADELANTE AÑADIR ESTO, JUNTO CON UN LOBBY MANAGER.
    }

    protected void Update()
    {
        if(!_isFlying) mayJumpTime = coyoteTime;
        mayJumpTime -= Time.deltaTime;
        HandleNetworkMovement();

    }

    private void HandleNetworkMovement()
    {
        //Check if client is owner. If it's not, it can't move the player
        if (!IsOwner && !isDebugScene) return;
        moveInput = moveAction.ReadValue<Vector2>();

        if (_isFlying && ((rb.velocity.x * moveInput.x < 0) || (Mathf.Abs(rb.velocity.x) <= 0.001f && Mathf.Abs(moveInput.x) > 0)))
        {
            moveSpeed = moveSpeedAir;
            justChangedD = true;
        }
        else if (justChangedD) moveSpeed = moveSpeedAir;
        else moveSpeed = moveSpeedGround;

        if (!_isFlying) justChangedD = false;
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
        if (mayJumpTime <= 0) return; // Check if Coyote Time still applies
        rb.velocity = Vector2.right * rb.velocity.x + Vector2.up * jumpForce;

    }

    public bool IsGrounded()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    protected void OnCollisionEnter2D(Collision2D other)
    {
        _isFlying = !IsGrounded();
        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    protected void OnCollisionExit2D(Collision2D other)
    {
        _isFlying = !IsGrounded();
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == Mathf.Log(pickaxeLayer, 2))
        {
            inputActions.PlayerControls.grabPickaxe.performed += OnGrabbingPickaxe;
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Mathf.Log(pickaxeLayer, 2))
        {
            inputActions.PlayerControls.grabPickaxe.performed -= OnGrabbingPickaxe;
        }
    }

    private void OnGrabbingPickaxe(InputAction.CallbackContext context)
    {
        if (!IsOwner && !isDebugScene) return;

        if (!IsServer)
        {
            RequestGrabPickaxeRpc();
        }
        else
        {
            GrabPickaxeOnServer();
        }

        inputActions.PlayerControls.grabPickaxe.performed -= OnGrabbingPickaxe;
        inputActions.PlayerControls.grabPickaxe.performed += OnReleasingPickaxe;
    }

    [Rpc(SendTo.Server)]
    private void RequestGrabPickaxeRpc()
    {
        GrabPickaxeOnServer();
    }

    private void GrabPickaxeOnServer()
    {
        FindObjectOfType<Pickaxe>().GetComponent<NetworkObject>().TrySetParent(transform);
    }

    private void OnReleasingPickaxe(InputAction.CallbackContext context)
    {

        if (!IsOwner && !isDebugScene) return;
        if (!IsServer)
        {
            RequestReleasePickaxeRpc();
        }
        else
        {
            ReleasePickaxeOnServer();
        }

        inputActions.PlayerControls.grabPickaxe.performed -= OnReleasingPickaxe;
    }

    [Rpc(SendTo.Server)]
    private void RequestReleasePickaxeRpc()
    {
        ReleasePickaxeOnServer();
    }

    private void ReleasePickaxeOnServer()
    {
        FindObjectOfType<Pickaxe>().GetComponent<NetworkObject>().TryRemoveParent();
    }
}
