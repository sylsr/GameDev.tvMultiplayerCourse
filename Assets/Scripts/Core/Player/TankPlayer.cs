using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vCamera;
    [field: SerializeField] public Health health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }
    [SerializeField] private int cameraPriority = 11;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserData(OwnerClientId);
            PlayerName.Value = userData.userName;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            vCamera.Priority = cameraPriority;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
