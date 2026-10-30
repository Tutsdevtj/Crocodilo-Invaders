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
public static class LakeBossEntrancePlayValidation
{
    private const string Key = "Crocodilo.Lake.EntranceValidation";
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
    static LakeBossEntrancePlayValidation()
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
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 200;
        previousMaxDeltaTime=Time.maximumDeltaTime;Time.maximumDeltaTime=.05f;
        Application.runInBackground = true; Application.logMessageReceived += OnLog;
        Directory.CreateDirectory("Logs/LagoDoPneu");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v17/TestSave");
        yield return EnterLake();
        var intro=Object.FindAnyObjectByType<LakeBossEntrance>();var announcer=Object.FindAnyObjectByType<WaveAnnouncer>();
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.GetComponent<Rigidbody2D>().position=new Vector2(-3,1);
        Check(waves.CurrentWave==10 && waves.IsBossWave(10),"Onda dez continua sendo a rodada do boss");
        Check(intro.CurrentStage==LakeBossEntrance.Stage.Round && announcer.IsPlaying && announcer.WaveNumber==10,"Anuncio original da onda dez precede apresentacao propria");
        Check(waves.ActiveBoss==null && waves.Phase==WaveController.RunPhase.Announcing,"Boss nao aparece antes do aviso");
        StartMovie();yield return Wait(.3f);Capture("round-10.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Warning,"aviso do lago");yield return Wait(.15f);
        Check(!player.Movement.enabled && !player.Combat.enabled && player.Health.IsInvulnerable,"Jogador protegido e sem atacar durante apresentacao");
        int hp=player.Health.CurrentHealth;Check(!player.Health.TakeDamage(2) && player.Health.CurrentHealth==hp,"Protecao da abertura impede dano real");
        Check(intro.PresentationCanvas.sortingOrder==240 && !intro.PresentationCanvas.GetComponent<CanvasGroup>().blocksRaycasts,"Apresentacao fica abaixo da pausa e nao bloqueia interface");Capture("lake-warning.png");
        float clock=intro.Clock,waterClock=lake.MotionClock;Time.timeScale=0;AudioListener.pause=true;
        double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(intro.Clock==clock && lake.MotionClock==waterClock && intro.CurrentStage==LakeBossEntrance.Stage.Warning,"Pausa congela aviso e ambiente juntos");Time.timeScale=1;AudioListener.pause=false;
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Reveal,"revelacao do nome");yield return Wait(.5f);
        Check(waves.Phase==WaveController.RunPhase.Announcing && waves.ActiveBoss==null,"Nome e retrato seguram anuncio sem iniciar luta");
        var title=intro.PresentationCanvas.GetComponentsInChildren<TMPro.TMP_Text>().First(t=>t.name=="BossTitle");title.ForceMeshUpdate();
        Check(title.text=="SAPENEU CURURU" && !title.isTextOverflowing,"Nome completo legivel na apresentacao propria");Capture("sapeneu-title.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Arrival && waves.ActiveBoss!=null,"sapo emergindo");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var body=boss.GetComponent<Rigidbody2D>();
        Check(boss.CombatHeld && !boss.Ready && body.position.y<lake.WaterY,"Sapeneu comeca submerso e ainda sem combate");
        float startY=body.position.y;yield return Wait(.7f);
        Check(body.position.y>startY && combat.CurrentState==SapoDoPneuCombat.State.Entering && !combat.Tongue.Busy,"Boss emerge sem atacar");
        Check(boss.Health.IsInvulnerable && !player.Movement.enabled,"Entrada fisica mantem protecao e controle bloqueado");Capture("water-emergence.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Finished,"fim da abertura");yield return Frames(4);
        Check(boss.Ready && !boss.CombatHeld && boss.Movement.HasArrived,"Fim da abertura libera boss pronto exatamente uma vez");
        Check(player.Movement.enabled && player.Combat.enabled && !player.Health.IsInvulnerable,"Controle e vulnerabilidade do jogador retornam");
        Check(!boss.Health.IsInvulnerable && !intro.PresentationCanvas.gameObject.activeSelf,"Boss pode receber dano e tela cinematica desaparece");
        Check(intro.Entrances==1 && combat.CurrentState==SapoDoPneuCombat.State.Float,"Sem repeticao da abertura ou ataque antecipado");Capture("battle-ready.png");
        player.Health.SetInvulnerable(typeof(LakeBossEntrancePlayValidation),true);yield return Until(()=>combat.Tongue.Busy,"combate apos abertura");
        Check(combat.Tongue.AliveFlyCount>0 && Object.FindObjectsByType<LakeLilyPads>().Length==1,"Moscas e folhas continuam apos a entrada");yield return Wait(1.2f);StopMovie();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"novo menu");yield return EnterLake();
        intro=Object.FindAnyObjectByType<LakeBossEntrance>();waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();player.GetComponent<PlayerInputReader>().enabled=false;player.GetComponent<Rigidbody2D>().position=new Vector2(-3,1);
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Warning,"aviso para cancelamento");intro.enabled=false;yield return Frames(3);
        Check(!intro.IsPlaying && player.Movement.enabled && !player.Health.IsInvulnerable,"Desativar apresentacao restaura jogador e remove protecao");
        yield return Until(()=>waves.ActiveBoss?.Ready==true,"boss sem apresentacao apos cancelamento");
        Check(waves.Phase==WaveController.RunPhase.Boss && !waves.ActiveBoss.CombatHeld,"Cancelamento libera token e nao prende a fase");

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"terceiro menu");yield return EnterLake();
        intro=Object.FindAnyObjectByType<LakeBossEntrance>();waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Reveal,"titulo para encerramento");
        player.Health.SetInvulnerable(intro,false);player.Health.TakeDamage(9999);yield return Frames(4);
        Check(waves.Phase==WaveController.RunPhase.Failed && !intro.IsPlaying && waves.ActiveBoss==null,"Encerrar run durante titulo cancela entrada sem spawn atrasado");
        Check(!player.Movement.enabled && !intro.PresentationCanvas.gameObject.activeSelf,"Derrota nao reativa controles nem deixa sobreposicao");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Progresso real preservado");
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
        Directory.CreateDirectory("Docs/ArtPreviews/SapeneuCururu_Entrance_v17");
        File.WriteAllBytes("Docs/ArtPreviews/SapeneuCururu_Entrance_v17/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v17/Frames"); movieCamera = Camera.main;
        movieTarget = new RenderTexture(960, 540, 24); moviePixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        movieCamera.targetTexture = movieTarget;
        movieCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = movieCamera; canvas.planeDistance = 1; canvas.sortingLayerName="LakeHUD"; }
        movieTimes = new List<float>(); movieStarted = Time.unscaledTime; nextMovieFrame = 0;
    }
    private static void RecordMovie()
    {
        if (movieTimes == null || Time.unscaledTime - movieStarted < nextMovieFrame) return;
        Canvas.ForceUpdateCanvases(); movieCamera.Render(); var previous = RenderTexture.active; RenderTexture.active = movieTarget;
        moviePixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); moviePixels.Apply(); RenderTexture.active = previous;
        File.WriteAllBytes("Logs/LagoDoPneu/v17/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v17/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v17/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}

