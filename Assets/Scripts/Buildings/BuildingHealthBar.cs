using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a health bar above a building using Unity UI.
/// Automatically creates the health bar UI and updates it based on CombatTarget health.
/// Attach to any building prefab that has a CombatTarget component.
/// </summary>
[RequireComponent(typeof(CombatTarget))]
public class BuildingHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [Tooltip("Additional height above the top of the sprite (in world units)")]
    [SerializeField] private float verticalOffset = 0.15f;

    [Tooltip("Width of the health bar in world units")]
    [SerializeField] private float barWidth = 1.5f;

    [Tooltip("Height of the health bar in world units")]
    [SerializeField] private float barHeight = 0.15f;

    [Tooltip("World-space canvas scale multiplier")]
    [SerializeField] private float canvasScale = 0.0025f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color healthColorFull = Color.green;
    [SerializeField] private Color healthColorMedium = Color.yellow;
    [SerializeField] private Color healthColorLow = Color.red;

    [Header("Visibility")]
    [Tooltip("Show health bar even at full health")]
    [SerializeField] private bool alwaysShow = true;

    [Tooltip("Fade out health bar when not damaged")]
    [SerializeField] private bool fadeWhenFull = false;

    [Tooltip("Time to wait before fading out (seconds)")]
    [SerializeField] private float fadeDelay = 3f;

    private CombatTarget combatTarget;
    private Canvas canvas;
    private SpriteRenderer spriteRenderer;
    private Image backgroundBar;
    private Image healthBar;
    private RectTransform canvasRect;
    private float fadeTimer;
    private bool isDamaged;
    private bool isInitialized;

    private void Awake()
    {
        combatTarget = GetComponent<CombatTarget>();
        if (combatTarget == null)
        {
            Debug.LogWarning($"BuildingHealthBar on {gameObject.name}: Missing CombatTarget component.");
            enabled = false;
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe to damage event
        combatTarget.Damaged += OnTargetDamaged;

        CreateHealthBarUI();
        isInitialized = canvasRect != null;
        UpdateHealthBar();
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateCanvasPlacement();

        // Update health bar
        UpdateHealthBar();

        // Handle fade out logic
        if (fadeWhenFull && !isDamaged)
        {
            fadeTimer += Time.deltaTime;
            if (fadeTimer >= fadeDelay)
            {
                float alpha = Mathf.Lerp(1f, 0f, (fadeTimer - fadeDelay) / 1f);
                SetAlpha(alpha);
            }
        }
    }

    /// <summary>
    /// Call this when the building takes damage to show the health bar.
    /// </summary>
    public void OnDamaged()
    {
        isDamaged = true;
        fadeTimer = 0f;
        SetAlpha(1f);
        UpdateHealthBar();
    }

    private void OnTargetDamaged(CombatTarget target, float damageAmount)
    {
        OnDamaged();
    }

    private void CreateHealthBarUI()
    {
        GameObject canvasObject = transform.Find("HealthBarCanvas")?.gameObject;
        if (canvasObject == null)
        {
            canvasObject = new GameObject("HealthBarCanvas");
            canvasObject.transform.SetParent(transform, false);
        }

        canvasRect = canvasObject.GetComponent<RectTransform>();
        if (canvasRect == null)
            canvasRect = canvasObject.AddComponent<RectTransform>();

        canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = canvasObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 250;
        canvasRect.sizeDelta = new Vector2(barWidth * 100, barHeight * 100);
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        UpdateCanvasPlacement();

        RectTransform backgroundRect = FindOrCreateRect(canvasRect, "Background");
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;

        backgroundBar = backgroundRect.GetComponent<Image>();
        if (backgroundBar == null)
            backgroundBar = backgroundRect.gameObject.AddComponent<Image>();
        backgroundBar.color = backgroundColor;
        backgroundBar.type = Image.Type.Simple;

        RectTransform healthRect = FindOrCreateRect(backgroundRect, "Health");
        healthRect.anchorMin = new Vector2(0, 0);
        healthRect.anchorMax = new Vector2(1, 1);
        healthRect.pivot = new Vector2(0, 0.5f);
        healthRect.sizeDelta = Vector2.zero;

        healthBar = healthRect.GetComponent<Image>();
        if (healthBar == null)
            healthBar = healthRect.gameObject.AddComponent<Image>();
        healthBar.color = healthColorFull;
        healthBar.type = Image.Type.Filled;
        healthBar.fillMethod = Image.FillMethod.Horizontal;
        healthBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBar.fillAmount = 1f;
    }

    private void UpdateCanvasPlacement()
    {
        if (canvasRect == null)
            return;

        float topOffset = spriteRenderer != null
            ? spriteRenderer.bounds.max.y - transform.position.y
            : 0.5f;

        canvasRect.localPosition = new Vector3(0f, topOffset + verticalOffset, 0f);
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || combatTarget == null) return;

        float healthPercent = combatTarget.CurrentHealth / combatTarget.MaxHealth;
        healthBar.fillAmount = Mathf.Clamp01(healthPercent);

        // Update color based on health percentage
        if (healthPercent > 0.6f)
        {
            healthBar.color = healthColorFull;
        }
        else if (healthPercent > 0.3f)
        {
            healthBar.color = healthColorMedium;
        }
        else
        {
            healthBar.color = healthColorLow;
        }

        // Handle visibility
        if (!alwaysShow && healthPercent >= 1f && !isDamaged)
        {
            SetAlpha(0f);
        }
        else if (alwaysShow || isDamaged)
        {
            SetAlpha(1f);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (canvasRect == null) return;

        Color bgColor = backgroundColor;
        bgColor.a = alpha;
        if (backgroundBar != null)
            backgroundBar.color = bgColor;

        if (healthBar != null)
        {
            Color healthColor = healthBar.color;
            healthColor.a = alpha;
            healthBar.color = healthColor;
        }
    }

    private void OnValidate()
    {
        barWidth = Mathf.Max(0.1f, barWidth);
        barHeight = Mathf.Max(0.05f, barHeight);
        verticalOffset = Mathf.Max(0f, verticalOffset);
        canvasScale = Mathf.Max(0.001f, canvasScale);
        fadeDelay = Mathf.Max(0f, fadeDelay);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (combatTarget != null)
        {
            combatTarget.Damaged -= OnTargetDamaged;
        }

    }

    private static RectTransform FindOrCreateRect(Transform parent, string childName)
    {
        GameObject childObject = parent.Find(childName)?.gameObject;
        if (childObject == null)
        {
            childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
        }

        RectTransform rect = childObject.GetComponent<RectTransform>();
        if (rect == null)
            rect = childObject.AddComponent<RectTransform>();

        return rect;
    }
}
