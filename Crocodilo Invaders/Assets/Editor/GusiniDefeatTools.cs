using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Pacote Gusini_Derrota_F2_F1 v2: 18 quadros compostos + loop do Gusini exausto na nave avariada.
// Timings e eventos vêm de SourceArt/GusiniDefeat/gusini_defeat.json; os PNGs não são alterados.
public static class GusiniDefeatTools
{
    [Serializable] private class Frame { public int index; public string file; public int duration_ms; public int start_ms; public string tag; }
    [Serializable] private class Then { public string[] frames; public int frame_ms; }
    [Serializable] private class Manifest { public int version; public int total_ms; public Frame[] frames; public Then then; }
    private const string Source = "SourceArt/GusiniDefeat/gusini_defeat.json";
    private const string Art = "Assets/Art/Boss/GusiniDefeat/";
    private const string Animations = "Assets/Animations/Boss/GusiniDefeat/";
    public const string Controller = Animations + "GusiniDefeat.controller";
    private const string BossPrefab = "Assets/Prefabs/Boss/Gusini.prefab";
    // Mesmo canvas 256x196 e pivô (120,118) medido do topo de todo o boss.
    private static readonly Vector2 Pivot = new Vector2(120f / 256f, 78f / 196f);

    [MenuItem("Crocodilo/Boss/Install Gusini defeat (F2 → recovered)")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        Manifest manifest = Load();
        foreach (string path in AllSprites(manifest)) ConfigureSprite(path);
        Directory.CreateDirectory(Animations);
        AssetDatabase.Refresh();
        AnimationClip defeat = BuildClip("P2Defeat", manifest.frames.Select(f => SpritePath(f.file)).ToArray(),
            manifest.frames.Select(f => f.duration_ms).ToArray(), false,
            manifest.frames.Where(f => !string.IsNullOrEmpty(f.tag)).Select(f => (f.tag, f.start_ms / 1000f))
                .Append(("clip_end", manifest.total_ms / 1000f)).ToArray());
        AnimationClip idle = BuildClip("P1DefeatedIdle", manifest.then.frames.Select(SpritePath).ToArray(),
            manifest.then.frames.Select(_ => manifest.then.frame_ms).ToArray(), true, new (string, float)[0]);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller) ?? AnimatorController.CreateAnimatorControllerAtPath(Controller);
        var machine = controller.layers[0].stateMachine;
        foreach (var state in machine.states) machine.RemoveState(state.state);
        var first = machine.AddState("P2Defeat");
        first.motion = defeat; first.writeDefaultValues = false;
        var loop = machine.AddState("P1DefeatedIdle");
        loop.motion = idle; loop.writeDefaultValues = false;
        machine.defaultState = first;
        EditorUtility.SetDirty(controller);
        var root = PrefabUtility.LoadPrefabContents(BossPrefab);
        try
        {
            var component = root.GetComponent<GusiniDefeat>() ?? root.AddComponent<GusiniDefeat>();
            var fields = new SerializedObject(component);
            fields.FindProperty("defeatController").objectReferenceValue = controller;
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, BossPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static Manifest Load()
    {
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source));
        if (manifest == null || manifest.version != 2 || manifest.frames == null || manifest.frames.Length != 18)
            throw new InvalidDataException("Use o pacote Gusini_Derrota_F2_F1 v2 (nave avariada).");
        return manifest;
    }

    private static string SpritePath(string file) => Art + Path.GetFileName(file);
    private static string[] AllSprites(Manifest manifest) => manifest.frames.Select(f => SpritePath(f.file)).Concat(manifest.then.frames.Select(SpritePath)).ToArray();

    private static void ConfigureSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new FileNotFoundException(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 1024;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = Pivot; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static AnimationClip BuildClip(string name, string[] frames, int[] durations, bool loop, (string value, float time)[] events)
    {
        string path = Animations + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.frameRate = 1000f;
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        float time = 0f;
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = time, value = AssetDatabase.LoadAssetAtPath<Sprite>(frames[i]) };
            time += durations[i] / 1000f;
        }
        // Chave final repete o último quadro: a duração variável dele é respeitada até o fim do clip.
        keys[keys.Length - 1] = new ObjectReferenceKeyframe { time = time, value = keys[keys.Length - 2].value };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AnimationUtility.SetAnimationEvents(clip, events.Select(e => new AnimationEvent { time = e.time, functionName = "OnDefeatEvent", stringParameter = e.value }).ToArray());
        EditorUtility.SetDirty(clip);
        return clip;
    }

    [MenuItem("Crocodilo/Boss/Validate Gusini defeat")]
    public static void Validate()
    {
        Manifest manifest = Load();
        foreach (string path in AllSprites(manifest))
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(sprite != null && sprite.rect.size == new Vector2(256f, 196f) && Vector2.Distance(sprite.pivot, new Vector2(120f, 78f)) < .001f, "Canvas/pivô " + path);
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "Importação pixel " + path);
        }
        var defeat = AssetDatabase.LoadAssetAtPath<AnimationClip>(Animations + "P2Defeat.anim");
        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(Animations + "P1DefeatedIdle.anim");
        Require(defeat != null && Mathf.Abs(defeat.length - manifest.total_ms / 1000f) < .002f && !defeat.isLooping, "P2Defeat 1,9 s");
        Require(idle != null && idle.isLooping && Mathf.Abs(idle.length - .48f) < .002f, "Loop exausto 4 × 120 ms");
        var events = AnimationUtility.GetAnimationEvents(defeat);
        foreach (string tag in new[] { "defeat_hit", "parasite_burst", "gusini_recovered", "defeated_hold", "clip_end" })
            Require(events.Any(e => e.stringParameter == tag && e.functionName == "OnDefeatEvent"), "Evento " + tag);
        var lastKey = AnimationUtility.GetObjectReferenceCurve(defeat, EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite")).Last();
        Require(lastKey.value.name == "gusini_defeat_17", "Último quadro da derrota é a pose avariada F17");
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefab).GetComponent<GusiniDefeat>();
        Require(boss != null && new SerializedObject(boss).FindProperty("defeatController").objectReferenceValue != null, "GusiniDefeat no prefab");
        Debug.Log("GUSINI DEFEAT VALIDATION PASSED: 22 original v2 sprites, native 1.9 s clip with JSON events, damaged recovered loop.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("GUSINI DEFEAT CHECK FAILED: " + message);
    }
}
