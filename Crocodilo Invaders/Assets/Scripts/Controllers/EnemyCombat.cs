using System;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public sealed class EnemyCombat : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Vector2 fireIntervalRange = new Vector2(1.2f, 2.2f);

    private Enemy enemy;
    private Collider2D ownerCollider;
    private Health target;
    private float nextShotTime;
    private bool ready;
    public event Action ShotFired;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        ownerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable() => ready = false;
    public void SetTarget(Health targetHealth) => target = targetHealth;

    private void Update()
    {
        if (Time.timeScale == 0f || target == null || target.IsDead || enemy.Health.IsDead
            || !enemy.Movement.HasArrived || projectilePrefab == null || shotOrigin == null) return;

        if (!ready)
        {
            ready = true;
            ScheduleNextShot();
            return;
        }
        if (Time.time < nextShotTime) return;

        Vector2 direction = (Vector2)target.transform.position - (Vector2)shotOrigin.position;
        Projectile shot = Instantiate(projectilePrefab, shotOrigin.position, Quaternion.identity);
        shot.Launch(direction, ownerCollider);
        ScheduleNextShot();
        ShotFired?.Invoke();
    }

    private void ScheduleNextShot()
    {
        float minimum = Mathf.Max(0.1f, Mathf.Min(fireIntervalRange.x, fireIntervalRange.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(fireIntervalRange.x, fireIntervalRange.y));
        nextShotTime = Time.time + UnityEngine.Random.Range(minimum, maximum);
    }
}
