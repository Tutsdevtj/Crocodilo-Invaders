using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMovement))]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class Boss : MonoBehaviour
{
    [SerializeField] private string displayName = "BOSS";
    [SerializeField] private Sprite portrait;
    [SerializeField, Tooltip("Apresentação PERIGO! antes da entrada. Vazio: só o anúncio da onda.")] private BossAlertTheme alertTheme;
    [SerializeField] private Rect patrolArea = new Rect(5.6f, -.2f, .8f, 3.8f);
    [SerializeField, Min(0)] private int pointsOnDefeat = 40000;
    [SerializeField, Min(.1f)] private float defeatDuration = 3f;

    public Health Health { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public IBossCombatController Combat { get; private set; }
    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public BossAlertTheme AlertTheme => alertTheme;
    public bool BattleActive { get; private set; }
    public bool Transitioning { get; private set; }
    // Uma cena pode segurar o início do combate; a entrada continua normalmente.
    public bool CombatHeld { get; private set; }
    public bool Ready => BattleActive && !Transitioning && !CombatHeld && Movement.HasArrived && !Health.IsDead;
    public int PointsOnDefeat => pointsOnDefeat;
    public float DefeatDuration => defeatDuration;
    public event Action<Boss> Defeated;
    public event Action BattleStopped;

    private Collider2D[] hitboxes;
    private Rigidbody2D body;
    private bool holdingForCombat;

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

    public void HoldCombat(bool hold)
    {
        if (CombatHeld == hold) return;
        CombatHeld = hold;
        if (holdingForCombat && !hold && !Transitioning) Movement.HoldPosition(false);
        holdingForCombat = false;
    }

    private void Update()
    {
        // Parado durante a conversa, sem trocar a lógica de entrada ou patrulha.
        if (CombatHeld && !holdingForCombat && BattleActive && Movement.HasArrived)
        {
            holdingForCombat = true;
            Movement.HoldPosition(true);
        }
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

    public void SetTransitioning(bool value)
    {
        Transitioning = value;
        Movement.HoldPosition(value);
        Health.SetInvulnerable(value);
        SetHitboxes(!value && BattleActive);
    }

    // Um boss resgatado (Gusini) continua em cena após a derrota; quem o retém cuida da apresentação.
    public bool RetainedAfterDefeat { get; private set; }
    public void RetainAfterDefeat() => RetainedAfterDefeat = true;

    private void OnDied()
    {
        StopBattle();
        if (!RetainedAfterDefeat) body.linearVelocity = new Vector2(-.55f, -1.1f);
        Defeated?.Invoke(this);
        if (!RetainedAfterDefeat) Destroy(gameObject, defeatDuration);
    }

    private void SetHitboxes(bool active)
    {
        foreach (Collider2D hitbox in hitboxes) hitbox.enabled = active;
    }
}
