using System;
using UnityEngine;

public static class Achievements
{
    static AchievementStore store;
    public static AchievementStore Store => store ?? (store = new AchievementStore(Application.persistentDataPath));
    public static bool CanRecord => !DevelopmentBossSession.IsActive && !DevelopmentCardSession.IsActive && !DevelopmentStageSession.IsActive;
    public static event Action Changed;
    public static event Action<AchievementDefinition> Unlocked;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { store = null; Changed = null; Unlocked = null; }
    public static void UseDirectory(string directory) { store = new AchievementStore(directory); Changed?.Invoke(); }
    public static bool Award(string id) => Set(id, AchievementCatalog.Get(id)?.Goal ?? 0);
    public static bool Set(string id, int value)
    {
        if (!CanRecord || !Store.Set(id, value, out bool unlocked)) return false;
        Notify(id, unlocked); return true;
    }
    public static bool Add(string id, int amount)
    {
        if (!CanRecord || !Store.Add(id, amount, out bool unlocked)) return false;
        Notify(id, unlocked); return true;
    }
    public static void Card(UpgradeCard card)
    {
        if (!CanRecord || card == null) return;
        if (Store.CollectCardKind(card.kind, out bool unlocked)) Notify(AchievementCatalog.Cards, unlocked);
        if (card.rarity == UpgradeRarity.Mythic) Award(AchievementCatalog.Mythic);
    }
    static void Notify(string id, bool unlocked)
    { Changed?.Invoke(); if (unlocked) Unlocked?.Invoke(AchievementCatalog.Get(id)); }
}
