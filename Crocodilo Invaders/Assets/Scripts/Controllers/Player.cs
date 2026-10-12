using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(Health))]
public sealed class Player : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public Health Health { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        Health.Died += OnDied;
    }

    private void OnDisable()
    {
        Health.Died -= OnDied;
    }

    private void OnDied()
    {
        Movement.enabled = false;
        Combat.enabled = false;
    }

    public void SetMoveInput(Vector2 direction)
    {
        if (Health.IsDead) return;
        Movement.SetMoveInput(direction);
    }

    public bool TryShoot() => Combat.TryShoot();

    public bool TrySpecialAttack() => Combat.TrySpecialAttack();
}
