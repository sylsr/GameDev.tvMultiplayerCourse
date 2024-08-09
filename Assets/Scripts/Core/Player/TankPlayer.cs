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
    [SerializeField] private SpriteRenderer minimapIcon;
    [SerializeField] private Color minimapSelfColor = Color.blue;
    
    [SerializeField] private Texture2D crosshair;


    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public NetworkVariable<int> TeamNumber = new NetworkVariable<int>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = null;
            if (IsHost)
            {
                 userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            
            PlayerName.Value = userData.userName;
            TeamNumber.Value = userData.teamNumber;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            vCamera.Priority = cameraPriority;
            minimapIcon.color = minimapSelfColor;
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
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
