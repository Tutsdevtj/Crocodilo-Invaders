using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class DeveloperMenuValidation
{
    private const string Key = "DeveloperMenu.Validation";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    static DeveloperMenuValidation()
    {
        EditorApplication.playModeStateChanged += state => {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 300;
                Application.runInBackground = true; steps.Clear(); steps.Push(Scenario());
                Application.logMessageReceived += Log; EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            { SessionState.SetBool(Key, false); EditorApplication.Exit(errors == 0 ? 0 : 1); }
        };
    }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    private static void Click(string name)
    {
        var button = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Single(b => b.name == name);
        Check(button.interactable, name + " interactable"); button.onClick.Invoke();
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(5);
        var menu = Object.FindAnyObjectByType<DeveloperMenuView>();
        yield return Until(() => { menu.Open(); return menu.IsVisible; }); Check(menu.IsVisible, "DEV opens");
        Canvas.ForceUpdateCanvases();
        var panel = GameObject.Find("DeveloperPanel").GetComponent<RectTransform>();
        foreach (string name in new[] { "Cards", "FinalBoss", "Base", "Close" })
        {
            var rect = GameObject.Find(name).GetComponent<RectTransform>();
            Check(rect.rect.width > 0 && rect.rect.height >= 40 && panel.rect.Contains(panel.InverseTransformPoint(rect.TransformPoint(rect.rect.center))), "layout " + name);
        }
        Directory.CreateDirectory("Logs/DeveloperMenu");
        ScreenCapture.CaptureScreenshot("Logs/DeveloperMenu/categories.png"); yield return Frames(10);
        Click("Cards"); Click("OfferMode"); Click("Rarity"); Click("Wave");
        Check(DevelopmentCardSession.CompletedWave == 5, "card settings preserved");
        menu.Back(); Click("FinalBoss"); Click("Boss2"); menu.Back();
        Check(GameObject.Find("Boss1") != null, "back returns to boss list");
        menu.Back(); menu.Back(); Check(!menu.IsVisible, "back closes root");
        for (int bossIndex = 1; bossIndex <= 2; bossIndex++)
        for (int phase = 1; phase <= 2; phase++)
        {
            menu = Object.FindAnyObjectByType<DeveloperMenuView>();
            yield return Until(() => { menu.Open(); return menu.IsVisible; });
            Click("FinalBoss"); Click("Boss" + bossIndex); Click("Phase" + phase);
            yield return Until(() => Object.FindAnyObjectByType<GameplayLoadingController>()?.State == GameplayLoadingController.LoadingState.Ready);
            var loading = Object.FindAnyObjectByType<GameplayLoadingController>();
            loading.StartCoroutine((IEnumerator)typeof(GameplayLoadingController).GetMethod("StartGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loading, null));
            yield return Until(() => Object.FindAnyObjectByType<WaveController>()?.ActiveBoss != null);
            var waves = Object.FindAnyObjectByType<WaveController>();
            var player = Object.FindAnyObjectByType<Player>(); player.Health.SetInvulnerable(typeof(DeveloperMenuValidation), true);
            var boss = waves.ActiveBoss;
            Check((boss.GetComponent<MacaquiniPhaseTwo>() != null) == (bossIndex == 2), "correct boss " + bossIndex);
            yield return Until(() => boss.Ready && (phase == 1 || (bossIndex == 1
                ? boss.GetComponent<GusiniPhaseTwo>().CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo
                : boss.GetComponent<MacaquiniPhaseTwo>().CurrentStage == MacaquiniPhaseTwo.Stage.HalfShell)));
            Check(phase == 2 || (bossIndex == 1 ? boss.GetComponent<GusiniPhaseTwo>().CurrentStage == GusiniPhaseTwo.Stage.PhaseOne
                : boss.GetComponent<MacaquiniPhaseTwo>().CurrentStage == MacaquiniPhaseTwo.Stage.ClosedShell), "correct phase " + phase);
            Check(DevelopmentBossSession.IsActive && !DevelopmentCardSession.IsActive, "isolated boss session");
            Time.timeScale = 1; SceneManager.LoadScene("Assets/Scenes/MainMenu.unity"); yield return Frames(6);
            Check(!DevelopmentBossSession.IsActive, "session cleared at menu");
        }
        menu = Object.FindAnyObjectByType<DeveloperMenuView>();
        yield return Until(() => { menu.Open(); return menu.IsVisible; }); Click("Base");
        yield return Until(() => SceneManager.GetActiveScene().name == "BaseImprovisada");
        Check(!DevelopmentBossSession.IsActive, "base loads without boss session");
    }
    private static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    private static IEnumerator Until(Func<bool> predicate)
    {
        double end = EditorApplication.timeSinceStartup + 50;
        while (!predicate()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException("DEV navigation"); yield return null; }
    }
    private static void Check(bool value, string label) { if (!value) throw new Exception(label); checks++; Debug.Log("DEV_CHECK " + label); }
    private static void Log(string message, string trace, LogType type)
    { if (!trace.Contains("UnityEditor.Search.SearchDatabase") && (type == LogType.Error || type == LogType.Exception)) errors++; }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("DEV total deadline");
            while (steps.Count > 0) {
                var step = steps.Peek(); if (!step.MoveNext()) { steps.Pop(); continue; }
                if (step.Current is IEnumerator child) { steps.Push(child); continue; } return;
            }
        } catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        Directory.CreateDirectory("Logs/DeveloperMenu");
        File.WriteAllText("Logs/DeveloperMenu/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Debug.Log("DEV_MENU_VALIDATION checks=" + checks + " errors=" + errors);
        Time.timeScale = 1; AudioListener.pause = false; EditorApplication.isPlaying = false;
    }
    public static void Build()
    {
        const string output = "Builds/Windows-Dev-Menu-v23/CrocodiloInvaders.exe";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.CompressWithLz4HC });
        Debug.Log("DEV_MENU_BUILD " + result.summary.result);
        EditorApplication.Exit(result.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
