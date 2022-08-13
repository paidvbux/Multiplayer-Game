using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> room = new Dictionary<ushort, Player>();
    public static Dictionary<ushort, Player> pregameRoom = new Dictionary<ushort, Player>();

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI playerIDText;
    public ushort id { get; private set; }
    public bool isLocal { get; private set; }

    private string username;

    private void Start()
    {
        UpdatePlayers();
    }

    private void Update()
    {
        if (UIManager.Singleton.ingamePlayerList.Count >= 1 && isLocal && UIManager.Singleton.ingamePlayerList.IndexOf(this) == 0)
        {
            //Do host specific things
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

        ingamePlayer.playerIDText.text = id.ToString();
        ingamePlayer.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        ingamePlayer.id = id;
        ingamePlayer.username = username;

        pregamePlayer.playerIDText.text = id.ToString();
        pregamePlayer.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        pregamePlayer.id = id;
        pregamePlayer.username = username;

        room.Add(id, ingamePlayer);
        pregameRoom.Add(id, pregamePlayer);
    }

    public static void UpdatePlayers()
    {
        UIManager.Singleton.ingamePlayerList.Clear();
        UIManager.Singleton.pregamePlayerList.Clear();
        foreach (KeyValuePair<ushort, Player> player in room)
        {
            UIManager.Singleton.ingamePlayerList.Add(player.Value);
        }
        foreach (KeyValuePair<ushort, Player> player in pregameRoom)
        {
            UIManager.Singleton.pregamePlayerList.Add(player.Value);
        }
        foreach (Player player in UIManager.Singleton.ingamePlayerList)
        {
            player.playerIDText.text = (UIManager.Singleton.ingamePlayerList.IndexOf(player) + 1).ToString();
            if (UIManager.Singleton.ingamePlayerList.IndexOf(player) == 0 && !player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host)";
            else if (!player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)}";
            if (UIManager.Singleton.ingamePlayerList.IndexOf(player) == 0 && player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host, You)";
            else if (player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (You)";
        }
        foreach (Player player in UIManager.Singleton.pregamePlayerList)
        {
            player.playerIDText.text = (UIManager.Singleton.pregamePlayerList.IndexOf(player) + 1).ToString();
            if (UIManager.Singleton.pregamePlayerList.IndexOf(player) == 0 && !player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host)";
            else if (!player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)}";
            if (UIManager.Singleton.pregamePlayerList.IndexOf(player) == 0 && player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (Host, You)";
            else if (player.isLocal) player.nameText.text = $"{(string.IsNullOrEmpty(player.username) ? "Guest" : player.username)} (You)";
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
}
