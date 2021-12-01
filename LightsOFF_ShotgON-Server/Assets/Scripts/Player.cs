using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public Color color;
    public string chatMessage;
    //private bool newMessage = false;

    private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
    private bool[] inputs;

    public void Initialize(int _id, string _username, Color _color)
    {
        id = _id;
        username = _username;
        color = _color;
        chatMessage = null;

        inputs = new bool[4];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
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

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        transform.position += _moveDirection * moveSpeed;

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

    public string ColorText(Color _color, string _text)
    {
        string _colortohexstring = ColorUtility.ToHtmlStringRGB(color); //converts the color variable into a string so then I can color code the text. 
        string _coloredText  = "<color=#" + _colortohexstring + ">" + _text + "</color>";

        return _coloredText;
    }
}
