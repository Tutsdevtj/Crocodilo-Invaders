using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeKind { AttackDamage, AttackSpeed, MaximumHealth, MoveSpeed, SpecialDamage }
public enum UpgradeRarity { Common, Rare, Epic, Legendary, Mythic }

[Serializable]
public sealed class UpgradeCard
{
    public UpgradeKind kind;
    public UpgradeRarity rarity;
    [Min(1)] public int percent;
    public string artworkPath;
}

[CreateAssetMenu(menuName = "Crocodilo/Upgrade Card Catalog")]
public sealed class UpgradeCardCatalog : ScriptableObject
{
    [SerializeField] private UpgradeCard[] cards;
    [SerializeField] private RarityWeights earlyRunWeights = new RarityWeights(70f, 24f, 5f, .9f, .1f);
    [SerializeField] private RarityWeights lateRunWeights = new RarityWeights(30f, 32f, 25f, 10f, 3f);
    private readonly Dictionary<string, Sprite> loadedArt = new Dictionary<string, Sprite>();
    public IReadOnlyList<UpgradeCard> Cards => cards;
    public int LoadedArtworkCount => loadedArt.Count;

    public bool IsValid
    {
        get
        {
            if (cards == null || cards.Length != 25 || !earlyRunWeights.IsValid || !lateRunWeights.IsValid) return false;
            var keys = new HashSet<int>();
            foreach (UpgradeCard card in cards)
            {
                if (card == null || card.percent <= 0 || string.IsNullOrEmpty(card.artworkPath)
                    || (int)card.kind < 0 || (int)card.kind > 4 || (int)card.rarity < 0 || (int)card.rarity > 4
                    || !keys.Add((int)card.kind * 5 + (int)card.rarity)) return false;
            }
            return true;
        }
    }

    public UpgradeCard Get(UpgradeKind kind, UpgradeRarity rarity)
    {
        foreach (UpgradeCard card in cards) if (card.kind == kind && card.rarity == rarity) return card;
        throw new InvalidOperationException("Carta ausente no catálogo.");
    }

    public UpgradeCard[] RollOffers(System.Random random, float progress = 0f)
    {
        if (random == null) throw new ArgumentNullException(nameof(random));
        WeightsAt(progress);
        if (!IsValid) throw new InvalidOperationException("O catálogo precisa conter os cinco buffs nas cinco raridades.");
        var kinds = new[] { UpgradeKind.AttackDamage, UpgradeKind.AttackSpeed, UpgradeKind.MaximumHealth, UpgradeKind.MoveSpeed, UpgradeKind.SpecialDamage };
        var offers = new UpgradeCard[3];
        for (int i = 0; i < offers.Length; i++)
        {
            int pick = random.Next(i, kinds.Length);
            (kinds[i], kinds[pick]) = (kinds[pick], kinds[i]);
            offers[i] = Get(kinds[i], RarityForRoll(random.NextDouble(), progress));
        }
        return offers;
    }

    public RarityWeights WeightsAt(float progress)
    {
        if (!earlyRunWeights.IsValid || !lateRunWeights.IsValid)
            throw new InvalidOperationException("Early/Late Run Weights precisam de pesos finitos, não negativos e soma maior que zero.");
        if (float.IsNaN(progress) || float.IsInfinity(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
        return RarityWeights.Lerp(earlyRunWeights, lateRunWeights, Mathf.Clamp01(progress));
    }

    // Cada carta tem seu próprio sorteio pelo peso total, sem exigir soma igual a 100.
    public UpgradeRarity RarityForRoll(double roll, float progress)
    {
        if (double.IsNaN(roll) || roll < 0d || roll >= 1d) throw new ArgumentOutOfRangeException(nameof(roll));
        RarityWeights weights = WeightsAt(progress);
        double threshold = roll * weights.Total;
        UpgradeRarity lastPositive = UpgradeRarity.Common;
        for (int i = 0; i < 5; i++)
        {
            var rarity = (UpgradeRarity)i;
            float weight = weights.Weight(rarity);
            if (weight > 0f) lastPositive = rarity;
            if (threshold < weight) return rarity;
            threshold -= weight;
        }
        return lastPositive;
    }

    public Sprite Artwork(UpgradeCard card)
    {
        if (loadedArt.TryGetValue(card.artworkPath, out Sprite sprite) && sprite != null) return sprite;
        Sprite[] sprites = Resources.LoadAll<Sprite>(card.artworkPath);
        if (sprites.Length != 1) throw new InvalidOperationException("Arte da carta não encontrada: " + card.artworkPath);
        loadedArt[card.artworkPath] = sprites[0];
        return sprites[0];
    }
}
