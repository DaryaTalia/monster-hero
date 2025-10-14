using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScarePoint : MonoBehaviour
{
    bool scareActive;
    [SerializeField]
    float scareDuration = 15f;
    float countdown;
    [SerializeField]
    float scareRadius = 5f;

    [SerializeField]
    GameObject cooldownGO;
    [SerializeField]
    TextMeshProUGUI cooldownText;

    private void Start()
    {
        ResetScarePoint();
    }

    void Update()
    {
        if (cooldownGO.activeInHierarchy)
        {
            if (countdown > 0)
            {
                countdown -= Time.deltaTime;
                cooldownText.text = (int)countdown + "";
            }
            else
            {
                cooldownGO.SetActive(false);
                scareActive = false;
            }
        }
            
    }

    public void ResetScarePoint()
    {
        cooldownGO.SetActive(false);
        scareActive = false;
        countdown = 0;
    }

    public void Activate()
    {
        if (!scareActive && countdown <= 0)
        {
            scareActive = true;
            countdown = scareDuration;
            cooldownGO.SetActive(true);

            var colliders = Physics2D.OverlapCircleAll(transform.position, scareRadius);

            foreach (var collider in colliders)
            {
                if (collider.gameObject.GetComponent<Townie>())
                {
                    collider.gameObject.GetComponent<Townie>().TriggerFlee();
                }
            }
        }
    }
}
