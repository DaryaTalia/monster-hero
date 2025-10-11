using UnityEngine;

public class Townie : MonoBehaviour
{
    public enum TownieState { idle, roaming, lured, fleeing, attacking };
    public TownieState currentState;
    Vector3 walkTowards;

    float movementSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.CheckGamePlaying())
        {

        }
    }

    void UpdateState(TownieState newState)
    {
        switch (newState)
        {
            case TownieState.idle:
                {
                    currentState = TownieState.idle;
                    break;
                }

            default:
                {
                    break;
                }
        }
    }

    public void Lure(Vector3 target)
    {
        UpdateState(TownieState.lured);
        walkTowards = target;
    }
}
