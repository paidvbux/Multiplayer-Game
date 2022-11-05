using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerToClientId : ushort //Creates enums that allow the server to communicate to the client
{ 
    playerSpawned = 1,
    playerRole,
    gameStarted,
    selectedPlayer,
    endNight,
    message,
}

public enum ClientToServerId : ushort //Creates enums that allow the server to communicate to the client
{
    name = 1,
    playerRole,
    gameStarted,
    selectedPlayer,
    endNight,
    message,
}
public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    public Server server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    public static List<int> rolesAvailable;

    void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        server = new Server();
        server.Start(port, maxClientCount);
        server.ClientDisconnected += PlayerLeft;
    }

    void FixedUpdate()
    {
        server.Tick();
    }

    void OnApplicationQuit()
    {
        server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }

    public static void StartGame(ushort id, bool gameStarted)
    {
        NetworkManager.Singleton.SendGameStart(); //Starts the game for everyone (idk if it works lol i will need to fix later)
    }

    private void SendGameStart()
    {
        server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.gameStarted).AddBool(true)); //Sends a message containing the boolean that tells the game to start 
    }

    public static void SendMessage(ushort id, string user, string text)
    {
        string[] info = new string[2] { user, text }; //Creates an array of the username and the message
        NetworkManager.Singleton.SendMsg(info);
    }
    private void SendMsg(string[] info)
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.message).AddStrings(info)); //Sends the message containing the username and text of the message
    }

    [MessageHandler((ushort)ClientToServerId.message)]
    private static void MessageSend(ushort fromClientId, Message message) //Runs the message send function
    {
        string[] info = message.GetStrings();
        SendMessage(fromClientId, info[0], info[1]);
    }

    [MessageHandler((ushort)ClientToServerId.gameStarted)]
    private static void GameStarted(ushort fromClientId, Message message) //Runs the function to start the game when requested
    {
        StartGame(fromClientId, message.GetBool());
        rolesAvailable = new List<int>(0); //Assigns the roles randomly
        for (int i = 0; i < NetworkManager.Singleton.server.ClientCount; i++) rolesAvailable.Add(i);
        foreach (ushort ids in Player.list.Keys)
        {
            int _roleId = rolesAvailable[Random.Range(0, rolesAvailable.Count - 1)];
            rolesAvailable.Remove(_roleId);
            Player.AddRole(ids, _roleId);
        }
    }

}
