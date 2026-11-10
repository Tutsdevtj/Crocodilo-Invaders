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
public static class LakeTirePlayValidation
{
    public static void Run() => Launch(typeof(LakeTirePlayValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakeTirePlayValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v25/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"entrada do boss");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();
        var tire=combat.Tire;var rb=player.GetComponent<Rigidbody2D>();var area=combat.Arena;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);
        rb.position=new Vector2(area.xMin+3,2);
        Check(tire!=null && tire.ArtReady,"Asset de pneu carregado no boss real");Check(boss.Health.MaxHealth==320,"Vida do boss em 320");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Prepare,"salto anterior ao pneu");rb.position=new Vector2(area.xMax-2,2);
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Tire,"pneu no ciclo natural");
        Check(combat.Landings>0 && combat.Tongue.Strikes>0,"Pneu aparece naturalmente depois de língua e salto");
        Check(tire.Preparing && tire.Held.enabled && tire.Projectile==null,"Pneu visível na preparação sem dano antecipado");
        yield return Until(()=>tire.PhaseTime>.8f,"trava da mira");Vector2 locked=tire.LockedAim;
        Check(Vector2.Distance(locked,rb.position)<.05f,"Mira do pneu acompanha a nave ate perto do arremesso");
        rb.position=new Vector2(area.xMin+2,2);yield return Wait(.1f);
        Check(tire.LockedAim==locked,"Mira do pneu trava antes do jogador desviar");Capture("tire-windup.png");
        float prepTime=tire.PhaseTime;Vector3 heldPosition=tire.Held.transform.position;Time.timeScale=0;
        double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(tire.PhaseTime==prepTime && tire.Held.transform.position==heldPosition,"Pausa congela pneu em preparação");Time.timeScale=1;
        yield return Until(()=>tire.Projectile!=null,"arremesso natural");var first=tire.Projectile;
        Check(!tire.Held.enabled && tire.Throws==1,"Um pneu lançado e objeto da mão ocultado");
        Check(Vector2.Distance(TireAt(tire.LaunchPoint,first),locked)<.05f,"Arco passa pela posição avisada, sem perseguição");
        Check(first.AimTime<=LakeTireProjectile.MaxAimTime,"Tempo de voo limitado");
        yield return Wait(.15f);Quaternion rotation=first.transform.rotation;yield return Wait(.12f);
        Check(first!=null && Quaternion.Angle(rotation,first.transform.rotation)>30,"Pneu gira durante voo");Capture("tire-flying.png");
        Vector2 frozen=first.GetComponent<Rigidbody2D>().position;float age=first.Clock;Time.timeScale=0;
        pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(first.Clock==age && first.GetComponent<Rigidbody2D>().position==frozen,"Pausa congela deslocamento e giro");Time.timeScale=1;
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"fim da primeira passagem");yield return Until(()=>tire.Projectile==null,"pneu sai de cena");
        Check(tire.Projectile==null && tire.DamageAttempts==0,"Desvio evita contato e pneu sai da arena");
        rb.position=new Vector2(area.center.x-2,combat.WaterY+1);Check(combat.TryBeginTire(),"Arremesso baixo para testar quique");yield return Until(()=>tire.Projectile!=null,"pneu baixo");var skipping=tire.Projectile;
        yield return Until(()=>skipping==null || skipping.Bounces>0,"pneu toca a agua");Check(skipping!=null && skipping.Bounces==1,"Pneu ricocheteia na agua");
        yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float && tire.Projectile==null,"fim dos quiques");
        rb.position=new Vector2(area.xMax-2,2);int hp=player.Health.CurrentHealth,attempts=tire.DamageAttempts;
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),false);Check(combat.TryBeginTire(),"Segundo arremesso disponível no intervalo");
        yield return Until(()=>tire.DamageAttempts>attempts,"contato real do pneu");
        Check(player.Health.CurrentHealth==hp-2 && tire.Hits==1,"Pneu causa exatamente dois HP no contato");
        Check(tire.Projectile!=null && tire.Projectile.ContactSpent,"Pneu mantém impulso após consumir contato");yield return Wait(.25f);
        Check(player.Health.CurrentHealth==hp-2,"Mesma passagem não repete dano");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"intervalo para dash");
        rb.position=new Vector2(area.xMax-2,2);hp=player.Health.CurrentHealth;attempts=tire.DamageAttempts;int hits=tire.Hits;
        Check(combat.TryBeginTire(),"Arremesso para teste de esquiva");yield return Until(()=>tire.Projectile!=null,"pneu para dash");
        yield return Until(()=>Vector2.Distance(tire.Projectile.GetComponent<Rigidbody2D>().position,rb.position)<1.4f,"aproximação do pneu");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),false);player.Movement.SetMoveInput(Vector2.left);Check(player.GetComponent<PlayerDash>().TryDash(),"Dash real durante aproximação");player.Movement.SetMoveInput(Vector2.zero);
        yield return Until(()=>tire.DamageAttempts>attempts,"contato esquivado");
        Check(player.Health.CurrentHealth==hp && tire.Hits==hits,"Dash atravessa pneu sem dano");yield return Wait(.3f);
        Check(player.Health.CurrentHealth==hp,"Contato esquivado não causa dano atrasado");
        player.Health.SetInvulnerable(typeof(LakeTirePlayValidation),true);yield return Until(()=>combat.CurrentState==SapoDoPneuCombat.State.Float,"intervalo para fúria");
        Check(combat.TryBeginTire(),"Preparação antes de cruzar metade da vida");yield return Wait(.15f);int throws=tire.Throws;
        boss.Health.TakeDamage(boss.Health.MaxHealth/2+1);yield return Wait(.1f);
        Check(combat.CurrentState==SapoDoPneuCombat.State.Rage && !tire.Busy && !tire.Held.enabled,"Fúria interrompe pneu em preparação");
        Check(tire.Throws==throws && tire.Projectile==null,"Interrupção não gera arremesso atrasado");
        yield return Until(()=>combat.TryBeginTire(),"pneu na fúria");yield return Until(()=>tire.Projectile!=null,"arremesso da fúria");
        Check(tire.Projectile.AimTime<=LakeTireProjectile.MaxAimTime,"Fúria usa o mesmo arco de pneu");
        Check(!combat.Tongue.Busy,"Pneu não se sobrepõe à língua");Capture("enraged-tire.png");
        throws=tire.Throws;combat.StopCombat();yield return Frames(4);yield return Wait(.9f);
        Check(!tire.Busy && tire.Projectile==null && (tire.Held==null || !tire.Held.enabled),"Encerrar combate limpa pneu e preparação");
        Check(Object.FindObjectsByType<LakeTireProjectile>().Length==0 && tire.Throws==throws,"Sem pneu órfão ou lançamento tardio");
    }
    private static IEnumerator EnterLake()
    {
        var menu=Object.FindAnyObjectByType<DeveloperMenuView>();yield return Until(()=>{menu.Open();return menu.IsVisible;},"menu DEV");
        GameObject.Find("FinalBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();GameObject.Find("LakeBoss").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return Until(()=>Object.FindAnyObjectByType<GameplayLoadingController>()?.State==GameplayLoadingController.LoadingState.Ready,"loading");
        Object.FindAnyObjectByType<GameplayLoadingController>().StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<LakeBossEntrance>()?.CurrentStage==LakeBossEntrance.Stage.Round,"onda dez da entrada");
    }
    private static Vector2 TireAt(Vector2 start,LakeTireProjectile wheel) => start+wheel.LaunchVelocity*wheel.AimTime+Vector2.down*(.5f*LakeTireProjectile.Gravity*wheel.AimTime*wheel.AimTime);
}
