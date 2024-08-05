using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int serverQPort;

    public NetworkServer NetworkServer { get; private set; }
    private MultiplayAllocationService multiplayAllocationService;

    private const string SceneName = "GameplayScene1";
    private MatchplayBackfiller backfiller;


    public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = serverQPort;
        NetworkServer = new NetworkServer(manager);
        multiplayAllocationService = new MultiplayAllocationService();
    }

    public void Dispose()
    {
        NetworkServer.onUserJoined -= UserJoined;
        NetworkServer.onUserLeft -= UserLeft;
        multiplayAllocationService?.Dispose();
        NetworkServer?.Dispose();
        backfiller?.Dispose();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if(matchmakerPayload != null)
            {
                await StartBackfill(matchmakerPayload);
                NetworkServer.onUserJoined += UserJoined;
                NetworkServer.onUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload timed out.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        if (!NetworkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("Network server did not start properly...");
        }
        NetworkManager.Singleton.SceneManager.LoadScene(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();
        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            Debug.Log("Matchmaker allocation successful.");
            return matchmakerPayloadTask.Result;
        }

        Debug.LogWarning("Matchmaker payload timed out.");
        return null;
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        Debug.Log($"Starting backfill with payload: {JsonConvert.SerializeObject(payload)}");
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 20);
        if (backfiller.NeedsPlayers())
        {
            Debug.Log("Backfill needed, beginning backfill process.");
            await backfiller.BeginBackfilling();
        }
    }

    private void UserJoined(UserData user)
    {
        Debug.Log($"User joined: {user.userName}");
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();
        if(!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            Debug.Log($"Stopping backfilling as lobby is full");
            _ = backfiller.StopBackfill();
        }
    }

    private void UserLeft(UserData user)
    {
        Debug.Log($"User left: {user.userName}");
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();
        if(playerCount <= 0)
        {
            Debug.Log($"No players remain, closing server.");
            CloseServer();
            return;
        }

        if (backfiller.NeedsPlayers() && ! backfiller.IsBackfilling)
        {
            Debug.Log($"More players are now needed, re-starting backfilling.");
            _ = backfiller.BeginBackfilling();
        }
    }

    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }
}
