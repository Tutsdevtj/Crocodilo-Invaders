using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public static class GusiniPhaseTwoTools
{
    [Serializable] public class SpriteSpec { public string path, key; public float pivotX, pivotY; public int width, height; }
    [Serializable] public class EventSpec { public string value; public float time; }
    [Serializable] public class ClipSpec { public string name; public string[] frames; public int[] durations; public bool loop; public EventSpec[] events; }
    [Serializable] public class Manifest { public SpriteSpec[] sprites; public ClipSpec[] clips; }
    public const string Source = "SourceArt/GusiniPhase2/RuntimeManifest.json";
    private const string Animations = "Assets/Animations/Boss/GusiniPhase2/";
    private const string Data = "Assets/Data/GusiniPhaseTwo.asset";
    private const string BossPrefab = "Assets/Prefabs/Boss/Gusini.prefab";

    [MenuItem("Crocodilo/Boss/Install Gusini Phase 2")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source));
        foreach (var sprite in manifest.sprites)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(sprite.path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(sprite.pivotX, sprite.pivotY);
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 1024;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(sprite.pivotX, sprite.pivotY); importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        Directory.CreateDirectory(Animations); AssetDatabase.Refresh();
        foreach (var spec in manifest.clips) BuildClip(spec);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Animations + "GusiniPhaseTwo.controller");
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(Animations + "GusiniPhaseTwo.controller");
        var machine = controller.layers[0].stateMachine;
        foreach (var state in machine.states) machine.RemoveState(state.state);
        foreach (var spec in manifest.clips)
        {
            var state = machine.AddState(spec.name);
            state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(Animations + spec.name + ".anim");
            state.writeDefaultValues = false;
            if (spec.name == "P2Idle") machine.defaultState = state;
        }
        var data = AssetDatabase.LoadAssetAtPath<GusiniPhaseTwoAssets>(Data);
        if (data == null) { data = ScriptableObject.CreateInstance<GusiniPhaseTwoAssets>(); AssetDatabase.CreateAsset(data, Data); }
        data.sprites = manifest.sprites.Select(s => new GusiniPhaseTwoAssets.Entry { key = s.key, sprite = AssetDatabase.LoadAssetAtPath<Sprite>(s.path) }).ToArray();
        EditorUtility.SetDirty(data);
        var root = PrefabUtility.LoadPrefabContents(BossPrefab);
        try
        {
            var phase = root.GetComponent<GusiniPhaseTwo>() ?? root.AddComponent<GusiniPhaseTwo>();
            var fields = new SerializedObject(phase);
            fields.FindProperty("assets").objectReferenceValue = data;
            fields.FindProperty("phaseTwoController").objectReferenceValue = controller;
            fields.FindProperty("visual").objectReferenceValue = root.transform.Find("Visual").GetComponent<SpriteRenderer>();
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, BossPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        InstallBar(data);
        EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        Validate();
    }
    private static void BuildClip(ClipSpec spec)
    {
        string path = Animations + spec.name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.frameRate = 1000f;
        var keys = new ObjectReferenceKeyframe[spec.frames.Length + 1];
        float time = 0f;
        for (int i = 0; i < spec.frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = time, value = AssetDatabase.LoadAssetAtPath<Sprite>(spec.frames[i]) };
            time += spec.durations[i] / 1000f;
        }
        keys[keys.Length - 1] = new ObjectReferenceKeyframe { time = time, value = keys[keys.Length - 2].value };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = spec.loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AnimationUtility.SetAnimationEvents(clip, spec.events.Select(e => new AnimationEvent { time = e.time, functionName = "OnPhaseTwoEvent", stringParameter = e.value }).ToArray());
        EditorUtility.SetDirty(clip);
    }
    private static void InstallBar(GusiniPhaseTwoAssets data)
    {
        const string path = "Assets/Prefabs/UI/BossHUD.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var bar = root.GetComponentInChildren<BossBarFX>(true);
            var addon = bar.GetComponent<BossBarPhaseTwo>() ?? bar.gameObject.AddComponent<BossBarPhaseTwo>();
            var fields = new SerializedObject(addon);
            fields.FindProperty("assets").objectReferenceValue = data;
            fields.FindProperty("silhouetteMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/LoadingSilhouette.mat");
            var motion = bar.transform.Find("Motion");
            Image Image(string name, string sprite, Vector2 position, Vector2 size)
            {
                var child = motion.Find(name);
                if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5;
                rect.SetParent(motion, false); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = position; rect.sizeDelta = size;
                var image = rect.gameObject.AddComponent<Image>(); image.raycastTarget = false; image.sprite = data.Get(sprite); image.enabled = false;
                return image;
            }
            fields.FindProperty("eye").objectReferenceValue = Image("AlienEye", "emblem_eye_0", new Vector2(428, 26), new Vector2(12, 10));
            var motes = fields.FindProperty("motes"); motes.arraySize = 6;
            for (int i = 0; i < 6; i++) motes.GetArrayElementAtIndex(i).objectReferenceValue = Image("Mote " + i, "fx_mote_0", Vector2.zero, new Vector2(4, 6));
            fields.FindProperty("takeover").objectReferenceValue = Image("Takeover", "takeover_0", Vector2.zero, new Vector2(476, 100));
            fields.FindProperty("nameImage").objectReferenceValue = bar.GetComponentsInChildren<Image>(true).Single(i => i.name == "name_text");
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    [MenuItem("Crocodilo/Boss/Validate Gusini Phase 2")]
    public static void Validate()
    {
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source));
        Require(manifest.sprites.Length == 213 && manifest.clips.Length == 12, "Runtime asset count");
        Require(manifest.sprites.Select(s => s.key).Distinct().Count() == manifest.sprites.Length, "Unique sprite keys");
        foreach (var spec in manifest.sprites)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spec.path); Require(sprite != null, spec.path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(spec.path);
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "Pixel import " + spec.path);
            Require(sprite.rect.size == new Vector2(spec.width, spec.height), "Original dimensions " + spec.path);
            Require(Vector2.Distance(sprite.pivot, new Vector2(spec.pivotX * spec.width, spec.pivotY * spec.height)) < .001f, "Pivot " + spec.path);
        }
        foreach (var spec in manifest.clips)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Animations + spec.name + ".anim"); Require(clip != null, spec.name);
            Require(Mathf.Abs(clip.length - spec.durations.Sum() / 1000f) < .002f, "Clip duration " + spec.name);
            var events = AnimationUtility.GetAnimationEvents(clip);
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Require(bindings.Length == 1 && bindings[0].path == "Visual" && AnimationUtility.GetCurveBindings(clip).Length == 0, "Sprite-only clip " + spec.name);
            var keys = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
            Require(keys.Length == spec.frames.Length + 1, "Frame keys " + spec.name);
            int elapsed = 0;
            for (int i = 0; i < spec.frames.Length; i++)
            {
                Require(Mathf.Abs(keys[i].time - elapsed / 1000f) < .0001f && AssetDatabase.GetAssetPath(keys[i].value) == spec.frames[i], "Variable timing " + spec.name + "/" + i);
                elapsed += spec.durations[i];
            }
            Require(events.Length == spec.events.Length, "Event count " + spec.name);
            foreach (var expected in spec.events)
                Require(events.Any(e => e.functionName == "OnPhaseTwoEvent" && e.stringParameter == expected.value && Mathf.Abs(e.time - expected.time) < .0001f), "Event " + spec.name + "/" + expected.value);
        }
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefab);
        Require(boss.GetComponent<GusiniCombat>() != null && boss.GetComponent<GusiniPhaseTwo>() != null, "Both phase controllers");
        Require(boss.GetComponent<Animator>().runtimeAnimatorController.name == "Gusini", "F1 remains initial state");
        var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab");
        Require(hud.GetComponentInChildren<BossBarPhaseTwo>(true) != null, "F2 HUD binding");
        foreach (var component in new Component[] { boss.GetComponent<GusiniPhaseTwo>(), hud.GetComponentInChildren<BossBarPhaseTwo>(true) })
        {
            var property = new SerializedObject(component).GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference) Require(property.objectReferenceValue != null, "Missing reference " + component.GetType().Name + "/" + property.propertyPath);
        }
        foreach (var prefab in new[] { boss, hud })
            foreach (var node in prefab.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "Missing script " + node.name);
        Debug.Log("GUSINI PHASE TWO ASSET VALIDATION PASSED: 213 original sprites, 12 event-driven clips, both controllers and shared HUD.");
    }
    public static void ValidateIntegration()
    {
        Validate();
        GusiniBossTools.Validate();
        GusiniBossBarTools.Validate();
        CrocodiloWindowsBuild.ValidateProject();
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
