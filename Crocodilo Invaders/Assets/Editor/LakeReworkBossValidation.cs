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
public static class LakeReworkBossValidation
{
    public static void Run() => Launch(typeof(LakeReworkBossValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakeReworkBossValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v26/Boss/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"boss pronto");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var rb=player.GetComponent<Rigidbody2D>();var water=Object.FindAnyObjectByType<LakeWildlife>();var art=UnityEditor.AssetDatabase.LoadAssetAtPath<LakeReworkArt>("Assets/Art/LakeRuntime/LakeReworkV26/art.asset");
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),true);
        rb.position=new Vector2(combat.Arena.xMin+3,2);
        Check(art!=null && art.angryBoss!=null && art.surfaceEyes!=null,"Novos assets e olhos carregados");
        Check(water.ActivePiranhas==10,"Piranhas já residem no lago antes de aproximar");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"primeiro salto");
        Check(combat.LockedLanding.x>=combat.LeftLimit,"Salto mirando jogador da esquerda limitado ao meio");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tire,"primeiro pneu");
        yield return Wait(.72f);Check(combat.transform.position.x>=combat.LeftLimit-.01f,"Boss permanece no meio ou na direita");
        var tire=combat.Tire;Check(tire.Held.enabled && tire.Held.transform.position.y>combat.WaterY,"Pneu emerge na mão antes de lançar");Capture("tire-in-hand.png");
        float heldWidth=tire.Held.sprite.bounds.size.x*tire.Held.transform.lossyScale.x;Vector2 aim=tire.LockedAim;
        yield return Until(()=>tire.Projectile!=null,"pneu sai da mão");
        var wheel=tire.Projectile;Check(Vector2.Distance(wheel.transform.position,tire.LaunchPoint)<.5f,"Primeiro quadro do pneu nasce na mão, longe do centro padrão");
        Check(Mathf.Abs(wheel.GetComponent<SpriteRenderer>().sprite.bounds.size.x*wheel.transform.lossyScale.x-heldWidth)<.35f,"Tamanho do pneu contínuo entre mão e voo");
        Check(wheel.Bounces==0 && wheel.AimTime<=LakeTireProjectile.MaxAimTime,"Trajetória em arco a partir da mira travada");
        yield return Wait(.12f);Capture("tire-release.png");float age=wheel.Clock;Vector2 fixedFish=water.Piranha(0).Position;
        Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(wheel.Clock==age && water.Piranha(0).Position==fixedFish,"Pausa congela pneu e fauna");Time.timeScale=1;
        boss.Health.TakeDamage(boss.Health.MaxHealth/2+1);yield return Wait(.3f);
        Check(combat.CurrentState==SapoDoPneuCombat.State.Rage && combat.Companion.Entries==0,"Raiva precede salto da Saperereca");
        Check(Object.FindAnyObjectByType<LakeBossBar>().CompanionInHud,"Sapa ainda na HUD durante aviso de raiva");
        Check(combat.GetComponentInChildren<SpriteRenderer>().sprite==art.angryBoss,"Expressão furiosa real do Sapeneu");Capture("rage-warning.png");
        yield return Until(()=>combat.Companion.CurrentState==SapererecaCompanion.State.Entering,"salto após aviso");yield return Wait(.35f);Capture("wife-joins.png");
        yield return Until(()=>combat.Companion.CurrentState==SapererecaCompanion.State.Warning && combat.Companion.EyesVisible,"olhos indicam origem da sapa");
        var wife=combat.Companion;Check(!wife.Visual.enabled && Mathf.Abs(wife.EyesPosition.x-wife.Landing.x)<.05f,"Olhos na superfície apontam ponto exato de emergência");
        Check(!wife.GetComponentsInChildren<Transform>().Any(t=>t.name=="LakeLandingWarning"),"Saperereca sem marcação circular azul");Capture("eyes-warning.png");
        yield return Until(()=>wife.Shots>0 && wife.ActiveDrops>0,"cuspida compacta da sapa");
        var shot=Object.FindObjectsByType<LakeDroplet>().First(d=>d.Owner==(ILakeDropOwner)wife);
        Check(shot.Drawing==UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LakeRuntime/LakeJourneyV22/water-drop.png"),"Sapa usa mesma gota dos sapinhos");
        Check(shot.GetComponent<SpriteRenderer>().bounds.size.y<.65f,"Tiro compacto, sem gota gigante");
        Check(Vector2.Distance(LakeDroplet.Point(wife.LastLaunch,wife.LastVelocity,wife.LastTravel),wife.LockedAim)<.7f,"Arco cruza mira travada com espalhamento pequeno");
        Check(Vector2.Distance(wife.LastLaunch,wife.Position)<1.3f,"Cuspida parte da boca da Saperereca");yield return Wait(.2f);Capture("wife-spits.png");
        combat.StopCombat();yield return Frames(4);Check(!tire.Busy && wife.ActiveDrops==0 && !wife.EyesVisible,"Fim do boss limpa pneu, olhos e gotas");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),false);
        rb.position=new Vector2(combat.Arena.xMin+3,combat.WaterY-.8f);int hp=player.Health.CurrentHealth,bites=water.Bites;
        yield return Until(()=>water.Bites>bites,"piranhas nadam e mordem jogador submerso");
        Check(water.WaterDangerActive && player.Health.CurrentHealth==hp-1,"Mordida real na água causa um HP");
        Check(GameObject.Find("PiranhaSurfaceWarning")==null,"Perseguição sem marca circular");Capture("piranhas-hunt.png");
        float at=Time.time;var previous=Enumerable.Range(0,10).Select(i=>water.Piranha(i).Position).ToArray();float maxStep=0;
        while(Time.time-at<.5f)
        {
            float before=Time.fixedTime;yield return Frames(1);
            if(Time.fixedTime==before)continue;
            for(int i=0;i<10;i++){maxStep=Mathf.Max(maxStep,Vector2.Distance(previous[i],water.Piranha(i).Position));previous[i]=water.Piranha(i).Position;}
        }
        Check(maxStep<.65f,"Piranhas se deslocam continuamente, sem reposição perto do jogador");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),true);
        rb.position=new Vector2(rb.position.x,combat.WaterY+.35f);int leaps=water.Launches;
        yield return Until(()=>water.Launches>leaps,"bote ao aproximar da superfície");Capture("piranhas-leap.png");
        rb.position=new Vector2(rb.position.x,2);yield return Wait(1.4f);
        Check(!water.WaterDangerActive && Enumerable.Range(0,10).All(i=>water.Piranha(i).Active),"Subir encerra perseguição sem sumir com peixes");
        int ambient=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>ambient,"salto ambiente raro");int after=water.AmbientJumps;yield return Wait(3);
        Check(water.AmbientJumps==after,"Saltos ambientes espaçados, sem sequência frequente");Capture("ambient-piranhas.png");
        player.Health.SetInvulnerable(typeof(LakeReworkBossValidation),false);player.Health.TakeDamage(999);yield return Frames(5);
        Check(water.ActivePiranhas==0 && waves.Phase==WaveController.RunPhase.Failed,"Derrota desativa dano e fauna agressiva");
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
