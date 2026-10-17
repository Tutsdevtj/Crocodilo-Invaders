using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RogueliteValidation
{
    [MenuItem("Crocodilo/Validate Roguelite")]
    public static void Run()
    {
        ValidateCatalog(AssetDatabase.LoadAssetAtPath<UpgradeCardCatalog>("Assets/Data/UpgradeCards.asset"));
        ValidateInvalidPools();
        Debug.Log("ROGUELITE VALIDATION PASSED");
    }

    public static void ValidateCatalog(UpgradeCardCatalog catalog)
    {
        Require(catalog != null && catalog.IsValid, "O catálogo precisa de 25 cartas e pesos de raridade válidos.");
        foreach (float progress in new[] { 0f, .5f, 1f })
        {
            const int samples = 100000;
            var random = new System.Random(7321);
            var counts = new int[5];
            for (int i = 0; i < samples; i++) counts[(int)catalog.RarityForRoll(random.NextDouble(), progress)]++;
            RarityWeights weights = catalog.WeightsAt(progress);
            for (int i = 0; i < counts.Length; i++)
            {
                double probability = weights.Weight((UpgradeRarity)i) / weights.Total;
                double expected = samples * probability;
                double tolerance = Math.Max(12d, 6d * Math.Sqrt(samples * probability * (1d - probability)));
                Require(Math.Abs(counts[i] - expected) <= tolerance, $"Distribuição incorreta em progress={progress}, raridade={i}.");
            }
            for (int i = 0; i < 1000; i++)
                Require(catalog.RollOffers(random, progress).Select(card => card.kind).Distinct().Count() == 3,
                    "As três cartas precisam de tipos distintos.");
            Debug.Log($"RARITY 100000 ROLLS progress={progress}: " + string.Join(" / ", counts.Select(count => (count / 1000d).ToString("F3") + "%")));
        }
        var copy = UnityEngine.Object.Instantiate(catalog);
        try
        {
            SetWeights(copy, "earlyRunWeights", new RarityWeights(6f, 3f, .8f, .18f, .02f));
            RarityWeights nonPercent = copy.WeightsAt(0f);
            Require(Math.Abs(nonPercent.Total - 10d) < .0001d, "Pesos de teste somam 10.");
            var counts = new int[5];
            for (int i = 0; i < 10000; i++) counts[(int)copy.RarityForRoll((i + .5d) / 10000d, 0f)]++;
            Require(counts.SequenceEqual(new[] { 6000, 3000, 800, 180, 20 }), "Normalização de pesos que não somam 100.");
            SetWeights(copy, "earlyRunWeights", new RarityWeights(0f, 0f, 1f, 0f, 0f));
            Require(copy.RarityForRoll(0d, 0f) == UpgradeRarity.Epic && copy.RarityForRoll(.999999d, 0f) == UpgradeRarity.Epic,
                "Pesos zero não podem ser sorteados.");
            foreach (var invalid in new[] { new RarityWeights(), new RarityWeights(-1f, 1f, 1f, 1f, 1f),
                new RarityWeights(float.NaN, 1f, 1f, 1f, 1f), new RarityWeights(float.PositiveInfinity, 1f, 1f, 1f, 1f) })
            {
                SetWeights(copy, "earlyRunWeights", invalid);
                Require(!copy.IsValid, "Pesos inválidos precisam invalidar o catálogo.");
                bool rejected = false;
                try { copy.RollOffers(new System.Random(1), 0f); }
                catch (InvalidOperationException error) { rejected = error.Message.Contains("Weights"); }
                Require(rejected, "Pesos inválidos precisam gerar erro claro antes do sorteio.");
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
        Debug.Log("RARITY VALIDATION PASSED: distributions, distinct kinds, normalization, zero weights and invalid configuration.");
    }

    public static void ValidatePools(WaveController waves)
    {
        Require(waves.ValidateWaves(out string error), error);
        var serialized = new SerializedObject(waves);
        var pools = serialized.FindProperty("wavePools");
        var data = JsonUtility.FromJson<PoolData>(JsonUtility.ToJson(waves));
        var first = new System.Random(12345);
        var second = new System.Random(12345);
        for (int slot = 0; slot < pools.arraySize; slot++)
        {
            // JsonUtility reads a pure data copy; no runtime state or scene data is changed.
            EnemyWavePool pool = data.wavePools[slot];
            var visited = new System.Collections.Generic.HashSet<EnemyWave>();
            for (int run = 0; run < 200; run++)
            {
                EnemyWave selected = pool.Pick(first);
                Require(ReferenceEquals(selected, pool.Pick(second)), "A seed precisa reproduzir a sequência.");
                Require(pool.options.Contains(selected), "Uma onda só pode sortear opções do próprio slot.");
                visited.Add(selected);
            }
            Require(pool.options.Length == 1 || visited.Count > 1, "O pool precisa produzir variações nos testes de múltiplas runs.");
        }
        ValidateInvalidPools();
    }

    [Serializable] private sealed class PoolData { public EnemyWavePool[] wavePools; }
    private static void ValidateInvalidPools()
    {
        foreach (var pool in new[] { new EnemyWavePool(), new EnemyWavePool { options = Array.Empty<EnemyWave>() },
            new EnemyWavePool { options = new EnemyWave[] { null } }, new EnemyWavePool { options = new[] { new EnemyWave() } } })
            Require(!pool.Validate(out string error) && !string.IsNullOrEmpty(error), "Pool vazio/nulo/inválido precisa de diagnóstico.");
    }

    private static void SetWeights(UpgradeCardCatalog catalog, string name, RarityWeights weights)
    {
        var serialized = new SerializedObject(catalog);
        var property = serialized.FindProperty(name);
        property.FindPropertyRelative("common").floatValue = weights.common;
        property.FindPropertyRelative("rare").floatValue = weights.rare;
        property.FindPropertyRelative("epic").floatValue = weights.epic;
        property.FindPropertyRelative("legendary").floatValue = weights.legendary;
        property.FindPropertyRelative("mythic").floatValue = weights.mythic;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
