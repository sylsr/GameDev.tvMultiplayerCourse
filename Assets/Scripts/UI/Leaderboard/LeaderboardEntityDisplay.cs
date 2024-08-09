using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    
    public int TeamNum { get; private set; }
    public ulong ClientId { get; private set; }
    private FixedString32Bytes displayName;
    public int Coins { get; private set; }

    public void Initialize(ulong clientId, FixedString32Bytes displayName, int coins)
    {
        this.ClientId = clientId;
        this.displayName = displayName;
        this.Coins = coins;

        UpdateDisplayText();
    }

    public void Initialize(int teamNum, FixedString32Bytes displayName, int coins)
    {
        this.TeamNum = teamNum;
        this.displayName = displayName;
        this.Coins = coins;

        UpdateDisplayText();
    }

    public void SetColor(Color color)
    {
        displayText.color = color;
    }

    public void UpdateCoins(int coins)
    {
        this.Coins = coins;
        UpdateDisplayText();
    }

    public void UpdateDisplayText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {displayName} ({Coins})";
    }
}
