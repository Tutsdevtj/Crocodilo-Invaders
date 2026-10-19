using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

/// <summary>Real Play Mode integration: synthetic devices feed the ordinary action asset and physics.</summary>
[InitializeOnLoad]
public static class HubPlayValidation
{
    private const string Session = "Crocodilo.Hub.Validation";
    private const string Output = "Logs/HubCamera";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static readonly List<string> report = new List<string>();
    private static Keyboard keyboard;
    private static Gamepad gamepad;
    private static Key key;
    private static float stick;
    private static bool running;
    private static int checks, errors, savedVsync, savedRate;
    private static double deadline;
    private static HubCharacterMotor2D motor;
    private static HubCameraController2D follow;
    private static Rigidbody2D body;
    private static Camera camera;

    static HubPlayValidation()
    {
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Session, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }

    [MenuItem("Crocodilo/Base/Validate HUB gameplay in Play Mode")]
    public static void Run()
    {
        RunValidation(false);
    }

    [MenuItem("Crocodilo/Base/Validate and capture complete Croc map tour")]
    public static void RunMapTour()
    {
        RunValidation(true);
    }

    [MenuItem("Crocodilo/Base/Validate existing pause in HUB")]
    public static void RunPauseMenu()
    {
        HubCharacterTools.InstallPauseMenu();
        RunValidation(false, true);
    }

    private static void RunValidation(bool tourOnly, bool pauseOnly = false)
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        HubCharacterTools.Validate();
        BaseImprovisadaMapTools.Validate();
        SetGameViewSize(1280, 720);
        SessionState.SetBool(Session, true);
        SessionState.SetBool(Session + ".TourOnly", tourOnly);
        SessionState.SetBool(Session + ".PauseOnly", pauseOnly);
        EditorApplication.isPlaying = true;
    }

    public static void InstallAndRun() { HubCharacterTools.Install(); Run(); }

    private static void Bind()
    {
        motor = Object.FindAnyObjectByType<HubCharacterMotor2D>();
        body = motor.GetComponent<Rigidbody2D>();
        camera = Camera.main;
        follow = camera.GetComponent<HubCameraController2D>();
    }

    private static IEnumerator Scenario()
    {
        yield return Frames(10);
        Bind();
        Check(Screen.width == 1280 && Screen.height == 720, $"Actual Game View {Screen.width}x{Screen.height}");
        Check(camera.orthographicSize == 4.5f, "720p uses the map's original nine-unit framing");
        yield return Seconds(.4f);
        Vector3 cam = camera.transform.position;
        var sprite = motor.GetComponentInChildren<SpriteRenderer>();
        var seen = new HashSet<string>();
        float minY = float.MaxValue, maxY = float.MinValue;
        yield return Seconds(1.4f, () =>
        {
            seen.Add(sprite.sprite.name);
            minY = Mathf.Min(minY, camera.transform.position.y);
            maxY = Mathf.Max(maxY, camera.transform.position.y);
        });
        Check(seen.Count >= 5 && sprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Real Animator cycles approved Idle sprites");
        Check((camera.transform.position - cam).sqrMagnitude < .000001f && maxY - minY < .00001f, "A: idle leaves the camera completely still, including Y");
        Check(Mathf.Abs(body.position.y) < .035f, "Feet rest on existing floor");
        yield return Capture("A_idle");

        Warp(30f);
        yield return Frames(4);
        cam = camera.transform.position;
        key = Key.D;
        yield return Seconds(.3f);
        Check(body.position.x > 30.4f && Mathf.Abs(camera.transform.position.x - cam.x) < .0001f, "B: movement inside the actual camera window does not move the camera");
        yield return Seconds(3f);
        Check(body.linearVelocity.x > 2.9f && camera.transform.position.x > cam.x + 2f,
            $"D accelerates and camera follows: x={body.position.x:0.000}, vx={body.linearVelocity.x:0.000}, cam={camera.transform.position.x:0.000}, window={follow.WindowCenter.x:0.000}, input={motor.GetComponent<HubPlayerInput>().Horizontal:0.00}");
        float rightScreen = camera.WorldToViewportPoint(follow.TrackingTarget.position).x;
        Check(rightScreen > .2f && rightScreen < .49f, $"Rightward anticipation leaves Croc behind centre ({rightScreen:0.000})");
        Check(sprite.transform.localScale.x > 0f && motor.transform.localScale == Vector3.one, "Facing right affects only the visual");
        yield return Capture("B_right");

        key = Key.LeftArrow;
        float lastCam = camera.transform.position.x, biggestStep = 0f;
        yield return Seconds(3f, () =>
        {
            biggestStep = Mathf.Max(biggestStep, Mathf.Abs(camera.transform.position.x - lastCam));
            lastCam = camera.transform.position.x;
            CheckVisible();
        });
        float leftScreen = camera.WorldToViewportPoint(follow.TrackingTarget.position).x;
        Check(body.linearVelocity.x < -2.9f && leftScreen > .51f && leftScreen < .8f, $"C: left arrow moves left with opposite framing ({leftScreen:0.000})");
        Check(sprite.transform.localScale.x < 0f && motor.transform.localScale == Vector3.one, "Only VisualRoot mirrors left; physics and tracking keep their orientation");
        Check(biggestStep < .7f, $"Reversal is continuous (largest rendered frame displacement {biggestStep:0.000})");
        yield return Capture("C_left");

        float beforeLook = follow.CurrentLookAhead;
        for (int i = 0; i < 6; i++) { key = i % 2 == 0 ? Key.D : Key.A; yield return Seconds(.06f); }
        Check(Mathf.Abs(follow.CurrentLookAhead - beforeLook) < .4f, "D: brief alternating taps do not flip anticipation");
        key = Key.None;
        yield return Seconds(1.8f);
        cam = camera.transform.position;
        yield return Seconds(.8f);
        Check(Mathf.Abs(body.linearVelocity.x) < .001f && Mathf.Abs(camera.transform.position.x - cam.x) < .002f, "E: release decelerates to zero and camera settles");
        Check(Mathf.Abs(camera.transform.position.y - 2.4f) < .0001f, "Walking and Idle never change the base framing height");
        yield return Capture("E_stop");

        stick = .55f;
        yield return Seconds(.7f);
        Check(body.linearVelocity.x > .8f && body.linearVelocity.x < 2.1f, $"Analog stick produces proportional movement ({body.linearVelocity.x:0.000})");
        stick = .05f;
        yield return Seconds(.4f);
        Check(Mathf.Abs(body.linearVelocity.x) < .001f, "Analog dead zone rejects stick drift");
        stick = 0f;
        key = Key.RightArrow;
        yield return Seconds(.4f);
        var input = motor.GetComponent<HubPlayerInput>();
        input.enabled = false;
        yield return Seconds(.4f);
        Check(Mathf.Abs(body.linearVelocity.x) < .001f && input.Horizontal == 0f, "Disable clears input and motor decelerates");
        input.enabled = true;
        yield return Seconds(.4f);
        Check(body.linearVelocity.x > 2.9f, "Re-enable reads the still-held arrow normally");
        Time.timeScale = 0f;
        cam = camera.transform.position;
        Vector2 paused = body.position;
        yield return Seconds(.4f);
        Check((camera.transform.position - cam).sqrMagnitude < .000001f && body.position == paused, "Pause freezes both physics and camera");
        key = Key.None;
        Time.timeScale = 1f;
        yield return Seconds(.4f);

        Warp(BaseImprovisadaMapTools.ExpansionGateX - 1.5f);
        key = Key.D;
        yield return Seconds(1.5f);
        Check(body.position.x < BaseImprovisadaMapTools.ExpansionGateX && body.position.x > BaseImprovisadaMapTools.ExpansionGateX - .75f, "F: existing locked expansion gate blocks actual Croc");
        cam = camera.transform.position;
        yield return Seconds(.8f);
        Check(Mathf.Abs(camera.transform.position.x - cam.x) < .02f, "Camera stops at a blocked character even with D held");
        var expansion = Object.FindObjectsByType<BaseSector>().Single(s => s.SectorId == "expansion");
        expansion.SetState(BaseSector.State.Open);
        Warp(BaseImprovisadaMapTools.PlayableRight - 1.5f);
        key = Key.D;
        yield return Seconds(1.5f);
        Check(body.position.x < BaseImprovisadaMapTools.PlayableRight && body.position.x > BaseImprovisadaMapTools.PlayableRight - .7f,
            $"Actual right physical wall holds Croc (x={body.position.x:0.000}, input={input.Horizontal:0.00})");
        CheckCoverage();
        yield return Capture("F_right_wall");
        Warp(BaseImprovisadaMapTools.PlayableLeft + 1.5f);
        key = Key.A;
        yield return Seconds(1.5f);
        Check(body.position.x > BaseImprovisadaMapTools.PlayableLeft && body.position.x < BaseImprovisadaMapTools.PlayableLeft + .7f, "Actual left physical wall holds Croc");
        CheckCoverage();
        yield return Capture("F_left_wall");
        key = Key.None;
        expansion.SetState(BaseSector.State.Locked);

        yield return WalkEntireMap();

        foreach (int fps in new[] { 30, 60, 120 })
        {
            Application.targetFrameRate = fps;
            Warp(30f);
            yield return Seconds(.3f);
            key = Key.D;
            yield return Seconds(.6f);
            float startX = body.position.x, startTime = Time.fixedTime;
            int count = 0; float duration = 0f, worstFrame = 0f, x = camera.transform.position.x;
            yield return Seconds(2f, () =>
            {
                count++; duration += Time.unscaledDeltaTime;
                worstFrame = Mathf.Max(worstFrame, Mathf.Abs(camera.transform.position.x - x));
                x = camera.transform.position.x;
                CheckVisible();
            });
            float measuredSpeed = (body.position.x - startX) / (Time.fixedTime - startTime);
            float actualFps = count / duration;
            Check(Mathf.Abs(measuredSpeed - motor.MaxMoveSpeed) < .035f, $"G: {fps} FPS cap, physics speed {measuredSpeed:0.000} units/sec");
            Check(camera.WorldToViewportPoint(follow.TrackingTarget.position).x < .49f, "Directional framing remains forward at cap " + fps);
            report.Add($"Requested {fps} FPS; actual rendered average {actualFps:0.0}; speed {measuredSpeed:0.000}; max camera step {worstFrame:0.0000}.");
            yield return Capture("G_move_cap_" + fps);
            key = Key.None;
        }

        foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(800, 600), new Vector2Int(1680, 720), new Vector2Int(2560, 720), new Vector2Int(320, 180) })
        {
            SetGameViewSize(size.x, size.y);
            yield return Frames(15);
            Check(Screen.width == size.x && Screen.height == size.y, $"H: actual Game View resized to {Screen.width}x{Screen.height}");
            float artPixelScale = Screen.height / (2f * camera.orthographicSize * 80f);
            Check(Mathf.Abs(artPixelScale - Mathf.Round(artPixelScale)) < .001f && camera.orthographicSize <= 4.5001f,
                $"Integer pixel scale {artPixelScale:0.00}, half-view {camera.orthographicSize:0.000}");
            foreach (float x in new[] { BaseImprovisadaMapTools.PlayableLeft + .4f, 30f, 78f, BaseImprovisadaMapTools.PlayableRight - .4f })
            {
                Warp(x);
                yield return Frames(4);
                CheckCoverage();
            }
            yield return Capture($"H_right_edge_{size.x}x{size.y}");
            Warp(30f);
            key = Key.D;
            yield return Seconds(2.5f);
            CheckVisible();
            yield return Capture($"H_hangar_{size.x}x{size.y}");
            key = Key.None;
        }

        // Oversized viewport: stable centre instead of inverted Clamp limits.
        follow.enabled = false;
        var perfect = camera.GetComponent<PixelPerfectCamera>();
        perfect.enabled = false;
        camera.orthographicSize = 100f;
        var confined = follow.Confine(new Vector2(999f, 999f));
        Check((confined - follow.CameraBounds.center).sqrMagnitude < .00001f, "Oversized viewport centres both axes stably");
        perfect.enabled = true;
        follow.enabled = true;
        yield return Frames(4);
        SetGameViewSize(1280, 720);
        yield return Frames(8);
        // Snapping comparison uses the same real movement and native renderer.
        Warp(30f);
        perfect.pixelSnapping = false;
        key = Key.D;
        yield return Seconds(2f);
        yield return Capture("pixel_comparison_without_sprite_snap");
        perfect.pixelSnapping = true;
        Warp(30f);
        key = Key.D;
        yield return Seconds(2f);
        yield return Capture("pixel_comparison_with_sprite_snap");
        key = Key.None;

        yield return SceneManager.LoadSceneAsync("BaseImprovisada");
        yield return Frames(10);
        Bind();
        key = Key.D;
        yield return Seconds(.5f);
        Check(motor != null && body.linearVelocity.x > 2.9f && follow.TrackingTarget != null && Object.FindObjectsByType<HubCharacterMotor2D>().Length == 1,
            "Scene reload reconnects actions, physics and camera without duplicates");
        key = Key.None;
        Directory.CreateDirectory(Output);
        File.WriteAllLines(Output + "/report.txt", report);
    }

    private static void Warp(float x)
    {
        key = Key.None; stick = 0f;
        body.position = new Vector2(x, .015f);
        body.transform.position = new Vector3(x, .015f, 0f);
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        follow.SnapToTarget();
    }

    private static IEnumerator WalkEntireMap()
    {
        Warp(BaseImprovisadaMapTools.PlayableLeft + 1f);
        Time.timeScale = 2f; // Physics still runs its ordinary fixed steps; faster inspection only.
        key = Key.D;
        var shots = new[] { ("A_entrance", 0f), ("B_infirmary", 9.8f), ("BC_yard", 20.9f), ("C_hangar", 29.8f),
            ("C_workshop", 38.5f), ("D_camp", 50f), ("E_command", 64.8f), ("F_radio", 77f) };
        int next = 0;
        float minY = 999f, maxY = -999f;
        double limit = EditorApplication.timeSinceStartup + 30;
        while (body.position.x < BaseImprovisadaMapTools.ExpansionGateX - .8f)
        {
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("Croc could not traverse the open base");
            yield return Frames(1);
            minY = Mathf.Min(minY, body.position.y); maxY = Mathf.Max(maxY, body.position.y);
            CheckVisible();
            if (next < shots.Length && body.position.x >= shots[next].Item2)
            {
                yield return Capture("tour_" + shots[next].Item1);
                next++;
            }
        }
        yield return Seconds(.5f);
        Check(next == shots.Length && body.position.x < BaseImprovisadaMapTools.ExpansionGateX, "Actual enlarged Croc traverses A-F without a break and stops at the locked gate");
        var expansion = Object.FindObjectsByType<BaseSector>().Single(s => s.SectorId == "expansion");
        expansion.SetState(BaseSector.State.Open);
        limit = EditorApplication.timeSinceStartup + 8;
        while (body.position.x < BaseImprovisadaMapTools.PlayableRight - .8f)
        {
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("Expanded route blocked");
            yield return Frames(1);
            minY = Mathf.Min(minY, body.position.y); maxY = Mathf.Max(maxY, body.position.y);
            CheckVisible();
        }
        yield return Capture("tour_G_expansion");
        key = Key.A;
        limit = EditorApplication.timeSinceStartup + 25;
        while (body.position.x > BaseImprovisadaMapTools.PlayableLeft + .8f)
        {
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("Return route blocked");
            yield return Frames(1);
            minY = Mathf.Min(minY, body.position.y); maxY = Mathf.Max(maxY, body.position.y);
            CheckVisible();
        }
        Check(minY > -.035f && maxY < .035f, $"Complete physical round trip has continuous support (root Y {minY:0.0000}..{maxY:0.0000})");
        report.Add("Enlarged Croc walked the whole map both ways, at timeScale 2 for inspection; no terrain gaps or loss of support.");
        key = Key.None; Time.timeScale = 1f;
        expansion.SetState(BaseSector.State.Locked);
        yield return Seconds(.4f);
    }

    private static void CheckVisible()
    {
        float x = camera.WorldToViewportPoint(follow.TrackingTarget.position).x;
        if (x < .10f || x > .90f) throw new InvalidOperationException($"Unsafe tracking at viewport x {x}");
        var visible = motor.GetComponentInChildren<SpriteRenderer>().bounds;
        float bottom = camera.WorldToViewportPoint(visible.min).y, top = camera.WorldToViewportPoint(visible.max).y;
        if (bottom < .02f || top > .98f) throw new InvalidOperationException($"Sprite clipped vertically: {bottom:0.000}..{top:0.000}");
    }

    private static void CheckCoverage()
    {
        float hy = camera.orthographicSize, hx = hy * camera.aspect;
        Vector3 p = camera.transform.position;
        var bounds = follow.CameraBounds;
        var panorama = GameObject.Find("BG0_Panorama").GetComponentInChildren<SpriteRenderer>().bounds;
        Check(p.x - hx >= bounds.xMin - .001f && p.x + hx <= bounds.xMax + .001f && p.y - hy >= bounds.yMin - .001f && p.y + hy <= bounds.yMax + .001f,
            "Entire viewport fits visual bounds at " + p.x.ToString("0.00"));
        Check(panorama.min.x <= p.x - hx && panorama.max.x >= p.x + hx && panorama.min.y <= p.y - hy + .001f && panorama.max.y >= p.y + hy - .001f,
            "Existing parallax panorama covers entire viewport");
        CheckVisible();
    }

    private static IEnumerator Capture(string name)
    {
        yield return Frames(2);
        Directory.CreateDirectory(Output);
        var texture = new RenderTexture(Screen.width, Screen.height, 24) { filterMode = FilterMode.Point, antiAliasing = 1 };
        var previous = RenderTexture.active;
        camera.targetTexture = texture;
        camera.Render();
        RenderTexture.active = texture;
        var png = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        png.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        png.Apply();
        File.WriteAllBytes(Output + "/" + name + ".png", png.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = previous;
        Object.Destroy(png); Object.Destroy(texture);
    }

    private static void InjectInput()
    {
        if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(keyboard, key == Key.None ? new KeyboardState() : new KeyboardState(key));
        var state = new GamepadState { leftStick = new Vector2(stick, 0f) };
        if (startButton) state = state.WithButton(GamepadButton.Start);
        InputSystem.QueueStateEvent(gamepad, state);
    }

    private static void Begin()
    {
        if (running) return;
        running = true; checks = errors = 0; key = Key.None; stick = 0; startButton = false;
        report.Clear(); steps.Clear();
        savedVsync = QualitySettings.vSyncCount; savedRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
        keyboard = InputSystem.AddDevice<Keyboard>(); gamepad = InputSystem.AddDevice<Gamepad>();
        InputSystem.onBeforeUpdate += InjectInput;
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 200;
        steps.Push(SessionState.GetBool(Session + ".PauseOnly", false) ? PauseScenario()
            : SessionState.GetBool(Session + ".TourOnly", false) ? TourScenario() : Scenario());
        EditorApplication.update += Tick;
    }

    private static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Session, false)) Begin();
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Session, false))
        {
            SessionState.SetBool(Session, false);
            if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetInt(Session + ".Errors", 1) == 0 ? 0 : 1);
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("HUB Play validation timed out");
            while (steps.Count > 0)
            {
                var top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                if (top.Current is AsyncOperation operation) { steps.Push(WaitOperation(operation)); continue; }
                return;
            }
            if (errors > 0) throw new InvalidOperationException(errors + " runtime errors logged");
            foreach (string line in report) Debug.Log("HUB REPORT: " + line);
            Debug.Log("HUB PLAY VALIDATION PASSED: " + checks + " assertions.");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        InputSystem.onBeforeUpdate -= InjectInput;
        Application.logMessageReceived -= OnLog;
        InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(gamepad);
        Time.timeScale = 1f;
        QualitySettings.vSyncCount = savedVsync; Application.targetFrameRate = savedRate;
        running = false;
        SessionState.SetInt(Session + ".Errors", errors);
        EditorApplication.isPlaying = false;
    }

    private static IEnumerator WaitOperation(AsyncOperation operation) { while (!operation.isDone) yield return null; }
    private static IEnumerator TourScenario()
    {
        yield return Frames(10);
        Bind();
        yield return WalkEntireMap();
        CheckCoverage();
    }

    private static bool startButton;
    private static IEnumerator PauseScenario()
    {
        yield return Frames(8);
        Bind();
        var pause = Object.FindAnyObjectByType<PauseController>();
        var view = pause.GetComponent<PauseMenuView>();
        var input = motor.GetComponent<HubPlayerInput>();
        var buttons = view.GetComponentsInChildren<UnityEngine.UI.Button>(true).ToDictionary(b => b.name);
        key = Key.D;
        yield return Seconds(.3f);
        Check(body.linearVelocity.x > 2.9f, "Croc moves before pause");
        key = Key.Escape;
        yield return Seconds(.35f);
        yield return Frames(15);
        Check(pause.IsPaused && SceneManager.GetActiveScene().name == "BaseImprovisada", "ESC opens existing pause and stays in the base");
        Check(Time.timeScale == 0f && AudioListener.pause && !input.enabled, "Pause freezes clock/audio and disables HUB movement input");
        yield return CapturePause("HUB_pause_existing_menu");
        Check(buttons["ContinueButton"].IsInteractable() && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttons["ContinueButton"].gameObject,
            $"Original pause UI ready: continue={buttons["ContinueButton"].IsInteractable()}, selection={UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.name}");
        Vector2 held = body.position;
        Vector3 cameraHeld = camera.transform.position;
        Sprite idleHeld = motor.GetComponentInChildren<SpriteRenderer>().sprite;
        key = Key.None;
        yield return Seconds(.3f);
        Check(body.position == held && camera.transform.position == cameraHeld && motor.GetComponentInChildren<SpriteRenderer>().sprite == idleHeld,
            "Croc physics, camera and Idle remain frozen");
        yield return CapturePause("HUB_pause_existing_menu");
        key = Key.Enter;
        yield return Frames(3);
        key = Key.None;
        yield return Seconds(.3f);
        Check(!pause.IsPaused && input.enabled && Time.timeScale == 1f && !AudioListener.pause, "Enter on Continue resumes the same base with controls/audio restored");
        key = Key.Escape;
        yield return Seconds(.3f);
        key = Key.None;
        yield return Frames(3);
        buttons["OptionsButton"].onClick.Invoke();
        yield return Seconds(.4f);
        Check(pause.IsPaused && view.ShowingOptions && Object.FindAnyObjectByType<AudioOptionsView>().IsVisible, "Original Options screen opens while the base stays paused");
        yield return CapturePause("HUB_pause_options");
        key = Key.Escape;
        yield return Seconds(.35f);
        key = Key.None;
        yield return Frames(3);
        Check(pause.IsPaused && !view.ShowingOptions, "ESC from Options returns to pause without unpausing");
        startButton = true;
        yield return Seconds(.3f);
        startButton = false;
        yield return Frames(3);
        Check(!pause.IsPaused && input.enabled && Time.timeScale == 1f, "Gamepad Start resumes the base");
        key = Key.Escape;
        yield return Seconds(.3f);
        key = Key.None;
        int loads = 0;
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> loaded = (scene, mode) => { if (scene.name == "MainMenu") loads++; };
        SceneManager.sceneLoaded += loaded;
        try
        {
            buttons["ReturnButton"].onClick.Invoke();
            buttons["ReturnButton"].onClick.Invoke();
            yield return WaitForMenu();
            yield return Frames(8);
            Check(loads == 1 && Time.timeScale == 1f && !AudioListener.pause, "Return button loads MainMenu once and restores global state");
            Check(motor == null && Object.FindAnyObjectByType<PauseController>() == null, "HUB player and pause UI unload together");
        }
        finally { SceneManager.sceneLoaded -= loaded; }
        yield return SceneManager.LoadSceneAsync("BaseImprovisada");
        yield return Frames(8);
        Bind();
        pause = Object.FindAnyObjectByType<PauseController>();
        startButton = true;
        yield return Seconds(.3f);
        startButton = false;
        yield return Frames(3);
        Check(pause.IsPaused && Object.FindObjectsByType<PauseController>().Length == 1, "Start opens a single existing pause after reloading the base");
        key = Key.Escape;
        yield return Seconds(.3f);
        key = Key.None;
        Check(!pause.IsPaused && motor.GetComponent<HubPlayerInput>().enabled, "ESC closes pause after reload");
        yield return SceneManager.LoadSceneAsync("Assets/Scenes/Tests/PlayerMovementTest.unity");
        yield return Frames(10);
        pause = Object.FindAnyObjectByType<PauseController>();
        var combatInput = new SerializedObject(pause).FindProperty("playerInput").objectReferenceValue as PlayerInputReader;
        Check(combatInput != null, "Combat pause keeps its existing PlayerInputReader reference");
        bool enabledBefore = combatInput.enabled;
        float scaleBefore = Time.timeScale;
        bool audioBefore = AudioListener.pause;
        pause.AllowPauseWhileFrozen(true);
        pause.PauseGame();
        yield return Frames(3);
        Check(pause.IsPaused && !combatInput.enabled, "Shared controller still pauses combat input");
        pause.ResumeGame();
        yield return Frames(3);
        Check(!pause.IsPaused && combatInput.enabled == enabledBefore && Time.timeScale == scaleBefore && AudioListener.pause == audioBefore,
            "Combat resume restores its own previous state");
    }

    private static IEnumerator CapturePause(string name)
    {
        int previousMask = camera.cullingMask;
        camera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var sorting = canvases.ToDictionary(c => c, c => c.sortingLayerID);
        foreach (var canvas in canvases)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f;
            // Screen overlay is always on top. For this offscreen capture only, preserve
            // that order above the map's named sprite layers instead of behind its sky.
            canvas.sortingLayerName = "AmbientEffects";
        }
        Canvas.ForceUpdateCanvases();
        yield return Capture(name);
        foreach (var canvas in canvases) if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; canvas.sortingLayerID = sorting[canvas];
        }
        camera.cullingMask = previousMask;
    }

    private static IEnumerator WaitForMenu()
    {
        double until = EditorApplication.timeSinceStartup + 20;
        while (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (EditorApplication.timeSinceStartup > until) throw new TimeoutException("HUB did not return to MainMenu");
            yield return Frames(1);
        }
    }
    private static void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("HUB PLAY FAILED: " + message);
        checks++; Debug.Log("HUB PLAY CHECK " + checks + ": " + message);
    }
    private static IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++) { int frame = Time.frameCount; while (Time.frameCount == frame) yield return null; }
    }
    private static IEnumerator Seconds(float seconds, Action sample = null)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < until) { yield return Frames(1); sample?.Invoke(); }
    }

    // Editor-only API: set the real Game View, so the official Pixel Perfect component
    // sees the requested Screen dimensions, rather than faking Camera.aspect or a lens.
    private static void SetGameViewSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleton.GetProperty("instance").GetValue(null);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeGroupType"), "Standalone") });
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        object fixedResolution = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
        object size = Activator.CreateInstance(sizeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new[] { fixedResolution, (object)width, height, "HUB validation " + width + "x" + height }, null);
        var groupType = group.GetType();
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int index = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
        var gameView = EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
        gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(gameView, index);
        gameView.Repaint();
    }
}
