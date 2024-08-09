using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Coins;
    public int TeamIndex;

    public bool Equals(LeaderboardEntityState other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && Coins == other.Coins && TeamIndex.Equals(other.TeamIndex);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Coins);
        serializer.SerializeValue(ref TeamIndex);
    }
}

