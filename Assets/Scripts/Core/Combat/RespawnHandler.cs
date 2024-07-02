using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float percentCoinKeepOnDeath = 0.5f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);

        foreach (TankPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerSpawned += HandlePlayerDespawned;
    }



    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerSpawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player)
    {
        int coinsToKeep = Mathf.CeilToInt(player.Wallet.TotalCoins.Value * percentCoinKeepOnDeath);
        int coinsToDrop = player.Wallet.TotalCoins.Value - coinsToKeep;
        Destroy(player.gameObject);
        StartCoroutine(RespawnPlayer(player.OwnerClientId, coinsToKeep));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int spawnCoins)
    {
        yield return null;

        TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        playerInstance.Wallet.SetCoins(spawnCoins);
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.health.OnDie += (health) => HandlePlayerDie(player);
    }
}
