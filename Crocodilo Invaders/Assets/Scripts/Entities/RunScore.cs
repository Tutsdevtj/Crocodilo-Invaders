using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemySpawner))]
public sealed class RunScore : MonoBehaviour
{
    private EnemySpawner spawner;
    private readonly HashSet<Enemy> trackedEnemies = new HashSet<Enemy>();

    public int Points { get; private set; }
    public int Kills { get; private set; }
    public event Action<int, int> Changed;

    public void ResetScore()
    {
        Points = 0;
        Kills = 0;
        Changed?.Invoke(Points, Kills);
    }

    public void AwardPoints(int amount)
    {
        if (amount <= 0) return;
        Points += amount;
        Changed?.Invoke(Points, Kills);
    }

    private void Awake() => spawner = GetComponent<EnemySpawner>();
    private void OnEnable() => spawner.EnemySpawned += Track;

    private void OnDisable()
    {
        spawner.EnemySpawned -= Track;
        foreach (Enemy enemy in trackedEnemies)
        {
            if (enemy == null) continue;
            enemy.Defeated -= RecordDefeat;
            enemy.Removed -= Untrack;
        }
        trackedEnemies.Clear();
    }

    private void Track(Enemy enemy)
    {
        if (!trackedEnemies.Add(enemy)) return;
        enemy.Defeated += RecordDefeat;
        enemy.Removed += Untrack;
    }

    private void RecordDefeat(Enemy enemy)
    {
        RecordDefeat(enemy.PointsOnDefeat);
    }

    public void RecordDefeat(int points)
    {
        Points += Mathf.Max(0, points);
        Kills++;
        Changed?.Invoke(Points, Kills);
    }

    private void Untrack(Enemy enemy)
    {
        enemy.Defeated -= RecordDefeat;
        enemy.Removed -= Untrack;
        trackedEnemies.Remove(enemy);
    }
}
