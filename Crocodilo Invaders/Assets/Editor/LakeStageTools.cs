using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class LakeStageTools
{
    public const string Stage = "Assets/Scenes/StageTwoLake.unity";
    public const string Test = "Assets/Scenes/Tests/SapoDoPneuTest.unity";
    public const string BossPath = "Assets/Prefabs/Boss/SapoDoPneu.prefab";
    private const string Art = "Assets/Art/LakeStage/";
    private static readonly string[] LayerNames = { "01_ceu", "02_nuvens_distantes", "03_margem_distante", "04_vegetacao_intermediaria", "05_margem_proxima", "06_agua_base", "07_superficie_agua", "08_primeiro_plano" };
    [MenuItem("Crocodilo/Lago do Pneu/Upgrade scrolling and water drops v03")]
    public static void UpgradeScrollingAndDrops()
    {
        UpgradeWaterMotion();
        AssetDatabase.Refresh(); Import(Art + "gota-lago-v03.png", 1000, Vector2.one * .5f);
        var root = PrefabUtility.LoadPrefabContents(BossPath);
        try
        {
            JungleStageTools.Set(root.GetComponent<SapoDoPneuCombat>(), "dropletSprite", AssetDatabase.LoadAssetAtPath<Sprite>(Art + "gota-lago-v03.png"));
            PrefabUtility.SaveAsPrefabAsset(root, BossPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("LAKE_SCROLL_DROPS_V03_INSTALL_OK");
    }
    [MenuItem("Crocodilo/Lago do Pneu/Upgrade fluid water v02")]
    public static void UpgradeWaterMotion()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        var motion = Material("Assets/Materials/LakeLayerMotion.mat", "Crocodilo/LakeLayerMotion");
        var contact = Material("Assets/Materials/LakeWaterContact.mat", "Crocodilo/LakeWaterContact");
        Require(!ShaderUtil.ShaderHasError(motion.shader) && !ShaderUtil.ShaderHasError(contact.shader), "Lake shaders compile");
        var root = PrefabUtility.LoadPrefabContents(BossPath);
        try
        {
            var view = root.transform.Find("SapoVisual").GetComponent<SpriteRenderer>();
            var water = root.GetComponent<LakeBossWaterContact>();
            if (water == null) water = root.AddComponent<LakeBossWaterContact>();
            JungleStageTools.Set(water, "visual", view); JungleStageTools.Set(water, "contactMaterial", contact);
            PrefabUtility.SaveAsPrefabAsset(root, BossPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        foreach (string path in new[] { Stage, Test })
        {
            var scene = EditorSceneManager.OpenScene(path); var lake = Find<LakeBackdrop>(scene);
            JungleStageTools.Set(lake, "motionMaterial", motion);
            JungleStageTools.Set(lake, "waterFXMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));
            if (lake.GetComponent<LakeSurfaceMotion>() == null) lake.gameObject.AddComponent<LakeSurfaceMotion>();
            lake.enabled = false; lake.enabled = true;
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(Stage);
        Debug.Log("LAKE_WATER_V02_INSTALL_OK");
    }
    private static Material Material(string path, string shaderName)
    {
        var shader = Shader.Find(shaderName); Require(shader != null, "Missing shader " + shaderName);
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        else { material.shader = shader; EditorUtility.SetDirty(material); }
        return material;
    }
    [MenuItem("Crocodilo/Lago do Pneu/Install stage 2")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        foreach (string id in LayerNames) Import(Art + id + ".png", 100, Vector2.one * .5f);
        Import(Art + "sapo-master.png", 300, new Vector2(.5f, .38f));
        Import(Art + "gota.png", 1000, Vector2.one * .5f);
        BuildBoss();
        var source = EditorSceneManager.OpenScene("Assets/Scenes/StageTwoJungle.unity");
        // Save a separate scene before mutating: the complete Macaquini stage remains available.
        EditorSceneManager.SaveScene(source, Stage);
        var scene = SceneManager.GetActiveScene();
        foreach (var backdrop in All<JungleBackdrop>(scene)) Object.DestroyImmediate(backdrop.gameObject);
        foreach (var intro in All<RunCutscenes>(scene)) Object.DestroyImmediate(intro);
        foreach (var start in All<BossDevelopmentStart>(scene)) Object.DestroyImmediate(start.gameObject);
        var waves = Find<WaveController>(scene);
        JungleStageTools.Set(waves, "bossPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(BossPath).GetComponent<Boss>());
        JungleStageTools.Set(waves, "cutscenes", null);
        JungleStageTools.Set(waves, "completeChapterOnBossDefeat", true);
        // Existing waves, cards, spawner, player and pause retain their working native references.
        var chapter = ScriptableObject.CreateInstance<ChapterDefinition>();
        const string chapterPath = "Assets/Data/Flow/Chapter2Lake.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ChapterDefinition>(chapterPath);
        if (existing != null) { Object.DestroyImmediate(chapter); chapter = existing; }
        else AssetDatabase.CreateAsset(chapter, chapterPath);
        JungleStageTools.Set(chapter, "chapterId", "chapter2_lake");
        JungleStageTools.Set(chapter, "nextScene", "BaseImprovisada");
        JungleStageTools.Set(chapter, "unavailableMessage", "LAGO DO PNEU CONCLUÍDO — RETORNO À BASE");
        var data = new SerializedObject(chapter); var flags = data.FindProperty("completionFlags"); flags.arraySize = 2;
        flags.GetArrayElementAtIndex(0).stringValue = "lago_do_pneu_completed";
        flags.GetArrayElementAtIndex(1).stringValue = "chapter2_completed"; data.ApplyModifiedPropertiesWithoutUndo();
        JungleStageTools.Set(Find<ChapterFlow>(scene), "chapter", chapter);
        var root = new GameObject("LakeBackdrop"); var lake = root.AddComponent<LakeBackdrop>();
        JungleStageTools.Set(lake, "gameplayCamera", Find<Camera>(scene));
        data = new SerializedObject(lake); var layers = data.FindProperty("layers"); layers.arraySize = 8;
        for (int i = 0; i < 8; i++) layers.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Art + LayerNames[i] + ".png");
        data.ApplyModifiedPropertiesWithoutUndo();
        // ExecuteAlways rebuilds its registered layer children after deserialization.
        lake.enabled = false; lake.enabled = true;
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.SaveScene(scene, Test, true);
        var test = EditorSceneManager.OpenScene(Test);
        new GameObject("LakeBossTestStart").AddComponent<LakeBossTestStart>();
        EditorSceneManager.SaveScene(test);
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == Stage)) scenes.Add(new EditorBuildSettingsScene(Stage, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets(); Validate();
        UpgradeScrollingAndDrops();
        LakeHudArtTools.ImportExpressions();
        EditorSceneManager.OpenScene(Stage);
        Debug.Log("LAKE_STAGE_INSTALL_OK");
    }
    private static void BuildBoss()
    {
        var root = new GameObject("SapoDoPneu"); root.layer = LayerMask.NameToLayer("Enemy");
        var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var box = root.AddComponent<BoxCollider2D>(); box.isTrigger = true;
        box.size = new Vector2(2.85f, 2.65f); box.offset = new Vector2(0, .3f);
        var health = root.AddComponent<Health>(); JungleStageTools.Set(health, "maxHealth", 200);
        var movement = root.AddComponent<EnemyMovement>(); JungleStageTools.Set(movement, "moveSpeed", 4f);
        var boss = root.AddComponent<Boss>(); JungleStageTools.Set(boss, "displayName", "SAPO DO PNEU");
        JungleStageTools.Set(boss, "defeatDuration", 2f);
        var data = new SerializedObject(boss); data.FindProperty("patrolArea").rectValue = new Rect(4, -1.5f, .05f, .05f); data.ApplyModifiedPropertiesWithoutUndo();
        var view = new GameObject("SapoVisual").AddComponent<SpriteRenderer>(); view.transform.SetParent(root.transform, false);
        view.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "sapo-master.png"); view.sortingOrder = 15;
        var combat = root.AddComponent<SapoDoPneuCombat>();
        JungleStageTools.Set(combat, "visual", view);
        JungleStageTools.Set(combat, "dropletSprite", AssetDatabase.LoadAssetAtPath<Sprite>(Art + "gota-lago-v03.png"));
        JungleStageTools.Set(combat, "hudFont", AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset"));
        Directory.CreateDirectory("Assets/Prefabs/Boss"); PrefabUtility.SaveAsPrefabAsset(root, BossPath); Object.DestroyImmediate(root);
    }
    private static void Import(string path, float ppu, Vector2 pivot)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new InvalidOperationException("Missing lake art: " + path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot;
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
    }
    public static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    public static T Find<T>(Scene scene) where T : Component => All<T>(scene).FirstOrDefault();
    [MenuItem("Crocodilo/Lago do Pneu/Validate stage")]
    public static void Validate()
    {
        var scene = EditorSceneManager.OpenScene(Stage); var waves = Find<WaveController>(scene);
        Require(waves != null && waves.ValidateWaves(out _), "Valid campaign waves and cards");
        Require(waves.BossPrefab.GetComponent<SapoDoPneuCombat>() != null, "Lake boss in campaign");
        Require(Find<LakeBackdrop>(scene) != null && All<JungleBackdrop>(scene).Length == 0, "Lake replaces only the copied scene background");
        Require(Find<ChapterFlow>(scene).Chapter.ChapterId == "chapter2_lake", "Own chapter completion");
        Require(Find<Player>(scene) != null && Find<BossHUD>(scene) != null, "Original player and HUD");
        foreach (string id in LayerNames)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(Art + id + ".png");
            Require(imp.filterMode == FilterMode.Point && !imp.mipmapEnabled && imp.textureCompression == TextureImporterCompression.Uncompressed, "Pixel layer: " + id);
        }
        Require(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == Stage), "Lake included in player build");
        Require(AssetDatabase.LoadAssetAtPath<GameObject>(JungleStageTools.BossPath) != null, "Macaquini preserved");
        foreach (Transform node in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)))
            Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "No missing script: " + node.name);
        Debug.Log("LAKE_STAGE_ASSETS_OK");
    }
    public static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    public static void BuildWindows()
    {
        string output = "Builds/Windows-LagoDoPneu-v01/CrocodiloInvaders.exe";
        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(), output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Lake player build failed");
        Debug.Log("LAKE_WINDOWS_BUILD_OK");
    }
}
