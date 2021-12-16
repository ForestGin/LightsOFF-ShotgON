﻿using System;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //[SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private TMP_Text playerName = null;
    [SerializeField] private GameObject playerModel = null;

    [SerializeField] private Text pressReadyText = null;
    [SerializeField] private Text readyText = null;

    public Transform camTransform;
    private bool spawned = false;
    private bool pressedReady = false;

    //Shoot
    public Transform spawnPoint;
    public GameObject muzzle;
    [SerializeField] private WeaponAnimator shotgunAnimator;
    [SerializeField] private float zoomInFieldOfView;
    private float baseFieldOfView;
    [SerializeField] private Camera mainCam;
    //public GameObject impact;

    private void Start()
    {
        baseFieldOfView = mainCam.fieldOfView;
        Cursor.visible = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //instantiate shoot particle
            GameObject muzzleInstance = Instantiate(muzzle, spawnPoint.position, spawnPoint.localRotation);
            muzzleInstance.transform.parent = spawnPoint;

            ClientSend.PlayerShoot(camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StopAllCoroutines();
            StartCoroutine(ZoomAnim(true));
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopAllCoroutines();
            StartCoroutine(ZoomAnim(false));
        }

        if (gameObject.GetComponent<PlayerManager>().inGame)
        {
            readyText.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!spawned) //This is false only the first time it loops
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
        else
        {
            if(!pressedReady)//for now you can't "unready"
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    pressedReady = true;
                    pressReadyText.gameObject.SetActive(false);
                    readyText.gameObject.SetActive(true);

                    SendReady();
                }
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
        //gameObject.GetComponent<PlayerManager>().isReady = true;
        
        ClientSend.PlayerReady(true);
    }

    private IEnumerator ZoomAnim(bool zoomIn)
    {
        shotgunAnimator.ZoomIn(zoomIn);
        float animTime = shotgunAnimator.ZoomAnimTime;
        float elapsed = 0f;
        float startFieldOfView = mainCam.fieldOfView;

        float endFieldOfView = (zoomIn) ? zoomInFieldOfView : baseFieldOfView;

        while (elapsed < animTime)
        {
            float currentFieldOfView = Mathf.Lerp(startFieldOfView, endFieldOfView, elapsed / animTime);
            mainCam.fieldOfView = currentFieldOfView;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
