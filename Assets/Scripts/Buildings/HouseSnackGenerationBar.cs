using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a snack generation progress bar above houses.
/// Tracks the SnackGenerator cycle and shows team-colored fill progress.
/// </summary>
[RequireComponent(typeof(SnackGenerator))]
public class HouseSnackGenerationBar : MonoBehaviour
{
    [Header("Bar Layout")]
    [SerializeField] private float verticalOffset = 0.32f;
    [SerializeField] private float barWidth = 1.5f;
    [SerializeField] private float barHeight = 0.1f;
    [SerializeField] private float canvasScale = 0.0025f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    [SerializeField] private Color playerFillColor = new Color(0.35f, 0.8f, 1f, 1f);
    [SerializeField] private Color enemyFillColor = new Color(1f, 0.45f, 0.35f, 1f);
    [SerializeField] private Color neutralFillColor = new Color(1f, 0.8f, 0.25f, 1f);

    [Header("Visibility")]
    [SerializeField] private bool alwaysShow = true;
    [SerializeField] private bool pulseOnGeneration = true;
    [SerializeField] private float pulseDuration = 0.35f;

    private SnackGenerator snackGenerator;
    private Canvas canvas;
    private SpriteRenderer spriteRenderer;
    private Image backgroundBar;
    private Image progressBar;
    private Slider progressSlider;
    private RectTransform canvasRect;
    private float pulseTimer;
    private bool isInitialized;

    private void Awake()
    {
        snackGenerator = GetComponent<SnackGenerator>();
        if (snackGenerator == null)
        {
            Debug.LogWarning($"HouseSnackGenerationBar on {gameObject.name}: Missing SnackGenerator component.");
            enabled = false;
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        snackGenerator.SnacksGenerated += OnSnacksGenerated;
        CreateBarUI();
        isInitialized = canvasRect != null;
        UpdateBar();
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        UpdateCanvasPlacement();
        UpdateBar();

        if (pulseTimer > 0f)
        {
            pulseTimer = Mathf.Max(0f, pulseTimer - Time.deltaTime);
            float pulseFactor = 1f + (pulseTimer / Mathf.Max(0.01f, pulseDuration)) * 0.12f;
            float scaledCanvas = canvasScale * pulseFactor;
            canvasRect.localScale = new Vector3(scaledCanvas, scaledCanvas, canvasScale);
        }
        else
        {
            canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);
        }
    }

    private void CreateBarUI()
    {
        GameObject canvasObject = transform.Find("SnackBarCanvas")?.gameObject;
        if (canvasObject == null)
        {
            canvasObject = new GameObject("SnackBarCanvas");
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
        canvas.sortingOrder = 260;
        canvasRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        UpdateCanvasPlacement();

        RectTransform sliderRect = FindOrCreateRect(canvasRect, "SnackSlider");
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = Vector2.zero;

        backgroundBar = sliderRect.GetComponent<Image>();
        if (backgroundBar == null)
            backgroundBar = sliderRect.gameObject.AddComponent<Image>();
        backgroundBar.color = backgroundColor;

        RectTransform fillAreaRect = FindOrCreateRect(sliderRect, "Fill Area");
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        RectTransform progressRect = FindOrCreateRect(fillAreaRect, "Fill");
        progressRect.anchorMin = Vector2.zero;
        progressRect.anchorMax = Vector2.one;
        progressRect.pivot = new Vector2(0f, 0.5f);
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;

        progressBar = progressRect.GetComponent<Image>();
        if (progressBar == null)
            progressBar = progressRect.gameObject.AddComponent<Image>();
        progressBar.type = Image.Type.Simple;

        progressSlider = sliderRect.GetComponent<Slider>();
        if (progressSlider == null)
            progressSlider = sliderRect.gameObject.AddComponent<Slider>();

        progressSlider.transition = Selectable.Transition.None;
        progressSlider.direction = Slider.Direction.LeftToRight;
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.wholeNumbers = false;
        progressSlider.handleRect = null;
        progressSlider.fillRect = progressRect;
        progressSlider.targetGraphic = backgroundBar;
        progressSlider.SetValueWithoutNotify(0f);
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

    private void UpdateBar()
    {
        if (progressSlider == null || progressBar == null || snackGenerator == null)
            return;

        progressSlider.SetValueWithoutNotify(snackGenerator.GetGenerationProgress01());
        progressBar.color = GetFillColor(snackGenerator.GetOwnerTeam());
        SetAlpha(alwaysShow ? 1f : progressSlider.value > 0f ? 1f : 0f);
    }

    private Color GetFillColor(CombatTeam team)
    {
        if (team == CombatTeam.Blue)
            return playerFillColor;

        if (team == CombatTeam.Red)
            return enemyFillColor;

        return neutralFillColor;
    }

    private void SetAlpha(float alpha)
    {
        if (backgroundBar != null)
        {
            Color bg = backgroundColor;
            bg.a = alpha;
            backgroundBar.color = bg;
        }

        if (progressBar != null)
        {
            Color fill = progressBar.color;
            fill.a = alpha;
            progressBar.color = fill;
        }
    }

    private void OnSnacksGenerated(int generatedAmount)
    {
        if (pulseOnGeneration)
            pulseTimer = pulseDuration;
    }

    private void OnValidate()
    {
        verticalOffset = Mathf.Max(0f, verticalOffset);
        barWidth = Mathf.Max(0.1f, barWidth);
        barHeight = Mathf.Max(0.05f, barHeight);
        canvasScale = Mathf.Max(0.001f, canvasScale);
        pulseDuration = Mathf.Max(0.05f, pulseDuration);
    }

    private void OnDestroy()
    {
        if (snackGenerator != null)
            snackGenerator.SnacksGenerated -= OnSnacksGenerated;

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
