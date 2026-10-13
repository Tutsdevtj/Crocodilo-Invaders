using System;
using UnityEngine;

[Serializable]
public sealed class EnemyWave
{
    [Serializable]
    public struct Group
    {
        public Enemy prefab;
        [Min(1)] public int count;
    }

    // Os grupos entram nesta ordem, preservando a apresentação de cada inimigo novo.
    public Group[] groups;
    [Tooltip("Intervalo entre grupos de spawn nesta onda, em segundos.")]
    public Vector2 spawnInterval = new Vector2(2.5f, 4f);
    [Min(2)] public int maxAlive = 6;
    public bool IsValid
    {
        get
        {
            if (groups == null || groups.Length == 0 || spawnInterval.x <= 0f
                || spawnInterval.y <= 0f || maxAlive < 2) return false;
            foreach (Group group in groups)
                if (group.prefab == null || group.count < 1) return false;
            return true;
        }
    }
}
