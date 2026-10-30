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
public static class LakeWildlifePlayValidation
{
    private const string Key = "Crocodilo.Lake.WildlifeValidation";
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
    static LakeWildlifePlayValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v11/TestSave");
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
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeWildlifePlayValidation),true);
        yield return Until(()=>waves.ActiveBoss!=null && waves.ActiveBoss.Ready,"entrada do boss");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var tongue=combat.Tongue;var body=player.GetComponent<Rigidbody2D>();var hud=Object.FindAnyObjectByType<LakeBossBar>();
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"flutuacao");
        var wildlife=Object.FindAnyObjectByType<LakeWildlife>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
        Check(wildlife!=null && wildlife.AmbientFishCount==8,"Oito peixes decorativos na fase 2 real");
        var art=Resources.Load<LakeWildlifeAssets>("LakeWildlifeV11/art");
        Check(art.fishFrames.Length==4 && art.piranhaFrames.Length==6,"Arte nativa com nado e mordida");
        Check(Enumerable.Range(0,8).All(i=>wildlife.AmbientFish(i).GetComponent<Collider2D>()==null),"Peixes de fundo nao colidem");
        var reeds=lake.transform.Find("Layer_7").GetComponentsInChildren<SpriteRenderer>();
        Check(reeds.Length==3 && reeds.All(r=>r.sortingLayerName=="LakeForeground" && r.sortingOrder==32000),"Mato e duas repeticoes acima do mundo");
        Check(wildlife.AmbientFish(0).sortingOrder>-190 && wildlife.AmbientFish(0).sortingOrder<-180,"Peixes entre base e reflexos da agua");
        body.position=new Vector2(-3,lake.WaterY+2);yield return Wait(.2f);
        float x0=wildlife.AmbientFish(0).transform.position.x,x1=wildlife.AmbientFish(1).transform.position.x;
        yield return Wait(.5f);
        Check(wildlife.AmbientFish(0).transform.position.x>x0 && wildlife.AmbientFish(1).transform.position.x<x1,"Peixes nadam nos dois sentidos");
        Check(!wildlife.WaterDangerActive && wildlife.Launches==0,"Nave acima da agua nao atrai piranhas");
        // Isola dano de piranha; o controlador do boss permanece no prefab original.
        combat.enabled=false;StartMovie();
        body.position=new Vector2(-3,lake.WaterY+.25f);yield return Until(()=>wildlife.WaterDangerActive,"entrada na agua");
        yield return Wait(.22f);
        Check(wildlife.Warnings==1 && wildlife.Launches==0 && wildlife.ActivePiranhas==5,"Grupo de cinco antecipa antes do primeiro bote");
        Check(GameObject.Find("PiranhaSurfaceWarning")!=null,"Agitacao visivel na superficie");Capture("surface-warning.png");
        float clock=wildlife.Clock;Vector3 fishPos=wildlife.AmbientFish(0).transform.position;Vector2 predatorPos=wildlife.Piranha(0).Position;
        Time.timeScale=0;double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(clock==wildlife.Clock && fishPos==wildlife.AmbientFish(0).transform.position && predatorPos==wildlife.Piranha(0).Position,"Pausa congela peixes e antecipacao");Time.timeScale=1;
        body.position=new Vector2(-3,lake.WaterY+1.5f);yield return Wait(.75f);
        Check(!wildlife.WaterDangerActive && wildlife.ActivePiranhas==0 && wildlife.Launches==0,"Subir durante aviso cancela todo o grupo");
        Check(GameObject.Find("PiranhaSurfaceWarning")==null,"Aviso cancelado nao permanece na agua");
        body.position=new Vector2(-3,lake.WaterY+.25f);yield return Until(()=>wildlife.Launches>0,"primeiro bote");yield return Wait(.18f);
        Check(wildlife.Launches>=2 && wildlife.Launches<5,"Botes saem em sequencia");
        bool airborne=false,open=false;float endTime=Time.time+1;
        while(Time.time<endTime)
        {
            for(int i=0;i<10;i++)if(wildlife.Piranha(i).Active)
            {
                var pred=wildlife.Piranha(i);if(pred.Position.y>lake.WaterY+.1f)airborne=true;
                if(pred.GetComponent<SpriteRenderer>().sprite.name=="piranha_4")open=true;
            }
            if(airborne && open)break;yield return null;
        }
        Check(airborne && open,"Piranhas saltam acima da agua com boca aberta");Capture("piranha-jump.png");
        Check(player.Health.CurrentHealth==player.Health.MaxHealth,"Invulnerabilidade impede dano das piranhas");
        player.Health.SetInvulnerable(typeof(LakeWildlifePlayValidation),false);int hp=player.Health.CurrentHealth;
        yield return Until(()=>wildlife.Bites>0,"mordida real");int hitHp=player.Health.CurrentHealth;
        Check(hitHp==hp-1,"Mordida causa um HP");yield return Wait(.25f);
        Check(player.Health.CurrentHealth==hitHp,"Grupo nao acumula mordidas instantaneas");
        Check(wildlife.Packs>=2 && wildlife.ActivePiranhas<=10,"Ficar baixo repete grupos com reserva limitada");
        body.position=new Vector2(-3,lake.WaterY+1.5f);yield return Wait(.1f);hp=player.Health.CurrentHealth;int launches=wildlife.Launches;
        Check(!wildlife.WaterDangerActive && !wildlife.CanHurt,"Subir desativa perigo imediatamente");yield return Wait(1.5f);
        Check(wildlife.ActivePiranhas==0 && wildlife.Launches==launches && player.Health.CurrentHealth==hp,"Piranhas voltam a agua sem perseguir nave alta");Capture("safe-above-water.png");
        int bites=wildlife.Bites;body.position=new Vector2(-3,lake.WaterY-1);yield return Until(()=>wildlife.Bites>bites,"mordida submersa");
        Check(wildlife.WaterDangerActive && wildlife.Packs>=3,"Mergulhar mais fundo tambem atrai ataques");
        body.position=new Vector2(-3,lake.WaterY+1.5f);yield return Wait(1.5f);StopMovie();
        body.position=new Vector2(-3,lake.WaterY+.25f);yield return Until(()=>wildlife.ActivePiranhas>0,"grupo antes da derrota");
        var flow=Object.FindAnyObjectByType<ChapterFlow>();flow.enabled=false;boss.Health.TakeDamage(9999);yield return Wait(.15f);
        Check(!wildlife.CanHurt && wildlife.ActivePiranhas==0 && GameObject.Find("PiranhaSurfaceWarning")==null,"Fim do combate limpa perigo e aviso");
        Check(wildlife.AmbientFishCount==8,"Peixes decorativos continuam apos combate");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Progresso real preservado pelo teste");
    }
    private static void Capture(string filename = "gameplay.png")
    {
        var camera = Camera.main; var old = camera.targetTexture;
        var overlays = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
        var rt = new RenderTexture(1280, 720, 24); var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        camera.targetTexture = rt;
        Object.FindAnyObjectByType<LakeBackdrop>().SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases(); camera.Render(); var previous = RenderTexture.active; RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_Wildlife_v11");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_Wildlife_v11/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v11/Frames"); movieCamera = Camera.main;
        movieTarget = new RenderTexture(960, 540, 24); moviePixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        movieCamera.targetTexture = movieTarget;
        movieCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = movieCamera; canvas.planeDistance = 1; }
        movieTimes = new List<float>(); movieStarted = Time.unscaledTime; nextMovieFrame = 0;
    }
    private static void RecordMovie()
    {
        if (movieTimes == null || Time.unscaledTime - movieStarted < nextMovieFrame) return;
        Canvas.ForceUpdateCanvases(); movieCamera.Render(); var previous = RenderTexture.active; RenderTexture.active = movieTarget;
        moviePixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); moviePixels.Apply(); RenderTexture.active = previous;
        File.WriteAllBytes("Logs/LagoDoPneu/v11/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v11/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v11/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
