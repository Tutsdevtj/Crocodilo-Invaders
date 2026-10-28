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
public static class MacaquiniJungleVoiceV26Validation
{
    private const string Key = "Crocodilo.JungleVoice26.Validation", Output = "Docs/ArtPreviews/MacaquiniJungleVoice_v26";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    private static JungleMotionAudioCapture capture;
    static MacaquiniJungleVoiceV26Validation()
    {
        EditorApplication.playModeStateChanged += state => {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin();
            if (state == PlayModeStateChange.EnteredEditMode) {
                SessionState.SetBool(Key, false); bool build = SessionState.GetBool(Key + ".Build", false); SessionState.SetBool(Key + ".Build", false);
                if (build && errors == 0) EditorApplication.delayCall += MacaquiniJungleVoiceV26Tools.Build;
                else EditorApplication.Exit(errors == 0 ? 0 : 1);
            }
        };
    }
    public static void Run() { MacaquiniJungleVoiceV26Tools.Import(); EditorSceneManager.OpenScene(MacaquiniJetpackTools.Test); SessionState.SetBool(Key, true); EditorApplication.isPlaying = true; }
    public static void RunAndBuild() { SessionState.SetBool(Key + ".Build", true); Run(); }
    private static void Begin()
    {
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 100; Application.runInBackground = true;
        Directory.CreateDirectory(Output); Application.logMessageReceived += OnLog; steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(4); CampaignProgress.UseDirectory("Logs/JungleStage/JungleVoice26Save"); LeaderboardSession.Cancel();
        var player = Object.FindAnyObjectByType<Player>(); player.GetComponent<PlayerInputReader>().enabled = false; player.Health.SetInvulnerable(typeof(MacaquiniJungleVoiceV26Validation), true);
        var waves = Object.FindAnyObjectByType<WaveController>(); yield return Until(() => waves.ActiveBoss != null && waves.ActiveBoss.Ready, "boss ready");
        var boss = waves.ActiveBoss; var phase = boss.GetComponent<MacaquiniPhaseTwo>(); var presentation = boss.GetComponent<MacaquiniTransformationPresentation>(); var jungle = boss.GetComponent<MacaquiniJungleVoice>();
        Check(jungle != null && jungle.Available, "Três vozes carregadas no boss atual");
        Check(jungle.CuesPlayed == 0 && !jungle.AnyPlaying, "Vocalizações não tocam antes da transformação");
        Check(jungle.Voices.Length == 3 && jungle.Voices.All(v => !v.loop && v.spatialBlend == 0 && !v.ignoreListenerPause), "Vozes estéreo sem loop respeitam pausa");
        var group = GameAudioSettings.Mixer.FindMatchingGroups("Effects")[0];
        Check(jungle.Voices.All(v => v.outputAudioMixerGroup == group), "Vozes passam pelo volume de efeitos do jogo");
        foreach (string name in new[] { "effort_woods", "tear_call", "power_call" })
        {
            var clip = Resources.Load<AudioClip>("Audio/MacaquiniJungleVoiceV26/" + name);
            Check(clip != null && clip.channels == 2 && clip.frequency == 44100 && clip.length > 2 && clip.length < 4, "WAV estéreo completo " + name);
            var samples = new float[clip.samples * clip.channels]; Check(clip.GetData(samples, 0), "PCM pré-carregado " + name);
            Check(samples.Max(v => Mathf.Abs(v)) < .71f, "Pico controlado " + name);
            double stereo = 0, tail = 0;
            int tailStart = Mathf.CeilToInt(clip.frequency * clip.channels * 1.6f);
            for (int i = 0; i < samples.Length - 1; i += 2) { stereo += Math.Pow(samples[i] - samples[i + 1], 2); if (i > tailStart) tail += samples[i] * samples[i]; }
            Check(stereo > 1 && tail > .01, "Eco estéreo e cauda preservados " + name);
        }
        capture = Camera.main.gameObject.AddComponent<JungleMotionAudioCapture>(); capture.Begin();
        boss.Health.TakeDamage(999); yield return Frames(2);
        Check(phase.CurrentStage == MacaquiniPhaseTwo.Stage.Tearing && jungle.CuesPlayed == 1, "Esforço começa junto da transformação nativa");
        yield return Until(() => presentation.CapReleases == 1, "native cap event");
        Check(jungle.CuesPlayed == 2, "Chamada curta sincronizada à retirada do topo");
        jungle.CapEffort(); Check(jungle.CuesPlayed == 2, "Evento repetido não duplica vocalização");
        var pause = Object.FindAnyObjectByType<PauseController>(); pause.PauseGame(); yield return WallWait(.12f);
        var positions = jungle.Voices.Select(v => v.timeSamples).ToArray(); float frozen = Time.time;
        yield return WallWait(.22f);
        Check(AudioListener.pause && Time.time == frozen && jungle.Voices.Select((v, i) => v.timeSamples == positions[i]).All(v => v), "Pausa congela voz, eco e transformação");
        pause.ResumeGame();
        yield return Until(() => jungle.CuesPlayed == 3, "native tear effort");
        Check(phase.CurrentStage == MacaquiniPhaseTwo.Stage.Tearing && jungle.Voices.Any(v => v.clip != null && v.clip.name == "tear_call" && v.isPlaying), "Esforço vocal acompanha o rasgo aos 3,25 s");
        jungle.TearEffort(); Check(jungle.CuesPlayed == 3, "Esforço não empilha em evento duplicado");
        yield return Until(() => jungle.RoarsPlayed == 1, "native power roar");
        Check(jungle.CuesPlayed == 4 && presentation.Roars == 1, "Grito final único sincronizado aos 5,5 s");
        jungle.PowerRoar(); Check(jungle.CuesPlayed == 4 && jungle.RoarsPlayed == 1, "Grito final não duplica nem reinicia ecos");
        yield return Until(() => phase.CurrentStage == MacaquiniPhaseTwo.Stage.HalfShell, "native transformation completed");
        boss.GetComponent<MacaquiniCombat>().StopCombat();
        Check(jungle.AnyPlaying, "Cauda do grito se estende suavemente à nova forma");
        yield return Until(() => !jungle.AnyPlaying, "forest tail ends");
        Check(jungle.CuesPlayed == 4, "Som termina sozinho, sem loops ou vocalização extra");
        capture.Save(Output + "/transformacao-mix-real.wav"); capture = null;
        Check(new FileInfo(Output + "/transformacao-mix-real.wav").Length > 44100, "Mixagem real capturada no AudioListener");
        boss.StopBattle(); Check(!jungle.AnyPlaying, "Fim da batalha sem cauda órfã");
        // A fresh native transformation tests interruption while the echo is still active.
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        yield return Frames(5); var nextPlayer = Object.FindAnyObjectByType<Player>(); nextPlayer.Health.SetInvulnerable(typeof(MacaquiniJungleVoiceV26Validation), true);
        yield return Until(() => Object.FindAnyObjectByType<WaveController>().ActiveBoss != null && Object.FindAnyObjectByType<WaveController>().ActiveBoss.Ready, "restart boss ready");
        boss = Object.FindAnyObjectByType<WaveController>().ActiveBoss; jungle = boss.GetComponent<MacaquiniJungleVoice>(); boss.Health.TakeDamage(999);
        yield return Wait(.2f); Check(jungle.AnyPlaying && jungle.CuesPlayed == 1, "Reinício começa sequência limpa de vozes");
        boss.StopBattle(); yield return Frames(3); Check(!jungle.AnyPlaying, "Interromper transformação cancela voz e ecos imediatamente");
        int cues = jungle.CuesPlayed; jungle.PowerRoar(); Check(jungle.CuesPlayed == cues && !jungle.AnyPlaying, "Callbacks tardios não ressuscitam o áudio");
    }
    private static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    private static IEnumerator WallWait(float seconds) { double end = EditorApplication.timeSinceStartup + seconds; while (EditorApplication.timeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label) { double end = EditorApplication.timeSinceStartup + 25; while (!predicate()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label); yield return null; } }
    private static void Check(bool value, string label) { if (!value) throw new InvalidOperationException("JUNGLE26_FAIL " + label); checks++; Debug.Log("JUNGLE26_CHECK " + label); }
    private static void OnLog(string message, string trace, LogType type) { if (trace.Contains("UnityEditor.Search.SearchDatabase")) return; if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("JungleVoice26 deadline");
            while (steps.Count > 0) { var current = steps.Peek(); if (!current.MoveNext()) { steps.Pop(); continue; } if (current.Current is IEnumerator child) { steps.Push(child); continue; } return; }
            Debug.Log("MACAQUINI_JUNGLE26_PLAY_OK checks=" + checks + " errors=" + errors);
        } catch (Exception error) { errors++; Debug.LogException(error); }
        if (capture != null) capture.Save(Output + "/transformacao-mix-real.wav");
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog; Time.timeScale = 1; AudioListener.pause = false;
        File.WriteAllText(Output + "/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}"); EditorApplication.isPlaying = false;
    }
}
