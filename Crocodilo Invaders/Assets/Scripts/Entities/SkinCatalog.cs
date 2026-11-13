using System;
using UnityEngine;

// Cosmetic slots of the Hangar/wardrobe. Append new values at the end: saves store the name.
public enum SkinSlot { Ship = 0, Pilot = 1 }

[Serializable]
public sealed class SkinDefinition
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public SkinSlot slot;
    [Tooltip("Override of Player.controller. Empty keeps the controller saved in the prefab (original airplane).")]
    public RuntimeAnimatorController controller;
    [Tooltip("Idle frames and their durations, only for the Hangar preview.")]
    public Sprite[] previewFrames = new Sprite[0];
    public float[] previewDurations = new float[0];
    [Tooltip("Achievement id required to equip. Empty = available from the start.")]
    public string unlockAchievement;
    [Tooltip("Visible in the Hangar as a teaser; cannot be equipped yet.")]
    public bool comingSoon;
}

[CreateAssetMenu(menuName = "Crocodilo/Skin Catalog", fileName = "SkinCatalog")]
public sealed class SkinCatalog : ScriptableObject
{
    public const string ResourcePath = "Wardrobe/SkinCatalog";
    [SerializeField] private SkinDefinition[] skins = new SkinDefinition[0];
    public SkinDefinition[] Skins => skins;

    private static SkinCatalog loaded;
    public static SkinCatalog Load() => loaded != null ? loaded : (loaded = Resources.Load<SkinCatalog>(ResourcePath));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache() => loaded = null;

    public SkinDefinition Get(string id)
    {
        foreach (var skin in skins) if (skin != null && skin.id == id) return skin;
        return null;
    }

    // First entry of the slot that needs no unlock; the catalog lists the original first.
    public SkinDefinition Default(SkinSlot slot)
    {
        foreach (var skin in skins)
            if (skin != null && skin.slot == slot && !skin.comingSoon && string.IsNullOrEmpty(skin.unlockAchievement)) return skin;
        return null;
    }

    public int Count(SkinSlot slot)
    {
        int count = 0;
        foreach (var skin in skins) if (skin != null && skin.slot == slot) count++;
        return count;
    }

    public SkinDefinition At(SkinSlot slot, int index)
    {
        foreach (var skin in skins)
            if (skin != null && skin.slot == slot && index-- == 0) return skin;
        return null;
    }
}
