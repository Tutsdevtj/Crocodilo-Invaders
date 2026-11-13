using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Equipped cosmetics per slot, saved atomically next to the achievements (wardrobe.json).
public sealed class WardrobeStore
{
    [Serializable] sealed class Equipped { public string slot, id; }
    [Serializable] sealed class SaveData { public int version = 1; public List<Equipped> equipped = new List<Equipped>(); }
    SaveData data;
    bool validPrimary;
    public string FilePath { get; }
    public string Error { get; private set; }

    public WardrobeStore(string directory)
    {
        FilePath = Path.Combine(directory, "wardrobe.json");
        validPrimary = Read(FilePath, out data);
        if (!validPrimary && !Read(FilePath + ".bak", out data)) data = new SaveData();
    }

    public string Get(SkinSlot slot)
    {
        string key = slot.ToString();
        foreach (var e in data.equipped) if (e.slot == key) return e.id;
        return null;
    }

    public bool Set(SkinSlot slot, string id)
    {
        string key = slot.ToString(), previous = Get(slot);
        if (previous == id) return true;
        var entry = data.equipped.Find(e => e.slot == key);
        bool added = entry == null;
        if (added) { entry = new Equipped { slot = key }; data.equipped.Add(entry); }
        entry.id = id;
        if (Save()) return true;
        if (added) data.equipped.Remove(entry); else entry.id = previous;
        return false;
    }

    bool Read(string file, out SaveData result)
    {
        result = null;
        if (!File.Exists(file)) return false;
        try
        {
            result = JsonUtility.FromJson<SaveData>(File.ReadAllText(file, Encoding.UTF8));
            if (result == null || result.version != 1 || result.equipped == null) throw new InvalidDataException("Invalid wardrobe save");
            var slots = new HashSet<string>();
            foreach (var e in result.equipped)
                if (e == null || string.IsNullOrWhiteSpace(e.slot) || string.IsNullOrWhiteSpace(e.id) || !slots.Add(e.slot))
                    throw new InvalidDataException("Invalid wardrobe entry");
            return true;
        }
        catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException)
        { Error = "Não foi possível ler o hangar. Usando a cópia de segurança."; result = null; return false; }
    }

    bool Save()
    {
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
        { Error = "Não foi possível salvar o hangar."; Debug.LogWarning(Error + " " + e.Message); return false; }
    }
}

public static class Wardrobe
{
    static WardrobeStore store;
    public static WardrobeStore Store => store ?? (store = new WardrobeStore(Application.persistentDataPath));
    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { store = null; Changed = null; }
    public static void UseDirectory(string directory) { store = new WardrobeStore(directory); Changed?.Invoke(); }

    public static bool IsUnlocked(SkinDefinition skin) =>
        skin != null && !skin.comingSoon &&
        (string.IsNullOrEmpty(skin.unlockAchievement) || Achievements.Store.Get(skin.unlockAchievement).Unlocked);

    // Saved choice when still valid; otherwise the slot default (a removed or relocked skin never breaks the game).
    public static SkinDefinition Equipped(SkinSlot slot)
    {
        var catalog = SkinCatalog.Load();
        if (catalog == null) return null;
        var saved = catalog.Get(Store.Get(slot));
        return saved != null && saved.slot == slot && IsUnlocked(saved) ? saved : catalog.Default(slot);
    }

    public static bool Equip(SkinDefinition skin)
    {
        if (!IsUnlocked(skin) || !Store.Set(skin.slot, skin.id)) return false;
        Changed?.Invoke();
        return true;
    }

    // Called by the airplane before its Animator starts; original prefab controller is the fallback.
    public static void ApplyShip(Animator animator)
    {
        if (animator == null) return;
        var skin = Equipped(SkinSlot.Ship);
        if (skin != null && skin.controller != null && animator.runtimeAnimatorController != skin.controller)
            animator.runtimeAnimatorController = skin.controller;
    }
}
