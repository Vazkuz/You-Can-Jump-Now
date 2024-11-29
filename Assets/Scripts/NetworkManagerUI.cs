using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    //[SerializeField] private Button serverBtn;
    [SerializeField] private GameObject joinMenu;
    [SerializeField] private TMP_Text joinCode;
    [SerializeField] private TMP_InputField joinInputField;
    [SerializeField] private TestRelay testRelay;

    private string joinCodeStr;

    protected void Awake()
    {
        //serverBtn.onClick.AddListener(() =>
        //{
        //    NetworkManager.Singleton.StartServer();
        //});
        //hostBtn.onClick.AddListener(() =>
        //{
        //    NetworkManager.Singleton.StartHost();
        //});

        joinCode.text = string.Empty;
        DisableJoinIF();
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

    public void SetJoinCode(string joinCode)
    {
        joinCodeStr = joinCode;
        this.joinCode.text = "Code: " + joinCodeStr;
    }

    public void JoinLobby()
    {
        testRelay.JoinRelay(joinCodeStr);
    }

    public void UpdateJoinCode(string joinCodeStr)
    {
        this.joinCodeStr = joinCodeStr;
    }
}
