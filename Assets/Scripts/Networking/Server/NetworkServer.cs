using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Netcode.NetworkManager;

public class NetworkServer : IDisposable
{

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
