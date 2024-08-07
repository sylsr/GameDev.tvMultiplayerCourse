using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private TMP_Text findMatchButtonText;
    [SerializeField] private TMP_Text timeInQueue;
    [SerializeField] private TMP_Text queueStatus;

    private bool isMatchmaking = false;
    private bool isCancelling = false;


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

    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
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
            findMatchButtonText.text = "Find Match";
            queueStatus.text = string.Empty;
            return;
        }

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
        findMatchButtonText.text = "Cancel Matchmaking";
        queueStatus.text = "Searching...";
        isMatchmaking = true;
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
