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

[LakePlaySuite(650)]
public static class LakeReworkFullRunValidation
{
    public static void Run() => Launch(typeof(LakeReworkFullRunValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakeReworkFullRunValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v26/Full/TestSave");
        var menu=Object.FindAnyObjectByType<MainMenuController>();
        yield return Until(()=>{menu.PlayLake();return Object.FindAnyObjectByType<PlayerNameView>()?.IsVisible==true;},"menu pronto para fase dois");
        var name=Object.FindAnyObjectByType<PlayerNameView>();Check(name!=null && name.IsVisible,"Entrada Fase 2 do menu abre identificação real");name.Close();
        var prefab=(GameplayLoadingController)new SerializedObject(menu).FindProperty("loadingPrefab").objectReferenceValue;
        var loading=Object.Instantiate(prefab);Check(loading.Begin(MainMenuController.LakeStageScene),"Loading da fase completa usa cena de produção");
        yield return Until(()=>loading.State==GameplayLoadingController.LoadingState.Ready,"loading do lago");loading.StartCoroutine("StartGame");
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"primeira onda");
        waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();var scenery=Object.FindAnyObjectByType<LakeJourneyScenery>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
        var wildlife=Object.FindAnyObjectByType<LakeWildlife>();var spawner=waves.GetComponent<EnemySpawner>();var cards=Object.FindAnyObjectByType<CardRewardController>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);var rb=player.GetComponent<Rigidbody2D>();rb.position=new Vector2(-4,1);
        player.Health.SetInvulnerable(typeof(LakeReworkFullRunValidation),true);
        Check(waves.CurrentWave==1 && waves.CombatWaveCount==9 && waves.WaveCount==10,"Fase inicia na onda um e mantém boss na décima");
        Check(!DevelopmentBossSession.IsActive && waves.ActiveBoss==null,"Modo campanha percorre hordas antes do boss");
        Check(scenery.PropCount==12,"Adereços de margem criados sem bloquear nave");
        var art=UnityEditor.AssetDatabase.LoadAssetAtPath<LakeJourneyArt>("Assets/Art/LakeRuntime/LakeJourneyV22/art.asset");Check(art.enemies.Length==16 && art.props.Length==4,"Dezesseis poses e quatro adereços importados");
        int rewards=0;var speciesSeen=new HashSet<LakeJourneyEnemy.Species>();var strikesSeen=new HashSet<LakeJourneyEnemy.Species>();
        bool pauseChecked=false,lockChecked=false,beetleChecked=false,frogChecked=false,piranhaChecked=false;Camera.main.aspect=16f/9f;Time.timeScale=3;
        for(int wave=1;wave<=9;wave++)
        {
            int expected=wave;yield return Until(()=>waves.CurrentWave==expected && waves.Phase==WaveController.RunPhase.Combat,"onda "+wave);
            player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);
            if(wave==7 && scenery.HintVisible)Capture("cururu-in-the-distance.png");
            float started=Time.time;int spawnedBefore=director.Spawned;bool captured=false;
            while(waves.Phase==WaveController.RunPhase.Combat)
            {
                var actors=Object.FindObjectsByType<LakeJourneyEnemy>();
                foreach(var actor in actors)
                {
                    speciesSeen.Add(actor.Kind);if(actor.Attacks>0)strikesSeen.Add(actor.Kind);
                    if(wave==1 && actor.Kind!=LakeJourneyEnemy.Species.Mosquito)throw new InvalidOperationException("Inimigo antecipado na primeira onda");
                    if(wave<4 && (actor.Kind==LakeJourneyEnemy.Species.Frog || actor.Kind==LakeJourneyEnemy.Species.Beetle))throw new InvalidOperationException("Inimigo de margem antecipado");
                    if(wave<7 && actor.Kind==LakeJourneyEnemy.Species.Beetle)throw new InvalidOperationException("Besouro antecipado");
                    if(!lockChecked && actor.CurrentMotion==LakeJourneyEnemy.Motion.Warning)
                    {
                        Vector2 aim=actor.LockedAim;rb.position=new Vector2(-3.8f,2.3f);yield return Wait(.12f);
                        Check(actor!=null && actor.LockedAim==aim,"Inimigo mantém mira avisada após nave sair");lockChecked=true;
                    }
                    if(!beetleChecked && actor.Vulnerable)
                    {
                        var hp=actor.GetComponent<Health>();float before=hp.PreciseCurrentHealth;hp.TakeDamage(1);
                        Check(Mathf.Abs(before-hp.PreciseCurrentHealth-1.8f)<.02f,"Barriga exposta do besouro recebe dano aumentado");beetleChecked=true;Capture("beetle-opening.png");
                    }
                    if(actor.Shots>0 && !frogChecked){Capture("frog-spitting.png");frogChecked=true;}
                }
                if(!pauseChecked && actors.Length>0 && actors[0].Clock>1)
                {
                    float c=actors[0].Clock,routeClock=scenery.Clock;Vector2 position=actors[0].GetComponent<Rigidbody2D>().position;
                    Time.timeScale=0;double end=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<end)yield return null;
                    Check(actors[0].Clock==c && scenery.Clock==routeClock && actors[0].GetComponent<Rigidbody2D>().position==position,"Pausa congela inimigos e travessia");Time.timeScale=3;pauseChecked=true;
                }
                if(!captured && Time.time-started>4 && actors.Length>0)
                {Capture("wave-"+wave+".png");captured=true;Check(director.Section==(wave<=3?0:wave<=6?1:2),"Trecho visual acompanha onda "+wave);}
                if(!piranhaChecked && wave==5 && Time.time-started>3)
                {
                    rb.position=new Vector2(-4,lake.WaterY+.1f);yield return Wait(.2f);Check(wildlife.WaterDangerActive,"Piranhas passam a defender linha da água na quinta onda");
                    rb.position=new Vector2(-4,2);yield return Wait(.15f);Check(!wildlife.WaterDangerActive,"Subir abandona a zona de piranhas");piranhaChecked=true;
                }
                if(wave<=3 && wildlife.CanHurt)throw new InvalidOperationException("Piranhas ativas cedo demais");
                bool observed=Time.time-started>(wave==7?13:9);
                foreach(var actor in actors)if(actor!=null && !actor.GetComponent<Health>().IsDead && observed)
                {
                    bool performed=actor.Kind==LakeJourneyEnemy.Species.Mosquito || (actor.Kind==LakeJourneyEnemy.Species.Frog?actor.Shots>0:actor.Attacks>0);
                    bool opened=actor.Kind!=LakeJourneyEnemy.Species.Beetle || beetleChecked;
                    if(performed && opened)actor.GetComponent<Health>().TakeDamage(999);
                }
                if(director.ActivePreparations>2)throw new InvalidOperationException("Preparações sobrepostas");
                yield return null;
            }
            Check(director.Spawned>spawnedBefore,"Onda "+wave+" efetivamente gerou inimigos novos");
            Check(spawner.RemainingToSpawn==0 && spawner.AliveCount==0,"Onda "+wave+" só termina sem inimigos vivos ou pendentes");
            yield return Until(()=>cards.State==CardRewardController.RewardState.Ready,"cartas após onda "+wave);
            Check(waves.AwaitingReward && Time.timeScale==0 && cards.Offers.Count==3,"Três cartas reais após onda "+wave);
            Check(director.ActiveDrops==0,"Transição de cartas limpa gotas da fase");
            if(wave==1)Capture("lake-cards.png");Check(cards.TryChoose(0),"Escolha de carta aplicada na onda "+wave);rewards++;
            yield return Until(()=>cards.State==CardRewardController.RewardState.Closed,"fechamento das cartas");Time.timeScale=3;
        }
        Check(rewards==9,"Nove recompensas antes do boss");Check(speciesSeen.Count==4,"Quatro espécies atravessaram a campanha");
        Check(strikesSeen.Contains(LakeJourneyEnemy.Species.Dragonfly) && strikesSeen.Contains(LakeJourneyEnemy.Species.Frog) && strikesSeen.Contains(LakeJourneyEnemy.Species.Beetle),"Três ataques especiais observados no percurso");
        Check(lockChecked && beetleChecked && frogChecked && piranhaChecked,"Avisos, barriga vulnerável, cuspidas e perigo da água exercitados");
        yield return Until(()=>waves.CurrentWave==10 && waves.Phase==WaveController.RunPhase.Announcing,"aviso final do boss");Time.timeScale=1;
        Check(director.Calm && scenery.Section==2,"Lago desacelera antes do boss");yield return Wait(.4f);Capture("boss-approach.png");
        yield return Until(()=>waves.ActiveBoss?.Ready==true,"Sapeneu após nona horda");
        Check(waves.ActiveBoss.DisplayName=="SAPENEU CURURU","Percurso termina no boss correto");
        Check(Object.FindAnyObjectByType<LakeBossEntrance>().Entrances==1 && spawner.RemainingToSpawn==0,"Abertura exclusiva ocorre uma vez e sem inimigos pendentes");
        Check(Object.FindObjectsByType<LakeJourneyEnemy>().Length==0 && director.ActiveDrops==0,"Nenhuma ameaça de horda invade a luta do boss");Capture("journey-reaches-boss.png");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();yield return Until(()=>combat.Tongue.Busy,"primeiro ataque do boss");yield return Wait(.8f);
        player.Health.SetInvulnerable(typeof(LakeReworkFullRunValidation),false);player.Health.TakeDamage(999);
        yield return Frames(4);Check(waves.Phase==WaveController.RunPhase.Failed && !combat.Active,"Derrota encerra run e ataques do boss");
        LeaderboardSession.Cancel();Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Teste não conclui capítulo real");
    }
}
