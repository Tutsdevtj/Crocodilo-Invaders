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
public static class LakeReworkJourneyValidation
{
    private const string Key = "Crocodilo.Lake.ReworkJourneyValidation";
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
    static LakeReworkJourneyValidation()
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
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 300;
        previousMaxDeltaTime=Time.maximumDeltaTime;Time.maximumDeltaTime=.05f;
        Application.runInBackground = true; Application.logMessageReceived += OnLog;
        Directory.CreateDirectory("Logs/LagoDoPneu");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"horda ativa");
        LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v26/Journey/TestSave");player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"inimigo controle");
        waves.GetComponent<EnemySpawner>().StopSpawning();foreach(var actor in Object.FindObjectsByType<LakeJourneyEnemy>())actor.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.GetComponent<Rigidbody2D>().position=new Vector2(director.Arena.xMin+3,2);
        player.Health.SetInvulnerable(typeof(LakeReworkJourneyValidation),true);var actors=new LakeJourneyEnemy[4];
        for(int i=0;i<4;i++)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/"+((LakeJourneyEnemy.Species)i)+".prefab");
            var node=Object.Instantiate(prefab,new Vector3(director.Arena.xMax+.5f,2,0),Quaternion.identity);
            actors[i]=node.GetComponent<LakeJourneyEnemy>();actors[i].SetTarget(player.Health);actors[i].Join(director,i);
        }
        StartMovie();var rework=Resources.Load<LakeReworkArt>("LakeReworkV26/art");bool turned=false,bigMoved=false,beetle=false,frog=false,queuedMoved=false;float warnAt=-1,maxWarning=0;
        Vector2 bigPrev=actors[1].transform.position;float lastClock=0;float start=Time.time;
        while(Time.time-start<36)
        {
            if(Time.time-start>17)player.GetComponent<Rigidbody2D>().position=new Vector2(director.Arena.xMax-1,2);
            var big=actors[1];Vector2 now=big.transform.position;
            if(lastClock!=big.Clock)
            {
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Queued && Vector2.Distance(bigPrev,now)>.002f)queuedMoved=true;
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Warning){if(warnAt<0)warnAt=big.Clock;maxWarning=Mathf.Max(maxWarning,big.Clock-warnAt);}
                else warnAt=-1;
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Strike && Vector2.Distance(bigPrev,now)>.02f)bigMoved=true;
                bigPrev=now;lastClock=big.Clock;
            }
            if(!turned && actors[0].Visual.flipX && Resources.Load<LakeJourneyArt>("LakeJourneyV22/art").enemies.Take(4).Contains(actors[0].Visual.sprite)){turned=true;Capture("mosquito-return.png");}
            if(!beetle && actors[3].Shots>=2 && actors[3].Vulnerable)
            {beetle=true;Check(Vector2.Distance(actors[3].Feet,actors[3].Lily.Seat)<.04f,"Besouro volta à folha depois de investir e cuspir");Capture("beetle-attacks.png");}
            if(!frog && actors[2].Shots>0){frog=true;Capture("small-frog.png");}
            yield return Frames(1);
        }
        Check(turned,"Mosquito acompanha troca de lado com desenho original espelhado");
        Check(queuedMoved,"Mosquito grande continua voando enquanto espera a vez");
        Check(bigMoved && actors[1].Attacks>=2,"Mosquito grande realiza investidas repetidas");
        Check(maxWarning<.85f,"Aviso do mosquito grande curto, sem piscar parado por vários segundos");
        Check(beetle && actors[3].Shots>=2,"Besouro produz ataque real com duas gotas na investida");
        Check(frog,"Sapinho conserva ataque original");
        Check(director.ActivePreparations<=2,"Fila preserva limite de preparações");
        float t=actors[0].Clock;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(actors[0].Clock==t,"Pausa congela novas viradas e movimentos");Time.timeScale=1;StopMovie();
        player.Health.SetInvulnerable(typeof(LakeReworkJourneyValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
        Check(director.ActiveDrops==0 && actors.All(x=>x.CurrentMotion==LakeJourneyEnemy.Motion.Stopped),"Derrota limpa gotas e fila de ataques");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_Rework_v26/Journey");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_Rework_v26/Journey/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v26/Journey/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v26/Journey/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v26/Journey/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v26/Journey/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
