using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public CinemachineVirtualCamera vCam;


    [Serializable]
    public enum GameState { mainmenu = 0, pause, howtoplay, credits, gameplay, savedAll, savedSome, savedNone, quit };
    public GameState currentState;

    [SerializeField]
    float gameLimit = 420;
    float gameLimitTimer = 0;

    [Header("Monster")]

    Monster monster;
    bool monsterDead;
    Vector3 monsterSpawn;

    [Header("Townies")]

    [SerializeField]
    GameObject environmentPrefab; // Townies + SpawnPoints
    List<Townie> townies;
    [SerializeField]
    int population;
    Transform fleePoint;

    #region UI

    #region Menus

    [Header("Menus")]

    [SerializeField]
    string gameName;

    [SerializeField]
    GameObject mainMenuPanel;
    [SerializeField]
    TextMeshProUGUI menuTitle;
    [SerializeField]
    Button menuBackButton;
    [SerializeField]
    GameObject howToPlayPanel;
    [SerializeField]
    GameObject creditsPanel;
    [SerializeField]
    GameObject gameEndScreen;
    [SerializeField]
    TextMeshProUGUI gameEndTitle;
    [SerializeField]
    TextMeshProUGUI gameEndText;

    [Header("Game Conditions")]

    [SerializeField]
    string savedAllTitleText;
    [SerializeField]
    [TextArea(4,7)]
    string savedAllBodyText;

    [SerializeField]
    string savedSomeTitleText;
    [SerializeField]
    [TextArea(4, 7)]
    string savedSomeBodyText;

    [SerializeField]
    string savedNoneTitleText;
    [SerializeField]
    [TextArea(4, 7)]
    string savedNoneBodyText;
    #endregion

    #region HUD

    [Header("HUD")]

    [SerializeField]
    GameObject gameplayHUD;

    [SerializeField]
    TextMeshProUGUI towniesRemainingText;
    [SerializeField]
    TextMeshProUGUI gameTimerText;

    [SerializeField]
    Image healthFill;

    [SerializeField]
    Image staminaFill;

    [SerializeField]
    GameObject visibleImage;
    [SerializeField]
    GameObject hiddenImage;

    #endregion

    #endregion

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        monster = GameObject.FindGameObjectWithTag("Monster").GetComponent<Monster>();
        monsterSpawn = monster.transform.position;
        monster.StartMonster();

        ResetGame();

        UpdateGameCondition(currentState);
    }

    void Update()
    {
        if (CheckGamePlaying()) 
        {
            CheckGameConditions();
            monster.UpdateMonster();
            foreach(Townie t in townies)
            {
                t.UpdateTownie();
            }
            UpdateHUD();
        }
    }

    private void FixedUpdate()
    {
        Monster.MonsterFixedUpdate();
    }

    public Monster Monster
    {
        get { return monster; }
    }

    public bool MonsterDead
    {
        set { monsterDead = value; }
    }

    public List<Townie> Townies
    {
        get {  return townies; }
    }

    public int Population
    {
        get { return population; }
    }

    public Transform FleePoint
    {
        get { return fleePoint; }
    }

    public void ResetGame()
    {
        // Reset Monster

        monster.ResetMonster();
        monster.transform.position = monsterSpawn;

        vCam.ForceCameraPosition(monsterSpawn, Quaternion.identity);

        // Reset Environment

        fleePoint = GameObject.FindGameObjectWithTag("FleePoint").transform;

        GameObject[] oldEnvironments = GameObject.FindGameObjectsWithTag("Environment");

        foreach(GameObject go in oldEnvironments)
        {
            Destroy(go);
        }

        GameObject[] scarePoints = GameObject.FindGameObjectsWithTag("ScarePoint");

        foreach(GameObject go in scarePoints)
        {
            go.GetComponent<ScarePoint>().ResetScarePoint();
        }

        GameObject[] patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");

        foreach(GameObject go in patrolPoints)
        {
            go.GetComponent<PatrolPoint>().isOccupied = false;
        }

        Instantiate(environmentPrefab);

        // Reset Townies

        population = 0;

        if(townies != null)
        {
            townies.Clear();
        }
        else
        {
            townies = new List<Townie>();
        }

        GameObject[] t = GameObject.FindGameObjectsWithTag("Townie");
        foreach (GameObject _t in t)
        {
            townies.Add(_t.GetComponent<Townie>());
            _t.GetComponent<Townie>().StartTownie();
        }
        population = townies.Count;

        gameLimitTimer = gameLimit;

        UpdateGameCondition(GameState.mainmenu);
    }

    public int CheckTownieCount()
    {
        return townies.Count;
    }

    public bool CheckGamePlaying()
    {
        if(instance != null && currentState == GameState.gameplay)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public void UpdateGameCondition(GameState condition)
    {
        currentState = condition;

        switch (condition)
        {
            case GameState.mainmenu:
                {
                    Time.timeScale = 0;

                    mainMenuPanel.SetActive(true);
                    howToPlayPanel.SetActive(false);
                    creditsPanel.SetActive(false);
                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(false);

                    menuBackButton.gameObject.SetActive(true);

                    menuBackButton.gameObject.GetComponent<GameStateComponent>().GameState = GameState.mainmenu;

                    menuBackButton.onClick.RemoveListener(() => ResetGame());
                    menuBackButton.gameObject.SetActive(false);

                    menuTitle.text = gameName;
                    break;
                }
            case GameState.pause:
                {
                    Time.timeScale = 0;

                    mainMenuPanel.SetActive(true);
                    howToPlayPanel.SetActive(false);
                    creditsPanel.SetActive(false);
                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(false);

                    menuBackButton.gameObject.SetActive(true);

                    menuBackButton.gameObject.GetComponent<GameStateComponent>().GameState = GameState.pause;

                    menuBackButton.gameObject.SetActive(false);

                    menuTitle.text = "Paused";
                    break;
                }
            case GameState.howtoplay:
                {
                    mainMenuPanel.SetActive(true);
                    howToPlayPanel.SetActive(true);

                    menuBackButton.gameObject.SetActive(true);

                    break;
                }
            case GameState.credits:
                {
                    mainMenuPanel.SetActive(true);
                    creditsPanel.SetActive(true);

                    menuBackButton.gameObject.SetActive(true);

                    break;
                }
            case GameState.gameplay:
                {
                    Time.timeScale = 1;

                    mainMenuPanel.SetActive(false);
                    gameplayHUD.SetActive(true);

                    visibleImage.SetActive(true);
                    hiddenImage.SetActive(false);

                    menuBackButton.gameObject.SetActive(false);

                    break;
                }
            case GameState.savedAll:
                {
                    Time.timeScale = 0;

                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(true);

                    gameEndTitle.text = savedAllTitleText;
                    gameEndText.text = savedAllBodyText;

                    menuBackButton.gameObject.SetActive(true);

                    menuBackButton.gameObject.GetComponent<GameStateComponent>().GameState = GameState.mainmenu;
                    menuBackButton.onClick.AddListener(() => ResetGame());

                    GameObject[] oldEnvironments = GameObject.FindGameObjectsWithTag("Environment");

                    foreach (GameObject go in oldEnvironments)
                    {
                        Destroy(go);
                    }

                    population = 0;
                    townies.Clear();

                    break;
                }
            case GameState.savedSome:
                {
                    Time.timeScale = 0;

                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(true);

                    gameEndTitle.text = savedSomeTitleText;
                    gameEndText.text = savedSomeBodyText;

                    menuBackButton.gameObject.SetActive(true);

                    menuBackButton.gameObject.GetComponent<GameStateComponent>().GameState = GameState.mainmenu;
                    menuBackButton.onClick.AddListener(() => ResetGame());

                    GameObject[] oldEnvironments = GameObject.FindGameObjectsWithTag("Environment");

                    foreach (GameObject go in oldEnvironments)
                    {
                        Destroy(go);
                    }

                    population = 0;
                    townies.Clear();

                    break;
                }
            case GameState.savedNone:
                {
                    Time.timeScale = 0;

                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(true);

                    gameEndTitle.text = savedNoneTitleText;
                    gameEndText.text = savedNoneBodyText;

                    menuBackButton.gameObject.SetActive(true);

                    menuBackButton.gameObject.GetComponent<GameStateComponent>().GameState = GameState.mainmenu;
                    menuBackButton.onClick.AddListener(() => ResetGame());

                    GameObject[] oldEnvironments = GameObject.FindGameObjectsWithTag("Environment");

                    foreach (GameObject go in oldEnvironments)
                    {
                        Destroy(go);
                    }

                    population = 0;
                    townies.Clear();

                    break;
                }
            case GameState.quit:
                {
                    Time.timeScale = 0;

                    mainMenuPanel.SetActive(true);
                    howToPlayPanel.SetActive(false);
                    creditsPanel.SetActive(false);
                    gameplayHUD.SetActive(false);
                    gameEndScreen.SetActive(false);

                    menuTitle.text = gameName;

                    Application.Quit();

                    break;
                }
        }
    }

    void CheckGameConditions()
    {
        if(gameLimitTimer <= 0 || monsterDead || CheckTownieCount() == 0)
        {
            if (CheckTownieCount() == 0)
            {
                UpdateGameCondition(GameState.savedAll);
            }
            else if (CheckTownieCount() == Population)
            {
                UpdateGameCondition(GameState.savedNone);
            }
            else
            {
                UpdateGameCondition(GameState.savedSome);
            }
        }
        else
        {
            gameLimitTimer -= Time.deltaTime;
        }
    }

    void UpdateHUD()
    {
        towniesRemainingText.text = "Townies Remaining: " + townies.Count;

        int timer = (int)gameLimitTimer;
        gameTimerText.text = "Time: " + timer;

        if (monster.Hidden)
        {
            visibleImage.SetActive(false);
            hiddenImage.SetActive(true);
        } 
        else
        {
            visibleImage.SetActive(true);
            hiddenImage.SetActive(false);
        }

        healthFill.fillAmount = monster.Health / monster.MaxHealth;
        staminaFill.fillAmount = monster.Stamina / monster.MaxStamina;
    }
}
