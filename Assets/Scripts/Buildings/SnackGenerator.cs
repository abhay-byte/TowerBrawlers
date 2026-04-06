using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Generates snacks over time for the owning team's wallet.
/// Attach to any placed building (e.g., House) to enable passive income.
/// Works for both Player and AI based on the BuildSlot's ownerTeam.
/// </summary>
[RequireComponent(typeof(PlacedBuilding))]
public class SnackGenerator : MonoBehaviour
{
    public event Action<int> SnacksGenerated;

    [Header("Generation Settings")]
    [Tooltip("Snacks generated per tick")]
    [SerializeField] private int snacksPerTick = 2;

    [Tooltip("Time in seconds between each generation tick")]
    [SerializeField] private float tickInterval = 2f;

    [Header("References")]
    [Tooltip("Optional: Reference to the wallet. If null, finds SnackWallet in scene.")]
    [SerializeField] private SnackWallet wallet;

    private PlacedBuilding placedBuilding;
    private BuildSlot ownerSlot;
    private float tickTimer;
    private bool isInitialized;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
        if (placedBuilding == null)
        {
            Debug.LogWarning($"SnackGenerator on {gameObject.name}: Missing PlacedBuilding component.");
            enabled = false;
            return;
        }

        isInitialized = true;
        tickTimer = 0f;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;

        // Reset timer on enable
        tickTimer = 0f;
    }

    private void Update()
    {
        if (!isInitialized) return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            GenerateSnacks();
            tickTimer -= tickInterval;
        }
    }

    private void GenerateSnacks()
    {
        ResolveWalletIfNeeded();
        if (wallet == null)
        {
            Debug.LogWarning($"SnackGenerator on {gameObject.name}: Wallet is null during generation!");
            return;
        }

        // Verify building still exists and is valid
        if (placedBuilding == null)
        {
            Debug.LogWarning($"SnackGenerator on {gameObject.name}: PlacedBuilding destroyed, disabling generator.");
            enabled = false;
            return;
        }

        // Add snacks to wallet
        wallet.AddSnacks(snacksPerTick);
        SnacksGenerated?.Invoke(snacksPerTick);
        
        CombatTeam team = GetOwnerTeam();
        string teamName = team == CombatTeam.Blue ? "Player" : team == CombatTeam.Red ? "AI" : "Neutral";
        Debug.Log($"🍎 {gameObject.name} generated {snacksPerTick} snacks for {teamName}. Total: {wallet.CurrentSnacks}");

        // Optional: Visual/audio feedback (uncomment if needed)
        // OnSnacksGenerated?.Invoke(snacksPerTick);
    }

    /// <summary>
    /// Called when the building is initialized after placement.
    /// Can be used to set generation rate based on building type.
    /// </summary>
    public void Initialize(BuildSlot slot)
    {
        ownerSlot = slot;
        ResolveWalletIfNeeded();
    }

    /// <summary>
    /// Set custom generation rates.
    /// </summary>
    public void SetGenerationRate(int snacksPerTick, float tickInterval)
    {
        this.snacksPerTick = Mathf.Max(0, snacksPerTick);
        this.tickInterval = Mathf.Max(0.1f, tickInterval);
    }

    /// <summary>
    /// Get snacks generated per second for UI display.
    /// </summary>
    public float GetSnacksPerSecond()
    {
        return tickInterval > 0 ? snacksPerTick / tickInterval : 0;
    }

    /// <summary>
    /// Get normalized progress through the current generation cycle.
    /// </summary>
    public float GetGenerationProgress01()
    {
        if (!isInitialized || tickInterval <= 0f)
            return 0f;

        return Mathf.Clamp01(tickTimer / tickInterval);
    }

    /// <summary>
    /// Get the configured time between snack generation ticks.
    /// </summary>
    public float GetTickInterval()
    {
        return tickInterval;
    }

    /// <summary>
    /// Get the number of snacks created on each generation tick.
    /// </summary>
    public int GetSnacksPerTick()
    {
        return snacksPerTick;
    }

    /// <summary>
    /// Get the owner team of this generator.
    /// </summary>
    public CombatTeam GetOwnerTeam()
    {
        return ownerSlot != null ? ownerSlot.OwnerTeam : CombatTeam.Neutral;
    }

    /// <summary>
    /// Check if this generator belongs to player (Blue team).
    /// </summary>
    public bool IsPlayerOwned()
    {
        return GetOwnerTeam() == CombatTeam.Blue;
    }

    /// <summary>
    /// Check if this generator belongs to AI (Red team).
    /// </summary>
    public bool IsAIOwned()
    {
        return GetOwnerTeam() == CombatTeam.Red;
    }

    private void OnValidate()
    {
        // Validate values in inspector
        snacksPerTick = Mathf.Max(0, snacksPerTick);
        tickInterval = Mathf.Max(0.1f, tickInterval);
    }

    private void ResolveWalletIfNeeded()
    {
        if (wallet != null)
            return;

        CombatTeam ownerTeam = GetOwnerTeam();
        SnackWallet[] wallets = FindObjectsByType<SnackWallet>(FindObjectsInactive.Exclude);

        wallet = wallets.FirstOrDefault(candidate => candidate != null && candidate.WalletTeam == ownerTeam);

        if (wallet == null && ownerTeam == CombatTeam.Blue)
            wallet = wallets.FirstOrDefault(candidate => candidate != null && candidate.GetComponent<BuildingPlacementController>() != null);

        if (wallet == null && ownerTeam == CombatTeam.Red)
            wallet = wallets.FirstOrDefault(candidate => candidate != null && candidate.GetComponent<AIBuildingPlacer>() != null);

        if (wallet == null && wallets.Length == 1)
            wallet = wallets[0];
    }

    private void OnDrawGizmosSelected()
    {
        // Visual indicator in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.cyan;
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.cyan;
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, 
        //    $"🍎 {snacksPerTick} snacks / {tickInterval}s", labelStyle);
    }
}
