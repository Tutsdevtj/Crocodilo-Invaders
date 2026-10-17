using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(Health))]
[RequireComponent(typeof(PlayerDash))]
public sealed class Player : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerDash Dash { get; private set; }
    public Health Health { get; private set; }
    private bool gameplayActive = true;

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Dash = GetComponent<PlayerDash>();
        Health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        Health.Died += OnDied;
    }

    private void OnDisable()
    {
        Health.Died -= OnDied;
        Dash.CancelDash();
    }

    private void OnDied()
    {
        SetGameplayActive(false);
    }

    public void SetGameplayActive(bool active)
    {
        gameplayActive = active && !Health.IsDead;
        Movement.SetMoveInput(Vector2.zero);
        Dash.enabled = gameplayActive;
        Movement.enabled = gameplayActive;
        Combat.enabled = gameplayActive;
    }

    public void SetMoveInput(Vector2 direction)
    {
        if (!gameplayActive || Health.IsDead) return;
        Movement.SetMoveInput(direction);
    }

    public bool TryShoot() => Combat.TryShoot();

    public bool TrySpecialAttack() => Combat.TrySpecialAttack();

    public bool TryDash() => isActiveAndEnabled && gameplayActive && !Health.IsDead && Dash.TryDash();
}
