using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Boss))]
public sealed class BossReinforcements : MonoBehaviour
{
    [SerializeField] private Enemy interceptorPrefab;
    [SerializeField, Min(0f)] private float initialDelay = 6f;
    [SerializeField] private Vector2 intervalRange = new Vector2(9f, 12f);
    [SerializeField, Min(1)] private int batchSize = 2;
    [SerializeField, Min(1)] private int maxAlive = 2;
    private readonly HashSet<Enemy> active = new HashSet<Enemy>();
    private Boss boss;
    private EnemySpawner spawner;
    private float nextSpawnAt;
    private bool started;
    public int AliveCount => active.Count;

    private void Awake() => boss = GetComponent<Boss>();
    private void OnEnable() => boss.BattleStopped += StopReinforcements;
    public void Configure(EnemySpawner enemySpawner) => spawner = enemySpawner;

    private void Update()
    {
        if (Time.timeScale <= 0f || !boss.Ready || spawner == null || interceptorPrefab == null) return;
        if (!started)
        {
            started = true;
            nextSpawnAt = Time.time + initialDelay;
        }
        if (Time.time < nextSpawnAt) return;
        int room = Mathf.Max(0, maxAlive - active.Count);
        Enemy[] spawned = spawner.SpawnReinforcements(interceptorPrefab, Mathf.Min(batchSize, room), spawner.AliveCount + room);
        foreach (Enemy enemy in spawned)
        {
            if (enemy == null || enemy.Health.IsDead) continue;
            active.Add(enemy);
            enemy.Defeated += Release;
            enemy.Removed += Release;
        }
        float minimum = Mathf.Max(.1f, Mathf.Min(intervalRange.x, intervalRange.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(intervalRange.x, intervalRange.y));
        nextSpawnAt = Time.time + Random.Range(minimum, maximum);
    }

    private void Release(Enemy enemy)
    {
        active.Remove(enemy);
        enemy.Defeated -= Release;
        enemy.Removed -= Release;
    }

    private void StopReinforcements()
    {
        started = false;
        var remaining = new Enemy[active.Count];
        active.CopyTo(remaining);
        foreach (Enemy enemy in remaining)
        {
            if (enemy == null) continue;
            Release(enemy);
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }
        active.Clear();
    }

    private void OnDisable()
    {
        boss.BattleStopped -= StopReinforcements;
        StopReinforcements();
    }
}
