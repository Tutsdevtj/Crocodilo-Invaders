using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Play Mode real: cenas de gameplay, Input System com teclado/mouse virtuais e Animation Events nativos.
// Não conclui runs: o ranking não recebe resultado.
[InitializeOnLoad]
public static class DialoguePlayValidation
{
    private const string Session = "Crocodilo.Dialogue.Validation";
    private const string Gameplay = "Assets/Scenes/Tests/PlayerMovementTest.unity";
    private const string BossScene = "Assets/Scenes/Tests/GusiniBossTest.unity";
    private static IEnumerator scenario;
    private static readonly System.Collections.Generic.Stack<IEnumerator> steps = new System.Collections.Generic.Stack<IEnumerator>();
    private static double deadline;
    private static int checks, errors, minimumChecks;
    private static Keyboard keyboard;
    private static Mouse mouse;
    private static InputSettings.BackgroundBehavior originalBackground;
    private static InputSettings.EditorInputBehaviorInPlayMode originalEditorBehavior;

    static DialoguePlayValidation()
    {
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Session, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }

    [MenuItem("Crocodilo/Dialogue/Validate cinematics in Play Mode")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        DialogueTools.Validate();
        DialogueTools.ValidateScenes();
        EditorSceneManager.OpenScene(Gameplay, OpenSceneMode.Single);
        SessionState.SetBool(Session, true);
        EditorApplication.isPlaying = true;
    }

    // Capturas locais (Logs/ é ignorado pelo Git) do painel e retratos sobre a cena real.
    [MenuItem("Crocodilo/Dialogue/Capture dialogue previews")]
    public static void RunCapture()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(BossScene, OpenSceneMode.Single);
        SessionState.SetBool(Session, true);
        SessionState.SetBool(Session + ".Capture", true);
        EditorApplication.isPlaying = true;
    }

    private static IEnumerator CaptureScenario()
    {
        yield return Frames(3);
        var runner = Find<RunCutscenes>().Dialogue;
        yield return Until(() => runner.IsPlaying && runner.LineIndex == 0 && runner.VisibleCharacters > 0, 20, "Croc line");
        yield return Press(Key.Enter);
        yield return Press(Key.Enter);
        yield return Until(() => runner.LineIndex == 1 && runner.VisibleCharacters > 0, 10, "Gusini line");
        yield return Press(Key.Enter);
        yield return Realtime(.6f);
        foreach (Vector2Int size in new[] { new Vector2Int(1200, 720), new Vector2Int(1920, 1080), new Vector2Int(800, 600) })
            yield return Capture(size.x, size.y, "boss_intro");
        var waves = Find<WaveController>();
        var cutscenes = Find<RunCutscenes>();
        yield return HoldSkip(.7f);
        yield return Until(() => waves.ActiveBoss.Ready, 5, "combat");
        waves.ActiveBoss.Health.TakeDamage(waves.ActiveBoss.Health.PreciseCurrentHealth + 1f);
        yield return Until(() => cutscenes.Playing == RunCutscenes.Scene.PhaseTransition && runner.LineIndex == 0 && runner.VisibleCharacters > 0, 10, "transition");
        yield return Press(Key.Enter);
        yield return Press(Key.Enter);
        yield return Until(() => runner.LineIndex == 1 && runner.VisibleCharacters > 0, 10, "Croc answer");
        yield return Press(Key.Enter);
        yield return Realtime(.6f);
        yield return Capture(1200, 720, "false_defeat");
        yield return HoldSkip(.7f);
        var phase = waves.ActiveBoss.GetComponent<GusiniPhaseTwo>();
        yield return Until(() => phase.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo, 8, "phase 2");
        waves.ActiveBoss.Health.TakeDamage(waves.ActiveBoss.Health.PreciseCurrentHealth + 1f);
        yield return Until(() => runner.IsPlaying && runner.LineIndex == 0 && runner.VisibleCharacters > 0, 10, "finale");
        yield return Press(Key.Enter);
        yield return Realtime(.6f);
        yield return Capture(1200, 720, "finale_recovered");
        for (int i = 0; i < 8; i++)
        {
            yield return Until(() => runner.LineIndex == i && runner.VisibleCharacters > 0, 10, "finale line");
            yield return Press(Key.Enter);
            yield return Press(Key.Enter);
        }
        yield return Until(() => runner.LineIndex == 8 && runner.VisibleCharacters > 0, 10, "radio line");
        yield return Press(Key.Enter);
        yield return Realtime(.3f);
        yield return Capture(1200, 720, "finale_radio");
        yield return HoldSkip(.7f);
        var transition = Find<ChapterFlow>().Transition;
        yield return Until(() => transition.Current == SceneTransition.State.DestinationUnavailable, 5, "fallback");
        yield return Realtime(1.2f);
        yield return Capture(1200, 720, "chapter_end_fallback");
        Check(true, "Captured dialogue previews in Logs/Dialogue");
    }

    private static IEnumerator Capture(int width, int height, string name)
    {
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var texture = new RenderTexture(width, height, 24);
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
        camera.targetTexture = texture;
        yield return Frames(4);
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = texture;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        pixels.Apply();
        RenderTexture.active = null;
        System.IO.Directory.CreateDirectory("Logs/Dialogue");
        System.IO.File.WriteAllBytes($"Logs/Dialogue/{name}_{width}x{height}.png", pixels.EncodeToPNG());
        camera.targetTexture = null;
        foreach (Canvas canvas in canvases) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(pixels);
        texture.Release();
        yield return Frames(2);
    }

    private static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Session, false)) Begin();
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Session, false))
        {
            SessionState.SetBool(Session, false);
            if (Application.isBatchMode) EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }

    private static void Begin()
    {
        if (scenario != null) return;
        checks = errors = 0;
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 240;
        var settings = InputSystem.settings;
        originalBackground = settings.backgroundBehavior;
        originalEditorBehavior = settings.editorInputBehaviorInPlayMode;
        // Batch não tem Game View focada; o teste precisa receber o teclado virtual.
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        // Progresso narrativo em pasta descartável: o teste não marca o save real do jogador.
        string progress = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CrocodiloDialogueValidation_" + Guid.NewGuid().ToString("N"));
        CampaignProgress.UseDirectory(progress);
        keyboard = InputSystem.AddDevice<Keyboard>("DialogueValidationKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("DialogueValidationMouse");
        bool capture = SessionState.GetBool(Session + ".Capture", false);
        SessionState.SetBool(Session + ".Capture", false);
        scenario = capture ? CaptureScenario() : Scenario();
        minimumChecks = capture ? 1 : 40;
        steps.Clear();
        steps.Push(scenario);
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Dialogue play validation timed out");
            // EditorApplication.update não executa enumeradores aninhados como uma coroutine; a pilha faz isso.
            while (steps.Count > 0)
            {
                IEnumerator top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            if (checks < minimumChecks) throw new InvalidOperationException("Too few assertions ran: " + checks);
            Debug.Log("DIALOGUE PLAY VALIDATION PASSED: " + checks + " assertions; intro, boss intro, false defeat, input, audio, pause and skip.");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        scenario = null;
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        if (mouse != null) InputSystem.RemoveDevice(mouse);
        InputSystem.settings.backgroundBehavior = originalBackground;
        InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorBehavior;
        Time.timeScale = 1f; AudioListener.pause = false;
        EditorApplication.isPlaying = false;
    }

    private static void OnLog(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("DIALOGUE CHECK FAILED: " + message);
        checks++; Debug.Log("DIALOGUE CHECK " + checks + ": " + message);
    }

    private static IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int frame = Time.frameCount;
            while (Time.frameCount == frame) yield return null;
        }
    }

    private static IEnumerator Until(Func<bool> condition, float seconds, string message)
    {
        double limit = EditorApplication.timeSinceStartup + seconds;
        while (!condition())
        {
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("Waiting for " + message);
            yield return null;
        }
    }

    private static IEnumerator Realtime(float seconds)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < until) yield return null;
    }

    private static IEnumerator Press(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return Frames(1);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return Frames(1);
    }

    private static IEnumerator Click()
    {
        InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left));
        yield return Frames(1);
        InputSystem.QueueStateEvent(mouse, new MouseState());
        yield return Frames(1);
    }

    private static IEnumerator HoldSkip(float seconds)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab));
        float started = Time.unscaledTime;
        while (Time.unscaledTime - started < seconds) yield return Frames(1);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return Frames(2);
    }

    private static T Find<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include).Single();

    private static IEnumerator Load(string path)
    {
        EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
        yield return Until(() => SceneManager.GetActiveScene().path == path && SceneManager.GetActiveScene().isLoaded, 20, "scene " + path);
        yield return Frames(3);
    }

    private static IEnumerator Scenario()
    {
        yield return Frames(3);
        yield return RunIntro();
        yield return Load(BossScene);
        yield return BossScenes();
        yield return Load(Gameplay);
        yield return SkippedIntro();
    }

    private static IEnumerator RunIntro()
    {
        var waves = Find<WaveController>();
        var cutscenes = Find<RunCutscenes>();
        var runner = cutscenes.Dialogue;
        var pause = Find<PauseController>();
        var player = Find<Player>();
        int shots = 0;
        player.Combat.ShotStarted += () => shots++;
        waves.StartRun();
        Check(waves.Phase == WaveController.RunPhase.Intro && runner.IsPlaying, "New run opens the radio intro before any wave");
        Check(!player.Movement.enabled && !player.Combat.enabled && !player.Dash.enabled, "Player is locked during the intro");
        yield return Until(() => runner.LineIndex == 0 && runner.VisibleCharacters >= 4, 10, "intro typing");
        Check(runner.VisibleCharacters < runner.TotalCharacters, "Text appears one letter at a time");
        Check(runner.LeftPortrait.enabled && runner.NameText.text == "B. Crocodilo", "Croc portrait and nameplate");
        int blips = runner.Blips.PlayedCount;
        Check(blips > 0 && blips <= runner.VisibleCharacters / 2 + 1, "Typing blips follow revealed letters with limited rate");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
        yield return Frames(4);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        Check(shots == 0, "Fire input causes no gameplay action during the intro");
        yield return Press(Key.Enter);
        Check(runner.LineIndex == 0 && runner.VisibleCharacters == runner.TotalCharacters && runner.AwaitingAdvance, "Enter reveals the whole line without advancing");
        Check(runner.Blips.PlayedCount <= blips + 1, "Instant reveal does not burst the remaining letter sounds");
        pause.PauseGame();
        yield return Press(Key.Enter);
        Check(pause.IsPaused && runner.LineIndex == 0 && runner.AwaitingAdvance, "Enter under the pause menu does not reach the dialogue");
        pause.ResumeGame();
        yield return Frames(2);
        Check(runner.LineIndex == 0 && runner.AwaitingAdvance, "Resuming does not advance by itself");
        yield return Press(Key.Enter);
        yield return Until(() => runner.LineIndex == 1 && runner.VisibleCharacters >= 2, 10, "second intro line");
        Check(runner.LineIndex == 1, "Second press advances exactly one line");
        pause.PauseGame();
        int frozen = runner.VisibleCharacters;
        yield return Frames(12);
        Check(runner.VisibleCharacters == frozen, "Pause freezes typing");
        pause.ResumeGame();
        yield return Frames(2);
        yield return Click();
        Check(runner.LineIndex == 1 && runner.VisibleCharacters == runner.TotalCharacters, "Left click reveals the whole line");
        Check(runner.LeftPortrait.sprite.name == "crocodilo_03_confuso_preocupado", "Worried expression on second line (" + runner.LeftPortrait.sprite.name + ")");
        yield return Click();
        yield return Until(() => runner.LineIndex == 2 && runner.VisibleCharacters > 0, 10, "third intro line");
        int lineStartBlips = runner.Blips.PlayedCount;
        yield return Until(() => runner.AwaitingAdvance, 10, "natural typing end");
        int letters = runner.Body.text.Count(char.IsLetterOrDigit);
        int played = runner.Blips.PlayedCount - lineStartBlips;
        Check(played > 3 && played <= letters / 2 + 1, $"Natural typing plays {played} blips for {letters} letters");
        Check(runner.LeftPortrait.sprite.name == "crocodilo_05_determinado", "Determined expression on last intro line");
        yield return Press(Key.Enter);
        yield return Until(() => !runner.IsPlaying, 5, "intro close");
        Check(waves.Phase == WaveController.RunPhase.Announcing && waves.CurrentWave == 1, "Wave 1 starts after the intro");
        Check(player.Movement.enabled && player.Combat.enabled && player.Dash.enabled, "Controls return after the intro");
        Check(!cutscenes.PlayRunIntro(null), "Intro plays once per run");
        yield return Until(() => waves.Phase == WaveController.RunPhase.Combat, 5, "wave 1 combat");
        Check(!runner.IsPlaying && cutscenes.Playing == RunCutscenes.Scene.None, "Wave start does not replay the intro");
    }

    private static IEnumerator BossScenes()
    {
        var waves = Find<WaveController>();
        var cutscenes = Find<RunCutscenes>();
        var runner = cutscenes.Dialogue;
        var pause = Find<PauseController>();
        var player = Find<Player>();
        yield return Until(() => waves.ActiveBoss != null, 10, "boss spawn");
        Boss boss = waves.ActiveBoss;
        var combat = boss.GetComponent<GusiniCombat>();
        var phase = boss.GetComponent<GusiniPhaseTwo>();
        var music = boss.GetComponent<GusiniMusic>();
        int phaseOneAttacks = 0, phaseTwoAttacks = 0;
        combat.AttackStarted += _ => phaseOneAttacks++;
        phase.AttackStarted += _ => phaseTwoAttacks++;
        Check(boss.CombatHeld && !runner.IsPlaying, $"Gusini combat held and no dialogue during the entrance (held {boss.CombatHeld}, arrived {boss.Movement.HasArrived}, playing {runner.IsPlaying}, t {Time.time:0.00})");
        Check(!player.Movement.enabled, "Player locked during the boss entrance");
        yield return Until(() => runner.IsPlaying && runner.LineIndex == 0, 15, "boss intro dialogue");
        Check(boss.Movement.HasArrived && !boss.Ready && boss.Health.IsInvulnerable, "Dialogue starts after arrival with the boss unhittable");
        var lines = runner.Sequence.Lines;
        var cast = runner.Sequence.Cast;
        for (int i = 0; i < lines.Length; i++)
        {
            yield return Until(() => runner.LineIndex == i && runner.VisibleCharacters > 0, 10, "boss line " + i);
            var speaker = cast.Find(lines[i].speakerId);
            var slot = speaker.side == DialogueCast.Side.Left ? runner.LeftPortrait : runner.RightPortrait;
            Check(slot.enabled && slot.sprite == DialogueCast.Portrait(speaker, lines[i].expressionId) && runner.NameText.text == speaker.displayName,
                $"Boss line {i + 1}: {speaker.displayName} / {lines[i].expressionId}");
            yield return Press(Key.Enter);
            Check(runner.LineIndex == i && runner.AwaitingAdvance, $"Boss line {i + 1}: reveal keeps the same line");
            yield return Press(Key.Enter);
            Check(runner.LineIndex != i + 2, $"Boss line {i + 1}: one press never skips two lines");
        }
        Check(phaseOneAttacks == 0 && combat.ThreatRoot == null, "Boss never attacks during the conversation");
        Check(music.Current == GusiniMusic.Track.None, "Phase 1 music waits for combat");
        yield return Until(() => !runner.IsPlaying, 5, "boss intro close");
        yield return Until(() => boss.Ready, 5, "combat release");
        Check(!boss.CombatHeld && player.Movement.enabled, "Combat and controls released after the conversation");
        yield return Until(() => music.Current == GusiniMusic.Track.PhaseOne && music.Source.isPlaying, 3, "phase 1 music");
        yield return Until(() => phaseOneAttacks > 0, 5, "first phase 1 attack");
        Check(cutscenes.Playing == RunCutscenes.Scene.None, "Boss intro does not repeat");

        boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth + 1f);
        Check(phase.CurrentStage == GusiniPhaseTwo.Stage.Transforming, "Phase 1 depletion starts the existing transformation");
        Check(cutscenes.Finale == RunCutscenes.FinaleStage.None && !boss.RetainedAfterDefeat, "Phase 1 depletion never starts the finale");
        yield return Until(() => cutscenes.Playing == RunCutscenes.Scene.PhaseTransition, 5, "false defeat freeze");
        float frozenAt = phase.TransformationTime;
        Check(Time.timeScale == 0f && frozenAt >= .6f && frozenAt < 1.2f, $"Dialogue holds the native apparent-defeat frame ({frozenAt:0.000}s)");
        Check(phase.RevealTime < 0f && phaseTwoAttacks == 0, "Transition dialogue comes before the corruption reveal");
        pause.PauseGame();
        Check(pause.IsPaused, "Pause still opens over the frozen cutscene");
        pause.ResumeGame();
        yield return Frames(2);
        Check(Time.timeScale == 0f && !pause.IsPaused, "Resuming returns to the frozen cutscene");
        yield return Until(() => runner.LineIndex == 0 && runner.VisibleCharacters > 0, 10, "transition line");
        Check(runner.RightPortrait.sprite.name == "gusini_p1_smug" && runner.NameText.text == "Gusini", "Gusini tired smirk portrait");
        Check(music.Current == GusiniMusic.Track.None || music.Source.volume < .05f, "Phase 1 music silenced for the false defeat");
        var transition = runner.Sequence.Lines;
        for (int i = 0; i < transition.Length - 1; i++)
        {
            yield return Until(() => runner.LineIndex == i && runner.VisibleCharacters > 0, 10, "transition line " + i);
            yield return Press(Key.Enter);
            yield return Press(Key.Enter);
        }
        bool glimpse = false;
        double limit = EditorApplication.timeSinceStartup + 5;
        while (runner.LineIndex != transition.Length - 1 || runner.VisibleCharacters == 0)
        {
            glimpse |= runner.RightPortrait.sprite != null && runner.RightPortrait.sprite.name == "gusini_p2_corrupted_furious";
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("outburst line");
            yield return null;
        }
        Check(glimpse, "Core pulse interference flashes the approved corrupted portrait");
        Check(phase.TransformationTime == frozenAt, "Transformation clock stays frozen during the dialogue");
        yield return HoldSkip(.7f);
        Check(!runner.IsPlaying && Time.timeScale == 1f && cutscenes.Playing == RunCutscenes.Scene.None, "Skip closes the dialogue and unfreezes the game");
        Check(!runner.Blips.GetComponentsInChildren<AudioSource>().Any(s => s.isPlaying && s.clip != null && s.clip.length < .2f), "Skip stops typing sounds");
        yield return Until(() => phase.RevealTime >= 0f, 5, "phase 2 reveal");
        yield return Until(() => phase.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo, 5, "phase 2 start");
        Check(phase.TransformationTime > 4f, "Native transformation completes after the dialogue");
        yield return Until(() => music.Current == GusiniMusic.Track.PhaseTwo && music.Source.isPlaying, 3, "phase 2 music");
        int musicVoices = Object.FindObjectsByType<AudioSource>().Count(s => s.isPlaying && s.clip != null && s.clip.length > 60f);
        Check(musicVoices == 1 && music.Source.clip.name == "MUS_Gusini_Phase2_Loop", "Exactly one music track plays in phase 2");
        Check(player.Movement.enabled && !boss.Health.IsDead && boss.Health.MaxHealth == 320, "Phase 2 starts with original HP and released controls");
        yield return Until(() => phaseTwoAttacks > 0, 5, "phase 2 attack");
        Check(cutscenes.Playing == RunCutscenes.Scene.None, "Transition dialogue does not repeat in phase 2");
        yield return Finale(waves, cutscenes, runner, player, boss, phase, music, () => phaseTwoAttacks);
    }

    private static IEnumerator Finale(WaveController waves, RunCutscenes cutscenes, DialogueRunner runner, Player player,
        Boss boss, GusiniPhaseTwo phase, GusiniMusic music, Func<int> phaseTwoAttacks)
    {
        var defeat = boss.GetComponent<GusiniDefeat>();
        var flow = Find<ChapterFlow>();
        var visual = boss.transform.Find("Visual").GetComponent<SpriteRenderer>();
        int runEnded = 0, recorded = 0;
        waves.RunEnded += _ => runEnded++;
        CampaignProgress.FlagRecorded += _ => recorded++;
        var stages = new System.Collections.Generic.List<RunCutscenes.FinaleStage>();
        cutscenes.FinaleStageChanged += stage => stages.Add(stage);
        string scene = SceneManager.GetActiveScene().path;
        Check(!CampaignProgress.Has("gusini_rescued"), "Isolated campaign progress starts empty");

        boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth + 1f);
        int attacksAtDefeat = phaseTwoAttacks();
        Check(phase.CurrentStage == GusiniPhaseTwo.Stage.Defeated && defeat.Current == GusiniDefeat.Stage.Collapsing, "Phase 2 depletion starts the native parasite defeat");
        Check(boss.RetainedAfterDefeat && waves.Phase == WaveController.RunPhase.BossDefeated && phase.ThreatRoot == null, "Gusini stays in scene; combat and threats stop");
        Check(!player.Movement.enabled && !player.Combat.enabled && !player.Dash.enabled, "Player offense and movement locked for the finale");
        Check(!boss.Health.TakeDamage(10f), "Defeated boss takes no further damage");
        Check(cutscenes.Finale == RunCutscenes.FinaleStage.AlienRecovery && !runner.IsPlaying, "Finale waits for the recovery animation");
        yield return Until(() => defeat.ParasiteDestroyed, 3, "parasite burst");
        Check(!runner.IsPlaying && !defeat.Recovered, "Dialogue does not start at the burst");
        yield return Until(() => defeat.Recovered, 3, "recovery");
        yield return Frames(2);
        Check(visual.sprite.name.StartsWith("gusini_p1_defeated_idle", StringComparison.Ordinal), $"Recovered damaged-ship loop holds after the clip ({visual.sprite.name})");
        Check(visual.color.a > .99f, "Recovered Gusini is not faded out like the old death");
        yield return Until(() => runner.IsPlaying && runner.LineIndex == 0 && runner.VisibleCharacters > 0, 5, "finale dialogue");
        Check(cutscenes.Finale == RunCutscenes.FinaleStage.FinalDialogue, "Final dialogue after recovery");
        Check(music.Current == GusiniMusic.Track.None, "Phase 2 music ended before the conversation");
        var lines = runner.Sequence.Lines;
        var cast = runner.Sequence.Cast;
        for (int i = 0; i < 18; i++)
        {
            yield return Until(() => runner.LineIndex == i && runner.VisibleCharacters > 0, 10, "finale line " + i);
            var speaker = cast.Find(lines[i].speakerId);
            if (speaker.side == DialogueCast.Side.None)
                Check(runner.NameText.text == "RÁDIO" && runner.LeftPortrait.color != Color.white && runner.RightPortrait.color != Color.white,
                    $"Finale line {i + 1}: RÁDIO without a human portrait");
            else
            {
                var slot = speaker.side == DialogueCast.Side.Left ? runner.LeftPortrait : runner.RightPortrait;
                Check(slot.sprite == DialogueCast.Portrait(speaker, lines[i].expressionId) && runner.NameText.text == speaker.displayName,
                    $"Finale line {i + 1}: {speaker.displayName} / {lines[i].expressionId}");
            }
            Check(runner.RightPortrait.sprite == null || !runner.RightPortrait.sprite.name.StartsWith("gusini_p2", StringComparison.Ordinal), $"Finale line {i + 1}: no corrupted portrait");
            var expected = i >= 15 ? RunCutscenes.FinaleStage.Farewell : i >= 8 ? RunCutscenes.FinaleStage.RadioTransmission : RunCutscenes.FinaleStage.FinalDialogue;
            Check(cutscenes.Finale == expected, $"Finale line {i + 1}: stage {expected}");
            if (i % 2 == 0) { yield return Press(Key.Enter); yield return Press(Key.Enter); }
            else { yield return Click(); yield return Click(); }
            Check(runner.LineIndex != i + 2, $"Finale line {i + 1}: one input per step");
        }
        yield return Until(() => runner.LineIndex == 18 && runner.VisibleCharacters > 0, 10, "finale line 18");
        yield return HoldSkip(.7f);
        Check(!runner.IsPlaying && waves.Phase == WaveController.RunPhase.ChapterCompleted && flow.Completed, "Skip reaches the same chapter completion");
        Check(runEnded == 0, "Old RunEnded/victory flow never fires");
        Check(recorded == 2 && CampaignProgress.Has("gusini_rescued") && CampaignProgress.Has("chapter1_completed"), "Chapter flags recorded once");
        Check(!CampaignProgress.Record("gusini_rescued"), "Recording a flag twice is ignored");
        Check(stages.SequenceEqual(new[] { RunCutscenes.FinaleStage.AlienRecovery, RunCutscenes.FinaleStage.FinalDialogue, RunCutscenes.FinaleStage.RadioTransmission,
            RunCutscenes.FinaleStage.Farewell, RunCutscenes.FinaleStage.SceneTransition }), "Finale stages in order: " + string.Join(" > ", stages));
        Check(flow.Transition.Current == SceneTransition.State.FadingOut && flow.Transition.Destination == "BaseImprovisada", "Fade-out toward the configured hub");
        Check(!flow.Transition.Go("BaseImprovisada", "x"), "Transition cannot start twice");
        yield return Until(() => flow.Transition.Current == SceneTransition.State.DestinationUnavailable, 5, "development fallback");
        yield return Realtime(1.2f);
        Check(SceneManager.GetActiveScene().path == scene && !SceneTransition.IsAvailable("BaseImprovisada"), "Missing hub scene is not loaded and causes no exception");
        Check(waves.Phase == WaveController.RunPhase.ChapterCompleted && phaseTwoAttacks() == attacksAtDefeat && runEnded == 0, "No new wave, attack or victory after the finale");
        Check(boss != null && Time.timeScale == 1f && !player.Movement.enabled, "Gusini kept, time normal, combat not resumed");
    }

    private static IEnumerator SkippedIntro()
    {
        var waves = Find<WaveController>();
        var cutscenes = Find<RunCutscenes>();
        var runner = cutscenes.Dialogue;
        var player = Find<Player>();
        waves.StartRun();
        yield return Until(() => runner.LineIndex == 0 && runner.VisibleCharacters > 2, 10, "fresh intro");
        yield return HoldSkip(.7f);
        Check(!runner.IsPlaying && waves.Phase == WaveController.RunPhase.Announcing && waves.CurrentWave == 1, "Skipping the intro reaches wave 1 like the normal ending");
        Check(player.Movement.enabled && player.Combat.enabled, "Skip restores controls");
    }
}
