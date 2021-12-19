using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    [HideInInspector]public int numClients;

    public int minPlayersToStart;

    public bool gameStarted = false;

    //Timers
    public int gameTimeMinutes;
    private float currentGameTime;
    public int actionTimeSeconds;
    private float currentActionTime;

    private float timeDifferenceThreshold;

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

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;//Limiting framerate bc we don't need it

        Server.Start(50, 26950);

        timeDifferenceThreshold = 0.05f; //0.1 bc its above and under
    }

    private void Update()
    {
        if (!gameStarted)
        {
            //If minimum players to start
            if (numClients >= minPlayersToStart)
            {
                //Check if all are ready
                if (CheckAllPlayersReady())
                {
                    GameStart();
                }
            }
        } 
    }

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            currentGameTime -= Time.deltaTime;
            currentActionTime -= Time.deltaTime;

            //Action
            if (currentActionTime <= 0)
            {
                //Perform selected action
                foreach (KeyValuePair<int, Client> entry in Server.clients)
                {
                    if (entry.Value.player != null)
                    {
                        entry.Value.player.PerformAction();
                    }
                }

                //Resetting time
                currentActionTime = actionTimeSeconds;
            }

            if (currentGameTime <= 0)
            {
                GameEnd();
            }
            //ServerSend.GameTimer();
            //ServerSend.ActionTimer();
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }

    public int CountAllPlayersConnnected()
    {
        int numPlayers = 0;
        foreach (KeyValuePair<int, Client> entry in Server.clients)
        {
            if (entry.Value != null)
            {
                numPlayers++;
            }
        }
        return numPlayers;
    }

    public bool CheckAllPlayersReady()//This function does not work if players connect and then disconnect before readying(needs fix)
    {
        int _playersReady = 0;
        foreach(KeyValuePair<int, Client> entry in Server.clients)
        {
            if (entry.Value.player != null)
            {
                if (entry.Value.player.currentPlayerGameState == Player.playerGameState.READY)
                {
                    _playersReady++;
                    //return false;
                }
            }
        }

        if (_playersReady != numClients) return false;
        else return true;
        //return true;
    }

    private void GameStart()
    {
        gameStarted = true;
        currentGameTime = gameTimeMinutes * 60;
        currentActionTime = actionTimeSeconds;

        foreach (KeyValuePair<int, Client> entry in Server.clients)
        {
            if (entry.Value.player != null)
            {
                entry.Value.player.currentPlayerGameState = Player.playerGameState.PLAYING;
            }
        }
        Debug.Log("Game Started with " + gameTimeMinutes.ToString() + " minutes of game time and " + actionTimeSeconds.ToString() + " seconds between actions");
        ServerSend.GameStart(gameStarted, gameTimeMinutes, actionTimeSeconds);
    }

    public void GameEnd()
    {
        gameStarted = false;

        foreach (KeyValuePair<int, Client> entry in Server.clients)
        {
            if (entry.Value.player != null)
            {
                entry.Value.player.TakeDamage(entry.Value.player.maxHealth);
                entry.Value.player.currentPlayerGameState = Player.playerGameState.SPAWNED;
            }
        }
        Debug.Log("Game Ended, players now will respawn and will be able to play again");
        ServerSend.GameEnd(gameStarted);
    }

    public void GameTimerReconciliation(int _fromclient, float _currentGameTime)
    {
        float difference = Mathf.Abs(_currentGameTime - currentGameTime);

        if (difference > timeDifferenceThreshold)
        {
            ServerSend.GameTimer(_fromclient, currentGameTime);
        }
    }

    public void ActionTimerReconciliation(int _fromclient, float _currentActionTime)
    {
        float difference = Mathf.Abs(_currentActionTime - currentActionTime);

        if (difference > timeDifferenceThreshold)
        {
            ServerSend.ActionTimer(_fromclient, currentActionTime);
        }
    }
}
