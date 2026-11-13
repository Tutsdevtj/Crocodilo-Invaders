using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Real Play Mode run of the Hangar: menu button, browsing, equip/save, locked teaser and the skin in gameplay.
[InitializeOnLoad]
public static class WardrobePlayValidation
{
    const string Key = "Crocodilo.Wardrobe.Validation";
    const string Output = "Docs/ArtPreviews/Wardrobe_v01";
    const string Gameplay = "Assets/Scenes/Tests/PlayerMovementTest.unity";
    static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    static double deadline; static int checks, errors; static bool finished;
    static string saveDirectory;

    static WardrobePlayValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin();
            if (state == PlayModeStateChange.EnteredEditMode)
            { SessionState.SetBool(Key, false); if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetInt(Key + ".Ok", 0) == 1 ? 0 : 1); }
        };
    }

    public static void Run()
    {
        WardrobeTools.Install();
        AchievementIconTools.Validate();
        SessionState.SetBool(Key, true); SessionState.SetInt(Key + ".Ok", 0);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying = true;
    }

    static void Begin()
    {
        checks = errors = 0; finished = false; deadline = EditorApplication.timeSinceStartup + 240;
        Directory.CreateDirectory(Output); Application.runInBackground = true;
        string id = Guid.NewGuid().ToString("N");
        saveDirectory = "Logs/Wardrobe/Play-" + id;
        Wardrobe.UseDirectory(saveDirectory);
        Achievements.UseDirectory("Logs/Wardrobe/Achievements-" + id);
        CampaignProgress.UseDirectory("Logs/Wardrobe/Campaign-" + id);
        LeaderboardSession.Cancel();
        Application.logMessageReceived += OnLog; Steps.Clear(); Steps.Push(Scenario()); EditorApplication.update += Tick;
    }

    static IEnumerator Scenario()
    {
        yield return Frames(6);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        var hangarButton = menu.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "HangarButton");
        yield return Until(() => hangarButton.IsInteractable(), "menu intro", 12);
        yield return Capture("main-menu-hangar.png", 1200, 720);
        Check(Wardrobe.Equipped(SkinSlot.Ship).id == "original", "fresh save starts with the original airplane");

        Click(hangarButton);
        yield return Frames(2);
        var view = Object.FindAnyObjectByType<WardrobeView>();
        Check(view != null && view.IsVisible, "Hangar opens from the menu button");
        Check(view.Selected.id == "original" && !view.EquipInteractable, "opens on the equipped skin, equip disabled");
        var seen = new HashSet<Sprite>();
        for (float end = Time.realtimeSinceStartup + 1.6f; Time.realtimeSinceStartup < end;) { seen.Add(view.PreviewSprite); yield return Frames(1); }
        Check(seen.Count > 1, "preview animates the idle frames (" + seen.Count + ")");
        yield return Capture("hangar-original.png", 1200, 720);

        view.Step(1);
        Check(view.Selected.id == "crocs" && view.EquipInteractable, "Crocs is available to equip");
        Check(AssetDatabase.GetAssetPath(view.PreviewSprite).StartsWith(CrocsSkinTools.Art + "/"), "Crocs preview uses Crocs frames");
        yield return Capture("hangar-crocs.png", 1200, 720);
        Check(view.EquipSelected(), "equip Crocs");
        Check(Wardrobe.Equipped(SkinSlot.Ship).id == "crocs" && !view.EquipInteractable, "Crocs equipped");
        Check(new WardrobeStore(saveDirectory).Get(SkinSlot.Ship) == "crocs", "choice persisted to wardrobe.json");
        yield return Capture("hangar-crocs-equipped.png", 1200, 720);

        view.Step(1);
        Check(view.Selected.id == "prototype" && !view.EquipInteractable && !view.EquipSelected(), "locked teaser cannot be equipped");
        Check(Wardrobe.Equipped(SkinSlot.Ship).id == "crocs", "failed equip keeps the previous skin");
        yield return Capture("hangar-locked.png", 1200, 720);
        yield return Capture("hangar-800x600.png", 800, 600);
        yield return Capture("hangar-1920x1080.png", 1920, 1080);
        view.Step(1);
        Check(view.Selected.id == "original", "browsing wraps around");

        var probe = new SkinDefinition { id = "probe", slot = SkinSlot.Ship, unlockAchievement = AchievementCatalog.FirstWave };
        Check(!Wardrobe.IsUnlocked(probe), "achievement-gated skin starts locked");
        Achievements.Award(AchievementCatalog.FirstWave);
        Check(Wardrobe.IsUnlocked(probe), "achievement unlock opens the gated skin");

        view.Close();
        yield return Frames(2);
        Check(!view.IsVisible && EventSystem.current.currentSelectedGameObject == hangarButton.gameObject, "closing returns focus to the Hangar button");

        EditorSceneManager.LoadSceneInPlayMode(Gameplay, new LoadSceneParameters(LoadSceneMode.Single));
        yield return Until(() => Object.FindAnyObjectByType<PlayerPresentation>() != null, "gameplay scene", 20);
        yield return Frames(10);
        var player = Object.FindAnyObjectByType<PlayerPresentation>();
        var crocs = SkinCatalog.Load().Get("crocs");
        Check(player.GetComponent<Animator>().runtimeAnimatorController == crocs.controller, "gameplay airplane uses the equipped Crocs");
        yield return Until(() => AssetDatabase.GetAssetPath(player.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite).StartsWith(CrocsSkinTools.Art + "/"), "Crocs frames on screen", 5);
        Check(true, "Crocs frames rendered in gameplay");
        yield return Capture("gameplay-crocs.png", 1200, 720);

        Check(Wardrobe.Equip(SkinCatalog.Load().Get("original")), "re-equip original");
        EditorSceneManager.LoadSceneInPlayMode(Gameplay, new LoadSceneParameters(LoadSceneMode.Single));
        yield return Frames(4);
        yield return Until(() => Object.FindAnyObjectByType<PlayerPresentation>() != null, "gameplay reload", 20);
        yield return Frames(4);
        Check(AssetDatabase.GetAssetPath(Object.FindAnyObjectByType<PlayerPresentation>().GetComponent<Animator>().runtimeAnimatorController)
              == "Assets/Animations/Player/Player.controller", "original airplane restored in gameplay");
        finished = true;
    }

    static void Click(UnityEngine.UI.Button button)
    {
        Check(button != null && button.IsInteractable(), "button interactable: " + button.name);
        ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    }

    static IEnumerator Capture(string name, int width, int height)
    {
        var camera = Camera.main; bool created = camera == null; if (created) camera = new GameObject("CaptureCamera").AddComponent<Camera>();
        var overlays = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
        var rt = new RenderTexture(width, height, 24); var oldTarget = camera.targetTexture; float oldAspect = camera.aspect;
        camera.targetTexture = rt; camera.aspect = width / (float)height;
        yield return Frames(4); Canvas.ForceUpdateCanvases(); camera.Render(); var old = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
        File.WriteAllBytes(Output + "/" + name, texture.EncodeToPNG());
        RenderTexture.active = old; camera.targetTexture = oldTarget; camera.aspect = oldAspect;
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
        rt.Release(); Object.Destroy(rt); Object.Destroy(texture); if (created) Object.Destroy(camera.gameObject);
    }

    static IEnumerator Frames(int n) { while (n-- > 0) { int frame = Time.frameCount; while (Time.frameCount == frame) yield return null; } }
    static IEnumerator Until(Func<bool> test, string label, float seconds)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (!test()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label); yield return Frames(1); }
    }
    static void Check(bool value, string label) { if (!value) throw new Exception("WARDROBE_PLAY_FAILED " + label); checks++; Debug.Log("WARDROBE_CHECK " + label); }
    static void OnLog(string m, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }

    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("wardrobe test deadline");
            while (Steps.Count > 0)
            {
                var step = Steps.Peek();
                if (!step.MoveNext()) { Steps.Pop(); continue; }
                if (step.Current is IEnumerator child) { Steps.Push(child); continue; }
                return;
            }
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog; Time.timeScale = 1; AudioListener.pause = false; LeaderboardSession.Cancel();
        File.WriteAllText(Output + "/validation.json", "{\"playChecks\":" + checks + ",\"errors\":" + errors + ",\"finished\":" + finished.ToString().ToLowerInvariant() + "}");
        SessionState.SetInt(Key + ".Ok", finished && errors == 0 ? 1 : 0);
        Debug.Log("WARDROBE_PLAY_RESULT checks=" + checks + " errors=" + errors + " finished=" + finished);
        EditorApplication.isPlaying = false;
    }
}
