using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CardAudioPackageTools
{
    private const string Folder = "Assets/Audio/CardRewards/";
    private const string Prefab = "Assets/Prefabs/UI/CardRewards.prefab";

    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Folder, "*.wav").Select(p => p.Replace('\\', '/')))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = false;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var view = new SerializedObject(root.GetComponentInChildren<CardRewardView>(true));
            Assign(view, "revealSound", "CommonReveal");
            Assign(view, "chooseSound", "ChooseReward");
            Assign(view, "rerollSound", "Reroll");
            Assign(view, "legendaryTreasure", "LegendaryTreasureLoop");
            Assign(view, "mythicTreasure", "MythicTreasureLoop");
            view.FindProperty("legendaryTreasureVolume").floatValue = .24f;
            view.FindProperty("mythicTreasureVolume").floatValue = .28f;
            view.ApplyModifiedPropertiesWithoutUndo();
            foreach (var audio in root.GetComponentsInChildren<CardRarityAudio>(true))
            {
                var fields = new SerializedObject(audio);
                foreach (string rarity in new[] { "Rare", "Epic", "Legendary", "Mythic" })
                    Assign(fields, rarity.ToLowerInvariant(), rarity + "Reveal");
                fields.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void Assign(SerializedObject target, string field, string clip)
    {
        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + clip + ".wav");
        Require(audio != null, "Clip ausente: " + clip);
        target.FindProperty(field).objectReferenceValue = audio;
    }

    public static void Validate()
    {
        CardFineTuningTools.Validate();
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var view = new SerializedObject(root.GetComponentInChildren<CardRewardView>(true));
        foreach (string field in new[] { "revealSound", "chooseSound", "rerollSound", "legendaryTreasure", "mythicTreasure" })
        {
            var clip = view.FindProperty(field).objectReferenceValue as AudioClip;
            Require(clip != null && AssetDatabase.GetAssetPath(clip).StartsWith(Folder), "Áudio original: " + field);
        }
        foreach (string name in new[] { "LegendaryTreasureLoop", "MythicTreasureLoop" })
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + name + ".wav");
            Require(clip != null && Mathf.Abs(clip.length - 6.4f) < .01f, "Loop completo de tesouro: " + name);
            clip.LoadAudioData();
            float[] data = new float[clip.samples * clip.channels];
            Require(clip.GetData(data, 0), "PCM legível: " + name);
            Require(Mathf.Abs(data[0] - data[data.Length - 1]) < .001f && data.Max(v => Mathf.Abs(v)) < .45f,
                "Junção sem salto e pico seguro: " + name);
        }
        var audioSource = view.FindProperty("audioSource").objectReferenceValue as AudioSource;
        Require(audioSource != null && audioSource.outputAudioMixerGroup != null, "Mixer de efeitos preservado e herdado pelos loops");
        Debug.Log("CARD ORIGINAL AUDIO VALIDATION PASSED: 9 procedural clips, two clean loops, prefab assignments and existing mixer routing.");
    }

    private static void Require(bool value, string detail)
    { if (!value) throw new InvalidOperationException("Card package audio: " + detail); }
}
