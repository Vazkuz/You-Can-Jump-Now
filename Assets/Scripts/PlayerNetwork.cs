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

    private Vector2 moveInput;

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

    protected void Update()
    {
        //Check if client is owner. If it's not, it can't move the player
        if (!IsOwner) return;
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;
        //test
        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            //TestClientRpc();
            //TestServerRpc();
            //randomNumber.Value = new prueba
            //{
            //    _int = 10,
            //    _bool = false,
            //    message = "Gaaaaa"
            //};
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }
    }

    protected void OnEnable()
    {
        inputActions.Enable();
        inputActions.PlayerControls.move.performed += OnMove;
        inputActions.PlayerControls.move.canceled += OnMove;
    }

    protected void OnDisable()
    {
        inputActions.PlayerControls.move.performed -= OnMove;
        inputActions.PlayerControls.move.canceled -= OnMove;
        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
        print(OwnerClientId);
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
