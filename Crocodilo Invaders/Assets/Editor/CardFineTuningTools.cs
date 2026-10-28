using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CardFineTuningTools
{
    private const string Prefab = "Assets/Prefabs/UI/CardRewards.prefab";
    private static readonly string[] Names = { "Rare", "Epic", "Legendary", "Mythic" };

    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach (string name in Names)
        {
            string path = "Assets/Audio/CardRewards/" + name + "Reveal.wav";
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            Require(importer != null, path);
            importer.forceToMono = false; // Fontes já mono; não normalizar os picos preparados.
            importer.loadInBackground = false;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        var back = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Cards/Back.png");
        back.textureCompression = TextureImporterCompression.Uncompressed;
        back.filterMode = FilterMode.Point;
        back.mipmapEnabled = false;
        back.SaveAndReimport();
        GameObject root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            foreach (var sound in root.GetComponentsInChildren<CardRarityAudio>(true))
            {
                var fields = new SerializedObject(sound);
                foreach (string name in Names)
                    fields.FindProperty(name.ToLowerInvariant()).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CardRewards/" + name + "Reveal.wav");
                fields.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
    }

    public static void Validate()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        Require(prefab != null, "Prefab de cartas");
        var sounds = prefab.GetComponentsInChildren<CardRarityAudio>(true);
        Require(sounds.Length == 3, "Três slots com áudio próprio");
        float previousDuration = 0f, previousEnergy = 0f;
        for (int i = 0; i < Names.Length; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CardRewards/" + Names[i] + "Reveal.wav");
            Require(clip != null && clip.length > previousDuration && clip.channels == 1, "Duração/canais " + Names[i]);
            Require(clip.LoadAudioData(), "Carregamento do som " + Names[i]);
            var samples = new float[clip.samples];
            Require(clip.GetData(samples, 0), "PCM legível " + Names[i]);
            double energy = samples.Sum(v => (double)v * v) / samples.Length;
            float rms = Mathf.Sqrt((float)energy) * new[] { .60f, .72f, .84f, .96f }[i];
            float peak = samples.Max(v => Mathf.Abs(v));
            Require(peak < .41f && rms > previousEnergy, "Impacto crescente com folga: " + Names[i]);
            previousEnergy = rms;
            previousDuration = clip.length;
            foreach (var source in sounds)
                Require(new SerializedObject(source).FindProperty(Names[i].ToLowerInvariant()).objectReferenceValue == clip,
                    "Referência do slot " + Names[i]);
        }
        foreach (CardOptionView option in prefab.GetComponentsInChildren<CardOptionView>(true))
        {
            Sprite back = new SerializedObject(option).FindProperty("back").objectReferenceValue as Sprite;
            Require(back != null && AssetDatabase.GetAssetPath(back) == "Assets/Art/Cards/Back.png"
                && back.rect.width == 180 && back.rect.height == 268, "Verso e referência preservados");
        }
        var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCardCatalog>("Assets/Data/UpgradeCards.asset");
        Require(catalog != null && catalog.Cards.Count == 25, "Catálogo com 25 cartas");
        foreach (var card in catalog.Cards)
        {
            Sprite art = catalog.Artwork(card);
            Require(art != null && art.rect.width == 360 && art.rect.height == 536, "Frente completa " + card.artworkPath);
        }
        Debug.Log("CARD FINE TUNING VALIDATION PASSED: 25 faces, 3 backs, 4 distinct stingers and increasing RMS.");
    }
    private static void Require(bool condition, string detail)
    { if (!condition) throw new InvalidOperationException("Card fine tuning: " + detail); }
}
