using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public static class GusiniPhaseTwoAudioTools
{
    [Serializable] private class SoundSpec { public string id, path, sha256; public float duration_ms, suggested_volume; }
    [Serializable] private class Manifest { public SoundSpec[] sounds; }
    private const string Source = "SourceArt/GusiniAudioPhase2/RuntimeManifest.json";
    private const string ConfigPath = "Assets/Data/GusiniPhaseTwoAudio.asset";
    private static readonly string[] Ids = {
        "TR01", "TR02", "TR03", "TR04", "TR05", "TR06", "EN01", "EN02", "EN03",
        "EG01", "EG02", "EG03", "EG04", "SQ01", "SQ02", "SQ03", "SQ04", "SQ05", "SQ06", "SQ07", "SQ08",
        "TE01", "TE02", "TE03", "TE04", "TE05", "TE06", "TE07", "TE08"
    };

    [MenuItem("Crocodilo/Boss/Install Gusini Phase 2 audio")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source));
        foreach (var spec in manifest.sounds)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(spec.path);
            importer.forceToMono = false; importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true; settings.quality = 1f;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        var config = AssetDatabase.LoadAssetAtPath<GusiniAudioConfig>(ConfigPath);
        if (config == null) { config = ScriptableObject.CreateInstance<GusiniAudioConfig>(); AssetDatabase.CreateAsset(config, ConfigPath); }
        var fields = new SerializedObject(config);
        fields.FindProperty("voiceBudget").floatValue = .82f;
        var sounds = fields.FindProperty("sounds"); sounds.arraySize = Ids.Length;
        for (int i = 0; i < Ids.Length; i++)
        {
            var spec = manifest.sounds.Single(s => s.id == Ids[i]);
            GusiniSound id = (GusiniSound)((int)GusiniSound.P2FinalHit + i);
            var sound = sounds.GetArrayElementAtIndex(i);
            sound.FindPropertyRelative("id").intValue = (int)id;
            sound.FindPropertyRelative("group").intValue = (int)Group(id);
            sound.FindPropertyRelative("volume").floatValue = Volume(id, spec.suggested_volume);
            sound.FindPropertyRelative("pitchRange").vector2Value = Vector2.one;
            sound.FindPropertyRelative("maxVoices").intValue = MaxVoices(id);
            sound.FindPropertyRelative("cooldown").floatValue = id == GusiniSound.P2HonkMid ? 2f : id == GusiniSound.P2EnergyPop || id == GusiniSound.P2GoosePoof ? .04f : .025f;
            sound.FindPropertyRelative("loop").boolValue = false;
            sound.FindPropertyRelative("enabled").boolValue = id != GusiniSound.P2HonkLow && id != GusiniSound.P2HonkHigh;
            var clips = sound.FindPropertyRelative("variants"); clips.arraySize = 1;
            clips.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(spec.path);
        }
        fields.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(config);
        const string prefab = "Assets/Prefabs/Boss/Gusini.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefab);
        try
        {
            var audio = new SerializedObject(root.GetComponent<GusiniAudio>());
            audio.FindProperty("phaseTwoConfig").objectReferenceValue = config;
            audio.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets(); Validate(); GusiniAudioValidation.Validate();
    }
    private static GusiniSoundGroup Group(GusiniSound id)
    {
        switch (id)
        {
            case GusiniSound.P2HonkLow: case GusiniSound.P2HonkMid: case GusiniSound.P2HonkHigh: return GusiniSoundGroup.Chorus;
            case GusiniSound.P2EnergyPop: case GusiniSound.P2EggCrack: case GusiniSound.P2GoosePoof: return GusiniSoundGroup.Damage;
            case GusiniSound.P2DiveWhoosh: return GusiniSoundGroup.Flyby;
            case GusiniSound.P2EnergyCharge: case GusiniSound.P2EggPulse: case GusiniSound.P2CorruptCall: case GusiniSound.P2EdgeMark:
            case GusiniSound.P2ModuleCrack: case GusiniSound.P2TentacleEmerge: return GusiniSoundGroup.Telegraph;
            default: return GusiniSoundGroup.Danger;
        }
    }
    private static int MaxVoices(GusiniSound id)
        => id == GusiniSound.P2EnergyPop ? 3 : id == GusiniSound.P2GoosePoof || id == GusiniSound.P2DiverWarning || id == GusiniSound.P2DiveWhoosh ? 2 : 1;
    private static float Volume(GusiniSound id, float suggested)
    {
        if (id == GusiniSound.P2HonkLow || id == GusiniSound.P2HonkMid || id == GusiniSound.P2HonkHigh) return .07f;
        if (id == GusiniSound.P2EnergyPop || id == GusiniSound.P2GoosePoof) return suggested * .25f;
        if (id == GusiniSound.P2EggPulse) return suggested * .3f;
        if (id == GusiniSound.P2LaneWarning || id == GusiniSound.P2DiverWarning || id == GusiniSound.P2ParachutePop) return suggested * .55f;
        return suggested * .48f;
    }
    [MenuItem("Crocodilo/Boss/Validate Gusini Phase 2 audio")]
    public static void Validate()
    {
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source));
        var config = AssetDatabase.LoadAssetAtPath<GusiniAudioConfig>(ConfigPath);
        Require(config != null && config.Sounds.Length == 29 && manifest.sounds.Length == 29, "29 masters/config entries");
        Require((int)GusiniSound.Hit == 19 && (int)GusiniSound.P2FinalHit == 20, "Stable F1 IDs");
        Require(config.Sounds.Select(s => s.id).Distinct().Count() == 29, "Unique F2 IDs");
        foreach (var spec in manifest.sounds)
        {
            string hash;
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(spec.path))).Replace("-", "").ToLowerInvariant();
            Require(hash == spec.sha256, "Master unchanged: " + spec.id);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(spec.path);
            Require(clip != null && clip.channels == 1 && clip.frequency == 48000 && Mathf.Abs(clip.length - spec.duration_ms / 1000f) < .003f, "Native format/duration: " + spec.id);
            var settings = ((AudioImporter)AssetImporter.GetAtPath(spec.path)).defaultSampleSettings;
            Require(settings.loadType == AudioClipLoadType.DecompressOnLoad && settings.preloadAudioData && settings.compressionFormat == AudioCompressionFormat.PCM, "Preloaded PCM: " + spec.id);
        }
        foreach (var sound in config.Sounds)
            Require(!sound.loop && sound.variants.Length == 1 && sound.variants[0] != null && sound.volume > 0f && sound.volume <= .5f, "F2 sound profile: " + sound.id);
        Require(!config.Sounds.Single(s => s.id == GusiniSound.P2HonkLow).enabled && !config.Sounds.Single(s => s.id == GusiniSound.P2HonkHigh).enabled, "No restored goose chorus");
        Require(config.Sounds.Single(s => s.id == GusiniSound.P2HonkMid).maxVoices == 1 && config.Sounds.Single(s => s.id == GusiniSound.P2HonkMid).volume <= .1f, "Quiet single collective honk");
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab");
        Require(boss.GetComponent<GusiniAudio>().PhaseTwoConfig == config && new SerializedObject(boss.GetComponent<GusiniAudio>()).FindProperty("poolSize").intValue == 10, "Shared ten-voice pool, separate F2 config");
        Require(Directory.GetFiles("Assets/Audio/Boss/GusiniPhase2", "*.wav", SearchOption.AllDirectories).Length == 29, "Masters only, no previews or candidates");
        Debug.Log("GUSINI PHASE TWO AUDIO ASSET VALIDATION PASSED: 29 unchanged PCM masters, preloading, stable F1 IDs, separate config and shared limited pool.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
