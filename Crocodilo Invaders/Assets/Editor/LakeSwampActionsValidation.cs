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
public static class LakeSwampActionsValidation
{
    private const string Key = "Crocodilo.Lake.SwampActionsValidation";
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
    static LakeSwampActionsValidation()
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
        LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v30/Actors/TestSave");player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"controle da horda");
        waves.GetComponent<EnemySpawner>().StopSpawning();foreach(var actor in Object.FindObjectsByType<LakeJourneyEnemy>())actor.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);var rb=player.GetComponent<Rigidbody2D>();rb.position=new Vector2(director.Arena.xMin+3,2);
        player.Health.SetInvulnerable(typeof(LakeSwampActionsValidation),true);var actors=new LakeJourneyEnemy[4];
        for(int i=0;i<4;i++)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/"+((LakeJourneyEnemy.Species)i)+".prefab");
            var node=Object.Instantiate(prefab,new Vector3(director.Arena.xMax+.5f,2,0),Quaternion.identity);
            actors[i]=node.GetComponent<LakeJourneyEnemy>();actors[i].SetTarget(player.Health);actors[i].Join(director,i);
        }
        StartMovie();
        Check(Mathf.Abs(actors[2].Lily.Drawing.transform.position.y-(director.WaterY-.12f))<.02f,"Folha nasce na água já no primeiro quadro, sem aparecer no centro");
        Check(actors[2].Lily.Drawing.enabled && actors[3].Lily.Drawing.enabled && !actors[2].Visual.enabled && !actors[3].Visual.enabled,"Folhas vazias anunciam criaturas antes delas aparecerem");
        int hp=actors[2].GetComponent<Health>().CurrentHealth;
        Check(!actors[2].GetComponent<Health>().TakeDamage(1) && actors[2].GetComponent<Health>().CurrentHealth==hp,"Criatura atrás da mata não recebe dano antecipado");
        yield return Until(()=>actors[2].CurrentMotion==LakeJourneyEnemy.Motion.WaitingLily,"folha chega primeiro");
        Check(!actors[2].Visual.enabled && actors[2].Lily.Arrived,"Folha parada na batalha antes do salto");Capture("empty-leaf-warning.png");
        float waited=actors[2].Clock;yield return Until(()=>actors[2].CurrentMotion==LakeJourneyEnemy.Motion.BankJump,"salto saindo da mata");
        Check(actors[2].Clock-waited>=.32f && actors[2].Visual.sortingOrder<0,"Pausa legível na folha e início atrás da vegetação");
        yield return Wait(.36f);
        Check(actors[2].GetComponent<Rigidbody2D>().position.y>actors[2].Lily.Seat.y+.8f,"Sapo sobe em arco antes de pousar na folha");Capture("bank-jump.png");
        Check(actors[2].Shots==0 && actors[3].Attacks==0,"Chegada não ataca o jogador");
        yield return Until(()=>!actors[2].IsEnteringBank && !actors[3].IsEnteringBank,"duas criaturas pousam");
        foreach(int index in new[]{2,3})
        {
            var actor=actors[index];
            Check(actor.Visual.enabled && actor.GetComponent<Collider2D>().enabled && !actor.GetComponent<Health>().IsInvulnerable,actor.Kind+" entra no combate depois do pouso");
            Check(Vector2.Distance(actor.Feet,actor.Lily.Seat)<.04f,actor.Kind+" termina salto apoiado na folha");
        }
        Capture("leaf-landing.png");
        yield return Until(()=>actors[0].CurrentMotion==LakeJourneyEnemy.Motion.Patrol,"pequeno começa perseguição");
        Vector2 pos=actors[0].GetComponent<Rigidbody2D>().position;float before=Vector2.Distance(pos,rb.position);yield return Wait(1.3f);
        Check(Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position)<before-.5f,"Mosquito pequeno avança em direção ao jogador");
        rb.position=new Vector2(director.Arena.xMax-1,2);before=Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position);
        yield return Wait(1.3f);
        Check(Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position)<before-.3f && actors[0].Visual.flipX,"Perseguição muda suavemente quando o jogador troca de lado");Capture("mosquito-chases.png");
        int defeats=0;actors[0].GetComponent<Enemy>().Defeated+=e=>defeats++;
        actors[0].GetComponent<Health>().TakeDamage(1);yield return Frames(1);
        Check(Object.FindObjectsByType<LakeMosquitoSlap>().Length==0 && actors[0].Visual.enabled,"Acerto não fatal não esmaga mosquito vivo");
        actors[0].GetComponent<Health>().TakeDamage(999);yield return Frames(1);
        var slap=Object.FindAnyObjectByType<LakeMosquitoSlap>();
        Check(defeats==1 && slap!=null && !actors[0].Visual.enabled,"Morte dispara uma chinelada e uma derrota");
        Check(slap.BloodCount==20 && slap.CrushedSprite.transform.localScale.y<.2f,"Mosquito pequeno achatado com sangue em pixels");
        Check(slap.GetComponentsInChildren<Collider2D>().Length==0 && slap.GetComponentsInChildren<Health>().Length==0,"Efeito não duplica inimigo, hitbox ou dano");Capture("small-mosquito-slap.png");
        float frozen=slap.Clock;Vector3 flat=slap.CrushedSprite.transform.position;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(slap.Clock==frozen && slap.CrushedSprite.transform.position==flat,"Pausa congela sangue e corpo esmagado");Time.timeScale=1;
        yield return Wait(1.2f);Check(Object.FindObjectsByType<LakeMosquitoSlap>().Length==0,"Sangue e corpo desaparecem sem resíduos permanentes");
        actors[1].GetComponent<Health>().TakeDamage(999);yield return Frames(1);slap=Object.FindAnyObjectByType<LakeMosquitoSlap>();
        Check(slap!=null && slap.BloodCount==26 && slap.CrushedSprite.transform.localScale.y<.2f,"Inseto grande também morre esmagado");Capture("large-mosquito-slap.png");
        yield return Until(()=>actors[2].Shots>0 && actors[3].Shots>0,"ataques após chegada");
        Check(actors[2].Attacks>0 && actors[3].Attacks>0,"Sapos e besouros conservam seus ataques depois do salto de entrada");
        StopMovie();player.Health.SetInvulnerable(typeof(LakeSwampActionsValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
        Check(director.ActiveDrops==0 && Object.FindObjectsByType<LakeMosquitoSlap>().Length==0,"Derrota limpa gotas e efeitos de morte");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LakeSwampActions_v30/Actors");
        File.WriteAllBytes("Docs/ArtPreviews/LakeSwampActions_v30/Actors/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v30/Actors/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v30/Actors/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v30/Actors/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v30/Actors/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
