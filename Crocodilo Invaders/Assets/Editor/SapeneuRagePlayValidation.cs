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
public static class SapeneuRagePlayValidation
{
    private const string Key = "Crocodilo.Lake.RageValidation";
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
    static SapeneuRagePlayValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v12/TestSave");
        var menu=Object.FindAnyObjectByType<DeveloperMenuView>();
        if(menu!=null)
        {
            yield return Until(()=>{menu.Open();return menu.IsVisible;},"menu DEV");
            GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"loading");
            Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
            yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss!=null,"boss real");
        }
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(SapeneuRagePlayValidation),true);
        yield return Until(()=>waves.ActiveBoss!=null && waves.ActiveBoss.Ready,"entrada do boss");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var tongue=combat.Tongue;var body=player.GetComponent<Rigidbody2D>();var hud=Object.FindAnyObjectByType<LakeBossBar>();
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"flutuacao");
        var lake=Object.FindAnyObjectByType<LakeBackdrop>();var wife=combat.Companion;
        Check(boss.DisplayName=="SAPENEU CURURU","Nome novo no boss real");
        Check(hud.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="SAPENEU CURURU"),"Nome novo na HUD");
        Check(wife!=null && wife.CurrentState==SapererecaCompanion.State.InHud && !wife.Visual.enabled,"Saperereca aguarda no HUD sem duplicar personagem no mundo");
        Check(hud.CompanionInHud && !hud.CompanionIcon.raycastTarget,"Saperereca simples e visivel no HUD sem bloquear controles");
        var art=Resources.Load<SapererecaAssets>("SapererecaV12/art");Check(art!=null && art.frames.Length==6,"Seis poses da Saperereca importadas");
        var reeds=lake.transform.Find("Layer_7").GetComponentsInChildren<SpriteRenderer>();
        Check(reeds.Length==3 && reeds.All(r=>r.sortingLayerName=="LakeForeground" && r.sortingOrder==32000),"Mato e repeticoes na camada frontal dedicada");
        int top=SortingLayer.GetLayerValueFromID(reeds[0].sortingLayerID);
        Check(Object.FindObjectsByType<Renderer>().Where(r=>!reeds.Contains(r)).All(r=>SortingLayer.GetLayerValueFromID(r.sortingLayerID)<top || (r.sortingLayerID==reeds[0].sortingLayerID && r.sortingOrder<32000)),"Mato esta acima de todos os renderers do mundo");
        Check(hud.GetComponentInParent<Canvas>().renderMode==RenderMode.ScreenSpaceOverlay,"Apenas HUD e UI ficam acima do mato");
        body.position=new Vector2(-3,1);yield return Frames(3);StartMovie();
        Check(movieCanvases.All(c=>c.sortingLayerName=="LakeHUD" && SortingLayer.GetLayerValueFromID(c.sortingLayerID)>top),"HUD permanece acima do mato tambem em captura com camera");
        yield return Wait(.25f);Capture("saperereca-hud.png");
        boss.Health.TakeDamage(boss.Health.MaxHealth/2-1);yield return Wait(.15f);
        Check(!combat.Enraged && wife.Entries==0 && hud.CompanionInHud,"Acima de 50 por cento nao inicia furia");
        boss.Health.TakeDamage(1);yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Rage,"furia na metade exata");
        Check(boss.Health.CurrentHealth==boss.Health.MaxHealth/2 && combat.Enraged && wife.Entries==1,"Metade exata dispara furia uma vez");
        Check(hud.IsCritical && hud.CompanionJumping && wife.CurrentState==SapererecaCompanion.State.Entering,"HUD e entrada respondem a mesma transicao");
        Check(!tongue.Busy && Object.FindObjectsByType<LakeDroplet>().Length==0,"Furia interrompe golpe anterior sem perigos sobrepostos");
        var start=hud.CompanionIcon.rectTransform.anchoredPosition;yield return Wait(.35f);
        Check(hud.CompanionIcon.rectTransform.anchoredPosition!=start && !wife.Visual.enabled,"A mesma personagem salta visualmente do HUD antes de aparecer no mundo");Capture("hud-jump.png");
        float clock=wife.Clock,hudClock=hud.AnimationClock;Vector2 pos=hud.CompanionIcon.rectTransform.anchoredPosition;
        Time.timeScale=0;double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(wife.Clock==clock && hud.AnimationClock==hudClock && hud.CompanionIcon.rectTransform.anchoredPosition==pos,"Pausa congela furia e salto do HUD");Time.timeScale=1;
        yield return Until(()=>wife.Joined && wife.Visual.enabled,"entrada da Saperereca");yield return Wait(.1f);
        Check(!hud.CompanionInHud && !hud.CompanionJumping && !hud.CompanionIcon.enabled,"Entrada remove a personagem do HUD");
        Check(Object.FindObjectsByType<SapererecaCompanion>().Length==1 && wife.Entries==1,"Ha apenas uma Saperereca no encontro");
        Check(wife.GetComponent<Health>()==null && wife.GetComponent<Collider2D>()==null,"Parceira compartilha encontro sem criar segunda barra ou bloquear tiros");
        Check(wife.Visual.sharedMaterial.shader.name=="Crocodilo/LakeWaterContact","Saperereca integrada a superficie com recorte e reflexo");Capture("saperereca-arrival.png");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Warning,"aviso do primeiro salto");
        Vector2 locked=wife.LockedAim;Check(wife.Shots==0 && wife.Hops==0,"Salto da parceira tem aviso proprio antes do ataque");
        body.position=new Vector2(-3,2);yield return Wait(.15f);Check(wife.LockedAim==locked,"Mira fixa permite escapar durante aviso");
        Check(!combat.TryBeginTongue(1),"Sapeneu aguarda ataque da parceira");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Jump && wife.PhaseTime>.3f,"salto da parceira");
        Check(wife.Visual.sprite==art.frames[2] && wife.Position.y>combat.WaterY+1,"Pose de salto acompanha arco acima da agua");Capture("saperereca-hop.png");
        yield return Until(()=>wife.Shots==3,"rajada de apoio");
        Check(wife.Hops==1 && wife.ActiveDrops==3,"Aterrissagem dispara tres gotas de apoio");
        Check(Object.FindObjectsByType<LakeDroplet>().All(d=>d.Drawing==combat.DropletSprite),"Gotas da parceira usam a mesma agua do lago");
        var drop=Object.FindObjectsByType<LakeDroplet>().First();var dropBody=drop.GetComponent<Rigidbody2D>();
        Vector2 intercept=dropBody.position+drop.Velocity*.15f+Vector2.down*(.5f*LakeDroplet.Gravity*.15f*.15f);
        int hp=player.Health.CurrentHealth;player.Health.SetInvulnerable(typeof(SapeneuRagePlayValidation),false);body.position=intercept;
        yield return Until(()=>player.Health.CurrentHealth<hp,"gota da Saperereca acerta");
        Check(player.Health.CurrentHealth==hp-1,"Gota da parceira causa um HP");player.Health.SetInvulnerable(typeof(SapeneuRagePlayValidation),true);body.position=new Vector2(-3,2);
        Capture("support-water.png");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tongue,"linguada apos apoio");
        Check(tongue.SequenceLength==4 && wife.CurrentState==SapererecaCompanion.State.Rest && wife.ActiveDrops==0,"Sapeneu usa quatro alvos em furia depois da rajada");
        boss.Health.Heal(999);boss.Health.TakeDamage(boss.Health.MaxHealth/2);yield return Wait(.1f);
        Check(wife.Entries==1 && combat.Enraged && Object.FindObjectsByType<SapererecaCompanion>().Length==1,"Curar e cruzar metade novamente nao duplica entrada");
        yield return Until(()=>combat.Landings>0,"salto do Sapeneu em furia");
        Check(combat.DropsLaunched==7,"Sapeneu mantem sete gotas em furia");yield return Wait(.3f);Capture("couple-gameplay.png");StopMovie();
        var flow=Object.FindAnyObjectByType<ChapterFlow>();flow.enabled=false;boss.Health.TakeDamage(9999);yield return Frames(5);
        Check(wife.CurrentState==SapererecaCompanion.State.Stopped && !wife.Visual.enabled && wife.ActiveDrops==0 && !hud.CompanionIcon.enabled,"Derrota do Sapeneu limpa parceira, HUD e ataques");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Teste preserva progresso real");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"novo menu");
        menu=Object.FindAnyObjectByType<DeveloperMenuView>();yield return Until(()=>{menu.Open();return menu.IsVisible;},"novo DEV");
        GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"novo loading");Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"novo boss");
        player=Object.FindAnyObjectByType<Player>();player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);
        waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();hud=Object.FindAnyObjectByType<LakeBossBar>();
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"nova flutuacao");boss.Health.TakeDamage(boss.Health.MaxHealth/2);
        yield return Until(()=>hud.CompanionJumping,"novo salto do HUD");wife=combat.Companion;player.Health.SetInvulnerable(false);player.Health.TakeDamage(9999);yield return Frames(5);
        Check(wife.CurrentState==SapererecaCompanion.State.Stopped && !hud.CompanionJumping && !hud.CompanionIcon.enabled,"Morte do jogador durante entrada cancela salto sem personagem fantasma");
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
        Directory.CreateDirectory("Docs/ArtPreviews/SapeneuCururu_Rage_v12");
        File.WriteAllBytes("Docs/ArtPreviews/SapeneuCururu_Rage_v12/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v12/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v12/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v12/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v12/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
