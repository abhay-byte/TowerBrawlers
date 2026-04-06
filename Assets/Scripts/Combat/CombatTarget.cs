using System;
using UnityEngine;

[Flags]
public enum CombatTargetType
{
    None = 0,
    Character = 1 << 0,
    Building = 1 << 1,
    Player = 1 << 2,
    Any = Character | Building | Player
}

public class CombatTarget : MonoBehaviour
{
    [SerializeField] private CombatTeam team = CombatTeam.Neutral;
    [SerializeField] private CombatTargetType targetType = CombatTargetType.Character;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private Vector2 aimPointOffset = new Vector2(0f, 0.2f);
    [SerializeField] private bool destroyOnDeath = true;

    private float currentHealth;

    public event Action<CombatTarget> Died;
    public event Action<CombatTarget, float> Damaged; // (target, damageAmount)

    public CombatTeam Team
    {
        get => team;
        set => team = value;
    }

    public CombatTargetType TargetType
    {
        get => targetType;
        set => targetType = value;
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public Vector3 AimPoint => transform.position + (Vector3)aimPointOffset;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);

        if (!Application.isPlaying)
            currentHealth = maxHealth;
    }

    public bool IsEnemy(CombatTeam otherTeam)
    {
        if (team == CombatTeam.Neutral || otherTeam == CombatTeam.Neutral)
            return false;

        return team != otherTeam;
    }

    public void SetMaxHealth(float value, bool refillHealth = true)
    {
        maxHealth = Mathf.Max(1f, value);

        if (refillHealth || currentHealth <= 0f)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
            return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        // Fire damaged event if health changed and still alive
        if (currentHealth > 0f && currentHealth < previousHealth)
        {
            Damaged?.Invoke(this, amount);
            return;
        }

        // Fire died event if health reached 0
        Died?.Invoke(this);

        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
