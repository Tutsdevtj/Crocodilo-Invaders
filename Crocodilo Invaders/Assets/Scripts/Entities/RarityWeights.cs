using System;
using UnityEngine;

[Serializable]
public struct RarityWeights
{
    [Min(0f)] public float common;
    [Min(0f)] public float rare;
    [Min(0f)] public float epic;
    [Min(0f)] public float legendary;
    [Min(0f)] public float mythic;

    public RarityWeights(float common, float rare, float epic, float legendary, float mythic)
    {
        this.common = common;
        this.rare = rare;
        this.epic = epic;
        this.legendary = legendary;
        this.mythic = mythic;
    }

    public double Total => (double)common + rare + epic + legendary + mythic;
    public bool IsValid => Valid(common) && Valid(rare) && Valid(epic) && Valid(legendary) && Valid(mythic) && Total > 0d;
    private static bool Valid(float weight) => weight >= 0f && !float.IsNaN(weight) && !float.IsInfinity(weight);

    public float Weight(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Common: return common;
            case UpgradeRarity.Rare: return rare;
            case UpgradeRarity.Epic: return epic;
            case UpgradeRarity.Legendary: return legendary;
            case UpgradeRarity.Mythic: return mythic;
            default: throw new ArgumentOutOfRangeException(nameof(rarity));
        }
    }

    public static RarityWeights Lerp(RarityWeights early, RarityWeights late, float progress)
        => new RarityWeights(Mathf.Lerp(early.common, late.common, progress),
            Mathf.Lerp(early.rare, late.rare, progress), Mathf.Lerp(early.epic, late.epic, progress),
            Mathf.Lerp(early.legendary, late.legendary, progress), Mathf.Lerp(early.mythic, late.mythic, progress));
}
