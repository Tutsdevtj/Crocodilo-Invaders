using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Stable IDs and a versioned atomic save. Test instances use a separate directory.
public sealed class AchievementStore
{
    [Serializable] public sealed class Entry
    {
        public string id;
        public int progress, mask;
        public long unlockedUtc;
        public bool Unlocked => unlockedUtc > 0;
    }
    [Serializable] sealed class SaveData
    { public int version = 1; public List<Entry> entries = new List<Entry>(); }
    SaveData data;
    bool validPrimary, readOnly;
    public string FilePath { get; }
    public string Error { get; private set; }
    public AchievementStore(string directory)
    {
        FilePath = Path.Combine(directory, "achievements.json");
        validPrimary = Read(FilePath, out data);
        if (!validPrimary && !Read(FilePath + ".bak", out data)) data = new SaveData();
    }
    public Entry Get(string id)
    {
        var entry = data.entries.Find(e => e.id == id);
        // Return a snapshot, so external code cannot mutate unsaved state.
        return entry == null ? new Entry { id = id } : new Entry { id = id, progress = entry.progress, mask = entry.mask, unlockedUtc = entry.unlockedUtc };
    }
    public int UnlockedCount
    { get { int n = 0; foreach (var d in AchievementCatalog.All) if (Get(d.Id).Unlocked) n++; return n; } }
    public bool Set(string id, int progress, out bool unlocked) => Change(id, progress, 0, false, out unlocked);
    public bool Add(string id, int amount, out bool unlocked)
    { return Change(id, Get(id).progress + Mathf.Clamp(amount, 0, 100), 0, false, out unlocked); }
    public bool CollectCardKind(UpgradeKind kind, out bool unlocked)
    {
        unlocked = false;
        if ((int)kind < 0 || (int)kind > 4) return false;
        int mask = Get(AchievementCatalog.Cards).mask | (1 << (int)kind), count = 0;
        for (int i = 0; i < 5; i++) if ((mask & (1 << i)) != 0) count++;
        return Change(AchievementCatalog.Cards, count, mask, true, out unlocked);
    }
    bool Change(string id, int progress, int mask, bool useMask, out bool unlocked)
    {
        unlocked = false;
        var definition = AchievementCatalog.Get(id);
        if (definition == null) return false;
        if (id == AchievementCatalog.Cards && !useMask) return false;
        if (readOnly) { Error = "Progresso criado por uma versão mais recente do jogo."; return false; }
        var old = Get(id);
        if (old.Unlocked) return false;
        progress = Mathf.Clamp(progress, old.progress, definition.Goal);
        if (old.progress == progress && (!useMask || old.mask == mask)) return false;
        var current = data.entries.Find(e => e.id == id);
        bool added = current == null;
        if (added) { current = new Entry { id = id }; data.entries.Add(current); }
        current.progress = progress;
        if (useMask) current.mask = mask;
        if (progress >= definition.Goal) current.unlockedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!Save())
        {
            if (added) data.entries.Remove(current);
            else { current.progress = old.progress; current.mask = old.mask; current.unlockedUtc = old.unlockedUtc; }
            return false;
        }
        unlocked = current.Unlocked;
        return true;
    }
    bool Read(string file, out SaveData result)
    {
        result = null;
        if (!File.Exists(file)) return false;
        try
        {
            result = JsonUtility.FromJson<SaveData>(File.ReadAllText(file, Encoding.UTF8));
            if (result != null && result.version > 1) readOnly = true;
            if (result == null || result.version != 1 || result.entries == null) throw new InvalidDataException("Invalid achievements save");
            var ids = new HashSet<string>();
            foreach (var e in result.entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.id) || !ids.Add(e.id) || e.progress < 0 || e.unlockedUtc < 0)
                    throw new InvalidDataException("Invalid achievement entry");
                var d = AchievementCatalog.Get(e.id);
                if (d != null && (e.progress > d.Goal || (e.Unlocked && e.progress != d.Goal) || (!e.Unlocked && e.progress == d.Goal)))
                    throw new InvalidDataException("Invalid achievement progress");
                if (e.id == AchievementCatalog.Cards && (e.mask < 0 || e.mask > 31)) throw new InvalidDataException("Invalid card collection");
                if (e.id == AchievementCatalog.Cards)
                {
                    int count = 0; for (int i = 0; i < 5; i++) if ((e.mask & (1 << i)) != 0) count++;
                    if (count != e.progress) throw new InvalidDataException("Invalid card count");
                }
            }
            return true;
        }
        catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException)
        { Error = "Não foi possível ler as conquistas. Tentando a cópia de segurança."; result = null; return false; }
    }
    bool Save()
    {
        if (readOnly) { Error = "Progresso criado por uma versão mais recente do jogo."; return false; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            string temp = FilePath + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(FilePath)) File.Replace(temp, FilePath, validPrimary ? FilePath + ".bak" : null);
            else { File.Move(temp, FilePath); File.Copy(FilePath, FilePath + ".bak", true); }
            validPrimary = true; Error = null; return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        { Error = "Não foi possível salvar as conquistas."; Debug.LogWarning(Error + " " + e.Message); return false; }
    }
}
