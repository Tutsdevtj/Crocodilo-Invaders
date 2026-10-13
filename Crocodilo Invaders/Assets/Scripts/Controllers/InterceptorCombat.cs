using UnityEngine;

public sealed class InterceptorCombat : EnemyAttack
{
    [SerializeField] private Vector2 attackInterval = new Vector2(3.5f, 4.5f);
    [SerializeField, Min(.1f)] private float warningDuration = .7f;
    [SerializeField, Min(.1f)] private float dashSpeed = 10f;
    [SerializeField, Min(0f)] private float recoveryDuration = .4f;
    [SerializeField] private Rect dashArea = new Rect(-8.2f, -2.3f, 10f, 7.4f);

    private enum Phase { Patrol, Warning, Dash, Recovery }
    private Phase phase;
    private float nextActionTime;
    private bool ready;

    private void OnEnable() { phase = Phase.Patrol; ready = false; }

    private void Update()
    {
        if (!CanAttack) return;
        if (!ready) { ready = true; ScheduleAttack(); }

        if (phase == Phase.Patrol && Time.time >= nextActionTime)
        {
            Vector2 origin = transform.position;
            Vector2 aim = Target.transform.position;
            float slope = (aim.y - origin.y) / Mathf.Min(-.1f, aim.x - origin.x);
            float y = Mathf.Clamp(origin.y + slope * (dashArea.xMin - origin.x),
                dashArea.yMin, dashArea.yMax);
            Enemy.Movement.HoldPosition(true);
            BeginTelegraph(new Vector2(dashArea.xMin, y));
            phase = Phase.Warning;
            nextActionTime = Time.time + warningDuration;
        }
        else if (phase == Phase.Warning && Time.time >= nextActionTime)
        {
            Vector2 destination = WarningDestination;
            EndTelegraph();
            Enemy.Movement.DashTo(destination, dashSpeed);
            phase = Phase.Dash;
            NotifyShotFired();
        }
        else if (phase == Phase.Dash && !Enemy.Movement.IsDashing)
        {
            phase = Phase.Recovery;
            nextActionTime = Time.time + recoveryDuration;
        }
        else if (phase == Phase.Recovery && Time.time >= nextActionTime)
        {
            Enemy.Movement.ReturnToPatrol();
            phase = Phase.Patrol;
            ready = false;
        }
    }

    private void ScheduleAttack() => nextActionTime = Time.time + Random.Range(
        Mathf.Max(.1f, Mathf.Min(attackInterval.x, attackInterval.y)),
        Mathf.Max(.1f, Mathf.Max(attackInterval.x, attackInterval.y)));

    protected override void OnDisable()
    {
        base.OnDisable();
        if (Enemy.Movement != null) Enemy.Movement.HoldPosition(false);
    }
}
