using UnityEngine;

public class GameStateComponent : MonoBehaviour
{
    public GameManager.GameState GameState;

    public void OnGameState()
    {
        GameManager.instance.UpdateGameCondition(GameState);
    }
}
