using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    //[SerializeField] private Button serverBtn;
    [SerializeField] private GameObject menuParent;
    [SerializeField] private GameObject joinMenuBox;
    [SerializeField] private TMP_Text joinCode;
    [SerializeField] private TMP_InputField joinInputField;
    [SerializeField] private TestRelay testRelay;
    private MenuInputActions menuActions;

    private string joinCodeStr;

    protected void Awake()
    {
        menuActions = new MenuInputActions();
        joinCode.text = string.Empty;
        DisableJoinIF();
        NetworkManager.OnClientConnectedCallback += ClientSideSetJoinCode;
    }

    protected void OnEnable()
    {
        menuActions.Enable();
        menuActions.MenuActions.toggleMenu.performed += ToggleMenuAction;
        menuParent.SetActive(false);
    }

    protected void OnDisable()
    {
        menuActions.Disable();
    }

    private void ToggleMenuAction(InputAction.CallbackContext context)
    {
        ToggleMenu(!menuParent.activeSelf);
    }

    private void ToggleMenu(bool newState)
    {
        menuParent.SetActive(newState);
        joinMenuBox.SetActive(false);
    }

    public void EnableJoinIF()
    {
        joinMenuBox.SetActive(true);
        //joinCode.gameObject.SetActive(false);
    }

    public void DisableJoinIF()
    {
        joinMenuBox.SetActive(false);
        joinCodeStr = string.Empty;
        joinInputField.text = string.Empty;
        //joinCode.gameObject.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    public void SetJoinCodeRpc(string joinCode)
    {
        joinCodeStr = joinCode;
        this.joinCode.text = "Code: " + joinCodeStr;
    }

    public void ClientSideSetJoinCode(ulong clientId)
    {
        if (IsServer)
        {
            SetJoinCodeRpc(joinCodeStr);
        }
    }

    public void CreateLobby()
    {
        testRelay.CreateRelay();
        ToggleMenu(false);
    }

    public void JoinLobby()
    {
        testRelay.JoinRelay(joinCodeStr);
        ToggleMenu(false);
    }

    public void UpdateJoinCode(string joinCodeStr)
    {
        this.joinCodeStr = joinCodeStr;
    }

    public void CopyLobbyCode()
    {
        TextEditor te = new TextEditor();
        te.text = joinCodeStr;
        te.SelectAll();
        te.Copy();
    }

}
