using System;
using UnityEngine;

public static class LeaderboardSession
{
    private static LeaderboardStore store;
    private static string runId;
    private static string playerName;
    private static bool started;
    private static LeaderboardEntry pending;
    public static LeaderboardStore Store => store ?? (store = new LeaderboardStore(Application.persistentDataPath));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        store = null;
        runId = playerName = null;
        started = false;
        pending = null;
    }

    public static bool Prepare(string name)
    {
        name = LeaderboardStore.NormalizeName(name);
        if (name.Length == 0 || !SaveCompletedRun()) return false;
        runId = Guid.NewGuid().ToString("N");
        playerName = name;
        started = false;
        Store.RememberName(name);
        return true;
    }

    public static void StartRun() => started = !string.IsNullOrEmpty(runId);

    public static void Complete(int points, int kills, bool won)
    {
        if (!started || pending != null) return;
        pending = new LeaderboardEntry { runId = runId, playerName = playerName, points = points,
            kills = kills, won = won, finishedUtcTicks = DateTime.UtcNow.Ticks };
        started = false;
    }

    public static bool SaveCompletedRun()
    {
        if (pending == null) return true;
        if (!Store.Add(pending)) return false;
        pending = null;
        runId = playerName = null;
        return true;
    }

    public static void Cancel()
    {
        if (pending != null) return;
        runId = playerName = null;
        started = false;
    }
}
