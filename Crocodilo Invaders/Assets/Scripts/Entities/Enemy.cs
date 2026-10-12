using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMovement), typeof(Collider2D))]
public sealed class Enemy : MonoBehaviour
{
    [SerializeField, Min(0)] private int contactDamage = 1;
    [SerializeField, Min(0)] private int pointsOnDefeat = 800;
    [SerializeField, Min(0f)] private float deathLifetime = 0.65f;

    public Health Health { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyCombat Combat { get; private set; }
    public int PointsOnDefeat => pointsOnDefeat;
    public event Action<Enemy> Defeated;
    public event Action<Enemy> Removed;

    private Collider2D hitbox;

    private void Awake()
    {
        Health = GetComponent<Health>();
        Movement = GetComponent<EnemyMovement>();
        Combat = GetComponent<EnemyCombat>();
        hitbox = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        Health.Damaged += OnDamaged;
        Health.Died += OnDied;
    }

    private void OnDisable()
    {
        Health.Damaged -= OnDamaged;
        Health.Died -= OnDied;
    }

    private void OnDamaged()
    {
        if (!Health.IsDead) Movement.ApplyHitSlow();
    }
    private void OnDestroy() => Removed?.Invoke(this);

    private void OnDied()
    {
        Movement.enabled = false;
        if (Combat != null) Combat.enabled = false;
        hitbox.enabled = false;
        Defeated?.Invoke(this);
        Destroy(gameObject, deathLifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Health.IsDead) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null) player.Health.TakeDamage(contactDamage);
    }
}
