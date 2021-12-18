using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public Color color;
    public string chatMessage;

    public enum playerGameState
    {
        SPAWNING = -1,
        SPAWNED,
        READY,
        PLAYING,
        WINNER,
        LOSER,
    }
    public playerGameState currentPlayerGameState;

    public bool isReady;
    
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;

    private bool[] inputs;
    private float yVelocity = 0;

    //Shoot
    public Transform spawnPoint;
    public GameObject muzzle;
    public GameObject impact;

    //Action
    public enum playerAction
    {
        SHOOT = 1,
        RELOAD,
        SHIELD,
    }
    public playerAction currentPlayerAction;

    private bool shoot;
    private bool reload;
    private bool shield;

    public int magazineSize;
    public int currentMagazine;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username, Color _color)
    {
        id = _id;
        username = _username;
        color = _color;
        chatMessage = null;
        currentPlayerGameState = playerGameState.SPAWNING;
        currentPlayerAction = playerAction.RELOAD;

        shoot = false;
        reload = false;
        shield = false;

        health = maxHealth;

        inputs = new bool[8];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);

        //-----------------------------------------------------
        if (inputs[5] && currentMagazine > 0)
        {
            currentPlayerAction = playerAction.SHOOT;

            ServerSend.ActionSelected(id, currentPlayerAction);
        }

        if (inputs[6])
        {
            currentPlayerAction = playerAction.RELOAD;
            ServerSend.ActionSelected(id, currentPlayerAction);
        }

        if (inputs[7])
        {
            currentPlayerAction = playerAction.SHIELD;
            ServerSend.ActionSelected(id, currentPlayerAction);
        }
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;

            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (shoot || currentPlayerGameState != playerGameState.PLAYING)
        {
            RaycastHit hit;
            RaycastHit hit2;
            RaycastHit hit3;
            RaycastHit hit4;

            //GameObject muzzleInstance = Instantiate(muzzle, spawnPoint.position, spawnPoint.localRotation);
            //muzzleInstance.transform.parent = spawnPoint;

            if (health <= 0f)
            {
                return;
            }

            if (Physics.Raycast(shootOrigin.position, _viewDirection, out hit, 50f))
            {
                //Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));

                if (hit.collider.CompareTag("Player"))
                {
                    //modify damage in function call to make it one shot 
                    hit.collider.GetComponent<Player>().TakeDamage(100f);
                }
            }

            if (Physics.Raycast(shootOrigin.position, _viewDirection + new Vector3(-.05f, 0f, 0f), out hit2, 50f))
            {
                //Instantiate(impact, hit2.point, Quaternion.LookRotation(hit2.normal));

                if (hit2.collider.CompareTag("Player"))
                {
                    //modify damage in function call to make it one shot 
                    hit2.collider.GetComponent<Player>().TakeDamage(100f);
                }
            }

            if (Physics.Raycast(shootOrigin.position, _viewDirection + new Vector3(0f, .05f, 0f), out hit3, 50f))
            {
                //Instantiate(impact, hit3.point, Quaternion.LookRotation(hit3.normal));

                if (hit3.collider.CompareTag("Player"))
                {
                    //modify damage in function call to make it one shot 
                    hit3.collider.GetComponent<Player>().TakeDamage(100f);
                }
            }

            if (Physics.Raycast(shootOrigin.position, _viewDirection + new Vector3(0f, -.05f, 0f), out hit4, 50f))
            {
                //Instantiate(impact, hit4.point, Quaternion.LookRotation(hit4.normal));

                if (hit4.collider.CompareTag("Player"))
                {
                    //modify damage in function call to make it one shot 
                    hit4.collider.GetComponent<Player>().TakeDamage(100f);
                }
            }

            ServerSend.BulletHit(hit, hit2, hit3, hit4);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (!shield && currentPlayerGameState == playerGameState.PLAYING)//Can only be damaged when playing and does not have shield activated
        {
            if (health <= 0)
            {
                return;
            }

            health -= _damage;
            if (health <= 0f)
            {
                health = 0f;
                controller.enabled = false;
                transform.position = new Vector3(0f, 25f, 0f);
                ServerSend.PlayerPosition(this);
                StartCoroutine(Respawn());
            }
        }
        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);

    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }

    public void SetChatMessage(string _message)
    {
        chatMessage = ColorText(color, username + ": ") + _message;
        ServerSend.ChatMessageFromPlayer(this);
    }

    public void SetWelcomeMessage()
    {
        chatMessage = "Welcome " + ColorText(color, username) + " to the chat!!";
        ServerSend.ChatMessageFromPlayer(id, this);
    }

    public void SetWelcomeMessage(int _id)
    {
        chatMessage =
            "Welcome to the chat!!\n" +
            "Your username and color is: " + ColorText(color, username) + "\n"/* +
            "You can type \"\\help\" to see the commands list."*/;

        ServerSend.ChatMessageWhisper(_id, this);
    }

    public void PerformAction()
    {
        if (currentMagazine == 0 && currentPlayerAction == playerAction.SHOOT) currentPlayerAction = playerAction.RELOAD;

        switch (currentPlayerAction)
        {
            case playerAction.SHOOT:
                shoot = true;
                reload = false;
                shield = false;
                currentMagazine--;
                //ServerSend. SOMETHING TO TRIGGER "YOU CAN SHOOT" ANIMATION TO VISUALLY REPRESENT
                break;
            case playerAction.RELOAD:
                shoot = false;
                reload = true;
                shield = false;
                currentMagazine++;
                //ServerSend. SOMETHING TO TRIGGER RELOAD ANIMATION TO VISUALLY REPRESENT
                break;
            case playerAction.SHIELD:
                shoot = false;
                reload = false;
                shield = true;
                //ServerSend. SOMETHING TO TRIGGER SHIELD TO VISUALLY REPRESENT
                break;
            default:
                break;
        }
    }

    public string ColorText(Color _color, string _text)
    {
        string _colortohexstring = ColorUtility.ToHtmlStringRGB(color); //converts the color variable into a string so then I can color code the text. 
        string _coloredText  = "<color=#" + _colortohexstring + ">" + _text + "</color>";

        return _coloredText;
    }
}
