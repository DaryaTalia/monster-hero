using UnityEngine;
using UnityEngine.InputSystem;

public class Monster : MonoBehaviour
{
    [Header("Self")]
    Animator animator;
    AudioSource audioSource;
    int health = 3;
    [SerializeField]
    int defaultHealth = 3;
    float stamina = 100;
    [SerializeField]
    float defaultStamina = 100;
    [SerializeField]
    float staminaDecay = 10f;
    [SerializeField]
    float staminaRecovery = 20f;

    [Header("Movement")]
    Rigidbody2D rb;
    float speed = 1.0f;
    [SerializeField]
    float defaultSpeed = 1.0f;
    [SerializeField]
    float sprintMultiplier = 3f;
    [SerializeField]
    bool sprinting;
    Vector3 movementVector;

    [Header("Interaction")]
    bool canScreech;
    bool canInteract;
    bool hidden;

    [SerializeField]
    float screechRadius = 10f;
    float screechCooldown;
    float screechBuffer = 7f;

    float interactCooldown;
    float interactBuffer = 1f;

    [SerializeField]
    float interactionRadius = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        health = defaultHealth;
        speed = defaultSpeed;
        stamina = defaultStamina;
    }

    void Update()
    {
        if (GameManager.instance.CheckGamePlaying())
        {
            #region Sprinting
            // Sprinting
            if (sprinting && stamina <= 0)
            {
                StopSprint();
            } 
            else if (sprinting)
            {
                stamina = Mathf.Clamp(stamina - (staminaDecay * Time.deltaTime), 0, defaultStamina);
            }
            else
            {
                stamina = Mathf.Clamp(stamina + (staminaRecovery * Time.deltaTime), 0, defaultStamina);
            }
            #endregion

            // Screech
            if(screechCooldown > 0)
            {
                screechCooldown -= 1 * Time.deltaTime;
            }

            // Interact
            if(interactCooldown > 0)
            {
                interactCooldown -= 1 * Time.deltaTime;
            }

            // Health
            if(health <= 0)
            {
                if(GameManager.instance.CheckTownieCount() == 0)
                {
                    GameManager.instance.UpdateGameCondition(GameManager.GameState.savedAll);
                }
                else if(GameManager.instance.CheckTownieCount() == GameManager.instance.DefaultPopulation)
                {
                    GameManager.instance.UpdateGameCondition(GameManager.GameState.savedNone);
                }
                else
                {
                    GameManager.instance.UpdateGameCondition(GameManager.GameState.savedSome);
                }
            }

        }
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.CheckGamePlaying() && !hidden)
        {
            rb.AddForce(movementVector * speed);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying())
        {
            Vector2 input = context.ReadValue<Vector2>();
            movementVector = new Vector3(input.x, input.y, 0);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying() && context.performed)
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);

            foreach (var collider in colliders)
            {
                Debug.Log($"{collider.gameObject.name} is nearby");
                if (collider.GetComponent<HidePoint>())
                {
                    Hide();

                    interactCooldown = interactBuffer;
                    return;
                }
                else if (collider.GetComponent<ScarePoint>())
                {
                    collider.GetComponent<ScarePoint>().Activate();
                    Debug.Log("Activate Scare");

                    interactCooldown = interactBuffer;
                    return;
                }
            }
        }
    }

    void Hide()
    {
        if (hidden)
        {
            hidden = false;
            animator.SetBool("hidden", false);
            Debug.Log("Visible");
        }
        else
        {
            hidden = true;
            animator.SetBool("hidden", true);
            Debug.Log("Hidden");
        }
    }

    public void Screech(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying() && context.performed)
        {
            if(screechCooldown <= 0)
            {
                animator.SetTrigger("screech");
                audioSource.Play();

                Debug.Log("Screech!!!!");

                var colliders = Physics2D.OverlapCircleAll(transform.position, screechRadius);

                foreach (var collider in colliders)
                {
                    if (collider.GetComponent<Townie>())
                    {
                        collider.GetComponent<Townie>().Lure(transform.position);
                    }
                }

                screechCooldown = screechBuffer;
            }
        }
    }

    public void StartSprint(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying() && context.performed)
        {
            if (!sprinting && stamina > 0)
            {
                speed *= sprintMultiplier;
                sprinting = true;
            }
        }
    }

    public void StopSprint(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying() && context.performed)
        {
            speed = defaultSpeed;
            sprinting = false;
        }
    }

    public void StopSprint()
    {
        if (GameManager.instance.CheckGamePlaying())
        {
            speed = defaultSpeed;
            sprinting = false;
        }
    }

}