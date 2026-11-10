using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LakePlaySuiteAttribute : Attribute
{
    public readonly int DeadlineSeconds;
    public LakePlaySuiteAttribute(int deadlineSeconds = 300) { DeadlineSeconds = deadlineSeconds; }
}

// Runs one Play Mode scenario per Editor launch. A suite only provides Scenario() and its entry points.
[InitializeOnLoad]
public static class LakePlayHarness
{
    private const string ActiveKey = "Crocodilo.Lake.ActiveSuite";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    private static float previousMaxDeltaTime;
    private static string outputFolder;

    static LakePlayHarness()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            string suite = SessionState.GetString(ActiveKey, "");
            if (suite.Length == 0) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin(Type.GetType(suite));
            if (state == PlayModeStateChange.EnteredEditMode) { SessionState.EraseString(ActiveKey); EditorApplication.Exit(errors == 0 ? 0 : 1); }
        };
    }
    public static void Launch(Type suite, string scene)
    {
        EditorSceneManager.OpenScene(scene);
        SessionState.SetString(ActiveKey, suite.FullName);
        EditorApplication.isPlaying = true;
    }
    private static void Begin(Type suite)
    {
        checks = errors = 0;
        deadline = EditorApplication.timeSinceStartup + suite.GetCustomAttribute<LakePlaySuiteAttribute>().DeadlineSeconds;
        outputFolder = "Logs/LagoDoPneu/Suites/" + suite.Name;
        Directory.CreateDirectory(outputFolder);
        previousMaxDeltaTime = Time.maximumDeltaTime;
        Time.maximumDeltaTime = .05f;
        Application.runInBackground = true;
        Application.logMessageReceived += OnLog;
        steps.Clear();
        steps.Push((IEnumerator)suite.GetMethod("Scenario", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null));
        EditorApplication.update += Tick;
    }
    public static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    public static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    public static IEnumerator Until(Func<bool> predicate, string label)
    {
        double end = EditorApplication.timeSinceStartup + 30;
        while (!predicate())
        {
            if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label);
            yield return null;
        }
    }
    public static void Check(bool value, string label)
    {
        if (!value) throw new InvalidOperationException("LAKE_CHECK_FAILED " + label);
        checks++;
        Debug.Log("LAKE_CHECK " + label);
    }
    public static void Capture(string filename = "gameplay.png")
    {
        var camera = Camera.main;
        var old = camera.targetTexture;
        var overlays = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingLayerName = "LakeHUD"; }
        var target = new RenderTexture(1280, 720, 24);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        Object.FindAnyObjectByType<LakeBackdrop>().SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        pixels.Apply();
        File.WriteAllBytes(outputFolder + "/" + filename, pixels.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = old;
        target.Release();
        Object.Destroy(target);
        Object.Destroy(pixels);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void OnLog(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Lake validation deadline");
            while (steps.Count > 0)
            {
                var top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator child) { steps.Push(child); continue; }
                return;
            }
            Debug.Log("LAKE_PLAY_OK checks=" + checks + " errors=" + errors);
        }
        catch (Exception e) { errors++; Debug.LogException(e); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        File.WriteAllText(outputFolder + "/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;
        Time.maximumDeltaTime = previousMaxDeltaTime;
        EditorApplication.isPlaying = false;
    }
}
