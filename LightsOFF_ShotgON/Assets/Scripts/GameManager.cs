using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject impact;

    private GameObject bullet1;
    private GameObject bullet2;
    private GameObject bullet3;
    private GameObject bullet4;

    private bool gameStarted = false;

    //Timers
    public Text gameTimer;
    public Text actionTimer;

    public int gameTimeMinutes;
    public float currentGameTime;
    public int actionTimeSeconds;
    public float currentActionTime;

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

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            currentGameTime -= Time.deltaTime;
            currentActionTime -= Time.deltaTime;

            //Since we have "server reconciliation" this action time would reset anyways
            //Action
            if (currentActionTime <= 0)
            {
                //Resetting time
                currentActionTime = actionTimeSeconds;
            }

            TimeSpan gameTime = TimeSpan.FromSeconds(currentGameTime);
            TimeSpan actionTime = TimeSpan.FromSeconds(currentActionTime);        
            
            gameTimer.text = gameTime.Minutes.ToString() + ":" + gameTime.Seconds.ToString();
            actionTimer.text = actionTime.Seconds.ToString();

            SendTimersToServer(currentGameTime, currentActionTime);
        }
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer(int _id, string _username, Color _color, Vector3 _position, Quaternion _rotation/*, float _gravity, float _jumpSpeed, float _moveSpeed, float _yVelocity*/)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().isLocal = true;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().isLocal = false;
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username, _color);
        //_player.GetComponent<PlayerManager>().SetMovement(_gravity, _jumpSpeed, _moveSpeed, _yVelocity);

        //_player.GetComponent<PlayerManager>().id = _id;
        //_player.GetComponent<PlayerManager>().username = _username;
        //_player.GetComponent<PlayerManager>().color = _color;

        players.Add(_id, _player.GetComponent<PlayerManager>());
    }

    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        GameObject _spawner = Instantiate(itemSpawnerPrefab, _position, itemSpawnerPrefab.transform.rotation);
        _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
    }

    public void CreateBulletHitParticles(Vector3 _hit, Vector3 _hit2, Vector3 _hit3, Vector3 _hit4, Vector3 _hitN, Vector3 _hit2N, Vector3 _hit3N, Vector3 _hit4N)
    {
        float time = 1f;

        bullet1 = (GameObject)Instantiate(impact, _hit, Quaternion.LookRotation(_hitN));
        bullet2 = (GameObject)Instantiate(impact, _hit2, Quaternion.LookRotation(_hit2N));
        bullet3 = (GameObject)Instantiate(impact, _hit3, Quaternion.LookRotation(_hit3N));
        bullet4 = (GameObject)Instantiate(impact, _hit4, Quaternion.LookRotation(_hit4N));
        Destroy(bullet1, time);
        Destroy(bullet2, time);
        Destroy(bullet3, time);
        Destroy(bullet4, time);

    }
    public Color RandomColor()
    {
        Color _color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        return _color;
    }

    public void GameStart(bool _gameStarted, int _gameTimeMinutes, int _actionTimeSeconds)
    {
        gameStarted = _gameStarted;
        gameTimeMinutes = _gameTimeMinutes;
        actionTimeSeconds = _actionTimeSeconds;

        currentGameTime = gameTimeMinutes * 60;
        currentActionTime = actionTimeSeconds;

        gameTimer.gameObject.SetActive(true);
        actionTimer.gameObject.SetActive(true);

        gameTimer.text = currentGameTime.ToString();
        actionTimer.text = currentActionTime.ToString();

        foreach (KeyValuePair<int, PlayerManager> entry in players)
        {
            entry.Value.isReady = false;
            entry.Value.inGame = true;
        }
    }

    public void GameEnd(bool _gameStarted)
    {
        gameStarted = _gameStarted;

        currentGameTime = 0f;
        currentActionTime = 0f;

        gameTimer.gameObject.SetActive(false);
        actionTimer.gameObject.SetActive(false);

        foreach (KeyValuePair<int, PlayerManager> entry in players)
        {
            entry.Value.isReady = false;
            entry.Value.inGame = false;
            entry.Value.inGameEnd = true;

            //entry.Value.SetHealth(0f);
        }
    }

    public void SendTimersToServer(float _currentGameTime, float _currentActionTime)
    {
        ClientSend.GameTimer(_currentGameTime);
        ClientSend.ActionTimer(_currentActionTime);
    }
    
    public void GameTimerReconciliation(float _currentGameTime)
    {
        currentGameTime = _currentGameTime;
    }

    public void ActionTimerReconciliation(float _currentActionTime)
    {
        currentActionTime = _currentActionTime;
    }
}
