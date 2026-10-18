using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public static class PlayerDashTools
{
    private const string Art = "Assets/Art/Player/DashVFX/";
    private const string Prefab = "Assets/Prefabs/Player.prefab";
    private const string DashAudio = "Assets/Audio/Player/SFX_Player_Dash.wav";
    [MenuItem("Crocodilo/Player/Install dash audio")]
    public static void InstallAudio()
    {
        AssetDatabase.Refresh();
        var importer = (AudioImporter)AssetImporter.GetAtPath(DashAudio);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        settings.preloadAudioData = true;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.SaveAndReimport();
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var fields = new SerializedObject(root.GetComponent<PlayerDashVFX>());
            fields.FindProperty("dashSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(DashAudio);
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
    }
    public static void InstallAudioAndValidatePlayMode() { InstallAudio(); PlayerDashPlayValidation.RunVisual(); }
    [Serializable] private class SpriteSpec { public string path, sha256; }
    [Serializable] private class Manifest { public SpriteSpec[] sprites; }
    [MenuItem("Crocodilo/Player/Install dash VFX")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = path.Contains("streak_h") ? new Vector2(128f/176f, .5f) : path.Contains("streak_d") ? new Vector2(118f/168f,118f/168f) : Vector2.one * .5f;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        const string materialPath = "Assets/Materials/DashAfterimage.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) { material = new Material(Shader.Find("Crocodilo/DashAfterimage")); AssetDatabase.CreateAsset(material, materialPath); }
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var dash = new SerializedObject(root.GetComponent<PlayerDash>());
            dash.FindProperty("baseDashDistance").floatValue = 2.2f;
            dash.FindProperty("baseDashDuration").floatValue = .16f;
            dash.FindProperty("baseCooldown").floatValue = 1.1f;
            dash.FindProperty("baseInvulnerabilityDuration").floatValue = .13f;
            dash.FindProperty("speedCurve").animationCurveValue = new AnimationCurve(new Keyframe(0,2,0,0),new Keyframe(.65f,.8f,-2,-2),new Keyframe(1,.2f,0,0));
            dash.ApplyModifiedPropertiesWithoutUndo();
            // Teste nativo: dash melhorado atravessou uma parede de 0,03 em 30Hz com Discrete.
            root.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var sources = root.GetComponentsInChildren<SpriteRenderer>(true);
            var visual = root.transform.Find("Visual").GetComponent<SpriteRenderer>();
            var controller = root.GetComponent<PlayerDashVFX>() ?? root.AddComponent<PlayerDashVFX>();
            var fields = new SerializedObject(controller);
            fields.FindProperty("ship").objectReferenceValue = visual;
            fields.FindProperty("afterimageMaterial").objectReferenceValue = material;
            var composition = fields.FindProperty("composition"); composition.arraySize = sources.Length;
            for (int i = 0; i < sources.Length; i++) composition.GetArrayElementAtIndex(i).objectReferenceValue = sources[i];
            // Mede somente o alpha dos PNGs existentes: não modifica nem redesenha sprites.
            var sprites = root.GetComponent<Animator>().runtimeAnimatorController.animationClips
                .SelectMany(clip => AnimationUtility.GetObjectReferenceCurveBindings(clip).SelectMany(binding => AnimationUtility.GetObjectReferenceCurve(clip,binding)))
                .Select(key => key.value as Sprite).Where(sprite => sprite != null).Concat(sources.Select(s => s.sprite)).Distinct().ToArray();
            var silhouettes = fields.FindProperty("silhouetteSprites"); silhouettes.arraySize = sprites.Length;
            var shades = fields.FindProperty("shadeThresholds"); shades.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                var pixels = new Texture2D(2,2); pixels.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprites[i])));
                var colors = pixels.GetPixels32(); int bottom = pixels.height, top = 0;
                for (int y = 0; y < pixels.height; y++) for (int x = 0; x < pixels.width; x++)
                    if (colors[y * pixels.width + x].a > 0) { bottom = Mathf.Min(bottom,y); top = Mathf.Max(top,y); }
                silhouettes.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
                shades.GetArrayElementAtIndex(i).floatValue = (bottom + .38f * (top - bottom + 1)) / pixels.height;
                UnityEngine.Object.DestroyImmediate(pixels);
            }
            foreach (var entry in new[] { ("ignitionHorizontal","dash_ignition","h",4), ("ignitionDiagonal","dash_ignition","d",4),
                ("streakHorizontal","dash_speed_streak","h",3), ("streakDiagonal","dash_speed_streak","d",3), ("endHorizontal","dash_end","h",4), ("endDiagonal","dash_end","d",4) })
            {
                var frames = fields.FindProperty(entry.Item1); frames.arraySize = entry.Item4;
                for (int i = 0; i < entry.Item4; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Art + entry.Item2 + "/" + entry.Item2 + "_" + entry.Item3 + "_" + i.ToString("00") + ".png");
            }
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets(); Validate();
    }
    [MenuItem("Crocodilo/Player/Validate dash VFX")]
    public static void Validate()
    {
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("SourceArt/DashVFX/RuntimeManifest.json"));
        Require(manifest.sprites.Length == 22, "22 original runtime sprites");
        foreach (var spec in manifest.sprites)
        {
            using (var sha = SHA256.Create()) Require(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(spec.path))).Replace("-", "").ToLowerInvariant() == spec.sha256, "Unchanged PNG " + spec.path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(spec.path);
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.spritePixelsPerUnit == 100 && importer.textureCompression == TextureImporterCompression.Uncompressed, "Pixel import " + spec.path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spec.path);
            Vector2 expected = spec.path.Contains("streak_h") ? new Vector2(128,56) : spec.path.Contains("streak_d") ? new Vector2(118,118) : sprite.rect.size / 2f;
            Require(Vector2.Distance(sprite.pivot, expected) < .001f, "Manifest pivot " + spec.path);
        }
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var dash = root.GetComponent<PlayerDash>(); var vfx = root.GetComponent<PlayerDashVFX>();
        Require(dash.BaseDashDistance == 2.2f && dash.BaseDashDuration == .16f && dash.BaseCooldown == 1.1f && dash.BaseInvulnerabilityDuration == .13f, "Requested base parameters");
        Require(vfx != null, "VFX on real aircraft prefab");
        var property = new SerializedObject(vfx).GetIterator();
        while (property.NextVisible(true)) if (property.propertyType == SerializedPropertyType.ObjectReference && property.propertyPath != "dashSound") Require(property.objectReferenceValue != null, "VFX reference " + property.propertyPath);
        Require(root.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic, "Dynamic Rigidbody preserved");
        Require(root.GetComponentsInChildren<Transform>(true).All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "No missing scripts");
        Debug.Log("PLAYER DASH ASSET VALIDATION PASSED: 22 unchanged sprites, precise pivots, pixel import, real player prefab, curve and requested base stats.");
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    public static void InstallAndValidatePlayMode() { Install(); PlayerDashPlayValidation.Run(); }
}
