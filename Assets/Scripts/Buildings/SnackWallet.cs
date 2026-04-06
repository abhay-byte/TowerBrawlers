using System;
using UnityEngine;

public class SnackWallet : MonoBehaviour
{
    [SerializeField] private CombatTeam walletTeam = CombatTeam.Neutral;
    [SerializeField] private int startingSnacks = 200;

    private int currentSnacks;

    public event Action<int> SnacksChanged;

    public int CurrentSnacks => currentSnacks;
    public CombatTeam WalletTeam => walletTeam;

    private void Awake()
    {
        currentSnacks = Mathf.Max(0, startingSnacks);
        SnacksChanged?.Invoke(currentSnacks);
    }

    public bool CanAfford(int amount)
    {
        return amount <= currentSnacks;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > currentSnacks)
            return false;

        currentSnacks -= amount;
        SnacksChanged?.Invoke(currentSnacks);
        return true;
    }

    public void AddSnacks(int amount)
    {
        if (amount <= 0)
            return;

        currentSnacks += amount;
        SnacksChanged?.Invoke(currentSnacks);
    }

    public void SetSnacks(int amount)
    {
        currentSnacks = Mathf.Max(0, amount);
        SnacksChanged?.Invoke(currentSnacks);
    }
}
