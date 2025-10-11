using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { mainmenu, pause, credits, gameplay, savedAll, savedSome, savedNone };
    public GameState currentState;
    public GameState lastState;

    Townie[] townies;
    [SerializeField]
    int defaultPopulation = 20;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int DefaultPopulation
    {
        get { return defaultPopulation; }
    }

    public int CheckTownieCount()
    {
        return townies.Length;
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
        switch (condition)
        {
            case GameState.mainmenu:
                {
                    break;
                }
            case GameState.pause:
                {
                    break;
                }
            case GameState.credits:
                {
                    break;
                }
            case GameState.gameplay:
                {
                    break;
                }
            case GameState.savedAll:
                {
                    break;
                }
            case GameState.savedSome:
                {
                    break;
                }
            case GameState.savedNone:
                {
                    break;
                }

            default:
                {
                    break;
                }
        }
    }
}
