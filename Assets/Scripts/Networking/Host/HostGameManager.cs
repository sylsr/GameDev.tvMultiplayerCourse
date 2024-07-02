using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private Lobby lobby;

    public string joinCode { get; private set; }

    private const int MaxConnections = 20;
    private const string SceneName = "GameplayScene1";
    private const int WaitTimeSeconds = 15;
    public NetworkServer NetworkServer { get; private set; }

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>()
            {
                {"joinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, options);
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(WaitTimeSeconds));

            NetworkServer = new NetworkServer(NetworkManager.Singleton);

            UserData userData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };
            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            return;
        }
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobby.Id);
            yield return delay;
        }
    }

    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
        if (lobby != null && !string.IsNullOrEmpty(lobby.Id))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
            lobby = null;
        }
        NetworkServer?.Dispose();
    }
}
