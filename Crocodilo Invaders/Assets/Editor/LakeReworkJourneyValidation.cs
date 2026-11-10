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
public static class LakeReworkJourneyValidation
{
    public static void Run() => Launch(typeof(LakeReworkJourneyValidation), LakeStageTools.Stage);
    public static void RunFromMenu() => Launch(typeof(LakeReworkJourneyValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"horda ativa");
        LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v26/Journey/TestSave");player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"inimigo controle");
        waves.GetComponent<EnemySpawner>().StopSpawning();foreach(var actor in Object.FindObjectsByType<LakeJourneyEnemy>())actor.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);player.GetComponent<Rigidbody2D>().position=new Vector2(director.Arena.xMin+3,2);
        player.Health.SetInvulnerable(typeof(LakeReworkJourneyValidation),true);var actors=new LakeJourneyEnemy[4];
        for(int i=0;i<4;i++)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/"+((LakeJourneyEnemy.Species)i)+".prefab");
            var node=Object.Instantiate(prefab,new Vector3(director.Arena.xMax+.5f,2,0),Quaternion.identity);
            actors[i]=node.GetComponent<LakeJourneyEnemy>();actors[i].SetTarget(player.Health);actors[i].Join(director,i);
        }
        var rework=UnityEditor.AssetDatabase.LoadAssetAtPath<LakeReworkArt>("Assets/Art/LakeRuntime/LakeReworkV26/art.asset");bool turned=false,bigMoved=false,beetle=false,frog=false,queuedMoved=false;float warnAt=-1,maxWarning=0;
        Vector2 bigPrev=actors[1].transform.position;float lastClock=0;float start=Time.time;
        while(Time.time-start<36)
        {
            if(Time.time-start>17)player.GetComponent<Rigidbody2D>().position=new Vector2(director.Arena.xMax-1,2);
            var big=actors[1];Vector2 now=big.transform.position;
            if(lastClock!=big.Clock)
            {
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Queued && Vector2.Distance(bigPrev,now)>.002f)queuedMoved=true;
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Warning){if(warnAt<0)warnAt=big.Clock;maxWarning=Mathf.Max(maxWarning,big.Clock-warnAt);}
                else warnAt=-1;
                if(big.CurrentMotion==LakeJourneyEnemy.Motion.Strike && Vector2.Distance(bigPrev,now)>.02f)bigMoved=true;
                bigPrev=now;lastClock=big.Clock;
            }
            if(!turned && actors[0].Visual.flipX && UnityEditor.AssetDatabase.LoadAssetAtPath<LakeJourneyArt>("Assets/Art/LakeRuntime/LakeJourneyV22/art.asset").enemies.Take(4).Contains(actors[0].Visual.sprite)){turned=true;Capture("mosquito-return.png");}
            if(!beetle && actors[3].Shots>=2 && actors[3].Vulnerable)
            {beetle=true;Check(Vector2.Distance(actors[3].Feet,actors[3].Lily.Seat)<.04f,"Besouro volta à folha depois de investir e cuspir");Capture("beetle-attacks.png");}
            if(!frog && actors[2].Shots>0){frog=true;Capture("small-frog.png");}
            yield return Frames(1);
        }
        Check(turned,"Mosquito acompanha troca de lado com desenho original espelhado");
        Check(queuedMoved,"Mosquito grande continua voando enquanto espera a vez");
        Check(bigMoved && actors[1].Attacks>=2,"Mosquito grande realiza investidas repetidas");
        Check(maxWarning<.85f,"Aviso do mosquito grande curto, sem piscar parado por vários segundos");
        Check(beetle && actors[3].Shots>=2,"Besouro produz ataque real com duas gotas na investida");
        Check(frog,"Sapinho conserva ataque original");
        Check(director.ActivePreparations<=2,"Fila preserva limite de preparações");
        float t=actors[0].Clock;Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<end)yield return null;
        Check(actors[0].Clock==t,"Pausa congela novas viradas e movimentos");Time.timeScale=1;
        player.Health.SetInvulnerable(typeof(LakeReworkJourneyValidation),false);player.Health.TakeDamage(999);yield return Frames(6);
        Check(director.ActiveDrops==0 && actors.All(x=>x.CurrentMotion==LakeJourneyEnemy.Motion.Stopped),"Derrota limpa gotas e fila de ataques");
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
