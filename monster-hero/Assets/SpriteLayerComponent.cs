using UnityEngine;

public class SpriteLayerComponent : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField]
    bool isStatic;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSortingOrder();
    }

    private void Update()
    {
        if(!isStatic)
        {
            UpdateSortingOrder();
        }
    }

    void UpdateSortingOrder()
    {
        spriteRenderer.sortingOrder = (int)transform.position.y;
    }
}
