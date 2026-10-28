using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class MacaquiniJetpackValidation
{
    private const string Key="Crocodilo.Jetpack.Validation";
    private static bool NaturalRun=>SessionState.GetBool(Key+".Natural",false);
    private static bool TransformationRun=>SessionState.GetBool(Key+".Transformation",false);
    private static bool ContinuityRun=>SessionState.GetBool(Key+".Continuity",false);
    private static bool HudRun=>SessionState.GetBool(Key+".HUD14",false);
    private static bool KickRun=>SessionState.GetBool(Key+".Kick20",false);
    private static bool EffectsRun=>SessionState.GetBool(Key+".Effects21",false);
    private static bool PendulumRun=>SessionState.GetBool(Key+".Pendulum19",false);
    private static bool EyesRun=>SessionState.GetBool(Key+".Eyes18",false);
    private static bool ChestRun=>SessionState.GetBool(Key+".Chest17",false);
    private static bool ScaleRun=>SessionState.GetBool(Key+".Scale16",false);
    private static int scaleSamples,scaleViolations;
    private static bool captureSuspended;
    private static Vector3 registeredWorldScale;
    private static bool RefinedHudRun=>SessionState.GetBool(Key+".HUD15",false);
    private static string Output=>EffectsRun?"Docs/ArtPreviews/MacaquiniKick_v21":KickRun?"Docs/ArtPreviews/MacaquiniKick_v20":PendulumRun?"Docs/ArtPreviews/MacaquiniPendulum_v19":EyesRun?"Docs/ArtPreviews/MacaquiniEyes_v18":ChestRun?"Docs/ArtPreviews/MacaquiniChest_v17":ScaleRun?"Docs/ArtPreviews/MacaquiniScale_v16":RefinedHudRun?"Docs/ArtPreviews/MacaquiniHUD_v15":HudRun?"Docs/ArtPreviews/MacaquiniHUD_v14":ContinuityRun?"Docs/ArtPreviews/Macaquini_v13":TransformationRun?"Docs/ArtPreviews/Macaquini_v12":NaturalRun?"Docs/ArtPreviews/Macaquini_v11":"Docs/ArtPreviews/Macaquini_v10";
    private static string Recording=>EffectsRun?"Logs/JungleStage/Kick21":KickRun?"Logs/JungleStage/Kick20":PendulumRun?"Logs/JungleStage/Pendulum19":EyesRun?"Logs/JungleStage/Eyes18":ChestRun?"Logs/JungleStage/Chest17":ScaleRun?"Logs/JungleStage/Scale16":RefinedHudRun?"Logs/JungleStage/HUD15":HudRun?"Logs/JungleStage/HUD14":ContinuityRun?"Logs/JungleStage/ContinuityV13":TransformationRun?"Logs/JungleStage/TransformationV12":NaturalRun?"Logs/JungleStage/MotionV11":"Logs/JungleStage/JetpackMotion";
    private static int lastTearFrame,backwards,blankFrames;
    private static readonly HashSet<int> tearFrames=new HashSet<int>();
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors;
    private static double deadline;
    private static Boss boss;
    private static MacaquiniCombat combat;
    private static MacaquiniFlightPresentation actor;
    private static MacaquiniPhaseTwo phase;
    private static Player player;
    private static Camera camera;
    private static RenderTexture capture;
    private static Texture2D pixels;
    private static List<float> times;
    private static float recordingStart,nextCapture;
    private static JungleMotionAudioCapture audio;
    static MacaquiniJetpackValidation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run(){SessionState.SetBool(Key+".HUD15",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab").GetComponentInChildren<MacaquiniFrameAperture>(true)!=null);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(JungleStageTools.BossPath);SessionState.SetBool(Key+".HUD14",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab").GetComponentInChildren<MacaquiniBossBar>(true)!=null);SessionState.SetBool(Key+".Kick20",prefab.GetComponent<MacaquiniSwingKick>()!=null);SessionState.SetBool(Key+".Pendulum19",prefab.GetComponent<MacaquiniCombat>().PendulumEnabled);SessionState.SetBool(Key+".Eyes18",AssetDatabase.GetAssetPath(new SerializedObject(prefab.GetComponent<MacaquiniFlightPresentation>()).FindProperty("transformation").GetArrayElementAtIndex(35).objectReferenceValue).Contains("Macaquini_v18"));SessionState.SetBool(Key+".Chest17",AssetDatabase.GetAssetPath(new SerializedObject(prefab.GetComponent<MacaquiniFlightPresentation>()).FindProperty("transformation").GetArrayElementAtIndex(18).objectReferenceValue).Contains("Macaquini_v17"));SessionState.SetBool(Key+".Scale16",prefab.GetComponent<MacaquiniFlightPresentation>().UniformSpriteScale);SessionState.SetBool(Key+".Natural",prefab.GetComponent<MacaquiniFlightPresentation>().NaturalMotion);SessionState.SetBool(Key+".Transformation",prefab.GetComponent<MacaquiniTransformationPresentation>()!=null);SessionState.SetBool(Key+".Continuity",prefab.GetComponent<MacaquiniFlightPresentation>().ChronologicalTransformation);EditorSceneManager.OpenScene(MacaquiniJetpackTools.Test);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    private static void Begin()
    {
        checks=errors=scaleSamples=scaleViolations=0;deadline=EditorApplication.timeSinceStartup+200;Application.runInBackground=true;
        lastTearFrame=-1;backwards=blankFrames=0;tearFrames.Clear();
        Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;
        steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(4);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/JungleStage/JetpackIsolatedSave");
        player=Object.FindAnyObjectByType<Player>();player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);
        var waves=Object.FindAnyObjectByType<WaveController>();yield return Until(()=>waves.ActiveBoss!=null&&waves.ActiveBoss.Ready,"boss ready");
        boss=waves.ActiveBoss;combat=boss.GetComponent<MacaquiniCombat>();actor=boss.GetComponent<MacaquiniFlightPresentation>();phase=boss.GetComponent<MacaquiniPhaseTwo>();
        Check(actor!=null&&!actor.VineVisible&&actor.JetpackAttached,"Primeira forma usa jetpack sem cipó");
        Check(!boss.transform.Find("Visual").gameObject.activeSelf,"Rig modular oculto; anatomia pertence ao desenho completo");
        Check(Object.FindAnyObjectByType<BossHUD>().ActiveBar.name=="MacaquiniBar","HUD usa a barra exclusiva Macaquini");
        var jungleHud=Object.FindAnyObjectByType<BossHUD>().ActiveBar.JungleView;
        if(HudRun){Check(jungleHud!=null&&jungleHud.PortraitSize==104,"Retrato ampliado para 104px no HUD exclusivo");Check(jungleHud.FirstPortrait!=jungleHud.SecondPortrait&&jungleHud.FormBlend==0,"Fase um tem retrato da banana infectada e segunda forma distinta");}
        if(RefinedHudRun)
        {
            var frameImage=jungleHud.transform.Find("JungleFrameOne").GetComponent<UnityEngine.UI.Image>();
            Check(jungleHud.transform.Find("PortraitWindow").GetSiblingIndex()<frameImage.transform.GetSiblingIndex(),"Os dois retratos ficam atrás das molduras, preservando cipós e contorno");
            Check(frameImage.useSpriteMesh&&frameImage.sprite.triangles.Length>100,"Moldura usa abertura real no mesh para mostrar o retrato ao fundo");
            Check(jungleHud.transform.Find("PortraitWindow").GetComponent<UnityEngine.UI.RectMask2D>()!=null,"Retrato permanece contido na janela sem vazar entre as folhas externas");
            var meter=jungleHud.transform.Find("hp_fill").GetComponent<UnityEngine.UI.Image>();
            Check(meter.fillOrigin==0&&meter.rectTransform.sizeDelta==new Vector2(337,18),"Vida preenche da esquerda para direita e se encaixa no interior da moldura");
        }
        Check(Object.FindAnyObjectByType<JungleBackdrop>().ParallaxLayerCount==4,"Selva preserva os quatro níveis de parallax");
        if(ContinuityRun)Check(actor.TransformationDrawingCount==42&&MacaquiniContinuityV13Tools.ValidateSources(),"42 poses cronológicas, casca sem regressão e mesma mão na busca/pegada");
        if(ScaleRun){MacaquiniScaleV16Tools.Validate();registeredWorldScale=boss.transform.Find("CompletePaintedBody").lossyScale;}
        StartRecord();
        if(NaturalRun){Check(actor.NaturalMotion&&actor.MotionDrawingCount==32,"Voo usa 32 desenhos completos e fase contínua, com expressões integradas à casca");boss.HoldCombat(true);yield return ObserveMotion(3.4f);boss.HoldCombat(false);}
        yield return Wait(.6f);
        int frame=actor.Frame;float heldPhase=actor.MotionPhase;Time.timeScale=0;yield return WallWait(.25f);Check(actor.Frame==frame,"Pausa congela os quadros de voo");if(NaturalRun)Check(actor.MotionPhase==heldPhase,"Pausa também congela a fase contínua, sem salto ao retomar");Time.timeScale=1;
        int releases=combat.Releases;combat.ForceAttack(MacaquiniCombat.Attack.JetStrafe);
        yield return Until(()=>combat.Releases>=releases+3,"native strafe releases");
        Check(combat.Releases==releases+3,"Rajada aérea usa três Animation Events nativos sem duplicação");
        Check(combat.CurrentAttack==MacaquiniCombat.Attack.JetStrafe&&boss.Movement.IsDashing,"Disparos acontecem durante a manobra vertical");
        combat.ForceAttack(MacaquiniCombat.Attack.JetDash);Vector3 initial=boss.transform.position;
        yield return Wait(.2f);Check(combat.IsTelegraphing&&Object.FindObjectsByType<MacaquiniHeatHazard>(FindObjectsSortMode.None).Length==1,"Rasante avisa uma única faixa antes de acelerar");
        yield return Until(()=>!combat.IsTelegraphing,"dash ignition");yield return Wait(.45f);
        Check(Vector3.Distance(initial,boss.transform.position)>2,"Rasante movimenta o corpo físico pela faixa anunciada");
        combat.ForceAttack(MacaquiniCombat.Attack.ExhaustGates);yield return Wait(.2f);
        Check(Object.FindObjectsByType<MacaquiniHeatHazard>(FindObjectsSortMode.None).Length==2,"Descarga lança dois reservatórios e deixa corredor de fuga");
        var hazard=Object.FindObjectsByType<MacaquiniHeatHazard>(FindObjectsSortMode.None)[0];
        float column=hazard.GetComponent<LineRenderer>().GetPosition(0).x;
        player.GetComponent<Rigidbody2D>().position=new Vector2(column,0);
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),false);int before=player.Health.CurrentHealth;
        yield return Wait(1.65f);Check(player.Health.CurrentHealth==before-2,"Descarga causa dois de dano uma única vez no alvo da faixa");
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);
        player.GetComponent<Rigidbody2D>().position=new Vector2(-6,0);
        combat.ForceAttack(MacaquiniCombat.Attack.ExhaustGates);yield return Wait(.15f);
        float left=Object.FindObjectsByType<MacaquiniHeatHazard>(FindObjectsSortMode.None).Min(h=>h.GetComponent<LineRenderer>().GetPosition(0).x);
        player.GetComponent<Rigidbody2D>().position=new Vector2(left+1.45f,0);
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),false);before=player.Health.CurrentHealth;yield return Wait(1.7f);
        Check(player.Health.CurrentHealth==before,"Corredor entre as descargas não causa dano");player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);
        player.GetComponent<Rigidbody2D>().position=new Vector2(-6,0);
        boss.Health.TakeDamage(45);yield return Wait(.35f);float lowSmoke=actor.SmokeRate;Check(actor.DamageLevel>=.25f&&lowSmoke>0,"Com 75% de vida começam as falhas e a fumaça");
        if(HudRun)Check(Mathf.Abs(jungleHud.ShownHealth-.75f)<.001f,"Preenchimento mostra 75% da vida real");
        yield return Snapshot("damage_75");boss.Health.TakeDamage(45);yield return Wait(.35f);float mediumSmoke=actor.SmokeRate;Check(mediumSmoke>lowSmoke,"Fumaça aumenta com 50% de vida");
        if(HudRun)Check(Mathf.Abs(jungleHud.ShownHealth-.5f)<.001f,"Preenchimento mostra 50% da vida real");
        boss.Health.TakeDamage(45);yield return Wait(.45f);Check(actor.SmokeRate>mediumSmoke&&actor.DamageLevel>=.75f,"Pane grave com 25% de vida intensifica fumaça e faíscas");
        yield return Snapshot("damage_25");
        int kills=waves.Score.Kills,points=waves.Score.Points;float beforeFall=boss.transform.position.y;boss.Health.TakeDamage(45);
        Check(phase.CurrentStage==MacaquiniPhaseTwo.Stage.Tearing&&boss.Health.CurrentHealth==180&&boss.Health.IsInvulnerable,"Primeiro esgotamento inicia transformação invulnerável e restaura a segunda barra");
        Check(waves.Score.Kills==kills&&waves.Score.Points==points&&combat.ThreatRoot==null,"Primeira forma não pontua e limpa todas as ameaças");
        if(HudRun)Check(jungleHud.HoldingEmpty&&jungleHud.ShownHealth==0,"Barra permanece vazia durante a transformação apesar de restaurar HP lógico");
        yield return Until(()=>actor.TearProgress>.25f,"cap removed");yield return Snapshot("cap_removed");
        var transformation=boss.GetComponent<MacaquiniTransformationPresentation>();
        if(TransformationRun){Check(transformation.CapReleases==1&&transformation.PackReleases==0,"Evento nativo solta apenas o topo na primeira etapa");var cap=transformation.ActiveDebris.Single();Check(cap.GetComponent<Rigidbody2D>()!=null&&cap.GetComponent<Collider2D>()==null,"Topo cai com gravidade e não causa dano por colisão");Vector3 capPosition=cap.transform.position;Time.timeScale=0;yield return WallWait(.25f);Check(cap!=null&&Vector3.Distance(cap.transform.position,capPosition)<.001f,"Pausa congela a queda física do topo");Time.timeScale=1;yield return Wait(.3f);Check(cap!=null&&cap.GetComponent<Rigidbody2D>().linearVelocity.y<0,"Topo continua caindo durante a troca dos quadros");}
        float progress=actor.TearProgress;Time.timeScale=0;yield return WallWait(.25f);Check(Mathf.Abs(actor.TearProgress-progress)<.001f,"Pausa congela a transformação nativa");Time.timeScale=1;
        yield return Until(()=>actor.TearProgress>.52f,"both hands peel");yield return Snapshot("two_hands_peel");
        if(HudRun){Check(jungleHud.FormBlend>0&&jungleHud.FormBlend<1,"Moldura e retrato atravessam valores intermediários suaves");float blend=jungleHud.FormBlend;Time.timeScale=0;yield return WallWait(.2f);Check(Mathf.Approximately(blend,jungleHud.FormBlend),"Pausa congela a transição visual da barra");Time.timeScale=1;}
        if(TransformationRun){yield return Until(()=>actor.TearProgress>.65f,"physical pack release");Check(transformation.CapReleases==1&&transformation.PackReleases==1,"Casca e jetpack soltam juntos uma única vez");Check(transformation.PiecesAtMapBottom==1,"Topo só é removido depois de cair ao fundo do mapa");var pack=transformation.ActiveDebris.Single();Check(pack.GetComponent<Collider2D>()==null&&pack.GetComponentsInChildren<ParticleSystem>().Length>0,"Conjunto descartado mantém fumaça e não atinge o jogador");Vector3 packPosition=pack.transform.position;Time.timeScale=0;yield return WallWait(.25f);Check(pack!=null&&Vector3.Distance(pack.transform.position,packPosition)<.001f,"Pausa também congela o conjunto com jetpack");Time.timeScale=1;yield return Snapshot("falling_jetpack");}
        yield return Until(()=>actor.TearProgress>.78f,"pack discarded");Check(!actor.JetpackAttached&&!actor.VineVisible,"Casca e jetpack saem antes de agarrar o cipó");yield return Snapshot("pack_discarded");
        if(TransformationRun)Check(transformation.Roars==1&&(ScaleRun?Mathf.Approximately(transformation.StrengthScale,1):transformation.StrengthScale>1.055f),ScaleRun?"Grito preservado sem redimensionar o corpo":"Grito nativo acompanha expansão da postura e aumento de força");
        if(ContinuityRun){
            var native=boss.GetComponent<Animator>();native.enabled=false;
            // Allow the physical body to consume the last authored progress before measuring a frozen curve.
            yield return Wait(.06f);yield return Frames(2);
            float held=actor.TearProgress;actor.AdvanceTearMilestone(held);int drawing=actor.Frame;Vector3 position=boss.transform.position;
            var progressData=new SerializedObject(boss.GetComponent<MacaquiniPresentation>());progressData.FindProperty("tearProgress").floatValue=.05f;progressData.ApplyModifiedPropertiesWithoutUndo();yield return Frames(3);
            Check(actor.TearProgress>=held&&actor.Frame>=drawing&&Vector3.Distance(position,boss.transform.position)<.04f,"Reset temporário da curva não faz desenho ou corpo voltarem para trás");
            progressData.Update();progressData.FindProperty("tearProgress").floatValue=held;progressData.ApplyModifiedPropertiesWithoutUndo();native.enabled=true;
        }
        yield return Until(()=>phase.CurrentStage==MacaquiniPhaseTwo.Stage.HalfShell,"second form");
        if(ContinuityRun){Check(backwards==0&&lastTearFrame==41,"Transformação chega ao último desenho sem voltar a quadros anteriores");Check(blankFrames==0&&tearFrames.Count>=30,"Corpo permanece visível/opaco durante toda a sequência; intermediários reais observados");}
        Check(boss.transform.position.y<beforeFall-.5f,"Sem o motor, o corpo cai e o cipó recebe o peso");
        yield return Wait(.3f);Check(actor.HalfShell&&actor.VineVisible&&!actor.JetpackAttached&&actor.SmokeRate==0,"Segunda forma está no cipó sem motor nem nova fumaça");
        if(TransformationRun){yield return Until(()=>transformation.PiecesAtMapBottom==2,"both props reach map bottom");Check(transformation.LastRemovalTop<transformation.MapBottom&&transformation.ActiveDebris.Length==0,"As duas peças percorrem o mapa inteiro e só então são removidas");}
        if(NaturalRun){Check(actor.MotionDrawingCount==24,"Cipó usa 24 desenhos selecionados por estabilidade dos pés");boss.HoldCombat(true);yield return ObserveMotion(2.5f);boss.HoldCombat(false);}
        yield return Snapshot("vine_form");yield return Until(()=>boss.Ready,"second form ready");
        if(HudRun){Check(jungleHud.FormBlend==1&&!jungleHud.HoldingEmpty,"Segunda forma usa moldura de espinhos e retrato do grito");Check(Mathf.Abs(jungleHud.ShownHealth-1)<.001f,"Vida recarrega completamente ao entrar na segunda forma");boss.Health.TakeDamage(45);yield return Wait(.4f);Check(Mathf.Abs(jungleHud.ShownHealth-.75f)<.001f,"Segunda barra também acompanha o dano real");yield return Snapshot("phase_two_75");}
        combat.ForceAttack(MacaquiniCombat.Attack.BananaFan);releases=combat.Releases;yield return Until(()=>combat.Releases>releases,"second form fan");
        Check(combat.ThreatRoot.GetComponentsInChildren<Projectile>().Length==7,"Segunda forma mantém leque de sete bananas");yield return Wait(.6f);if(ScaleRun)Check(scaleSamples>100&&scaleViolations==0,"Voo, rasgo e cipó mantêm 80 PPU e escala mundial constante em "+scaleSamples+" amostras");
        if(combat.PendulumEnabled)yield return TestPendulum();
        if(KickRun)yield return TestKick();
        StopRecord();
        if(NaturalRun)Check(AssetDatabase.GetAssetPath(actor.CurrentSprite).StartsWith(MacaquiniMotionV11Tools.Art)&&!actor.CurrentSprite.name.Contains("attack"),"Disparo preserva a postura contínua das pernas em vez de trocar para o ataque antigo");
        if(TransformationRun){boss.HoldCombat(true);yield return TestTransformationCancellation();}
        boss.Health.TakeDamage(180);
        Check(waves.Score.Kills==kills+1&&waves.Score.Points==points+40000,"Só a derrota final concede um abate e 40000 pontos");
        Check(combat.ThreatRoot==null&&!combat.IsTelegraphing,"Morte final remove projéteis, descargas e avisos");
        Check(boss.GetComponentsInChildren<AudioSource>().Where(a=>a.name=="JetpackMotor").All(a=>!a.isPlaying),"Morte interrompe o motor");
        if(HudRun){yield return Frames(3);Check(Object.FindAnyObjectByType<BossHUD>().ActiveBar==null,"Derrota final oculta a barra exclusiva");}
    }
    private static IEnumerator TestPendulum()
    {
        var input=player.GetComponent<PlayerInputReader>();input.enabled=false;
        var body=player.GetComponent<Rigidbody2D>();player.Movement.SetMoveInput(Vector2.zero);body.position=Vector2.zero;
        combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);yield return Frames(2);var swing=combat.Pendulum;
        Check(swing!=null&&swing.PassCount==1&&!swing.Dangerous,"Fase dois cria balanço com aviso e uma passagem antes da meia vida");
        Check(swing.PaintedSegments>0,"Balanço usa cipós desenhados com folhas em vez de linha luminosa");
        Check(combat.ThreatRoot.GetComponentsInChildren<Projectile>().Length==0&&Object.FindObjectsByType<JungleVineHazard>(FindObjectsSortMode.None).Length==0,"Balanço limpa projéteis e não sobrepõe cipós antigos");
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),false);int hp=player.Health.CurrentHealth;
        yield return Wait(.45f);Check(player.Health.CurrentHealth==hp&&!swing.Dangerous,"Aviso não causa dano");yield return Snapshot("pendulum_warning");
        float frozen=swing.Age;Time.timeScale=0;yield return WallWait(.2f);Check(Mathf.Approximately(swing.Age,frozen),"Pausa congela aviso e posição do pêndulo");Time.timeScale=1;
        player.Movement.SetMoveInput(Vector2.right);
        yield return Until(()=>swing==null||swing.DamageHits>0,"walking into pendulum");
        Check(swing!=null&&swing.DamageHits==1&&player.Health.CurrentHealth==hp-2,"Cipó varre a arena e andar sem dash recebe dois de dano");
        yield return Wait(.15f);Check(player.Health.CurrentHealth==hp-2,"Mesmo balanço não aplica dano repetido");
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);player.Movement.SetMoveInput(Vector2.zero);body.position=Vector2.zero;
        combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);yield return Frames(2);swing=combat.Pendulum;
        hp=player.Health.CurrentHealth;player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),false);
        captureSuspended=true;
        yield return Until(()=>swing.FirstCrossingIn(body.position)<.15f,"dash crossing timing");
        string dashEvidence=$" hp={hp}/{player.Health.CurrentHealth} age={swing.Age} crossing={swing.FirstCrossingIn(body.position)} position={body.position} angle={swing.Angle}";
        player.Movement.SetMoveInput(new Vector2(Mathf.Sign(swing.Angle),0));Check(player.TryDash(),"Dash real do jogador inicia antes da passagem");
        yield return Wait(.28f);captureSuspended=false;Check(player.Health.CurrentHealth==hp&&swing.DamageHits==0,"Janela real de invulnerabilidade atravessa o cipó sem dano"+dashEvidence+$" endHp={player.Health.CurrentHealth} hits={swing.DamageHits} end={body.position}");
        player.Movement.SetMoveInput(Vector2.zero);yield return Snapshot("pendulum_dash");
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);body.position=Vector2.zero;boss.Health.TakeDamage(50);
        combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);yield return Frames(2);swing=combat.Pendulum;
        Check(swing.PassCount==2,"Abaixo de 50% da segunda barra o cipó volta para uma segunda passagem");
        yield return Until(()=>swing.Age>2.85f,"turnaround warning");
        Check(!swing.Dangerous&&player.Dash.IsReady,"Retorno tem aviso sem dano e tempo para recuperar o dash");yield return Snapshot("pendulum_turn");
        yield return Until(()=>swing.PassIndex==1&&swing.Dangerous,"return sweep");yield return Wait(.6f);yield return Snapshot("pendulum_return");
        combat.StopCombat();yield return Frames(3);
        Check(swing==null&&Object.FindObjectsByType<MacaquiniPendulumVine>(FindObjectsSortMode.None).Length==0,"Cancelar combate remove o balanço e todos os segmentos");
        input.enabled=true;
    }
    private static IEnumerator TestKick()
    {
        var input=player.GetComponent<PlayerInputReader>();input.enabled=false;player.Movement.SetMoveInput(Vector2.zero);
        var body=player.GetComponent<Rigidbody2D>();body.position=Vector2.zero;
        combat.SetTarget(player.Health);yield return Until(()=>boss.Ready,"kick ready");
        combat.ForceAttack(MacaquiniCombat.Attack.SwingKick);yield return Frames(3);
        var kick=boss.GetComponent<MacaquiniSwingKick>();
        Check(kick.Running&&kick.PoseCount==6&&kick.CurrentStage==MacaquiniSwingKick.Stage.Windup,"Chute tem seis poses completas e começa ganhando impulso");
        var home=kick.Anchor;int hp=player.Health.CurrentHealth;
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),false);
        yield return Wait(.5f);Check(!kick.Dangerous&&player.Health.CurrentHealth==hp,"Embalo no cipó não causa dano");
        float frozen=kick.Age;Time.timeScale=0;yield return WallWait(.2f);Check(kick.Age==frozen,"Pausa congela a sequência de chute");Time.timeScale=1;
        yield return Until(()=>kick.Age>2.15f,"kick warning");yield return Snapshot("kick_windup");
        var locked=kick.LockedAim;yield return Until(()=>kick.Dangerous,"kick release");yield return Wait(.14f);yield return Snapshot("kick_release");
        Check(boss.GetComponentsInChildren<LineRenderer>().Any(line=>line.name=="KickWindArc"&&line.enabled)&&boss.GetComponentsInChildren<LineRenderer>().Count(line=>line.name.StartsWith("KickSpeedWind")&&line.enabled)==3,"Chute exibe arco no pé e três rastros de vento");
        yield return Until(()=>kick.DamageHits>0||!kick.Dangerous,"kick crossing");
        Check(kick.DamageHits==1&&player.Health.CurrentHealth==hp-3,"Pé atravessa a arena e acerta uma vez com três de dano");
        Check(Vector2.Distance(locked,kick.LockedAim)<.001f,"Chute mantém a direção avisada em vez de perseguir o jogador");
        yield return Until(()=>kick.CurrentStage==MacaquiniSwingKick.Stage.Offscreen,"kick offscreen");
        Check(!kick.Dangerous,"Ao sair da tela o chute deixa de causar dano");
        yield return Until(()=>kick.CurrentStage==MacaquiniSwingKick.Stage.Background,"background lap");yield return Wait(.9f);yield return Snapshot("kick_background");
        Check(kick.PerspectiveScale<.34f&&boss.transform.Find("CompletePaintedBody").localScale.x<.34f&&!kick.Dangerous&&!boss.Health.TakeDamage(1),"Volta ao fundo é menor, atrás das árvores e sem colisão de combate");
        var bossBody=boss.GetComponent<Rigidbody2D>();float previousX=bossBody.position.x;yield return Until(()=>Mathf.Abs(bossBody.position.x-previousX)>.05f,"background advances after snapshot");Check(kick.CurrentStage==MacaquiniSwingKick.Stage.Background&&boss.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().flipX==(bossBody.position.x>previousX),"No fundo o macaco olha na direção do deslocamento");
        yield return Until(()=>kick.CurrentStage==MacaquiniSwingKick.Stage.Catch,"catch vine");yield return Wait(.4f);yield return Snapshot("kick_catch");
        yield return Until(()=>!kick.Running,"kick return");
        Check(Vector2.Distance(boss.transform.position,home)<.2f&&Mathf.Approximately(kick.PerspectiveScale,1),"Retorno restaura posição e proporção normal do boss");
        player.Health.SetInvulnerable(typeof(MacaquiniJetpackValidation),true);
        combat.ForceAttack(MacaquiniCombat.Attack.SwingKick);yield return Frames(3);combat.StopCombat();yield return Frames(3);
        Check(!kick.Running&&Mathf.Approximately(kick.PerspectiveScale,1)&&!boss.Health.IsInvulnerable,"Cancelar chute limpa sequência e proteção temporária");
        input.enabled=true;
    }
    private static IEnumerator TestTransformationCancellation()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(JungleStageTools.BossPath);
        for(int mode=0;mode<3;mode++){
            var targetRoot=new GameObject("TransformationCancellationTarget");targetRoot.layer=LayerMask.NameToLayer("Player");targetRoot.transform.position=new Vector3(-6,0);
            var target=targetRoot.AddComponent<Health>();
            var isolated=Object.Instantiate(prefab,new Vector3(4,1),Quaternion.identity).GetComponent<Boss>();isolated.BeginBattle(target);
            yield return Until(()=>isolated.Ready,"isolated transformation ready");isolated.Health.TakeDamage(180);
            var fx=isolated.GetComponent<MacaquiniTransformationPresentation>();
            yield return Until(()=>mode==1?fx.PackReleases==1:fx.CapReleases==1,"isolated physical debris");
            var pieces=fx.ActiveDebris;Check(pieces.Length>0,"Peça física existe antes do cancelamento "+mode);
            if(mode==0)target.TakeDamage(9999);else if(mode==1)isolated.gameObject.SetActive(false);else Object.Destroy(isolated.gameObject);
            yield return Frames(3);Check(pieces.All(p=>p==null),"Cancelamento limpa peças em queda: "+(mode==0?"morte do alvo":mode==1?"disable":"destruição"));
            if(isolated!=null)Object.Destroy(isolated.gameObject);Object.Destroy(targetRoot);yield return Frames(2);
        }
    }
    private static void StartRecord()
    {
        Directory.CreateDirectory(Recording);camera=Camera.main;capture=new RenderTexture(1200,720,24);pixels=new Texture2D(1200,720,TextureFormat.RGB24,false);camera.targetTexture=capture;
        foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay)){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
        recordingStart=Time.unscaledTime;nextCapture=0;times=new List<float>();audio=camera.gameObject.AddComponent<JungleMotionAudioCapture>();audio.Begin();
    }
    private static void RecordFrame()
    {
        if(captureSuspended)return;
        if(ScaleRun&&times!=null&&actor!=null&&actor.CurrentSprite!=null)
        {
            scaleSamples++;
            if(Mathf.Abs(actor.CurrentSprite.pixelsPerUnit-80)>.001f||Vector3.Distance(boss.transform.Find("CompletePaintedBody").lossyScale,registeredWorldScale)>.001f)scaleViolations++;
        }
        if(times!=null&&ContinuityRun&&actor!=null&&actor.Transforming){if(actor.Frame<lastTearFrame)backwards++;lastTearFrame=actor.Frame;tearFrames.Add(actor.Frame);if(!actor.BodyVisible)blankFrames++;}
        if(times==null||Time.unscaledTime-recordingStart<nextCapture)return;
        camera.Render();RenderTexture.active=capture;pixels.ReadPixels(new Rect(0,0,1200,720),0,0);pixels.Apply();RenderTexture.active=null;
        File.WriteAllBytes(Recording+"/frame_"+times.Count.ToString("D5")+".png",pixels.EncodeToPNG());times.Add(Time.unscaledTime-recordingStart);nextCapture=Time.unscaledTime-recordingStart+.0667f;
    }
    private static void StopRecord()
    {
        if(times==null)return;var list=new System.Text.StringBuilder("ffconcat version 1.0\n");
        for(int i=0;i<times.Count;i++){list.Append("file 'frame_"+i.ToString("D5")+".png'\n");list.Append("duration "+(i+1<times.Count?times[i+1]-times[i]:.0667f).ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+"\n");}
        list.Append("file 'frame_"+(times.Count-1).ToString("D5")+".png'\n");File.WriteAllText(Recording+"/frames.ffconcat",list.ToString());audio.Save(Recording+"/audio.wav");times=null;camera.targetTexture=null;capture.Release();Object.Destroy(pixels);Object.Destroy(capture);
    }
    private static IEnumerator ObserveMotion(float duration)
    {
        var seen=new HashSet<Sprite>();float until=Time.time+duration;
        while(Time.time<until){seen.Add(actor.CurrentSprite);yield return null;}
        Check(seen.Count>=16,"Intermediários novos aparecem no ciclo completo de "+(actor.HalfShell?"cipó":"voo"));
    }
    private static IEnumerator Snapshot(string name){yield return Frames(2);RecordFrame();File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());}
    private static IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    private static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
    private static IEnumerator WallWait(float seconds){double end=EditorApplication.timeSinceStartup+seconds;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static IEnumerator Until(Func<bool> predicate,string label){double end=EditorApplication.timeSinceStartup+25;while(!predicate()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    private static void Check(bool value,string message){if(!value)throw new InvalidOperationException("JETPACK_CHECK_FAILED "+message);checks++;Debug.Log("JETPACK_CHECK "+message);}
    private static void OnLog(string text,string trace,LogType type){if(trace.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try{
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Jetpack scenario deadline");RecordFrame();
            while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}
            Debug.Log("MACAQUINI_JETPACK_PLAY_OK checks="+checks+" errors="+errors);
        }catch(Exception error){errors++;Debug.LogException(error);}
        StopRecord();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+"}");Time.timeScale=1;EditorApplication.isPlaying=false;
    }
}
