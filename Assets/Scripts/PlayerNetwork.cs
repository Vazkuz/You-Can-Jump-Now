using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    private PlayerInputActions inputActions;

    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;

    [Header("Player Movement")]
    [SerializeField] float jumpForce = 50f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    InputAction moveAction;
    InputAction jumpAction;
    private Vector2 moveInput;
    private new Rigidbody2D rb;

    private NetworkVariable<prueba> randomNumber = new NetworkVariable<prueba>(
        new prueba
        {
            _int = 1,
            _bool = true
        }
        , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct prueba : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        randomNumber.OnValueChanged += (prueba previousValue, prueba newValue) =>
        {
            print(OwnerClientId + ": " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

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
        if (!IsOwner) return;
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
        //rb.velocity = Vector2.up * jumpForce;
    }

    protected void OnEnable()
    {
        inputActions.Enable();
        moveAction = inputActions.PlayerControls.move;

        jumpAction = inputActions.PlayerControls.jump;

        inputActions.PlayerControls.jump.performed += OnJump;
        //inputActions.PlayerControls.jump. = () =>
        //{
        //    print("jumping");
        //}
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        rb.velocity = Vector2.up * jumpForce;
        //rigidbody2D.AddForce(jumpForce2D, ForceMode2D.Impulse);

    }

    protected void OnDisable()
    {
        inputActions.PlayerControls.jump.performed -= OnJump;
        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        print("TestServerRpc " + OwnerClientId);
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        print("TestClientRpc");
    }
}
