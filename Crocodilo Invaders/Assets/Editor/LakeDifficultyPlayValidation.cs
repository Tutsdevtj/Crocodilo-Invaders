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
public static class LakeDifficultyPlayValidation
{
    private const string Key = "Crocodilo.Lake.DifficultyValidation";
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
    static LakeDifficultyPlayValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v21/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"entrada do boss");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var rb=player.GetComponent<Rigidbody2D>();var area=combat.Arena;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);
        rb.position=new Vector2(area.xMin+3,area.yMax-1.1f);player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);
        Check(boss.Health.MaxHealth==200,"HP do boss preservado");
        Check(Object.FindAnyObjectByType<LakeLilyPads>().Count==6,"Seis folhas pequenas, sem os dois apoios grandes");
        Check(Mathf.Approximately(SapoDoPneuCombat.BurpDuration,1.4f),"Arroto reduzido a 1,4 segundo");StartMovie();
        yield return Until(()=>combat.IsVulnerable,"janela do arroto");
        float burpStart=Time.time;
        Check(boss.Health.IncomingDamageMultiplier==2,"Arroto continua dobrando dano");Capture("burp-opening.png");
        yield return Until(()=>!combat.IsVulnerable,"fim do arroto");
        Check(Time.time-burpStart<1.48f,"Janela real do arroto menor que 1,48s");
        Check(boss.Health.IncomingDamageMultiplier==1,"Multiplicador restaurado ao terminar arroto");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"salto de perseguição");
        Vector2 locked=combat.LockedLanding;
        Check(Mathf.Abs(locked.x-rb.position.x)<.15f,"Destino do salto usa a posição do jogador");Capture("pursuit-warning.png");
        rb.position=new Vector2(area.xMax-3,area.yMax-1.1f);yield return Wait(.2f);
        Check(combat.LockedLanding==locked,"Mira do salto permanece travada depois do aviso");
        int low=combat.LowWaves,high=combat.HighWaves,drops=combat.DropsLaunched;
        yield return Until(()=>combat.LowWaves>low,"onda baixa");
        Check(combat.HighWaves==high && combat.DropsLaunched-drops==3,"Primeira onda tem três gotas rápidas e baixas");
        var lows=Object.FindObjectsByType<LakeDroplet>();
        Check(lows.Any(d=>Mathf.Abs(d.Velocity.x)>=4.7f && d.Velocity.y<4.3f),"Velocidade horizontal caracteriza onda baixa");
        Capture("low-wave.png");yield return Until(()=>combat.HighWaves>high,"onda alta atrasada");
        Check(combat.DropsLaunched-drops==5,"Segunda onda completa cinco gotas sem aumentar volume");
        Check(Object.FindObjectsByType<LakeDroplet>().Any(d=>d.Velocity.y>8),"Gotas altas sobem mais e cruzam devagar");Capture("high-wave.png");
        yield return Until(()=>combat.Tongue.CampingFlies>0,"mosca contra camping");
        Check(Enumerable.Range(0,combat.Tongue.FlyCount).Any(i=>combat.Tongue.Fly(i).Present && combat.Tongue.Fly(i).Anchor.y>area.center.y),"Mosca pressiona corredor alto");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float && !combat.HasWaterDrops,"intervalo para fúria");
        boss.Health.TakeDamage(boss.Health.MaxHealth/2);yield return Until(()=>combat.Companion.Joined,"entrada da Saperereca");
        var wife=combat.Companion;Check(combat.Enraged && wife.Entries==1,"Fúria acontece uma única vez na metade da vida");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Dive,"mergulho");Capture("saperereca-dive.png");
        Vector2 before=wife.Position;yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Swim,"nado submerso");
        Check(!wife.Visual.enabled && wife.Position.y<combat.WaterY-.8f,"Sapa desaparece sob a superfície");
        Check(Object.FindAnyObjectByType<LakeDiveBubbles>()!=null,"Bolhas avisam o ponto de emergência");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Warning,"aviso da rajada");
        Vector2 shotAim=wife.LockedAim;rb.position=new Vector2(area.xMin+3,area.yMax-1.1f);
        yield return Wait(.15f);Check(wife.LockedAim==shotAim,"Rajada trava mira durante o aviso");Capture("locked-volley-warning.png");
        float pauseClock=wife.Clock;Vector2 pausePosition=wife.Position;Time.timeScale=0;
        double until=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<until)yield return null;
        Check(wife.Clock==pauseClock && wife.Position==pausePosition,"Pausa congela mergulho e rajada");Time.timeScale=1;
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Spit && wife.Shots%3==2,"tiro central");
        Vector2 intercept=wife.LastLaunch+wife.LastVelocity*wife.LastTravel+Vector2.down*(.5f*LakeDroplet.Gravity*wife.LastTravel*wife.LastTravel);
        Check(Vector2.Distance(intercept,shotAim)<.05f,"Balística central alcança a posição alta avisada");Capture("aimed-volley.png");
        yield return Until(()=>wife.Shots>=3 && wife.CurrentState==SapererecaCompanion.State.Rest,"fim da rajada");
        Check(wife.Shots%3==0,"Rajada contém três tiros espaçados");
        yield return Until(()=>wife.Combos>0 && combat.Tongue.Busy,"combinação com língua");
        int comboShots=wife.Shots;Check(wife.CurrentState==SapererecaCompanion.State.Dive,"Mergulho começa enquanto língua prepara ataque");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Ready,"sapa aguarda língua recolher");
        Check(wife.Shots==comboShots,"Preparação conjunta não dispara durante língua");Capture("rage-combo.png");
        yield return Until(()=>wife.Shots>comboShots,"rajada após língua");
        Check(!combat.Tongue.Busy,"Rajada só sai depois da língua recolhida");
        int hp=player.Health.CurrentHealth;
        Check(!wife.TryHit() && player.Health.CurrentHealth==hp,"Dano da sapa respeita proteção do jogador");
        yield return Wait(.7f);
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),false);
        var dash=player.GetComponent<PlayerDash>();Check(dash.TryDash(),"Dash real disponível durante rajada");
        var contact=new GameObject("QA_WaterContact",typeof(Rigidbody2D),typeof(SpriteRenderer));contact.transform.SetParent(wife.transform.Find("SapererecaWaterThreats"),false);
        contact.AddComponent<LakeDroplet>().Configure(combat,player.Health,combat.DropletSprite,rb.position,Vector2.right*.1f,combat.WaterY,wife);
        yield return Until(()=>contact==null,"gota consumida na esquiva");
        Check(player.Health.CurrentHealth==hp,"Contato real de gota durante dash não causa dano");
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);yield return Wait(.8f);
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),false);
        contact=new GameObject("QA_WaterContact",typeof(Rigidbody2D),typeof(SpriteRenderer));contact.transform.SetParent(wife.transform.Find("SapererecaWaterThreats"),false);
        contact.AddComponent<LakeDroplet>().Configure(combat,player.Health,combat.DropletSprite,rb.position,Vector2.right*.1f,combat.WaterY,wife);
        yield return Until(()=>contact==null,"gota atinge sem proteção");
        Check(player.Health.CurrentHealth==hp-1,"Contato real sem dash causa exatamente um dano");
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);
        int rageLow=combat.LowWaves,rageHigh=combat.HighWaves,rageDrops=combat.DropsLaunched;
        yield return Until(()=>combat.LowWaves>rageLow,"onda baixa da fúria");
        Check(combat.DropsLaunched-rageDrops==4,"Fúria mantém quatro gotas na onda baixa");
        yield return Until(()=>combat.HighWaves>rageHigh,"onda alta da fúria");
        Check(combat.DropsLaunched-rageDrops==7,"Fúria limita duas ondas a sete gotas");
        combat.StopCombat();yield return Frames(4);
        Check(wife.CurrentState==SapererecaCompanion.State.Stopped && !wife.Visual.enabled,"Encerramento cancela sapa e visual");
        Check(Object.FindObjectsByType<LakeDiveBubbles>().Length==0 && Object.FindObjectsByType<LakeDroplet>().Length==0,"Encerramento limpa bolhas e projéteis pendentes");
        Check(!combat.Tongue.Busy && !combat.HasWaterDrops,"Sem ataques atrasados depois de encerrar");StopMovie();
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
        Directory.CreateDirectory("Docs/ArtPreviews/SapeneuCururu_Difficulty_v21");
        File.WriteAllBytes("Docs/ArtPreviews/SapeneuCururu_Difficulty_v21/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v21/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v21/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v21/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v21/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}


