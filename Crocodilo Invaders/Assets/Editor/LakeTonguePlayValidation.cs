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
public static class LakeTonguePlayValidation
{
    private const string Key = "Crocodilo.Lake.TongueValidation";
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
    static LakeTonguePlayValidation()
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
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v10/TestSave");
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
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeTonguePlayValidation),true);
        yield return Until(()=>waves.ActiveBoss!=null && waves.ActiveBoss.Ready,"entrada do boss");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var tongue=combat.Tongue;var body=player.GetComponent<Rigidbody2D>();var hud=Object.FindAnyObjectByType<LakeBossBar>();
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"flutuacao");
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path==LakeStageTools.Stage,"Campo de moscas integrado na fase 2 real");
        Check(tongue.ArtReady && tongue.FlyCount==6,"Seis moscas persistentes com arte e lingua completas");
        Check(Enumerable.Range(0,6).Count(i=>tongue.Fly(i).IsRearFly)==1,"Apenas uma mosca e dedicada a ficar atras da nave");
        Check(Enumerable.Range(1,5).All(i=>!tongue.Fly(i).IsRearFly),"Cinco moscas ocupam pontos do mapa");
        Check(Enumerable.Range(0,6).All(i=>tongue.Fly(i).GetComponent<Collider2D>()==null),"Moscas nao causam dano nem bloqueiam tiros");
        var anchors=Enumerable.Range(0,6).Select(i=>tongue.Fly(i).Anchor).ToArray();var instances=Enumerable.Range(0,6).Select(i=>tongue.Fly(i)).ToArray();
        int idleFrame=tongue.Fly(2).Frame;yield return Wait(.12f);
        Check(tongue.AgitatingCount==0 && Enumerable.Range(1,5).All(i=>Vector2.Distance(tongue.Fly(i).transform.position,anchors[i])<.001f),"Moscas nao selecionadas ficam paradas nos pontos");
        Check(tongue.Fly(2).Frame!=idleFrame,"Moscas paradas continuam batendo asas");
        body.position=new Vector2(-3,1);StartMovie();int hp=player.Health.CurrentHealth,attempts=tongue.DamageAttempts;
        Check(combat.TryBeginTongue(1),"Sequencia simples inicia pelo aviso da mosca");
        var rear=tongue.TargetFly;
        Check(rear==tongue.Fly(0) && Vector2.Dot(rear.Anchor-body.position,(body.position-tongue.Mouth).normalized)>.35f,"Primeiro alvo e a unica mosca posicionada atras da nave");
        Vector2 selectedAnchor=rear.Anchor;yield return Wait(.12f);
        Check(tongue.AgitatingCount==1 && Vector2.Distance(rear.transform.position,rear.Anchor)>.025f,"Somente a mosca escolhida se agita");
        Check(Enumerable.Range(1,5).All(i=>Vector2.Distance(tongue.Fly(i).transform.position,anchors[i])<.001f),"Outras cinco moscas nao acompanham a agitacao");
        body.position+=Vector2.up*2;player.Health.SetInvulnerable(typeof(LakeTonguePlayValidation),false);yield return Wait(.1f);
        Check(rear.Anchor==selectedAnchor,"Alvo fica no ponto e nao segue a esquiva da nave");
        Check(!tongue.MouthOpen && boss.GetComponentInChildren<SpriteRenderer>().sprite.name=="sapo-master" && tongue.DamageAttempts==attempts,"Aviso usa apenas a mosca, com boca fechada e sem dano");
        Capture("one-active-fly.png");
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Locked,"fim do aviso");
        float clock=tongue.Clock,hudClock=hud.AnimationClock;Vector3 pausedFly=rear.transform.position;
        Time.timeScale=0;double pause=EditorApplication.timeSinceStartup+.25;
        while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(tongue.Clock==clock && rear.transform.position==pausedFly && hud.AnimationClock==hudClock,"Pausa congela aviso, moscas e HUD");Time.timeScale=1;
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Extend,"abertura e disparo");
        tongue.SendMessage("LateUpdate");
        Check(tongue.MouthOpen && tongue.Extension>0 && tongue.Skin.Visible,"Boca abre com a lingua ja estendida no mesmo passo");Capture("instant-mouth-shot.png");
        yield return Until(()=>!tongue.Busy,"primeira sequencia");
        Check(player.Health.CurrentHealth==hp,"Esquiva para fora da linha evita a linguada rapida");
        Check(tongue.Strikes==1 && tongue.CapturedFlies==1,"Uma selecao produz um unico golpe e captura");
        Check(tongue.FlyCount==6 && Object.FindObjectsByType<LakeTongueFly>(FindObjectsSortMode.None).Length==6 && tongue.AgitatingCount==0,"Moscas do mapa permanecem apos o ataque sem duplicar objetos");
        Check(Enumerable.Range(0,6).All(i=>tongue.Fly(i)==instances[i]),"Campo reutiliza as mesmas seis moscas");

        body.position=new Vector2(-3,1);yield return Frames(2);Check(combat.TryBeginTongue(1),"Segundo aviso inicia normalmente");
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Locked,"trava do acerto");
        body.position=Vector2.Lerp(tongue.Mouth,tongue.Aim,.86f);hp=player.Health.CurrentHealth;int hits=tongue.Hits;
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Extend,"golpe visivel");yield return Wait(.06f);
        Check(player.Health.CurrentHealth==hp-2 && tongue.Hits==hits+1,"Lingua rapida causa exatamente dois HP uma vez");
        tongue.SendMessage("LateUpdate");Check(tongue.Skin.Visible && tongue.Skin.SurfaceCount==1 && Vector2.Distance(tongue.Skin.RootPoint,tongue.Mouth)<.02f,"Asset inteiro segue encaixado na boca durante o disparo rapido");Capture("fast-tongue-strike.png");
        yield return Until(()=>!tongue.Busy,"recolhimento");Check(player.Health.CurrentHealth==hp-2 && !tongue.Skin.Visible,"Recolhimento esconde a lingua sem repetir dano");
        player.Health.Heal(999);player.Health.SetInvulnerable(typeof(LakeTonguePlayValidation),true);

        body.position=new Vector2(-3,1);yield return Frames(2);int strikes=tongue.Strikes,caught=tongue.CapturedFlies;
        Check(combat.TryBeginTongue(3) && tongue.SequenceLength==3,"Sequencia percorre tres alvos do campo");
        var selected=new HashSet<LakeTongueFly>();int rearSelections=0;bool oneAtATime=true,positionsFixed=true,sawMap=false;bool capturedView=false;
        while(tongue.Busy)
        {
            if(tongue.AgitatingCount>1)oneAtATime=false;
            for(int i=1;i<6;i++)if(tongue.Fly(i).Anchor!=anchors[i] || (!tongue.Fly(i).Focused && !tongue.Fly(i).Caught && Vector2.Distance(tongue.Fly(i).transform.position,anchors[i])>.001f))positionsFixed=false;
            if(tongue.TargetFly!=null && (tongue.CurrentPhase==LakeTongueAttack.Phase.Tracking || tongue.CurrentPhase==LakeTongueAttack.Phase.Locked))
            {
                var current=tongue.TargetFly;
                if(selected.Add(current)) { if(current.IsRearFly)rearSelections++;else sawMap=true; }
                if(!current.IsRearFly && tongue.PhaseTime>.1f && !capturedView) { Capture("map-fly-warning.png");capturedView=true; }
            }
            yield return null;
        }
        Check(oneAtATime && positionsFixed,"Durante toda a sequencia so o alvo ativo se move e pontos do mapa permanecem fixos");
        Check(selected.Count==3 && rearSelections==1 && sawMap,"Sequencia usa uma atras da nave e depois moscas distintas do mapa");
        Check(tongue.Strikes==strikes+3 && tongue.CapturedFlies==caught+3,"Cada mosca agitada recebe exatamente uma linguada");
        float interval=tongue.LastStrikeTime-tongue.PreviousStrikeTime;
        Check(interval>=player.Dash.Cooldown && interval<1.5f,"Golpes acelerados mantem intervalo suficiente para recarregar dash");
        Check(tongue.FlyCount==6 && Enumerable.Range(0,6).All(i=>tongue.Fly(i)==instances[i]),"Sequencia nao cria moscas extras");

        body.position=new Vector2(-3,1);yield return Frames(2);Check(combat.TryBeginTongue(1),"Aviso para testar dash");
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Locked && tongue.PhaseTime>=LakeTongueAttack.LockTime-.04f,"fim do aviso rapido");
        body.position=Vector2.Lerp(tongue.Mouth,tongue.Aim,.55f);hp=player.Health.CurrentHealth;attempts=tongue.DamageAttempts;
        player.Health.SetInvulnerable(typeof(LakeTonguePlayValidation),false);Check(player.TryDash(),"Dash nativo pode atravessar o golpe rapido");
        yield return Until(()=>!tongue.Busy,"dash");Check(tongue.DamageAttempts>attempts && player.Health.CurrentHealth==hp,"Dash protege sem dano atrasado no recolhimento (HP "+player.Health.CurrentHealth+"/"+hp+", contatos "+tongue.DamageAttempts+"/"+attempts+")");player.Health.SetInvulnerable(typeof(LakeTonguePlayValidation),true);
        body.position=new Vector2(Mathf.Min(combat.Arena.xMax-.8f,boss.transform.position.x+1.7f),1);yield return Frames(2);Check(combat.TryBeginTongue(1),"Alvo atras da nave funciona tambem a direita do boss");
        yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Extend,"disparo a direita");tongue.SendMessage("LateUpdate");
        Check(boss.GetComponentInChildren<SpriteRenderer>().flipX && tongue.Mouth.x>boss.transform.position.x && tongue.Skin.Visible,"Boca e lingua viram e disparam juntas para a direita");Capture("fast-shot-right.png");
        yield return Until(()=>!tongue.Busy,"recolhimento a direita");Check(!boss.GetComponentInChildren<SpriteRenderer>().flipX,"Orientacao normal retorna apos recolher");
        body.position=new Vector2(-3,1);yield return Frames(2);Check(combat.TryBeginTongue(1),"Campo aceita novo aviso a esquerda");yield return Until(()=>!tongue.Busy,"ultimo golpe simples");
        StopMovie();

        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tongue,"sequencia natural");
        Check(tongue.SequenceLength==3 && tongue.FlyCount==6,"Ciclo natural usa tres alvos e campo persistente");
        yield return Until(()=>!tongue.Busy,"fim da sequencia natural");boss.Health.TakeDamage(boss.Health.CurrentHealth-60);
        yield return Until(()=>combat.Landings>0,"salto natural");Check(combat.DropsLaunched==7,"Salto e sete gotas da furia continuam no repertorio");Check(!combat.TryBeginTongue(1),"Golpes nao sobrepoem impacto e gotas");
        bool separated=true;
        yield return Until(()=>{if(combat.CurrentState==SapoDoPneuCombat.State.Tongue && Object.FindObjectsByType<LakeDroplet>(FindObjectsSortMode.None).Length>0)separated=false;return combat.CurrentState==SapoDoPneuCombat.State.Tongue;},"nova sequencia apos gotas");
        Check(separated && tongue.SequenceLength==4,"Furia usa quatro alvos depois que as gotas terminam");Capture("gameplay.png");
        var flow=Object.FindAnyObjectByType<ChapterFlow>();flow.enabled=false;boss.Health.TakeDamage(9999);yield return Frames(4);
        Check(!tongue.Busy && tongue.FlyCount==0 && Object.FindObjectsByType<LakeTongueFly>(FindObjectsSortMode.None).Length==0 && !tongue.Skin.Visible,"Derrota do boss remove todo o campo e a lingua");Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Teste nao altera progresso real");

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"novo menu");menu=Object.FindAnyObjectByType<DeveloperMenuView>();yield return Until(()=>{menu.Open();return menu.IsVisible;},"novo DEV");
        GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"novo loading");Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"novo boss");player=Object.FindAnyObjectByType<Player>();player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();tongue=combat.Tongue;body=player.GetComponent<Rigidbody2D>();yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"nova flutuacao");
        body.position=new Vector2(-3,1);player.Health.SetMaximumHealth(2,false);Check(combat.TryBeginTongue(1),"Encontro para validar golpe fatal");yield return Until(()=>tongue.CurrentPhase==LakeTongueAttack.Phase.Locked,"trava fatal");body.position=Vector2.Lerp(tongue.Mouth,tongue.Aim,.85f);
        yield return Until(()=>player==null || player.Health.IsDead,"golpe fatal");yield return Frames(3);
        Check(!tongue.Busy && combat.CurrentState==SapoDoPneuCombat.State.Stopped && tongue.FlyCount==0 && Object.FindObjectsByType<LakeTongueFly>(FindObjectsSortMode.None).Length==0 && !tongue.Skin.Visible,"Derrota do jogador limpa campo, aviso e lingua sem reiniciar combate");
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
        Directory.CreateDirectory("Docs/ArtPreviews/LagoDoPneu_FlyTargets_v10");
        File.WriteAllBytes("Docs/ArtPreviews/LagoDoPneu_FlyTargets_v10/" + filename, tex.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = old; rt.Release(); Object.Destroy(rt); Object.Destroy(tex);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void StartMovie()
    {
        Directory.CreateDirectory("Logs/LagoDoPneu/v10/Frames"); movieCamera = Camera.main;
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
        File.WriteAllBytes("Logs/LagoDoPneu/v10/Frames/frame_" + movieTimes.Count.ToString("D5") + ".png", moviePixels.EncodeToPNG());
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
        File.WriteAllText("Logs/LagoDoPneu/v10/Frames/frames.ffconcat", text.ToString());
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
        File.WriteAllText("Logs/LagoDoPneu/v10/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        Time.timeScale = 1;Time.maximumDeltaTime=previousMaxDeltaTime; EditorApplication.isPlaying = false;
    }
}
