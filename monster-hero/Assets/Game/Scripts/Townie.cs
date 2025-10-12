using UnityEngine;
using UnityEngine.AI;

public class Townie : MonoBehaviour
{
    public enum TownieState { idle, roaming, lured, fleeing, attacking };
    public TownieState currentState;

    NavMeshAgent agent;

    Vector3 walkTowards;

    [SerializeField]
    float distanceThreshold = 2f;
    [SerializeField]
    float sightRange = 3f;

    float cooldown;

    // Roaming Var
    [SerializeField]
    PatrolPoint[] patrolRoute;
    int nextPosition;

    // Idle Var
    [SerializeField]
    float idleDuration = 5f;

    // Lured Var
    [SerializeField]
    float luredDuration = 4f;

    // Attacking Var
    [SerializeField]
    float attackDuration = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.CheckGamePlaying())
        {
            CheckForMonster();
            TownieMachine();
        }
    }

    void TownieMachine()
    {
        switch (currentState)
        {
            case TownieState.idle:
                {
                    Idle();
                    break;
                }
            case TownieState.roaming:
                {
                    Roam();
                    break;
                }
            case TownieState.lured:
                {
                    Lure();
                    break;
                }
            case TownieState.fleeing:
                {
                    Flee();
                    break;
                }
            case TownieState.attacking:
                {
                    Attack();
                    break;
                }
        }
    }

    void MoveToPosition()
    {
        if (agent.isOnNavMesh)
        {
            do
            {
                agent.SetDestination(walkTowards);
            } 
        
            while (Vector3.Distance(walkTowards, transform.position) <= distanceThreshold);
        }        
    }

    void UpdateState(TownieState newState)
    {
        switch (newState)
        {
            case TownieState.idle:
                {
                    currentState = TownieState.idle;

                    cooldown = idleDuration;
                    patrolRoute[nextPosition].isOccupied = true;
                    break;
                }
            case TownieState.roaming:
                {
                    currentState = TownieState.roaming;

                    patrolRoute[nextPosition].isOccupied = false;
                    UpdateRoamPosition();
                    break;
                }
            case TownieState.lured:
                {
                    currentState = TownieState.lured;

                    cooldown = luredDuration;
                    break;
                }
            case TownieState.fleeing:
                {
                    currentState = TownieState.fleeing;

                    walkTowards = GameManager.instance.FleePoint.position;
                    break;
                }
            case TownieState.attacking:
                {
                    currentState = TownieState.attacking;

                    walkTowards = GameManager.instance.Monster.transform.position;
                    break;
                }
        }
    }

    void Idle()
    {
        if(cooldown <= 0)
        {            
            UpdateState(TownieState.roaming);
        }
        else
        {
            cooldown -= Time.deltaTime;
        }
    }

    void Roam()
    {
        if(Vector3.Distance(transform.position, walkTowards) <= distanceThreshold)
        {
            UpdateState(TownieState.idle);
        }
        else
        {
            MoveToPosition();
        }
    }

    void Lure()
    {
        if (Vector3.Distance(transform.position, walkTowards) <= distanceThreshold)
        {
            if (cooldown <= 0)
            {
                UpdateState(TownieState.roaming);
            }
            else
            {
                cooldown -= Time.deltaTime;
            }
        }
        else
        {
            MoveToPosition();
        }
    }

    void Flee()
    {
        if (Vector3.Distance(transform.position, walkTowards) <= distanceThreshold)
        {
            GameManager.instance.Townies.Remove(this);
            Destroy(gameObject);
        }
        else
        {
            MoveToPosition();
        }
    }

    void Attack()
    {
        if (!CheckForMonster())
        {
            UpdateState(TownieState.roaming);
        }
        else if (Vector3.Distance(transform.position, walkTowards) <= distanceThreshold)
        {
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
            } 
            else
            {
                GameManager.instance.Monster.Hit();
                cooldown = attackDuration;
            }
        }
        else
        {
            MoveToPosition();
        }
    }


    bool CheckForMonster()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, sightRange);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Monster>() && !collider.GetComponent<Monster>().Hidden)
            {
                TriggerAttack();
                return true;
            }
        }
        return false;
    }

    void UpdateRoamPosition()
    {
        if (nextPosition + 1 == patrolRoute.Length)
        {
            nextPosition = 0;
        }
        else
        {
            nextPosition++;
        }

        if (patrolRoute[nextPosition].isOccupied) { 
            UpdateRoamPosition();
        }

        walkTowards = patrolRoute[nextPosition].transform.position;
    }

    public void TriggerLure(Vector3 target)
    {
        UpdateState(TownieState.lured);
        walkTowards = target;
    }

    public void TriggerFlee()
    {
        UpdateState(TownieState.fleeing);
    }

    public void TriggerAttack()
    {
        UpdateState(TownieState.attacking);
    }


}
