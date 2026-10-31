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
public static class LakePiranhaHighLeapValidation
{
    private const string Key = "Crocodilo.Lake.PiranhaHighLeapValidation";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static Camera movieCamera;
    private static RenderTexture movieTarget;
    private static Texture2D moviePixels;
    private static Canvas[] movieCanvases;
    private static List<float> movieTimes;
    private static float movieStarted, nextMovieFrame;
    private static float previousMaxDeltaTime;
    static LakePiranhaHighLeapValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin();
            if (state == PlayModeStateChange.EnteredEditMode) { SessionState.SetBool(Key, false); EditorApplication.Exit(errors == 0 ? 0 : 1); }
        };
    }
    public static void Run()
    {
        EditorSceneManager.OpenScene(LakeStageTools.Test);
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    public static void RunFromMenu()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    private static void Begin()
    {
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 300;
        previousMaxDeltaTime=Time.maximumDeltaTime;Time.maximumDeltaTime=.05f;
        Application.runInBackground = true; Application.logMessageReceived += OnLog;
        Directory.CreateDirectory("Logs/LagoDoPneu");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v30/Piranhas/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"boss pronto");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();combat.StopCombat();
        var rb=player.GetComponent<Rigidbody2D>();var water=Object.FindAnyObjectByType<LakeWildlife>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),true);rb.position=new Vector2(-4,2);
        StartMovie();int count=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>count,"primeiro salto ambiente alto");
        float first=water.Clock;var jumper=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.CurrentState==LakePiranha.State.Leaping && p.Velocity.y>0);
        float peak=jumper.Position.y+jumper.Velocity.y*jumper.Velocity.y/28;
        Check(peak-water.WaterY>=1.6f,"Salto ambiente sobe pelo menos 1,6 unidade acima da água");
        yield return Wait(.28f);Check(jumper.Position.y>water.WaterY+.65f,"Piranha atravessa visivelmente a superfície");Capture("higher-ambient-jump.png");
        float clock=water.Clock;Vector2 position=jumper.Position;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(water.Clock==clock && jumper.Position==position,"Pausa congela novo arco das piranhas");Time.timeScale=1;
        count=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>count,"próximo salto mais frequente");
        float interval=water.Clock-first;Check(interval>=6.9f && interval<=11.2f,"Saltos ambientes agora espaçados em sete a onze segundos");
        Check(!water.WaterDangerActive && water.ActivePiranhas==10,"Saltos não criam nem escondem residentes");
        yield return Wait(1.8f);
        var nearby=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.Available);
        rb.position=new Vector2(nearby.Position.x,water.WaterY+.35f);player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),false);
        int hp=player.Health.CurrentHealth,bites=water.Bites,launches=water.Launches;
        yield return Until(()=>water.Launches>launches,"bote contra jogador perto da água");
        jumper=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.CurrentState==LakePiranha.State.Leaping && p.Velocity.y>0);
        peak=jumper.Position.y+jumper.Velocity.y*jumper.Velocity.y/28;
        Check(peak-water.WaterY>=1.6f,"Bote ofensivo usa o mesmo salto mais alto");
        yield return Until(()=>water.Bites>bites,"contato real do bote");
        Check(player.Health.CurrentHealth==hp-1,"Piranha continua causando um HP de dano");
        yield return Wait(.25f);Check(player.Health.CurrentHealth==hp-1,"Grupo respeita intervalo de dano");Capture("higher-attack-jump.png");
        player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),true);rb.position=new Vector2(rb.position.x,2);yield return Wait(1.5f);
        Check(!water.WaterDangerActive && water.ActivePiranhas==10,"Subir devolve piranhas ao nado");
        player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
        Check(water.ActivePiranhas==0 && waves.Phase==WaveController.RunPhase.Failed,"Derrota limpa saltos e perseguição");StopMovie();
        Debug.Log("PIRANHA_HIGH_LEAP interval="+interval+" peakAboveWater="+(peak-water.WaterY));
    }
    private static IEnumerator EnterLake()
    {
        var menu=Object.FindAnyObjectByType<DeveloperMenuView>();yield return Until(()=>{menu.Open();return menu.IsVisible;},"menu DEV");
        GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"loading");
        Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<LakeBossEntrance>()?.CurrentStage==LakeBossEntrance.Stage.Round,"onda dez da entrada");
    }
    private static void Capture(string filename = "gameplay.png")
    {
        var camera = Camera.main; var old = camera.targetTexture;
        var overlays = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingLayerName="LakeHUD"; }
        var rt = new RenderTexture(1280, 720, 24); var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        camera.targetTexture = rt;
        Object.FindAnyObjectByType<LakeBackdrop>().SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases(); camera.Render(); var previous = RenderTexture.active; RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        Directory.CreateDirectory("Docs/ArtPreviews/LakeSwampActions_v30/Piranhas");
        File.WriteAllBytes("Docs/ArtPreviews/LakeSwampActions_v30/Piranhas/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v30/Piranhas/Frames"); movieCamera = Camera.main;
        movieTarget = new RenderTexture(960, 540, 24); moviePixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        movieCamera.targetTexture = movieTarget;movieCamera.aspect=960f/540f;
        movieCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = movieCamera; canvas.planeDistance = 1; canvas.sortingLayerName="LakeHUD"; }
        movieTimes = new List<float>(); movieStarted = Time.unscaledTime; nextMovieFrame = 0;
    }
    private static void RecordMovie()
    {
        if (movieTimes == null || Time.unscaledTime - movieStarted < nextMovieFrame) return;
        Canvas.ForceUpdateCanvases(); movieCamera.Render(); var previous = RenderTexture.active; RenderTexture.active = movieTarget;
        moviePixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); moviePixels.Apply(); RenderTexture.active = previous;
        File.WriteAllBytes("Logs/LagoDoPneu/v30/Piranhas/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
        movieTimes.Add(Time.unscaledTime - movieStarted); nextMovieFrame = Time.unscaledTime - movieStarted + .05f;
    }
    private static void StopMovie()
    {
        if (movieTimes == null) return;
        var text = new System.Text.StringBuilder("ffconcat version 1.0\n");
        for (int i = 0; i < movieTimes.Count; i++)
        {
            text.Append("file 'frame_" + i.ToString("D5") + ".png'\n");
            text.Append("duration " + (i + 1 < movieTimes.Count ? movieTimes[i + 1] - movieTimes[i] : .05f).ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + "\n");
        }
        File.WriteAllText("Logs/LagoDoPneu/v30/Piranhas/Frames/frames.ffconcat", text.ToString());
        movieTimes = null; movieCamera.targetTexture = null;
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
        movieTarget.Release(); Object.Destroy(movieTarget); Object.Destroy(moviePixels);
    }
    private static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label)
    { double end = EditorApplication.timeSinceStartup + 30; while (!predicate()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label); yield return null; } }
    private static void Check(bool value, string label)
    { if (!value) throw new InvalidOperationException("LAKE_CHECK_FAILED " + label); checks++; Debug.Log("LAKE_CHECK " + label); }
    private static void OnLog(string message, string trace, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Lake validation deadline");
            RecordMovie();
            while (steps.Count > 0) { var top = steps.Peek(); if (!top.MoveNext()) { steps.Pop(); continue; } if (top.Current is IEnumerator child) { steps.Push(child); continue; } return; }
            Debug.Log("LAKE_PLAY_OK checks=" + checks + " errors=" + errors);
        }
        catch (Exception e) { errors++; Debug.LogException(e); }
        StopMovie();
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
        File.WriteAllText("Logs/LagoDoPneu/v30/Piranhas/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
