using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

//MoMoney is cool 

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> room = new Dictionary<ushort, Player>(); //Holds the list of players in the game room
    public static Dictionary<ushort, Player> pregameRoom = new Dictionary<ushort, Player>(); //Holds the list of players in the pregame room

    //Used for easier access
    public static Player localIngamePlayer; //Holds the local player in game
    public static Player localPregamePlayer; //Holds the local player in pregame

    public TextMeshProUGUI nameText; //Holds the variable which holds the text to hold the name
    public TextMeshProUGUI playerIDText; //Holds the variable which holds the text to hold the id of the player
    public ushort id { get; private set; } //Holds the id of the player
    public bool isLocal { get; private set; } //Holds if the player is local to the computer

    public string username; //Holds the name of the player

    public int roleId; //Holds the id of the role
    public Role role; //Holds the role of the player

    public GameObject selectionObject; //Holds the image that shows that the player is selected

    [HideInInspector] public bool turnEnded; //Holds if the player's turn has ended
    private void Start()
    {
        UpdatePlayers();
    }

    private void Update()
    {
        //Check if there is a room or not and do host specific things
        if (UIManager.Singleton.ingamePlayerList.Count >= 1 && isLocal)
        {
            //Do host specific things
            if (UIManager.Singleton.ingamePlayerList.IndexOf(this) == 0 || UIManager.Singleton.pregamePlayerList.IndexOf(this) == 0)
                UIManager.Singleton.StartButton.SetActive(true);
            else
                UIManager.Singleton.StartButton.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        //Remove itself from the room after it gets destroyed
        room.Remove(id);
        pregameRoom.Remove(id);
    }

    public static void SetRole(ushort id, int _roleId)
    {
        room[id].role = GameLogic.Singleton.roles[_roleId];
        pregameRoom[id].role = GameLogic.Singleton.roles[_roleId];

        room[id].roleId = _roleId;
        pregameRoom[id].roleId = _roleId;
    } //Sets the role of the player

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player ingamePlayer;
        Player pregamePlayer;
        if (id == NetworkManager.Singleton.client.Id)
        {
            //Instantiate local players in game screen
            ingamePlayer = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            ingamePlayer.transform.SetParent(GameLogic.Singleton.IngamePlayerParent);
            ingamePlayer.transform.localScale = Vector3.one;
            ingamePlayer.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)} (You)";
            ingamePlayer.isLocal = true;

            //Instantiate local players in pregame screen
            pregamePlayer = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            pregamePlayer.transform.SetParent(GameLogic.Singleton.PregamePlayerParent);
            pregamePlayer.transform.localScale = Vector3.one;
            pregamePlayer.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)} (You)";
            pregamePlayer.isLocal = true;
        }
        else
        {
            //Instantiate players in game screen
            ingamePlayer = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            ingamePlayer.transform.SetParent(GameLogic.Singleton.IngamePlayerParent);
            ingamePlayer.transform.localScale = Vector3.one;
            ingamePlayer.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            ingamePlayer.isLocal = false;

            //Instantiate local players in pregame screen
            pregamePlayer = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            pregamePlayer.transform.SetParent(GameLogic.Singleton.PregamePlayerParent);
            pregamePlayer.transform.localScale = Vector3.one;
            pregamePlayer.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            pregamePlayer.isLocal = false;
        }

        //Set all the UI stuff for the player in the game screen
        ingamePlayer.playerIDText.text = id.ToString();
        ingamePlayer.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        ingamePlayer.id = id;
        ingamePlayer.username = username;

        //Set all the UI stuff for the player in the pregame screen
        pregamePlayer.playerIDText.text = id.ToString();
        pregamePlayer.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        pregamePlayer.id = id;
        pregamePlayer.username = username;

        //Add players to the dictionaries for future reference
        room.Add(id, ingamePlayer);
        pregameRoom.Add(id, pregamePlayer);
    } //Spawns the player

    public static void UpdatePlayers()
    {
        //Clear lists to avoid conflicts and remove any empty players
        UIManager.Singleton.ingamePlayerList.Clear();
        UIManager.Singleton.pregamePlayerList.Clear();

        //Recreate the lists
        foreach (KeyValuePair<ushort, Player> player in room)
        {
            UIManager.Singleton.ingamePlayerList.Add(player.Value);
        }
        foreach (KeyValuePair<ushort, Player> player in pregameRoom)
        {
            UIManager.Singleton.pregamePlayerList.Add(player.Value);
        }
        
        //Sort lists
        UIManager.Singleton.ingamePlayerList.Sort((p1, p2) => p1.id.CompareTo(p2.id));
        UIManager.Singleton.pregamePlayerList.Sort((p1, p2) => p1.id.CompareTo(p2.id));

        //Go through each player to update the UI
        foreach (Player player in UIManager.Singleton.ingamePlayerList)
        {
            //Update the display ID for each player
            player.playerIDText.text = (UIManager.Singleton.ingamePlayerList.IndexOf(player) + 1).ToString();
            
            //Update the name to show who is the current host
            if (UIManager.Singleton.ingamePlayerList.IndexOf(player) == 0 && !player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host)";
            else if (!player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)}";
            if (UIManager.Singleton.ingamePlayerList.IndexOf(player) == 0 && player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host, You)";
            else if (player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (You)";
        }

        foreach (Player player in UIManager.Singleton.pregamePlayerList)
        {
            //Update the display ID for each player
            player.playerIDText.text = (UIManager.Singleton.pregamePlayerList.IndexOf(player) + 1).ToString();
            
            //Update the name to show who is the current host
            if (UIManager.Singleton.pregamePlayerList.IndexOf(player) == 0 && !player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host)";
            else if (!player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)}";
            if (UIManager.Singleton.pregamePlayerList.IndexOf(player) == 0 && player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host, You)";
            else if (player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (You)";
        }

        UpdateLocalPlayers();
    } //Update the players so that the game does not break
    public static void UpdateLocalPlayers()
    {
        foreach (Player player in UIManager.Singleton.ingamePlayerList) { if (player.isLocal) { localIngamePlayer = player; break; } }
        foreach (Player player in UIManager.Singleton.pregamePlayerList) { if (player.isLocal) { localPregamePlayer = player; break; } }
    } //Update the players so that the game does not break

    public void SelectPlayer(Player player)
    {
        if (pregameRoom.Values.Contains<Player>(player)) return;
        foreach (Player p in room.Values)
        {
            if (p.id == player.id)
            {
                p.selectionObject.SetActive(true);
                GameLogic.Singleton.playerSelected = true;
                GameLogic.Singleton.selectedPlayer = p;
            }
            else
            {
                p.selectionObject.SetActive(false);
            }
        } //Select
    } //Select the player

    [MessageHandler((ushort)ServerToClientId.message)]
    public static void AddMessage(Message message) //Adds the message to the chat message array
    {
        string[] storedStrings = message.GetStrings(); //Creates a string array to hold the username of the sender and the message sent
        Player selectedPlayer = null; //Creates a temp player reference for later use
        foreach (Player player in room.Values) //Checks to see which player sent the message
        {
            if (player.username == storedStrings[0])
            {
                selectedPlayer = player; //Sets the temp player to the reference
                break;
            }
        }
        ChatScript.Message.Types type = ChatScript.Message.Types.normal; //Adds the type of message which changes the color of the text
        if (selectedPlayer.isLocal) type = ChatScript.Message.Types.local; //Changes the type to local which highlights it in yellow
        else if (GameLogic.Singleton.gameStarted && selectedPlayer.role.isBad && localIngamePlayer.role.isBad) type = ChatScript.Message.Types.wolf; //Changes the type to be wolf which highlights it in red
        ChatScript.Singleton.messages.Add(new ChatScript.Message(storedStrings[0], storedStrings[1], type)); //Appends and creates the message
        ChatScript.Singleton.UpdateMessages(); //Update the messages
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message) //Receives the id, name and position and spawns the player
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.gameStarted)]
    private static void StartGame(Message message) //Receives the start of the game
    {
        UIManager.Singleton.GameUI.SetActive(true);
        UIManager.Singleton.PregameUI.SetActive(false);
        GameLogic.Singleton.currentTurn = "Werewolf";
        GameLogic.Singleton.isNight = true;
    }

    [MessageHandler((ushort)ServerToClientId.playerRole)]
    private static void GetRole(Message message) //Receives the role defined by the server
    {
        SetRole(message.GetUShort(), message.GetInt());
    }
}
