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
public static class LakeJourneyLilyValidation
{
    private const string Key = "Crocodilo.Lake.JourneyLilyValidation";
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
    static LakeJourneyLilyValidation()
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
        Directory.CreateDirectory("Logs/LagoDoPneu/v24");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"primeira horda");
        waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();LeaderboardSession.Cancel();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();var spawner=waves.GetComponent<EnemySpawner>();
        yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"inimigo de controle");spawner.StopSpawning();
        foreach(var actor in Object.FindObjectsByType<LakeJourneyEnemy>())actor.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeJourneyLilyValidation),true);
        player.GetComponent<Rigidbody2D>().position=new Vector2(-4,2);
        var actors=new LakeJourneyEnemy[2];
        for(int i=0;i<2;i++)
        {
            string role=i==0?"Frog":"Beetle";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/"+role+".prefab");
            var node=Object.Instantiate(prefab,new Vector3(director.Arena.xMax+.3f,3+i,0),Quaternion.identity);
            actors[i]=node.GetComponent<LakeJourneyEnemy>();actors[i].SetTarget(player.Health);actors[i].Join(director,i+2);
            Check(actors[i].Lily!=null && actors[i].UsesLily,role+" tem apoio de vitória-régia");
            Check(Vector2.Distance(actors[i].Feet,actors[i].Lily.Seat)<.02f,role+" nasce com pés sobre folha");
            Check(actors[i].transform.Find("SapinhoBankLog")==null,"Tronco preso ao inimigo removido");
        }
        StartMovie();
        yield return Until(()=>actors[0].CurrentMotion!=LakeJourneyEnemy.Motion.Arriving && actors[1].CurrentMotion!=LakeJourneyEnemy.Motion.Arriving,"folhas chegando pela superfície");
        Capture("grounded-on-lilies.png");
        File.WriteAllText("Logs/LagoDoPneu/v24/positions.txt","Arena="+director.Arena+" frog="+actors[0].Lily.Seat+" state="+actors[0].CurrentMotion+" arrived="+actors[0].Lily.Arrived+" beetle="+actors[1].Lily.Seat+" state="+actors[1].CurrentMotion+" arrived="+actors[1].Lily.Arrived+" active="+director.CombatActive+" gap="+Mathf.Abs(actors[0].Lily.Seat.x-actors[1].Lily.Seat.x));
        Check(Mathf.Abs(actors[0].Lily.Seat.x-actors[1].Lily.Seat.x)>1.7f,"Apoios reservados em posições distintas");
        Vector2 frogStart=actors[0].Feet;float start=Time.time;
        while(Time.time-start<1)
        {
            foreach(var actor in actors)if(actor.CurrentMotion!=LakeJourneyEnemy.Motion.Strike && Vector2.Distance(actor.Feet,actor.Lily.Seat)>.025f)throw new InvalidOperationException("Pés deslizando fora da folha");
            yield return null;
        }
        Check(Vector2.Distance(frogStart,actors[0].Feet)>.005f,"Personagem acompanha deriva e oscilação da folha");
        float frozen=actors[0].Clock;Vector2 feet=actors[0].Feet,seat=actors[0].Lily.Seat;Time.timeScale=0;
        double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(actors[0].Clock==frozen && actors[0].Feet==feet && actors[0].Lily.Seat==seat,"Pausa congela passageiro e apoio juntos");Time.timeScale=1;
        yield return Until(()=>actors[0].Shots>0,"sapinho cuspindo apoiado");
        Check(Vector2.Distance(actors[0].Feet,actors[0].Lily.Seat)<.025f,"Sapinho permanece sobre folha ao cuspir");Capture("frog-on-lily.png");
        yield return Until(()=>actors[1].CurrentMotion==LakeJourneyEnemy.Motion.Strike,"besouro saltando da folha");yield return Wait(.25f);
        Check(actors[1].Feet.y>actors[1].Lily.Seat.y+.4f,"Besouro só sai da folha durante salto de ataque");Capture("beetle-leaping.png");
        yield return Until(()=>actors[1].Vulnerable,"besouro de volta à folha");
        Check(Vector2.Distance(actors[1].Feet,actors[1].Lily.Seat)<.025f,"Besouro aterrissa antes de expor barriga");Capture("beetle-landed.png");
        Check(actors[1].GetComponent<Health>().IncomingDamageMultiplier==1.8f,"Vulnerabilidade preservada na aterrissagem");
        var released=actors[0].Lily;actors[0].GetComponent<Health>().TakeDamage(999);yield return Frames(2);
        Check(released!=null && released.Released,"Folha se desprende quando inimigo morre");yield return Wait(1);
        Check(released==null,"Folha liberada sai sem deixar apoio órfão");StopMovie();
        player.Health.SetInvulnerable(typeof(LakeJourneyLilyValidation),false);player.Health.TakeDamage(999);yield return Frames(4);
        Check(Object.FindObjectsByType<LakeEnemyLily>().Length==0,"Derrota limpa folhas ocupadas");
        Check(Object.FindAnyObjectByType<LakeLilyPads>().Count==6,"Seis folhas decorativas existentes preservadas");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_EnemyLilies_v24");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_EnemyLilies_v24/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v24/Frames"); movieCamera = Camera.main;
        movieTarget = new RenderTexture(960, 540, 24); moviePixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        movieCamera.targetTexture = movieTarget;
        movieCamera.aspect=960f/540f;
        movieCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in movieCanvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = movieCamera; canvas.planeDistance = 1; canvas.sortingLayerName="LakeHUD"; }
        movieTimes = new List<float>(); movieStarted = Time.unscaledTime; nextMovieFrame = 0;
    }
    private static void RecordMovie()
    {
        if (movieTimes == null || Time.unscaledTime - movieStarted < nextMovieFrame) return;
        Canvas.ForceUpdateCanvases(); movieCamera.Render(); var previous = RenderTexture.active; RenderTexture.active = movieTarget;
        moviePixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); moviePixels.Apply(); RenderTexture.active = previous;
        File.WriteAllBytes("Logs/LagoDoPneu/v24/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v24/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v24/combat-validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}


