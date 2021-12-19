using System;
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

    //Action
    public enum playerAction
    {
        SHOOT = 1,
        RELOAD,
        SHIELD,
        NONE
    }

    public playerAction currentPlayerAction;
    public Image imageShoot;
    public Image imageReload;
    public Image imageShield;

    public Image imageBullet1;
    public Image imageBullet2;
    public Image imageBullet3;
    public Image imageBullet4;
    public Image imageBullet5;
    public int currentMagazine;

    private void Start()
    {
        baseFieldOfView = mainCam.fieldOfView;
        Cursor.visible = false;
        currentPlayerAction = playerAction.NONE;
        Color unselected = new Color(1f, 1f, 1f, 0.4f);
        imageShoot.color = unselected;
        imageReload.color = unselected;
        imageShield.color = unselected;

        currentMagazine = 0;
        UpdateBulletsImage(currentMagazine);
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

        if (Input.GetKeyDown(KeyCode.R) && !pressedReady)//for now you can't "unready"
        {
            pressedReady = true;
            pressReadyText.gameObject.SetActive(false);
            readyText.gameObject.SetActive(true);

            SendReady();
        }

        if (gameObject.GetComponent<PlayerManager>().inGame)
        {
            if (readyText.gameObject.activeSelf)
            {
                readyText.gameObject.SetActive(false);
            }

            Color selected = new Color(1f, 1f, 1f, 1f);
            Color unselected = new Color(1f, 1f, 1f, 0.4f);

            switch (currentPlayerAction)
            {
                case playerAction.SHOOT:
                    imageShoot.color = selected;
                    imageReload.color = unselected;
                    imageShield.color = unselected;
                    break;
                case playerAction.RELOAD:
                    imageShoot.color = unselected;
                    imageReload.color = selected;
                    imageShield.color = unselected;
                    break;
                case playerAction.SHIELD:
                    imageShoot.color = unselected;
                    imageReload.color = unselected;
                    imageShield.color = selected;
                    break;
                default:
                    break;
            }
        }

        UpdateBulletsImage(currentMagazine);
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
        //------------
        
        //Vector2 _inputDirection = Vector2.zero;
        //if (Input.GetKey(KeyCode.W))
        //{
        //    _inputDirection.y += 1;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    _inputDirection.y -= 1;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    _inputDirection.x -= 1;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    _inputDirection.x += 1;
        //}

        SendInputToServer();

        //Move(_inputDirection);
        
        
    }

    //private void Move(Vector2 _inputDirection)
    //{
    //    Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
    //    _moveDirection *= gameObject.GetComponent<PlayerManager>().moveSpeed;

    //    if (gameObject.GetComponent<CharacterController>().isGrounded)
    //    {
    //        gameObject.GetComponent<PlayerManager>().yVelocity = 0f;

    //        if (Input.GetKey(KeyCode.Space))
    //        {
    //            gameObject.GetComponent<PlayerManager>().yVelocity = gameObject.GetComponent<PlayerManager>().jumpSpeed;
    //        }
    //    }
    //    gameObject.GetComponent<PlayerManager>().yVelocity += gameObject.GetComponent<PlayerManager>().gravity;

    //    _moveDirection.y = gameObject.GetComponent<PlayerManager>().yVelocity;
    //    gameObject.GetComponent<CharacterController>().Move(_moveDirection);

    //    //HACER CLIENT SENT
    //    ClientSend.PlayerPosition(this);
    //    ClientSend.PlayerRotation(this);
    //}

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
            //Input.GetKey(KeyCode.R),//do that eventually
            Input.GetKey(KeyCode.Alpha1),
            Input.GetKey(KeyCode.Alpha2),
            Input.GetKey(KeyCode.Alpha3)
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

    public void UpdateBulletsImage(int currentMagazine)
    {
        Color selected = new Color(1f, 1f, 1f, 1f);
        Color unselected = new Color(1f, 1f, 1f, 0.4f);

        switch (currentMagazine)
        {
            case 0:
                imageBullet1.color = unselected;
                imageBullet2.color = unselected;
                imageBullet3.color = unselected;
                imageBullet4.color = unselected;
                imageBullet5.color = unselected;
                break;
            case 1:
                imageBullet1.color = selected;
                imageBullet2.color = unselected;
                imageBullet3.color = unselected;
                imageBullet4.color = unselected;
                imageBullet5.color = unselected;
                break;
            case 2:
                imageBullet1.color = selected;
                imageBullet2.color = selected;
                imageBullet3.color = unselected;
                imageBullet4.color = unselected;
                imageBullet5.color = unselected;
                break;
            case 3:
                imageBullet1.color = selected;
                imageBullet2.color = selected;
                imageBullet3.color = selected;
                imageBullet4.color = unselected;
                imageBullet5.color = unselected;
                break;
            case 4:
                imageBullet1.color = selected;
                imageBullet2.color = selected;
                imageBullet3.color = selected;
                imageBullet4.color = selected;
                imageBullet5.color = unselected;
                break;
            case 5:
                imageBullet1.color = selected;
                imageBullet2.color = selected;
                imageBullet3.color = selected;
                imageBullet4.color = selected;
                imageBullet5.color = selected;
                break;
        }
    }
}
