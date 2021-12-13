using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //[SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private TMP_Text playerName = null;
    [SerializeField] private GameObject playerModel = null;

    [SerializeField] private GameObject readyButton = null;

    public Transform camTransform;
    private bool spawned = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        if (!spawned)
        {
            //Checking because the chat UI depends on the player GO so until the player spawns it cannot recieve messages
            ClientSend.PlayerSpawned();
            spawned = true;

            //THIS SHOUL BE DONE TO EVERY PLAYER BY THE GAME MANAGER
            //Checking if the player is local one 
            if (gameObject.GetComponent<PlayerManager>().isLocal)
            {
                //Setting Name Tag
                playerName.text = gameObject.GetComponent<PlayerManager>().username;

                //Creating a material and setting its color
                Renderer rend = playerModel.GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Standard"));
                rend.material.color = gameObject.GetComponent<PlayerManager>().color;
            }   
        }

        SendInputToServer();
    }

    public void HandleChatMessage(string _message)
    {
        chatText.text += _message;
    }

    public void CheckInputBoxMessage()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }

        string _message = inputField.text;

        if (string.IsNullOrWhiteSpace(_message)) { return; }

        SendChatMessageToServer(_message);

        inputField.text = string.Empty;
    }

    public void SendChatMessageToServer(string _message)
    {
        ClientSend.ChatMessage(_message);
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),
        };

        
        ClientSend.PlayerMovement(_inputs);
        
    }

    private void SendReady()
    {
        gameObject.GetComponent<PlayerManager>().isReady = true;
        ClientSend.PlayerReady(true);
    }
}
