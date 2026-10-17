using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Transform specialOrigin;
    [SerializeField] private Projectile normalProjectile;
    [SerializeField] private Projectile specialProjectile;
    [SerializeField, Min(0.01f)] private float shotInterval = 0.45f;
    [SerializeField, Min(1)] private int specialChargeRequired = 10;

    private Collider2D ownerCollider;
    private Health health;
    private bool pendingShot;
    private bool pendingSpecial;
    private float nextShotTime;
    private Projectile activeSpecialShot;
    private float attackSpeedMultiplier = 1f;
    private float normalDamageMultiplier = 1f;
    private float specialDamageMultiplier = 1f;
    public float BaseShotInterval => shotInterval;
    public float ShotInterval => shotInterval / attackSpeedMultiplier;
    public float BaseNormalDamage => normalProjectile != null ? normalProjectile.BaseDamage : 0f;
    public float BaseSpecialDamage => specialProjectile != null ? specialProjectile.BaseDamage : 0f;
    public float NormalDamage => normalProjectile != null ? normalProjectile.BaseDamage * normalDamageMultiplier : 0f;
    public float SpecialDamage => specialProjectile != null ? specialProjectile.BaseDamage * specialDamageMultiplier : 0f;

    public void SetUpgradeMultipliers(float attackSpeed, float normalDamage, float specialDamage)
    {
        attackSpeedMultiplier = Mathf.Max(.01f, attackSpeed);
        normalDamageMultiplier = Mathf.Max(0f, normalDamage);
        specialDamageMultiplier = Mathf.Max(0f, specialDamage);
    }

    public int Charge { get; private set; }
    public int SpecialChargeRequired => specialChargeRequired;
    public bool HasPendingAttack => pendingShot || pendingSpecial;
    public event Action ShotStarted;
    public event Action SpecialStarted;
    public event Action ShotFired;
    public event Action SpecialFired;
    public event Action<int, int> ChargeChanged;

    private void Awake()
    {
        ownerCollider = GetComponent<Collider2D>();
        health = GetComponent<Health>();
    }

    private void OnEnable() => health.Died += CancelPendingAttacks;

    private void OnDisable()
    {
        health.Died -= CancelPendingAttacks;
        CancelPendingAttacks();
    }

    public void CancelPendingAttacks() => pendingShot = pendingSpecial = false;

    public bool TryShoot()
    {
        if (!isActiveAndEnabled || health.IsDead || HasPendingAttack || Time.timeScale == 0f || Time.time < nextShotTime
            || normalProjectile == null || shotOrigin == null || ShotStarted == null) return false;

        pendingShot = true;
        nextShotTime = Time.time + ShotInterval;
        ShotStarted?.Invoke();
        return true;
    }

    // Called by the animation event, not by input or a second cooldown timer.
    public void ReleaseNormalShot()
    {
        if (!pendingShot) return;
        pendingShot = false;
        if (!isActiveAndEnabled || health.IsDead) return;
        Projectile shot = Instantiate(normalProjectile, shotOrigin.position, shotOrigin.rotation);
        shot.Launch(shotOrigin.right, ownerCollider, damageMultiplier: normalDamageMultiplier);
        SetCharge(Mathf.Min(specialChargeRequired, Charge + 1));
        ShotFired?.Invoke();
    }

    public bool TrySpecialAttack()
    {
        if (!isActiveAndEnabled || health.IsDead || HasPendingAttack || Time.timeScale == 0f || activeSpecialShot != null
            || Charge < specialChargeRequired || specialProjectile == null || specialOrigin == null || SpecialStarted == null) return false;

        pendingSpecial = true;
        SpecialStarted?.Invoke();
        return true;
    }

    public void ReleaseSpecial()
    {
        if (!pendingSpecial) return;
        pendingSpecial = false;
        if (!isActiveAndEnabled || health.IsDead) return;
        activeSpecialShot = Instantiate(specialProjectile, specialOrigin.position, specialOrigin.rotation);
        activeSpecialShot.Launch(specialOrigin.right, ownerCollider, damageMultiplier: specialDamageMultiplier);
        SetCharge(0);
        SpecialFired?.Invoke();
    }

    private void SetCharge(int value)
    {
        if (Charge == value) return;
        Charge = value;
        ChargeChanged?.Invoke(Charge, specialChargeRequired);
    }
}
