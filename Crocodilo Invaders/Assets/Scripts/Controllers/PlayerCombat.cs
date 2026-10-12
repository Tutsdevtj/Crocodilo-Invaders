using System;
using UnityEngine;

public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Transform specialOrigin;
    [SerializeField] private Projectile normalProjectile;
    [SerializeField] private Projectile specialProjectile;
    [SerializeField, Min(0.01f)] private float shotInterval = 0.2f;
    [SerializeField, Min(1)] private int specialChargeRequired = 10;

    private Collider2D ownerCollider;
    private float nextShotTime;
    private Projectile activeSpecialShot;

    public int Charge { get; private set; }
    public int SpecialChargeRequired => specialChargeRequired;
    public event Action ShotFired;
    public event Action SpecialFired;
    public event Action<int, int> ChargeChanged;

    private void Awake()
    {
        ownerCollider = GetComponent<Collider2D>();
    }

    public bool TryShoot()
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || Time.time < nextShotTime
            || normalProjectile == null || shotOrigin == null) return false;

        Projectile shot = Instantiate(normalProjectile, shotOrigin.position, shotOrigin.rotation);
        shot.Launch(shotOrigin.right, ownerCollider);
        // A cadência pertence ao combate; o tiro anterior pode continuar voando.
        nextShotTime = Time.time + shotInterval;
        SetCharge(Mathf.Min(specialChargeRequired, Charge + 1));
        ShotFired?.Invoke();
        return true;
    }

    public bool TrySpecialAttack()
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || activeSpecialShot != null
            || Charge < specialChargeRequired || specialProjectile == null || specialOrigin == null) return false;

        activeSpecialShot = Instantiate(specialProjectile, specialOrigin.position, specialOrigin.rotation);
        activeSpecialShot.Launch(specialOrigin.right, ownerCollider);
        SetCharge(0);
        SpecialFired?.Invoke();
        return true;
    }

    private void SetCharge(int value)
    {
        if (Charge == value) return;
        Charge = value;
        ChargeChanged?.Invoke(Charge, specialChargeRequired);
    }
}
