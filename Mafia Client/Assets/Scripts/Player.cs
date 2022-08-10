using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> room = new Dictionary<ushort, Player>();
    public static Dictionary<ushort, Player> roomHost = new Dictionary<ushort, Player>(1);

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI playerIDText;
    public ushort id { get; private set; }
    public bool isLocal { get; private set; }

    private string username;

    private void OnDestroy()
    {
        room.Remove(id);
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.transform.SetParent(GameLogic.Singleton.PlayerParent);
            player.transform.localScale = Vector3.one;
            player.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)} (You)";
            player.isLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.transform.SetParent(GameLogic.Singleton.PlayerParent);
            player.transform.localScale = Vector3.one;
            player.nameText.text = $"{(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            player.isLocal = false;
        }

        player.playerIDText.text = id.ToString();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = username;

        room.Add(id, player);
    }

    public static void UpdatePlayers()
    {
        //int idOffset = 0;
        //Dictionary<ushort, Player> newDict = room;
        //for (int i = 0; i < room.Count; i++)
        //{
        //    Player player = room[newDict.Keys.Min()];
        //    room.Remove(newDict.Keys.Min());
        //    player.id = (ushort)(1 + idOffset);
        //    player.playerIDText.text = player.id.ToString();
        //    room.Add((ushort)(1 + idOffset), player);
        //    idOffset++;
        //}
        //room = newDict;
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
}
