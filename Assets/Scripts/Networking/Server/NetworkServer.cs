using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using static Unity.Netcode.NetworkManager;

public class NetworkServer : IDisposable
{
    public Action<string> OnClientLeft;

    private NetworkManager m_NetworkManager;
    private Dictionary<ulong, string> m_PlayerIdAuthId;
    private Dictionary<string, UserData> m_AuthIdUserData;
    public NetworkServer(NetworkManager networkManager)
    {
        this.m_NetworkManager = networkManager;

        this.m_NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
        this.m_NetworkManager.OnServerStarted += OnNetworkReady;
        m_PlayerIdAuthId = new Dictionary<ulong, string>();
        m_AuthIdUserData = new Dictionary<string, UserData>();
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        return m_NetworkManager.StartServer();
    }
    private void OnNetworkReady()
    {
        this.m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong obj)
    {
        if(m_PlayerIdAuthId.TryGetValue(obj, out string authId))
        {
            m_PlayerIdAuthId.Remove(obj);
            m_AuthIdUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    private void ApprovalCheck(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        m_PlayerIdAuthId[request.ClientNetworkId] = userData.userAuthId;
        m_AuthIdUserData[userData.userAuthId] = userData;

        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    public void Dispose()
    {
        if (this.m_NetworkManager != null)
        {
            this.m_NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            this.m_NetworkManager.OnServerStarted -= OnNetworkReady;
            this.m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;

            if (this.m_NetworkManager.IsListening)
            {
                this.m_NetworkManager.Shutdown();
            }
        }
    }

    public UserData GetUserData(ulong clientId)
    {
        if(m_PlayerIdAuthId.TryGetValue(clientId, out var authId))
        {
            if(m_AuthIdUserData.TryGetValue(authId, out var userData))
            {
                return userData;
            }
        }
        return null;
    }
}
