using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CrocodiloLeaderboardValidation
{
    public static void Check(bool passed, string message)
    {
        if (!passed) throw new Exception("LEADERBOARD CHECK FAILED: " + message);
        Debug.Log("PASS: " + message);
    }

    [MenuItem("Crocodilo/Validate Leaderboard Storage")]
    public static void Storage()
    {
        string folder = Path.GetFullPath("Logs/LeaderboardTests/" + Guid.NewGuid().ToString("N"));
        var store = new LeaderboardStore(folder);
        Check(store.Entries.Count == 0, "New installation starts with an empty ranking");
        Check(LeaderboardStore.NormalizeName("   ") == "", "Blank names are rejected");
        Check(LeaderboardStore.NormalizeName("  João   Silva  ") == "João Silva", "Accents and spaces survive name normalization");
        Check(LeaderboardStore.NormalizeName(new string('A', 40)).Length == 16, "Names respect the length limit");
        Check(!LeaderboardStore.NormalizeName("<color=red>Arthur</color>\n").Contains('<'), "Markup and control characters cannot become UI tags");
        Check(store.RememberName("João"), "Last player name is persisted");
        var first = Entry("first", "Arthur", 100, 1);
        Check(store.Add(first), "First result saves");
        Check(store.Add(Entry("second", "Arthur", 900, 2)), "Same name can have multiple runs");
        Check(store.Add(Entry("tie", "Croco", 900, 3)), "Tied result saves");
        Check(store.Add(Entry("zero", "Zero", 0, 4)), "A completed zero-score run is valid");
        Check(store.Add(Entry("maximum", "Max", int.MaxValue, 5)), "Large scores save without sorting overflow");
        Check(store.Entries.Select(e => e.runId).SequenceEqual(new[] { "maximum", "second", "tie", "first", "zero" }), "Descending sort with stable chronological ties");
        Check(store.Add(first) && store.Entries.Count == 5, "Repeated save of the same run does not duplicate it");
        var reloaded = new LeaderboardStore(folder);
        Check(reloaded.Entries.Count == 5 && reloaded.LastPlayerName == "João", "Scores and name survive a new repository instance");
        File.WriteAllText(store.FilePath, "broken json");
        reloaded.Reload();
        Check(reloaded.Entries.Count == 5 && reloaded.Error != null, "Corrupt primary recovers valid backup");
        Check(reloaded.Add(Entry("recovered", "Recuperado", 500, 6)), "Recovered ranking can save again");
        File.WriteAllText(reloaded.FilePath, "broken again");
        reloaded.Reload();
        Check(reloaded.Entries.Count == 5, "Recovery preserves the valid backup instead of replacing it with corruption");
        string brokenFolder = Path.Combine(folder, "NoBackup"); Directory.CreateDirectory(brokenFolder);
        File.WriteAllText(Path.Combine(brokenFolder, "leaderboard.json"), "broken");
        Check(new LeaderboardStore(brokenFolder).Entries.Count == 0, "Missing backup and corrupt save do not crash the menu");
        string lockedFolder = Path.Combine(folder, "Blocked"); File.WriteAllText(lockedFolder, "directory path occupied by a file");
        var blocked = new LeaderboardStore(lockedFolder);
        Check(!blocked.Add(first) && blocked.Error != null && blocked.Entries.Count == 1, "Write failures retain the score and report a usable error");
        Check(!blocked.Add(first) && blocked.Entries.Count == 1, "Retry after write failure does not duplicate the score");
        Debug.Log("LEADERBOARD STORAGE VALIDATION PASSED");
    }

    public static LeaderboardEntry Entry(string id, string name, int points, long time) =>
        new LeaderboardEntry { runId = id, playerName = name, points = points, kills = 1, won = false, finishedUtcTicks = time };

    public static void Runtime()
    {
        Storage();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}
