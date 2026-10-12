using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Vector2 heightRange = new Vector2(-1.4f, 5.2f);
    [SerializeField] private Vector2 pairGapRange = new Vector2(1.2f, 2.2f);
    [SerializeField] private Vector2 intervalRange = new Vector2(2.5f, 4f);
    [SerializeField, Min(0f)] private float initialDelay = 1f;
    [SerializeField] private Rect patrolArea = new Rect(3.2f, -1.4f, 4.5f, 6.6f);
    [SerializeField, Min(2)] private int maxAliveEnemies = 6;

    private float nextSpawnTime;
    private readonly HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    public event Action<Enemy> EnemySpawned;

    private void Awake()
    {
        if (enemyPrefab != null) return;
        Debug.LogError("Configure o prefab do inimigo no EnemySpawner.", this);
        enabled = false;
    }

    private void OnEnable() => nextSpawnTime = Time.time + initialDelay;

    private void Update()
    {
        if (Time.timeScale == 0f || Time.time < nextSpawnTime || enemyPrefab == null
            || activeEnemies.Count > maxAliveEnemies - 2
            || (playerHealth != null && playerHealth.IsDead)) return;

        SpawnPair();
        float minimum = Mathf.Max(0.1f, Mathf.Min(intervalRange.x, intervalRange.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(intervalRange.x, intervalRange.y));
        nextSpawnTime = Time.time + UnityEngine.Random.Range(minimum, maximum);
    }

    private void SpawnPair()
    {
        float bottom = Mathf.Min(heightRange.x, heightRange.y);
        float top = Mathf.Max(heightRange.x, heightRange.y);
        float gapMin = Mathf.Clamp(Mathf.Min(pairGapRange.x, pairGapRange.y), 0f, top - bottom);
        float gapMax = Mathf.Clamp(Mathf.Max(pairGapRange.x, pairGapRange.y), gapMin, top - bottom);
        float gap = UnityEngine.Random.Range(gapMin, gapMax);
        float center = UnityEngine.Random.Range(bottom + gap / 2f, top - gap / 2f);
        SpawnAt(center - gap / 2f);
        SpawnAt(center + gap / 2f);
    }

    private void SpawnAt(float height)
    {
        Vector3 position = transform.position + Vector3.up * height;
        Enemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
        enemy.Movement.ConfigurePatrol(patrolArea);
        if (enemy.Combat != null) enemy.Combat.SetTarget(playerHealth);
        activeEnemies.Add(enemy);
        enemy.Defeated += Release;
        enemy.Removed += Release;
        EnemySpawned?.Invoke(enemy);
    }

    private void Release(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        enemy.Defeated -= Release;
        enemy.Removed -= Release;
    }

    private void OnDestroy()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null) continue;
            enemy.Defeated -= Release;
            enemy.Removed -= Release;
        }
        activeEnemies.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up * heightRange.x,
            transform.position + Vector3.up * heightRange.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(patrolArea.center.x, patrolArea.center.y, transform.position.z),
            new Vector3(patrolArea.width, patrolArea.height, 0f));
    }
}
