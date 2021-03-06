using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void ChatMessageFromPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _msg = _packet.ReadString();

        Debug.Log($"Chat message recieved: {_msg}");

        GameObject _player = GameObject.Find("LocalPlayer(Clone)");//this needs to be revised

        if (_player != null)
        {
            _player.GetComponent<PlayerController>().HandleChatMessage(_msg);
        }
    }

    public static void ChatMessageFromServer(Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Server chat message recieved: {_msg}");

        GameObject _player = GameObject.Find("LocalPlayer(Clone)");//this needs to be revised

        if (_player != null)
        {
            _player.GetComponent<PlayerController>().HandleChatMessage(_msg);
        }
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Color _color = _packet.ReadColor(); 
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        //------------------
        //CharacterController controller =_packet.Read PROBLEMA PARA LEER CHARACTER CONTROLLER, SERA MEJOR CREARLO EN EL PREFAB Y SCRIPT DEL CLIENTE
        //float _gravity = _packet.ReadFloat();
        //float _jumpSpeed = _packet.ReadFloat();
        //float _moveSpeed = _packet.ReadFloat();
        //float _yVelocity = _packet.ReadFloat();

        GameManager.instance.SpawnPlayer(_id, _username, _color, _position, _rotation/*, _gravity, _jumpSpeed, _moveSpeed, _yVelocity*/);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        //call new transform calculated from player manager
        //GameManager.players[_id].transform.position = _position;
        GameManager.players[_id].SetPosition(_position);
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.players[_byPlayer].itemCount++;
    }

    public static void BulletHit(Packet _packet)
    {
        Vector3 _hit = _packet.ReadVector3();
        Vector3 _hit2 = _packet.ReadVector3();
        Vector3 _hit3 = _packet.ReadVector3();
        Vector3 _hit4 = _packet.ReadVector3();

        Vector3 _hitN = _packet.ReadVector3();
        Vector3 _hit2N = _packet.ReadVector3();
        Vector3 _hit3N = _packet.ReadVector3();
        Vector3 _hit4N = _packet.ReadVector3();

        GameManager.instance.CreateBulletHitParticles(_hit, _hit2, _hit3, _hit4, _hitN, _hit2N, _hit3N, _hit4N);
    }

    public static void GameStart(Packet _packet)
    {
        bool _gameStarted = _packet.ReadBool();
        int _gameTimeMinutes = _packet.ReadInt();
        int _actionTimeSeconds = _packet.ReadInt();

        GameManager.instance.GameStart(_gameStarted, _gameTimeMinutes, _actionTimeSeconds);
    }
    public static void GameEnd(Packet _packet)
    {
        bool _gameStarted = _packet.ReadBool();

        GameManager.instance.GameEnd(_gameStarted);
    }

    public static void GameTimer(Packet _packet)
    {
        float _currentGameTime = _packet.ReadFloat();

        GameManager.instance.GameTimerReconciliation(_currentGameTime);
    }

    public static void ActionTimer(Packet _packet)
    {
        float _currentActionTime = _packet.ReadFloat();

        GameManager.instance.ActionTimerReconciliation(_currentActionTime);
    }

    public static void ActionSelected(Packet _packet)
    {
        int _byPlayer = _packet.ReadInt();
        int _currentPlayerAction = _packet.ReadInt();

        GameManager.players[_byPlayer].gameObject.GetComponent<PlayerController>().currentPlayerAction = (PlayerController.playerAction)_currentPlayerAction;
    }

    public static void CurrentMagazine(Packet _packet)
    {
        int _byPlayer = _packet.ReadInt();
        int _currentMagazine = _packet.ReadInt();

        GameManager.players[_byPlayer].gameObject.GetComponent<PlayerController>().currentMagazine = _currentMagazine;
    }

    public static void ShieldFeedback(Packet _packet)
    {
        int _byPlayer = _packet.ReadInt();
        bool _shieldActive = _packet.ReadBool();

        GameManager.players[_byPlayer].ShieldActive(_shieldActive);
    }

    public static void PlayerShoot(Packet _packet)
    {
        int _byPlayer = _packet.ReadInt();
        GameManager.players[_byPlayer].gameObject.GetComponent<PlayerController>().Shoot();
        
    }
}
