using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class JungleStageTools
{
    public const string Stage = "Assets/Scenes/StageTwoJungle.unity";
    public const string Test = "Assets/Scenes/Tests/MacaquiniBossTest.unity";
    public const string BossPath = "Assets/Prefabs/Boss/Macaquini.prefab";
    private const string Art = "Assets/Art/JungleStage/";
    private const string Animation = "Assets/Animations/Boss/Macaquini/";
    [Serializable] private class Layer { public string id; public int x, y, sorting_order; public int[] pivot_top_left_px; }
    [Serializable] private class Layout { public Layer[] layers; }
    [Serializable] private class Frame { public string file; public int duration_ms; }
    [Serializable] private class Weapon { public Frame[] frames; }
    [Serializable] private class PoseFrame { public int[] body, hand, muzzle; }
    [Serializable] private class PoseSheet { public PoseFrame[] frames; }

    [MenuItem("Crocodilo/Fase 2/Install jungle stage")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Animation);
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
            Import(path.Replace('\\', '/'), path.Contains("Weapon") ? new Vector2(86f / 128f, 36f / 96f) : path.Contains("vine_cluster") ? new Vector2(.5f, 1f) : new Vector2(.5f, .5f));
        foreach (string path in Directory.GetFiles(Art + "Motion_v02", "pose_*.png"))
        {
            Import(path.Replace('\\', '/'), new Vector2(.5f, 102f / 192f));
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            importer.spritePixelsPerUnit = 50; importer.SaveAndReimport();
        }
        foreach (string path in Directory.GetFiles("Assets/Audio/Boss/Macaquini", "*.wav"))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            var settings = importer.defaultSampleSettings; settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad; settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        Projectile banana = BuildBanana();
        JungleVineHazard vine = BuildVine();
        var bat = BuildCreature(false, banana);
        var wasp = BuildCreature(true, banana);
        Boss macaquini = BuildBoss(banana, vine);
        var chapter = AssetDatabase.LoadAssetAtPath<ChapterDefinition>("Assets/Data/Flow/Chapter2.asset");
        if (chapter == null) { chapter = ScriptableObject.CreateInstance<ChapterDefinition>(); AssetDatabase.CreateAsset(chapter, "Assets/Data/Flow/Chapter2.asset"); }
        Set(chapter, "chapterId", "chapter2"); Set(chapter, "nextScene", "BaseImprovisada");
        var chapterData = new SerializedObject(chapter);
        var flags = chapterData.FindProperty("completionFlags"); flags.arraySize = 1; flags.GetArrayElementAtIndex(0).stringValue = "chapter2_completed";
        chapterData.ApplyModifiedPropertiesWithoutUndo();
        Set(chapter, "unavailableMessage", "FIM DO CAPÍTULO 2 — RETORNO À BASE");
        BuildScene(bat, wasp, macaquini, chapter);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Test, true);
        var test = EditorSceneManager.OpenScene(Test);
        var testWaves = Find<WaveController>(test);
        var starter = new GameObject("MacaquiniDevelopmentStart").AddComponent<BossDevelopmentStart>();
        Set(starter, "waves", testWaves);
        starter.gameObject.AddComponent<MacaquiniDebugControls>();
        EditorSceneManager.SaveScene(test);
        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(s => s.path == Stage)) buildScenes.Add(new EditorBuildSettingsScene(Stage, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.SaveAssets();
        Validate();
        Debug.Log("JUNGLE_INSTALL_OK");
    }
    private static void Import(string path, Vector2 pivot)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot;
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }
    private static Projectile BuildBanana()
    {
        var root = new GameObject("JungleBanana");
        Body(root);
        var collider = root.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(.45f, .16f);
        var projectile = root.AddComponent<Projectile>();
        Set(projectile, "speed", 6f); Set(projectile, "damage", 1); Set(projectile, "lifetime", 4f); Set(projectile, "targetLayers", 1 << LayerMask.NameToLayer("Player"));
        Sprite(root.transform, "Visual", Load("banana.png"), Vector3.zero, 40);
        var trail = root.AddComponent<TrailRenderer>(); trail.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat");
        trail.time = .14f; trail.startWidth = .09f; trail.endWidth = 0; trail.minVertexDistance = .04f;
        trail.startColor = new Color(.55f, 1f, .15f, .6f); trail.endColor = new Color(.25f, .7f, .1f, 0); trail.sortingOrder = 39;
        return Save(root, "Assets/Prefabs/Boss/JungleBanana.prefab").GetComponent<Projectile>();
    }
    private static JungleVineHazard BuildVine()
    {
        var root = new GameObject("JungleVine");
        var line = Line(root.transform, "Vine", new Color(.4f, 1f, .2f), .12f);
        var hazard = line.gameObject.AddComponent<JungleVineHazard>();
        Set(hazard, "vineSprite", Load("vine_cluster.png"));
        var prefab = Save(line.gameObject, "Assets/Prefabs/Boss/JungleVine.prefab");
        UnityEngine.Object.DestroyImmediate(root);
        return prefab.GetComponent<JungleVineHazard>();
    }
    private static Enemy BuildCreature(bool wasp, Projectile banana)
    {
        string name = wasp ? "JungleWasp" : "JungleBat";
        var root = new GameObject(name); root.layer = LayerMask.NameToLayer("Enemy");
        Body(root);
        var collider = root.AddComponent<BoxCollider2D>(); collider.isTrigger = true;
        collider.size = wasp ? new Vector2(.46f, .3f) : new Vector2(.5f, .27f);
        var health = root.AddComponent<Health>(); Set(health, "maxHealth", wasp ? 4 : 5);
        var movement = root.AddComponent<EnemyMovement>(); Set(movement, "moveSpeed", 3.8f); Set(movement, "patrolSpeed", wasp ? 2.3f : 1.7f);
        var enemy = root.AddComponent<Enemy>(); Set(enemy, "contactDamage", wasp ? 2 : 1);
        var visual = Sprite(root.transform, "Visual", Load(wasp ? "wasp.png" : "bat.png"), Vector3.zero, 8);
        var guide = Line(root.transform, "Warning", new Color(1f, .75f, .15f, .7f), .045f); guide.enabled = false;
        if (wasp)
        {
            var attack = root.AddComponent<InterceptorCombat>();
            Set(attack, "warningDuration", .85f); Set(attack, "dashSpeed", 8f);
            Set(attack, "attackInterval", new Vector2(3.2f, 4.5f));
        }
        else
        {
            var origin = new GameObject("ShotOrigin").transform; origin.SetParent(root.transform, false); origin.localPosition = new Vector3(-.2f, -.1f);
            var attack = root.AddComponent<ArtilleryCombat>();
            Set(attack, "projectilePrefab", banana); Set(attack, "shotOrigin", origin);
            Set(attack, "warningDuration", 1f); Set(attack, "projectileCount", 3); Set(attack, "fanAngle", 36f);
            Set(attack, "projectileSpeed", 3.5f); Set(attack, "fireInterval", new Vector2(3.8f, 5.2f));
        }
        var presentation = root.AddComponent<JungleCreaturePresentation>(); Set(presentation, "visual", visual); Set(presentation, "warning", guide);
        return Save(root, "Assets/Prefabs/Enemies/" + name + ".prefab").GetComponent<Enemy>();
    }
    [MenuItem("Crocodilo/Fase 2/Upgrade Macaquini anatomy and HUD")]
    public static void UpgradeMacaquini()
    {
        AssetDatabase.Refresh();
        ImportMacaquini();
        BuildBoss(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/JungleBanana.prefab").GetComponent<Projectile>(),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/JungleVine.prefab").GetComponent<JungleVineHazard>());
        AssetDatabase.SaveAssets(); Validate(); Debug.Log("MACAQUINI_V7_INSTALL_OK");
    }
    private static void ImportMacaquini()
    {
        string folder = Art + "Macaquini_v05/";
        foreach (string path in Directory.GetFiles(folder, "*.png"))
        {
            Vector2 pivot = new Vector2(.5f, 176f / 384);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == "gun_upper") pivot = new Vector2(100f / 320, (384f - 127) / 384);
            if (name == "gun_lower") pivot = new Vector2(77f / 320, (384f - 175) / 384);
            if (name == "support_upper") pivot = new Vector2(207f / 320, (384f - 116) / 384);
            if (name == "support_lower") pivot = new Vector2(238f / 320, (384f - 167) / 384);
            if (name.StartsWith("peel_")) pivot = new Vector2(.5f, 166f / 384);
            if (name.StartsWith("grip_")) pivot = new Vector2(.5f, 40f / 96);
            if (name.StartsWith("hud_")) pivot = new Vector2(.5f, .5f);
            Import(path.Replace('\\', '/'), pivot);
            if (name.StartsWith("transformacao_"))
            {
                Import(path.Replace('\\', '/'), new Vector2(.5f, 63f / 128));
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.spritePixelsPerUnit = 50; importer.SaveAndReimport();
            }
        }
    }
    private static Boss BuildBoss(Projectile banana, JungleVineHazard vine)
    {
        ImportMacaquini();
        var root = new GameObject("Macaquini"); root.layer = LayerMask.NameToLayer("Enemy"); root.transform.localScale = Vector3.one * 1.1f;
        Body(root);
        root.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        var collider = root.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(1.1f, 2.4f); collider.offset = new Vector2(0, .6f);
        var health = root.AddComponent<Health>(); Set(health, "maxHealth", 180);
        var movement = root.AddComponent<EnemyMovement>(); Set(movement, "moveSpeed", 3f); Set(movement, "patrolSpeed", 1.8f);
        Set(movement, "smoothDashes", true);
        var boss = root.AddComponent<Boss>(); Set(boss, "displayName", "MACAQUINI"); Set(boss, "defeatDuration", 2f);
        var bossData = new SerializedObject(boss); bossData.FindProperty("patrolArea").rectValue = new Rect(2.8f, .6f, 2.2f, 3.2f); bossData.ApplyModifiedPropertiesWithoutUndo();
        var visual = new GameObject("Visual").transform; visual.SetParent(root.transform, false); visual.localPosition = new Vector3(0, -1.1f);
        Sprite ArtSprite(string id) => Load("Macaquini_v05/" + id + ".png");
        var core = Sprite(visual, "ActionDrawings", ArtSprite("core"), new Vector3(0, 1.1f), 15);
        var transformation = Sprite(visual, "Transformation", null, new Vector3(0, 1.1f), 24); transformation.enabled = false;
        var peelLeft = Sprite(visual, "PeelLeft", ArtSprite("peel_left"), new Vector3(0, 1f), 20); peelLeft.enabled = false;
        var peelRight = Sprite(visual, "PeelRight", ArtSprite("peel_right"), new Vector3(0, 1f), 20); peelRight.enabled = false;
        var gunUpper = Sprite(visual, "braco_distante", ArtSprite("gun_upper"), Vector3.zero, 12);
        var gunLower = Sprite(visual, "GunForearm", ArtSprite("gun_lower"), Vector3.zero, 13);
        var nearUpper = Sprite(visual, "braco_proximo", ArtSprite("support_upper"), Vector3.zero, 14);
        var nearLower = Sprite(visual, "SupportForearm", ArtSprite("support_lower"), Vector3.zero, 19);
        var gunHand = Sprite(visual, "GunPalm", ArtSprite("grip_3"), Vector3.zero, 17);
        var nearHand = Sprite(visual, "SupportPalm", ArtSprite("grip_3"), Vector3.zero, 21);
        gunHand.transform.localScale = nearHand.transform.localScale = Vector3.one * MacaquiniPresentation.HandScale;
        gunUpper.sprite = RoundedLimb(ArtSprite("gun_upper"), "gun_upper", new Vector2(100,127), new Vector2(77,175), 16, 13);
        gunLower.sprite = RoundedLimb(ArtSprite("gun_lower"), "gun_lower", new Vector2(77,175), new Vector2(35,257), 13, 10);
        nearUpper.sprite = RoundedLimb(ArtSprite("support_upper"), "support_upper", new Vector2(207,116), new Vector2(238,167), 18, 13);
        nearLower.sprite = RoundedLimb(ArtSprite("support_lower"), "support_lower", new Vector2(238,167), new Vector2(267,260), 13, 10);
        Sprite(gunLower.transform, "ElbowOverlap", JointPatch("gun_elbow", new Vector2(77,175), 13), Vector3.zero, 14);
        Sprite(nearLower.transform, "ElbowOverlap", JointPatch("support_elbow", new Vector2(238,167), 13), Vector3.zero, 20);
        var gunShoulder = Sprite(visual, "GunShoulderOverlap", JointPatch("gun_shoulder", new Vector2(100,127), 16), Vector3.zero, 16);
        var nearShoulder = Sprite(visual, "SupportShoulderOverlap", JointPatch("support_shoulder", new Vector2(207,116), 18), Vector3.zero, 16);
        foreach (var view in new[] { gunUpper, gunLower, nearUpper, nearLower, gunShoulder, nearShoulder, gunLower.transform.Find("ElbowOverlap").GetComponent<SpriteRenderer>(), nearLower.transform.Find("ElbowOverlap").GetComponent<SpriteRenderer>() })
        {
            var shape = jointMeshes[view.sprite.name];
            view.gameObject.AddComponent<MacaquiniSpriteMesh>().Configure(shape.vertices, shape.triangles);
            if (!view.sprite.name.EndsWith("_rounded")) view.enabled = false;
        }
        var weapon = Sprite(gunUpper.transform, "Weapon", Load("Weapon/weapon_00.png"), Vector3.zero, 16); weapon.transform.localScale = Vector3.one * (1.1f / 1.3f);
        var muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(weapon.transform, false); muzzle.localPosition = new Vector3(-.54f, .21f);
        var hand = new GameObject("SupportHand").transform; hand.SetParent(nearUpper.transform, false);
        var support = Line(visual, "SupportVine", new Color(.29f, .39f, .12f), .055f); support.sortingOrder = 20;
        var charge = Sprite(muzzle, "Charge", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Boss/Charge/charge_00.png"), Vector3.zero, 25); charge.enabled = false;
        var guide = Line(root.transform, "AimGuide", new Color(1f, .8f, .2f, .65f), .045f); guide.enabled = false;
        var animator = root.AddComponent<Animator>(); animator.runtimeAnimatorController = WeaponController();
        var combat = root.AddComponent<MacaquiniCombat>(); Set(combat, "bananaPrefab", banana); Set(combat, "vinePrefab", vine); Set(combat, "muzzle", muzzle); Set(combat, "animator", animator); Set(combat, "aimGuide", guide);
        var presentation = root.AddComponent<MacaquiniPresentation>(); var voice = root.AddComponent<AudioSource>(); voice.playOnAwake = false; voice.volume = .65f;
        Set(presentation, "visualRig", visual); Set(presentation, "actionSprite", core);
        Set(presentation, "transformationView", transformation); Set(presentation, "closedCore", ArtSprite("core")); Set(presentation, "halfCore", ArtSprite("halfcore"));
        Set(presentation, "peelLeft", peelLeft); Set(presentation, "peelRight", peelRight);
        Set(presentation, "farArm", gunUpper.transform); Set(presentation, "gunForearm", gunLower.transform); Set(presentation, "gunPalm", gunHand.transform);
        Set(presentation, "nearArm", nearUpper.transform); Set(presentation, "supportForearm", nearLower.transform); Set(presentation, "supportPalm", nearHand.transform);
        Set(presentation, "gunShoulderCover", gunShoulder.transform); Set(presentation, "supportShoulderCover", nearShoulder.transform);
        Set(presentation, "weapon", weapon.transform); Set(presentation, "muzzle", muzzle); Set(presentation, "supportHand", hand); Set(presentation, "supportVine", support); Set(presentation, "charge", charge);
        Set(presentation, "hudPortraitHalf", ArtSprite("hud_portrait_half")); Set(presentation, "hudTitle", ArtSprite("hud_title")); Set(presentation, "hudPortrait", ArtSprite("hud_portrait"));
        Set(presentation, "effectMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));
        foreach (string sound in new[] { "launch", "charge", "whip", "growl", "hurt" }) Set(presentation, sound + "Sound", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Boss/Macaquini/" + sound + ".wav"));
        var data = new SerializedObject(presentation);
        var renderers = root.GetComponentsInChildren<SpriteRenderer>().Where(s => s != charge && s != transformation && s != peelLeft && s != peelRight).ToArray(); var parts = data.FindProperty("parts"); parts.arraySize = renderers.Length;
        for (int i = 0; i < renderers.Length; i++) { parts.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i]; renderers[i].sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteHitFlash.mat"); }
        var axes = data.FindProperty("boneAxes"); axes.arraySize = 4;
        Vector2[] vectors = { new Vector2(-.23f, -.48f), new Vector2(-.42f, -.82f), new Vector2(.31f, -.51f), new Vector2(.29f, -.93f) };
        for (int i = 0; i < vectors.Length; i++) axes.GetArrayElementAtIndex(i).vector2Value = vectors[i];
        var shoulders = data.FindProperty("shoulders"); shoulders.arraySize = 2; shoulders.GetArrayElementAtIndex(0).vector2Value = new Vector2(-.5f, .8f); shoulders.GetArrayElementAtIndex(1).vector2Value = new Vector2(.45f, .87f);
        foreach (var limb in new[] { gunUpper, gunLower, nearUpper, nearLower }) limb.transform.localScale = Vector3.one * 1.3f;
        var halfShoulders = data.FindProperty("halfShoulders"); halfShoulders.arraySize = 2; halfShoulders.GetArrayElementAtIndex(0).vector2Value = new Vector2(-.6f, .81f); halfShoulders.GetArrayElementAtIndex(1).vector2Value = new Vector2(.47f, .92f);
        var grips = data.FindProperty("gripFrames"); grips.arraySize = 4; for (int i = 0; i < 4; i++) grips.GetArrayElementAtIndex(i).objectReferenceValue = ArtSprite("grip_" + i);
        var frames = Directory.GetFiles("Assets/Art/Boss/Charge", "*.png").OrderBy(p => p).Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
        var charges = data.FindProperty("chargeFrames"); charges.arraySize = frames.Length; for (int i = 0; i < frames.Length; i++) charges.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        data.ApplyModifiedPropertiesWithoutUndo(); presentation.PreviewPose(); root.AddComponent<MacaquiniPhaseTwo>(); return Save(root, BossPath).GetComponent<Boss>();
    }
    // Reuse the approved pixels. Rounded mesh ends and overlapping fur hide straight slicing seams.
    private static Sprite RoundedLimb(Sprite source, string id, Vector2 start, Vector2 end, float startRadius, float endRadius)
    {
        var sprite = UnityEngine.Sprite.Create(source.texture, source.rect, source.pivot / source.rect.size, 100, 0, SpriteMeshType.FullRect);
        Vector2 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x);
        var points = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i <= 12; i++) { float a = angle + Mathf.PI * .5f + Mathf.PI * i / 12; points.Add(start + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * startRadius); }
        for (int i = 0; i <= 12; i++) { float a = angle - Mathf.PI * .5f + Mathf.PI * i / 12; points.Add(end + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * endRadius); }
        jointMeshes[id + "_rounded"] = SetMesh(sprite, points.Select(p => new Vector2(p.x - source.pivot.x, source.rect.height - p.y - source.pivot.y) / 100).ToArray());
        return SaveJointSprite(sprite, id + "_rounded");
    }
    private static Sprite JointPatch(string id, Vector2 center, float radius)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Macaquini_v05/joint_source.png");
        var sprite = UnityEngine.Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(center.x / texture.width, 1 - center.y / texture.height), 100, 0, SpriteMeshType.FullRect);
        var points = Enumerable.Range(0,24).Select(i => new Vector2(Mathf.Cos(i * Mathf.PI / 12), Mathf.Sin(i * Mathf.PI / 12)) * (radius / 100)).ToArray();
        jointMeshes[id] = SetMesh(sprite, points); return SaveJointSprite(sprite, id);
    }
    private static readonly System.Collections.Generic.Dictionary<string, (Vector2[] vertices, ushort[] triangles)> jointMeshes = new();
    private static (Vector2[] vertices, ushort[] triangles) SetMesh(Sprite sprite, Vector2[] points)
    {
        var triangles = new ushort[(points.Length - 2) * 3];
        for (int i = 0; i < points.Length - 2; i++) { triangles[i*3] = 0; triangles[i*3+1] = (ushort)(i+1); triangles[i*3+2] = (ushort)(i+2); }
        Vector2[] pixels = points.Select(p => p * sprite.pixelsPerUnit + sprite.pivot).ToArray();
        return (pixels, triangles);
    }
    private static Sprite SaveJointSprite(Sprite sprite, string id)
    {
        string path = Art + "Macaquini_v05/" + id + ".asset"; sprite.name = id;
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) { EditorUtility.CopySerialized(sprite, existing); UnityEngine.Object.DestroyImmediate(sprite); EditorUtility.SetDirty(existing); return existing; }
        AssetDatabase.CreateAsset(sprite, path); return sprite;
    }
    private static AnimatorController WeaponController()
    {
        var weapon = JsonUtility.FromJson<Weapon>(File.ReadAllText("SourceArt/JungleStage_v01/weapon_animation.json"));
        var idle = Clip("Idle"); var fire = Clip("Fire");
        var binding = EditorCurveBinding.PPtrCurve("Visual/braco_distante/Weapon", typeof(SpriteRenderer), "m_Sprite");
        // Remove the old root-level binding when upgrading the prototype.
        foreach (var old in AnimationUtility.GetObjectReferenceCurveBindings(idle)) AnimationUtility.SetObjectReferenceCurve(idle, old, null);
        foreach (var old in AnimationUtility.GetObjectReferenceCurveBindings(fire)) AnimationUtility.SetObjectReferenceCurve(fire, old, null);
        AnimationUtility.SetObjectReferenceCurve(idle, binding, new[] { new ObjectReferenceKeyframe { time = 0, value = Load("Weapon/weapon_00.png") }, new ObjectReferenceKeyframe { time = 1, value = Load("Weapon/weapon_00.png") } });
        var keys = new System.Collections.Generic.List<ObjectReferenceKeyframe>(); float time = 0;
        foreach (Frame frame in weapon.frames) { keys.Add(new ObjectReferenceKeyframe { time = time, value = Load("Weapon/" + Path.GetFileName(frame.file)) }); time += frame.duration_ms / 1000f; }
        keys.Add(new ObjectReferenceKeyframe { time = time, value = Load("Weapon/weapon_00.png") });
        AnimationUtility.SetObjectReferenceCurve(fire, binding, keys.ToArray());
        AnimationUtility.SetAnimationEvents(fire, new[] { new AnimationEvent { functionName = "MacaquiniRelease", time = .32f }, new AnimationEvent { functionName = "MacaquiniClipComplete", time = time - .001f } });
        var idleSettings = AnimationUtility.GetAnimationClipSettings(idle); idleSettings.loopTime = true; AnimationUtility.SetAnimationClipSettings(idle, idleSettings);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Animation + "Macaquini.controller");
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(Animation + "Macaquini.controller");
        var machine = controller.layers[0].stateMachine;
        foreach (var state in machine.states) machine.RemoveState(state.state);
        var idleState = machine.AddState("Idle"); idleState.motion = idle; machine.defaultState = idleState;
        var fireState = machine.AddState("Fire"); fireState.motion = fire;
        var tear = Clip("Tear");
        const float tearTime = 4.22f;
        foreach (var old in AnimationUtility.GetObjectReferenceCurveBindings(tear)) AnimationUtility.SetObjectReferenceCurve(tear, old, null);
        AnimationUtility.SetEditorCurve(tear, EditorCurveBinding.FloatCurve("", typeof(MacaquiniPresentation), "tearProgress"), AnimationCurve.Linear(0, 0, tearTime, 1));
        AnimationUtility.SetAnimationEvents(tear, new[] { new AnimationEvent { functionName = "MacaquiniTearComplete", time = tearTime - .001f } });
        var tearState = machine.AddState("Tear"); tearState.motion = tear;
        var transition = fireState.AddTransition(idleState); transition.hasExitTime = true; transition.exitTime = 1f; transition.duration = 0;
        EditorUtility.SetDirty(controller);
        return controller;
    }
    private static AnimationClip Clip(string name)
    {
        string path = Animation + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip { name = name, frameRate = 1000 }; AssetDatabase.CreateAsset(clip, path); }
        return clip;
    }
    private static void BuildScene(Enemy bat, Enemy wasp, Boss boss, ChapterDefinition chapter)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");
        // Fork the scene before touching objects; the chapter-one asset stays intact.
        EditorSceneManager.SaveScene(scene, Stage);
        // Opening a different scene can unload unreferenced prefab assets. Resolve after the switch.
        bat = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/JungleBat.prefab").GetComponent<Enemy>();
        wasp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/JungleWasp.prefab").GetComponent<Enemy>();
        boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPath).GetComponent<Boss>();
        chapter = AssetDatabase.LoadAssetAtPath<ChapterDefinition>("Assets/Data/Flow/Chapter2.asset");
        foreach (var root in scene.GetRootGameObjects())
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            if (root.name.Contains("BattleSky") || (!string.IsNullOrEmpty(path) && path.Contains("BattleSky"))) UnityEngine.Object.DestroyImmediate(root);
        }
        foreach (var cutscene in All<RunCutscenes>(scene)) UnityEngine.Object.DestroyImmediate(cutscene);
        var waves = Find<WaveController>(scene);
        Set(waves, "cutscenes", null); Set(waves, "bossPrefab", boss); Set(waves, "completeChapterOnBossDefeat", true);
        var data = new SerializedObject(waves); data.FindProperty("wavePools").arraySize = 0;
        var composition = data.FindProperty("waveComposition"); composition.arraySize = 9;
        for (int i = 0; i < 9; i++)
        {
            var wave = composition.GetArrayElementAtIndex(i);
            int wasps = i < 2 ? 0 : 2 + (i - 2) * 2;
            var groups = wave.FindPropertyRelative("groups"); groups.arraySize = wasps > 0 ? 2 : 1;
            groups.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue = bat;
            groups.GetArrayElementAtIndex(0).FindPropertyRelative("count").intValue = 4 + i * 2;
            if (wasps > 0) { groups.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue = wasp; groups.GetArrayElementAtIndex(1).FindPropertyRelative("count").intValue = wasps; }
            wave.FindPropertyRelative("maxAlive").intValue = i < 2 ? 3 : i < 5 ? 4 : 6;
            wave.FindPropertyRelative("spawnInterval").vector2Value = new Vector2(Mathf.Lerp(2.8f, 1.7f, i / 8f), Mathf.Lerp(3.5f, 2.3f, i / 8f));
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        var spawner = Find<EnemySpawner>(scene); Set(spawner, "enemyPrefab", bat);
        var flow = Find<ChapterFlow>(scene); Set(flow, "chapter", chapter);
        var camera = Find<Camera>(scene);
        var backdrop = new GameObject("JungleBackdrop");
        var sprite = backdrop.AddComponent<SpriteRenderer>(); sprite.sprite = Load("Motion_v02/deep.png"); sprite.sortingLayerName = "Default"; sprite.sortingOrder = -110;
        var fit = backdrop.AddComponent<JungleBackdrop>(); Set(fit, "gameplayCamera", camera);
        Set(fit, "distantTrees", Load("Motion_v02/far.png")); Set(fit, "middleTrees", Load("Motion_v02/trunks.png"));
        Set(fit, "canopySprite", Load("Motion_v02/canopy.png")); Set(fit, "fernSprite", Load("Motion_v02/ferns.png"));
        Set(fit, "vineSprite", Load("vine_cluster.png")); Set(fit, "effectMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));
        // Preview the same cover sizing in Edit Mode as runtime.
        float scale = Mathf.Max(camera.orthographicSize * 2 / sprite.sprite.bounds.size.y, camera.orthographicSize * 2 * camera.aspect / sprite.sprite.bounds.size.x);
        backdrop.transform.localScale = new Vector3(scale, scale, 1); backdrop.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 5);
        EditorSceneManager.SaveScene(scene);
    }
    private static void Body(GameObject root)
    {
        var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    private static SpriteRenderer Sprite(Transform parent, string name, Sprite sprite, Vector3 position, int order)
    {
        var node = new GameObject(name); node.transform.SetParent(parent, false); node.transform.localPosition = position;
        var renderer = node.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order; return renderer;
    }
    private static LineRenderer Line(Transform parent, string name, Color color, float width)
    {
        var node = new GameObject(name); node.transform.SetParent(parent, false);
        var line = node.AddComponent<LineRenderer>(); line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat");
        line.useWorldSpace = true; line.positionCount = 2; line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = color; line.sortingOrder = 30; return line;
    }
    private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name);
    private static GameObject Save(GameObject root, string path)
    {
        var saved = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root); return saved;
    }
    public static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
    public static T Find<T>(Scene scene) where T : Component => All<T>(scene).FirstOrDefault();
    public static void Set(UnityEngine.Object target, string name, object value)
    {
        var data = new SerializedObject(target); var property = data.FindProperty(name);
        if (property == null) throw new InvalidOperationException(target.name + "." + name);
        if (value == null || value is UnityEngine.Object) property.objectReferenceValue = value as UnityEngine.Object;
        else if (value is int integer) property.intValue = integer;
        else if (value is float number) property.floatValue = number;
        else if (value is bool boolean) property.boolValue = boolean;
        else if (value is string text) property.stringValue = text;
        else if (value is Vector2 vector) property.vector2Value = vector;
        else throw new ArgumentException(name);
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    [MenuItem("Crocodilo/Fase 2/Validate jungle assets")]
    public static void Validate()
    {
        foreach (string path in new[] { BossPath, "Assets/Prefabs/Boss/JungleBanana.prefab", "Assets/Prefabs/Boss/JungleVine.prefab", "Assets/Prefabs/Enemies/JungleBat.prefab", "Assets/Prefabs/Enemies/JungleWasp.prefab" })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(root != null, path);
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Missing script " + child.name);
        }
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "Native import " + path);
        }
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPath);
        Require(boss.GetComponent<Boss>().DisplayName == "MACAQUINI" && boss.GetComponent<MacaquiniCombat>() != null && boss.GetComponent<GusiniCombat>() == null, "Own boss controller");
        var fire = AssetDatabase.LoadAssetAtPath<AnimationClip>(Animation + "Fire.anim");
        Require(AnimationUtility.GetAnimationEvents(fire).Any(e => e.functionName == "MacaquiniRelease" && Mathf.Abs(e.time - .32f) < .001f), "Native release event");
        Require(AnimationUtility.GetCurveBindings(fire).Length == 0, "No baked transform motion");
        var scene = EditorSceneManager.OpenScene(Stage);
        var waves = Find<WaveController>(scene); Require(waves.ValidateWaves(out string error), error);
        Require(waves.CombatWaveCount == 9 && waves.WaveCount == 10 && waves.BossPrefab.DisplayName == "MACAQUINI", "Nine jungle hordes and boss");
        Require(Find<RunCutscenes>(scene) == null, "No Gusini dialogue in chapter two");
        Require(Find<ChapterFlow>(scene).Chapter.ChapterId == "chapter2", "Own chapter definition");
        var background = Find<JungleBackdrop>(scene).GetComponent<SpriteRenderer>();
        Require(background.sortingLayerID == 0 && background.sortingOrder < -100, "Backdrop stays behind combat and HUD");
        Require(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == Stage), "Stage enabled in build");
        var original = EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");
        Require(Find<WaveController>(original).BossPrefab.GetComponent<GusiniCombat>() != null && Find<RunCutscenes>(original) != null, "Chapter one preserved");
        EditorSceneManager.OpenScene(Stage);
        Debug.Log("JUNGLE_ASSETS_OK");
    }
    public static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    [MenuItem("Crocodilo/Fase 2/Open jungle stage")]
    public static void OpenStage() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(Stage); }
    [MenuItem("Crocodilo/Fase 2/Open Macaquini boss test")]
    public static void OpenTest() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(Test); }
}
