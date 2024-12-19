using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestRelay : MonoBehaviour
{
    [SerializeField] NetworkManagerUI NetworkManagerUI;
    [SerializeField] GameObject lobbyOptions;
    [SerializeField] Button leaveBtn;
    [SerializeField] Button copyLobbyBtn;

    protected async void Start()
    {
        lobbyOptions.SetActive(false);
        leaveBtn.gameObject.SetActive(false);
        copyLobbyBtn.gameObject.SetActive(false);
        await UnityServices.InitializeAsync(); //Cualquier codigo luego de esto esperara a que esto pase para ejecutarse

        AuthenticationService.Instance.SignedIn += () =>
        {
            print("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        lobbyOptions.SetActive(true);
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            NetworkManagerUI.SetJoinCodeRpc(joinCode);

            lobbyOptions.SetActive(false);
            leaveBtn.gameObject.SetActive(true);
            copyLobbyBtn.gameObject.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            print("Joining Relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            print(joinAllocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            lobbyOptions.SetActive(false);
            leaveBtn.gameObject.SetActive(true);
            copyLobbyBtn.gameObject.SetActive(true); //AQUI HACER UN ASYNC O ALGO XD
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }

    public void LeaveRelay()
    {
        NetworkManager.Singleton.Shutdown();

        lobbyOptions.SetActive(true);
        leaveBtn.gameObject.SetActive(false);
        copyLobbyBtn.gameObject.SetActive(false);
    }
}
