using RiptideNetworking;
using TMPro;
using UnityEngine;
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

    [Header("Ingame UI")]
    public GameObject asleepUI;
    public GameObject selectionUI;

    [Header("Other")]
    public List<Player> ingamePlayerList;
    public List<Player> pregamePlayerList;
    
    void Awake()
    {
        _singleton = this;
    }

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectUI.SetActive(false);
        loadingUI.SetActive(true);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        loadingUI.SetActive(false);
        gameUI.SetActive(false);
        failedconnectionUI.SetActive(true);
        foreach (KeyValuePair<ushort, Player> player in Player.room)
        {
            Destroy(player.Value.gameObject);
        }
        Player.room.Clear();
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        loadingUI.SetActive(false);
        pregameUI.SetActive(true);
        NetworkManager.Singleton.client.Send(message);
    }

    public void SendStartGame()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.gameStarted);
        message.AddBool(true);
        NetworkManager.Singleton.client.Send(message);
    }
}
