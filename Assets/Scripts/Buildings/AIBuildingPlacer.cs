using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class AIBuildingPlacer : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private SnackWallet wallet;
    [SerializeField] private List<BuildSlot> ownedSlots = new List<BuildSlot>();
    [SerializeField] private float buildInterval = 4f;
    [SerializeField] private int minimumHousesBeforeMilitary = 2;
    [SerializeField] private int desiredHouseCount = 4;
    [SerializeField] private int minimumSnackReserve = 40;

    private float nextBuildTime;

    private void Awake()
    {
        if (placementController == null)
            placementController = FindFirstObjectByType<BuildingPlacementController>();

        if (wallet == null)
            wallet = GetComponent<SnackWallet>();
    }

    private void Update()
    {
        if (placementController == null || wallet == null || Time.time < nextBuildTime)
            return;

        nextBuildTime = Time.time + buildInterval;

        List<BuildSlot> emptyOwnedSlots = ownedSlots
            .Where(slot => slot != null && slot.IsEmpty)
            .ToList();

        if (emptyOwnedSlots.Count == 0)
            return;

        BuildingCatalogEntry chosenEntry = ChooseBuildingToPlace();
        if (chosenEntry == null || wallet.CurrentSnacks - chosenEntry.snackCost < minimumSnackReserve)
            return;

        foreach (BuildSlot slot in emptyOwnedSlots)
        {
            if (placementController.TryPlaceFromAI(chosenEntry, slot, wallet))
                return;
        }
    }

    private BuildingCatalogEntry ChooseBuildingToPlace()
    {
        List<BuildingCatalogEntry> aiEntries = placementController.BuildingCatalog
            .Where(entry => entry != null && entry.availableForAI && !entry.isCastle)
            .ToList();

        if (aiEntries.Count == 0)
            return null;

        int houseCount = CountTeamBuildings("house");
        bool needsEconomy = houseCount < minimumHousesBeforeMilitary || wallet.CurrentSnacks < minimumSnackReserve * 2;

        if (needsEconomy)
            return FindCheapestMatching(aiEntries, entry => IsHouse(entry.buildingId));

        if (houseCount < desiredHouseCount)
        {
            BuildingCatalogEntry houseEntry = FindCheapestMatching(aiEntries, entry => IsHouse(entry.buildingId));
            if (houseEntry != null && wallet.CurrentSnacks < minimumSnackReserve * 3)
                return houseEntry;
        }

        string[] militaryPriority = { "barracks", "archery", "monastery", "tower" };
        foreach (string buildingId in militaryPriority)
        {
            if (CountTeamBuildings(buildingId) > 0 && buildingId != "tower")
                continue;

            BuildingCatalogEntry militaryEntry = FindCheapestMatching(aiEntries, entry => string.Equals(entry.buildingId, buildingId, System.StringComparison.OrdinalIgnoreCase));
            if (militaryEntry != null)
                return militaryEntry;
        }

        BuildingCatalogEntry fallbackHouse = FindCheapestMatching(aiEntries, entry => IsHouse(entry.buildingId));
        if (fallbackHouse != null)
            return fallbackHouse;

        return aiEntries.OrderBy(entry => entry.snackCost).FirstOrDefault();
    }

    private int CountTeamBuildings(string buildingId)
    {
        int count = 0;
        PlacedBuilding[] buildings = FindObjectsByType<PlacedBuilding>(FindObjectsInactive.Exclude);
        foreach (PlacedBuilding building in buildings)
        {
            if (building == null)
                continue;

            if (!string.Equals(building.BuildingId, buildingId, System.StringComparison.OrdinalIgnoreCase) && !(buildingId == "house" && IsHouse(building.BuildingId)))
                continue;

            CombatTarget target = building.GetComponent<CombatTarget>();
            if (target != null && target.Team == CombatTeam.Red)
                count++;
        }

        return count;
    }

    private static bool IsHouse(string buildingId)
    {
        return !string.IsNullOrWhiteSpace(buildingId) && buildingId.StartsWith("house", System.StringComparison.OrdinalIgnoreCase);
    }

    private static BuildingCatalogEntry FindCheapestMatching(IEnumerable<BuildingCatalogEntry> entries, System.Func<BuildingCatalogEntry, bool> predicate)
    {
        return entries
            .Where(predicate)
            .OrderBy(entry => entry.snackCost)
            .FirstOrDefault();
    }
}
