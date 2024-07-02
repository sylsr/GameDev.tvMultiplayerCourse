using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject serverProjectilePrefab;

    [SerializeField]
    private GameObject clientProjectilePrefab;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private GameObject muzzleFlash;

    [SerializeField]
    private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField]
    private float projectileSpeed = 2f;

    [SerializeField]
    private float fireRate = 1f;

    [SerializeField]
    private float muzzleFlashDuration = 0.25f;

    [SerializeField]
    private int costToFire = 1;

    private bool shouldFire = false;

    private float previousFireTime;

    private float muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    void Update()
    {
        if(muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;
        }
        else
        {
            muzzleFlash.SetActive(false);
        }
        if ((!IsOwner || !shouldFire))
        {
            return;
        }

        if(Time.time - previousFireTime < fireRate)
        {
            return;
        }

        if (!EnoughCoinsToFire())
        {
            return; 
        }
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        previousFireTime = Time.time;
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        SpawnProjectile(clientProjectilePrefab, spawnPos, direction);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (this.TryGetComponent<CoinWallet>(out CoinWallet coinWallet))
        {
            coinWallet.SpendCoins(costToFire);
        }
        SpawnProjectile(serverProjectilePrefab, spawnPos, direction);
        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner)
        {
            return;
        }
        SpawnDummyProjectile(spawnPos, direction);
    }

    private bool EnoughCoinsToFire()
    {
        if(this.TryGetComponent<CoinWallet>(out CoinWallet coinWallet))
        {
            return coinWallet.TotalCoins.Value >= costToFire; 
        }
        return false;
    }

    private void SpawnProjectile(GameObject projectilePrefab, Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        
        if(projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up.normalized * projectileSpeed;
        }
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }
}
