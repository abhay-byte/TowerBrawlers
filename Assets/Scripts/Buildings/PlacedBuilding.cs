using UnityEngine;

[DisallowMultipleComponent]
public class PlacedBuilding : MonoBehaviour
{
    [SerializeField] private string buildingId;
    [SerializeField] private int snackCost;
    [SerializeField] [Range(0f, 1f)] private float sellRefundMultiplier = 0.5f;

    private BuildSlot ownerSlot;

    public string BuildingId => buildingId;
    public int SellValue => Mathf.RoundToInt(snackCost * sellRefundMultiplier);

    public void Initialize(BuildSlot slot, CombatTeam ownerTeam, BuildingCatalogEntry entry)
    {
        ownerSlot = slot;
        buildingId = entry.buildingId;
        snackCost = entry.snackCost;
        sellRefundMultiplier = Mathf.Clamp01(entry.sellRefundMultiplier);

        CombatTarget target = GetComponent<CombatTarget>();
        if (target != null)
        {
            target.Team = ownerTeam;
            target.TargetType = CombatTargetType.Building;
        }

        // Initialize any snack generators on this building
        SnackGenerator generator = GetComponent<SnackGenerator>();
        if (generator != null)
        {
            generator.Initialize(slot);

            HouseSnackGenerationBar snackBar = GetComponent<HouseSnackGenerationBar>();
            if (snackBar == null)
                snackBar = gameObject.AddComponent<HouseSnackGenerationBar>();
        }

        // Add health bar if it doesn't exist
        BuildingHealthBar healthBar = GetComponent<BuildingHealthBar>();
        if (healthBar == null && target != null)
        {
            healthBar = gameObject.AddComponent<BuildingHealthBar>();
        }
    }

    private void OnDestroy()
    {
        if (ownerSlot != null)
            ownerSlot.NotifyBuildingDestroyed(this);
    }
}
