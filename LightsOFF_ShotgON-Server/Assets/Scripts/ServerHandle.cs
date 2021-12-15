using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();
        Color _color = _packet.ReadColor();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username, _color);
    }

    public static void PlayerSpawned(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();

        Debug.Log($"{Server.clients[_fromClient].player.username} spawned successfully");

        Server.clients[_fromClient].player.currentPlayerGameState = Player.playerGameState.SPAWNED;
        Server.clients[_fromClient].player.SetWelcomeMessage();
        Server.clients[_fromClient].player.SetWelcomeMessage(_fromClient);
    }

    public static void ChatMessage(int _fromClient, Packet _packet)
    {
        string _message = _packet.ReadString();

        Server.clients[_fromClient].player.SetChatMessage(_message); 
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }

    public static void PlayerReady(int _fromClient, Packet _packet)
    {
        bool _isReady = _packet.ReadBool();

        //Reads content of bool and sets player state (for now can only be set to ready and can't be unset)
        if (_isReady) Server.clients[_fromClient].player.currentPlayerGameState = Player.playerGameState.READY;
        else Server.clients[_fromClient].player.currentPlayerGameState = Player.playerGameState.SPAWNED;
    }
}
    
