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
public static class LakeReworkBossValidation
{
    private const string Key = "Crocodilo.Lake.ReworkBossValidation";
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
    static LakeReworkBossValidation()
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
        LakeReworkArtTools.Install();
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v26/Boss/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"boss pronto");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var rb=player.GetComponent<Rigidbody2D>();var water=Object.FindAnyObjectByType<LakeWildlife>();var art=Resources.Load<LakeReworkArt>("LakeReworkV26/art");
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),true);
        rb.position=new Vector2(combat.Arena.xMin+3,2);StartMovie();
        Check(art!=null && art.rightWings.Length==8 && art.angryBoss!=null && art.surfaceEyes!=null,"Novos assets e olhos carregados");
        Check(water.ActivePiranhas==10,"Piranhas já residem no lago antes de aproximar");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"primeiro salto");
        Check(combat.LockedLanding.x>=combat.LeftLimit,"Salto mirando jogador da esquerda limitado ao meio");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tire,"primeiro pneu");
        yield return Wait(.72f);Check(combat.transform.position.x>=combat.LeftLimit-.01f,"Boss permanece no meio ou na direita");
        var tire=combat.Tire;Check(tire.Held.enabled && tire.Held.transform.position.y>combat.WaterY,"Pneu emerge na mão antes de lançar");Capture("tire-in-hand.png");
        float heldWidth=tire.Held.sprite.bounds.size.x*tire.Held.transform.lossyScale.x;Vector2 aim=tire.LockedAim;
        yield return Until(()=>tire.Projectile!=null,"pneu sai da mão");
        var wheel=tire.Projectile;Check(Vector2.Distance(wheel.transform.position,tire.LaunchPoint)<.5f,"Primeiro quadro do pneu nasce na mão, longe do centro padrão");
        Check(Mathf.Abs(wheel.GetComponent<SpriteRenderer>().sprite.bounds.size.x*wheel.transform.lossyScale.x-heldWidth)<.35f,"Tamanho do pneu contínuo entre mão e voo");
        Check(Vector2.Dot(wheel.Velocity.normalized,(aim-tire.LaunchPoint).normalized)>.999f,"Trajetória ainda usa mira fixa");
        yield return Wait(.12f);Capture("tire-release.png");float age=wheel.Clock;Vector2 fixedFish=water.Piranha(0).Position;
        Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(wheel.Clock==age && water.Piranha(0).Position==fixedFish,"Pausa congela pneu e fauna");Time.timeScale=1;
        boss.Health.TakeDamage(101);yield return Wait(.3f);
        Check(combat.CurrentState==SapoDoPneuCombat.State.Rage && combat.Companion.Entries==0,"Raiva precede salto da Saperereca");
        Check(Object.FindAnyObjectByType<LakeBossBar>().CompanionInHud,"Sapa ainda na HUD durante aviso de raiva");
        Check(combat.GetComponentInChildren<SpriteRenderer>().sprite==art.angryBoss,"Expressão furiosa real do Sapeneu");Capture("rage-warning.png");
        yield return Until(()=>combat.Companion.CurrentState==SapererecaCompanion.State.Entering,"salto após aviso");yield return Wait(.35f);Capture("wife-joins.png");
        yield return Until(()=>combat.Companion.CurrentState==SapererecaCompanion.State.Warning && combat.Companion.EyesVisible,"olhos indicam origem da sapa");
        var wife=combat.Companion;Check(!wife.Visual.enabled && Mathf.Abs(wife.EyesPosition.x-wife.Landing.x)<.05f,"Olhos na superfície apontam ponto exato de emergência");
        Check(!wife.GetComponentsInChildren<Transform>().Any(t=>t.name=="LakeLandingWarning"),"Saperereca sem marcação circular azul");Capture("eyes-warning.png");
        yield return Until(()=>wife.Shots>0 && wife.ActiveDrops>0,"cuspida compacta da sapa");
        var shot=wife.GetComponentsInChildren<LakeDroplet>()[0];
        Check(shot.Drawing==Resources.Load<Sprite>("LakeJourneyV22/water-drop"),"Sapa usa mesma gota dos sapinhos");
        Check(shot.GetComponent<SpriteRenderer>().bounds.size.y<.65f,"Tiro compacto, sem gota gigante");
        Check(Vector2.Distance(LakeDroplet.Point(wife.LastLaunch,wife.LastVelocity,wife.LastTravel),wife.LockedAim)<.7f,"Arco cruza mira travada com espalhamento pequeno");
        Check(Vector2.Distance(wife.LastLaunch,wife.Position)<1.3f,"Cuspida parte da boca da Saperereca");yield return Wait(.2f);Capture("wife-spits.png");
        combat.StopCombat();yield return Frames(4);Check(!tire.Busy && wife.ActiveDrops==0 && !wife.EyesVisible,"Fim do boss limpa pneu, olhos e gotas");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),false);
        rb.position=new Vector2(combat.Arena.xMin+3,combat.WaterY-.8f);int hp=player.Health.CurrentHealth,bites=water.Bites;
        yield return Until(()=>water.Bites>bites,"piranhas nadam e mordem jogador submerso");
        Check(water.WaterDangerActive && player.Health.CurrentHealth==hp-1,"Mordida real na água causa um HP");
        Check(GameObject.Find("PiranhaSurfaceWarning")==null,"Perseguição sem marca circular");Capture("piranhas-hunt.png");
        float at=Time.time;var previous=Enumerable.Range(0,10).Select(i=>water.Piranha(i).Position).ToArray();float maxStep=0;
        while(Time.time-at<.5f)
        {
            float before=Time.fixedTime;yield return Frames(1);
            if(Time.fixedTime==before)continue;
            for(int i=0;i<10;i++){maxStep=Mathf.Max(maxStep,Vector2.Distance(previous[i],water.Piranha(i).Position));previous[i]=water.Piranha(i).Position;}
        }
        Check(maxStep<.65f,"Piranhas se deslocam continuamente, sem reposição perto do jogador");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),true);
        rb.position=new Vector2(rb.position.x,combat.WaterY+.35f);int leaps=water.Launches;
        yield return Until(()=>water.Launches>leaps,"bote ao aproximar da superfície");Capture("piranhas-leap.png");
        rb.position=new Vector2(rb.position.x,2);yield return Wait(1.4f);
        Check(!water.WaterDangerActive && Enumerable.Range(0,10).All(i=>water.Piranha(i).Active),"Subir encerra perseguição sem sumir com peixes");
        int ambient=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>ambient,"salto ambiente raro");int after=water.AmbientJumps;yield return Wait(3);
        Check(water.AmbientJumps==after,"Saltos ambientes espaçados, sem sequência frequente");Capture("ambient-piranhas.png");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),false);player.Health.TakeDamage(999);yield return Frames(5);
        Check(water.ActivePiranhas==0 && waves.Phase==WaveController.RunPhase.Failed,"Derrota desativa dano e fauna agressiva");StopMovie();
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_Rework_v26/Boss");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_Rework_v26/Boss/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v26/Boss/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v26/Boss/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v26/Boss/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v26/Boss/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
