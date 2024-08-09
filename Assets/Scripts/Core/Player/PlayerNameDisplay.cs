using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer player;
    [SerializeField] private TMP_Text displayNameText;
    
    private void Start()
    {
        HandlePlayerNameChanged(String.Empty, player.PlayerName.Value);
        player.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        displayNameText.text = newValue.ToString();
    }

    private void OnDestroy()
    {
        player.PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }
}
