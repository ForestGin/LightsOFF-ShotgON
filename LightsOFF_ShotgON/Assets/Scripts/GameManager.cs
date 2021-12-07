using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        Client.instance.Disconnect();
    //    }
    //}

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer(int _id, string _username, Color _color, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().islocal = true;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().islocal = false;
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username, _color);
        //_player.GetComponent<PlayerManager>().id = _id;
        //_player.GetComponent<PlayerManager>().username = _username;
        //_player.GetComponent<PlayerManager>().color = _color;

        players.Add(_id, _player.GetComponent<PlayerManager>());
    }

    public Color RandomColor()
    {
        Color _color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        return _color;
    }
}
