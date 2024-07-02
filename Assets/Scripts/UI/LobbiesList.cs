using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;

    private bool isJoining = false;

    private bool isRefreshing = false;

    private void OnEnable()
    {
        RefreshLobbyList();
    }

    public async void RefreshLobbyList()
    {
        if (isRefreshing) { return; }
        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0", QueryFilter.OpOptions.GT ),
                new QueryFilter(QueryFilter.FieldOptions.IsLocked,"0", QueryFilter.OpOptions.EQ)
            };
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach(Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }
            foreach(Lobby lobby in lobbies.Results)
            {
                LobbyItem newLobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                newLobbyItem.Initialize(this, lobby);


            }
        }catch(LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            isRefreshing = false;
        }

        
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) { return; }
        isJoining = true;
        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["joinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);

            return;
        }
        finally
        {
            isJoining = false;
        }

    }
}
