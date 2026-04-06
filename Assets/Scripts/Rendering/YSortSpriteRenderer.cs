using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class YSortSpriteRenderer : MonoBehaviour
{
    [SerializeField] private int sortingOrderOffset;
    [SerializeField] private float yPositionOffset;
    [SerializeField] private int unitsPerOrder = 100;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        CacheRenderer();
        ApplySorting();
    }

    private void OnEnable()
    {
        CacheRenderer();
        ApplySorting();
    }

    private void LateUpdate()
    {
        ApplySorting();
    }

    private void OnValidate()
    {
        if (unitsPerOrder < 1)
            unitsPerOrder = 1;

        CacheRenderer();
        ApplySorting();
    }

    private void CacheRenderer()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void ApplySorting()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;

        float y = transform.position.y + yPositionOffset;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-y * unitsPerOrder) + sortingOrderOffset;
    }
}
