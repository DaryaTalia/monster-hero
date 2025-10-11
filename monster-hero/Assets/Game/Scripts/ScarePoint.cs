using UnityEngine;

public class ScarePoint : MonoBehaviour
{
    bool scareActive;
    float scareDuration = 15f;
    float countdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        if (!scareActive && countdown <= 0)
        {
            scareActive = true;
            countdown = scareDuration;
        }
    }
}
