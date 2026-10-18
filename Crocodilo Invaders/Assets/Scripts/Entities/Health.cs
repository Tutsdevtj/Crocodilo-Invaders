using System;
using UnityEngine;

public sealed class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 30;

    private float remainingHealth;
    public int CurrentHealth => Mathf.CeilToInt(remainingHealth);
    public float PreciseCurrentHealth => remainingHealth;
    public float Fraction => remainingHealth / maxHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => remainingHealth <= 0f;
    public bool IsInvulnerable { get; private set; }
    public float IncomingDamageMultiplier { get; set; } = 1f;
    public void SetInvulnerable(bool invulnerable) => IsInvulnerable = invulnerable;

    private void OnDisable() => IsInvulnerable = false;
    public event Action<int, int> HealthChanged;
    public event Action Damaged;
    public event Action Healed;
    public event Action Died;
    // Uma entidade multifase pode consumir o esgotamento antes da morte definitiva.
    public event Action Depleted;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        remainingHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public bool TakeDamage(int amount)
        => TakeDamage((float)amount);

    public bool TakeDamage(float amount)
    {
        if (!isActiveAndEnabled || IsDead || IsInvulnerable || amount <= 0f || float.IsNaN(amount)) return false;

        remainingHealth -= Mathf.Min(amount * Mathf.Max(0f, IncomingDamageMultiplier), remainingHealth);
        if (remainingHealth < .0001f) remainingHealth = 0f;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke();
        if (IsDead) Depleted?.Invoke();
        if (IsDead) Died?.Invoke();
        return true;
    }

    public bool RestoreNextPhase(int maximum)
    {
        // Não é cura/respawn: somente o dono de uma transição invulnerável pode usar.
        if (!isActiveAndEnabled || !IsDead || !IsInvulnerable || maximum < 1) return false;
        maxHealth = maximum;
        remainingHealth = maximum;
        IncomingDamageMultiplier = 1f;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        return true;
    }

    public bool Heal(int amount)
    {
        // Cura recupera vida de uma entidade viva; não representa um respawn.
        if (!isActiveAndEnabled || IsDead || amount <= 0 || remainingHealth >= maxHealth) return false;

        remainingHealth += Mathf.Min(amount, maxHealth - remainingHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Healed?.Invoke();
        return true;
    }

    public bool SetMaximumHealth(int maximum, bool fillAddedCapacity)
    {
        if (IsDead || maximum < 1 || maximum == maxHealth) return false;
        int increase = maximum - maxHealth;
        maxHealth = maximum;
        if (fillAddedCapacity && increase > 0) remainingHealth += increase;
        remainingHealth = Mathf.Min(remainingHealth, maxHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        return true;
    }

    [ContextMenu("Teste/Dano de 5 HP")]
    private void TestDamage()
    {
        if (Application.isPlaying) TakeDamage(5);
    }

    [ContextMenu("Teste/Cura de 6 HP")]
    private void TestHeal()
    {
        if (Application.isPlaying) Heal(6);
    }
}
