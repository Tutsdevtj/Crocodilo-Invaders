using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// A cosmetic override: base sprites, controller, timings and events stay intact.
public static class CrocsSkinTools
{
    public const string Art = "Assets/Art/Player/Crocs";
    public const string Animations = "Assets/Animations/Player/Crocs";
    public const string Controller = Animations + "/PlayerCrocs.overrideController";
    const string Prefab = "Assets/Prefabs/Player.prefab";
    static readonly string[] States = { "Idle", "Fly", "Shoot", "Special", "Hurt" };

    [MenuItem("Crocodilo/Skins/Save Crocs assets (not equipped)")]
    public static void Install()
    {
        Directory.CreateDirectory(Animations);
        AssetDatabase.Refresh();
        foreach (string file in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.spritePivot = new Vector2(.45f, 1f / 3f);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.45f, 1f / 3f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }
        var original = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/Player.controller");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Controller);
        if (controller == null)
        {
            controller = new AnimatorOverrideController(original);
            AssetDatabase.CreateAsset(controller, Controller);
        }
        controller.runtimeAnimatorController = original;
        var replacements = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (string state in States)
        {
            var source = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/Player" + state + ".anim");
            string targetPath = Animations + "/PlayerCrocs" + state + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, targetPath); }
            EditorUtility.CopySerialized(source, clip);
            clip.name = "PlayerCrocs" + state;
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                for (int i = 0; i < keys.Length; i++)
                {
                    string sourcePath = AssetDatabase.GetAssetPath(keys[i].value);
                    string skinPath = sourcePath.Replace("/Bombardiro/", "/Crocs/");
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(skinPath);
                    if (sprite == null) throw new InvalidOperationException("Missing skin frame: " + skinPath);
                    keys[i].value = sprite;
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            }
            EditorUtility.SetDirty(clip);
            replacements.Add(new KeyValuePair<AnimationClip, AnimationClip>(source, clip));
        }
        controller.ApplyOverrides(replacements);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("Crocodilo/Skins/Equip original airplane")]
    public static void EquipOriginal() => Equip(
        AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/Player.controller"),
        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Player/Bombardiro/Idle/bombardiro_idle_0.png"));

    static void Equip(RuntimeAnimatorController controller, Sprite idle)
    {
        var player = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            player.GetComponent<Animator>().runtimeAnimatorController = controller;
            player.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite = idle;
            PrefabUtility.SaveAsPrefabAsset(player, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(player); }
        AssetDatabase.SaveAssets();
    }

    public static void Validate()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Controller);
        if (controller == null || controller.overridesCount != 5) throw new Exception("Expected all five skin clips");
        var frames = new HashSet<Sprite>();
        var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        controller.GetOverrides(pairs);
        foreach (var pair in pairs)
        {
            var a = pair.Key; var b = pair.Value;
            if (b == null || !Mathf.Approximately(a.length, b.length) || a.isLooping != b.isLooping)
                throw new Exception("Skin clip timing mismatch: " + a.name);
            var eventsA = AnimationUtility.GetAnimationEvents(a);
            var eventsB = AnimationUtility.GetAnimationEvents(b);
            if (eventsA.Length != eventsB.Length) throw new Exception("Event count mismatch");
            for (int i = 0; i < eventsA.Length; i++)
                if (eventsA[i].time != eventsB[i].time || eventsA[i].functionName != eventsB[i].functionName)
                    throw new Exception("Attack event changed");
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(a))
            {
                var keysA = AnimationUtility.GetObjectReferenceCurve(a, binding);
                var keysB = AnimationUtility.GetObjectReferenceCurve(b, binding);
                if (keysA.Length != keysB.Length) throw new Exception("Frame count changed");
                for (int i = 0; i < keysA.Length; i++)
                {
                    var sprite = keysB[i].value as Sprite;
                    if (keysA[i].time != keysB[i].time || sprite == null || !AssetDatabase.GetAssetPath(sprite).StartsWith(Art + "/"))
                        throw new Exception("Invalid skin keyframe");
                    if (sprite.rect.size != new Vector2(160, 120) || sprite.pixelsPerUnit != 100 ||
                        Vector2.Distance(sprite.pivot, new Vector2(72, 40)) > .01f) throw new Exception("Sprite registration changed");
                    frames.Add(sprite);
                }
            }
        }
        if (frames.Count != 26) throw new Exception("Expected 26 unique skin frames, got " + frames.Count);
        Debug.Log("CROCS_SKIN_DATA_OK: 26 saved frames, five clips, timings, release events and pivots verified. No automatic equipment.");
    }
}
