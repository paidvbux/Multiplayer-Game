using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public bool isHost;

    public ushort id { get; private set; }
    public string username { get; private set; }

    [System.Serializable]
    public class Role
    {
        public string name;
        public string description;
        public int count;
    }

    public Role[] roles;

    private void OnDestroy()
    {
        list.Remove(id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
    
        player.SendSpawned();
        list.Add(id, player);
    }

    public static void StartGame(ushort id, bool gameStarted)
    {
        foreach(Player player in list.Values)
            player.SendGameStart(id);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private void SendGameStart(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(Message.Create(MessageSendMode.reliable, ServerToClientId.gameStarted).AddBool(true), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.gameStarted)]
    private static void GameStarted(ushort fromClientId, Message message)
    {
        StartGame(fromClientId, message.GetBool());
    }
    #endregion
}
