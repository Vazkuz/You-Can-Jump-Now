using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    //[SerializeField] private Button serverBtn;
    [SerializeField] private GameObject joinMenu;
    [SerializeField] private TMP_Text joinCode;
    [SerializeField] private TMP_InputField joinInputField;
    [SerializeField] private TestRelay testRelay;

    private string joinCodeStr;

    protected void Awake()
    {
        joinCode.text = string.Empty;
        DisableJoinIF();
        NetworkManager.OnClientConnectedCallback += ClientSideSetJoinCode;
    }

    public void EnableJoinIF()
    {
        joinMenu.SetActive(true);
    }

    public void DisableJoinIF()
    {
        joinMenu.SetActive(false);
        joinCodeStr = string.Empty;
        joinInputField.text = string.Empty;
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

    

    public void JoinLobby()
    {
        testRelay.JoinRelay(joinCodeStr);
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
