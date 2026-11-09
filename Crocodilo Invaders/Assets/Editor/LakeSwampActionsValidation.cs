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
public static class LakeSwampActionsValidation
{
    public static void Run() => Launch(typeof(LakeSwampActionsValidation), LakeStageTools.Stage);
    public static void RunFromMenu() => Launch(typeof(LakeSwampActionsValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
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
        
        Check(Mathf.Abs(actors[2].Lily.Drawing.transform.position.y-(director.WaterY-.12f))<.02f,"Folha nasce na �gua j� no primeiro quadro, sem aparecer no centro");
        Check(actors[2].Lily.Drawing.enabled && actors[3].Lily.Drawing.enabled && !actors[2].Visual.enabled && !actors[3].Visual.enabled,"Folhas vazias anunciam criaturas antes delas aparecerem");
        int hp=actors[2].GetComponent<Health>().CurrentHealth;
        Check(!actors[2].GetComponent<Health>().TakeDamage(1) && actors[2].GetComponent<Health>().CurrentHealth==hp,"Criatura atr�s da mata n�o recebe dano antecipado");
        yield return Until(()=>actors[2].CurrentMotion==LakeJourneyEnemy.Motion.WaitingLily,"folha chega primeiro");
        Check(!actors[2].Visual.enabled && actors[2].Lily.Arrived,"Folha parada na batalha antes do salto");Capture("empty-leaf-warning.png");
        float waited=actors[2].Clock;yield return Until(()=>actors[2].CurrentMotion==LakeJourneyEnemy.Motion.BankJump,"salto saindo da mata");
        Check(actors[2].Clock-waited>=.32f && actors[2].Visual.sortingOrder<0,"Pausa leg�vel na folha e in�cio atr�s da vegeta��o");
        yield return Wait(.36f);
        Check(actors[2].GetComponent<Rigidbody2D>().position.y>actors[2].Lily.Seat.y+.8f,"Sapo sobe em arco antes de pousar na folha");Capture("bank-jump.png");
        Check(actors[2].Shots==0 && actors[3].Attacks==0,"Chegada n�o ataca o jogador");
        yield return Until(()=>!actors[2].IsEnteringBank && !actors[3].IsEnteringBank,"duas criaturas pousam");
        foreach(int index in new[]{2,3})
        {
            var actor=actors[index];
            Check(actor.Visual.enabled && actor.GetComponent<Collider2D>().enabled && !actor.GetComponent<Health>().IsInvulnerable,actor.Kind+" entra no combate depois do pouso");
            Check(Vector2.Distance(actor.Feet,actor.Lily.Seat)<.04f,actor.Kind+" termina salto apoiado na folha");
        }
        Capture("leaf-landing.png");
        yield return Until(()=>actors[0].CurrentMotion==LakeJourneyEnemy.Motion.Patrol,"pequeno come�a persegui��o");
        Vector2 pos=actors[0].GetComponent<Rigidbody2D>().position;float before=Vector2.Distance(pos,rb.position);yield return Wait(1.3f);
        Check(Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position)<before-.5f,"Mosquito pequeno avan�a em dire��o ao jogador");
        rb.position=new Vector2(director.Arena.xMax-1,2);before=Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position);
        yield return Wait(1.3f);
        Check(Vector2.Distance(actors[0].GetComponent<Rigidbody2D>().position,rb.position)<before-.3f && actors[0].Visual.flipX,"Persegui��o muda suavemente quando o jogador troca de lado");Capture("mosquito-chases.png");
        int defeats=0;actors[0].GetComponent<Enemy>().Defeated+=e=>defeats++;
        actors[0].GetComponent<Health>().TakeDamage(1);yield return Frames(1);
        Check(Object.FindObjectsByType<LakeMosquitoSlap>().Length==0 && actors[0].Visual.enabled,"Acerto n�o fatal n�o esmaga mosquito vivo");
        actors[0].GetComponent<Health>().TakeDamage(999);yield return Frames(1);
        var slap=Object.FindAnyObjectByType<LakeMosquitoSlap>();
        Check(defeats==1 && slap!=null && !actors[0].Visual.enabled,"Morte dispara uma chinelada e uma derrota");
        Check(slap.BloodCount==20 && slap.CrushedSprite.transform.localScale.y<.2f,"Mosquito pequeno achatado com sangue em pixels");
        Check(slap.GetComponentsInChildren<Collider2D>().Length==0 && slap.GetComponentsInChildren<Health>().Length==0,"Efeito n�o duplica inimigo, hitbox ou dano");Capture("small-mosquito-slap.png");
        float frozen=slap.Clock;Vector3 flat=slap.CrushedSprite.transform.position;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(slap.Clock==frozen && slap.CrushedSprite.transform.position==flat,"Pausa congela sangue e corpo esmagado");Time.timeScale=1;
        yield return Wait(1.2f);Check(Object.FindObjectsByType<LakeMosquitoSlap>().Length==0,"Sangue e corpo desaparecem sem res�duos permanentes");
        actors[1].GetComponent<Health>().TakeDamage(999);yield return Frames(1);slap=Object.FindAnyObjectByType<LakeMosquitoSlap>();
        Check(slap!=null && slap.BloodCount==26 && slap.CrushedSprite.transform.localScale.y<.2f,"Inseto grande tamb�m morre esmagado");Capture("large-mosquito-slap.png");
        yield return Until(()=>actors[2].Shots>0 && actors[3].Shots>0,"ataques ap�s chegada");
        Check(actors[2].Attacks>0 && actors[3].Attacks>0,"Sapos e besouros conservam seus ataques depois do salto de entrada");
        player.Health.SetInvulnerable(typeof(LakeSwampActionsValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
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
}
