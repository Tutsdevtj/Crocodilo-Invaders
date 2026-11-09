using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using static LakePlayHarness;

[LakePlaySuite(300)]
public static class LakeDifficultyPlayValidation
{
    public static void Run() => Launch(typeof(LakeDifficultyPlayValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakeDifficultyPlayValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v21/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"entrada do boss");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var rb=player.GetComponent<Rigidbody2D>();var area=combat.Arena;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);
        rb.position=new Vector2(area.xMin+3,area.yMax-1.1f);player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);
        Check(boss.Health.MaxHealth==320,"Vida do boss em 320");
        var wavePools=new SerializedObject(waves).FindProperty("wavePools");
        Check(WaveTotal(wavePools,0)==10 && WaveTotal(wavePools,8)==30,"Hordas vao de 10 a 30 inimigos");
        Check(Object.FindAnyObjectByType<LakeJourneyDirector>().MaxStrikes==2,"Trecho final permite dois ataques simultaneos");
        Check(Object.FindAnyObjectByType<LakeLilyPads>().Count==6,"Seis folhas pequenas, sem os dois apoios grandes");
        Check(UnityEditor.AssetDatabase.GetAssetPath(combat.Tuning).EndsWith("LakeDifficulty.asset"),"Balanceamento vem do asset unico");
        Check(Resources.Load<Sprite>("LakeHudV05/hud-frame")==null && combat.GetComponentInChildren<LakeFrogExpressions>()!=null,"Arte do lago saiu de Resources");
        Check(Mathf.Approximately(combat.Tuning.burpDuration,1.4f),"Arroto reduzido a 1,4 segundo");
        yield return Until(()=>combat.Tongue.CurrentPhase==LakeTongueAttack.Phase.Locked,"mosca travada");
        Check(DistanceToSegment(rb.position,combat.Tongue.Mouth,combat.Tongue.Aim)<combat.Tuning.lineRadius,"Linha da lingua passa pela nave");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.BurpWave,"leque do arroto");
        int beforeRing=combat.DropsLaunched;Check(!combat.IsVulnerable,"Boss nao fica exposto antes de soltar o leque");
        yield return Until(()=>combat.IsVulnerable,"janela do arroto");
        Check(combat.DropsLaunched-beforeRing==7,"Arroto solta sete gotas antes de expor");
        float burpStart=Time.time;
        Check(boss.Health.IncomingDamageMultiplier==2,"Arroto continua dobrando dano");Capture("burp-opening.png");
        yield return Until(()=>!combat.IsVulnerable,"fim do arroto");
        Check(Time.time-burpStart<1.48f,"Janela real do arroto menor que 1,48s");
        Check(boss.Health.IncomingDamageMultiplier==1,"Multiplicador restaurado ao terminar arroto");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"salto de perseguição");
        Vector2 locked=combat.LockedLanding;
        Check(Mathf.Abs(locked.x-Mathf.Clamp(rb.position.x,combat.LeftLimit,area.xMax-1.7f))<.15f,"Destino do salto usa a posição do jogador");Capture("pursuit-warning.png");
        rb.position=new Vector2(area.xMax-3,area.yMax-1.1f);yield return Wait(.2f);
        Check(combat.LockedLanding.x>locked.x+.3f,"Salto acompanha a nave durante o aviso");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Fall,"queda do salto");yield return Wait(.12f);
        locked=combat.LockedLanding;rb.position=new Vector2(area.xMin+3,area.yMax-1.1f);yield return Wait(.15f);
        Check(combat.LockedLanding==locked,"Pouso fica travado durante a queda");
        int low=combat.LowWaves,high=combat.HighWaves,drops=combat.DropsLaunched;
        yield return Until(()=>combat.LowWaves>low,"onda baixa");
        Check(combat.HighWaves==high && combat.DropsLaunched-drops==3,"Primeira onda tem três gotas rápidas e baixas");
        var lows=Object.FindObjectsByType<LakeDroplet>();
        Check(lows.Any(d=>Mathf.Abs(d.Velocity.x)>=4.7f && d.Velocity.y<5.7f),"Velocidade horizontal caracteriza onda baixa");
        Capture("low-wave.png");yield return Until(()=>combat.HighWaves>high,"onda alta atrasada");
        Check(combat.DropsLaunched-drops==6,"Segunda onda inclui gota mirada na nave");
        Check(Object.FindObjectsByType<LakeDroplet>().Any(d=>d.Velocity.y>8),"Gotas altas sobem mais e cruzam devagar");Capture("high-wave.png");
        yield return Until(()=>combat.Tongue.CampingFlies>0,"mosca contra camping");
        Check(Enumerable.Range(0,combat.Tongue.FlyCount).Any(i=>combat.Tongue.Fly(i).Present && combat.Tongue.Fly(i).Anchor.y>area.center.y),"Mosca pressiona corredor alto");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float && !combat.HasWaterDrops,"intervalo para fúria");
        boss.Health.TakeDamage(boss.Health.MaxHealth/2);yield return Until(()=>combat.Companion.Joined,"entrada da Saperereca");
        var wife=combat.Companion;Check(combat.Enraged && wife.Entries==1,"Fúria acontece uma única vez na metade da vida");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Dive,"mergulho");Capture("saperereca-dive.png");
        Vector2 before=wife.Position;yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Swim,"nado submerso");
        Check(!wife.Visual.enabled && wife.Position.y<combat.WaterY-.8f,"Sapa desaparece sob a superfície");
        yield return Until(()=>wife.EyesVisible,"olhos na superficie");Check(wife.EyesVisible,"Olhos avisam o ponto de emergência");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Warning,"aviso da rajada");
        Vector2 shotAim=wife.LockedAim;rb.position=new Vector2(area.xMin+3,area.yMax-1.1f);
        yield return Wait(.15f);Check(wife.LockedAim==shotAim,"Rajada trava mira durante o aviso");Capture("locked-volley-warning.png");
        float pauseClock=wife.Clock;Vector2 pausePosition=wife.Position;Time.timeScale=0;
        double until=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<until)yield return null;
        Check(wife.Clock==pauseClock && wife.Position==pausePosition,"Pausa congela mergulho e rajada");Time.timeScale=1;
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Spit && wife.Shots%3==2,"tiro central");
        Vector2 intercept=wife.LastLaunch+wife.LastVelocity*wife.LastTravel+Vector2.down*(.5f*LakeDroplet.Gravity*wife.LastTravel*wife.LastTravel);
        Check(Vector2.Distance(intercept,wife.LockedAim)<.05f && Vector2.Distance(wife.LockedAim,rb.position)<.2f,"Mira da rajada recente, na posicao atual da nave");Capture("aimed-volley.png");
        yield return Until(()=>wife.Shots>=3 && wife.CurrentState==SapererecaCompanion.State.Rest,"fim da rajada");
        Check(wife.Shots%3==0,"Rajada contém três tiros espaçados");
        yield return Until(()=>wife.Combos>0 && combat.Tongue.Busy,"combinação com língua");
        int comboShots=wife.Shots;Check(wife.CurrentState==SapererecaCompanion.State.Dive,"Mergulho começa enquanto língua prepara ataque");
        yield return Until(()=>wife.CurrentState==SapererecaCompanion.State.Ready,"sapa aguarda língua recolher");
        Check(wife.Shots==comboShots,"Preparação conjunta não dispara durante língua");Capture("rage-combo.png");
        yield return Until(()=>wife.Shots>comboShots,"rajada após língua");
        Check(!combat.Tongue.Busy,"Rajada só sai depois da língua recolhida");
        int hp=player.Health.CurrentHealth;
        Check(!wife.TryHit() && player.Health.CurrentHealth==hp,"Dano da sapa respeita proteção do jogador");
        yield return Wait(.7f);
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),false);
        var dash=player.GetComponent<PlayerDash>();Check(dash.TryDash(),"Dash real disponível durante rajada");
        var contact=LakeDroplet.Fire(wife,combat.DropletSprite,rb.position,Vector2.right*.1f,LakeDropStyle.Helper);
        yield return Until(()=>!contact.Live,"gota consumida na esquiva");
        Check(player.Health.CurrentHealth==hp,"Contato real de gota durante dash não causa dano");
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);yield return Wait(.8f);
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),false);
        contact=LakeDroplet.Fire(wife,combat.DropletSprite,rb.position,Vector2.right*.1f,LakeDropStyle.Helper);
        yield return Until(()=>!contact.Live,"gota atinge sem proteção");
        Check(player.Health.CurrentHealth==hp-1,"Contato real sem dash causa exatamente um dano");
        player.Health.SetInvulnerable(typeof(LakeDifficultyPlayValidation),true);
        int rageLow=combat.LowWaves,rageHigh=combat.HighWaves,rageDrops=combat.DropsLaunched;
        while(combat.LowWaves==rageLow){rageDrops=combat.DropsLaunched;yield return null;}
        yield return Until(()=>combat.LowWaves>rageLow,"onda baixa da fúria");
        Check(combat.DropsLaunched-rageDrops==4,"Fúria mantém quatro gotas na onda baixa");
        yield return Until(()=>combat.HighWaves>rageHigh,"onda alta da fúria");
        Check(combat.DropsLaunched-rageDrops==8,"Fúria soma oito gotas nas duas ondas");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"descanso da furia");float restStart=Time.time;
        yield return Until(()=>combat.CurrentState!=SapoDoPneuCombat.State.Float,"ataque seguinte da furia");
        Check(Time.time-restStart<.8f,"Furia reduz o descanso real entre golpes");
        combat.StopCombat();yield return Frames(4);
        Check(wife.CurrentState==SapererecaCompanion.State.Stopped && !wife.Visual.enabled,"Encerramento cancela sapa e visual");
        Check(Object.FindObjectsByType<LakeDroplet>().Length==0,"Encerramento limpa projéteis pendentes");
        Check(LakeFxPool.Instance.LiveDropCount==0 && LakeFxPool.Instance.SpareDropCount>0,"Gotas voltam para o pool");
        Check(Object.FindObjectsByType<LakeDroplet>(FindObjectsInactive.Include).Length<=48,"Gotas reaproveitadas, sem criar objetos a cada tiro");
        Check(Object.FindObjectsByType<LakeWaterFX>(FindObjectsInactive.Include).Length<=32 && GameObject.Find("WaterPixel")==null,"Respingos reaproveitados, sem um objeto por pixel");
        Check(!combat.Tongue.Busy && !combat.HasWaterDrops,"Sem ataques atrasados depois de encerrar");
    }
    private static int WaveTotal(SerializedProperty pools,int wave)
    {
        var groups=pools.GetArrayElementAtIndex(wave).FindPropertyRelative("options").GetArrayElementAtIndex(0).FindPropertyRelative("groups");
        int total=0;for(int i=0;i<groups.arraySize;i++)total+=groups.GetArrayElementAtIndex(i).FindPropertyRelative("count").intValue;
        return total;
    }
    private static float DistanceToSegment(Vector2 point,Vector2 a,Vector2 b)
    {
        Vector2 ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(point-a,ab)/Mathf.Max(.0001f,ab.sqrMagnitude));
        return Vector2.Distance(point,a+ab*t);
    }
    private static IEnumerator EnterLake()
    {
        var menu=Object.FindAnyObjectByType<DeveloperMenuView>();yield return Until(()=>{menu.Open();return menu.IsVisible;},"menu DEV");
        GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"loading");
        Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<LakeBossEntrance>()?.CurrentStage==LakeBossEntrance.Stage.Round,"onda dez da entrada");
    }
}


