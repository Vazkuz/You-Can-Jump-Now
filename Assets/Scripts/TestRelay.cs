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
    [SerializeField] Button joinBtn;
    [SerializeField] Button createBtn;
    [SerializeField] Button leaveBtn;

    protected async void Start()
    {
        joinBtn.gameObject.SetActive(false);
        createBtn.gameObject.SetActive(false);
        leaveBtn.gameObject.SetActive(false);
        await UnityServices.InitializeAsync(); //Cualquier codigo luego de esto esperara a que esto pase para ejecutarse

        AuthenticationService.Instance.SignedIn += () =>
        {
            print("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        joinBtn.gameObject.SetActive(true);
        createBtn.gameObject.SetActive(true);
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

            NetworkManagerUI.SetJoinCode(joinCode);

            joinBtn.gameObject.SetActive(false);
            createBtn.gameObject.SetActive(false);
            leaveBtn.gameObject.SetActive(true);
        }
        catch(RelayServiceException e)
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

            joinBtn.gameObject.SetActive(false);
            createBtn.gameObject.SetActive(false);
            leaveBtn.gameObject.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }

    public async void LeaveRelay()
    {
        NetworkManager.Singleton.Shutdown();

        joinBtn.gameObject.SetActive(true);
        createBtn.gameObject.SetActive(true);
        leaveBtn.gameObject.SetActive(false);
    }

}