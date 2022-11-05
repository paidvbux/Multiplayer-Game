using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject GameUI => gameUI;
    public GameObject PregameUI => pregameUI;
    public GameObject StartButton => startButton;

    [Header("Connect")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject failedconnectionUI;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pregameUI;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private GameObject startButton;

    [Header("Player Card Settings")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image playerImage;

    [Header("Ingame UI")]
    public GameObject asleepUI;
    public GameObject endTurnButton;

    [Header("Other")]
    public List<Player> ingamePlayerList;
    public List<Player> pregamePlayerList;
    
    void Awake()
    {
        _singleton = this; //Easy reference for the component
    }

    public void ConnectClicked()
    {
        //Disables the components to not break the game
        usernameField.interactable = false;
        connectUI.SetActive(false);
        loadingUI.SetActive(true);

        NetworkManager.Singleton.Connect(); //Connects the player to the server
    }

    public void BackToMain()
    {
        //Re-enables and disables some components to not break the game
        usernameField.interactable = true;
        loadingUI.SetActive(false);
        gameUI.SetActive(false);
        failedconnectionUI.SetActive(true);

        foreach (KeyValuePair<ushort, Player> player in Player.room) //Destroys each player gameobjects
        {
            Destroy(player.Value.gameObject);
        }
        Player.room.Clear(); //Clears the player list
    }

    public void SendName() 
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name); //Create message
        message.AddString(usernameField.text); //Adds the username to the message
        //UI changes
        loadingUI.SetActive(false);
        pregameUI.SetActive(true);
        
        NetworkManager.Singleton.client.Send(message); //Sends the message to the server
    }

    public void SendStartGame()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.gameStarted); //Creates message
        message.AddBool(true); //Adds the gameStarted variable
        NetworkManager.Singleton.client.Send(message); //Sends the message to the server
    }

    public void SendMessage(TMP_InputField chatBox)
    {
        if (string.IsNullOrEmpty(chatBox.text)) return; //If the string is empty, ignore the click
        string[] messageInfo = new string[] { Player.localIngamePlayer.username, chatBox.text }; //Create a list that contains the username of the sender and the message
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.message); //Creates the message
        message.AddStrings(messageInfo); //Adds the info
        NetworkManager.Singleton.client.Send(message); //Sends the message to the server
        chatBox.text = "";
    }
}
