using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
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
    public const string Controller = "Assets/Animations/CrocHub/CrocHub.controller";
    public const string PhysicsMaterial = "Assets/Physics/CrocHubFrictionless.physicsMaterial2D";
    public const string Scene = "Assets/Scenes/BaseImprovisada.unity";

    [MenuItem("Crocodilo/Base/Install simplified HUB character and camera")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        foreach (string path in Directory.GetFiles(Art, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 80;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f, 1f / 72f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
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
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Idle);
        if (clip == null)
        {
            clip = new AnimationClip { name = "CrocHub_Idle", frameRate = 6f };
            var keys = new ObjectReferenceKeyframe[6];
            for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe
                { time = i / 6f, value = AssetDatabase.LoadAssetAtPath<Sprite>(Art + $"croc_idle_{i % 6:00}.png") };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.stopTime = 1f;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, Idle);
        }
        var animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
        if (animator == null)
        {
            animator = AnimatorController.CreateAnimatorControllerAtPath(Controller);
            animator.AddMotion(clip);
            animator.layers[0].stateMachine.defaultState.name = "Idle";
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
        {
            var root = new GameObject("CrocHub");
            var visual = new GameObject("VisualRoot");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(2f, 2f, 1f);
            var sprite = visual.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "croc_idle_00.png");
            sprite.sortingLayerName = "WorldCharacters";
            var anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = animator;
            anim.applyRootMotion = false;
            var target = new GameObject("CameraTarget").transform;
            target.SetParent(root.transform, false);
            target.localPosition = new Vector3(0f, .85f, 0f);
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = root.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(.88f, 1.7f);
            collider.offset = new Vector2(0f, .85f);
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
        if (existingCollider.sharedMaterial == null)
        {
            existingCollider.sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(contents, Prefab);
        }
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
        perfect.assetsPPU = 80;
        perfect.refResolutionX = 1280;
        perfect.refResolutionY = 720;
        perfect.upscaleRT = false;
        perfect.pixelSnapping = true;
        perfect.cropFrameX = perfect.cropFrameY = perfect.stretchFill = false;
        camera.allowMSAA = false;
        camera.orthographicSize = 4.5f;
        var follow = camera.GetComponent<HubCameraController2D>();
        if (follow == null) follow = camera.gameObject.AddComponent<HubCameraController2D>();
        SetObject(follow, "trackingTarget", player.transform.Find("CameraTarget"));
        SetObject(follow, "pixelPerfect", perfect);
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
        Require(player.GetComponentsInChildren<Transform>().Length == 3, "Flat hierarchy: physics root, VisualRoot, CameraTarget");
        var renderer = player.GetComponentInChildren<SpriteRenderer>();
        Require(renderer.sortingLayerName == "WorldCharacters" && renderer.transform.localScale == new Vector3(2f, 2f, 1f), "Map character sorting and 2x integer visual scale");
        Require((player.GetComponent<CapsuleCollider2D>().size - new Vector2(.88f, 1.7f)).sqrMagnitude < .0001f, "Collider fits enlarged Croc");
        for (int i = 0; i < 6; i++)
        {
            string path = Art + $"croc_idle_{i:00}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(sprite.rect.size == new Vector2(48, 72) && sprite.pixelsPerUnit == 80f && (sprite.pivot - new Vector2(24, 1)).sqrMagnitude < .001f
                && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "Idle native import " + i);
        }
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Idle);
        Require(clip.frameRate == 6f && Mathf.Approximately(clip.length, 1f) && AnimationUtility.GetAnimationClipSettings(clip).loopTime, "Approved one-second, six FPS loop");
        Require(AnimationUtility.GetCurveBindings(clip).Length == 0 && AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 1, "Only sprite keys; no transform or CameraTarget keys");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
        Require(controller.layers.Length == 1 && controller.layers[0].stateMachine.states.Length == 1 && controller.animationClips.All(c => c == clip), "Animator only Idle; no Run");
        var camera = Object.FindObjectsByType<Camera>().Single();
        var follow = camera.GetComponent<HubCameraController2D>();
        Require(follow != null && follow.TrackingTarget == player.transform.Find("CameraTarget") && follow.TrackingTarget.parent == player.transform, "Camera follows unanimated physics target");
        var perfect = camera.GetComponent<PixelPerfectCamera>();
        Require(perfect != null && perfect.assetsPPU == 80 && perfect.pixelSnapping && !perfect.upscaleRT && !perfect.cropFrameX && !perfect.cropFrameY, "Single Built-in pixel renderer");
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
