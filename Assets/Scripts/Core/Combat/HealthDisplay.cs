using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [SerializeField]
    private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }
        health.currentHealth.OnValueChanged += HandleHeathChanged;
        HandleHeathChanged(0, health.currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }
        health.currentHealth.OnValueChanged -= HandleHeathChanged;
    }

    private void HandleHeathChanged(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth /(float)health.maxHealth;
    }
}
