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
public static class LakeTirePlayValidation
{
    private const string Key = "Crocodilo.Lake.TireValidation";
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
    static LakeTirePlayValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v25/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"entrada do boss");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var tire=combat.Tire;var rb=player.GetComponent<Rigidbody2D>();var area=combat.Arena;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);
        rb.position=new Vector2(area.xMin+3,2);
        Check(tire!=null && tire.ArtReady,"Asset de pneu carregado no boss real");Check(boss.Health.MaxHealth==200,"HP do boss preservado");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"salto anterior ao pneu");rb.position=new Vector2(area.xMax-2,2);
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tire,"pneu no ciclo natural");
        Check(combat.Landings>0 && combat.Tongue.Strikes>0,"Pneu aparece naturalmente depois de língua e salto");
        Check(tire.Preparing && tire.Held.enabled && tire.Projectile==null,"Pneu visível na preparação sem dano antecipado");
        Vector2 locked=tire.LockedAim;rb.position=new Vector2(area.xMin+2,2);StartMovie();yield return Wait(.25f);
        Check(tire.LockedAim==locked,"Mira do pneu trava antes do jogador desviar");Capture("tire-windup.png");
        float prepTime=tire.PhaseTime;Vector3 heldPosition=tire.Held.transform.position;Time.timeScale=0;
        double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(tire.PhaseTime==prepTime && tire.Held.transform.position==heldPosition,"Pausa congela pneu em preparação");Time.timeScale=1;
        yield return Until(()=>tire.Projectile!=null,"arremesso natural");var first=tire.Projectile;
        Check(!tire.Held.enabled && tire.Throws==1,"Um pneu lançado e objeto da mão ocultado");
        Check(Vector2.Dot(first.Velocity.normalized,(locked-tire.LaunchPoint).normalized)>.999f,"Trajetória segue posição avisada, sem perseguição");
        Check(Mathf.Abs(first.Velocity.magnitude-10.5f)<.01f,"Velocidade normal de arremesso");
        yield return Wait(.15f);Quaternion rotation=first.transform.rotation;yield return Wait(.12f);
        Check(first!=null && Quaternion.Angle(rotation,first.transform.rotation)>30,"Pneu gira durante voo");Capture("tire-flying.png");
        Vector2 frozen=first.GetComponent<Rigidbody2D>().position;float age=first.Clock;Time.timeScale=0;
        pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(first.Clock==age && first.GetComponent<Rigidbody2D>().position==frozen,"Pausa congela deslocamento e giro");Time.timeScale=1;
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"fim da primeira passagem");
        Check(tire.Projectile==null && tire.DamageAttempts==0,"Desvio evita contato e pneu sai da arena");
        rb.position=new Vector2(area.xMax-2,2);int hp=player.Health.CurrentHealth,attempts=tire.DamageAttempts;
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),false);Check(combat.TryBeginTire(),"Segundo arremesso disponível no intervalo");
        yield return Until(()=>tire.DamageAttempts>attempts,"contato real do pneu");
        Check(player.Health.CurrentHealth==hp-2 && tire.Hits==1,"Pneu causa exatamente dois HP no contato");
        Check(tire.Projectile!=null && tire.Projectile.ContactSpent,"Pneu mantém impulso após consumir contato");yield return Wait(.25f);
        Check(player.Health.CurrentHealth==hp-2,"Mesma passagem não repete dano");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"intervalo para dash");
        rb.position=new Vector2(area.xMax-2,2);hp=player.Health.CurrentHealth;attempts=tire.DamageAttempts;int hits=tire.Hits;
        Check(combat.TryBeginTire(),"Arremesso para teste de esquiva");yield return Until(()=>tire.Projectile!=null,"pneu para dash");
        yield return Until(()=>Vector2.Distance(tire.Projectile.GetComponent<Rigidbody2D>().position,rb.position)<1.4f,"aproximação do pneu");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),false);player.Movement.SetMoveInput(Vector2.left);Check(player.GetComponent<PlayerDash>().TryDash(),"Dash real durante aproximação");player.Movement.SetMoveInput(Vector2.zero);
        yield return Until(()=>tire.DamageAttempts>attempts,"contato esquivado");
        Check(player.Health.CurrentHealth==hp && tire.Hits==hits,"Dash atravessa pneu sem dano");yield return Wait(.3f);
        Check(player.Health.CurrentHealth==hp,"Contato esquivado não causa dano atrasado");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"intervalo para fúria");
        Check(combat.TryBeginTire(),"Preparação antes de cruzar metade da vida");yield return Wait(.15f);int throws=tire.Throws;
        boss.Health.TakeDamage(boss.Health.MaxHealth/2+1);yield return Frames(4);
        Check(combat.CurrentState==SapoDoPneuCombat.State.Rage && !tire.Busy && !tire.Held.enabled,"Fúria interrompe pneu em preparação");
        Check(tire.Throws==throws && tire.Projectile==null,"Interrupção não gera arremesso atrasado");
        yield return Until(()=>combat.TryBeginTire(),"pneu na fúria");yield return Until(()=>tire.Projectile!=null,"arremesso da fúria");
        Check(Mathf.Abs(tire.Projectile.Velocity.magnitude-12.5f)<.01f,"Fúria acelera o mesmo pneu");
        Check(combat.Companion.ActiveDrops==0 && !combat.Tongue.Busy && !combat.HasWaterDrops,"Pneu não se sobrepõe à língua ou rajada da sapa");Capture("enraged-tire.png");
        throws=tire.Throws;combat.StopCombat();yield return Frames(4);yield return Wait(.9f);
        Check(!tire.Busy && tire.Projectile==null && !tire.Held.enabled,"Encerrar combate limpa pneu e preparação");
        Check(Object.FindObjectsByType<LakeTireProjectile>().Length==0 && tire.Throws==throws,"Sem pneu órfão ou lançamento tardio");StopMovie();
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
        Directory.CreateDirectory("Docs/ArtPreviews/Sapeneu_TireAttack_v25");
        File.WriteAllBytes("Docs/ArtPreviews/Sapeneu_TireAttack_v25/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v25/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v25/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v25/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v25/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
