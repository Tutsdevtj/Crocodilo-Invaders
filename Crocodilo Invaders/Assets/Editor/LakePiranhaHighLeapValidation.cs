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
public static class LakePiranhaHighLeapValidation
{
    public static void Run() => Launch(typeof(LakePiranhaHighLeapValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakePiranhaHighLeapValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v30/Piranhas/TestSave");yield return EnterLake();
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.ActiveBoss?.Ready==true,"boss pronto");yield return Frames(4);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();combat.StopCombat();
        var rb=player.GetComponent<Rigidbody2D>();var water=Object.FindAnyObjectByType<LakeWildlife>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),true);rb.position=new Vector2(-4,2);
        int count=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>count,"primeiro salto ambiente alto");
        float first=water.Clock;var jumper=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.CurrentState==LakePiranha.State.Leaping && p.Velocity.y>0);
        float peak=jumper.Position.y+jumper.Velocity.y*jumper.Velocity.y/28;
        Check(peak-water.WaterY>=1.6f,"Salto ambiente sobe pelo menos 1,6 unidade acima da �gua");
        yield return Wait(.28f);Check(jumper.Position.y>water.WaterY+.65f,"Piranha atravessa visivelmente a superf�cie");Capture("higher-ambient-jump.png");
        float clock=water.Clock;Vector2 position=jumper.Position;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(water.Clock==clock && jumper.Position==position,"Pausa congela novo arco das piranhas");Time.timeScale=1;
        count=water.AmbientJumps;yield return Until(()=>water.AmbientJumps>count,"pr�ximo salto mais frequente");
        float interval=water.Clock-first;Check(interval>=6.9f && interval<=11.2f,"Saltos ambientes agora espa�ados em sete a onze segundos");
        Check(!water.WaterDangerActive && water.ActivePiranhas==10,"Saltos n�o criam nem escondem residentes");
        yield return Wait(1.8f);
        var nearby=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.Available);
        rb.position=new Vector2(nearby.Position.x,water.WaterY+.35f);player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),false);
        int hp=player.Health.CurrentHealth,bites=water.Bites,launches=water.Launches;
        yield return Until(()=>water.Launches>launches,"bote contra jogador perto da �gua");
        jumper=Enumerable.Range(0,10).Select(i=>water.Piranha(i)).First(p=>p.CurrentState==LakePiranha.State.Leaping && p.Velocity.y>0);
        peak=jumper.Position.y+jumper.Velocity.y*jumper.Velocity.y/28;
        Check(peak-water.WaterY>=1.6f,"Bote ofensivo usa o mesmo salto mais alto");
        yield return Until(()=>water.Bites>bites,"contato real do bote");
        Check(player.Health.CurrentHealth==hp-1,"Piranha continua causando um HP de dano");
        yield return Wait(.25f);Check(player.Health.CurrentHealth==hp-1,"Grupo respeita intervalo de dano");Capture("higher-attack-jump.png");
        player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),true);rb.position=new Vector2(rb.position.x,2);yield return Wait(1.5f);
        Check(!water.WaterDangerActive && water.ActivePiranhas==10,"Subir devolve piranhas ao nado");
        player.Health.SetInvulnerable(typeof(LakePiranhaHighLeapValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
        Check(water.ActivePiranhas==0 && waves.Phase==WaveController.RunPhase.Failed,"Derrota limpa saltos e persegui��o");
        Debug.Log("PIRANHA_HIGH_LEAP interval="+interval+" peakAboveWater="+(peak-water.WaterY));
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
