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
public static class MacaquiniFlowV30Validation
{
    private const string Key="Crocodilo.Flow30.Validation",Output="Docs/ArtPreviews/MacaquiniFlow_v30";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors,rounds,overlaps;private static double deadline;
    private static Boss boss;private static MacaquiniCombat combat;private static MacaquiniFighter fighter;private static Player player;
    private static float maxRelocationSpeed,cueTravel,minDangerGap=100,minRecovery=100;
    private static readonly HashSet<int> combos=new HashSet<int>(),sides=new HashSet<int>();
    static MacaquiniFlowV30Validation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);bool build=SessionState.GetBool(Key+".Build",false);SessionState.SetBool(Key+".Build",false);if(build&&errors==0)EditorApplication.delayCall+=MacaquiniFlowV30Tools.Build;else EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run(){EditorSceneManager.OpenScene(MacaquiniFighterV22Tools.Test);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    public static void RunAndBuild(){SessionState.SetBool(Key+".Build",true);Run();}
    private static void Begin(){checks=errors=rounds=overlaps=0;maxRelocationSpeed=cueTravel=0;minDangerGap=minRecovery=100;combos.Clear();sides.Clear();deadline=EditorApplication.timeSinceStartup+240;Application.runInBackground=true;Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;}
    private static IEnumerator Scenario()
    {
        yield return Frames(4);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/JungleStage/Flow30Save");
        player=Object.FindAnyObjectByType<Player>();player.Health.SetInvulnerable(typeof(MacaquiniFlowV30Validation),true);player.GetComponent<PlayerInputReader>().enabled=false;
        var waves=Object.FindAnyObjectByType<WaveController>();yield return Until(()=>waves.ActiveBoss!=null&&waves.ActiveBoss.GetComponent<MacaquiniCombat>().PhaseTwo&&waves.ActiveBoss.Ready,"second form ready");
        boss=waves.ActiveBoss;combat=boss.GetComponent<MacaquiniCombat>();fighter=boss.GetComponent<MacaquiniFighter>();combat.StopCombat();boss.Movement.HoldPosition(true);yield return Frames(3);
        Check(fighter!=null&&!fighter.Enraged&&boss.Health.CurrentHealth==180,"Segunda forma mantém vida e começa no ritmo normal");
        StartRound(0);yield return Until(()=>fighter.CurrentState==MacaquiniFighter.State.Cue,"jab cue");
        Check(fighter.CueDuration>=.60f&&!fighter.Dangerous,"Primeiro golpe conserva aviso legível antes de causar dano");
        var frozen=boss.transform.position;var aim=fighter.LockedAim;Time.timeScale=0;yield return WallWait(.2f);
        Check(boss.transform.position==frozen&&fighter.LockedAim==aim,"Pausa congela preparação e mira");Time.timeScale=1;
        int hp=player.Health.CurrentHealth;player.Health.SetInvulnerable(typeof(MacaquiniFlowV30Validation),false);
        yield return Until(()=>player.Health.CurrentHealth<hp,"physical jab hit");yield return Wait(.12f);
        Check(player.Health.CurrentHealth==hp-1,"Soco continua com dano único e contato real");
        fighter.Cancel();yield return Frames(3);player.Health.SetInvulnerable(typeof(MacaquiniFlowV30Validation),true);
        boss.GetComponent<Rigidbody2D>().position=new Vector2(4,1);player.GetComponent<Rigidbody2D>().position=Vector2.zero;StartRound(0);
        yield return Until(()=>fighter.CurrentState==MacaquiniFighter.State.Cue,"dash escape cue");aim=fighter.LockedAim;hp=player.Health.CurrentHealth;
        player.Health.SetInvulnerable(typeof(MacaquiniFlowV30Validation),false);player.Movement.SetMoveInput(Vector2.up);
        Check(player.TryDash(),"Dash real disponível para a esquiva");int strikes=fighter.StrikesStarted;
        yield return Until(()=>fighter.StrikesStarted>strikes,"evaded strike");yield return Wait(.34f);player.Movement.SetMoveInput(Vector2.zero);
        Check(player.Health.CurrentHealth==hp&&fighter.LockedAim==aim,"Golpe não persegue a nave após o aviso e pode ser esquivado");
        fighter.Cancel();yield return Frames(3);player.Health.SetInvulnerable(typeof(MacaquiniFlowV30Validation),true);player.GetComponent<Rigidbody2D>().position=Vector2.zero;
        yield return ObserveRound();yield return ObserveRound();
        Check(combos.Count>=2,"Combinações diferentes aparecem no ritmo normal");
        Check(cueTravel>.08f,"Boss prepara golpes com movimento de peso em vez de ficar estático");
        Check(maxRelocationSpeed<32,"Aproximações e recuos não dão saltos de alta velocidade");
        Check(sides.Count==2,"Cruzados alternam o lado de aproximação");
        boss.Health.TakeDamage(100);Check(fighter.Enraged,"Menos de 45% de vida ativa pressão extra");
        int interleaved=fighter.InterleavedStrikes;bool four=false;
        for(int i=0;i<9&&!four;i++){yield return ObserveRound();four=fighter.ComboLength==4;}
        Check(four,"Fase ferida executa sequência real de quatro golpes");
        Check(fighter.InterleavedStrikes>interleaved,"Boss intercala seus golpes nas rodadas dos ajudantes");
        Check(overlaps==0,"Boss e ajudantes alternam janelas de dano sem sobreposição");
        Check(minDangerGap>=1.10f,"Sequências preservam tempo para recarregar a esquiva");
        Check(minRecovery<=.41f,"Rodadas feridas têm recuperação mais curta");
        Check(fighter.ControlsDrawing&&!fighter.Running,"Guarda continua visível entre rodadas sem piscar para outra pose");
        fighter.Cancel();yield return Frames(2);Check(!fighter.Running&&!fighter.ControlsDrawing&&Object.FindAnyObjectByType<MacaquiniFighterMinion>()==null,"Cancelar limpa poses, efeitos e ajudantes");
        // Return to the real combat scheduler after the directed observations.
        combat.SetTarget(player.Health);int before=fighter.StrikesStarted;yield return Until(()=>fighter.StrikesStarted>before,"natural scheduler strike");
        Check(combat.PerformingAttack&&fighter.Running,"Fluxo natural do boss executa os combos ajustados");
        boss.StopBattle();yield return Frames(3);
        Check(!fighter.Running&&!fighter.ControlsDrawing&&Object.FindAnyObjectByType<MacaquiniFighterMinion>()==null,"Fim da batalha interrompe o fluxo sem ameaças órfãs");
    }
    private static void StartRound(int forced=-1)
    {
        fighter.StartCoroutine(fighter.Round(player.Health,AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"),null,forced));
    }
    private static IEnumerator ObserveRound()
    {
        StartRound();yield return Frames(2);var body=boss.GetComponent<Rigidbody2D>();Vector2 previous=body.position;float priorTime=Time.fixedTime;var priorState=fighter.CurrentState;int priorStrike=fighter.StrikesStarted;
        while(fighter.Running)
        {
            Vector2 position=body.position;float dt=Time.fixedTime-priorTime;
            bool relocating=fighter.CurrentState==MacaquiniFighter.State.Approach||fighter.CurrentState==MacaquiniFighter.State.Recovery;
            if(relocating&&priorState==fighter.CurrentState&&dt>.001f)maxRelocationSpeed=Mathf.Max(maxRelocationSpeed,Vector2.Distance(previous,position)/dt);
            if(fighter.CurrentState==MacaquiniFighter.State.Cue&&priorState==fighter.CurrentState)cueTravel+=Vector2.Distance(previous,position);
            if(fighter.CurrentState==MacaquiniFighter.State.Cue)sides.Add(position.x>=player.transform.position.x?1:-1);
            if(fighter.StrikesStarted!=priorStrike){minDangerGap=Mathf.Min(minDangerGap,fighter.LastDangerGap);priorStrike=fighter.StrikesStarted;}
            if(fighter.ComboLength>0)combos.Add(fighter.ComboId);
            if(fighter.RecoveryDuration>0)minRecovery=Mathf.Min(minRecovery,fighter.RecoveryDuration);
            var helpers=Object.FindObjectsByType<MacaquiniFighterMinion>();
            if(helpers.Count(h=>h.Dangerous)>1||fighter.Dangerous&&helpers.Any(h=>h.Dangerous))overlaps++;
            previous=position;priorTime=Time.fixedTime;priorState=fighter.CurrentState;yield return null;
        }
        rounds++;
    }
    private static IEnumerator Frames(int n){while(n-->0)yield return null;}
    private static IEnumerator Wait(float n){float end=Time.time+n;while(Time.time<end)yield return null;}
    private static IEnumerator WallWait(float n){double end=EditorApplication.timeSinceStartup+n;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static IEnumerator Until(Func<bool> p,string label){double end=EditorApplication.timeSinceStartup+30;while(!p()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    private static void Check(bool value,string label){if(!value)throw new InvalidOperationException("FLOW30_FAIL "+label);checks++;Debug.Log("FLOW30_CHECK "+label);}
    private static void OnLog(string m,string t,LogType type){if(t.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick(){EditorApplication.QueuePlayerLoopUpdate();try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Flow30 deadline");while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}Debug.Log("MACAQUINI_FLOW30_PLAY_OK checks="+checks+" errors="+errors);}catch(Exception e){errors++;Debug.LogException(e);}EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;Time.timeScale=1;File.WriteAllText(Output+"/validation.json",JsonUtility.ToJson(new Metrics{checks=checks,errors=errors,rounds=rounds,maxRelocationSpeed=maxRelocationSpeed,cueTravel=cueTravel,minDangerGap=minDangerGap,minRecovery=minRecovery,overlaps=overlaps,combos=combos.Count},true));EditorApplication.isPlaying=false;}
    [Serializable] private class Metrics {public int checks,errors,rounds,overlaps,combos;public float maxRelocationSpeed,cueTravel,minDangerGap,minRecovery;}
}
