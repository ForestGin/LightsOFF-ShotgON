using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    private int numClients;
    public int minPlayersToStart;

    private bool gameStarted = false;

    public int gameTimeMinutes;
    private float currentTime;
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
        else
        {

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

    public void CountPlayers()
    {
        //numClients = Server.clients.Count;//This counts the maxplayer so its useless
    }

    public bool CheckAllPlayersReady()
    {
        foreach(KeyValuePair<int, Client> entry in Server.clients)
        {
            if(!entry.Value.player.isReady)
            {
                return false;
            }
        }
        return true;
    }

    private void GameStart()
    {
        gameStarted = true;
        currentTime = gameTimeMinutes * 60;
        currentActionTime = actionTimeSeconds;
    }
}
