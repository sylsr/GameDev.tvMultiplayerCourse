using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BountyCoin bountyCoinPrefab;
    [SerializeField] private Health health;

    [Header("Settings")]
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 2;
    [SerializeField] private float coinSpread;
    [SerializeField] private float bountyPercentage = 0.5f;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }
        health.OnDie -= HandleDie;
    }

    private void HandleDie(Health obj)
    {
        int bountyValue = Mathf.CeilToInt(TotalCoins.Value * bountyPercentage);
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if(bountyCoinValue < minBountyCoinValue)
        {
            return;
        }

        for (int i = 0; i< bountyCoinCount; i++)
        {
            BountyCoin instance = Instantiate(bountyCoinPrefab, GetSpawnPoint(), Quaternion.identity);
            instance.SetValue(bountyCoinValue);
            instance.NetworkObject.Spawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Coin>(out Coin coin))
        {
            int coinValue = coin.Collect();
            if (!IsServer)
            {
                return;
            }
            TotalCoins.Value += coinValue;
        }
    }

    public void SpendCoins(int numToSpend)
    {
        TotalCoins.Value -= numToSpend;
    }

    public void SetCoins(int coins)
    {
        TotalCoins.Value = coins;
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2) this.transform.position + Random.insideUnitCircle * coinSpread;
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

            if (numColliders == 0)
            {
                return spawnPoint;
            }

        }
    }
}
