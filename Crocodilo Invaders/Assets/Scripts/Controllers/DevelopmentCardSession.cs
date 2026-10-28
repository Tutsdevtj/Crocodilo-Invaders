using UnityEngine;

// Session-only controls: no PlayerPrefs, catalog edits or saved ranking entries.
public static class DevelopmentCardSession
{
    public enum OfferMode { Normal, FixedRarity, AllCards }
    public static bool IsActive { get; private set; }
    public static OfferMode Mode { get; set; }
    public static UpgradeRarity Rarity { get; set; } = UpgradeRarity.Common;
    public static int CompletedWave { get; set; } = 1;
    public static int SequenceCursor { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        IsActive = false;
        Mode = OfferMode.Normal;
        Rarity = UpgradeRarity.Common;
        CompletedWave = 1;
        SequenceCursor = 0;
    }

    public static void Begin() { IsActive = true; SequenceCursor = 0; }
    public static void End() { IsActive = false; SequenceCursor = 0; }
    public static void RestartSequence() => SequenceCursor = 0;

    public static UpgradeCard[] RollOffers(UpgradeCardCatalog catalog, System.Random random, float progress)
    {
        if (!IsActive || Mode == OfferMode.Normal) return catalog.RollOffers(random, progress);
        if (Mode == OfferMode.FixedRarity)
        {
            UpgradeCard[] offers = catalog.RollOffers(random, progress);
            for (int i = 0; i < offers.Length; i++) offers[i] = catalog.Get(offers[i].kind, Rarity);
            return offers;
        }
        var sequence = new UpgradeCard[3];
        for (int i = 0; i < sequence.Length; i++)
        {
            int slot = (SequenceCursor + i) % 25;
            sequence[i] = catalog.Get((UpgradeKind)(slot % 5), (UpgradeRarity)(slot / 5));
        }
        SequenceCursor = (SequenceCursor + sequence.Length) % 25;
        return sequence;
    }

    public static string ModeLabel => Mode == OfferMode.Normal ? "SORTEIO NORMAL"
        : Mode == OfferMode.FixedRarity ? "RARIDADE FIXA" : "CICLO DAS 25 CARTAS";
    public static string RarityLabel => new[] { "COMUM", "RARA", "ÉPICA", "LENDÁRIA", "MÍTICA" }[(int)Rarity];
}
