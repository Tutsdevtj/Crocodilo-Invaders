using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public static class HubCharacterTools
{
    public const string Art = "Assets/Art/Characters/CrocHub/";
    public const string Prefab = "Assets/Prefabs/Characters/CrocHub.prefab";
    public const string Idle = "Assets/Animations/CrocHub/CrocHub_Idle.anim";
    public const string Run = "Assets/Animations/CrocHub/CrocHub_Run.anim";
    public const string IdleSheet = Art + "CrocHub_Idle.png";
    public const string RunSheet = Art + "CrocHub_Run.png";
    public const string TailSheet = Art + "CrocHub_Tail.png";
    public const string Controller = "Assets/Animations/CrocHub/CrocHub.controller";
    public const string PhysicsMaterial = "Assets/Physics/CrocHubFrictionless.physicsMaterial2D";
    public const string Scene = "Assets/Scenes/BaseImprovisada.unity";

    [MenuItem("Crocodilo/Base/Install supplied Croc Idle and Run")]
    public static void InstallAnimations()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var controller = ConfigureAnimationAssets();
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try { ApplyAnimationPrefab(root, controller); PrefabUtility.SaveAsPrefabAsset(root, Prefab); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        // Os originais da animação anterior continuam no ZIP em SourceArt.
        var paths = AssetDatabase.FindAssets("t:Prefab").Concat(AssetDatabase.FindAssets("t:Scene")).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var dependencies = AssetDatabase.GetDependencies(paths, true);
        foreach (string old in Directory.GetFiles(Art, "croc_*.png").Select(p => p.Replace('\\', '/')))
            if (!dependencies.Contains(old)) AssetDatabase.DeleteAsset(old);
        var scene = EditorSceneManager.OpenScene(Scene);
        ConfigureBaseFraming(Object.FindAnyObjectByType<Camera>());
        EditorSceneManager.SaveScene(scene);
        Validate();
    }

    public static void UpdateRunLoop()
    {
        ImportSheet(RunSheet, "CrocHub_Run_", 3, 47, 73, 1, 8, 80f, 2f);
        WriteClip(Run, Frames(RunSheet), 12f);
        ConfigureTailClips();
        AssetDatabase.SaveAssets();
        Validate();
        CreatePreviewFrames();
        Debug.Log("HUB RUN LOOP UPDATED: frames 01-08, 12 FPS, 2/3 second loop.");
    }

    private static AnimatorController ConfigureAnimationAssets()
    {
        ImportSheet(IdleSheet, "CrocHub_Idle_", 3, 47, 73, 0, 5, 80f, 2f);
        ImportSheet(RunSheet, "CrocHub_Run_", 3, 47, 73, 1, 8, 80f, 2f);
        var idle = WriteClip(Idle, Frames(IdleSheet), 6f);
        var run = WriteClip(Run, Frames(RunSheet), 12f);
        ConfigureTailClips();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(Controller);
        if (!controller.parameters.Any(p => p.name == "Speed")) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var machine = controller.layers[0].stateMachine;
        var idleState = machine.states.FirstOrDefault(s => s.state.name == "Idle").state;
        if (idleState == null) idleState = machine.AddState("Idle", new Vector3(240, 80));
        var runState = machine.states.FirstOrDefault(s => s.state.name == "Run").state;
        if (runState == null) runState = machine.AddState("Run", new Vector3(480, 80));
        idleState.motion = idle; runState.motion = run; machine.defaultState = idleState;
        idleState.writeDefaultValues = runState.writeDefaultValues = false;
        runState.speedParameter = "Speed"; runState.speedParameterActive = true;
        Transition(idleState, runState, AnimatorConditionMode.Greater);
        Transition(runState, idleState, AnimatorConditionMode.Less);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
    {
        var transition = from.transitions.FirstOrDefault(t => t.destinationState == to);
        if (transition == null) transition = from.AddTransition(to);
        foreach (var condition in transition.conditions) transition.RemoveCondition(condition);
        transition.hasExitTime = false; transition.duration = 0f;
        transition.AddCondition(mode, .02f, "Speed");
    }

    private static void ApplyAnimationPrefab(GameObject root, AnimatorController controller)
    {
        var renderer = root.transform.Find("VisualRoot").GetComponent<SpriteRenderer>();
        renderer.sprite = Frames(IdleSheet)[0];
        renderer.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        var collider = root.GetComponent<CapsuleCollider2D>();
        collider.size = new Vector2(.792f, 1.53f); collider.offset = new Vector2(0f, .765f);
        root.transform.Find("CameraTarget").localPosition = new Vector3(0f, .765f, 0f);
        var animator = renderer.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
        var presentation = root.GetComponent<HubCharacterPresentation>();
        if (presentation == null) presentation = root.AddComponent<HubCharacterPresentation>();
        SetObject(presentation, "animator", animator);
        ConfigureTailPrefab(renderer);
    }

    public static void InstallRunTail()
    {
        // The supplied tail pixels are six times larger than the body pixels.
        ImportSheet(TailSheet, "CrocHub_Tail_", 3, 226, 228, 0, 9, 480f, 120f, 205f / 226f);
        ConfigureTailClips();
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            ConfigureTailPrefab(root.transform.Find("VisualRoot").GetComponent<SpriteRenderer>());
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
        CreatePreviewFrames();
        Debug.Log("HUB RUN TAIL INSTALLED AND VALIDATED.");
    }

    private static void ConfigureTailPrefab(SpriteRenderer body)
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(TailSheet) == null) return;
        var tail = body.transform.Find("Tail");
        if (tail == null)
        {
            tail = new GameObject("Tail", typeof(SpriteRenderer)).transform;
            tail.SetParent(body.transform, false);
        }
        tail.localPosition = new Vector3(.02f, .30f, 0f);
        tail.localScale = new Vector3(.85f, .85f, 1f);
        var renderer = tail.GetComponent<SpriteRenderer>();
        renderer.sharedMaterial = body.sharedMaterial;
        renderer.sortingLayerID = body.sortingLayerID;
        renderer.sortingOrder = body.sortingOrder - 1;
        renderer.sprite = null; // Idle hides it; Run supplies its sprite through the same Animator.
    }

    private static void ConfigureTailClips()
    {
        var frames = Frames(TailSheet);
        if (frames.Length == 0) return;
        var binding = EditorCurveBinding.PPtrCurve("Tail", typeof(SpriteRenderer), "m_Sprite");
        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(Idle);
        AnimationUtility.SetObjectReferenceCurve(idle, binding, new[] { new ObjectReferenceKeyframe { time = 0f, value = null } });
        var run = AssetDatabase.LoadAssetAtPath<AnimationClip>(Run);
        // Tail 08 returns to the initial pose. Use 00-07 for the eight-frame Run loop.
        AnimationUtility.SetObjectReferenceCurve(run, binding, Enumerable.Range(0, 8)
            .Select(i => new ObjectReferenceKeyframe { time = i / 12f, value = frames[i] }).ToArray());
        EditorUtility.SetDirty(idle); EditorUtility.SetDirty(run);
    }

    private static Sprite[] Frames(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();

    private static AnimationClip WriteClip(string path, Sprite[] frames, float fps)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.ClearCurves(); clip.frameRate = fps;
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i % frames.Length] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true; settings.stopTime = frames.Length / fps;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void ImportSheet(string path, string prefix, int columns, int width, int height, int first, int count, float ppu, float feet, float pivotX = .5f)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new InvalidOperationException("Missing supplied sprite sheet: " + path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ppu; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var existing = provider.GetSpriteRects();
        importer.GetSourceTextureWidthAndHeight(out int sheetWidth, out int sheetHeight);
        if (sheetWidth != columns * width || sheetHeight % height != 0) throw new InvalidOperationException("Unexpected sheet grid: " + path);
        var rects = Enumerable.Range(first, count).Select(index => {
            string name = prefix + index.ToString("00");
            return new SpriteRect { name = name, rect = new Rect(index % columns * width, sheetHeight - (index / columns + 1) * height, width, height),
                alignment = SpriteAlignment.Custom, pivot = new Vector2(pivotX, feet / height), spriteID = existing.FirstOrDefault(s => s.name == name)?.spriteID ?? GUID.Generate() };
        }).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply(); importer.SaveAndReimport();
    }

    [MenuItem("Crocodilo/Base/Install simplified HUB character and camera")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory("Assets/Animations/CrocHub");
        Directory.CreateDirectory("Assets/Prefabs/Characters");
        Directory.CreateDirectory("Assets/Physics");
        AssetDatabase.Refresh();
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterial);
        if (material == null)
        {
            material = new PhysicsMaterial2D("CrocHubFrictionless") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, PhysicsMaterial);
        }
        var animator = ConfigureAnimationAssets();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
        {
            var root = new GameObject("CrocHub");
            var visual = new GameObject("VisualRoot");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            var sprite = visual.AddComponent<SpriteRenderer>();
            sprite.sprite = Frames(IdleSheet)[0];
            sprite.sortingLayerName = "WorldCharacters";
            var anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = animator;
            anim.applyRootMotion = false;
            var target = new GameObject("CameraTarget").transform;
            target.SetParent(root.transform, false);
            target.localPosition = new Vector3(0f, .765f, 0f);
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = root.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(.792f, 1.53f);
            collider.offset = new Vector2(0f, .765f);
            collider.sharedMaterial = material;
            var input = root.AddComponent<HubPlayerInput>();
            SetObject(input, "actions", AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Inputs/Hub/HubInputActions.inputactions"));
            SetObject(root.AddComponent<HubCharacterMotor2D>(), "visualRoot", visual.transform);
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
            Object.DestroyImmediate(root);
        }
        // Preserve existing user tuning. Supply only the missing frictionless body material.
        var contents = PrefabUtility.LoadPrefabContents(Prefab);
        var existingCollider = contents.GetComponent<Collider2D>();
        if (existingCollider.sharedMaterial == null) existingCollider.sharedMaterial = material;
        ApplyAnimationPrefab(contents, animator);
        PrefabUtility.SaveAsPrefabAsset(contents, Prefab);
        PrefabUtility.UnloadPrefabContents(contents);
        var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        var characters = scene.GetRootGameObjects().Single(r => r.name == "Characters");
        foreach (Transform child in characters.transform.Cast<Transform>().Where(t => t.name.StartsWith("CrocScaleReference")).ToArray())
            Object.DestroyImmediate(child.gameObject);
        var player = Object.FindAnyObjectByType<HubCharacterMotor2D>();
        if (player == null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), characters.transform);
            instance.transform.position = GameObject.Find("PlayerSpawn").transform.position;
            player = instance.GetComponent<HubCharacterMotor2D>();
        }
        var camera = Object.FindAnyObjectByType<Camera>();
        var perfect = camera.GetComponent<PixelPerfectCamera>();
        if (perfect == null) perfect = camera.gameObject.AddComponent<PixelPerfectCamera>();
        perfect.assetsPPU = 72;
        perfect.refResolutionX = 1280;
        perfect.refResolutionY = 720;
        perfect.upscaleRT = false;
        perfect.pixelSnapping = true;
        perfect.cropFrameX = perfect.cropFrameY = perfect.stretchFill = false;
        camera.allowMSAA = false;
        camera.orthographicSize = 5f;
        var follow = camera.GetComponent<HubCameraController2D>();
        if (follow == null) follow = camera.gameObject.AddComponent<HubCameraController2D>();
        SetObject(follow, "trackingTarget", player.transform.Find("CameraTarget"));
        SetObject(follow, "pixelPerfect", perfect);
        ConfigureBaseFraming(camera);
        ConfigurePause(player.GetComponent<HubPlayerInput>());
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void SetObject(Object owner, string field, Object value)
    {
        var data = new SerializedObject(owner);
        data.FindProperty(field).objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void ConfigureBaseFraming(Camera camera)
    {
        camera.orthographicSize = 5f;
        camera.GetComponent<PixelPerfectCamera>().assetsPPU = 72;
        var fields = new SerializedObject(camera.GetComponent<HubCameraController2D>());
        fields.FindProperty("maximumOrthoSize").floatValue = 5f;
        fields.FindProperty("maximumViewWidth").floatValue = 23.333334f;
        fields.FindProperty("subjectHeight").floatValue = 1.65f;
        fields.FindProperty("cameraBounds").rectValue = new Rect(-13.5f, -2.6f, 120f, 10f);
        fields.ApplyModifiedPropertiesWithoutUndo();
        var sky = GameObject.Find("BG0_Panorama").GetComponentInChildren<SpriteRenderer>();
        Vector3 center = sky.bounds.center;
        sky.transform.localScale *= 10.25f / sky.bounds.size.y;
        sky.transform.position += new Vector3(center.x - sky.bounds.center.x, 2.4f - sky.bounds.center.y, 0f);
    }

    public static void CreatePreviewFrames()
    {
        var preview = new PreviewRenderUtility();
        var visual = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).transform.Find("VisualRoot").gameObject);
        var renderer = visual.GetComponent<SpriteRenderer>();
        Object.DestroyImmediate(visual.GetComponent<Animator>());
        preview.AddSingleGO(visual);
        var camera = preview.camera;
        camera.orthographic = true; camera.orthographicSize = 1.05f;
        camera.transform.position = new Vector3(0f, .78f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.10f, .13f, .13f, 1f);
        visual.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        try
        {
            foreach (string path in new[] { Idle, Run })
            {
                string name = path == Idle ? "Idle" : "Run";
                string folder = "Logs/CrocHubPreview-20261024/" + name;
                Directory.CreateDirectory(folder);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, AnimationUtility.GetObjectReferenceCurveBindings(clip).Single(b => b.path == ""));
                int count = keys.Length;
                for (int i = 0; i < count; i++)
                {
                    renderer.sprite = (Sprite)keys[i].value;
                    var tail = visual.transform.Find("Tail");
                    if (tail != null)
                    {
                        var tailKeys = AnimationUtility.GetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Tail", typeof(SpriteRenderer), "m_Sprite"));
                        tail.GetComponent<SpriteRenderer>().sprite = tailKeys == null ? null
                            : (Sprite)tailKeys.Last(k => k.time <= keys[i].time + .001f).value;
                    }
                    preview.BeginStaticPreview(new Rect(0, 0, 320, 320));
                    preview.Render();
                    var png = preview.EndStaticPreview();
                    File.WriteAllBytes(folder + $"/frame_{i:00}.png", png.EncodeToPNG());
                    Object.DestroyImmediate(png);
                }
            }
        }
        finally { preview.Cleanup(); }
    }

    public static void InstallPauseMenu()
    {
        var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        ConfigurePause(Object.FindAnyObjectByType<HubPlayerInput>());
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void ConfigurePause(HubPlayerInput input)
    {
        var pause = Object.FindAnyObjectByType<PauseController>();
        if (pause == null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/PauseMenu.prefab"));
            pause = instance.GetComponent<PauseController>();
        }
        SetObject(pause, "playerInput", input);
        SetObject(pause.GetComponent<PauseBackdrop>(), "gameCamera", Object.FindAnyObjectByType<Camera>());
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("EventSystem", typeof(EventSystem));
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }

    [MenuItem("Crocodilo/Base/Validate simplified HUB character and camera")]
    public static void Validate()
    {
        var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        Require(all.All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "No missing scripts");
        var player = Object.FindObjectsByType<HubCharacterMotor2D>().Single();
        Require(PrefabUtility.IsPartOfPrefabInstance(player), "Reusable HUB prefab instance");
        var pause = Object.FindObjectsByType<PauseController>().Single();
        Require(new SerializedObject(pause).FindProperty("playerInput").objectReferenceValue == player.GetComponent<HubPlayerInput>()
            && PrefabUtility.GetCorrespondingObjectFromSource(pause.gameObject) == AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/PauseMenu.prefab"), "Original pause prefab controls HUB input");
        Require(Object.FindObjectsByType<EventSystem>().Length == 1 && Object.FindAnyObjectByType<InputSystemUIInputModule>() != null, "Single Input System UI event system");
        var body = player.GetComponent<Rigidbody2D>();
        Require(body.bodyType == RigidbodyType2D.Dynamic && body.gravityScale > 0f && body.freezeRotation && body.interpolation == RigidbodyInterpolation2D.Interpolate, "Dynamic, gravity, frozen rotation, interpolation");
        Require(player.GetComponentsInChildren<Collider2D>().Length == 1 && player.GetComponent<CapsuleCollider2D>() != null, "Single body collider");
        Require(player.GetComponent<Collider2D>().sharedMaterial != null && player.GetComponent<Collider2D>().sharedMaterial.friction == 0f, "Frictionless horizontal contact");
        bool hasTail = Frames(TailSheet).Length > 0;
        Require(player.GetComponentsInChildren<Transform>().Length == (hasTail ? 4 : 3), "Physics root, VisualRoot, CameraTarget and optional visual tail");
        var renderer = player.GetComponentInChildren<SpriteRenderer>();
        Require(renderer.sortingLayerName == "WorldCharacters" && renderer.transform.localScale == new Vector3(1.8f, 1.8f, 1f), "Map character sorting and smaller visual scale");
        Require((player.GetComponent<CapsuleCollider2D>().size - new Vector2(.792f, 1.53f)).sqrMagnitude < .0001f, "Collider fits the smaller Croc");
        Require(player.GetComponent<HubCharacterPresentation>() != null, "Single presentation reads real horizontal velocity");
        Require(renderer.sprite == Frames(IdleSheet)[0], "Prefab starts with the first transparent Idle frame");
        foreach (var sheet in new[] { IdleSheet, RunSheet })
        {
            bool isIdle = sheet == IdleSheet;
            var frames = Frames(sheet);
            Require(frames.Length == (isIdle ? 5 : 8), "Only selected supplied frames: " + sheet);
            Require(isIdle || frames.Select(f => f.name).SequenceEqual(Enumerable.Range(1, 8).Select(i => "CrocHub_Run_" + i.ToString("00"))),
                "Run uses exactly frames 01-08; frame 00 is excluded");
            var importer = (TextureImporter)AssetImporter.GetAtPath(sheet);
            Require(importer.spriteImportMode == SpriteImportMode.Multiple && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
                && importer.textureCompression == TextureImporterCompression.Uncompressed, "Native sheet import: " + sheet);
            foreach (var sprite in frames)
                Require(sprite.rect.size == new Vector2(47, 73)
                    && sprite.pixelsPerUnit == 80f && (sprite.pivot - new Vector2(23.5f, 2f)).sqrMagnitude < .001f,
                    "Native frame and stable feet pivot: " + sprite.name);
            Require(!isIdle || frames.All(f => f.name != "CrocHub_Idle_05"), "Empty final Idle cell is excluded");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(isIdle ? Idle : Run);
            Require(clip.frameRate == (isIdle ? 6f : 12f) && Mathf.Approximately(clip.length, frames.Length / clip.frameRate)
                && AnimationUtility.GetAnimationClipSettings(clip).loopTime, "Native looping clip: " + clip.name);
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Require(AnimationUtility.GetCurveBindings(clip).Length == 0 && bindings.Length == (hasTail ? 2 : 1)
                && bindings.All(b => b.type == typeof(SpriteRenderer) && (b.path == "" || b.path == "Tail")), "Only sprite keys; no physics or camera curves");
            var keys = AnimationUtility.GetObjectReferenceCurve(clip, bindings.Single(b => b.path == ""));
            Require(keys.Length == frames.Length && keys.Select(k => k.value).SequenceEqual(frames.Cast<Object>()), "Exact sheet order without extra or excluded frames");
            if (hasTail)
            {
                var tailKeys = AnimationUtility.GetObjectReferenceCurve(clip, bindings.Single(b => b.path == "Tail"));
                Require(isIdle ? tailKeys.Length == 1 && tailKeys[0].value == null
                    : tailKeys.Length == 8 && tailKeys.Select(k => k.value).SequenceEqual(Frames(TailSheet).Take(8).Cast<Object>()),
                    "Tail is hidden in Idle and synchronized with all eight Run frames");
            }
        }
        if (hasTail)
        {
            var tail = renderer.transform.Find("Tail").GetComponent<SpriteRenderer>();
            Require(tail.sortingLayerID == renderer.sortingLayerID && tail.sortingOrder < renderer.sortingOrder
                && tail.transform.localScale == new Vector3(.85f, .85f, 1f) && tail.GetComponents<Component>().Length == 2,
                "Tail renders behind the body, inherits facing and adds no physics or runtime controller");
        }
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
        Require(controller.layers.Length == 1 && controller.layers[0].stateMachine.states.Length == 2
            && controller.layers[0].stateMachine.defaultState.name == "Idle" && controller.parameters.Length == 1 && controller.parameters[0].name == "Speed",
            "Animator has only Idle, Run and the real-speed parameter");
        var camera = Object.FindObjectsByType<Camera>().Single();
        var follow = camera.GetComponent<HubCameraController2D>();
        Require(follow != null && follow.TrackingTarget == player.transform.Find("CameraTarget") && follow.TrackingTarget.parent == player.transform, "Camera follows unanimated physics target");
        var perfect = camera.GetComponent<PixelPerfectCamera>();
        Require(perfect != null && perfect.assetsPPU == 72 && perfect.pixelSnapping && !perfect.upscaleRT && !perfect.cropFrameX && !perfect.cropFrameY, "Single Built-in pixel renderer");
        Require(camera.GetComponents<MonoBehaviour>().Length == 2 && Object.FindObjectsByType<ParallaxLayer2D>().Length == 4, "One camera position owner; original four parallax layers");
        Require(Object.FindObjectsByType<BaseSector>().Length == 7, "Existing sectors preserved");
        Debug.Log("HUB ASSET VALIDATION PASSED.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("HUB CHECK FAILED: " + message);
        Debug.Log("HUB CHECK: " + message);
    }
}
