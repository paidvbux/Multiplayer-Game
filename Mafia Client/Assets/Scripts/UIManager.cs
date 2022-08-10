using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RiptideNetworking;

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

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject failedconnectionUI;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private TMP_InputField usernameField;

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
        failedconnectionUI.SetActive(true);
    }
    
    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        loadingUI.SetActive(false);
        NetworkManager.Singleton.client.Send(message);
    }
}
