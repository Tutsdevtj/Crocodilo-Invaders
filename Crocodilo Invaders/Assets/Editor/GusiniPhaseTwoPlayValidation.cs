using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Exercita o PlayerLoop nativo, Animation Events, Physics2D e pausa sem salvar ranking.
[InitializeOnLoad]
public static class GusiniPhaseTwoPlayValidation
{
    private const string Session = "Crocodilo.GusiniP2.Validation";
    private static IEnumerator scenario;
    private static double deadline;
    private static int checks, errors;
    private static readonly Dictionary<string, List<float>> releases = new Dictionary<string, List<float>>();
    private static readonly List<GusiniPhaseTwo.Attack> attacks = new List<GusiniPhaseTwo.Attack>();
    private static readonly List<Vector3> eggBurstPositions = new List<Vector3>();
    private static readonly HashSet<GusiniAcidYolk> eggYolks = new HashSet<GusiniAcidYolk>();
    private static readonly List<float> eggYolkSpeeds = new List<float>();
    private static Boss boss;
    private static Health target;
    private static GusiniPhaseTwo phase;
    private static Camera camera;
    private static BossBarFX bar;
    private static int defeated;
    private static int lowHpDiversSpawned;
    private static int highHpDiversSpawned;
    private sealed class AudioCue { public GusiniSound id; public float time, volume; public AudioClip clip; }
    private static readonly List<AudioCue> audioCues = new List<AudioCue>();
    private static bool AudioChecks => SessionState.GetBool(Session + ".Audio", false);

    static GusiniPhaseTwoPlayValidation()
    {
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Session, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }
    [MenuItem("Crocodilo/Boss/Validate Phase 2 in Play Mode")]
    public static void Run()
        => StartValidation(false, false);
    public static void RunHudRebind()
        => StartValidation(true, false);
    [MenuItem("Crocodilo/Boss/Validate Phase 2 audio in Play Mode")]
    public static void RunAudio()
        => StartValidation(false, true);
    private static void StartValidation(bool hudOnly, bool audioChecks)
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        GusiniPhaseTwoTools.ValidateIntegration();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Session, true);
        SessionState.SetBool(Session + ".HudOnly", hudOnly);
        SessionState.SetBool(Session + ".Audio", audioChecks);
        EditorApplication.isPlaying = true;
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
        checks = errors = defeated = lowHpDiversSpawned = highHpDiversSpawned = 0; releases.Clear(); attacks.Clear(); audioCues.Clear();
        eggBurstPositions.Clear(); eggYolks.Clear(); eggYolkSpeeds.Clear();
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 150;
        scenario = SessionState.GetBool(Session + ".HudOnly", false) ? HudScenario() : Scenario(); EditorApplication.update += Tick;
    }
    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Native play validation timed out");
            if (scenario.MoveNext()) return;
            Debug.Log("GUSINI PHASE TWO PLAY VALIDATION PASSED: " + checks + " assertions; " + (SessionState.GetBool(Session + ".HudOnly", false) ? "HUD rebind and preserved phase state." : "transformation, native releases, hazards, weakpoint, pause and cleanup."));
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        scenario = null; Time.timeScale = 1f; AudioListener.pause = false; EditorApplication.isPlaying = false;
    }
    private static void OnLog(string message, string trace, LogType type)
    {
        // Unity 6000.6 can fail in its startup search index; this is outside the player simulation.
        // Matches the filter already used by the card and jungle Play Mode validators.
        if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("P2 CHECK FAILED: " + message);
        checks++; Debug.Log("P2 CHECK " + checks + ": " + message);
    }
    private static IEnumerator Scenario()
    {
        var cameraRoot = new GameObject("Validation camera"); cameraRoot.tag = "MainCamera";
        camera = cameraRoot.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.8f;
        camera.aspect = 1200f / 720f; camera.transform.position = new Vector3(0f, 1.8f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.22f, .55f, .75f);
        cameraRoot.AddComponent<AudioListener>();
        var sky = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/BattleSky.prefab");
        Object.Instantiate(sky);
        var targetRoot = new GameObject("Isolated target (no run or ranking)"); targetRoot.layer = 8;
        targetRoot.transform.position = new Vector3(-5f, 1.5f, 0);
        target = targetRoot.AddComponent<Health>(); target.SetInvulnerable(true);
        targetRoot.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
        var targetBody = targetRoot.AddComponent<Rigidbody2D>(); targetBody.bodyType = RigidbodyType2D.Kinematic;
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var targetVisual = targetRoot.AddComponent<SpriteRenderer>();
        targetVisual.sprite = playerPrefab.GetComponentInChildren<SpriteRenderer>(true).sprite;
        targetVisual.sortingOrder = 3; targetRoot.transform.localScale = Vector3.one * 2f;
        boss = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab"), new Vector3(6f, 2f, 0), Quaternion.identity).GetComponent<Boss>();
        phase = boss.GetComponent<GusiniPhaseTwo>();
        var bossAudio = boss.GetComponent<GusiniAudio>();
        if (AudioChecks) bossAudio.SoundPlayed += (id, clip, gain) => audioCues.Add(new AudioCue { id = id, time = Time.time, clip = clip, volume = gain });
        boss.Defeated += _ => defeated++;
        phase.AttackStarted += attack => attacks.Add(attack);
        phase.Released += (kind, entity) =>
        {
            if (!releases.ContainsKey(kind)) releases[kind] = new List<float>();
            releases[kind].Add(Time.time);
            if (kind == "egg_burst")
            {
                eggBurstPositions.Add(entity.transform.position);
                foreach (var yolk in phase.ThreatRoot.GetComponentsInChildren<GusiniAcidYolk>())
                    if (eggYolks.Add(yolk)) eggYolkSpeeds.Add(yolk.GetComponent<Rigidbody2D>().linearVelocity.magnitude / (Mathf.Abs(yolk.transform.lossyScale.x) / 100f));
            }
            if (kind == "goose" && (bool)typeof(GusiniCorruptedGoose).GetField("diver", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entity.GetComponent<GusiniCorruptedGoose>()))
            {
                if (boss.Health.Fraction < .6f) lowHpDiversSpawned++;
                else highHpDiversSpawned++;
            }
        };
        var hudRoot = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab"));
        var canvas = hudRoot.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f;
        typeof(BossHUD).GetMethod("Bind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(hudRoot.GetComponent<BossHUD>(), new object[] { boss });
        bar = hudRoot.GetComponentInChildren<BossBarFX>(true);
        boss.BeginBattle(target);
        float until = Time.time + 6f;
        while ((boss.GetComponent<GusiniCombat>().ThreatRoot == null) && Time.time < until) yield return null;
        Check(boss.Ready && boss.GetComponent<GusiniCombat>().ThreatRoot != null, "F1 enters and releases through its existing controller");
        float transformStart = Time.time;
        boss.Health.TakeDamage(10000);
        Check(phase.CurrentStage == GusiniPhaseTwo.Stage.Transforming && defeated == 0, "F1 depletion starts transformation without victory");
        Check(boss.Health.MaxHealth == 320 && !boss.Health.IsDead && boss.Health.IsInvulnerable, "Next phase owns an invulnerable fresh health pool");
        Check(boss.GetComponent<GusiniCombat>().ThreatRoot == null, "F1 threats cleared immediately");
        Check(Fill().fillAmount == 0f, "F1 bar remains empty during apparent defeat");
        Check(!boss.Health.TakeDamage(5), "Transformation rejects incoming shots");
        float pauseAt = Time.time + .4f;
        while (Time.time < pauseAt) yield return null;
        if (AudioChecks)
        {
            var sources = bossAudio.GetComponentsInChildren<AudioSource>();
            Check(sources.Length == 10 && sources.All(s => s.outputAudioMixerGroup != null && !s.ignoreListenerPause && s.spatialBlend == 0f), "F2 uses the shared ten-source Effects mixer pool");
        }
        Time.timeScale = 0f;
        AudioListener.pause = true;
        yield return null;
        float frozenTime = phase.TransformationTime;
        var pausedSource = AudioChecks ? bossAudio.GetComponentsInChildren<AudioSource>().FirstOrDefault(s => s.clip != null && s.clip.name.Contains("EngineStall")) : null;
        int pausedSample = pausedSource != null ? pausedSource.timeSamples : 0;
        Sprite frozenSprite = boss.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite;
        double realUntil = EditorApplication.timeSinceStartup + .25;
        while (EditorApplication.timeSinceStartup < realUntil) yield return null;
        Check(Mathf.Abs(phase.TransformationTime - frozenTime) < .001f && frozenSprite == boss.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite, "Pause freezes native transform clip and phase clock");
        if (AudioChecks) Check(AudioListener.pause && pausedSource != null && Mathf.Abs(pausedSource.timeSamples - pausedSample) <= 2048, "Pause freezes the native source sample clock without restarting the clip");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        until = Time.time + 6f;
        while (phase.CurrentStage != GusiniPhaseTwo.Stage.PhaseTwo && Time.time < until) yield return null;
        Check(phase.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo && Time.time - transformStart >= 4.09f, "AI waits for the entire 4.1s clip");
        Check(!boss.Health.IsInvulnerable && boss.GetComponent<Animator>().speed == 1f, "F2 restores combat with native animation speed");
        Check(Fill().sprite.name == "hp_fill" && AssetDatabase.GetAssetPath(Fill().sprite).Contains("GusiniBossBarF2"), "Shared HUD switches to corrupted artwork");
        Check(Fill().fillOrigin == 1 && Fill().fillAmount > .98f, "F2 health recharges with right origin and no slide");
        if (AudioChecks)
        {
            var ids = new[] { GusiniSound.P2FinalHit, GusiniSound.P2EngineStall, GusiniSound.P2FirstPulse, GusiniSound.P2CorruptionSurge, GusiniSound.P2MutationPeak, GusiniSound.P2RevealCry };
            float[] expected = { 0f, .16f, 1.33f, 1.51f, 2.53f, 3.23f };
            for (int i = 0; i < ids.Length; i++)
                Check(Cues(ids[i]).Count() == 1 && Mathf.Abs(Cues(ids[i]).Single().time - transformStart - expected[i]) < .09f, "Native transformation sound timing: " + ids[i]);
            Check(!audioCues.Any(c => c.time - transformStart > .69f && c.time - transformStart < 1.32f), "No sound is triggered inside the apparent-defeat silence");
            Check(Cues(GusiniSound.P2EngineStall).Single().time + Cues(GusiniSound.P2EngineStall).Single().clip.length < transformStart + .69f, "Engine stall tail ends before the silent hold");
            Check(!bossAudio.GetComponentsInChildren<AudioSource>().Any(s => s.loop && s.clip != null), "F2 never inherits the healthy F1 engine loop");
        }
        Capture("phase2-idle", 1200, 720);
        until = Time.time + 4f;
        while (Count("energy") < 3 && Time.time < until) yield return null;
        Check(Count("energy") == 3 && attacks[0] == GusiniPhaseTwo.Attack.Energy, "First F2 attack releases exactly three balls");
        Check(Mathf.Abs(releases["energy"][1] - releases["energy"][0] - .3f) < .06f && Mathf.Abs(releases["energy"][2] - releases["energy"][1] - .3f) < .06f, "Energy releases retain 300ms spacing");
        if (AudioChecks)
        {
            Check(Cues(GusiniSound.P2EnergyCharge).Count() == 1 && Cues(GusiniSound.P2EnergyFire).Count() == 3, "One charge and one sound per energy projectile, with no F1 cannon layer");
            for (int i = 0; i < 3; i++) Check(Mathf.Abs(Cues(GusiniSound.P2EnergyFire).ElementAt(i).time - releases["energy"][i]) < .02f, "Fire sound matches actual release " + i);
        }
        Capture("phase2-energy", 1200, 720);
        until = Time.time + 6f;
        while (Count("egg") == 0 && Time.time < until) yield return null;
        var egg = Object.FindFirstObjectByType<GusiniCorruptedEgg>();
        Check(egg != null && egg.GetComponent<Health>() == null && egg.GetComponent<Collider2D>() == null, "Corrupted egg is indestructible");
        until = Time.time + 5f;
        while (Count("goose") < 5 && Time.time < until) yield return null;
        var geese = Object.FindObjectsByType<GusiniCorruptedGoose>(FindObjectsSortMode.None);
        Check(geese.Length == 5 && Count("warning") == 5, "Each of five geese receives an advance edge marker");
        Check(highHpDiversSpawned == 2, "The first squadron already has two divers above 60 percent health");
        Check(Mathf.Approximately(boss.Movement.PatrolSpeedMultiplier, 1.5f), "Phase two uses the faster patrol without changing phase one");
        Check(geese.All(g => g.GetComponent<Health>().MaxHealth == 2), "Geese take two unbuffed shots to defeat");
        // Abate o líder; os dois mergulhadores precisam sobreviver para os checks de áudio.
        Health gooseHealth = geese.First(g => g.name == "Corrupted goose 0").GetComponent<Health>(); gooseHealth.TakeDamage(1);
        Check(gooseHealth.CurrentHealth == 1 && !gooseHealth.IsDead, "First goose hit survives"); gooseHealth.TakeDamage(1);
        yield return null;
        Check(gooseHealth == null || !gooseHealth.gameObject.activeInHierarchy, "Second goose hit removes it");
        if (AudioChecks) Check(Cues(GusiniSound.P2GoosePoof).Count() == 1, "Goose defeat releases the new poof once");
        until = Time.time + 5f;
        while (Count("egg_burst") < 3 && Time.time < until) yield return null;
        Check(Count("egg") == 3 && Count("egg_burst") == 3 && eggYolks.Count == 15, "Three separate eggs each release five acid yolks");
        Check(eggYolks.Where(y => y != null).All(y => y.transform.rotation == Quaternion.identity), "Pre-oriented yolk sprites remain unrotated");
        Check(eggYolkSpeeds.All(speed => Mathf.Abs(speed - 420f) < .1f), "Every egg projectile launches at 420 pixels per second");
        Check(eggBurstPositions.All(p => phase.Arena.Contains(p)), "Every egg bursts inside the visible arena");
        Check(Vector3.Distance(eggBurstPositions[0], eggBurstPositions[1]) >= 2.15f && Vector3.Distance(eggBurstPositions[0], eggBurstPositions[2]) >= 2.15f && Vector3.Distance(eggBurstPositions[1], eggBurstPositions[2]) >= 2.15f, "Random burst points remain spatially separated");
        Check(releases["egg_burst"][1] - releases["egg_burst"][0] >= .4f && releases["egg_burst"][2] - releases["egg_burst"][1] >= .4f, "Eggs burst individually instead of simultaneously");
        if (AudioChecks)
        {
            float eggStart = releases["egg"][0];
            Check(Cues(GusiniSound.P2ParachutePop).Count() == 1 && Mathf.Abs(Cues(GusiniSound.P2ParachutePop).First().time - eggStart - .25f) < .06f, "Simultaneous deploy keeps one shared parachute POP");
            Check(Cues(GusiniSound.P2EggPulse).Count() >= 4 && Cues(GusiniSound.P2EggPulse).Count() <= 12, "Independent egg pulse clocks retain the shared voice limits");
            Check(Cues(GusiniSound.P2EggCrack).Count() >= 2 && Cues(GusiniSound.P2EggCrack).Count() <= 6, "Independent cracks retain the shared voice limits");
            Check(Cues(GusiniSound.P2EggBurst).Any() && Cues(GusiniSound.P2EggBurst).All(c => releases["egg_burst"].Any(t => Mathf.Abs(t - c.time) < .02f)), "Egg burst audio follows actual staggered explosions");
        }
        until = Time.time + 6f;
        while (Count("lane_warning") == 0 && Time.time < until) yield return null;
        Check(Count("lane_warning") == 2, "Two marked lanes leave exactly one safe lane");
        Capture("phase2-tentacle-warning", 1200, 720);
        until = Time.time + 2f;
        while (Count("lane") == 0 && Time.time < until) yield return null;
        Capture("phase2-tentacle-lash", 1200, 720);
        until = Time.time + 7f;
        while (!phase.WeakpointOpen && Time.time < until) yield return null;
        Check(phase.WeakpointOpen && Count("lane") == 6 && Count("lane_warning") == 6, "Three complete strikes precede resistance");
        float hp = boss.Health.PreciseCurrentHealth;
        boss.Health.TakeDamage(1.05f);
        Check(Mathf.Abs(hp - boss.Health.PreciseCurrentHealth - 2.1f) < .001f, "Exposed core doubles precise fractional damage");
        float weakStart = Time.time;
        Capture("phase2-weakpoint", 1200, 720);
        until = Time.time + 3f;
        while (phase.WeakpointOpen && Time.time < until) yield return null;
        Check(Time.time - weakStart >= 1.9f && boss.Health.IncomingDamageMultiplier == 1f, "Core window lasts 2s then restores normal damage");
        if (AudioChecks)
        {
            Check(Cues(GusiniSound.P2CorruptCall).Count() == 1 && Cues(GusiniSound.P2EdgeMark).Count() == 1, "Corrupted call and one common marker cue replace the F1 squadron sounds");
            // O líder abatido imediatamente pode não chegar ao OnQuack; os demais não formam coral.
            Check(Cues(GusiniSound.P2HonkMid).Count() <= 1 && !Cues(GusiniSound.P2HonkLow).Any() && !Cues(GusiniSound.P2HonkHigh).Any(), "No restored five-voice goose chorus");
            Check(Cues(GusiniSound.P2ModuleCrack).Count() == 1 && Cues(GusiniSound.P2TentacleEmerge).Count() == 1, "One module crack and emerge per tentacle attack");
            Check(Cues(GusiniSound.P2LaneWarning).Count() == 3 && Cues(GusiniSound.P2Lash).Count() == 3 && Cues(GusiniSound.P2TipCrack).Count() == 3, "Three warning/lash/tip cues, shared across the simultaneous two lanes");
            for (int i = 0; i < 3; i++)
            {
                Check(Mathf.Abs(Cues(GusiniSound.P2Lash).ElementAt(i).time - Cues(GusiniSound.P2LaneWarning).ElementAt(i).time - .9f) < .07f, "900ms warning before lash " + i);
                Check(Mathf.Abs(Cues(GusiniSound.P2TipCrack).ElementAt(i).time - Cues(GusiniSound.P2Lash).ElementAt(i).time - .16f) < .06f, "Tip crack uses lane animation clock " + i);
            }
            // O close é Animation Event: aguarda seu primeiro frame antes de conferir.
            until = Time.time + .1f; while (!Cues(GusiniSound.P2WeakpointClose).Any() && Time.time < until) yield return null;
            Check(Cues(GusiniSound.P2WeakpointOpen).Count() == 1 && Cues(GusiniSound.P2WeakpointClose).Count() == 1, "Weakpoint loop triggers open only once and closes once");
            Check(Mathf.Abs(Cues(GusiniSound.P2WeakpointClose).Single().time - Cues(GusiniSound.P2WeakpointOpen).Single().time - 2f) < .07f, "Weakpoint audio preserves the full 2000ms exposure");
        }
        hp = boss.Health.PreciseCurrentHealth; boss.Health.TakeDamage(1.05f);
        Check(Mathf.Abs(hp - boss.Health.PreciseCurrentHealth - 1.05f) < .001f, "Normal damage remains fractional after resistance");
        Capture("phase2-hud-1080", 1920, 1080); Capture("phase2-hud-4by3", 800, 600);
        boss.Health.TakeDamage(boss.Health.MaxHealth * .65f);
        until = Time.time + 24f;
        while ((attacks.Count < 8 || attacks.Skip(4).Take(3).Contains(GusiniPhaseTwo.Attack.Squadron) == false) && Time.time < until) yield return null;
        Check(attacks.Count >= 8 && attacks[3] == GusiniPhaseTwo.Attack.Tentacles && attacks[7] == GusiniPhaseTwo.Attack.Tentacles, "One tentacle attack per cycle of the three ordinary attacks");
        Check(attacks.Skip(4).Take(3).Distinct().Count() == 3, "Subsequent bag includes every ordinary attack once");
        // Deixa os gansinhos do segundo ciclo chegar ao aviso do mergulho.
        Check(lowHpDiversSpawned == 2, "Below 60 percent, precisely the two designated slots become divers");
        var diverData = AssetDatabase.LoadAssetAtPath<GusiniPhaseTwoAssets>("Assets/Data/GusiniPhaseTwo.asset");
        var isolatedDiver = GusiniCorruptedGoose.Create(diverData, phase.ThreatRoot, new Vector3(3.6f, 5f, 0), .02f, 1, target, true, phase.Arena);
        var secondGeese = new[] { isolatedDiver };
        until = Time.time + 4f;
        var diverField = typeof(GusiniCorruptedGoose).GetField("diver", BindingFlags.NonPublic | BindingFlags.Instance);
        var divingField = typeof(GusiniCorruptedGoose).GetField("diving", BindingFlags.NonPublic | BindingFlags.Instance);
        var directionField = typeof(GusiniCorruptedGoose).GetField("diveDirection", BindingFlags.NonPublic | BindingFlags.Instance);
        while (!secondGeese.Any(g => g != null && (bool)divingField.GetValue(g)) && Time.time < until) yield return null;
        var diver = secondGeese.FirstOrDefault(g => g != null && (bool)diverField.GetValue(g) && (bool)divingField.GetValue(g));
        Check(diver != null, "Below 60 percent, a designated goose enters its straight dive");
        Vector2 lockedDirection = (Vector2)directionField.GetValue(diver);
        Vector2 diveStart = diver.GetComponent<Rigidbody2D>().position;
        float diveStartTime = Time.fixedTime;
        target.transform.position += Vector3.up * 2f;
        float diveCheckAt = Time.time + .15f;
        while (Time.time < diveCheckAt) yield return null;
        Check(diver != null && Vector2.Distance(lockedDirection, (Vector2)directionField.GetValue(diver)) < .001f, "Diver keeps its locked direction when target moves");
        float measuredDiveSpeed = Vector2.Distance(diveStart, diver.GetComponent<Rigidbody2D>().position) / (Time.fixedTime - diveStartTime);
        Check(Mathf.Abs(measuredDiveSpeed - 12f) < .2f, "Diver travels at 600 pixels per second at the test scale");
        if (AudioChecks)
        {
            until = Time.time + 4f; while (Cues(GusiniSound.P2DiveWhoosh).Count() < 4 && Time.time < until) yield return null;
            Check(Cues(GusiniSound.P2DiverWarning).Count() == 4 && Cues(GusiniSound.P2DiveWhoosh).Count() == 4, "Both squadrons emit warnings and launches for their two divers");
            for (int i = 0; i < 4; i++) Check(Mathf.Abs(Cues(GusiniSound.P2DiveWhoosh).ElementAt(i).time - Cues(GusiniSound.P2DiverWarning).ElementAt(i).time - .4f) < .07f, "400ms warning before diver " + i);
            Check(Cues(GusiniSound.P2EnergyPop).Any() && audioCues.Where(c => c.id >= GusiniSound.P2FinalHit).All(c => c.volume <= .5f), "Projectile pops use the new master and F2 per-sound gain remains limited");
            Check(phase.ThreatRoot.GetComponentsInChildren<AudioSource>(true).Length == 0, "Threat entities add no independent audio sources");
        }
        boss.Health.TakeDamage(10000);
        Check(defeated == 1 && boss.Health.IsDead, "Only F2 depletion produces final defeat once");
        // Desde o encerramento do capítulo 1, a derrota da F2 passa para GusiniDefeat (parasita morre, Gusini fica).
        Check(phase.ThreatRoot == null && phase.CurrentStage == GusiniPhaseTwo.Stage.Defeated && boss.RetainedAfterDefeat, "Final defeat clears all F2 threats immediately and hands over to the parasite defeat");
        Check(Mathf.Approximately(boss.Movement.PatrolSpeedMultiplier, 1f), "Ending phase two clears its movement multiplier");
        if (AudioChecks) Check(bossAudio.ActiveVoiceCount == 0, "Boss defeat stops all F2 audio immediately");
        yield return null;
        Check(Object.FindObjectsByType<GusiniEnergyBall>(FindObjectsSortMode.None).Length == 0 && Object.FindObjectsByType<GusiniCorruptedGoose>(FindObjectsSortMode.None).Length == 0, "No orphan projectiles or geese remain");
        // Cancelamento durante transformação, sem esperar pelo boss anterior sair.
        var other = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab"), new Vector3(6f, 2f, 0), Quaternion.identity).GetComponent<Boss>();
        other.BeginBattle(target);
        until = Time.time + 4f; while (!other.Ready && Time.time < until) yield return null;
        other.Health.TakeDamage(10000); var otherPhase = other.GetComponent<GusiniPhaseTwo>();
        Check(otherPhase.CurrentStage == GusiniPhaseTwo.Stage.Transforming, "Fresh boss starts its own transition");
        other.StopBattle();
        Check(otherPhase.CurrentStage == GusiniPhaseTwo.Stage.Stopped && otherPhase.ThreatRoot == null, "Battle cancellation interrupts transformation and cleans ownership");
        Object.Destroy(other.gameObject);
        // Colisão real: um golpe atravessado em i-frames não causa dano atrasado.
        target.transform.position = new Vector3(6f, 1.8f, 0f);
        target.SetInvulnerable(true);
        var data = AssetDatabase.LoadAssetAtPath<GusiniPhaseTwoAssets>("Assets/Data/GusiniPhaseTwo.asset");
        var holder = new GameObject("Isolated lane ownership");
        GusiniTentacleLane.Create(data, holder.transform, new Rect(-8f, -3f, 16f, 9.6f), 1.8f, false);
        float beforeHp = target.PreciseCurrentHealth;
        float wait = Time.time + .1f; while (Time.time < wait) yield return null;
        target.SetInvulnerable(false);
        wait = Time.time + .3f; while (Time.time < wait) yield return null;
        Check(target.PreciseCurrentHealth == beforeHp, "Tentacle i-frame contact cannot deal delayed damage on stay");
        Object.Destroy(holder);
        target.SetInvulnerable(true);
        // Morte do alvo durante F2 remove os ataques e impede novas liberações.
        var deathBoss = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab"), new Vector3(6f, 2f, 0), Quaternion.identity).GetComponent<Boss>();
        deathBoss.BeginBattle(target);
        until = Time.time + 4f; while (!deathBoss.Ready && Time.time < until) yield return null;
        deathBoss.Health.TakeDamage(10000);
        var deathPhase = deathBoss.GetComponent<GusiniPhaseTwo>();
        until = Time.time + 7f; while (deathPhase.ThreatRoot == null && Time.time < until) yield return null;
        Check(deathPhase.ThreatRoot != null, "Second active F2 owns live threats");
        target.SetInvulnerable(false); target.TakeDamage(10000);
        Check(deathPhase.CurrentStage == GusiniPhaseTwo.Stage.Stopped && deathPhase.ThreatRoot == null, "Player death cleans active F2 threats and cancels coroutines");
        if (AudioChecks) Check(deathBoss.GetComponent<GusiniAudio>().ActiveVoiceCount == 0, "Player death stops transformation and entity audio voices");
        Object.Destroy(deathBoss.gameObject);
        yield return null;
        // Unload real de cena durante transformação (sem objetos persistentes do boss).
        var unloadTarget = new GameObject("Unload target").AddComponent<Health>();
        unloadTarget.SetInvulnerable(true);
        var testScene = SceneManager.CreateScene("Gusini phase two unload validation");
        var unloadBoss = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab"), new Vector3(6f, 2f, 0), Quaternion.identity).GetComponent<Boss>();
        SceneManager.MoveGameObjectToScene(unloadBoss.gameObject, testScene);
        unloadBoss.BeginBattle(unloadTarget);
        until = Time.time + 4f; while (!unloadBoss.Ready && Time.time < until) yield return null;
        unloadBoss.Health.TakeDamage(10000);
        var unloadPhase = unloadBoss.GetComponent<GusiniPhaseTwo>();
        Check(unloadPhase.CurrentStage == GusiniPhaseTwo.Stage.Transforming, "Scene-owned boss can transition in an additive scene");
        var operation = SceneManager.UnloadSceneAsync(testScene);
        while (!operation.isDone) yield return null;
        // O boss derrotado no início do teste fica retido (Gusini resgatado); só o da cena descarregada deve sumir.
        Check(unloadPhase == null && Object.FindObjectsByType<GusiniPhaseTwo>(FindObjectsSortMode.None).All(p => p == phase), "Unloading scene destroys the boss without persistent ownership");
        Object.Destroy(unloadTarget.gameObject);
        Check(errors == 0, "No Unity errors during native simulation");
    }
    private static int Count(string kind) => releases.TryGetValue(kind, out var list) ? list.Count : 0;
    private static System.Collections.Generic.IEnumerable<AudioCue> Cues(GusiniSound id) => audioCues.Where(c => c.id == id);
    private static IEnumerator HudScenario()
    {
        var cameraRoot = new GameObject("HUD test camera"); cameraRoot.tag = "MainCamera";
        camera = cameraRoot.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.8f;
        camera.aspect = 1200f / 720f; camera.transform.position = new Vector3(0, 1.8f, -10);
        cameraRoot.AddComponent<AudioListener>();
        target = new GameObject("Isolated HUD target").AddComponent<Health>(); target.SetInvulnerable(true);
        target.transform.position = Vector3.left * 6f;
        boss = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab"), new Vector3(6f, 2f, 0), Quaternion.identity).GetComponent<Boss>();
        phase = boss.GetComponent<GusiniPhaseTwo>();
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab"));
        var hud = root.GetComponent<BossHUD>(); bar = root.GetComponentInChildren<BossBarFX>(true);
        var bind = typeof(BossHUD).GetMethod("Bind", BindingFlags.NonPublic | BindingFlags.Instance);
        bind.Invoke(hud, new object[] { boss }); boss.BeginBattle(target);
        float until = Time.time + 4f; while (!boss.Ready && Time.time < until) yield return null;
        boss.Health.TakeDamage(10000);
        until = Time.time + .5f; while (Time.time < until) yield return null;
        root.SetActive(false); root.SetActive(true); bind.Invoke(hud, new object[] { boss });
        until = Time.time + .05f; while (Time.time < until) yield return null;
        Check(Fill().fillAmount == 0f, "Rebinding during apparent defeat preserves empty F1 bar");
        until = Time.time + 5f; while (phase.CurrentStage != GusiniPhaseTwo.Stage.PhaseTwo && Time.time < until) yield return null;
        boss.Health.TakeDamage(20.5f);
        root.SetActive(false); root.SetActive(true); bind.Invoke(hud, new object[] { boss });
        until = Time.time + .05f; while (Time.time < until) yield return null;
        Check(AssetDatabase.GetAssetPath(Fill().sprite).Contains("GusiniBossBarF2"), "Rebinding active F2 restores the corrupted skin");
        Check(Mathf.Abs(Fill().fillAmount - boss.Health.Fraction) < .001f, "Rebinding F2 restores current precise HP without replaying recharge");
        var barGroup = (RectTransform)new SerializedObject(bar).FindProperty("barGroup").objectReferenceValue;
        Check(barGroup.anchoredPosition == Vector2.zero, "Rebinding F2 never replays the F1 slide");
        Check(phase.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo && !boss.Health.IsInvulnerable, "HUD lifecycle does not reset combat or health");
        boss.Health.TakeDamage(10000);
        root.SetActive(false); root.SetActive(true);
        Check(!bar.gameObject.activeSelf, "A dead boss is not reopened by HUD reactivation");
        Check(errors == 0, "HUD lifecycle completes without Unity errors");
    }
    private static Image Fill() => (Image)new SerializedObject(bar).FindProperty("healthFill").objectReferenceValue;
    private static void Capture(string name, int width, int height)
    {
        Directory.CreateDirectory("Logs/GusiniPhaseTwo");
        float aspect = camera.aspect; camera.aspect = width / (float)height;
        var texture = new RenderTexture(width, height, 24);
        camera.targetTexture = texture; Canvas.ForceUpdateCanvases(); camera.Render();
        var before = RenderTexture.active; RenderTexture.active = texture;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
        File.WriteAllBytes("Logs/GusiniPhaseTwo/" + name + ".png", pixels.EncodeToPNG());
        camera.targetTexture = null; camera.aspect = aspect; RenderTexture.active = before;
        Object.Destroy(pixels); Object.Destroy(texture);
    }
}
