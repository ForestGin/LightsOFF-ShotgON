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

    public enum playerAction
    {
        SHOOT,
        RELOAD,
        SHIELD,
    }
    public playerAction currentPlayerAction;

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

    public bool[] inputs;
    public float yVelocity = 0;


    //Shoot
    public Transform spawnPoint;
    public GameObject muzzle;
    public GameObject impact;

    //private float timeDifferenceThreshold;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        //timeDifferenceThreshold = 0.05f;
    }

    public void Initialize(int _id, string _username, Color _color)
    {
        id = _id;
        username = _username;
        color = _color;
        chatMessage = null;
        currentPlayerGameState = playerGameState.SPAWNING;

        health = maxHealth;

        inputs = new bool[5];
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

    public void TakeDamage(float _damage)
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

    public string ColorText(Color _color, string _text)
    {
        string _colortohexstring = ColorUtility.ToHtmlStringRGB(color); //converts the color variable into a string so then I can color code the text. 
        string _coloredText  = "<color=#" + _colortohexstring + ">" + _text + "</color>";

        return _coloredText;
    }

    //public void PlayerPositionReconciliation(Vector3 _currentPosition)
    //{
    //    Vector3 difference = new Vector3(
    //        Mathf.Abs(_currentPosition.x - this.transform.position.x),
    //        Mathf.Abs(_currentPosition.y - this.transform.position.y),
    //        Mathf.Abs(_currentPosition.z - this.transform.position.z));
              
    //    if (difference.x > timeDifferenceThreshold ||
    //        difference.y > timeDifferenceThreshold ||
    //        difference.z > timeDifferenceThreshold)
    //    {
    //        ServerSend.PlayerPosition(this);
    //    }
    //}

    //public void PlayerRotationReconciliation(Quaternion _currentRotation)
    //{
    //    Quaternion difference = new Quaternion(
    //        Mathf.Abs(_currentRotation.x - this.transform.rotation.x),
    //        Mathf.Abs(_currentRotation.y - this.transform.rotation.y),
    //        Mathf.Abs(_currentRotation.z - this.transform.rotation.z),
    //        Mathf.Abs(_currentRotation.w - this.transform.rotation.w));

    //    if (difference.x > timeDifferenceThreshold ||
    //        difference.y > timeDifferenceThreshold ||
    //        difference.z > timeDifferenceThreshold ||
    //        difference.w > timeDifferenceThreshold)
    //    {
    //        ServerSend.PlayerRotation(this);
    //    }
    //}
}
