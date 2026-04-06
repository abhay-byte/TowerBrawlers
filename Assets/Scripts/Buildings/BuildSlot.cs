using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class BuildSlot : MonoBehaviour
{
    [SerializeField] private CombatTeam ownerTeam = CombatTeam.Blue;
    [SerializeField] private Transform placementAnchor;
    [SerializeField] private bool allowPlayerBuilding = true;
    [SerializeField] private bool allowAIBuilding = true;
    [SerializeField] private bool allowSelling = true;
    [SerializeField] private BuildingSlotCategory allowedCategories = BuildingSlotCategory.Building;

    private PlacedBuilding currentBuilding;

    public CombatTeam OwnerTeam => ownerTeam;
    public bool IsEmpty => currentBuilding == null;
    public bool HasBuilding => currentBuilding != null;
    public PlacedBuilding CurrentBuilding => currentBuilding;

    public bool CanPlace(BuildingCatalogEntry entry, bool isAIBuilder)
    {
        if (entry == null || entry.isCastle || currentBuilding != null)
            return false;

        if (entry.ResolvePrefabForTeam(ownerTeam) == null)
            return false;

        if ((allowedCategories & entry.slotCategory) == 0)
            return false;

        if (isAIBuilder && !allowAIBuilding)
            return false;

        if (!isAIBuilder && !allowPlayerBuilding)
            return false;

        return true;
    }

    public bool TryPlace(BuildingCatalogEntry entry, SnackWallet wallet, bool isAIBuilder)
    {
        if (!CanPlace(entry, isAIBuilder))
            return false;

        if (wallet == null || !wallet.TrySpend(entry.snackCost))
            return false;

        SpawnBuilding(entry);
        return true;
    }

    public bool TrySell(SnackWallet wallet)
    {
        if (!allowSelling || currentBuilding == null || wallet == null)
            return false;

        wallet.AddSnacks(currentBuilding.SellValue);
        Destroy(currentBuilding.gameObject);
        currentBuilding = null;
        return true;
    }

    public void NotifyBuildingDestroyed(PlacedBuilding building)
    {
        if (currentBuilding == building)
            currentBuilding = null;
    }

    private void SpawnBuilding(BuildingCatalogEntry entry)
    {
        Transform anchor = placementAnchor != null ? placementAnchor : transform;
        GameObject prefab = entry.ResolvePrefabForTeam(ownerTeam);
        GameObject instance = Instantiate(prefab, anchor.position, anchor.rotation, anchor);
        instance.name = prefab.name;

        currentBuilding = instance.GetComponent<PlacedBuilding>();
        if (currentBuilding == null)
            currentBuilding = instance.AddComponent<PlacedBuilding>();

        currentBuilding.Initialize(this, ownerTeam, entry);
    }

    private void OnValidate()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
            collider2D.isTrigger = true;
    }
}
