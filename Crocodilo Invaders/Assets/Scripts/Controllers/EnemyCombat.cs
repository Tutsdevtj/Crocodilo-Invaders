using UnityEngine;

[RequireComponent(typeof(Enemy))]
public sealed class EnemyCombat : EnemyAttack
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Vector2 fireIntervalRange = new Vector2(1.2f, 2.2f);

    private float nextShotTime;
    private bool ready;

    private void OnEnable() => ready = false;

    private void Update()
    {
        if (!CanAttack || projectilePrefab == null || shotOrigin == null) return;

        if (!ready)
        {
            ready = true;
            ScheduleNextShot();
            return;
        }
        if (Time.time < nextShotTime) return;

        Vector2 direction = (Vector2)Target.transform.position - (Vector2)shotOrigin.position;
        Projectile shot = Instantiate(projectilePrefab, shotOrigin.position, Quaternion.identity);
        shot.Launch(direction, OwnerCollider);
        ScheduleNextShot();
        NotifyShotFired();
    }

    private void ScheduleNextShot()
    {
        float minimum = Mathf.Max(0.1f, Mathf.Min(fireIntervalRange.x, fireIntervalRange.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(fireIntervalRange.x, fireIntervalRange.y));
        nextShotTime = Time.time + UnityEngine.Random.Range(minimum, maximum);
    }
}
