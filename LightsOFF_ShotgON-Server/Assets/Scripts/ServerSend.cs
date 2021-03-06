using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {           
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ChatMessageFromPlayer(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.chatMessageFromPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.chatMessage + "\n");

            SendTCPDataToAll(_packet);
        }
    }

    public static void ChatMessageFromPlayer(int _exceptClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.chatMessageFromPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.chatMessage + "\n");

            SendTCPDataToAll(_exceptClient, _packet);
        }
    }

    public static void ChatMessageWhisper(int _targetClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.chatMessageFromPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.chatMessage + "\n");

            SendTCPData(_targetClient, _packet);
        }
    }

    public static void ChatMessageFromServer(string _message)
    {
        using (Packet _packet = new Packet((int)ServerPackets.chatMessageFromServer))
        {
            _packet.Write("Server: " + _message + "\n");

            SendTCPDataToAll(_packet);
        }
    }

    public static void ChatMessageFromServer(int _exceptClient, string _message)
    {
        using (Packet _packet = new Packet((int)ServerPackets.chatMessageFromServer))
        {
            _packet.Write("Server: " + _message + "\n");

            SendTCPDataToAll(_exceptClient, _packet);
        }
    }


    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.color);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            //_packet.Write(_player.gravity);
            //_packet.Write(_player.jumpSpeed);
            //_packet.Write(_player.moveSpeed);
            //_packet.Write(_player.yVelocity);
            //_packet.Write(_player.controller);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _spawnerPosition, bool _hasItem)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_spawnerPosition);
            _packet.Write(_hasItem);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);
            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);

            SendTCPDataToAll(_packet);
        }
    }

    public static void BulletHit(RaycastHit _hit, RaycastHit _hit2, RaycastHit _hit3, RaycastHit _hit4)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bulletHit))
        {
            _packet.Write(_hit.point);
            _packet.Write(_hit2.point);
            _packet.Write(_hit3.point);
            _packet.Write(_hit4.point);

            _packet.Write(_hit.normal);
            _packet.Write(_hit2.normal);
            _packet.Write(_hit3.normal);
            _packet.Write(_hit4.normal);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GameStart(bool _gameStarted, int _gameTimeMinutes, int _actionTimeSeconds)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameStart))
        {
            _packet.Write(_gameStarted);
            _packet.Write(_gameTimeMinutes);
            _packet.Write(_actionTimeSeconds);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GameEnd(bool _gameStarted)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameEnd))
        {
            _packet.Write(_gameStarted);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GameTimer(int _toClient, float _currentGameTime)
    {
        using (Packet _packet = new Packet((int)ServerPackets.currentGameTime))
        {
            _packet.Write(_currentGameTime);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ActionTimer(int _toClient, float _currentActionTime)
    {
        using (Packet _packet = new Packet((int)ServerPackets.currentActionTime))
        {
            _packet.Write(_currentActionTime);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ActionSelected(int _toClient, Player.playerAction _currentPlayerAction)
    {
        using (Packet _packet = new Packet((int)ServerPackets.currentPlayerAction))
        {
            _packet.Write(_toClient);
            _packet.Write((int)_currentPlayerAction);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void CurrentMagazine(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.currentMagazine))
        {
            _packet.Write(_toClient);
            _packet.Write(_player.currentMagazine);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ShieldFeedback(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.shieldFeedback))
        {
            _packet.Write(_toClient);
            _packet.Write(_player.shield);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerShoot(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerShoot))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    #endregion
}
