using UnityEngine;

public sealed class ArtilleryCombat : EnemyAttack
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Vector2 fireInterval = new Vector2(3.2f, 4.2f);
    [SerializeField, Min(.1f)] private float warningDuration = .9f;
    [SerializeField, Range(3, 9)] private int projectileCount = 5;
    [SerializeField, Range(10f, 100f)] private float fanAngle = 48f;
    [SerializeField, Min(.1f)] private float projectileSpeed = 3.6f;

    private float nextActionTime;
    private bool ready;

    private void OnEnable() => ready = false;

    private void Update()
    {
        if (!CanAttack || projectilePrefab == null || shotOrigin == null) return;
        if (!ready) { ready = true; ScheduleShot(); }
        if (Time.time < nextActionTime) return;
        if (!IsTelegraphing)
        {
            Enemy.Movement.HoldPosition(true);
            BeginTelegraph(Target.transform.position);
            nextActionTime = Time.time + warningDuration;
            return;
        }

        // A mira fica travada durante o aviso: os tiros não perseguem o jogador.
        Vector2 aim = WarningDestination - (Vector2)shotOrigin.position;
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        int count = Mathf.Max(3, projectileCount);
        for (int i = 0; i < count; i++)
        {
            float radians = (angle + Mathf.Lerp(-fanAngle / 2f, fanAngle / 2f, (float)i / (count - 1))) * Mathf.Deg2Rad;
            Projectile shot = Instantiate(projectilePrefab, shotOrigin.position, Quaternion.identity);
            shot.Launch(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)), OwnerCollider, projectileSpeed);
        }
        EndTelegraph();
        Enemy.Movement.HoldPosition(false);
        NotifyShotFired();
        ScheduleShot();
    }

    private void ScheduleShot() => nextActionTime = Time.time + Random.Range(
        Mathf.Max(.1f, Mathf.Min(fireInterval.x, fireInterval.y)),
        Mathf.Max(.1f, Mathf.Max(fireInterval.x, fireInterval.y)));

    protected override void OnDisable()
    {
        base.OnDisable();
        if (Enemy.Movement != null) Enemy.Movement.HoldPosition(false);
    }
}
