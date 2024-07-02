using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager m_NetworkManager;
    private const string MenuSceneName = "Menu";
    public NetworkClient(NetworkManager networkManager)
    {
        this.m_NetworkManager = networkManager;

        this.m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if(clientId != 0 && clientId != m_NetworkManager.LocalClientId)
        {
            return;
        }

        if(SceneManager.GetActiveScene().name != "Menu")
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        if (m_NetworkManager.IsConnectedClient)
        {
            m_NetworkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if(m_NetworkManager != null)
        {
            this.m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
