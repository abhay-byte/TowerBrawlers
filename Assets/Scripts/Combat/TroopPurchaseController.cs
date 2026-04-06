using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TroopPurchaseController : MonoBehaviour
{
    [Serializable]
    public class TroopOption
    {
        public string troopId;
        public string displayName;
        public string requiredBuildingId;
        public GameObject bluePrefab;
        public GameObject redPrefab;
        public int snackCost = 25;
    }

    [Serializable]
    private sealed class TroopButtonBinding
    {
        public string troopId;
        public Button button;
        public Image icon;
        public Text nameText;
        public Text costText;
        public Text badgeText;
        public Text lockText;
    }

    private sealed class DeployedTroop
    {
        public string troopId;
        public CombatTeam team;
        public GameObject instance;
        public CombatTarget target;
        public UnitCombatController controller;
        public bool returning;
        public CombatTarget recallCastle;
    }

    [Header("References")]
    [SerializeField] private SnackWallet playerWallet;
    [SerializeField] private Camera worldCamera;

    [Header("Troops")]
    [SerializeField] private List<TroopOption> troopOptions = new List<TroopOption>();

    [Header("Spawn")]
    [SerializeField] private Transform blueSpawnPoint;
    [SerializeField] private Transform redSpawnPoint;
    [SerializeField] private float spawnForwardOffset = 1.75f;
    [SerializeField] private float spawnVerticalJitter = 0.35f;
    [SerializeField] private float deploySpread = 0.45f;

    [Header("Behaviour")]
    [SerializeField] private bool autoDeployRedTroops = true;
    [SerializeField] private float castleAbsorbRadius = 2.25f;

    [Header("UI")]
    [SerializeField] private RectTransform troopPanel;
    [SerializeField] private Text totalTroopsText;
    [SerializeField] private Button attackButton;
    [SerializeField] private Text attackButtonText;
    [SerializeField] private string attackLabel = "Attack";
    [SerializeField] private string recallLabel = "Recall";
    [SerializeField] private List<TroopButtonBinding> troopButtons = new List<TroopButtonBinding>();

    private readonly Dictionary<string, int> blueOwnedCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, int> redOwnedCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, int> blueGarrisonCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, int> redGarrisonCounts = new Dictionary<string, int>();
    private readonly List<DeployedTroop> deployedTroops = new List<DeployedTroop>();

    private bool blueTroopsDeployed;
    private bool redTroopsDeployed;
    private bool blueTroopsRecalling;
    private bool redTroopsRecalling;
    private float nextUnlockCheckTime;

    private const float UnlockCheckInterval = 0.5f;

    public IReadOnlyList<TroopOption> TroopOptions => troopOptions;

    private void Awake()
    {
        if (playerWallet == null)
            playerWallet = GetComponent<SnackWallet>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        BindUi();
        RefreshPlayerUi();
    }

    private void OnEnable()
    {
        if (playerWallet != null)
            playerWallet.SnacksChanged += OnPlayerWalletChanged;
    }

    private void OnDisable()
    {
        if (playerWallet != null)
            playerWallet.SnacksChanged -= OnPlayerWalletChanged;

        for (int index = deployedTroops.Count - 1; index >= 0; index--)
            UnregisterDeployedTroop(deployedTroops[index]);
    }

    private void Update()
    {
        if (Time.time >= nextUnlockCheckTime)
        {
            nextUnlockCheckTime = Time.time + UnlockCheckInterval;
            RefreshPlayerUi();
        }

        UpdateReturningTroops();

        if (blueTroopsDeployed && GetActiveTroopCount(CombatTeam.Blue) == 0)
        {
            blueTroopsDeployed = false;
            RefreshPlayerUi();
        }

        if (redTroopsDeployed && GetActiveTroopCount(CombatTeam.Red) == 0)
            redTroopsDeployed = false;

        if (blueTroopsRecalling && !HasReturningTroops(CombatTeam.Blue))
        {
            blueTroopsRecalling = false;
            RefreshPlayerUi();
        }

        if (redTroopsRecalling && !HasReturningTroops(CombatTeam.Red))
            redTroopsRecalling = false;
    }

    public bool TryPurchaseForTeam(int optionIndex, CombatTeam team, SnackWallet walletOverride = null)
    {
        if (optionIndex < 0 || optionIndex >= troopOptions.Count)
            return false;

        TroopOption option = troopOptions[optionIndex];
        if (option == null || !IsTroopUnlocked(option, team))
            return false;

        GameObject prefab = ResolvePrefabForTeam(option, team);
        if (prefab == null)
            return false;

        SnackWallet wallet = walletOverride != null ? walletOverride : ResolveWalletForTeam(team);
        if (wallet == null || !wallet.TrySpend(option.snackCost))
            return false;

        AddCount(GetOwnedCounts(team), option.troopId, 1);
        AddCount(GetGarrisonCounts(team), option.troopId, 1);

        if ((team == CombatTeam.Blue && blueTroopsDeployed && !blueTroopsRecalling) ||
            (team == CombatTeam.Red && redTroopsDeployed && !redTroopsRecalling))
        {
            DeployQueuedTroops(team);
        }
        else if (team == CombatTeam.Red && autoDeployRedTroops)
        {
            SetTeamDeployment(CombatTeam.Red, true);
        }

        if (team == CombatTeam.Blue)
            RefreshPlayerUi();

        return true;
    }

    public void ToggleBlueTroopDeployment()
    {
        SetTeamDeployment(CombatTeam.Blue, !blueTroopsDeployed);
    }

    public void SetTeamDeployment(CombatTeam team, bool deployed)
    {
        if (deployed)
            DeployTeamTroops(team);
        else
            RecallTeamTroops(team);

        if (team == CombatTeam.Blue)
            RefreshPlayerUi();
    }

    private void BindUi()
    {
        for (int index = 0; index < troopButtons.Count; index++)
        {
            TroopButtonBinding binding = troopButtons[index];
            if (binding == null || binding.button == null)
                continue;

            int optionIndex = FindOptionIndex(binding.troopId);
            if (optionIndex < 0)
                continue;

            binding.button.onClick.RemoveAllListeners();
            binding.button.onClick.AddListener(() => TryPurchaseForTeam(optionIndex, CombatTeam.Blue));
        }

        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(ToggleBlueTroopDeployment);
        }
    }

    private void RefreshPlayerUi()
    {
        int totalOwned = 0;
        int totalReady = 0;

        foreach (TroopButtonBinding binding in troopButtons)
        {
            if (binding == null)
                continue;

            TroopOption option = troopOptions.Find(candidate => candidate != null && candidate.troopId == binding.troopId);
            if (option == null)
                continue;

            int ownedCount = GetCount(GetOwnedCounts(CombatTeam.Blue), binding.troopId);
            totalOwned += ownedCount;
            totalReady += GetCount(GetGarrisonCounts(CombatTeam.Blue), binding.troopId);

            bool unlocked = IsTroopUnlocked(option, CombatTeam.Blue);
            bool canAfford = playerWallet != null && playerWallet.CanAfford(option.snackCost);

            if (binding.icon != null)
            {
                Sprite troopSprite = ResolveIcon(option, CombatTeam.Blue);
                binding.icon.sprite = troopSprite;
                binding.icon.preserveAspect = true;
                binding.icon.color = troopSprite != null
                    ? Color.white
                    : new Color(0.5f, 0.5f, 0.6f, 0.4f);
            }

            if (binding.nameText != null)
                binding.nameText.text = option.displayName;
            if (binding.costText != null)
                binding.costText.text = $"{option.snackCost} snacks";
            if (binding.badgeText != null)
                binding.badgeText.text = ownedCount.ToString();

            if (binding.button != null)
            {
                binding.button.interactable = unlocked && canAfford;
                if (binding.button.image != null)
                    binding.button.image.color = new Color(0.12f, 0.20f, 0.34f, 1f);
            }

            if (binding.lockText != null)
            {
                GameObject lockOverlay = binding.lockText.transform.parent.gameObject;
                bool showLock = !unlocked;
                lockOverlay.SetActive(showLock);

                if (showLock)
                {
                    string requirement = option.requiredBuildingId;
                    binding.lockText.text = string.IsNullOrWhiteSpace(requirement)
                        ? "LOCKED"
                        : $"Build {UppercaseFirst(requirement)}";
                }
            }
        }

        if (totalTroopsText != null)
            totalTroopsText.text = $"Owned: {totalOwned}  Ready: {totalReady}";

        if (attackButton != null)
            attackButton.interactable = blueTroopsDeployed || blueTroopsRecalling || totalReady > 0;

        if (attackButtonText != null)
            attackButtonText.text = (blueTroopsDeployed || blueTroopsRecalling) ? recallLabel : attackLabel;
    }

    private void DeployTeamTroops(CombatTeam team)
    {
        DeployQueuedTroops(team);

        SetTeamDeployedFlag(team, GetActiveTroopCount(team) > 0);
    }

    private void RecallTeamTroops(CombatTeam team)
    {
        CombatTarget castle = FindCastleForTeam(team);
        if (castle == null)
        {
            SetTeamDeployedFlag(team, false);
            return;
        }

        for (int index = deployedTroops.Count - 1; index >= 0; index--)
        {
            DeployedTroop deployed = deployedTroops[index];
            if (deployed == null || deployed.team != team || deployed.returning || deployed.instance == null)
                continue;

            deployed.returning = true;
            deployed.recallCastle = castle;
            Vector3 returnPoint = castle.transform.position;

            if (deployed.controller != null)
            {
                deployed.controller.SetAdvanceTarget(null);
                deployed.controller.SetManualDestination(returnPoint, true);
            }
        }

        SetTeamDeployedFlag(team, false);
        SetTeamRecallingFlag(team, GetActiveTroopCount(team) > 0);
    }

    private void CompleteTroopReturn(DeployedTroop deployed)
    {
        if (deployed == null)
            return;

        if (!deployedTroops.Contains(deployed))
            return;

        AddCount(GetGarrisonCounts(deployed.team), deployed.troopId, 1);
        UnregisterDeployedTroop(deployed);
        deployedTroops.Remove(deployed);

        if (deployed.instance != null)
            Destroy(deployed.instance);

        if (deployed.team == CombatTeam.Blue)
            RefreshPlayerUi();
    }

    private void UpdateReturningTroops()
    {
        for (int index = deployedTroops.Count - 1; index >= 0; index--)
        {
            DeployedTroop deployed = deployedTroops[index];
            if (deployed == null || !deployed.returning || deployed.instance == null || deployed.recallCastle == null)
                continue;

            float absorbRadius = castleAbsorbRadius;
            SpriteRenderer castleSprite = deployed.recallCastle.GetComponent<SpriteRenderer>();
            if (castleSprite != null)
                absorbRadius = Mathf.Max(absorbRadius, castleSprite.bounds.extents.x + 1.25f);

            float distance = Vector2.Distance(deployed.instance.transform.position, deployed.recallCastle.AimPoint);
            if (distance <= absorbRadius)
                CompleteTroopReturn(deployed);
        }
    }

    private void RegisterDeployedTroop(DeployedTroop deployed)
    {
        if (deployed?.target != null)
            deployed.target.Died += HandleDeployedTroopDied;
    }

    private void UnregisterDeployedTroop(DeployedTroop deployed)
    {
        if (deployed?.target != null)
            deployed.target.Died -= HandleDeployedTroopDied;
    }

    private void HandleDeployedTroopDied(CombatTarget deadTarget)
    {
        if (deadTarget == null)
            return;

        DeployedTroop deployed = deployedTroops.Find(candidate => candidate != null && candidate.target == deadTarget);
        if (deployed == null)
            return;

        UnregisterDeployedTroop(deployed);
        deployedTroops.Remove(deployed);

        AddCount(GetOwnedCounts(deployed.team), deployed.troopId, -1);

        if (deployed.team == CombatTeam.Blue)
        {
            if (GetActiveTroopCount(CombatTeam.Blue) == 0)
                blueTroopsDeployed = false;

            if (!HasReturningTroops(CombatTeam.Blue))
                blueTroopsRecalling = false;

            RefreshPlayerUi();
        }
        else if (deployed.team == CombatTeam.Red && GetActiveTroopCount(CombatTeam.Red) == 0)
        {
            redTroopsDeployed = false;
            if (!HasReturningTroops(CombatTeam.Red))
                redTroopsRecalling = false;
        }
    }

    private SnackWallet ResolveWalletForTeam(CombatTeam team)
    {
        if (team == CombatTeam.Blue && playerWallet != null)
            return playerWallet;

        SnackWallet[] wallets = FindObjectsByType<SnackWallet>(FindObjectsInactive.Exclude);
        foreach (SnackWallet candidate in wallets)
        {
            if (candidate != null && candidate.WalletTeam == team)
                return candidate;
        }

        return null;
    }

    private GameObject ResolvePrefabForTeam(TroopOption option, CombatTeam team)
    {
        if (team == CombatTeam.Blue)
            return option.bluePrefab;

        if (team == CombatTeam.Red)
            return option.redPrefab;

        return null;
    }

    private Sprite ResolveIcon(TroopOption option, CombatTeam team)
    {
        GameObject prefab = ResolvePrefabForTeam(option, team) ?? option.bluePrefab ?? option.redPrefab;
        if (prefab == null)
            return null;

        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    private Vector3 ResolveSpawnPosition(CombatTeam team, CombatTarget castle, int spawnIndex)
    {
        Transform explicitSpawn = team == CombatTeam.Blue ? blueSpawnPoint : redSpawnPoint;
        if (explicitSpawn != null)
            return explicitSpawn.position + ResolveFormationOffset(team, spawnIndex);

        if (castle != null)
            return castle.transform.position + ResolveFormationOffset(team, spawnIndex);

        float fallbackX = team == CombatTeam.Blue ? -3f : 3f;
        return new Vector3(fallbackX, 0f, 0f) + ResolveFormationOffset(team, spawnIndex);
    }

    private Vector3 ResolveFormationOffset(CombatTeam team, int spawnIndex)
    {
        float direction = team == CombatTeam.Blue ? 1f : -1f;
        int row = spawnIndex / 3;
        int column = spawnIndex % 3;
        float xOffset = direction * (spawnForwardOffset + (row * deploySpread));
        float yOffset = (column - 1) * deploySpread + UnityEngine.Random.Range(-spawnVerticalJitter, spawnVerticalJitter);
        return new Vector3(xOffset, yOffset, 0f);
    }

    private CombatTarget FindCastleForTeam(CombatTeam team)
    {
        CombatTarget[] targets = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude);
        foreach (CombatTarget target in targets)
        {
            if (target == null || target.Team != team)
                continue;

            if ((target.TargetType & CombatTargetType.Building) == 0)
                continue;

            if (target.name.IndexOf("castle", StringComparison.OrdinalIgnoreCase) >= 0)
                return target;
        }

        return null;
    }

    private void DeployQueuedTroops(CombatTeam team)
    {
        Dictionary<string, int> garrisonCounts = GetGarrisonCounts(team);
        CombatTarget castle = FindCastleForTeam(team);
        int spawnIndex = GetActiveTroopCount(team);

        foreach (TroopOption option in troopOptions)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.troopId))
                continue;

            int readyCount = GetCount(garrisonCounts, option.troopId);
            if (readyCount <= 0)
                continue;

            for (int count = 0; count < readyCount; count++)
            {
                SpawnTroop(team, option, castle, spawnIndex);
                spawnIndex++;
            }

            SetCount(garrisonCounts, option.troopId, 0);
        }
    }

    private void SpawnTroop(CombatTeam team, TroopOption option, CombatTarget castle, int spawnIndex)
    {
        GameObject prefab = ResolvePrefabForTeam(option, team);
        if (prefab == null)
            return;

        Vector3 spawnPosition = ResolveSpawnPosition(team, castle, spawnIndex);
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        instance.name = prefab.name;

        CombatTarget target = instance.GetComponent<CombatTarget>();
        if (target != null)
        {
            target.Team = team;
            target.TargetType = CombatTargetType.Character;
        }

        UnitCombatController controller = instance.GetComponent<UnitCombatController>();
        controller?.SetAdvanceTarget(FindNearestEnemyBuilding(team, spawnPosition));
        controller?.ClearManualDestination();

        DeployedTroop deployed = new DeployedTroop
        {
            troopId = option.troopId,
            team = team,
            instance = instance,
            target = target,
            controller = controller,
            returning = false
        };

        deployedTroops.Add(deployed);
        RegisterDeployedTroop(deployed);
    }

    private CombatTarget FindNearestEnemyBuilding(CombatTeam team, Vector3 fromPosition)
    {
        CombatTarget[] targets = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude);
        CombatTarget nearest = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (CombatTarget target in targets)
        {
            if (target == null || !target.IsAlive || !target.IsEnemy(team))
                continue;

            if ((target.TargetType & CombatTargetType.Building) == 0)
                continue;

            float distanceSqr = (target.AimPoint - fromPosition).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            nearest = target;
        }

        return nearest;
    }

    private bool IsTroopUnlocked(TroopOption option, CombatTeam team)
    {
        if (option == null || string.IsNullOrWhiteSpace(option.requiredBuildingId))
            return true;

        PlacedBuilding[] buildings = FindObjectsByType<PlacedBuilding>(FindObjectsInactive.Exclude);
        foreach (PlacedBuilding building in buildings)
        {
            if (building == null)
                continue;

            if (!string.Equals(building.BuildingId, option.requiredBuildingId, StringComparison.OrdinalIgnoreCase))
                continue;

            CombatTarget target = building.GetComponent<CombatTarget>();
            if (target != null && target.Team == team)
                return true;
        }

        return false;
    }

    private Dictionary<string, int> GetOwnedCounts(CombatTeam team)
    {
        return team == CombatTeam.Blue ? blueOwnedCounts : redOwnedCounts;
    }

    private Dictionary<string, int> GetGarrisonCounts(CombatTeam team)
    {
        return team == CombatTeam.Blue ? blueGarrisonCounts : redGarrisonCounts;
    }

    private int GetCount(Dictionary<string, int> counts, string troopId)
    {
        return counts.TryGetValue(troopId, out int count) ? count : 0;
    }

    private void SetCount(Dictionary<string, int> counts, string troopId, int value)
    {
        if (value <= 0)
        {
            counts.Remove(troopId);
            return;
        }

        counts[troopId] = value;
    }

    private void AddCount(Dictionary<string, int> counts, string troopId, int delta)
    {
        int nextValue = GetCount(counts, troopId) + delta;
        SetCount(counts, troopId, nextValue);
    }

    private int GetActiveTroopCount(CombatTeam team)
    {
        int activeCount = 0;

        foreach (DeployedTroop deployed in deployedTroops)
        {
            if (deployed == null || deployed.team != team || deployed.instance == null)
                continue;

            activeCount++;
        }

        return activeCount;
    }

    private bool HasReturningTroops(CombatTeam team)
    {
        foreach (DeployedTroop deployed in deployedTroops)
        {
            if (deployed != null && deployed.team == team && deployed.returning && deployed.instance != null)
                return true;
        }

        return false;
    }

    private void SetTeamDeployedFlag(CombatTeam team, bool deployed)
    {
        if (team == CombatTeam.Blue)
            blueTroopsDeployed = deployed;
        else if (team == CombatTeam.Red)
            redTroopsDeployed = deployed;
    }

    private void SetTeamRecallingFlag(CombatTeam team, bool recalling)
    {
        if (team == CombatTeam.Blue)
            blueTroopsRecalling = recalling;
        else if (team == CombatTeam.Red)
            redTroopsRecalling = recalling;
    }

    private int FindOptionIndex(string troopId)
    {
        for (int index = 0; index < troopOptions.Count; index++)
        {
            TroopOption option = troopOptions[index];
            if (option != null && option.troopId == troopId)
                return index;
        }

        return -1;
    }

    private string UppercaseFirst(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return char.ToUpper(value[0]) + value.Substring(1);
    }

    private void OnPlayerWalletChanged(int snacks)
    {
        RefreshPlayerUi();
    }
}
