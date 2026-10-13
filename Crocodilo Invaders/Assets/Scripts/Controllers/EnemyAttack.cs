using System;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    protected Enemy Enemy { get; private set; }
    protected Health Target { get; private set; }
    protected Collider2D OwnerCollider { get; private set; }
    protected bool CanAttack => Time.timeScale > 0f && Target != null && !Target.IsDead
        && !Enemy.Health.IsDead && Enemy.Movement.HasArrived;

    public bool IsTelegraphing { get; private set; }
    public Vector2 WarningDestination { get; private set; }
    public event Action ShotFired;
    public event Action<Vector2> TelegraphStarted;
    public event Action TelegraphFinished;

    protected virtual void Awake()
    {
        Enemy = GetComponent<Enemy>();
        OwnerCollider = GetComponent<Collider2D>();
    }

    public void SetTarget(Health target) => Target = target;
    protected void NotifyShotFired() => ShotFired?.Invoke();

    protected void BeginTelegraph(Vector2 destination)
    {
        WarningDestination = destination;
        IsTelegraphing = true;
        TelegraphStarted?.Invoke(destination);
    }

    protected void EndTelegraph()
    {
        if (!IsTelegraphing) return;
        IsTelegraphing = false;
        TelegraphFinished?.Invoke();
    }

    protected virtual void OnDisable() => EndTelegraph();
}
