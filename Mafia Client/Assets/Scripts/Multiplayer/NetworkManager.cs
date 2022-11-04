using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
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

    public Client client { get; private set; }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        client = new Client();
        client.Connected += DidConnect;
        client.ConnectionFailed += FailedToConnect;
        client.ClientDisconnected += PlayerLeft;
        client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        client.Tick();
    }

    private void OnApplicationQuit()
    {
        client.Disconnect();
    }

    public void Connect()
    {
        client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.room[e.Id].gameObject);
        Destroy(Player.pregameRoom[e.Id].gameObject);
        Player.room.Remove(e.Id);
        Player.pregameRoom.Remove(e.Id);
        Player.UpdatePlayers();
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
        foreach (Player player in Player.room.Values) Destroy(player.gameObject);
        foreach (Player player in Player.pregameRoom.Values) Destroy(player.gameObject);
        Player.room.Clear();
        Player.pregameRoom.Clear();
        Player.localIngamePlayer = null;
        Player.localPregamePlayer = null;
        UIManager.Singleton.ingamePlayerList.Clear();
    }
}
