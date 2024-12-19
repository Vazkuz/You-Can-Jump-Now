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

    [Header("Character Appearance")]
    [SerializeField] private GameObject characterBody;
    public Transform Hand { get { return hand; } private set { hand = value; } }
    [SerializeField] private Transform hand;

    [Header("Character Movement Variables")]
    [SerializeField] private float moveSpeedGround = 3f;
    [SerializeField] private float moveSpeedAir = 3f;
    private float moveSpeed;
    private PlayerInputActions inputActions;
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.1f;
    public NetworkVariable<bool> canJump = new NetworkVariable<bool>(false, default, NetworkVariableWritePermission.Owner);

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
    public NetworkVariable<bool> hasPickaxe { get; private set; } = new NetworkVariable<bool>(false, default, NetworkVariableWritePermission.Owner);

    [Header("Mineral Variables")]
    [SerializeField] private LayerMask breakableLayer;
    [SerializeField] private bool canMine = false;
    public static event Action<ulong> OnMining;

    [Header("Exit Vars")]
    [SerializeField] private LayerMask exitLayer;
    [SerializeField] private bool insideDoorFrame = false;
    public static event Action<ulong> OnExit;

    [Header("Lobby Vars")]
    [SerializeField] SpriteRenderer hostSign;


    public static event Action<ulong> OnPlayerPrefabSpawn;

    protected void Awake()
    {
        inputActions = new PlayerInputActions();
        hasPickaxe.Value = false;
        canJump.Value = false;
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _isFlying = !IsGrounded();
        mayJumpTime = 0f;
        moveSpeed = moveSpeedGround;
        //if(!IsServer) hostSign.enabled = false; // MAS ADELANTE AÑADIR ESTO, JUNTO CON UN LOBBY MANAGER.
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnPlayerPrefabSpawn?.Invoke(OwnerClientId);
    }
    protected void OnEnable()
    {
        EnableMovement();
        Mineral.OnFinishedMine += OnFinishedMine;
    }

    protected void OnDisable()
    {
        // Unsubscribe to all events to prevent Memory Leaks.
        inputActions.PlayerControls.grabPickaxe.performed -= OnGrabbingPickaxe;
        inputActions.PlayerControls.enterDoor.performed -= OnPlayerGoThroughDoor;
        inputActions.PlayerControls.grabPickaxe.performed -= OnReleasingPickaxe;
        inputActions.PlayerControls.mine.performed -= OnTryingToMine;
        Mineral.OnFinishedMine -= OnFinishedMine;
        DisableMovement();
    }

    private void EnableMovement()
    {
        inputActions.Enable();
        moveAction = inputActions.PlayerControls.move;
        jumpAction = inputActions.PlayerControls.jump;
        inputActions.PlayerControls.jump.performed += OnJump;
    }

    private void DisableMovement()
    {
        inputActions.PlayerControls.jump.performed -= OnJump;
        inputActions.Disable();
    }

    protected void Update()
    {
        if (!_isFlying) mayJumpTime = coyoteTime;
        mayJumpTime -= Time.deltaTime;
        HandleNetworkMovement();

    }

    /// <summary>
    /// Drawing the cube that is used to check if player is grounded.
    /// </summary>
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
        if (collision.gameObject.layer == Mathf.Log(pickaxeLayer, 2))
        {
            inputActions.PlayerControls.grabPickaxe.performed += OnGrabbingPickaxe;
        }

        if (collision.gameObject.layer == Mathf.Log(breakableLayer, 2))
        {
            canMine = true;
        }

        if(collision.gameObject.layer == Mathf.Log(exitLayer, 2))
        {
            inputActions.PlayerControls.enterDoor.performed += OnPlayerGoThroughDoor;
            insideDoorFrame = true;
        }

    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Mathf.Log(pickaxeLayer, 2))
        {
            inputActions.PlayerControls.grabPickaxe.performed -= OnGrabbingPickaxe;
        }

        if (collision.gameObject.layer == Mathf.Log(breakableLayer, 2))
        {
            canMine = false;
        }

        if (collision.gameObject.layer == Mathf.Log(exitLayer, 2))
        {
            inputActions.PlayerControls.enterDoor.performed -= OnPlayerGoThroughDoor;
            insideDoorFrame = false;
        }
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

    public bool IsGrounded()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer);
    }

    /// <summary>
    /// Method called when jumping. Subscribed initially on OnEnable
    /// </summary>
    /// <param name="context"></param>
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!canJump.Value) return;
        if (!IsOwner && !isDebugScene) return;
        if (mayJumpTime <= 0) return; // Check if we are still under coyote time, if not, we can't jump.
        if (insideDoorFrame) return; // El jugador no puede saltar cuando esta dentro de la zona de salida.

        canJump.Value = false;
        rb.velocity = Vector2.right * rb.velocity.x + Vector2.up * jumpForce;

    }

    private void HandleJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !jumpAction.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    /// <summary>
    /// Method to grab the pickaxe. Subscribed only OnTriggerEnter the pickaxe.
    /// </summary>
    /// <param name="context"></param>
    private void OnGrabbingPickaxe(InputAction.CallbackContext context)
    {
        if (!IsOwner && !isDebugScene) return;

        canJump.Value = false;

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
        inputActions.PlayerControls.mine.performed += OnTryingToMine;

        hasPickaxe.Value = true;
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

    /// <summary>
    /// Method to release the pickaxe. Subscribed when the player has grabbed the pickaxe.
    /// </summary>
    /// <param name="context"></param>
    private void OnReleasingPickaxe(InputAction.CallbackContext context)
    {

        if (!IsOwner && !isDebugScene) return;
        HandleReleasePickaxe();

        hasPickaxe.Value = false;
    }

    /// <summary>
    ///  OJO CON ESTA FUNCION: Actualmente ejectua para todos, ya que usa ReleasePickaxeOnServer. Chequear esta funcion para mas detalles.
    /// </summary>
    private void HandleReleasePickaxe()
    {
        if (!IsServer)
        {
            RequestReleasePickaxeRpc();
        }
        else
        {
            ReleasePickaxeOnServer();
        }

        inputActions.PlayerControls.grabPickaxe.performed -= OnReleasingPickaxe;
        inputActions.PlayerControls.mine.performed -= OnTryingToMine;
    }

    [Rpc(SendTo.Server)]
    private void RequestReleasePickaxeRpc()
    {
        ReleasePickaxeOnServer();
    }

    /// <summary>
    /// OJO CON ESTA FUNCION: Actualmente intenta remover el padre de Pickaxe, sin importar si el jugador que llama la funcion es o no su padre.
    /// Podria ser necesario hacer una verificacion de esto antes de hacer TryRemoveParent. Por ahora no es necesario, pero revisitar esto.
    /// </summary>
    private void ReleasePickaxeOnServer()
    {
        FindObjectOfType<Pickaxe>().GetComponent<NetworkObject>().TryRemoveParent();
    }

    /// <summary>
    /// Method to mine using the pickaxe. Subscribed when the player grabs the pickaxe.
    /// </summary>
    /// <param name="context"></param>
    private void OnTryingToMine(InputAction.CallbackContext context)
    {
        if (canMine)
        {
            if (IsServer)
            {
                MineOnServer(OwnerClientId);
            }
            else
            {
                RequestMineRpc(OwnerClientId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestMineRpc(ulong clientId)
    {
        MineOnServer(clientId);
    }

    private void MineOnServer(ulong clientId)
    {
        OnMining?.Invoke(clientId);
    }

    /// <summary>
    /// Method called when finish mining a mineral. Initially subscribed OnEnable.
    /// </summary>
    /// <param name="lastMinerId"></param>
    private void OnFinishedMine(ulong lastMinerId)
    {
        //Check that is the owner who is trying to run this.
        if (!IsOwner && !isDebugScene) return;

        //Check if the player was the last to mine the mineral. If that's the case, they can't be granted jump.
        if (OwnerClientId == lastMinerId) return;

        canJump.Value = true;
    }

    /// <summary>
    /// Method to execute passing through the exit door. Subscribed OnTriggerEnter the exit.
    /// </summary>
    /// <param name="context"></param>
    private void OnPlayerGoThroughDoor(InputAction.CallbackContext context)
    {
        if(!IsOwner && !isDebugScene) return;

        if (hasPickaxe.Value)
        {
            HandleReleasePickaxe();
            hasPickaxe.Value = false;
        }
        HideRpc();
        OnExit?.Invoke(OwnerClientId);
    }

    [Rpc(SendTo.Everyone)]
    private void HideRpc()
    {
        characterBody.SetActive(false);
        DisableMovement();
    }

    public void SetUpPlayer(Vector3 newPos)
    {
        SetUpPlayerRpc(newPos);
    }

    [Rpc(SendTo.Everyone)]
    private void SetUpPlayerRpc(Vector3 newPos)
    {
        transform.position = newPos;
        characterBody.SetActive(true);
        EnableMovement();
    }

}
