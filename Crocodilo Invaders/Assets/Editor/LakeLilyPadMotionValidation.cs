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
public static class LakeLilyPadMotionValidation
{
    private const string Key = "Crocodilo.Lake.LilyPadMotionValidation";
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
    static LakeLilyPadMotionValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v19/TestSave");
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
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeLilyPadMotionValidation),true);
        yield return Until(()=>waves.ActiveBoss!=null && waves.ActiveBoss.Ready,"entrada do boss");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var tongue=combat.Tongue;var body=player.GetComponent<Rigidbody2D>();var hud=Object.FindAnyObjectByType<LakeBossBar>();
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"flutuacao");
        var lake=Object.FindAnyObjectByType<LakeBackdrop>();var wife=combat.Companion;
        var pads=lake.GetComponent<LakeLilyPads>();
        Check(pads!=null && pads.Count==8,"Oito vitorias-regias na cena real");
        Check(Enumerable.Range(0,8).All(i=>pads.Pad(i).GetComponent<Collider2D>()==null),"Folhas decorativas nao bloqueiam nave ou tiros");
        Check(pads.Pad(0).sortingOrder<wife.Visual.sortingOrder && pads.Pad(0).sortingOrder>-180,"Folha de apoio acima da agua e sob a Saperereca");
        var startPads=Enumerable.Range(0,8).Select(i=>pads.Pad(i).transform.position).ToArray();yield return Wait(1.5f);
        Check(Vector2.Distance(pads.Pad(0).transform.position,startPads[0])>.03f,"Folha de apoio deriva no lago");
        Check(Enumerable.Range(2,6).All(i=>pads.Pad(i).transform.position.x<startPads[i].x-.03f),"Folhas decorativas seguem a corrente");
        var pausedPads=Enumerable.Range(0,8).Select(i=>pads.Pad(i).transform.position).ToArray();
        var pausedAngles=Enumerable.Range(0,8).Select(i=>pads.Pad(i).transform.rotation).ToArray();
        Time.timeScale=0;double stop=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<stop)yield return null;
        Check(Enumerable.Range(0,8).All(i=>pads.Pad(i).transform.position==pausedPads[i] && pads.Pad(i).transform.rotation==pausedAngles[i]),"Pausa congela deriva e balanco de todas as folhas");Time.timeScale=1;
        float padY=pads.Pad(0).transform.position.y;yield return Wait(.3f);
        Check(Mathf.Abs(pads.Pad(0).transform.position.y-padY)>.001f,"Folhas oscilam suavemente na superficie");
        body.position=new Vector2(-3,1);yield return Frames(3);StartMovie();yield return Wait(1.1f);
        Check(hud.CompanionInHud && hud.CompanionIcon.rectTransform.sizeDelta.x==44 && hud.CompanionIcon.rectTransform.anchoredPosition==new Vector2(386,110),"Saperereca pequena sentada no topo do pneu");
        Check(hud.CompanionIcon.transform.GetSiblingIndex()>hud.CompanionIcon.transform.parent.Find("LakeArtwork").GetSiblingIndex(),"Enfeite aparece em cima da arte do pneu");
        hud.Title.ForceMeshUpdate();Check(hud.Title.rectTransform.rect.width>=220 && !hud.Title.isTextOverflowing && hud.Title.textInfo.lineCount==1,"Nome inteiro cabe na placa larga em uma linha");
        Check(tongue.FlyCount==3 && tongue.AliveFlyCount>=1 && tongue.AliveFlyCount<=3,"Reserva de no maximo tres moscas");
        Check(Object.FindObjectsByType<LakeTongueFly>().Length==3,"Moscas reutilizam apenas tres objetos");
        var fly=Enumerable.Range(0,3).Select(i=>tongue.Fly(i)).FirstOrDefault(f=>f.Present && !f.Focused);
        if(fly!=null){Vector2 point=fly.transform.position;yield return Wait(.2f);Check(Vector2.Distance(point,fly.transform.position)>.02f,"Mosca fora do aviso tambem voa pelo mapa");}
        Capture("hud-ornament.png");
        float full=hud.Water.FilledWidth,height=hud.Water.SurfaceSample;int spills=hud.Spills;
        int hp=boss.Health.CurrentHealth;boss.Health.TakeDamage(30);yield return Wait(.2f);
        Check(boss.Health.CurrentHealth==hp-30,"Dano normal preservado antes do arroto");
        Check(hud.Water.FilledWidth<full && hud.Water.FilledWidth>full*.7f,"Vida esvazia horizontalmente pelo comprimento");
        Check(Mathf.Abs(hud.Water.SurfaceSample-height)<2,"Altura da agua permanece constante ao perder HP");
        Check(hud.Spills==spills+1 && hud.IsPouring && hud.GetComponentInChildren<LakeHudSpill>().Outlet.x<335,"Jato sai da ponta horizontal que recua");Capture("horizontal-water-spill.png");
        bool one=true;int spawned=tongue.FliesSpawned;
        yield return Until(()=>{if(tongue.AgitatingCount>1)one=false;return combat.IsVulnerable;},"arroto natural apos limpar moscas");
        Check(one && tongue.AliveFlyCount==0 && tongue.CapturedFlies>=1,"Uma mosca marcada por vez e todas consumidas antes do arroto");
        Check(tongue.MealPending && !tongue.Busy && combat.Burps==1,"Limpar tela inicia uma janela de arroto");
        var visual=boss.GetComponentInChildren<SpriteRenderer>();var burp=Resources.Load<Sprite>("SapeneuBurpV13/burp");
        Check(visual.sprite==burp && burp!=null,"Asset do arroto visivel");
        Check(burp.pixelsPerUnit==UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LakeStage/sapo-master.png").pixelsPerUnit,"Pose do arroto registrada na mesma escala");
        Check(boss.Health.IncomingDamageMultiplier==2,"Arroto dobra dano recebido");
        hp=boss.Health.CurrentHealth;boss.Health.TakeDamage(2);Check(boss.Health.CurrentHealth==hp-4,"Dois de dano viram quatro durante vulnerabilidade");
        yield return Frames(2);Check(hud.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.enabled && t.text.Contains("DANO x2")),"HUD sinaliza abertura de dano dobrado");Capture("burp-vulnerable.png");
        float clock=tongue.Clock,hudClock=hud.AnimationClock;float multiplier=boss.Health.IncomingDamageMultiplier;
        Time.timeScale=0;double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(tongue.Clock==clock && hud.AnimationClock==hudClock && combat.IsVulnerable && boss.Health.IncomingDamageMultiplier==multiplier,"Pausa congela janela de vulnerabilidade");Time.timeScale=1;
        int strikes=tongue.Strikes,landings=combat.Landings;spawned=tongue.FliesSpawned;
        yield return Until(()=>!combat.IsVulnerable,"fim do arroto");
        Check(tongue.Strikes==strikes && combat.Landings==landings && tongue.FliesSpawned==spawned,"Sem ataques ou reposicao de moscas durante arroto");
        Check(boss.Health.IncomingDamageMultiplier==1 && visual.sprite.name=="sapo-master","Fim da janela restaura pose e defesa normais");
        hp=boss.Health.CurrentHealth;boss.Health.TakeDamage(2);Check(boss.Health.CurrentHealth==hp-2,"Dano volta ao valor normal depois do arroto");
        yield return Wait(1);Check(tongue.AliveFlyCount==0 && tongue.FliesSpawned==spawned,"Moscas nao renascem imediatamente depois de comidas");
        yield return Until(()=>tongue.FliesSpawned>spawned,"nova mosca gradual");
        Check(tongue.AliveFlyCount==1,"Nova leva comeca por uma mosca apenas");
        var fresh=Enumerable.Range(0,3).Select(i=>tongue.Fly(i)).First(f=>f.Present);var anchor=fresh.Anchor;
        body.position=new Vector2(-5,2);yield return Wait(.25f);
        Check(fresh.Anchor==anchor && Vector2.Distance(anchor,body.position)>.9f,"Nova mosca aparece no mapa sem se colar atras da nave");Capture("roaming-flies.png");
        boss.Health.TakeDamage(boss.Health.CurrentHealth-boss.Health.MaxHealth/2);yield return Until(()=>hud.CompanionJumping,"entrada da parceira maior");yield return Wait(.4f);Capture("larger-hud-jump.png");
        yield return Until(()=>wife.Joined,"parceira no mundo");yield return Frames(3);
        Check(Vector2.Distance(wife.Position,pads.Support(.46f))<.1f,"Pes da Saperereca apoiados na folha de entrada");
        Check(wife.transform.localScale.x>1.6f && wife.Visual.bounds.size.y>1.5f,"Saperereca entra bem maior na arena");
        Check(!hud.CompanionIcon.enabled && !hud.CompanionInHud && wife.Entries==1,"Enfeite sai do HUD sem duplicar personagem");Capture("larger-saperereca.png");
        yield return Until(()=>wife.Hops>0 && wife.Shots>=3,"apoio da parceira maior");
        Check(wife.Shots>=3 && combat.Enraged,"Parceira maior continua ajudando com agua");
        float observeUntil=Time.time+2;float maxFootError=0;
        while(Time.time<observeUntil)
        {
            if(wife.CurrentState!=SapererecaCompanion.State.Jump && wife.CurrentState!=SapererecaCompanion.State.Entering)
                maxFootError=Mathf.Max(maxFootError,Mathf.Min(Vector2.Distance(wife.Position,pads.Support(.46f)),Vector2.Distance(wife.Position,pads.Support(.60f))));
            yield return null;
        }
        Check(maxFootError<.1f,"Saperereca acompanha a folha movel sem escorregar");
        float distance=Mathf.Min(Vector2.Distance(wife.Position,pads.Support(.46f)),Vector2.Distance(wife.Position,pads.Support(.60f)));
        Check(distance<.1f,"Salto termina apoiado em outra vitoria-regia");Capture("saperereca-on-lilypad.png");
        yield return Until(()=>combat.IsVulnerable,"arroto com casal na arena");int shots=wife.Shots;float wifeClock=wife.Clock;yield return Wait(.25f);
        Check(wife.Shots==shots && wife.Clock==wifeClock,"Parceira tambem da espaco durante abertura do arroto");Capture("couple-burp.png");StopMovie();
        var flow=Object.FindAnyObjectByType<ChapterFlow>();flow.enabled=false;boss.Health.TakeDamage(9999);yield return Frames(5);
        Check(!combat.IsVulnerable && boss.Health.IncomingDamageMultiplier==1 && tongue.FlyCount==0,"Derrota no arroto restaura multiplicador e limpa moscas");
        Check(wife.CurrentState==SapererecaCompanion.State.Stopped && !wife.Visual.enabled,"Derrota limpa parceira maior");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Progresso real preservado");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_LilyMotion_v19");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_LilyMotion_v19/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v19/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v19/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v19/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v19/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}

