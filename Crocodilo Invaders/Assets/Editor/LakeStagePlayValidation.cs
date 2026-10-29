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
public static class LakeStagePlayValidation
{
    private const string Key = "Crocodilo.Lake.PlayValidation";
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
    static LakeStagePlayValidation()
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
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 150;
        Application.runInBackground = true; Application.logMessageReceived += OnLog;
        Directory.CreateDirectory("Logs/LagoDoPneu");
        steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(5); LeaderboardSession.Cancel(); CampaignProgress.UseDirectory("Logs/LagoDoPneu/TestSave");
        var menu = Object.FindAnyObjectByType<DeveloperMenuView>();
        if (menu != null)
        {
            Check(GameObject.Find("JungleMission").GetComponentInChildren<TMPro.TMP_Text>().text == "FASE 2 · LAGO DO PNEU", "Menu identifica o lago como fase 2");
            yield return Until(() => { menu.Open(); return menu.IsVisible; }, "menu pronto");
            GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return Until(() => Object.FindAnyObjectByType<GameplayLoadingController>()?.State == GameplayLoadingController.LoadingState.Ready, "loading do lago");
            Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
            yield return Until(() => Object.FindAnyObjectByType<WaveController>()?.ActiveBoss != null, "boss pela entrada real");
            Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == LakeStageTools.Stage, "Acesso DEV carrega a fase de produção correta");
        }
        player = Object.FindAnyObjectByType<Player>(); waves = Object.FindAnyObjectByType<WaveController>();
        Check(player != null && waves != null, "Player e controlador nativos presentes");
        player.GetComponent<PlayerInputReader>().enabled = false;
        player.Health.SetInvulnerable(typeof(LakeStagePlayValidation), true);
        yield return Until(() => waves.ActiveBoss != null && waves.ActiveBoss.Ready, "entrada do sapo");
        boss = waves.ActiveBoss; combat = boss.GetComponent<SapoDoPneuCombat>();
        Check(combat != null && boss.DisplayName == "SAPO DO PNEU", "Boss próprio ativo na fase");
        Check(boss.GetComponent<GusiniCombat>() == null && boss.GetComponent<MacaquiniCombat>() == null, "Sem controladores concorrentes");
        var lake = Object.FindAnyObjectByType<LakeBackdrop>();
        Check(lake.LayerCount == 8 && lake.TileCount == 22 && lake.GetComponentsInChildren<SpriteRenderer>().Length == 22,
            "Oito camadas com tiles de repetição para rolagem contínua");
        var contact = boss.GetComponent<LakeBossWaterContact>();
        var expression = boss.GetComponent<LakeFrogExpressions>();
        Check(contact != null && lake.GetComponent<LakeSurfaceMotion>().CrestCount == 22, "Contato com água e 22 cristas ambientes presentes");
        yield return Until(() => combat.CurrentState == SapoDoPneuCombat.State.Float, "flutuação");
        yield return Wait(.3f);
        var hud = Object.FindAnyObjectByType<LakeBossBar>();
        Check(expression != null && expression.HasEyelids, "Expressao do sapo usa recorte registrado dos olhos");
        Check(hud != null && hud.IsBuilt, "HUD exclusiva do pneu criada na entrada real do boss");
        Check(hud.DisplayedHealth > 0, "HUD carrega vida na entrada");
        Check(hud.FrameSprite != null && hud.FrameSprite.name == "hud-frame" && hud.Water != null && hud.GetComponentInChildren<LakeHudSpill>() != null, "Arte aprovada com reservatorio animado e derramamento de vida presentes");
        Check(hud.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="SAPO DO PNEU"), "Nome proprio legivel na HUD do sapo");
        Check(contact.ContactWeight > .7f && contact.Reflection != null, "Pneu flutua com menisco e reflexo");
        Check(boss.GetComponentInChildren<SpriteRenderer>().sharedMaterial.shader.name == "Crocodilo/LakeWaterContact", "Recorte de submersão aplicado ao sprite aprovado");
        Capture("hud-normal.png");
        Vector3 oldBreath = boss.GetComponentInChildren<SpriteRenderer>().transform.localScale;
        float oldHudSurface=hud.Water.SurfaceSample;
        float oldClock = lake.MotionClock;
        float oldShoreDistance = lake.ScrollDistance(4), oldFarDistance = lake.ScrollDistance(2);
        var shore = lake.transform.Find("Layer_4").GetComponent<SpriteRenderer>();
        float oldShoreX = shore.transform.position.x;
        Vector3 oldCrest = lake.GetComponentInChildren<LineRenderer>().GetPosition(4);
        yield return Wait(.3f);
        Check(Vector3.Distance(oldBreath,boss.GetComponentInChildren<SpriteRenderer>().transform.localScale)>.001f, "Sapo respira enquanto flutua");
        Check(Mathf.Abs(hud.Water.SurfaceSample-oldHudSurface)>.001f,"Corrente e ondas se movem dentro da HUD");
        Check(lake.MotionClock > oldClock && Vector3.Distance(oldCrest, lake.GetComponentInChildren<LineRenderer>().GetPosition(4)) > .001f, "Reflexos e cristas mudam ao longo do tempo");
        Check(lake.ScrollDistance(4) - oldShoreDistance > .2f && shore.transform.position.x < oldShoreX - .15f, "Margem realmente avança para a esquerda");
        Check(lake.ScrollDistance(4) - oldShoreDistance > lake.ScrollDistance(2) - oldFarDistance, "Parallax move margem próxima mais rápido que floresta distante");
        var right = shore.transform.Find("RepeatRight").GetComponent<SpriteRenderer>();
        Check(Mathf.Abs(shore.bounds.max.x - right.bounds.min.x) < .002f && shore.flipX != right.flipX, "Tiles se encontram sem lacuna e compartilham a mesma borda");
        yield return Until(() => combat.CurrentState == SapoDoPneuCombat.State.Prepare, "antecipação");
        Vector2 locked = combat.LockedLanding; Vector2 position = boss.GetComponent<Rigidbody2D>().position;
        Check(Object.FindObjectsByType<LakeWaterFX>(FindObjectsSortMode.None).Any(x => x.name.Contains("Warning")), "Aviso de aterrissagem visível antes do salto");
        double wait = EditorApplication.timeSinceStartup + .25; float time = Time.time;
        Time.timeScale = 0;
        float pausedClock = lake.MotionClock;
        float pausedHudClock = hud.AnimationClock;
        float pausedShore = shore.transform.position.x;
        while (EditorApplication.timeSinceStartup < wait) yield return null;
        Check(Time.time == time && Vector2.Distance(position, boss.GetComponent<Rigidbody2D>().position) < .001f, "Pausa congela aviso e boss");
        Check(hud.AnimationClock == pausedHudClock, "Pausa congela agua e animacao da HUD");
        Check(lake.MotionClock == pausedClock, "Pausa congela também água, vento e reflexo");
        Check(shore.transform.position.x == pausedShore, "Pausa congela a rolagem do cenário");
        Time.timeScale = 1;
        player.GetComponent<Rigidbody2D>().position = new Vector2(-3, 2);
        Check(combat.LockedLanding == locked, "Destino travado não persegue a nave");
        yield return Until(() => combat.CurrentState == SapoDoPneuCombat.State.Rise, "subida");
        yield return Wait(.3f);
        Check(boss.transform.position.y > position.y + .8f, "Salto realmente eleva o Rigidbody2D");
        Check(contact.ContactWeight < .15f, "Menisco e reflexo desaparecem quando o pneu sai da água");
        yield return Until(() => combat.Landings >= 1, "impacto");
        Check(combat.DropsLaunched == 5, "Impacto lança cinco gotas em arco");
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).Length > 0, "Gotas existem na simulação");
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).All(d => d.Drawing != null && d.Drawing.name == "gota-lago-v03"), "Leque usa a gota na paleta da água do lago");
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).All(d => Vector2.Dot(d.FrontDirection, d.Velocity.normalized) > .96f), "Gotas nascem com a parte arredondada à frente");
        Check(Object.FindObjectsByType<LakeSurfaceRipple>(FindObjectsSortMode.None).Length > 0, "Aterrissagem produz ondas independentes na superfície");
        yield return Wait(.2f);
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).All(d => Vector2.Dot(d.FrontDirection, d.Velocity.normalized) > .96f), "Parte maior lidera durante a subida");
        yield return Until(() => Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).Any(d => d.Returning), "queda orientada");
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).Where(d => d.Returning).All(d => Vector2.Dot(d.FrontDirection, d.Velocity.normalized) > .96f), "Parte maior lidera também na queda");
        yield return Until(() => combat.DropsReturned > 0, "gotas retornam à água");
        Check(combat.DropsReturned > 0, "Gotas caem e são consumidas na superfície");
        int bossHp = boss.Health.CurrentHealth;
        player.GetComponent<Rigidbody2D>().position = (Vector2)boss.transform.position + new Vector2(-3, .3f);
        yield return Frames(3);
        Check(player.TryShoot(), "Tiro nativo do Bombardiro funciona");
        yield return Until(() => boss.Health.CurrentHealth < bossHp, "tiro acerta sapo");
        Check(boss.Health.CurrentHealth < bossHp, "Hitbox do sapo recebe dano do projétil real");
        // Deterministic physics contact with an actual generated water drop.
        foreach (var d in Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None)) Object.Destroy(d.gameObject);
        player.GetComponent<Rigidbody2D>().position = new Vector2(-4, 1);
        player.Movement.SetMoveInput(Vector2.zero); yield return Frames(4);
        player.Health.SetInvulnerable(typeof(LakeStagePlayValidation), false);
        int hp = player.Health.CurrentHealth;
        SpawnDrop(player.GetComponent<Rigidbody2D>().position, Vector2.up);
        yield return Wait(.08f);
        Check(player.Health.CurrentHealth == hp - 1, "Contato de gota causa exatamente 1 HP");
        yield return Wait(.15f);
        Check(player.Health.CurrentHealth == hp - 1, "Gota consumida não repete dano");
        yield return Until(() => player.Dash.IsReady, "dash disponível");
        hp = player.Health.CurrentHealth;
        Check(player.TryDash(), "Dash real disponível na fase");
        SpawnDrop(player.GetComponent<Rigidbody2D>().position, Vector2.up);
        yield return Wait(.06f);
        Check(player.Health.CurrentHealth == hp, "Invulnerabilidade do dash protege contra gotas");
        player.Health.SetInvulnerable(typeof(LakeStagePlayValidation), true);
        StartMovie(); yield return Wait(.65f);
        int oldSpills=hud.Spills; float oldWaterLevel=hud.Water.Level;
        boss.Health.TakeDamage(boss.Health.CurrentHealth - 60);
        yield return Wait(.16f);
        Check(hud.Spills>oldSpills && hud.IsPouring,"Perda de vida dispara jatos de agua pelas bordas da HUD");
        Check(hud.Water.Level<oldWaterLevel && hud.Water.Level>boss.Health.Fraction,"Nivel de agua escoa gradualmente ate o HP real");
        Check(hud.SpilledVolume>.5f,"Volume derramado acompanha o dano grande");
        Capture("hud-spill.png");
        float waterClock=hud.AnimationClock, waterLevel=hud.Water.Level, surface=hud.Water.SurfaceSample;
        Time.timeScale=0; double waterPause=EditorApplication.timeSinceStartup+.25;
        while(EditorApplication.timeSinceStartup<waterPause)yield return null;
        Check(hud.AnimationClock==waterClock && hud.Water.Level==waterLevel && hud.Water.SurfaceSample==surface && hud.IsPouring,"Pausa congela nivel, corrente e derramamento da HUD");
        Time.timeScale=1;
        Check(combat.Enraged, "Fúria ativa abaixo de 40% de vida");
        int launched = combat.DropsLaunched, landed = combat.Landings;
        yield return Until(() => combat.Landings > landed, "salto em fúria");
        Check(combat.DropsLaunched - launched == 7, "Fúria mantém leque legível de sete gotas");
        // A visual capture is for art/layout integration, separate from the physics assertions.
        yield return Wait(.45f); Check(hud.IsCritical && Mathf.Abs(hud.DisplayedHealth-boss.Health.Fraction)<.001f, "HUD mostra furia e acompanha dano real");
        Capture();
        yield return Wait(12); StopMovie();
        Check(!hud.IsPouring,"Agua para de derramar depois que a perda de vida termina");
        Check(expression.Blinks >= 3,"Piscadas naturais se repetem durante o combate");
        yield return Until(()=>expression.IsBlinking,"piscar sincronizado");
        Check(hud.IsBlinking,"Retrato da HUD fecha os olhos junto com o sapo");
        Check(boss.GetComponentInChildren<SpriteRenderer>().sprite.name=="sapo-master","Corpo aprovado permanece intacto ao piscar");
        Capture("blink.png");
        float blinkClock=expression.AnimationClock; Time.timeScale=0;
        double blinkPause=EditorApplication.timeSinceStartup+.25;
        while(EditorApplication.timeSinceStartup<blinkPause)yield return null;
        Check(expression.AnimationClock==blinkClock && expression.IsBlinking && hud.IsBlinking,"Pausa congela a piscada do boss e da HUD");
        Time.timeScale=1;
        yield return Until(()=>!expression.IsBlinking,"reabertura dos olhos");
        Check(!hud.IsBlinking,"Retrato reabre os olhos com o sapo");
        Check(lake.ScrollDistance(4) > lake.GameplayCamera.orthographicSize * 2 * lake.GameplayCamera.aspect * 1.025f, "Rolagem atravessa a largura de um tile durante a sessão");
        var flow = Object.FindAnyObjectByType<ChapterFlow>(); flow.enabled = false;
        boss.Health.TakeDamage(9999); yield return Frames(4);
        Check(!combat.Active && Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).Length == 0, "Derrota remove todos os projéteis");
        yield return Until(() => waves.Phase == WaveController.RunPhase.ChapterCompleted, "conclusão do capítulo");
        Check(waves.Phase == WaveController.RunPhase.ChapterCompleted, "Vitória usa encerramento de capítulo nativo");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"), "Teste não grava progresso real");
    }
    private static void SpawnDrop(Vector2 p, Vector2 velocity)
    {
        var go = new GameObject("ValidationDrop"); go.AddComponent<Rigidbody2D>(); go.AddComponent<SpriteRenderer>();
        go.AddComponent<LakeDroplet>().Configure(combat, player.Health, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LakeStage/gota-lago-v03.png"), p, velocity, combat.WaterY);
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_HudWater_v07");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_HudWater_v07/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v07/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v07/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v07/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1; EditorApplication.isPlaying = false;
    }
}
