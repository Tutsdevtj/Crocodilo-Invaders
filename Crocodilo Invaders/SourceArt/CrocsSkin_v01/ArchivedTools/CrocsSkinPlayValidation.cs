using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class CrocsSkinPlayValidation
{
    const string Key = "Crocodilo.Crocs.Validation";
    const string Output = "Docs/ArtPreviews/CrocsSkin_v01";
    static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    static double deadline;
    static int checks, errors;
    static bool finished;

    static CrocsSkinPlayValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin();
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                if (Application.isBatchMode) EditorApplication.Exit(finished && errors == 0 ? 0 : 1);
            }
        };
    }

    public static void Run()
    {
        CrocsSkinTools.Install();
        SessionState.SetBool(Key, true);
        EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");
        EditorApplication.isPlaying = true;
    }

    static void Begin()
    {
        checks = errors = 0; finished = false;
        deadline = EditorApplication.timeSinceStartup + 90;
        Application.runInBackground = true;
        Directory.CreateDirectory(Output);
        Application.logMessageReceived += OnLog;
        Steps.Clear(); Steps.Push(Scenario());
        EditorApplication.update += Tick;
    }

    static IEnumerator Scenario()
    {
        yield return Frames(8);
        LeaderboardSession.Cancel();
        var player = Object.FindAnyObjectByType<Player>();
        Check(player != null, "Campaign player exists");
        player.GetComponent<PlayerInputReader>().enabled = false;
        player.Health.SetInvulnerable(true);
        var waves = Object.FindAnyObjectByType<WaveController>();
        if (waves != null) waves.enabled = false;
        player.SetGameplayActive(true);
        var sprite = player.transform.Find("Visual").GetComponent<SpriteRenderer>();
        var animator = player.GetComponent<Animator>();
        Check(animator.runtimeAnimatorController is AnimatorOverrideController, "Skin controller used by scene instance");
        yield return Frames(4);
        Check(AssetDatabase.GetAssetPath(sprite.sprite).StartsWith(CrocsSkinTools.Art), "Animated Crocs sprite visible");
        yield return Capture("crocs-in-game.png");
        int shots = 0, specials = 0;
        player.Combat.ShotFired += () => shots++;
        player.Combat.SpecialFired += () => specials++;
        Check(player.Combat.TryShoot(), "Normal shot accepted");
        Check(shots == 0, "Normal projectile waits for animation event");
        yield return Wait(.24f);
        Check(shots == 1 && !player.Combat.HasPendingAttack, "Normal animation event releases exactly once");
        yield return Wait(.3f);
        player.Combat.ConfigureBossTestSpecial(1);
        Check(player.Combat.TrySpecialAttack(), "Special accepted");
        Check(specials == 0, "Special projectile waits for animation event");
        yield return Wait(.39f);
        Check(specials == 1 && !player.Combat.HasPendingAttack, "Special animation event releases exactly once");
        yield return Wait(.5f);
        Check(player.GetComponent<PlayerDash>().TryDash(), "Native dash accepted with skin");
        yield return Wait(.24f);
        Check(!player.GetComponent<PlayerDash>().IsDashing, "Native dash completes");
        yield return Wait(.25f);
        Time.timeScale = 0;
        var paused = sprite.sprite;
        yield return Frames(8);
        Check(sprite.sprite == paused, "Skin animation obeys pause");
        Time.timeScale = 1;
        yield return Capture("crocs-after-dash.png");
        // Sample every clip in the real scene so all states can be reviewed visually.
        player.GetComponent<PlayerPresentation>().enabled = false;
        var controller = (AnimatorOverrideController)animator.runtimeAnimatorController;
        foreach (string state in new[] { "Idle", "Fly", "Shoot", "Special", "Hurt" })
        {
            var clip = controller.animationClips.First(c => c.name == "PlayerCrocs" + state);
            clip.SampleAnimation(player.gameObject, state == "Special" ? .29f : .12f);
            Check(AssetDatabase.GetAssetPath(sprite.sprite).StartsWith(CrocsSkinTools.Art), state + " uses Crocs");
            yield return Capture("crocs-" + state.ToLowerInvariant() + ".png");
        }
        finished = true;
    }

    static IEnumerator Capture(string name)
    {
        var camera = Camera.main;
        var overlays = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var c in overlays) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = camera; c.planeDistance = 1; }
        var rt = new RenderTexture(1280, 720, 24);
        var previousTarget = camera.targetTexture;
        float aspect = camera.aspect;
        camera.targetTexture = rt; camera.aspect = 16f / 9f;
        yield return Frames(3);
        Canvas.ForceUpdateCanvases(); camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = rt;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
        File.WriteAllBytes(Output + "/" + name, image.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = previousTarget; camera.aspect = aspect;
        foreach (var c in overlays) { c.renderMode = RenderMode.ScreenSpaceOverlay; c.worldCamera = null; }
        rt.Release(); Object.Destroy(rt); Object.Destroy(image);
    }

    static IEnumerator Frames(int n) { while (n-- > 0) { int frame = Time.frameCount; while (Time.frameCount == frame) yield return null; } }
    static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    static void Check(bool value, string label) { if (!value) throw new Exception("CROCS_FAILED " + label); checks++; Debug.Log("CROCS_CHECK " + label); }
    static void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Crocs validation");
            while (Steps.Count > 0)
            {
                var step = Steps.Peek();
                if (!step.MoveNext()) { Steps.Pop(); continue; }
                if (step.Current is IEnumerator child) { Steps.Push(child); continue; }
                return;
            }
        }
        catch (Exception exception) { errors++; Debug.LogException(exception); }
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
        Time.timeScale = 1; LeaderboardSession.Cancel();
        File.WriteAllText(Output + "/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + ",\"finished\":" + finished.ToString().ToLowerInvariant() + "}");
        Debug.Log("CROCS_PLAY_RESULT checks=" + checks + " errors=" + errors + " finished=" + finished);
        EditorApplication.isPlaying = false;
    }
}
