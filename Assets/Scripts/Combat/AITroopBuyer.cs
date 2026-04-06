using UnityEngine;
using System;

[DisallowMultipleComponent]
public class AITroopBuyer : MonoBehaviour
{
    [SerializeField] private TroopPurchaseController troopPurchaseController;
    [SerializeField] private SnackWallet wallet;
    [SerializeField] private float buyInterval = 5f;
    [SerializeField] private float initialAttackDelaySeconds = 120f;
    [SerializeField] private int minimumSnacksToKeep = 75;
    [SerializeField] private int minimumHouseCountBeforeBuying = 2;
    [SerializeField] private int maximumQueuedTroops = 6;

    private float nextBuyTime;
    private float attackDelayTimer;

    public float RemainingAttackDelaySeconds => Mathf.Max(0f, initialAttackDelaySeconds - attackDelayTimer);
    public bool CanAttack => RemainingAttackDelaySeconds <= 0f;

    private void Awake()
    {
        if (troopPurchaseController == null)
            troopPurchaseController = FindAnyObjectByType<TroopPurchaseController>();

        if (wallet == null)
            wallet = GetComponent<SnackWallet>();
    }

    private void Update()
    {
        if (troopPurchaseController == null || wallet == null)
            return;

        if (!CanAttack)
        {
            attackDelayTimer += Time.deltaTime;
            return;
        }

        if (Time.time < nextBuyTime)
            return;

        nextBuyTime = Time.time + buyInterval;

        if (CountRedHouses() < minimumHouseCountBeforeBuying)
            return;

        if (CountQueuedTroops() >= maximumQueuedTroops)
            return;

        for (int index = 0; index < troopPurchaseController.TroopOptions.Count; index++)
        {
            TroopPurchaseController.TroopOption option = troopPurchaseController.TroopOptions[index];
            if (option == null)
                continue;

            if (wallet.CurrentSnacks - option.snackCost < minimumSnacksToKeep)
                continue;

            if (troopPurchaseController.TryPurchaseForTeam(index, CombatTeam.Red, wallet))
                return;
        }
    }

    private int CountRedHouses()
    {
        int count = 0;
        PlacedBuilding[] buildings = FindObjectsByType<PlacedBuilding>(FindObjectsInactive.Exclude);
        foreach (PlacedBuilding building in buildings)
        {
            if (building == null || !building.BuildingId.StartsWith("house", StringComparison.OrdinalIgnoreCase))
                continue;

            CombatTarget target = building.GetComponent<CombatTarget>();
            if (target != null && target.Team == CombatTeam.Red)
                count++;
        }

        return count;
    }

    private int CountQueuedTroops()
    {
        int count = 0;
        CombatTarget[] characters = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude);
        foreach (CombatTarget target in characters)
        {
            if (target == null || target.Team != CombatTeam.Red)
                continue;

            if ((target.TargetType & CombatTargetType.Character) == 0)
                continue;

            count++;
        }

        return count;
    }
}
