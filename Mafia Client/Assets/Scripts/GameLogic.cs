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

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if (Player.localIngamePlayer != null)
        {
            switch (Player.localIngamePlayer.role.name)
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
        Debug.Log(currentTurn);
    }

    public Player WaitForSelection()
    {
        while (!playerSelected) ;
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
    }

    private void WaitForNightEnd()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        UIManager.Singleton.endTurnButton.SetActive(false);
        while (isNight);
        UIManager.Singleton.asleepUI.SetActive(false);
    }

    private void WaitForDetective()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        UIManager.Singleton.endTurnButton.SetActive(false);
        while (currentTurn != "Detective" && currentTurn != "") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        UIManager.Singleton.endTurnButton.SetActive(true);
        Debug.Log(WaitForSelection() ? "Bad" : "Good");
        selectedPlayer = null;
        playerSelected = false;
        currentTurn = "";
        isNight = false;
        UIManager.Singleton.endTurnButton.SetActive(false);
        WaitForNightEnd();
    }

    private void WaitForWitch()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        while (currentTurn != "Witch" && currentTurn != "") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        UIManager.Singleton.endTurnButton.SetActive(true);
        Debug.Log(WaitForSelection());
        currentTurn = "Detective";
        UIManager.Singleton.endTurnButton.SetActive(false);
        WaitForNightEnd();
    }

    private void WaitForWerewolf()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        while (currentTurn != "Werewolf" && currentTurn != "") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        UIManager.Singleton.endTurnButton.SetActive(true);
        Debug.Log(WaitForSelection()) ;
        UIManager.Singleton.endTurnButton.SetActive(false);
        currentTurn = "Witch";
        WaitForNightEnd();
    }
}
