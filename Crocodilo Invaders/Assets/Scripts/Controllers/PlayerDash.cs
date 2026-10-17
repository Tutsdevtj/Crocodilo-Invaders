using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(PlayerMovement), typeof(Health))]
public sealed class PlayerDash : MonoBehaviour
{
    [SerializeField, Min(.01f)] private float baseDashDistance = 1.5f;
    [SerializeField, Min(.01f)] private float baseDashDuration = .15f;
    [SerializeField, Min(.01f)] private float baseCooldown = 1f;
    [SerializeField, Min(0f)] private float baseInvulnerabilityDuration = .11f;

    private PlayerMovement movement;
    private Health health;
    private Vector2 lastDirection = Vector2.right;
    private Vector2 dashVelocity;
    private float remainingDuration;
    private float invulnerableUntil;
    private float readyAt;
    private float cooldownAtStart;
    private float distanceMultiplier = 1f;
    private float cooldownMultiplier = 1f;
    private float invulnerabilityMultiplier = 1f;

    public float BaseDashDistance => Mathf.Max(.01f, baseDashDistance);
    public float DashDistance => BaseDashDistance * distanceMultiplier;
    public float BaseDashDuration => Mathf.Max(.01f, baseDashDuration);
    public float DashDuration => BaseDashDuration;
    public float BaseCooldown => Mathf.Max(.01f, baseCooldown);
    public float Cooldown => BaseCooldown * cooldownMultiplier;
    public float BaseInvulnerabilityDuration => Mathf.Max(0f, baseInvulnerabilityDuration);
    public float InvulnerabilityDuration => Mathf.Min(DashDuration, BaseInvulnerabilityDuration * invulnerabilityMultiplier);
    public bool IsDashing { get; private set; }
    public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
    // 1 = cooldown completo; 0 = recarregado. Modificadores valem para o próximo dash.
    public float CooldownFraction => cooldownAtStart > 0f ? Mathf.Clamp01(CooldownRemaining / cooldownAtStart) : 0f;
    public bool IsReady => isActiveAndEnabled && movement.isActiveAndEnabled && !health.IsDead
        && Time.timeScale > 0f && !IsDashing && CooldownRemaining <= 0f;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<Health>();
    }

    private void OnEnable() => health.Died += CancelDash;

    private void OnDisable()
    {
        health.Died -= CancelDash;
        CancelDash();
    }

    public void SetUpgradeMultipliers(float distance, float cooldown, float invulnerability)
    {
        distanceMultiplier = ValidMultiplier(distance, .01f);
        // Multiplica a duração: 0,8 reduz o cooldown em 20%; 1 mantém o valor base.
        cooldownMultiplier = ValidMultiplier(cooldown, .01f);
        invulnerabilityMultiplier = ValidMultiplier(invulnerability, 0f);
    }

    private static float ValidMultiplier(float value, float minimum)
        => float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Max(minimum, value);

    public bool TryDash()
    {
        if (!IsReady) return false;
        RememberDirection();
        dashVelocity = lastDirection * (DashDistance / DashDuration);
        remainingDuration = DashDuration;
        cooldownAtStart = Cooldown;
        readyAt = Time.time + cooldownAtStart;
        invulnerableUntil = Time.time + InvulnerabilityDuration;
        IsDashing = true;
        health.SetInvulnerable(InvulnerabilityDuration > 0f);
        movement.SetVelocityOverride(dashVelocity);
        return true;
    }

    private void Update()
    {
        RememberDirection();
        if (IsDashing && Time.time >= invulnerableUntil) health.SetInvulnerable(false);
    }

    private void RememberDirection()
    {
        if (movement.MoveInput.sqrMagnitude > .0001f) lastDirection = movement.MoveInput.normalized;
    }

    private void FixedUpdate()
    {
        if (!IsDashing) return;
        if (!movement.isActiveAndEnabled || remainingDuration <= 0f)
        {
            CancelDash();
            return;
        }
        // Executa antes de PlayerMovement. O último passo parcial preserva a distância
        // mesmo quando a duração não é múltipla do passo da física (0,15 / 0,02).
        movement.SetVelocityOverride(dashVelocity * Mathf.Min(1f, remainingDuration / Time.fixedDeltaTime));
        remainingDuration -= Time.fixedDeltaTime;
        if (Time.time >= invulnerableUntil) health.SetInvulnerable(false);
    }

    public void CancelDash()
    {
        IsDashing = false;
        remainingDuration = 0f;
        movement.ClearVelocityOverride();
        health.SetInvulnerable(false);
    }
}
