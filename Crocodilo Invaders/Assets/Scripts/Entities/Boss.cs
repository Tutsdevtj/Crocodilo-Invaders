using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMovement))]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class Boss : MonoBehaviour
{
    [SerializeField] private string displayName = "BOSS";
    [SerializeField] private Sprite portrait;
    [SerializeField] private Rect patrolArea = new Rect(5.6f, -.2f, .8f, 3.8f);
    [SerializeField, Min(0)] private int pointsOnDefeat = 40000;
    [SerializeField, Min(.1f)] private float defeatDuration = 3f;

    public Health Health { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public IBossCombatController Combat { get; private set; }
    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public bool BattleActive { get; private set; }
    public bool Ready => BattleActive && Movement.HasArrived && !Health.IsDead;
    public int PointsOnDefeat => pointsOnDefeat;
    public float DefeatDuration => defeatDuration;
    public event Action<Boss> Defeated;
    public event Action BattleStopped;

    private Collider2D[] hitboxes;
    private Rigidbody2D body;

    private void Awake()
    {
        Health = GetComponent<Health>();
        Movement = GetComponent<EnemyMovement>();
        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
            if (component is IBossCombatController controller) { Combat = controller; break; }
        body = GetComponent<Rigidbody2D>();
        hitboxes = GetComponentsInChildren<Collider2D>();
        SetHitboxes(false);
        Health.SetInvulnerable(true);
    }

    private void OnEnable() => Health.Died += OnDied;
    private void OnDisable()
    {
        Health.Died -= OnDied;
        StopBattle();
    }

    public void BeginBattle(Health target)
    {
        if (Combat == null || target == null || target.IsDead || Health.IsDead) return;
        BattleActive = true;
        Movement.ConfigurePatrol(patrolArea);
        Combat.SetTarget(target);
    }

    private void Update()
    {
        // A entrada não causa dano nem permite acertar um boss fora da arena.
        if (Ready && !hitboxes[0].enabled)
        {
            Health.SetInvulnerable(false);
            SetHitboxes(true);
        }
    }

    public void StopBattle()
    {
        BattleActive = false;
        Movement.enabled = false;
        Combat?.StopCombat();
        Health.SetInvulnerable(true);
        SetHitboxes(false);
        BattleStopped?.Invoke();
    }

    private void OnDied()
    {
        StopBattle();
        body.linearVelocity = new Vector2(-.55f, -1.1f);
        Defeated?.Invoke(this);
        Destroy(gameObject, defeatDuration);
    }

    private void SetHitboxes(bool active)
    {
        foreach (Collider2D hitbox in hitboxes) hitbox.enabled = active;
    }
}
