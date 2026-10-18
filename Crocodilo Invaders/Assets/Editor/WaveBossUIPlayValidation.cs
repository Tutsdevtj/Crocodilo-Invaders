using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Play Mode real das apresentações: composição dos números, coreografia, alerta do boss, áudio,
// pausa, pulo/cancelamento, integração com WaveController/RunCutscenes e capturas em Logs/WaveBossUI.
// O ranking usa uma pasta temporária; nenhuma run é registrada.
[InitializeOnLoad]
public static class WaveBossUIPlayValidation
{
    private const string Session = "Crocodilo.WaveBossUI.Validation";
    private const string Gameplay = "Assets/Scenes/Tests/PlayerMovementTest.unity";
    private const string BossScene = "Assets/Scenes/Tests/GusiniBossTest.unity";
    private const string Output = "Logs/WaveBossUI";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static bool running;
    private static double deadline;
    private static int checks, errors;
    private static Keyboard keyboard;
    private static InputSettings.BackgroundBehavior originalBackground;
    private static InputSettings.EditorInputBehaviorInPlayMode originalEditorBehavior;
    private static readonly List<string> report = new List<string>();

    static WaveBossUIPlayValidation()
    {
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Session, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }

    [MenuItem("Crocodilo/UI/Validate announcements in Play Mode")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        WaveBossUITools.Validate();
        EditorSceneManager.OpenScene(Gameplay, OpenSceneMode.Single);
        SessionState.SetBool(Session, true);
        EditorApplication.isPlaying = true;
    }

    private static IEnumerator Scenario()
    {
        yield return Frames(3);
        yield return Composition();
        yield return BossTimeline();
        yield return SkipAndCancel();
        yield return RealWaveFlow();
        yield return Load(BossScene);
        yield return DevelopmentBossFlow();
        yield return Load("Assets/Scenes/BaseImprovisada.unity");
        var baseObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        Check(!baseObjects.SelectMany(r => r.GetComponentsInChildren<Animator>(true)).Any()
            && !baseObjects.SelectMany(r => r.GetComponentsInChildren<Rigidbody2D>(true)).Any()
            && Camera.main != null && Camera.main.GetComponents<MonoBehaviour>().Length == 0,
            "Chapter base loads without the old character or camera scripts");
        yield return Capture(1200, 720, "base_after_cleanup");
    }

    // ---------------------------------------------------------------- ONDA N (determinístico, tempo congelado)
    private static IEnumerator Composition()
    {
        var ui = Find<WavesUI>();
        var announcer = ui.Announcer;
        var banner = announcer.Wave;
        var style = banner.Style;
        ui.SuppressStartPanel(true);
        Time.timeScale = 0f;
        var values = new int[WaveBannerStyle.MaxDigits];
        var xs = new int[WaveBannerStyle.MaxDigits];
        int completed = 0;
        foreach (int wave in new[] { 1, 7, 9, 10, 15, 99, 100, 1234 })
        {
            completed = 0;
            announcer.Cancel();
            announcer.Play(wave, null, () => completed++);
            var layout = style.Compose(wave, values, xs);
            Check(announcer.IsPlaying && !announcer.HasBoss && banner.Visible && Mathf.Approximately(announcer.Duration, 1050f), $"ONDA {wave}: wave-only announcement of 1050 ms");
            announcer.SeekForValidation(500f);
            float expected = BannerMotion.EvenFloor(600f - layout.CenterX * 2f + BannerMotion.Evaluate(style.motion, 500f)) - 600f;
            Check(Mathf.Approximately(banner.Root.anchoredPosition.x, expected) && Mathf.Approximately(banner.Root.anchoredPosition.y, -160f)
                && Mathf.RoundToInt(banner.Root.anchoredPosition.x) % 2 == 0, $"ONDA {wave}: centred on its box ({layout.CenterX}) at even screen px");
            bool digitsOk = true;
            for (int i = 0; i < layout.digitCount; i++)
            {
                Image digit = banner.DigitImage(i);
                digitsOk &= digit.enabled && digit.sprite == style.digits[values[i]].normal
                    && digit.rectTransform.anchoredPosition == new Vector2(xs[i] * 2, -style.digitsTop * 2)
                    && digit.rectTransform.sizeDelta == digit.sprite.rect.size * 2f;
            }
            Check(digitsOk, $"ONDA {wave}: {layout.digitCount} digit(s) at manifest x [{string.Join(",", xs.Take(layout.digitCount))}], native size ×2, never compressed");
            Check(Mathf.Approximately(banner.BarMid.rectTransform.sizeDelta.x, Mathf.Max(0, layout.barLength - 150) * 2f)
                && banner.BarEnd.rectTransform.anchoredPosition.x == (14 + layout.barLength - 12) * 2, $"ONDA {wave}: bar grows to L = {layout.barLength}");
            Check(banner.Burst.enabled && banner.Burst.rectTransform.anchoredPosition == new Vector2(layout.burstX * 2, -layout.burstY * 2), $"ONDA {wave}: burst on the last digit");
            var shown = banner.Root.GetComponentsInChildren<Image>().Where(i => i.enabled).Select(i => i.name).ToArray();
            Check(new[] { "BarHead", "BarEnd", "Onda", "Burst" }.All(shown.Contains) && shown.Contains("BarMid") == layout.barLength > 150
                && !shown.Contains("Speedlines"), $"ONDA {wave}: visible pieces [{string.Join(",", shown)}]");
            if (wave == 10 || wave == 1 || wave == 7 || wave == 15 || wave == 99)
                yield return Capture(1200, 720, $"wave_{wave}_500ms");
            announcer.SeekForValidation(1060f);
            Check(completed == 1 && !announcer.IsPlaying && !banner.Visible, $"ONDA {wave}: completes once and leaves no banner");
        }

        // ONDA 10: ênfase dos algarismos e speedlines pela velocidade
        announcer.Cancel();
        announcer.Play(10, null, () => completed++);
        Sprite[] lines = style.speedlines;
        var poses = new (float ms, string suffix, bool burst)[] { (100, "", false), (190, "_flash", true), (220, "_pop1", true), (260, "", true), (700, "", true) };
        foreach (var pose in poses)
        {
            announcer.SeekForValidation(pose.ms);
            bool ok = banner.DigitImage(0).sprite.name == "wave_digit_1" + pose.suffix && banner.DigitImage(1).sprite.name == "wave_digit_0" + pose.suffix;
            Check(ok && banner.Burst.enabled == pose.burst, $"ONDA 10 at {pose.ms} ms: digits '{banner.DigitImage(0).sprite.name}', burst {pose.burst}");
        }
        completed = 0;
        announcer.Cancel();
        announcer.Play(10, null, () => completed++);
        // Tamanho pela velocidade (s < 60, m < 105, l), quadro _0/_1 alternando a cada 33 ms.
        Sprite Line(int size, float ms) => lines[size * 2 + (Mathf.FloorToInt(ms / 33f) & 1)];
        var speeds = new (float ms, Sprite sprite)[] { (5, Line(2, 5)), (30, Line(1, 30)), (40, Line(1, 40)), (60, Line(0, 60)), (100, Line(0, 100)), (150, null), (500, null), (1000, Line(1, 1000)) };
        foreach (var speed in speeds)
        {
            announcer.SeekForValidation(speed.ms);
            bool visible = banner.Speedlines.enabled;
            Check(speed.sprite == null ? !visible : visible && banner.Speedlines.sprite == speed.sprite,
                $"Speedlines at {speed.ms} ms: {(visible ? banner.Speedlines.sprite.name : "hidden")}");
        }
        announcer.Cancel();
        foreach (float ms in new[] { 60f, 120f, 180f, 260f, 500f, 820f, 980f })
        {
            announcer.Cancel();
            announcer.Play(10, null, null);
            announcer.SeekForValidation(ms);
            yield return Capture(1200, 720, $"wave_10_key_{ms:0000}ms");
        }
        announcer.SeekForValidation(500f);
        yield return Capture(1920, 1080, "wave_10_500ms");
        yield return Capture(800, 600, "wave_10_500ms");
        announcer.Cancel();
        Time.timeScale = 1f;
    }

    // ---------------------------------------------------------------- ONDA 10 → penas → PERIGO! GOOSINI
    private static IEnumerator BossTimeline()
    {
        var ui = Find<WavesUI>();
        var announcer = ui.Announcer;
        var boss = announcer.Boss;
        var wave = announcer.Wave;
        var theme = AssetDatabase.LoadAssetAtPath<BossAlertTheme>("Assets/Data/UI/GoosiniBossAlert.asset");
        Time.timeScale = 0f;
        int completed = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => completed++);
        Check(announcer.HasBoss && Mathf.Approximately(announcer.Duration, 2500f) && Mathf.Approximately(announcer.BossStart, 1200f), "Boss sequence: 1050 + 150 + 1300 = 2500 ms");
        bool overlap = false;
        for (float ms = 0f; ms <= 2490f; ms += 10f)
        {
            announcer.SeekForValidation(ms);
            overlap |= wave.Visible && boss.Visible;
            if (ms < 1050f && (!wave.Visible || boss.Visible)) overlap = true;
            if (ms >= 1050f && ms < 1200f && (wave.Visible || boss.Visible)) overlap = true;
            if (ms >= 1200f && !boss.Visible) overlap = true;
            if (ms < 1050f && boss.SoundsPlayed != 0) overlap = true;
        }
        Check(!overlap && completed == 0, "ONDA 10 only until 1050, nothing but feathers until 1200, GOOSINI from 1200; never together");
        Check(boss.SoundsPlayed == 1 && boss.AlertAudio.clip == theme.alertSound && boss.AlertAudio.clip.name == "goosini_boss_alert_final"
            && Mathf.Approximately(boss.AlertAudio.volume, .7f) && !boss.AlertAudio.loop && boss.AlertAudio.spatialBlend == 0f
            && boss.AlertAudio.outputAudioMixerGroup != null && boss.AlertAudio.outputAudioMixerGroup.name == "Effects",
            "Final WAV plays once, 2D, volume 0.70, routed to the Effects (SFX) mixer group");
        announcer.SeekForValidation(2510f);
        Check(completed == 1 && !announcer.IsPlaying && !boss.Visible, "Boss sequence completes once at 2500 ms");

        completed = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => completed++);
        announcer.SeekForValidation(1100f);
        Check(boss.TransitionFeather(0).enabled && !boss.TransitionFeather(3).enabled && !wave.Visible && !boss.Visible,
            "Transition at 1100 ms: first feathers cross the empty sky");
        Check(boss.SoundsPlayed == 1 && boss.AlertAudio.isPlaying, "Alert WAV starts at 1050 ms, during the feather transition");
        yield return Capture(1200, 720, "boss_seq_1110ms");
        announcer.SeekForValidation(1170f);
        yield return Capture(1200, 720, "boss_seq_1170ms");
        var checksAt = new (float local, BossAlertBanner.AlertState state, bool flash, bool impact, int scarf)[]
        {
            (100, BossAlertBanner.AlertState.Hidden, false, false, 1), (210, BossAlertBanner.AlertState.Flash, true, true, 2),
            (250, BossAlertBanner.AlertState.Amber, false, true, 2), (300, BossAlertBanner.AlertState.Red, false, false, 3),
            (450, BossAlertBanner.AlertState.Amber, false, false, 1), (500, BossAlertBanner.AlertState.Red, false, false, 1),
            (560, BossAlertBanner.AlertState.Amber, false, false, 2), (700, BossAlertBanner.AlertState.Red, false, false, 3),
            (1000, BossAlertBanner.AlertState.Red, false, false, 3),
        };
        foreach (var at in checksAt)
        {
            announcer.SeekForValidation(1200f + at.local);
            bool alertOk = boss.CurrentAlert == at.state && boss.Alert.enabled == (at.state != BossAlertBanner.AlertState.Hidden);
            if (at.state == BossAlertBanner.AlertState.Flash) alertOk &= boss.Alert.sprite.name == "boss_alert_perigo_flash";
            if (at.state == BossAlertBanner.AlertState.Amber) alertOk &= boss.Alert.sprite.name == "boss_alert_perigo_amber";
            if (at.state == BossAlertBanner.AlertState.Red) alertOk &= boss.Alert.sprite.name == "boss_alert_perigo_red";
            Check(alertOk && boss.Title.sprite == (at.flash ? theme.titleFlash : theme.title) && boss.Impact.enabled == at.impact
                && boss.Trail.sprite == theme.trailFrames[at.scarf]
                && Mathf.Approximately(boss.Root.anchoredPosition.x, boss.BannerX(at.local) - 600f) && Mathf.Approximately(boss.Root.anchoredPosition.y, -108f),
                $"GOOSINI +{at.local} ms: PERIGO! {at.state}, title flash {at.flash}, impact {at.impact}, scarf {at.scarf}");
        }
        var bossShown = boss.Root.GetComponentsInChildren<Image>().Where(i => i.enabled).Select(i => i.name).ToArray();
        Check(new[] { "Trail", "BandHead", "BandMid", "BandEnd", "Front", "Top", "Title", "Alert" }.All(bossShown.Contains),
            $"GOOSINI visible pieces [{string.Join(",", bossShown)}]");
        Check(boss.Title.rectTransform.anchoredPosition == new Vector2(132, -68) && boss.Title.rectTransform.sizeDelta == new Vector2(420, 96)
            && boss.Trail.rectTransform.anchoredPosition == new Vector2(536, -104), "Title (66,34) and scarf (268,52) at the manifest anchors, 2×");

        // Penas do banner: ficam onde nasceram, derivam 0,18 px/ms
        announcer.Cancel();
        announcer.Play(10, theme, null);
        announcer.SeekForValidation(1300f);
        var layer = boss.Particles;
        var before = Enumerable.Range(0, layer.PoolSize).Where(i => layer.PoolImage(i).enabled).ToDictionary(i => i, i => layer.PoolImage(i).rectTransform.anchoredPosition);
        float bannerBefore = boss.Root.anchoredPosition.x;
        announcer.SeekForValidation(1330f);
        var stayed = before.Where(p => layer.PoolImage(p.Key).enabled).Select(p => layer.PoolImage(p.Key).rectTransform.anchoredPosition - p.Value).ToArray();
        Check(layer.ActiveCount > 0 && stayed.Length > 0 && stayed.All(d => d.x >= 2 && d.x <= 8) && Mathf.Abs(boss.Root.anchoredPosition.x - bannerBefore) > 40,
            $"{layer.ActiveCount} entry feathers stay in screen space (drift {string.Join(" ", stayed.Select(d => d.x))} px) while the banner moves {boss.Root.anchoredPosition.x - bannerBefore:0} px");
        // anchoredPosition.y cresce para cima: descer na tela é y negativo.
        Check(stayed.All(d => d.y <= 0 && d.y >= -4), "Feathers drift down slowly");
        announcer.SeekForValidation(2290f);
        Check(layer.ActiveCount >= 4, "Exit feathers burst out (4 at once at +1090 ms)");
        bool dissolveSeen = false;
        for (float ms = 2300f; ms < 2500f; ms += 10f)
        {
            announcer.SeekForValidation(ms);
            dissolveSeen |= Enumerable.Range(0, layer.PoolSize).Any(i => layer.PoolImage(i).enabled && layer.PoolImage(i).sprite.name.Contains("dissolve"));
        }
        Check(dissolveSeen, "Feathers dissolve by sprite swap (dissolve_1/_2)");
        announcer.Cancel();

        foreach (float ms in new[] { 120f, 500f, 950f, 1300f, 1450f, 1900f, 2350f })
        {
            announcer.Cancel();
            announcer.Play(10, theme, null);
            announcer.SeekForValidation(ms);
            yield return Capture(1200, 720, $"boss_seq_{ms:0000}ms");
        }
        announcer.Cancel();
        announcer.Play(10, theme, null);
        announcer.SeekForValidation(1800f);
        yield return Capture(1920, 1080, "boss_seq_1800ms");
        yield return Capture(800, 600, "boss_seq_1800ms");
        announcer.Cancel();
        Check(!boss.AlertAudio.isPlaying && layer.ActiveCount == 0, "Cancel stops the alert and clears feathers");
        Time.timeScale = 1f;
    }

    // ---------------------------------------------------------------- pulo, cancelamento, sobreposição, pausa
    private static IEnumerator SkipAndCancel()
    {
        var announcer = Find<WavesUI>().Announcer;
        var boss = announcer.Boss;
        var wave = announcer.Wave;
        var theme = AssetDatabase.LoadAssetAtPath<BossAlertTheme>("Assets/Data/UI/GoosiniBossAlert.asset");
        var pause = Find<PauseController>();
        Time.timeScale = 0f;
        int a = 0, b = 0;
        announcer.Cancel();
        announcer.Play(5, null, () => a++);
        announcer.SeekForValidation(300f);
        announcer.Skip();
        Check(Mathf.Approximately(announcer.Clock, 800f) && wave.Visible, "Wave skip jumps to 800 ms: the exit still plays");
        announcer.Skip();
        announcer.SeekForValidation(1060f);
        Check(a == 1, "Skipped wave completes exactly once");

        a = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => a++);
        announcer.SeekForValidation(400f);
        announcer.Skip();
        announcer.SeekForValidation(1000f);
        bool bossEarly = boss.Visible;
        announcer.SeekForValidation(1100f);
        Check(!bossEarly && boss.Visible && Mathf.Approximately(announcer.Clock - announcer.BossStart, 1100f) && !boss.TransitionFeather(0).enabled,
            "Skipping the whole sequence: wave exit, then GOOSINI already at 1050 ms (exit only), no transition");
        announcer.SeekForValidation(1310f);
        Check(a == 1 && boss.SoundsPlayed == 0 && !announcer.IsPlaying, "Whole-sequence skip: one callback, no alert sound");

        a = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => a++);
        announcer.SeekForValidation(1600f);
        int alive = boss.Particles.ActiveCount;
        announcer.Skip();
        Check(Mathf.Approximately(announcer.Clock - announcer.BossStart, 1050f) && boss.Visible && boss.SoundsPlayed == 1, "Boss skip jumps to 1050 ms without replaying the sound");
        announcer.SeekForValidation(announcer.BossStart + 1200f);
        Check(boss.Particles.ActiveCount <= alive, "After a skip no new feathers are spawned");
        Time.timeScale = 1f;
        float volume = boss.AlertAudio.volume;
        yield return Frames(8);
        Check(!boss.AlertAudio.isPlaying || boss.AlertAudio.volume < volume, "Skipped alert fades out instead of cutting");
        Time.timeScale = 0f;
        announcer.SeekForValidation(announcer.BossStart + 1310f);
        Check(a == 1 && boss.SoundsPlayed == 1, "Skipped boss completes once");

        a = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => a++);
        announcer.SeekForValidation(1500f);
        announcer.Cancel();
        Check(!announcer.IsPlaying && !boss.Visible && !wave.Visible && boss.Particles.ActiveCount == 0 && !boss.AlertAudio.isPlaying, "Cancel restores an empty, silent state");
        Time.timeScale = 1f;
        yield return Frames(5);
        Check(a == 0, "A cancelled announcement never calls back");

        a = b = 0;
        Time.timeScale = 0f;
        announcer.Cancel();
        announcer.Play(3, null, () => a++);
        announcer.SeekForValidation(400f);
        announcer.Play(4, null, () => b++);
        Time.timeScale = 1f;
        float started = Time.unscaledTime;
        bool sawFour = false;
        while (announcer.IsPlaying && Time.unscaledTime - started < 4f)
        {
            sawFour |= announcer.WaveNumber == 4;
            yield return Frames(1);
        }
        Check(a == 0 && b == 1 && sawFour, "A new wave compresses the old exit (120 ms) and only the new callback runs");

        // Pausa: congela relógio, partículas e o WAV; continua sincronizado.
        a = 0;
        announcer.Cancel();
        announcer.Play(10, theme, () => a++);
        yield return Until(() => announcer.Clock >= 1500f, 6, "alert reveal");
        pause.PauseGame();
        float clock = announcer.Clock;
        int samples = boss.AlertAudio.timeSamples;
        var feather = Enumerable.Range(0, boss.Particles.PoolSize).Select(i => boss.Particles.PoolImage(i).rectTransform.anchoredPosition).ToArray();
        yield return Realtime(.4f);
        Check(pause.IsPaused && Mathf.Approximately(announcer.Clock, clock) && Mathf.Abs(boss.AlertAudio.timeSamples - samples) <= 2048
            && feather.SequenceEqual(Enumerable.Range(0, boss.Particles.PoolSize).Select(i => boss.Particles.PoolImage(i).rectTransform.anchoredPosition)),
            "Pause freezes timeline, WAV and feathers");
        pause.ResumeGame();
        yield return Until(() => !announcer.IsPlaying, 4, "resumed alert end");
        Check(a == 1 && boss.SoundsPlayed == 1, "Resumed alert finishes once with a single sound");
    }

    // ---------------------------------------------------------------- fluxo real em PlayerMovementTest
    private static IEnumerator RealWaveFlow()
    {
        var waves = Find<WaveController>();
        var ui = Find<WavesUI>();
        var announcer = ui.Announcer;
        var cutscenes = Find<RunCutscenes>();
        var spawner = waves.GetComponent<EnemySpawner>();
        ui.SuppressStartPanel(false);
        int bossStarted = 0;
        waves.BossStarted += _ => bossStarted++;
        waves.StartRun();
        yield return Until(() => cutscenes.Dialogue.IsPlaying, 10, "run intro");
        yield return HoldSkip(.7f);
        yield return Until(() => waves.Phase == WaveController.RunPhase.Announcing && waves.CurrentWave == 1, 5, "wave 1 announcement");
        Check(announcer.IsPlaying && announcer.WaveNumber == 1 && !announcer.HasBoss, "WaveAnnounced(1) starts the new ONDA 1 banner");
        Check(ui.GetComponentsInChildren<TMP_Text>(true).All(t => !t.text.StartsWith("ONDA 1"))
            && ui.transform.Find("WaveBanner") == null && ui.transform.Find("WaveCounter") == null,
            "Wave announcement uses sprites; legacy text banner and corner counter are absent");
        var playerHud = Find<PlayerHUD>();
        Check(!playerHud.GetComponentsInChildren<TMP_Text>(true).Any(t => t.name == "PointsText" || t.name == "KillsText")
            && playerHud.GetComponentInChildren<PlayerHUDCockpit>(true) != null, "Player cockpit remains without points or kills labels");
        bool early = false;
        while (announcer.IsPlaying)
        {
            early |= waves.Phase != WaveController.RunPhase.Announcing || spawner.AliveCount > 0;
            yield return Frames(1);
        }
        Check(!early, "Combat waits for the whole 1050 ms presentation (no spawns during it)");
        yield return Frames(2);
        Check(waves.Phase == WaveController.RunPhase.Combat, "Combat starts right after the presentation signals completion");

        // Salto para a onda do boss pelo próprio WaveController (música das ondas tocando).
        var music = waves.GetComponent<WaveMusic>();
        var musicSource = (AudioSource)typeof(WaveMusic).GetField("source", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(music);
        Check(musicSource.isPlaying && musicSource.volume > .1f, $"Wave music playing ({musicSource.volume:0.00}) before the boss wave");
        spawner.StopCombat();
        typeof(WaveController).GetProperty("CurrentWave").GetSetMethod(true).Invoke(waves, new object[] { waves.CombatWaveCount });
        typeof(WaveController).GetMethod("AnnounceNextWave", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(waves, null);
        foreach (Enemy enemy in Object.FindObjectsByType<Enemy>()) Object.Destroy(enemy.gameObject);
        Check(waves.IsBossWave(waves.CurrentWave) && announcer.HasBoss && announcer.WaveNumber == waves.WaveCount, "Boss wave identified by WaveController (ONDA " + waves.WaveCount + ")");
        var boss = announcer.Boss;
        float musicAtAlert = -1f;
        bool bossEarly = false, both = false;
        while (announcer.IsPlaying)
        {
            if (musicAtAlert < 0f && boss.SoundsPlayed > 0) musicAtAlert = musicSource.isPlaying ? musicSource.volume : 0f;
            bossEarly |= waves.ActiveBoss != null || waves.Phase != WaveController.RunPhase.Announcing;
            both |= announcer.Wave.Visible && boss.Visible;
            yield return Frames(1);
        }
        Check(musicAtAlert >= 0f && musicAtAlert <= .001f, $"Wave music already faded out when the alert starts (volume {musicAtAlert:0.000})");
        Check(!bossEarly && !both, "Boss does not spawn during ONDA 10 → GOOSINI; banners never overlap");
        yield return Frames(2);
        Check(waves.Phase == WaveController.RunPhase.Boss && waves.ActiveBoss != null && bossStarted == 1, "BossStarted fires once, after the presentation");
        Check(waves.ActiveBoss.CombatHeld && !waves.ActiveBoss.Movement.HasArrived, "Existing cinematic entrance begins after 2500 ms with combat held");
        yield return Frames(30);
        Check(!musicSource.isPlaying, "Wave music stopped by BossStarted");
        Check(ui.transform.Find("WaveCounter") == null, "Boss entry does not recreate a corner wave counter");

        // Derrota continua funcionando (ranking em pasta temporária).
        var player = Find<Player>();
        player.GetComponent<Health>().TakeDamage(100000f);
        yield return Frames(2);
        var panel = ui.transform.Find("StartAndResult");
        Check(waves.Phase == WaveController.RunPhase.Failed && !announcer.IsPlaying && panel.gameObject.activeSelf
            && panel.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "FIM DE JOGO")
            && panel.GetComponentsInChildren<Button>(true).Any(bt => bt.name == "ReturnToMenuButton" && bt.gameObject.activeSelf),
            "Player defeat still shows FIM DE JOGO with the return button");
    }

    // ---------------------------------------------------------------- GusiniBossTest: início real do boss de desenvolvimento
    private static IEnumerator DevelopmentBossFlow()
    {
        var waves = Find<WaveController>();
        var announcer = Find<WavesUI>().Announcer;
        var boss = announcer.Boss;
        var pause = Find<PauseController>();
        var cutscenes = Find<RunCutscenes>();
        var player = Find<Player>();
        Check(waves.Phase == WaveController.RunPhase.Announcing && waves.IsBossWave(waves.CurrentWave) && announcer.HasBoss && waves.ActiveBoss == null,
            "GusiniBossTest goes through ONDA 10 → GOOSINI before the boss");
        Check(player.Health.MaxHealth == 240, "Boss test kit still applied (240 HP)");
        float soundStart = -1f, maxDrift = 0f;
        bool paused = false, both = false, early = false;
        while (announcer.IsPlaying)
        {
            float local = announcer.Clock - announcer.BossStart;
            if (soundStart < 0f && boss.SoundsPlayed > 0) soundStart = announcer.Clock;
            if (boss.TryGetSoundClock(out float sound) && boss.AlertAudio.timeSamples > 0 && local > -100f)
                maxDrift = Mathf.Max(maxDrift, Mathf.Abs(sound - local));
            both |= announcer.Wave.Visible && boss.Visible;
            early |= waves.ActiveBoss != null;
            if (!paused && announcer.Clock >= 1400f)
            {
                paused = true;
                float peakError = Mathf.Abs(sound - local);
                report.Add($"At visual {local:0} ms after banner start the WAV is at {sound - boss.Theme.alertSoundOffsetMs:0} ms (impact_peak 349 ms)");
                pause.PauseGame();
                float frozen = announcer.Clock;
                int samples = boss.AlertAudio.timeSamples;
                yield return Realtime(.35f);
                Check(Mathf.Approximately(frozen, announcer.Clock) && Mathf.Abs(samples - boss.AlertAudio.timeSamples) <= 2048 && AudioListener.pause,
                    "Real pause during GOOSINI freezes animation and the WAV");
                pause.ResumeGame();
                Check(peakError < 60f, $"Visual slam and audio peak within {peakError:0} ms");
            }
            yield return Frames(1);
        }
        report.Add($"Alert started at sequence {soundStart:0} ms; max audio/visual drift {maxDrift:0} ms");
        Check(soundStart >= 1050f && soundStart < 1120f, $"WAV starts at {soundStart:0} ms of the sequence (target 1050)");
        Check(maxDrift <= 60f, $"Audio and animation stay in sync (max drift {maxDrift:0} ms)");
        Check(!both && !early && boss.SoundsPlayed == 1, "One sound, no overlap, no early boss");
        yield return Until(() => waves.ActiveBoss != null, 2, "boss spawn");
        var gusini = waves.ActiveBoss;
        var music = gusini.GetComponent<GusiniMusic>();
        Check(waves.Phase == WaveController.RunPhase.Boss && gusini.CombatHeld && !cutscenes.Dialogue.IsPlaying, "Entrance first, dialogue later");
        yield return Until(() => cutscenes.Dialogue.IsPlaying, 20, "boss intro dialogue");
        Check(gusini.Movement.HasArrived && cutscenes.Playing == RunCutscenes.Scene.BossIntro && !boss.AlertAudio.isPlaying,
            "Boss dialogue starts after the entrance, with the alert already finished");
        Check(music == null || music.Current == GusiniMusic.Track.None, "Gusini combat music not started before combat");
        yield return HoldSkip(.7f);
        yield return Until(() => gusini.Ready, 5, "boss combat");
        if (music != null) yield return Until(() => music.Current == GusiniMusic.Track.PhaseOne, 3, "phase 1 music");
        Check(music == null || music.Source.isPlaying, "Phase 1 music starts with combat, as before");
    }

    // ---------------------------------------------------------------- infraestrutura
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
        if (running) return;
        running = true;
        checks = errors = 0;
        report.Clear();
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 300;
        var settings = InputSystem.settings;
        originalBackground = settings.backgroundBehavior;
        originalEditorBehavior = settings.editorInputBehaviorInPlayMode;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        string folder = Path.Combine(Path.GetTempPath(), "CrocodiloWaveBossUI_" + Guid.NewGuid().ToString("N"));
        typeof(LeaderboardSession).GetField("store", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new LeaderboardStore(folder));
        CampaignProgress.UseDirectory(folder);
        keyboard = InputSystem.AddDevice<Keyboard>("WaveBossUIValidationKeyboard");
        Directory.CreateDirectory(Output);
        steps.Clear();
        steps.Push(Scenario());
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Wave/boss UI play validation timed out");
            while (steps.Count > 0)
            {
                IEnumerator top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            if (checks < 60) throw new InvalidOperationException("Too few assertions ran: " + checks);
            foreach (string line in report) Debug.Log("WAVE BOSS UI REPORT: " + line);
            Debug.Log("WAVE BOSS UI PLAY VALIDATION PASSED: " + checks + " assertions.");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        running = false;
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
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
        if (!condition) throw new InvalidOperationException("WAVE BOSS UI CHECK FAILED: " + message);
        checks++; Debug.Log("WAVE BOSS UI CHECK " + checks + ": " + message);
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

    private static IEnumerator HoldSkip(float seconds)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab));
        float started = Time.unscaledTime;
        while (Time.unscaledTime - started < seconds) yield return Frames(1);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return Frames(2);
    }

    private static IEnumerator Load(string path)
    {
        EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
        yield return Until(() => SceneManager.GetActiveScene().path == path && SceneManager.GetActiveScene().isLoaded, 20, "scene " + path);
        yield return Frames(3);
    }

    private static T Find<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include).Single();

    // Mesmo procedimento das capturas de diálogo: Canvas overlay renderizado pela câmera num RenderTexture.
    private static IEnumerator Capture(int width, int height, string name)
    {
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var texture = new RenderTexture(width, height, 24);
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
        camera.targetTexture = texture;
        yield return Frames(2);
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = texture;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        pixels.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes($"{Output}/{name}_{width}x{height}.png", pixels.EncodeToPNG());
        camera.targetTexture = null;
        foreach (Canvas canvas in canvases) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(pixels);
        texture.Release();
        yield return Frames(1);
    }
}
