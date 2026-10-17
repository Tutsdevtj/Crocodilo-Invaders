using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BossCombat : MonoBehaviour, IBossCombatController
{
    public enum Attack { Fan, Missiles, Sweep, Lasers }

    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Projectile redProjectile;
    [SerializeField] private Projectile missilePrefab;
    [SerializeField] private BossLaser laserPrefab;
    [SerializeField] private Rect playerArena = new Rect(-9.28f, -3.01f, 11.45f, 9.01f);
    [SerializeField] private Rect laserCoverage = new Rect(-9.5f, -4.2f, 19f, 10.6f);
    [SerializeField, Min(.1f)] private float warningDuration = .75f;
    [SerializeField, Min(.1f)] private float recoveryDuration = 1.45f;
    [SerializeField, Min(.1f)] private float redProjectileSpeed = 4.8f;
    [SerializeField, Min(.1f)] private float missileSpeed = 3.7f;
    [SerializeField, Range(.01f, .99f)] private float enrageHealthFraction = .5f;

    private Boss boss;
    private Health target;
    private Collider2D ownerCollider;
    private readonly Attack[] attackBag = { Attack.Fan, Attack.Missiles, Attack.Sweep, Attack.Lasers };
    private int nextAttackIndex = 4;
    private Attack? lastAttack;
    private bool verticalLasers;
    private readonly List<Projectile> shots = new List<Projectile>();
    private readonly List<BossLaser> lasers = new List<BossLaser>();

    public bool IsEnraged { get; private set; }
    public Transform ShotOrigin => shotOrigin;
    public event Action<Attack, Vector2> TelegraphStarted;
    public event Action TelegraphFinished;
    public event Action<Attack> ShotFired;
    public event Action EnrageStarted;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        ownerCollider = GetComponent<Collider2D>();
    }

    public void SetTarget(Health targetHealth) => target = targetHealth;
    public void StopCombat() => enabled = false;

    private IEnumerator Start()
    {
        while (target == null || !boss.Ready) yield return null;
        while (boss.BattleActive && !target.IsDead)
        {
            // A fase muda entre padrões, sem acelerar um ataque já em andamento.
            if (!IsEnraged && boss.Health.Fraction <= enrageHealthFraction)
            {
                IsEnraged = true;
                ClearAttacks();
                EnrageStarted?.Invoke();
                yield return new WaitForSeconds(1.1f);
            }

            Attack attack = NextAttack();
            Vector2 aimPoint = target.transform.position;
            TelegraphStarted?.Invoke(attack, aimPoint);
            yield return new WaitForSeconds(warningDuration);
            TelegraphFinished?.Invoke();

            switch (attack)
            {
                case Attack.Fan: yield return FireFans(aimPoint); break;
                case Attack.Missiles: yield return FireMissiles(aimPoint); break;
                case Attack.Sweep: yield return FireSweep(); break;
                case Attack.Lasers: yield return FireLasers(aimPoint); break;
            }
            shots.RemoveAll(shot => shot == null);
            lasers.RemoveAll(laser => laser == null);
            yield return new WaitForSeconds(recoveryDuration * (IsEnraged ? .8f : 1f));
        }
    }

    private Attack NextAttack()
    {
        if (nextAttackIndex == attackBag.Length)
        {
            for (int i = attackBag.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (attackBag[i], attackBag[j]) = (attackBag[j], attackBag[i]);
            }
            if (lastAttack == attackBag[0]) (attackBag[0], attackBag[1]) = (attackBag[1], attackBag[0]);
            nextAttackIndex = 0;
        }
        Attack attack = attackBag[nextAttackIndex++];
        lastAttack = attack;
        return attack;
    }

    private IEnumerator FireFans(Vector2 aimPoint)
    {
        int count = IsEnraged ? 9 : 7;
        int volleys = IsEnraged ? 3 : 2;
        for (int volley = 0; volley < volleys; volley++)
        {
            float aim = AimAngle(aimPoint);
            float offset = volley % 2 == 0 ? 0f : 6f;
            for (int i = 0; i < count; i++)
                SpawnShot(redProjectile, aim + Mathf.Lerp(-42f, 42f, (float)i / (count - 1)) + offset, redProjectileSpeed);
            ShotFired?.Invoke(Attack.Fan);
            yield return new WaitForSeconds(.55f);
        }
    }

    private IEnumerator FireMissiles(Vector2 aimPoint)
    {
        int count = IsEnraged ? 4 : 3;
        for (int i = 0; i < count; i++)
        {
            float offset = Mathf.Lerp(-12f, 12f, (float)i / (count - 1));
            SpawnShot(missilePrefab, AimAngle(aimPoint) + offset, missileSpeed * (IsEnraged ? 1.12f : 1f));
            ShotFired?.Invoke(Attack.Missiles);
            yield return new WaitForSeconds(.25f);
        }
    }

    private IEnumerator FireSweep()
    {
        int count = IsEnraged ? 22 : 18;
        bool reverse = UnityEngine.Random.value < .5f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float angle = Mathf.Lerp(126f, 234f, reverse ? 1f - t : t);
            SpawnShot(redProjectile, angle, redProjectileSpeed * .9f);
            if (i % 3 == 0) ShotFired?.Invoke(Attack.Sweep);
            yield return new WaitForSeconds(.12f);
        }
    }

    private IEnumerator FireLasers(Vector2 aimPoint)
    {
        verticalLasers = !verticalLasers;
        float minimum = verticalLasers ? playerArena.xMin : playerArena.yMin;
        float maximum = verticalLasers ? playerArena.xMax : playerArena.yMax;
        float first = Mathf.Clamp(verticalLasers ? aimPoint.x : aimPoint.y, minimum + .4f, maximum - .4f);
        float distance = verticalLasers ? 2.7f : 2f;
        float second = first + (first > (minimum + maximum) * .5f ? -distance : distance);
        second = Mathf.Clamp(second, minimum + .4f, maximum - .4f);
        float length = verticalLasers ? laserCoverage.height : laserCoverage.width;
        foreach (float coordinate in new[] { first, second })
        {
            Vector2 center = verticalLasers ? new Vector2(coordinate, laserCoverage.center.y) : new Vector2(laserCoverage.center.x, coordinate);
            BossLaser laser = Instantiate(laserPrefab);
            laser.Begin(center, verticalLasers, length, IsEnraged ? 1f : 1.15f, IsEnraged ? 1.1f : .9f);
            lasers.Add(laser);
        }
        yield return new WaitForSeconds(2.35f);
    }

    private float AimAngle(Vector2 point)
    {
        Vector2 direction = point - (Vector2)shotOrigin.position;
        // O boss atira para a arena à esquerda, mesmo se o alvo estiver num limite.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        return Mathf.Clamp(angle, 140f, 220f);
    }

    private void SpawnShot(Projectile prefab, float angle, float speed)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Projectile shot = Instantiate(prefab, shotOrigin.position, Quaternion.Euler(0f, 0f, angle - 180f));
        shot.Launch(direction, ownerCollider, speed);
        shots.Add(shot);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        TelegraphFinished?.Invoke();
        ClearAttacks();
    }

    private void ClearAttacks()
    {
        foreach (Projectile shot in shots)
        {
            if (shot == null) continue;
            shot.gameObject.SetActive(false);
            Destroy(shot.gameObject);
        }
        foreach (BossLaser laser in lasers)
        {
            if (laser == null) continue;
            laser.Disarm();
            Destroy(laser.gameObject);
        }
        shots.Clear();
        lasers.Clear();
    }
}
