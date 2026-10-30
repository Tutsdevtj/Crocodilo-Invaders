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
public static class LakeJourneyCombatValidation
{
    private const string Key = "Crocodilo.Lake.JourneyCombatValidation";
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
    static LakeJourneyCombatValidation()
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
        EditorSceneManager.OpenScene(LakeStageTools.Stage);
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    public static void RunFromMenu()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    private static void Begin()
    {
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 650;
        previousMaxDeltaTime=Time.maximumDeltaTime;Time.maximumDeltaTime=.05f;
        Application.runInBackground = true; Application.logMessageReceived += OnLog;
        Directory.CreateDirectory("Logs/LagoDoPneu");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"primeira horda");
        waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();LeaderboardSession.Cancel();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();var scenery=Object.FindAnyObjectByType<LakeJourneyScenery>();
        var spawner=waves.GetComponent<EnemySpawner>();
        yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"inimigo de controle");
        spawner.StopSpawning();
        foreach(var enemy in Object.FindObjectsByType<LakeJourneyEnemy>())enemy.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);var rb=player.GetComponent<Rigidbody2D>();
        rb.position=new Vector2(director.Arena.xMin+2,2);int hp=player.Health.CurrentHealth;
        Check(player.GetComponent<PlayerDash>().TryDash(),"Dash real disponível na fase");
        var drop=LakeJourneyDrop.Fire(director,rb.position,Vector2.right*.1f);yield return Until(()=>drop==null,"gota consumida durante dash");
        Check(player.Health.CurrentHealth==hp,"Gota de sapinho é consumida sem dano durante dash");
        yield return Wait(.8f);drop=LakeJourneyDrop.Fire(director,rb.position,Vector2.right*.1f);yield return Until(()=>drop==null,"contato sem proteção");
        Check(player.Health.CurrentHealth==hp-1,"Gota sem proteção causa exatamente um HP");player.Health.SetInvulnerable(typeof(LakeJourneyCombatValidation),true);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/Beetle.prefab");
        var node=Object.Instantiate(prefab,new Vector3(1,2,0),Quaternion.identity);var beetle=node.GetComponent<LakeJourneyEnemy>();beetle.SetTarget(player.Health);beetle.Join(director,3);
        yield return Wait(.08f);var health=node.GetComponent<Health>();float before=health.PreciseCurrentHealth;health.TakeDamage(4);
        Check(Mathf.Abs(before-health.PreciseCurrentHealth-1)<.02f,"Casco pela frente recebe um quarto do dano");
        rb.position=new Vector2(5,3);yield return Wait(.08f);before=health.PreciseCurrentHealth;health.TakeDamage(1);
        Check(Mathf.Abs(before-health.PreciseCurrentHealth-1)<.02f,"Parte traseira do besouro recebe dano normal");
        yield return Until(()=>beetle.CurrentMotion==LakeJourneyEnemy.Motion.Warning,"aviso do besouro");Vector2 aim=beetle.LockedAim;
        rb.position=new Vector2(-4,2);yield return Wait(.12f);Check(beetle.LockedAim==aim,"Besouro trava mira antes de investir");
        yield return Until(()=>beetle.Vulnerable,"abertura do casco");Check(health.IncomingDamageMultiplier==1.8f,"Casco abre depois da investida");Capture("beetle-vulnerable.png");
        var frogPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/Frog.prefab");
        var frogNode=Object.Instantiate(frogPrefab,new Vector3(5,director.WaterY+.15f,0),Quaternion.identity);var frog=frogNode.GetComponent<LakeJourneyEnemy>();frog.SetTarget(player.Health);frog.Join(director,1);
        yield return Until(()=>frog.Shots>0,"cuspida real");Check(frog.Shots==2 && director.ActiveDrops==2,"Sapinho solta duas gotas na entrada do lago fechado");Capture("frog-spitting.png");
        scenery.enabled=false;Check(Object.FindAnyObjectByType<LakeBackdrop>().TravelScale==1,"Desativar travessia restaura deslocamento do cenário");
        player.Health.SetInvulnerable(typeof(LakeJourneyCombatValidation),false);player.Health.TakeDamage(999);yield return Frames(4);
        Check(waves.Phase==WaveController.RunPhase.Failed && director.ActiveDrops==0,"Derrota cancela gotas da horda");
        Check(beetle.CurrentMotion==LakeJourneyEnemy.Motion.Stopped && frog.CurrentMotion==LakeJourneyEnemy.Motion.Stopped,"Atores param sem ataques tardios na derrota");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_Journey_v22");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_Journey_v22/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v22/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v22/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
        movieTimes.Add(Time.unscaledTime - movieStarted); nextMovieFrame = Time.unscaledTime - movieStarted + .10f;
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
        File.WriteAllText("Logs/LagoDoPneu/v22/Frames/frames.ffconcat", text.ToString());
        movieTimes = null; movieCamera.targetTexture = null;
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
        movieTarget.Release(); Object.Destroy(movieTarget); Object.Destroy(moviePixels);
    }
    private static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label)
    { double end = EditorApplication.timeSinceStartup + 90; while (!predicate()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label); yield return null; } }
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
        File.WriteAllText("Logs/LagoDoPneu/v22/combat-validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}


