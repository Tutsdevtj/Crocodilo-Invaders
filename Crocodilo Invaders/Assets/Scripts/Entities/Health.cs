using System;
using UnityEngine;

public sealed class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 30;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth == 0;
    public event Action<int, int> HealthChanged;
    public event Action Damaged;
    public event Action Healed;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public bool TakeDamage(int amount)
    {
        if (!isActiveAndEnabled || IsDead || amount <= 0) return false;

        CurrentHealth -= Mathf.Min(amount, CurrentHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke();
        if (IsDead) Died?.Invoke();
        return true;
    }

    public bool Heal(int amount)
    {
        // Cura recupera vida de uma entidade viva; não representa um respawn.
        if (!isActiveAndEnabled || IsDead || amount <= 0 || CurrentHealth == maxHealth) return false;

        CurrentHealth += Mathf.Min(amount, maxHealth - CurrentHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Healed?.Invoke();
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
