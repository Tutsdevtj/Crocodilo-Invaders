using System;
using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(PlayerMovement), typeof(Health))]
public sealed class PlayerDash : MonoBehaviour
{
    [SerializeField, Min(.01f)] private float baseDashDistance = 2.2f;
    [SerializeField, Min(.01f)] private float baseDashDuration = .16f;
    [SerializeField, Min(.01f)] private float baseCooldown = 1.1f;
    [SerializeField, Min(0f)] private float baseInvulnerabilityDuration = .13f;
    [SerializeField] private AnimationCurve speedCurve = new AnimationCurve(
        new Keyframe(0f, 2f, 0f, 0f), new Keyframe(.65f, .8f, -2f, -2f), new Keyframe(1f, .2f, 0f, 0f));
    private const int IntegrationSteps = 256;
    private readonly float[] cumulative = new float[IntegrationSteps + 1];
    private PlayerMovement movement;
    private Health health;
    private Vector2 lastDirection = Vector2.right;
    private float elapsed, distance, duration, startedAt, invulnerableUntil, readyAt, cooldownAtStart;
    private bool protection;
    private float distanceMultiplier = 1f, cooldownMultiplier = 1f, invulnerabilityMultiplier = 1f;
    public float BaseDashDistance => Mathf.Max(.01f, baseDashDistance);
    public float DashDistance => BaseDashDistance * distanceMultiplier;
    public float BaseDashDuration => Mathf.Max(.01f, baseDashDuration);
    public float DashDuration => BaseDashDuration;
    public float BaseCooldown => Mathf.Max(.01f, baseCooldown);
    public float Cooldown => BaseCooldown * cooldownMultiplier;
    public float BaseInvulnerabilityDuration => Mathf.Max(0f, baseInvulnerabilityDuration);
    public float InvulnerabilityDuration => Mathf.Min(DashDuration, BaseInvulnerabilityDuration * invulnerabilityMultiplier);
    public bool IsDashing { get; private set; }
    public bool HasInvulnerability => protection && Time.time < invulnerableUntil;
    public Vector2 Direction { get; private set; } = Vector2.right;
    public float ActiveDuration => duration;
    public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
    public float CooldownFraction => cooldownAtStart > 0f ? Mathf.Clamp01(CooldownRemaining / cooldownAtStart) : 0f;
    public bool IsReady => isActiveAndEnabled && movement.isActiveAndEnabled && !health.IsDead
        && Time.timeScale > 0f && !IsDashing && CooldownRemaining <= 0f;
    public event Action<Vector2> DashStarted;
    public event Action InvulnerabilityEnded;
    public event Action DashEnded;
    public event Action DashCancelled;

    private void Awake() { movement = GetComponent<PlayerMovement>(); health = GetComponent<Health>(); }
    private void OnEnable() => health.Died += CancelDash;
    private void OnDisable() { health.Died -= CancelDash; CancelDash(); }
    public void SetUpgradeMultipliers(float distance, float cooldown, float invulnerability)
    {
        distanceMultiplier = ValidMultiplier(distance, .01f);
        cooldownMultiplier = ValidMultiplier(cooldown, .01f);
        invulnerabilityMultiplier = ValidMultiplier(invulnerability, 0f);
    }
    private static float ValidMultiplier(float value, float minimum)
        => float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Max(minimum, value);
    public bool TryDash()
    {
        if (!IsReady) return false;
        RememberDirection(); Direction = lastDirection;
        distance = DashDistance; duration = DashDuration; elapsed = 0f;
        startedAt = Time.time; cooldownAtStart = Cooldown; readyAt = startedAt + cooldownAtStart;
        invulnerableUntil = startedAt + InvulnerabilityDuration;
        BakeIntegral(); IsDashing = true;
        protection = InvulnerabilityDuration > 0f;
        health.SetInvulnerable(this, protection);
        movement.SetVelocityOverride(StepVelocity(0f, Mathf.Min(duration, Time.fixedDeltaTime)));
        DashStarted?.Invoke(Direction);
        return true;
    }
    private void Update()
    {
        RememberDirection(); ExpireProtection();
        if (IsDashing && elapsed >= duration && Time.time >= startedAt + duration) Finish(false);
    }
    private void RememberDirection()
    {
        if (movement.MoveInput.sqrMagnitude > .0001f) lastDirection = movement.MoveInput.normalized;
    }
    private void FixedUpdate()
    {
        if (!IsDashing) return;
        ExpireProtection();
        if (!movement.isActiveAndEnabled) { CancelDash(); return; }
        if (elapsed >= duration) { Finish(false); return; }
        float next = Mathf.Min(duration, elapsed + Time.fixedDeltaTime);
        movement.SetVelocityOverride(StepVelocity(elapsed, next));
        elapsed = next;
    }
    private Vector2 StepVelocity(float from, float to)
        => Direction * (distance * (Integral(to / duration) - Integral(from / duration)) / Time.fixedDeltaTime);
    private void BakeIntegral()
    {
        cumulative[0] = 0f;
        float previous = CurveValue(0f);
        for (int i = 1; i <= IntegrationSteps; i++)
        {
            float next = CurveValue(i / (float)IntegrationSteps);
            cumulative[i] = cumulative[i - 1] + (previous + next) * .5f / IntegrationSteps;
            previous = next;
        }
        float area = cumulative[IntegrationSteps];
        for (int i = 1; i <= IntegrationSteps; i++) cumulative[i] = area > .00001f ? cumulative[i] / area : i / (float)IntegrationSteps;
        cumulative[IntegrationSteps] = 1f;
    }
    private float CurveValue(float time)
    {
        float value = speedCurve == null || speedCurve.length == 0 ? 1f : speedCurve.Evaluate(time);
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);
    }
    private float Integral(float normalized)
    {
        float sample = Mathf.Clamp01(normalized) * IntegrationSteps;
        int index = Mathf.Min(IntegrationSteps - 1, (int)sample);
        return Mathf.Lerp(cumulative[index], cumulative[index + 1], sample - index);
    }
    private void ExpireProtection()
    {
        if (!protection || Time.time < invulnerableUntil) return;
        ReleaseProtection();
    }
    private void ReleaseProtection()
    {
        if (!protection) return;
        protection = false; health.SetInvulnerable(this, false); InvulnerabilityEnded?.Invoke();
    }
    private void Finish(bool cancelled)
    {
        bool active = IsDashing;
        IsDashing = false; elapsed = 0f;
        movement.ClearVelocityOverride(); ReleaseProtection();
        if (!active) return;
        if (cancelled) DashCancelled?.Invoke(); else DashEnded?.Invoke();
    }
    public void CancelDash() => Finish(true);
}
