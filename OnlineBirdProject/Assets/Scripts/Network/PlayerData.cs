using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public struct datatata
{
    public int colorId;
    public FixedString64Bytes playerName; // No es un string porque no deja serializar strings normales
}

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public FixedString64Bytes playerName; // No es un string porque no deja serializar strings normales
    public FixedString64Bytes playerId; // Utilizado por el paquete de Lobby

    public datatata gagaga;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId
            && colorId == other.colorId
            && playerName == other.playerName
            && playerId == other.playerId

            && gagaga.colorId == other.gagaga.colorId
            && gagaga.playerName == other.gagaga.playerName;
    }

    // Necesario si se quiere usar como NetworkVariable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);

        serializer.SerializeValue(ref gagaga.colorId);
        serializer.SerializeValue(ref gagaga.playerName);
    }
}
