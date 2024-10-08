using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int maxPlayerDisplay = 8;
    [SerializeField] private Transform teamLeaderboardEntityHolder;
    [SerializeField] private GameObject teamLeaderboardBackground;
    [SerializeField] private Color ownerColor;
    [SerializeField] private string[] teamNames;
    [SerializeField] private TeamColorLookup teamColorLookup;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private List<LeaderboardEntityDisplay> entitiesDisplay = new List<LeaderboardEntityDisplay>();
    private List<LeaderboardEntityDisplay> teamEntityDisplays = new List<LeaderboardEntityDisplay>();

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            if (ClientSingleton.Instance.GameManager.UserData.userGamePreferences.gameQueue == GameQueue.Team)
            {
                teamLeaderboardBackground.SetActive(true);
                for (int i = 0; i < teamNames.Length; i++)
                {
                    LeaderboardEntityDisplay teamLeaderboardEntity = Instantiate(leaderboardEntityPrefab, teamLeaderboardEntityHolder);
                    teamLeaderboardEntity.Initialize(i, teamNames[i], 0);

                    Color teamColor = teamColorLookup.GetTeamColor(i);
                    teamLeaderboardEntity.SetColor(teamColor);
                    teamEntityDisplays.Add(teamLeaderboardEntity);
                }
            }

            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanges;
            foreach (LeaderboardEntityState state in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanges(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = state,
                });
            }
        }

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
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }

        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanges;
        }

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandleLeaderboardEntitiesChanges(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        if (!gameObject.scene.isLoaded)
        {
            return;
        }
        //Update on value changes, player joins, player leaves.
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (!entitiesDisplay.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardEntityDisplay instance = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    instance.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                    if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId)
                    {
                        instance.SetColor(ownerColor);
                    }
                    entitiesDisplay.Add(instance);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove = entitiesDisplay.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    entitiesDisplay.Remove(displayToRemove);
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);

                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate = entitiesDisplay.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;

        }

        //Sort list by who has most score, only show top players.
        entitiesDisplay.Sort((a, b) => b.Coins.CompareTo(a.Coins));
        for (int i = 0; i < entitiesDisplay.Count; i++)
        {
            entitiesDisplay[i].transform.SetSiblingIndex(i);
            entitiesDisplay[i].UpdateDisplayText();
            entitiesDisplay[i].gameObject.SetActive(i <= maxPlayerDisplay - 1);
        }

        //If you are not in top player we remove bottom player from leaderboard and put you on it so you can always see yourself.
        LeaderboardEntityDisplay playerDisplay = entitiesDisplay.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);
        if (playerDisplay != null && playerDisplay.transform.GetSiblingIndex() >= maxPlayerDisplay)
        {
            leaderboardEntityHolder.GetChild(maxPlayerDisplay - 1).gameObject.SetActive(false);
            playerDisplay.gameObject.SetActive(true);
        }

        if (!teamLeaderboardBackground.activeSelf)
        {
            return;
        }

        LeaderboardEntityDisplay teamDisplay = teamEntityDisplays.FirstOrDefault(x => x.TeamNum == changeEvent.Value.TeamIndex);

        if (teamDisplay != null)
        {
            if (changeEvent.Type == NetworkListEvent<LeaderboardEntityState>.EventType.Remove)
            {
                teamDisplay.UpdateCoins(teamDisplay.Coins - changeEvent.Value.Coins);
            }
            else
            {
                teamDisplay.UpdateCoins(teamDisplay.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
            }

            teamEntityDisplays.Sort((a, b) => b.Coins.CompareTo(a.Coins));

            for (int i = 0; i < teamEntityDisplays.Count; i++)
            {
                teamEntityDisplays[i].transform.SetSiblingIndex(i);
                teamEntityDisplays[i].UpdateDisplayText();
            }
        }
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            TeamIndex = player.TeamNumber.Value,
            Coins = 0,
        });

        player.Wallet.TotalCoins.OnValueChanged += (int old, int newVal) => HandleCoinsChanged(player.OwnerClientId, newVal);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        if (leaderboardEntities == null)
        {
            return;
        }
        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if (entity.ClientId != player.OwnerClientId)
            {
                continue;
            }
            else
            {
                leaderboardEntities.Remove(entity);
                break;
            }
        }

        player.Wallet.TotalCoins.OnValueChanged -= (int old, int newVal) => HandleCoinsChanged(player.OwnerClientId, newVal);

    }

    private void HandleCoinsChanged(ulong clientId, int newValue)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId != clientId)
            {
                continue;
            }

            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                TeamIndex = leaderboardEntities[i].TeamIndex,
                Coins = newValue,
            };
            return;
        }
    }
}
