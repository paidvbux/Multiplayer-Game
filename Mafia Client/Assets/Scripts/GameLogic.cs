using RiptideNetworking;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public Role[] roles;

    private static GameLogic _singleton;
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public Transform IngamePlayerParent => ingamePlayerParent;
    public Transform PregamePlayerParent => pregamePlayerParent;

    public CanvasGroup canvasGroup;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform ingamePlayerParent;
    [SerializeField] private Transform pregamePlayerParent;

    public bool isNight;
    public string currentTurn;

    public Player selectedPlayer;
    public bool playerSelected;

    public bool healingSelected;
    public bool poisonSelected;
    public bool hasHealing;
    public bool hasPoison;
    public bool gameStarted;

    private void Awake()
    {
        Singleton = this;
    }

    void Update()
    {
        if (Player.localIngamePlayer != null) //Checks if there is a player in game
        {
            if (Player.localIngamePlayer.role != null && UIManager.Singleton.playerImage.sprite != Player.localIngamePlayer.role.displayImage) //Changes the image of the role
            {
                foreach (Player player in Player.room.Values)
                {
                    if (player.isLocal) UIManager.Singleton.playerImage.sprite = player.role.displayImage;
                }
            }
            if (isNight) //Checks if the game is in nighttime
            {
                if (Player.localIngamePlayer.role == null) return; //Exits if it tries to run without a player
                switch (Player.localIngamePlayer.role.roleName)
                {
                    //Innocent Roles
                    case "Civilian":
                        WaitForNightEnd();
                        break;
                    case "Detective":
                        WaitForDetective();
                        break;
                    case "Witch":
                        WaitForWitch();
                        break;
                    case "Knight":
                        WaitForNightEnd();
                        break;
                    case "Hunter":
                        WaitForNightEnd();
                        break;

                    //Werewolf Roles
                    case "Werewolf":
                        WaitForWerewolf();
                        break;
                    case "Werewolf King":
                        WaitForWerewolf();
                        break;
                }
            }
        }
    }

    public Player WaitForSelection() //Check for selection
    {
        if (!playerSelected) return null;
        return selectedPlayer;
    }

    public void ConfirmSelection()
    {
        playerSelected = true;
    }

    public void EndTurn()
    {
        switch (currentTurn)
        {
            case "Detective":
                currentTurn = "";
                bool isBad = selectedPlayer.role.isBad;
                Debug.Log(isBad);
                break;
            case "Witch":
                currentTurn = "Detective";
                break;
            case "Werewolf":
                currentTurn = "Witch";
                break;
        }
        selectedPlayer = null;
        Player.localIngamePlayer.turnEnded = true;
    }

    private void SetSleepState(bool isSleeping)
    {
        UIManager.Singleton.asleepUI.SetActive(isSleeping);
        UIManager.Singleton.endTurnButton.SetActive(!isSleeping);
        canvasGroup.alpha = isSleeping ? 0 : 1;
    }

    private void WaitForNightEnd()
    {
        //Turns on UI that makes the user inactive
        SetSleepState(true);
        if (isNight) return;
        SetSleepState(false);
    }

    private void WaitForDetective()
    {
        //Turns on UI that makes the user inactive
        SetSleepState(true);

        if (currentTurn != "Detective" && currentTurn != "") return; //Reruns the function if the turn is not yet theirs

        //Turns off UI that makes the user inactive
        SetSleepState(false);
        playerSelected = false;
        currentTurn = "";
        isNight = false;
        UIManager.Singleton.endTurnButton.SetActive(false);
        WaitForNightEnd(); //Run function which just waits until the night ends
    }

    private void WaitForWitch()
    {
        //Turns on UI that makes the user inactive
        SetSleepState(true);

        if (currentTurn != "Witch" && currentTurn != "") return; //Reruns the function if the turn is not yet theirs
        
        //Turns off UI that makes the user inactive
        SetSleepState(false);
        Debug.Log(WaitForSelection());
        currentTurn = "Detective";
        UIManager.Singleton.endTurnButton.SetActive(false);
        WaitForNightEnd(); //Run function which just waits until the night ends
    }

    private void WaitForWerewolf()
    {
        //Turns off UI that makes the user inactive
        playerSelected = false;
        SetSleepState(currentTurn != "Werewolf");
        Player selected = WaitForSelection();
        if (selected.role.roleName != "")
        {
            EndTurn();
        }
        if (Player.localIngamePlayer.turnEnded)
        {
            UIManager.Singleton.endTurnButton.SetActive(false);
            WaitForNightEnd(); //Run function which just waits until the night ends
        }
    }

    public void SendSelected()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.selectedPlayer); //Create message with the id of the selectedPlayer along with the selectionType of the selection
        message.AddUShort(selectedPlayer.id);
        message.AddString(Player.localIngamePlayer.role.selectionType.ToString());
    }
}
