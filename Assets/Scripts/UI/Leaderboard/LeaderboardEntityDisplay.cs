using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor = Color.green;
    
    public ulong ClientId { get; private set; }
    private FixedString32Bytes playerName;
    public int Coins { get; private set; }

    public void Initialize(ulong clientId, FixedString32Bytes playerName, int coins)
    {
        this.ClientId = clientId;
        this.playerName = playerName;
        this.Coins = coins;

        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor;
        }

        UpdateDisplayText();
    }

    public void UpdateCoins(int coins)
    {
        this.Coins = coins;
        UpdateDisplayText();
    }

    public void UpdateDisplayText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {playerName} ({Coins})";
    }
}
