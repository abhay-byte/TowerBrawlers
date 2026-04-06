using UnityEngine;

/// <summary>
/// House building that generates snacks for the owning team.
/// This component should be added to House prefabs alongside the standard building components.
/// Works for both Player (Blue) and AI (Red) based on the BuildSlot's ownerTeam.
/// </summary>
[RequireComponent(typeof(PlacedBuilding))]
[RequireComponent(typeof(SnackGenerator))]
[RequireComponent(typeof(CombatTarget))]
public class House : MonoBehaviour
{
    [Header("House Settings")]
    [Tooltip("Display name for this house")]
    [SerializeField] private string houseName = "House";

    [Header("Snack Generation")]
    [Tooltip("Override default snack generation rate")]
    [SerializeField] private bool customGenerationRate = false;

    [Tooltip("Snacks generated per tick (if custom generation is enabled)")]
    [SerializeField] private int customSnacksPerTick = 2;

    [Tooltip("Time between generation ticks (if custom generation is enabled)")]
    [SerializeField] private float customTickInterval = 3f;

    private SnackGenerator snackGenerator;
    private PlacedBuilding placedBuilding;
    private bool isInitialized;

    private void Awake()
    {
        snackGenerator = GetComponent<SnackGenerator>();
        placedBuilding = GetComponent<PlacedBuilding>();

        if (snackGenerator == null)
        {
            Debug.LogWarning($"House on {gameObject.name}: Missing SnackGenerator component.");
            return;
        }

        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized) return;

        // Apply custom generation settings if enabled
        if (customGenerationRate)
        {
            snackGenerator.SetGenerationRate(customSnacksPerTick, customTickInterval);
        }

        string ownerType = snackGenerator.IsPlayerOwned() ? "Player" : "AI";
        Debug.Log($"🏠 {houseName} placed for {ownerType} - Generating snacks");
    }

    /// <summary>
    /// Called when this house is placed.
    /// </summary>
    public void OnPlaced(BuildSlot slot, BuildingCatalogEntry entry)
    {
        // Any house-specific placement logic can go here
        // The base initialization is handled by PlacedBuilding.Initialize()
    }

    /// <summary>
    /// Get current generation stats for UI display.
    /// </summary>
    public string GetGenerationStats()
    {
        if (snackGenerator == null) return "No generator";

        CombatTeam team = snackGenerator.GetOwnerTeam();
        string teamName = team == CombatTeam.Blue ? "Player" : 
                         team == CombatTeam.Red ? "AI" : "Neutral";

        return $"{houseName} ({teamName}): {snackGenerator.GetSnacksPerSecond():F1} snacks/sec";
    }

    private void OnValidate()
    {
        // Validate values in inspector
        customSnacksPerTick = Mathf.Max(0, customSnacksPerTick);
        customTickInterval = Mathf.Max(0.1f, customTickInterval);
    }
}
