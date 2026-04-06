using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CombatTarget))]
public class UnitHealthBar : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float verticalOffset = 0.12f;
    [SerializeField] private float barWidth = 0.55f;
    [SerializeField] private float barHeight = 0.08f;
    [SerializeField] private float canvasScale = 0.002f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    [SerializeField] private Color healthColorFull = new Color(0.35f, 0.95f, 0.35f, 1f);
    [SerializeField] private Color healthColorMedium = new Color(1f, 0.82f, 0.22f, 1f);
    [SerializeField] private Color healthColorLow = new Color(1f, 0.28f, 0.28f, 1f);

    private CombatTarget combatTarget;
    private SpriteRenderer spriteRenderer;
    private RectTransform canvasRect;
    private Image healthFill;

    private void Awake()
    {
        combatTarget = GetComponent<CombatTarget>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (combatTarget == null)
        {
            enabled = false;
            return;
        }

        CreateUi();
        UpdatePlacement();
        UpdateHealth();
    }

    private void Update()
    {
        if (canvasRect == null || combatTarget == null)
            return;

        UpdatePlacement();
        UpdateHealth();
    }

    private void CreateUi()
    {
        canvasRect = FindOrCreateRect(transform, "UnitHealthCanvas");
        Canvas canvas = canvasRect.GetComponent<Canvas>();
        if (canvas == null)
            canvas = canvasRect.gameObject.AddComponent<Canvas>();

        if (canvasRect.GetComponent<GraphicRaycaster>() != null)
            Destroy(canvasRect.GetComponent<GraphicRaycaster>());

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 260;

        canvasRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        RectTransform backgroundRect = FindOrCreateRect(canvasRect, "Background");
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundRect.GetComponent<Image>();
        if (background == null)
            background = backgroundRect.gameObject.AddComponent<Image>();
        background.color = backgroundColor;

        RectTransform fillRect = FindOrCreateRect(backgroundRect, "Health");
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        healthFill = fillRect.GetComponent<Image>();
        if (healthFill == null)
            healthFill = fillRect.gameObject.AddComponent<Image>();
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFill.color = healthColorFull;
    }

    private void UpdatePlacement()
    {
        float spriteTop = spriteRenderer != null
            ? spriteRenderer.bounds.max.y - transform.position.y
            : 0.3f;

        canvasRect.localPosition = new Vector3(0f, spriteTop + verticalOffset, 0f);
    }

    private void UpdateHealth()
    {
        float healthPercent = combatTarget.MaxHealth > 0f
            ? combatTarget.CurrentHealth / combatTarget.MaxHealth
            : 0f;

        healthPercent = Mathf.Clamp01(healthPercent);
        healthFill.fillAmount = healthPercent;

        if (healthPercent > 0.6f)
            healthFill.color = healthColorFull;
        else if (healthPercent > 0.3f)
            healthFill.color = healthColorMedium;
        else
            healthFill.color = healthColorLow;
    }

    private static RectTransform FindOrCreateRect(Transform parent, string childName)
    {
        GameObject child = parent.Find(childName)?.gameObject;
        if (child == null)
        {
            child = new GameObject(childName);
            child.transform.SetParent(parent, false);
        }

        RectTransform rect = child.GetComponent<RectTransform>();
        if (rect == null)
            rect = child.AddComponent<RectTransform>();

        return rect;
    }
}
