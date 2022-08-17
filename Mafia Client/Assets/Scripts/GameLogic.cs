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

    bool isNight;
    string currentTurn;

    public Player selectedPlayer;

    public bool healingSelected;
    public bool poisonSelected;

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
    }

    public void EndTurn()
    {
        switch (currentTurn)
        {
            case "Detective":
                currentTurn = "";
                bool isBad = false;
                switch (selectedPlayer.role.name)
                {
                    case "Werewolf":
                        isBad = true;
                        break;
                    case "Werewolf King": 
                        isBad = true; 
                        break;
                    default: 
                        isBad = false;
                        break;
                }
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
        while (isNight);
        UIManager.Singleton.asleepUI.SetActive(false);
    }

    private void WaitForDetective()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        while (currentTurn != "Detective") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        while (currentTurn == "Detective") ;
        currentTurn = "";
        isNight = false;
        WaitForNightEnd();
    }

    private void WaitForWitch()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        while (currentTurn != "Witch") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        while (currentTurn == "Witch")
        {
            //Do witch stuff
        }
        currentTurn = "Detective";
        WaitForNightEnd();
    }

    private void WaitForWerewolf()
    {
        UIManager.Singleton.asleepUI.SetActive(true);
        while (currentTurn != "Werewolf") ;
        UIManager.Singleton.asleepUI.SetActive(false);
        while (currentTurn == "Werewolf")
        {
            //Do werewolf stuff
        }
        currentTurn = "Witch";
        WaitForNightEnd();
    }
}
