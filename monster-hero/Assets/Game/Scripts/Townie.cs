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

    // Idle Var
    [SerializeField]
    float idleDuration = 5f;

    // Roaming Var
    [SerializeField]
    PatrolPoint[] patrolRoute;
    int nextPosition;

    // Lured Var
    [SerializeField]
    float luredDuration = 4f;

    // Attacking Var
    [SerializeField]
    float attackDuration = 5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
                    break;
                }

            default:
                {
                    break;
                }
        }
    }

    void Idle()
    {
        if(cooldown <= 0)
        {            
            currentState = TownieState.roaming;
            patrolRoute[nextPosition].isOccupied = false;
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
            UpdateRoamPosition();
            cooldown = idleDuration;
            currentState = TownieState.idle;
            patrolRoute[nextPosition].isOccupied = true;
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
                currentState = TownieState.roaming;
            }
            else
            {
                cooldown -= Time.deltaTime;
            }

            UpdateRoamPosition();
            currentState = TownieState.roaming;
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
            currentState = TownieState.roaming;
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
            if (collider.GetComponent<Monster>() && !collider.GetComponent<Monster>().Hidden)
            {
                TriggerAttack();
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    void UpdateRoamPosition()
    {
        if (nextPosition == patrolRoute.Length)
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
        cooldown = luredDuration;
    }

    public void TriggerFlee()
    {
        UpdateState(TownieState.fleeing);
        walkTowards = GameManager.instance.FleePoint.position;
    }

    public void TriggerAttack()
    {
        UpdateState(TownieState.attacking);
        walkTowards = GameManager.instance.Monster.transform.position;
    }


}
