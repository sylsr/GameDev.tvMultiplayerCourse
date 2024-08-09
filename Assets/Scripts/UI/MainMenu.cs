using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private TMP_Text findMatchButtonText;
    [SerializeField] private TMP_Text timeInQueue;
    [SerializeField] private TMP_Text queueStatus;
    [SerializeField] private Toggle teamToggle;
    [SerializeField] private Toggle privateLobbyToggle;


    private bool isMatchmaking = false;
    private bool isCancelling = false;
    private float queueTimer = 0f;
    private bool isBusy = false;

    private void Start()
    {
        if (ClientSingleton.Instance == null)
        {
            return;
        }

        timeInQueue.text = string.Empty;
        queueStatus.text = string.Empty;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (!isMatchmaking || isCancelling)
        {
            timeInQueue.text = String.Empty;
            queueTimer = 0f;
            return;
        }
        queueTimer += Time.deltaTime;
        TimeSpan ts = TimeSpan.FromSeconds(queueTimer);
        timeInQueue.text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isBusy) { return; }
        isBusy = true;
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
            isBusy = false;
        }

    }

    public async void StartHost()
    {
        if (isBusy)
        {
            return;
        }
        isBusy = true;
        await HostSingleton.Instance.GameManager.StartHostAsync(privateLobbyToggle.isOn);
        isBusy = false;
    }

    public async void StartClient()
    {
        if (isBusy)
        {
            return;
        }
        isBusy = true;

        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        isBusy = false;
    }

    public async void FindMatch()
    {
        if (isCancelling)
        {
            return;
        }

        if (isMatchmaking)
        {
            queueStatus.text = "Canceling...";
            isCancelling = true;

            await ClientSingleton.Instance.GameManager.CancelMatchmaking();
            isCancelling = false;
            isMatchmaking = false;
            isBusy = false;
            findMatchButtonText.text = "Find Match";
            queueStatus.text = string.Empty;
            return;
        }

        if (isBusy)
        {
            return;
        }

        ClientSingleton.Instance.GameManager.MatchmakeAsync(teamToggle.isOn, OnMatchMade);
        findMatchButtonText.text = "Cancel Matchmaking";
        queueStatus.text = "Searching...";
        isMatchmaking = true;
        isBusy = true;
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatus.text = "Connecting...";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                queueStatus.text = "TicketCreationError...";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                queueStatus.text = "TicketCancellationError...";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                queueStatus.text = "TicketRetrievalError...";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                queueStatus.text = "MatchAssignmentError...";
                break;

        }
    }
}
