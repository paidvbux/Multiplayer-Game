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

    public int roleId;

    public static List<int> rolesAvailable;

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

    public static void AddRole(ushort id, int _roleId)
    {
        list[id].roleId = _roleId;
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendRole(id);

        list[id].SendRole();
    }

    public static void StartGame(ushort id, bool gameStarted)
    {
        foreach (Player player in list.Values)
            player.SendGameStart(id);

        list[id].SendGameStart();
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

    private void SendGameStart()
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.gameStarted).AddBool(true));
    }

    private void SendGameStart(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(Message.Create(MessageSendMode.reliable, ServerToClientId.gameStarted).AddBool(true), toClientId);
    }
    private void SendRole()
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.playerRole).AddUShort(id).AddInt(roleId));
    }
    
    private void SendRole(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(Message.Create(MessageSendMode.reliable, ServerToClientId.playerRole).AddUShort(id).AddInt(roleId), toClientId);
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
        rolesAvailable = new List<int>(0);
        for (int i = 0; i < NetworkManager.Singleton.server.ClientCount; i++) rolesAvailable.Add(i);
        foreach (ushort ids in list.Keys)
        {
            int _roleId = rolesAvailable[Random.Range(0, rolesAvailable.Count - 1)];
            rolesAvailable.Remove(_roleId);
            AddRole(ids, _roleId);
        }
    }
    #endregion
}
