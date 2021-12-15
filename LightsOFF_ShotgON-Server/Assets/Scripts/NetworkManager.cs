using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    [HideInInspector]public int numClients;

    public int minPlayersToStart;

    private bool gameStarted = false;

    public int gameTimeMinutes;
    private float currentGameTime;
    public int actionTimeSeconds;
    private float currentActionTime;

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
                        //entry.Value.player.PerformAction();
                    }
                }

                //Resetting time
                currentActionTime = actionTimeSeconds;
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

    public bool CheckAllPlayersReady()
    {
        foreach(KeyValuePair<int, Client> entry in Server.clients)
        {
            if (entry.Value.player != null)
            {
                if (entry.Value.player.currentPlayerGameState != Player.playerGameState.READY)
                {
                    return false;
                }
            }
        }
        return true;
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
        ServerSend.GameStart(gameStarted);
    }
}
