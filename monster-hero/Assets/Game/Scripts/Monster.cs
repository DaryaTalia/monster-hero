using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Monster : MonoBehaviour
{
    public static Monster instance;

    [Header("Self")]
    Animator animator;
    AudioSource audioSource;
    int health = 3;
    [SerializeField]
    int maxHealth = 3;
    float stamina = 100;
    [SerializeField]
    float maxStamina = 100;
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
    bool hidden;

    [SerializeField]
    float screechRadius = 10f;
    float screechCooldown;
    float screechBuffer = 7f;

    float interactCooldown;
    float interactBuffer = 1f;

    [SerializeField]
    float interactionRadius = 2f;

    [Header("World Space UI")]

    [SerializeField]
    GameObject screechText;
    [SerializeField]
    GameObject hurtText;
    [SerializeField]
    GameObject hideText;

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
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        health = maxHealth;
        speed = defaultSpeed;

        screechText.SetActive(false);
        hurtText.SetActive(false);
        hideText.SetActive(false);

        stamina = maxStamina;
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
                stamina = Mathf.Clamp(stamina - (staminaDecay * Time.deltaTime), 0, maxStamina);
            }
            else
            {
                stamina = Mathf.Clamp(stamina + (staminaRecovery * Time.deltaTime), 0, maxStamina);
            }
            #endregion

            // Screech
            if (screechCooldown > 0)
            {
                screechCooldown -= Time.deltaTime;
            }

            // Interact
            if(interactCooldown > 0)
            {
                interactCooldown -= Time.deltaTime;
            }

            // Health
            if(health <= 0)
            {
                GameManager.instance.MonsterDead = true;
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

    public float Health
    {
        get { return  health; }
    }

    public float MaxHealth
    {
        get { return  maxHealth; }
    }

    public float Stamina
    {
        get { return  stamina; }
    }

    public float MaxStamina
    {
        get { return  maxStamina; }
    }

    public float ScreechCooldown
    {
        get { return screechCooldown; }
    }

    public float ScreechBuffer
    {
        get { return screechBuffer; }
    }

    public float InteractCooldown
    {
        get { return interactCooldown; }
    }

    public float InteractBuffer
    {
        get { return interactBuffer; }
    }


    public bool Hidden
    {
        get { return hidden; }
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
                if (collider.gameObject.GetComponent<HidePoint>())
                {
                    Hide();

                    interactCooldown = interactBuffer;
                    return;
                }
                else if (collider.gameObject.GetComponent<ScarePoint>())
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
            StartCoroutine(HideTextTimer());
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
                StartCoroutine(ScreechTextTimer());

                Debug.Log("Screech!!!!");

                var colliders = Physics2D.OverlapCircleAll(transform.position, screechRadius);

                foreach (var collider in colliders)
                {
                    if (collider.gameObject.GetComponent<Townie>())
                    {
                        collider.GetComponent<Townie>().TriggerLure(transform.position);
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


    public void Pause(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CheckGamePlaying() && context.performed)
        {
            GameManager.instance.UpdateGameCondition(GameManager.GameState.pause);
        }
    }

    public void Quit(InputAction.CallbackContext context)
    {
        GameManager.instance.UpdateGameCondition(GameManager.GameState.mainmenu);
        Application.Quit();
    }

    public void Hit()
    {
        health--;
        StartCoroutine(HitTextTimer());
    }

    IEnumerator HitTextTimer()
    {
        hurtText.SetActive(true);
        yield return new WaitForSeconds(3);
        hurtText.SetActive(false);
    }

    IEnumerator ScreechTextTimer()
    {
        screechText.SetActive(true);
        yield return new WaitForSeconds(3);
        screechText.SetActive(false);
    }

    IEnumerator HideTextTimer()
    {
        hideText.SetActive(true);
        yield return new WaitForSeconds(3);
        hideText.SetActive(false);
    }
}