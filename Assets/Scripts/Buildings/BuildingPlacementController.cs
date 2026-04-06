using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class BuildingPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private SnackWallet wallet;

    [Header("Catalog")]
    [SerializeField] private List<BuildingCatalogEntry> buildingCatalog = new List<BuildingCatalogEntry>();

    [Header("UI")]
    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private BuildingDragCardUI cardTemplate;
    [SerializeField] private Text snacksText;
    [SerializeField] private Text selectedSlotText;
    [SerializeField] private Button sellButton;

    [Header("Battlefield Camera")]
    [SerializeField] private Component overviewDolly;
    [SerializeField] private float overviewMoveSpeed = 0.35f;
    [SerializeField] private Key moveOverviewTowardBlueKey = Key.A;
    [SerializeField] private Key moveOverviewTowardRedKey = Key.D;
    [SerializeField] private Key snapOverviewBlueKey = Key.Home;
    [SerializeField] private Key snapOverviewRedKey = Key.End;
    [SerializeField] [Range(0f, 1f)] private float overviewInitialPosition;

    private readonly List<BuildingDragCardUI> cards = new List<BuildingDragCardUI>();

    private BuildSlot selectedSlot;
    private BuildingCatalogEntry draggedEntry;
    private Image dragPreview;
    private RectTransform dragPreviewRect;
    private PropertyInfo overviewCameraPositionProperty;

    public IReadOnlyList<BuildingCatalogEntry> BuildingCatalog => buildingCatalog;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (wallet == null)
            wallet = GetComponent<SnackWallet>();

        CacheOverviewDollyProperty();
        ApplyOverviewPosition(overviewInitialPosition);
        BuildCards();
        RefreshWalletLabel(wallet != null ? wallet.CurrentSnacks : 0);
        UpdateSelectionUI();
    }

    private void OnEnable()
    {
        if (wallet != null)
            wallet.SnacksChanged += RefreshWalletLabel;

        if (sellButton != null)
            sellButton.onClick.AddListener(HandleSellPressed);
    }

    private void OnDisable()
    {
        if (wallet != null)
            wallet.SnacksChanged -= RefreshWalletLabel;

        if (sellButton != null)
            sellButton.onClick.RemoveListener(HandleSellPressed);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Vector2 pointerPosition = mouse.position.ReadValue();
        HandleOverviewCameraInput(Keyboard.current);

        if (dragPreviewRect != null && dragPreviewRect.gameObject.activeSelf)
            dragPreviewRect.position = pointerPosition;

        if (draggedEntry != null)
            return;

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        BuildSlot slot = FindSlotUnderPointer(pointerPosition);
        if (slot != null)
            SelectSlot(slot);
    }

    public void BeginDrag(BuildingCatalogEntry entry)
    {
        draggedEntry = entry;
        EnsureDragPreview();
        dragPreview.sprite = entry != null ? entry.GetResolvedIcon() : null;
        dragPreview.color = Color.white;
        dragPreviewRect.gameObject.SetActive(dragPreview.sprite != null);
    }

    public void Drag(Vector2 screenPosition)
    {
        if (dragPreviewRect != null)
            dragPreviewRect.position = screenPosition;
    }

    public void EndDrag(Vector2 screenPosition)
    {
        if (dragPreviewRect != null)
            dragPreviewRect.gameObject.SetActive(false);

        if (draggedEntry == null)
            return;

        BuildSlot slot = FindSlotUnderPointer(screenPosition);
        if (slot != null && wallet != null && slot.TryPlace(draggedEntry, wallet, isAIBuilder: false))
            SelectSlot(slot);

        draggedEntry = null;
    }

    public bool TryPlaceFromAI(BuildingCatalogEntry entry, BuildSlot slot, SnackWallet aiWallet)
    {
        return slot != null && slot.TryPlace(entry, aiWallet, isAIBuilder: true);
    }

    private void HandleSellPressed()
    {
        if (selectedSlot == null || wallet == null)
            return;

        if (selectedSlot.TrySell(wallet))
            UpdateSelectionUI();
    }

    private void SelectSlot(BuildSlot slot)
    {
        selectedSlot = slot;
        UpdateSelectionUI();
    }

    private BuildSlot FindSlotUnderPointer(Vector2 screenPosition)
    {
        if (worldCamera == null)
            return null;

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);

        // Use RaycastAll to find all colliders at the click point
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero, 0.01f);

        // First, try to find a BuildSlot directly
        foreach (RaycastHit2D rayHit in hits)
        {
            BuildSlot slot = rayHit.collider.GetComponent<BuildSlot>();
            if (slot != null)
                return slot;
        }

        // Second, check if we hit a placed building and find its owner slot
        foreach (RaycastHit2D rayHit in hits)
        {
            PlacedBuilding building = rayHit.collider.GetComponent<PlacedBuilding>();
            if (building != null)
            {
                // Search parent hierarchy for BuildSlot
                Transform current = building.transform.parent;
                while (current != null)
                {
                    BuildSlot slot = current.GetComponent<BuildSlot>();
                    if (slot != null && slot.CurrentBuilding == building)
                        return slot;
                    current = current.parent;
                }
            }
        }

        // Fallback: OverlapPoint (original method)
        Collider2D overlapHit = Physics2D.OverlapPoint(worldPosition);
        if (overlapHit == null)
            return null;

        return overlapHit.GetComponent<BuildSlot>() ?? overlapHit.GetComponentInParent<BuildSlot>();
    }

    private void RefreshWalletLabel(int snacks)
    {
        if (snacksText != null)
            snacksText.text = $"Snacks: {snacks}";

        foreach (BuildingDragCardUI card in cards)
            card.Refresh(wallet);
    }

    private void UpdateSelectionUI()
    {
        if (selectedSlotText != null)
        {
            if (selectedSlot == null)
                selectedSlotText.text = "Selected: None";
            else if (selectedSlot.HasBuilding)
            {
                PlacedBuilding building = selectedSlot.CurrentBuilding;
                CombatTarget target = building.GetComponent<CombatTarget>();
                string healthText = target != null ? $" (HP: {target.CurrentHealth:F0}/{target.MaxHealth})" : "";
                selectedSlotText.text = $"Selected: {building.BuildingId}{healthText}";
            }
            else
                selectedSlotText.text = $"Selected: Empty Slot ({selectedSlot.OwnerTeam})";
        }

        if (sellButton != null)
            sellButton.interactable = selectedSlot != null && selectedSlot.HasBuilding;
    }

    private void BuildCards()
    {
        if (cardContainer == null || cardTemplate == null)
            return;

        List<GameObject> staleCards = new List<GameObject>();
        foreach (Transform child in cardContainer)
        {
            if (child != cardTemplate.transform)
                staleCards.Add(child.gameObject);
        }

        foreach (GameObject staleCard in staleCards)
        {
            if (Application.isPlaying)
                Destroy(staleCard);
            else
                DestroyImmediate(staleCard);
        }

        cards.Clear();
        cardTemplate.gameObject.SetActive(false);

        foreach (BuildingCatalogEntry entry in buildingCatalog)
        {
            if (entry == null || !entry.availableForPlayer || entry.isCastle || entry.ResolvePrefabForTeam(CombatTeam.Blue) == null)
                continue;

            BuildingDragCardUI card = Instantiate(cardTemplate, cardContainer);
            card.gameObject.name = $"{entry.displayName}_Card";
            card.gameObject.SetActive(true);
            card.Initialize(this, entry);
            card.SetVisuals(entry);
            card.Refresh(wallet);
            cards.Add(card);
        }
    }

    private void OnValidate()
    {
        overviewMoveSpeed = Mathf.Max(0.01f, overviewMoveSpeed);
        overviewInitialPosition = Mathf.Clamp01(overviewInitialPosition);
        CacheOverviewDollyProperty();

        if (!gameObject.scene.IsValid())
            return;

        if (cardContainer == null || cardTemplate == null)
            return;

        BuildCards();
        RefreshWalletLabel(wallet != null ? wallet.CurrentSnacks : 0);
        UpdateSelectionUI();
    }

    private void EnsureDragPreview()
    {
        Canvas canvas = cardContainer != null ? cardContainer.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
            return;

        if (dragPreview != null)
            return;

        GameObject previewObject = new GameObject("DragPreview", typeof(RectTransform), typeof(Image));
        previewObject.transform.SetParent(canvas.transform, false);
        dragPreviewRect = previewObject.GetComponent<RectTransform>();
        dragPreviewRect.sizeDelta = new Vector2(84f, 84f);
        dragPreview = previewObject.GetComponent<Image>();
        dragPreview.preserveAspect = true;
        dragPreview.raycastTarget = false;
        dragPreview.color = Color.white;
        previewObject.SetActive(false);
    }

    private void HandleOverviewCameraInput(Keyboard keyboard)
    {
        if (keyboard == null || overviewCameraPositionProperty == null || overviewDolly == null)
            return;

        float currentPosition = ReadOverviewPosition();
        float direction = 0f;

        if (keyboard[moveOverviewTowardBlueKey].isPressed)
            direction -= 1f;

        if (keyboard[moveOverviewTowardRedKey].isPressed)
            direction += 1f;

        if (direction != 0f)
            ApplyOverviewPosition(currentPosition + direction * overviewMoveSpeed * Time.unscaledDeltaTime);

        if (keyboard[snapOverviewBlueKey].wasPressedThisFrame)
            ApplyOverviewPosition(0f);

        if (keyboard[snapOverviewRedKey].wasPressedThisFrame)
            ApplyOverviewPosition(1f);
    }

    private void CacheOverviewDollyProperty()
    {
        overviewCameraPositionProperty = overviewDolly != null
            ? overviewDolly.GetType().GetProperty("CameraPosition", BindingFlags.Instance | BindingFlags.Public)
            : null;
    }

    private float ReadOverviewPosition()
    {
        if (overviewCameraPositionProperty == null || overviewDolly == null)
            return 0f;

        object value = overviewCameraPositionProperty.GetValue(overviewDolly);
        return value is float position ? position : 0f;
    }

    private void ApplyOverviewPosition(float position)
    {
        if (overviewCameraPositionProperty == null || overviewDolly == null)
            return;

        overviewCameraPositionProperty.SetValue(overviewDolly, Mathf.Clamp01(position));
    }
}
