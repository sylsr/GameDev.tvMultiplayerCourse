using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image healPowerBar;
    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 1;
    [SerializeField] private int healthPerTick = 25;

    private List<TankPlayer> playersInZone = new List<TankPlayer>();
    private float remainingCoolDown;
    private float tickTimer;

    private NetworkVariable<int> healPower = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            healPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, healPower.Value);
        }

        if (IsServer)
        {
            healPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            healPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer && collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            playersInZone.Add(player);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsServer && collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            playersInZone.Remove(player);
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if(remainingCoolDown > 0)
        {
            remainingCoolDown -= Time.deltaTime;
        }

        if(remainingCoolDown <= 0)
        {
            healPower.Value = maxHealPower;
        }
        else
        {
            return;
        }

        tickTimer += Time.deltaTime;
        if(tickTimer >= 1/healTickRate)
        {
            foreach(TankPlayer player in playersInZone)
            {
                if(healPower.Value <= 0)
                {
                    break;
                }
                if(player.health.currentHealth.Value == player.health.maxHealth)
                {
                    continue;
                }
                if(player.Wallet.TotalCoins.Value < coinsPerTick)
                {
                    continue;
                }
                player.Wallet.SpendCoins(coinsPerTick);
                player.health.RestoreHealth(healthPerTick);
                healPower.Value -= 1;
                if(healPower.Value <= 0)
                {
                    remainingCoolDown = healCooldown;
                }
            }
            tickTimer = tickTimer % (1 / healTickRate);
        }
    }

    private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
    {
        healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
    }
}
