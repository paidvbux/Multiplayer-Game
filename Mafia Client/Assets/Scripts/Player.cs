using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class Role
    {
        public string name;
        public Sprite imageSprite;
        public string description;
        public int count;
    }

    public static Dictionary<ushort, Player> room = new Dictionary<ushort, Player>();
    public static Dictionary<ushort, Player> pregameRoom = new Dictionary<ushort, Player>();

    public static Player localIngamePlayer;
    public static Player localPregamePlayer;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI playerIDText;
    public ushort id { get; private set; }
    public bool isLocal { get; private set; }

    private string username;

    public Role role;

    private void Start()
    {
        UpdatePlayers();
    }

    private void Update()
    {
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
        room.Remove(id);
    }

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
    }

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
    }

    public static void UpdateLocalPlayers()
    {
        if (localIngamePlayer != null)
        {
            foreach (Player player in UIManager.Singleton.ingamePlayerList) { if (player.isLocal) { localIngamePlayer = player; break; } }
        }

        if (localPregamePlayer != null)
        {
            foreach (Player player in UIManager.Singleton.pregamePlayerList) { if (player.isLocal) { localPregamePlayer = player; break; } }
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.gameStarted)]
    private static void StartGame(Message message)
    {
        UIManager.Singleton.GameUI.SetActive(true);
        UIManager.Singleton.PregameUI.SetActive(false);
    }
}
