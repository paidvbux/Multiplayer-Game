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

    public static void Spawn(ushort id, string username) //Spawns a gameobject as a player which allows easier storage of values
    {
        foreach (Player otherPlayer in list.Values) //Sends a message to each player to tell it to spawn a player that has joined
            otherPlayer.SendSpawned(id);

        //Spawns the gameobject with the name
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
    
        //Send the name to everyone
        player.SendSpawned();
        list.Add(id, player); //Add the player script to the room
    }

    public static void AddRole(ushort id, int _roleId)
    {
        list[id].roleId = _roleId; //Assigns it server side so less network traffic
        foreach (Player otherPlayer in list.Values) //Sends the message to each of the players telling each player of their role
            otherPlayer.SendRole(id);

        //list[id].SendRole(); //Send to all
    }

    public static void StartGame(ushort id, bool gameStarted)
    {
        list[0].SendGameStart(); //Starts the game for everyone (idk if it works lol i will need to fix later)
    }

    public static void SendMessage(ushort id, string user, string text)
    {
        string[] info = new string[2] { user, text }; //Creates an array of the username and the message
        foreach (Player player in list.Values) //Sends the message to each other player
            player.SendMsg(info);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned))); //Sends a message containing the spawn data of the player
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId); //Sends a message containing the spawn data of the player to everyone
    }

    private void SendGameStart()
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.gameStarted).AddBool(true)); //Sends a message containing the boolean that tells the game to start 
    }
    
    //Probably works without the need of this function

    //private void SendRole()
    //{
    //    NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.playerRole).AddUShort(id).AddInt(roleId));
    //}

    private void SendRole(ushort toClientId)
    {
        NetworkManager.Singleton.server.Send(Message.Create(MessageSendMode.reliable, ServerToClientId.playerRole).AddUShort(id).AddInt(roleId), toClientId); //Sends the id of the role to each player
    }

    private void SendMsg(string[] info)
    {
        NetworkManager.Singleton.server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.message).AddStrings(info)); //Sends the message containing the username and text of the message
    }

    private Message AddSpawnData(Message message)
    {
        //Adds the general spawn data of the player

        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message) //Runs the function when it receives the spawn request
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.gameStarted)]
    private static void GameStarted(ushort fromClientId, Message message) //Runs the function to start the game when requested
    {
        StartGame(fromClientId, message.GetBool());
        rolesAvailable = new List<int>(0); //Assigns the roles randomly
        for (int i = 0; i < NetworkManager.Singleton.server.ClientCount; i++) rolesAvailable.Add(i);
        foreach (ushort ids in list.Keys)
        {
            print(rolesAvailable.Count - 1);
            int _roleId = rolesAvailable[Random.Range(0, rolesAvailable.Count - 1)];
            rolesAvailable.Remove(_roleId);
            AddRole(ids, _roleId);
        }
    }

    [MessageHandler((ushort)ClientToServerId.message)]
    private static void MessageSend(ushort fromClientId, Message message) //Runs the message send function
    {
        string[] info = message.GetStrings();
        SendMessage(fromClientId, info[0], info[1]);
    }

    #endregion
}
