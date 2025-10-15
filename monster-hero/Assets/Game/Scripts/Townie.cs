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

    float movementSpeed = 2f;
    [SerializeField]
    float defaultMovementSpeed = 2f;
    [SerializeField]
    float lureSpeedModifier = .7f;
    [SerializeField]
    float fleeSpeedModifier = 3f;
    [SerializeField]
    float attackSpeedModifier = 1.5f;

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

    [Header("Audio")]
    AudioSource townieAudio;
    [SerializeField]
    AudioClip[] grassSteps;
    [SerializeField]
    AudioClip[] stoneSteps;

    int footstepIndex;

    [SerializeField]
    float footstepSpeed = 1.5f;
    float footstepCounter;

    [SerializeField]
    float minPitchDeviation = .5f;
    [SerializeField]
    float maxPitchDeviation = .5f;
    float defaultPitch;

    [SerializeField]
    LayerMask grassLayer;
    [SerializeField]
    LayerMask stoneLayer;

    public void StartTownie()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = distanceThreshold;

        movementSpeed = defaultMovementSpeed;
        agent.speed = movementSpeed;

        townieAudio = GetComponent<AudioSource>();
        defaultPitch = townieAudio.pitch;
    }

    public void UpdateTownie()
    {
        if (GameManager.instance.CheckGamePlaying())
        {
            CheckForMonster();
            TownieMachine();
            CalculateFootsteps();
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

                    movementSpeed = defaultMovementSpeed;
                    agent.speed = movementSpeed;

                    patrolRoute[nextPosition].isOccupied = false;
                    UpdateRoamPosition();
                    break;
                }
            case TownieState.lured:
                {
                    currentState = TownieState.lured;

                    movementSpeed = defaultMovementSpeed * lureSpeedModifier;
                    agent.speed = movementSpeed;

                    cooldown = luredDuration;
                    break;
                }
            case TownieState.fleeing:
                {
                    currentState = TownieState.fleeing;

                    movementSpeed = defaultMovementSpeed * fleeSpeedModifier;
                    agent.speed = movementSpeed;

                    walkTowards = GameManager.instance.FleePoint.position;
                    break;
                }
            case TownieState.attacking:
                {
                    currentState = TownieState.attacking;

                    movementSpeed = defaultMovementSpeed * attackSpeedModifier;
                    agent.speed = movementSpeed;

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
            if (collider.gameObject.GetComponent<Monster>() && !collider.GetComponent<Monster>().Hidden && currentState != TownieState.fleeing)
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
        if(currentState != TownieState.fleeing)
        {
            UpdateState(TownieState.lured);
            walkTowards = target;
        }
    }

    public void TriggerFlee()
    {
        if (currentState != TownieState.fleeing)
        {
            UpdateState(TownieState.fleeing);
        }
    }

    public void TriggerAttack()
    {
        if (currentState != TownieState.fleeing)
        {
            UpdateState(TownieState.attacking);
        }
    }

    void CalculateFootsteps()
    {
        if (GameManager.instance.CheckGamePlaying() && currentState != TownieState.idle) //or use rb velocity > 0
        {
            if (footstepCounter >= footstepSpeed)
            {
                // Footstep Audio
                footstepCounter = 0;

                var colliders = Physics2D.OverlapCircleAll(transform.position, 1);

                foreach (var collider in colliders)
                {
                    if (collider.gameObject.layer == Mathf.Log(grassLayer, 2))
                    {
                        if (footstepIndex + 1 >= grassSteps.Length)
                        {
                            footstepIndex = 0;
                        }
                        else
                        {
                            footstepIndex++;
                        }

                        townieAudio.Stop();
                        townieAudio.clip = grassSteps[footstepIndex];
                        RandomizePitch();
                        townieAudio.Play();
                    }
                    else if (collider.gameObject.layer == Mathf.Log(stoneLayer, 2))
                    {
                        if (footstepIndex + 1 >= stoneSteps.Length)
                        {
                            footstepIndex = 0;
                        }
                        else
                        {
                            footstepIndex++;
                        }

                        townieAudio.resource = stoneSteps[footstepIndex];
                        RandomizePitch();
                        townieAudio.Play();
                    }
                }                
            }
            else
            {
                footstepCounter += Time.deltaTime;
            }
        }
    }

    void RandomizePitch()
    {
        townieAudio.pitch = Random.Range(-minPitchDeviation, maxPitchDeviation) + defaultPitch;
    }


}
