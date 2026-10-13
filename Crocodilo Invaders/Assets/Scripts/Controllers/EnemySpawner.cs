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
    private Vector2 currentInterval;
    private int currentMaxAlive;
    // -1 mantém o spawn contínuo nas cenas que ainda não usam ondas.
    private int spawnBudget = -1;
    private readonly Queue<Enemy> wavePrefabs = new Queue<Enemy>();
    private readonly HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    public int AliveCount => activeEnemies.Count;
    public int RemainingToSpawn => Mathf.Max(0, spawnBudget);
    public int MaxAlive => currentMaxAlive;
    public event Action<Enemy> EnemySpawned;

    public void BeginWave(int enemyCount)
    {
        wavePrefabs.Clear();
        currentInterval = intervalRange;
        currentMaxAlive = maxAliveEnemies;
        spawnBudget = Mathf.Max(0, enemyCount);
        nextSpawnTime = Time.time;
    }

    public void BeginWave(EnemyWave wave)
    {
        wavePrefabs.Clear();
        currentInterval = wave != null ? wave.spawnInterval : intervalRange;
        currentMaxAlive = wave != null ? Mathf.Max(2, wave.maxAlive) : maxAliveEnemies;
        if (wave != null && wave.groups != null)
            foreach (EnemyWave.Group group in wave.groups)
                if (group.prefab != null)
                    for (int i = 0; i < group.count; i++) wavePrefabs.Enqueue(group.prefab);
        spawnBudget = wavePrefabs.Count;
        nextSpawnTime = Time.time;
    }

    public void StopSpawning()
    {
        spawnBudget = 0;
        wavePrefabs.Clear();
    }

    public void StopCombat()
    {
        StopSpawning();
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null) continue;
            enemy.Movement.enabled = false;
            if (enemy.Combat != null) enemy.Combat.enabled = false;
        }
    }

    private void Awake()
    {
        currentInterval = intervalRange;
        currentMaxAlive = maxAliveEnemies;
        if (enemyPrefab != null) return;
        Debug.LogError("Configure o prefab do inimigo no EnemySpawner.", this);
        enabled = false;
    }

    private void OnEnable() => nextSpawnTime = Time.time + initialDelay;

    private void Update()
    {
        int batchSize = spawnBudget == 1 ? 1 : 2;
        if (Time.timeScale == 0f || Time.time < nextSpawnTime || enemyPrefab == null
            || spawnBudget == 0 || activeEnemies.Count + batchSize > currentMaxAlive
            || (playerHealth != null && playerHealth.IsDead)) return;

        if (batchSize == 2) SpawnPair();
        else SpawnAt(UnityEngine.Random.Range(Mathf.Min(heightRange.x, heightRange.y),
            Mathf.Max(heightRange.x, heightRange.y)));
        if (spawnBudget > 0) spawnBudget -= batchSize;
        float minimum = Mathf.Max(0.1f, Mathf.Min(currentInterval.x, currentInterval.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(currentInterval.x, currentInterval.y));
        nextSpawnTime = Time.time + UnityEngine.Random.Range(minimum, maximum);
    }

    private void SpawnPair()
    {
        Vector2 heights = PickPairHeights();
        SpawnAt(heights.x);
        SpawnAt(heights.y);
    }

    // Reforços usam a mesma área, alvo, registro de abates e limite das hordas.
    // Não consomem nem alteram o orçamento da onda.
    public Enemy[] SpawnReinforcements(Enemy prefab, int count, int aliveLimit)
    {
        if (!isActiveAndEnabled || prefab == null || Time.timeScale <= 0f
            || (playerHealth != null && playerHealth.IsDead)) return Array.Empty<Enemy>();
        int available = Mathf.Max(0, Mathf.Min(currentMaxAlive, aliveLimit) - activeEnemies.Count);
        int amount = Mathf.Clamp(count, 0, available);
        if (amount == 0) return Array.Empty<Enemy>();
        var spawned = new Enemy[amount];
        for (int index = 0; index < amount;)
        {
            Vector2 heights = PickPairHeights();
            spawned[index++] = SpawnAt(heights.x, prefab);
            if (index < amount) spawned[index++] = SpawnAt(heights.y, prefab);
        }
        return spawned;
    }

    private Vector2 PickPairHeights()
    {
        float bottom = Mathf.Min(heightRange.x, heightRange.y);
        float top = Mathf.Max(heightRange.x, heightRange.y);
        float gapMin = Mathf.Clamp(Mathf.Min(pairGapRange.x, pairGapRange.y), 0f, top - bottom);
        float gapMax = Mathf.Clamp(Mathf.Max(pairGapRange.x, pairGapRange.y), gapMin, top - bottom);
        float gap = UnityEngine.Random.Range(gapMin, gapMax);
        float center = UnityEngine.Random.Range(bottom + gap / 2f, top - gap / 2f);
        return new Vector2(center - gap / 2f, center + gap / 2f);
    }

    private Enemy SpawnAt(float height, Enemy reinforcementPrefab = null)
    {
        Vector3 position = transform.position + Vector3.up * height;
        Enemy prefab = reinforcementPrefab != null ? reinforcementPrefab
            : wavePrefabs.Count > 0 ? wavePrefabs.Dequeue() : enemyPrefab;
        Enemy enemy = Instantiate(prefab, position, Quaternion.identity, transform);
        enemy.Movement.ConfigurePatrol(patrolArea);
        if (enemy.Combat != null) enemy.Combat.SetTarget(playerHealth);
        activeEnemies.Add(enemy);
        enemy.Defeated += Release;
        enemy.Removed += Release;
        EnemySpawned?.Invoke(enemy);
        return enemy;
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
