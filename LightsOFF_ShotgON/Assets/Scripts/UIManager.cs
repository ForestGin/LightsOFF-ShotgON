using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject connectMenu;
    public GameObject disconnectMenu;
    public InputField usernameField;

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

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer()
    {
        connectMenu.SetActive(false);
        //disconnectMenu.SetActive(true);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }

    public void DisconnectFromServer()
    {
        //connectMenu.SetActive(true);
        //disconnectMenu.SetActive(false);
        //usernameField.interactable = true;
        //Client.instance.Disconnect();
    }
}
