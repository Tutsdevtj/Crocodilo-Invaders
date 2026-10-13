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
    private readonly Dictionary<string, Sprite> loadedArt = new Dictionary<string, Sprite>();
    public IReadOnlyList<UpgradeCard> Cards => cards;
    public int LoadedArtworkCount => loadedArt.Count;

    public bool IsValid
    {
        get
        {
            if (cards == null || cards.Length != 25) return false;
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

    public UpgradeCard[] RollOffers(System.Random random)
    {
        if (!IsValid) throw new InvalidOperationException("O catálogo precisa conter os cinco buffs nas cinco raridades.");
        var kinds = new[] { UpgradeKind.AttackDamage, UpgradeKind.AttackSpeed, UpgradeKind.MaximumHealth, UpgradeKind.MoveSpeed, UpgradeKind.SpecialDamage };
        var offers = new UpgradeCard[3];
        for (int i = 0; i < offers.Length; i++)
        {
            int pick = random.Next(i, kinds.Length);
            (kinds[i], kinds[pick]) = (kinds[pick], kinds[i]);
            offers[i] = Get(kinds[i], RarityForRoll(random.Next(10000)));
        }
        return offers;
    }

    // Cada carta tem seu próprio sorteio; os tipos distintos não alteram as chances de raridade.
    public static UpgradeRarity RarityForRoll(int roll)
    {
        if (roll < 0 || roll >= 10000) throw new ArgumentOutOfRangeException(nameof(roll));
        return roll < 6000 ? UpgradeRarity.Common : roll < 8500 ? UpgradeRarity.Rare
            : roll < 9600 ? UpgradeRarity.Epic : roll < 9950 ? UpgradeRarity.Legendary : UpgradeRarity.Mythic;
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
