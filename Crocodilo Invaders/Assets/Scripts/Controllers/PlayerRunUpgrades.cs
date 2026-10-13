using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(Health))]
public sealed class PlayerRunUpgrades : MonoBehaviour
{
    private readonly int[] bonuses = new int[5];
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Health health;
    private float baseMoveSpeed;
    private int baseMaxHealth;
    public int AcquiredCount { get; private set; }
    public event Action Changed;
    public int Bonus(UpgradeKind kind) => bonuses[(int)kind];

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<Health>();
        baseMoveSpeed = movement.MoveSpeed;
        baseMaxHealth = health.MaxHealth;
    }

    public bool Apply(UpgradeCard card)
    {
        if (card == null || card.percent <= 0 || (int)card.kind < 0 || (int)card.kind >= bonuses.Length || health.IsDead) return false;
        bonuses[(int)card.kind] += card.percent;
        AcquiredCount++;
        UpdateStats(card.kind == UpgradeKind.MaximumHealth);
        Changed?.Invoke();
        return true;
    }

    public float Value(UpgradeKind kind, int additionalPercent = 0)
    {
        float multiplier = 1f + (Bonus(kind) + additionalPercent) / 100f;
        switch (kind)
        {
            case UpgradeKind.AttackDamage: return combat.BaseNormalDamage * multiplier;
            case UpgradeKind.AttackSpeed: return multiplier / combat.BaseShotInterval;
            case UpgradeKind.MaximumHealth: return Mathf.CeilToInt(baseMaxHealth * multiplier);
            case UpgradeKind.MoveSpeed: return baseMoveSpeed * multiplier;
            case UpgradeKind.SpecialDamage: return combat.BaseSpecialDamage * multiplier;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private void UpdateStats(bool fillAddedCapacity)
    {
        combat.SetUpgradeMultipliers(1f + Bonus(UpgradeKind.AttackSpeed) / 100f,
            1f + Bonus(UpgradeKind.AttackDamage) / 100f, 1f + Bonus(UpgradeKind.SpecialDamage) / 100f);
        movement.SetMoveSpeed(Value(UpgradeKind.MoveSpeed));
        health.SetMaximumHealth((int)Value(UpgradeKind.MaximumHealth), fillAddedCapacity);
    }

    public void ResetForRun()
    {
        Array.Clear(bonuses, 0, bonuses.Length);
        AcquiredCount = 0;
        UpdateStats(false);
        Changed?.Invoke();
    }
}
