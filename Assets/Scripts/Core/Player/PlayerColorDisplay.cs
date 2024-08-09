using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerColorDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] playerColorSprites;
    [SerializeField] private TeamColorLookup teamColorLookup;
    private void Start()
    {
        HandlePlayerColorChanged(-1, player.TeamNumber.Value);
        player.TeamNumber.OnValueChanged += HandlePlayerColorChanged;
    }

    private void HandlePlayerColorChanged(int previousValue, int newValue)
    {
        Debug.Log($"Team number is: {newValue} for player {player.PlayerName.Value}");
        Color setColor = teamColorLookup.GetTeamColor(newValue);
        Debug.Log($"Setting color to {setColor}");
        foreach (SpriteRenderer r in playerColorSprites)
        {
            r.color = setColor;
        }
    }

    private void OnDestroy()
    {
        player.TeamNumber.OnValueChanged -= HandlePlayerColorChanged;
    }
}
