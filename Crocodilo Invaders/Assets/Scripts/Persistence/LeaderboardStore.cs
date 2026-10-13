using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class LeaderboardEntry
{
    public string runId;
    public string playerName;
    public int points;
    public int kills;
    public bool won;
    public long finishedUtcTicks;
}

// A separate JSON file keeps a growing list of results out of PlayerPrefs.
public sealed class LeaderboardStore
{
    public const int NameLimit = 16;
    [Serializable] private sealed class SaveData
    {
        public int version = 1;
        public string lastPlayerName = "";
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private SaveData data;
    private bool validPrimary;
    private bool readOnly;
    public string FilePath { get; }
    public string Error { get; private set; }
    public string LastPlayerName => data.lastPlayerName;
    public IReadOnlyList<LeaderboardEntry> Entries => data.entries.AsReadOnly();

    public LeaderboardStore(string directory)
    {
        FilePath = Path.Combine(directory, "leaderboard.json");
        Reload();
    }

    public static string NormalizeName(string name)
    {
        var clean = new StringBuilder();
        foreach (char character in (name ?? "").Normalize(NormalizationForm.FormC))
        {
            if (clean.Length >= NameLimit) break;
            if (char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == '.') clean.Append(character);
            else if (character == ' ' && clean.Length > 0 && clean[clean.Length - 1] != ' ') clean.Append(' ');
        }
        return clean.ToString().Trim();
    }

    public void Reload()
    {
        Error = null;
        readOnly = false;
        validPrimary = TryRead(FilePath, out data);
        if (!validPrimary && !TryRead(FilePath + ".bak", out data)) data = new SaveData();
        Sort();
    }

    private bool TryRead(string path, out SaveData result)
    {
        result = null;
        if (!File.Exists(path)) return false;
        try
        {
            result = JsonUtility.FromJson<SaveData>(File.ReadAllText(path, Encoding.UTF8));
            if (result == null || result.entries == null || result.version != 1)
            {
                if (result != null && result.version > 1) readOnly = true;
                throw new InvalidDataException("Formato de ranking inválido.");
            }
            result.lastPlayerName = NormalizeName(result.lastPlayerName);
            result.entries = result.entries.Where(entry => entry != null && !string.IsNullOrEmpty(entry.runId)
                && entry.points >= 0 && entry.kills >= 0 && entry.finishedUtcTicks > 0
                && !string.IsNullOrEmpty(NormalizeName(entry.playerName)))
                .GroupBy(entry => entry.runId).Select(group => group.First()).ToList();
            foreach (var entry in result.entries) entry.playerName = NormalizeName(entry.playerName);
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        {
            Error = "Não foi possível ler o ranking. A cópia de segurança será usada quando disponível.";
            result = null;
            return false;
        }
    }

    public bool RememberName(string name)
    {
        string normalized = NormalizeName(name);
        if (normalized.Length == 0) return false;
        data.lastPlayerName = normalized;
        return Save();
    }

    public bool Add(LeaderboardEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.runId) || entry.points < 0 || entry.kills < 0
            || entry.finishedUtcTicks <= 0 || NormalizeName(entry.playerName).Length == 0) return false;
        if (!data.entries.Any(saved => saved.runId == entry.runId))
        {
            data.entries.Add(new LeaderboardEntry { runId = entry.runId, playerName = NormalizeName(entry.playerName),
                points = entry.points, kills = entry.kills, won = entry.won, finishedUtcTicks = entry.finishedUtcTicks });
            Sort();
        }
        // A retry after a failed disk write persists the same run without duplicating it.
        return Save();
    }

    private void Sort() => data.entries.Sort((a, b) =>
    {
        int comparison = b.points.CompareTo(a.points);
        if (comparison == 0) comparison = a.finishedUtcTicks.CompareTo(b.finishedUtcTicks);
        return comparison == 0 ? string.CompareOrdinal(a.runId, b.runId) : comparison;
    });

    private bool Save()
    {
        if (readOnly) { Error = "Este ranking foi criado por uma versão mais recente do jogo."; return false; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            string temporary = FilePath + ".tmp";
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(FilePath)) File.Replace(temporary, FilePath, validPrimary ? FilePath + ".bak" : null);
            else { File.Move(temporary, FilePath); File.Copy(FilePath, FilePath + ".bak", true); }
            validPrimary = true;
            Error = null;
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
        {
            Error = "Não foi possível salvar o ranking. Verifique o espaço e a permissão da pasta do jogo.";
            Debug.LogWarning(Error + " " + error.Message);
            return false;
        }
    }
}
