using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;
    private const string MenuSceneName = "Menu";
    private const string SceneName = "GameplayScene1";
    private NetworkClient networkClient;
    private MatchplayMatchmaker matchmaker;

    private UserData userData;
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        networkClient = new NetworkClient(NetworkManager.Singleton);
        matchmaker = new MatchplayMatchmaker();
        AuthState authState = await AuthenticationWrapper.DoAuth();

        userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };

        return authState == AuthState.Authenticated;
    }

    internal void GoToMainMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            ConnectClient();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

    }

    private void StartClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
    }

    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (matchmaker.IsMatchmaking)
        {
            Debug.LogWarning("Already matchmaking.");
            return;
        }

        MatchmakerPollingResult result = await GetMatchAsync();
        Debug.Log($"Matchmaking result: {result}");
        onMatchmakeResponse?.Invoke(result);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult result = await matchmaker.Matchmake(userData);
        if (result.result == MatchmakerPollingResult.Success)
        {
            Debug.Log($"Match found. Server IP: {result.ip}, Port: {result.port}");
            StartClient(result.ip, result.port);
        }
        else
        {
            Debug.LogWarning($"Matchmaking failed: {result.result}");
        }

        return result.result;
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }

    internal void Disconnect()
    {
        networkClient.Disconnect();
    }

    public async Task CancelMatchmaking()
    {
        await matchmaker.CancelMatchmaking();
    }
}
