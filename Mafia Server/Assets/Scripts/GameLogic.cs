using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public Deck[] decks;

    private static GameLogic _singleton;
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        Singleton = this;
    }

    private static void SendSelectedToAll(ushort selectedId)
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.selectedPlayer).AddUShort(selectedId)); //Sends the message
    }

    [MessageHandler((ushort)ClientToServerId.selectedPlayer)]
    private static void SelectedPlayer(ushort fromClientId, Message message) //Runs the function to send the selected player
    {
        SendSelectedToAll(message.GetUShort());
    }

}
