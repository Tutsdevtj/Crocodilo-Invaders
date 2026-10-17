using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BattleSkyTools
{
    private const string Art = "Assets/Art/Environment/BattleSky/";
    private const string Prefab = "Assets/Prefabs/Environment/BattleSky.prefab";
    private static readonly string[] Scenes = {"PlayerMovementTest", "StageOneWaves", "GusiniBossTest"};
    [Serializable] private sealed class Layout { public Layer[] layers; public Mothership mothership; }
    [Serializable] private sealed class Layer
    {
        public string name, file;
        public int[] size_px;
        public int x_px_in_view, top_y_px_in_600x360_view, suggested_sortingOrder;
        public bool repeat_horizontal;
        public float suggested_scrollSpeed_units_per_s;
    }
    [Serializable] private sealed class Mothership { public Points damage_points_local_units_from_center; }
    [Serializable] private sealed class Points { public float[] breach_top, prow_hole, rear, dome, fin3_stump; }

    [MenuItem("Crocodilo/Environment/Install Battle Sky")]
    public static void Install()
    {
        BuildPrefab();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        foreach (string name in Scenes)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Tests/" + name + ".unity");
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            foreach (var t in all.Where(t => t.name == "SkyEnvironment" || t.name == "ScrollingGround" || (t.name == "Background" && t.GetComponent<BackgroundFit>() != null)).ToArray())
                UnityEngine.Object.DestroyImmediate(t.gameObject);
            foreach (var t in all.Where(t => t != null && t.name == "BattleSky").ToArray()) UnityEngine.Object.DestroyImmediate(t.gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.GetComponent<BackgroundFit>().FitToCamera();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        Validate();
    }

    [MenuItem("Crocodilo/Environment/Rebuild Battle Sky prefab")]
    public static void BuildPrefab()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 50; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = Path.GetFileName(path).StartsWith("fx_ms_smoke_") ? new Vector2(.18f, .06f) : Vector2.one * .5f;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var data = JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/BattleSky/background_layers.json"));
        var root = new GameObject("BattleSky");
        try
        {
            var fit = root.AddComponent<BackgroundFit>(); SpriteRenderer baseSky = null, mothership = null;
            foreach (var layer in data.layers)
            {
                Sprite sprite = Load("Layers/" + layer.name);
                float y = 3.6f - (layer.top_y_px_in_600x360_view + layer.size_px[1] * .5f) / 50f;
                if (layer.repeat_horizontal)
                {
                    var group = Node(layer.name, root.transform, new Vector2(0, y));
                    var segments = new SpriteRenderer[2];
                    for (int i = 0; i < segments.Length; i++) segments[i] = Renderer("Segment" + i, group, sprite, new Vector2(2 + i * 16, 0), layer.suggested_sortingOrder);
                    var scrolling = group.gameObject.AddComponent<ScrollingLayer>(); var fields = new SerializedObject(scrolling);
                    fields.FindProperty("scrollSpeed").floatValue = layer.suggested_scrollSpeed_units_per_s;
                    fields.FindProperty("alternateMirroring").boolValue = false; fields.FindProperty("spacing").floatValue = 0;
                    References(fields.FindProperty("segments"), segments); fields.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    float x = -6f + (layer.x_px_in_view + layer.size_px[0] * .5f) / 50f;
                    var renderer = Renderer(layer.name, root.transform, sprite, new Vector2(x, y), layer.suggested_sortingOrder);
                    if (layer.suggested_sortingOrder == -100) baseSky = renderer; else mothership = renderer;
                }
            }
            var fitFields = new SerializedObject(fit); fitFields.FindProperty("background").objectReferenceValue = baseSky; fitFields.ApplyModifiedPropertiesWithoutUndo();
            BuildMothership(mothership.transform, data.mothership.damage_points_local_units_from_center);
            BuildBattle(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        AssetDatabase.SaveAssets();
    }

    private static void BuildMothership(Transform parent, Points points)
    {
        Vector2 Point(float[] p) => new Vector2(p[0], p[1]);
        var effect = parent.gameObject.AddComponent<MothershipBackgroundFX>(); var fields = new SerializedObject(effect);
        var damagePoints = new[] {Point(points.breach_top), Point(points.prow_hole), Point(points.rear), Point(points.dome), Point(points.fin3_stump)};
        var property = fields.FindProperty("damagePoints"); property.arraySize = damagePoints.Length;
        for (int i = 0; i < damagePoints.Length; i++) property.GetArrayElementAtIndex(i).vector2Value = damagePoints[i];
        var smokePoints = new[] {damagePoints[0], damagePoints[2], damagePoints[4]};
        References(fields.FindProperty("smoke"), smokePoints.Select((p, i) => Renderer("Smoke" + i, parent, Load("FX/fx_ms_smoke_0"), p, -95, .75f)).ToArray());
        var glitches = new[] {damagePoints[3], damagePoints[1]}.Select((p, i) => Renderer("Glitch" + i, parent, Load("FX/fx_ms_glitch_0"), p, -95, .65f)).ToArray();
        foreach (var r in glitches) r.enabled = false; References(fields.FindProperty("glitches"), glitches);
        var explosion = Renderer("Explosion", parent, Load("FX/fx_ms_explosion_0"), Vector2.zero, -95, .75f); explosion.enabled = false;
        var flash = Renderer("Flash", parent, Load("FX/fx_ms_flash_0"), Vector2.zero, -94, .45f); flash.enabled = false;
        fields.FindProperty("explosion").objectReferenceValue = explosion; fields.FindProperty("flash").objectReferenceValue = flash;
        var debris = Enumerable.Range(0, 6).Select(i => Renderer("Debris" + i, parent, Load("FX/fx_ms_debris_0"), Vector2.zero, -95, .65f)).ToArray();
        foreach (var r in debris) r.enabled = false; References(fields.FindProperty("debris"), debris);
        Frames(fields.FindProperty("smokeFrames"), "FX/fx_ms_smoke_", 8); Frames(fields.FindProperty("explosionFrames"), "FX/fx_ms_explosion_", 6);
        Frames(fields.FindProperty("flashFrames"), "FX/fx_ms_flash_", 3); Frames(fields.FindProperty("glitchFrames"), "FX/fx_ms_glitch_", 4); Frames(fields.FindProperty("debrisFrames"), "FX/fx_ms_debris_", 4);
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildBattle(Transform parent)
    {
        var group = Node("DistantAirBattle", parent, Vector2.zero); var effect = group.gameObject.AddComponent<DistantAirBattle>(); var fields = new SerializedObject(effect);
        var names = new[] {"battle_human_fighter", "battle_human_bomber", "battle_alien_dart", "battle_alien_saucer"};
        var positions = new[] {new Vector2(-2.5f, 1.65f), new Vector2(-3.3f, 2.65f), new Vector2(2.3f, 1.8f), new Vector2(3.2f, 2.5f)};
        var ships = names.Select((name, i) => Renderer(name, group, Load("Battle/" + name), positions[i], -87, .7f)).ToArray();
        foreach (var ship in ships) ship.color = new Color(.624f, .722f, .847f, .7f);
        References(fields.FindProperty("ships"), ships);
        var shots = new[] {"battle_tracer", "battle_alien_shot"}.Select(name => Renderer(name, group, Load("Battle/" + name), Vector2.zero, -87, .55f)).ToArray();
        foreach (var shot in shots) { shot.color = new Color(.624f, .722f, .847f, .55f); shot.enabled = false; } References(fields.FindProperty("shots"), shots);
        var explosion = Renderer("BattleExplosion", group, Load("Battle/battle_explosion_0"), Vector2.zero, -87, .65f); explosion.enabled = false;
        var flak = Renderer("BattleFlak", group, Load("Battle/battle_flak_0"), Vector2.zero, -87, .65f); flak.enabled = false;
        fields.FindProperty("explosion").objectReferenceValue = explosion; fields.FindProperty("flak").objectReferenceValue = flak;
        Frames(fields.FindProperty("explosionFrames"), "Battle/battle_explosion_", 4); Frames(fields.FindProperty("flakFrames"), "Battle/battle_flak_", 3);
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem("Crocodilo/Environment/Validate Battle Sky")]
    public static void Validate()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        Require(prefab != null && prefab.GetComponent<BackgroundFit>() != null, "Fitted sky prefab");
        Require(prefab.GetComponentsInChildren<ScrollingLayer>(true).Length == 6, "Six repeating parallax layers");
        Require(prefab.GetComponentsInChildren<Collider2D>(true).Length == 0 && prefab.GetComponentsInChildren<AudioSource>(true).Length == 0, "Visual-only background");
        Require(prefab.GetComponentsInChildren<SpriteRenderer>(true).All(r => r.sortingOrder < 0), "Scenery renders behind gameplay");
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(importer.spritePixelsPerUnit == 50 && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "Native pixel import: " + path);
        }
        Require(Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories).Length == 46, "46 native runtime sprites only");
        foreach (string name in Scenes)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Tests/" + name + ".unity");
            var objects = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            Require(objects.Count(t => t.name == "BattleSky") == 1, "One new sky: " + name);
            Require(!objects.Any(t => t.name == "SkyEnvironment" || t.name == "AnimatedSun" || t.name == "ScrollingGround"), "Legacy visuals replaced: " + name);
            Require(objects.Any(t => t.name == "ArenaBounds"), "Gameplay arena retained: " + name);
        }
        Debug.Log("BATTLE SKY VALIDATION PASSED: eight layers, six scrolling loops, visual-only FX, 46 native sprites, three gameplay scenes.");
    }

    private static Transform Node(string name, Transform parent, Vector2 position)
    { var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = position; return t; }
    private static SpriteRenderer Renderer(string name, Transform parent, Sprite sprite, Vector2 position, int order, float alpha = 1f)
    {
        var r = Node(name, parent, position).gameObject.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sortingOrder = order; r.color = new Color(1, 1, 1, alpha); return r;
    }
    private static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + path + ".png") ?? throw new InvalidOperationException(path);
    private static void References(SerializedProperty property, SpriteRenderer[] renderers)
    { property.arraySize = renderers.Length; for (int i = 0; i < renderers.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i]; }
    private static void Frames(SerializedProperty property, string prefix, int count)
    { property.arraySize = count; for (int i = 0; i < count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = Load(prefix + i); }
    private static void Require(bool pass, string message) { if (!pass) throw new InvalidOperationException(message); }
}
