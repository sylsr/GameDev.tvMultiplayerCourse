using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private TMP_Text lobbyCapacity;

    private LobbiesList lobbiesList;
    private Lobby lobby;

    public void Initialize(LobbiesList lobbiesList, Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        lobbyCapacity.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        this.lobbiesList = lobbiesList;
        this.lobby = lobby;
    }

    public void Join()
    {
        lobbiesList.JoinAsync(lobby);
    }

}
