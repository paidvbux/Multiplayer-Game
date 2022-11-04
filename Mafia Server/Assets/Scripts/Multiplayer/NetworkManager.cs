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
}
