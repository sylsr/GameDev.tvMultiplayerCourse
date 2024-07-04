using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int serverQPort;

    private NetworkServer networkServer;
    private MultiplayAllocationService multiplayAllocationService;

    private const string SceneName = "GameplayScene1";


    public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = serverQPort;
        networkServer = new NetworkServer(manager);
        multiplayAllocationService = new MultiplayAllocationService();
    }

    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }

    internal async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();
        if(!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("Network server did not start properly...");
        }
        NetworkManager.Singleton.SceneManager.LoadScene(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
