using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BuildingDragCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text costText;

    private BuildingPlacementController controller;
    private BuildingCatalogEntry entry;

    public void Initialize(BuildingPlacementController placementController, BuildingCatalogEntry catalogEntry)
    {
        controller = placementController;
        entry = catalogEntry;
        if (background == null)
            background = GetComponent<Image>();
    }

    public void SetVisuals(BuildingCatalogEntry catalogEntry)
    {
        entry = catalogEntry;

        if (iconImage != null)
            iconImage.sprite = entry.GetResolvedIcon();

        if (nameText != null)
            nameText.text = entry.displayName;

        if (costText != null)
            costText.text = $"Snacks: {entry.snackCost}";
    }

    public void Refresh(SnackWallet wallet)
    {
        if (background == null)
            background = GetComponent<Image>();

        bool affordable = wallet == null || wallet.CanAfford(entry.snackCost);
        background.color = affordable ? new Color(0.12f, 0.16f, 0.22f, 0.92f) : new Color(0.27f, 0.12f, 0.12f, 0.92f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controller?.BeginDrag(entry);
    }

    public void OnDrag(PointerEventData eventData)
    {
        controller?.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        controller?.EndDrag(eventData.position);
    }
}
