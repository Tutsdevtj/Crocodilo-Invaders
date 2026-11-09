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
public static class LakeJourneyCombatValidation
{
    public static void Run() => Launch(typeof(LakeJourneyCombatValidation), LakeStageTools.Stage);
    public static void RunFromMenu() => Launch(typeof(LakeJourneyCombatValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Until(()=>Object.FindAnyObjectByType<WaveController>()?.Phase==WaveController.RunPhase.Combat,"primeira horda");
        waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();LeaderboardSession.Cancel();
        var director=Object.FindAnyObjectByType<LakeJourneyDirector>();var scenery=Object.FindAnyObjectByType<LakeJourneyScenery>();
        var spawner=waves.GetComponent<EnemySpawner>();
        yield return Until(()=>Object.FindObjectsByType<LakeJourneyEnemy>().Length>0,"inimigo de controle");
        spawner.StopSpawning();
        foreach(var enemy in Object.FindObjectsByType<LakeJourneyEnemy>())enemy.enabled=false;
        player.GetComponent<PlayerInputReader>().enabled=false;player.Movement.SetMoveInput(Vector2.zero);var rb=player.GetComponent<Rigidbody2D>();
        rb.position=new Vector2(director.Arena.xMin+2,2);int hp=player.Health.CurrentHealth;
        Check(player.GetComponent<PlayerDash>().TryDash(),"Dash real disponível na fase");
        var drop=LakeDroplet.Fire(director,director.DropSprite,rb.position,Vector2.right*.1f,LakeDropStyle.Journey);yield return Until(()=>!drop.Live,"gota consumida durante dash");
        Check(player.Health.CurrentHealth==hp,"Gota de sapinho é consumida sem dano durante dash");
        yield return Wait(.8f);drop=LakeDroplet.Fire(director,director.DropSprite,rb.position,Vector2.right*.1f,LakeDropStyle.Journey);yield return Until(()=>!drop.Live,"contato sem proteção");
        Check(player.Health.CurrentHealth==hp-1,"Gota sem proteção causa exatamente um HP");player.Health.SetInvulnerable(typeof(LakeJourneyCombatValidation),true);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/Beetle.prefab");
        var node=Object.Instantiate(prefab,new Vector3(1,2,0),Quaternion.identity);var beetle=node.GetComponent<LakeJourneyEnemy>();beetle.SetTarget(player.Health);beetle.Join(director,3);node.GetComponent<Health>().SetInvulnerable(beetle,false);
        yield return Wait(.08f);var health=node.GetComponent<Health>();float before=health.PreciseCurrentHealth;health.TakeDamage(4);
        Check(Mathf.Abs(before-health.PreciseCurrentHealth-2.2f)<.02f,"Casco pela frente reduz o dano para 55%");
        rb.position=new Vector2(5,3);yield return Wait(.08f);before=health.PreciseCurrentHealth;health.TakeDamage(1);
        Check(Mathf.Abs(before-health.PreciseCurrentHealth-1)<.02f,"Parte traseira do besouro recebe dano normal");
        yield return Until(()=>beetle.CurrentMotion==LakeJourneyEnemy.Motion.Warning,"aviso do besouro");Vector2 aim=beetle.LockedAim;
        rb.position=new Vector2(-4,2);yield return Wait(.12f);Check(beetle.LockedAim==aim,"Besouro trava mira antes de investir");
        yield return Until(()=>beetle.Vulnerable,"abertura do casco");Check(health.IncomingDamageMultiplier==1.8f,"Casco abre depois da investida");Capture("beetle-vulnerable.png");
        var frogPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/LakeV22/Frog.prefab");
        var frogNode=Object.Instantiate(frogPrefab,new Vector3(5,director.WaterY+.15f,0),Quaternion.identity);var frog=frogNode.GetComponent<LakeJourneyEnemy>();frog.SetTarget(player.Health);frog.Join(director,1);
        yield return Until(()=>frog.Shots>0,"cuspida real");Check(frog.Shots==2 && director.ActiveDrops==2,"Sapinho solta duas gotas na entrada do lago fechado");Capture("frog-spitting.png");
        scenery.enabled=false;Check(Object.FindAnyObjectByType<LakeBackdrop>().TravelScale==1,"Desativar travessia restaura deslocamento do cenário");
        player.Health.SetInvulnerable(typeof(LakeJourneyCombatValidation),false);player.Health.TakeDamage(999);yield return Frames(4);
        Check(waves.Phase==WaveController.RunPhase.Failed && director.ActiveDrops==0,"Derrota cancela gotas da horda");
        Check(beetle.CurrentMotion==LakeJourneyEnemy.Motion.Stopped && frog.CurrentMotion==LakeJourneyEnemy.Motion.Stopped,"Atores param sem ataques tardios na derrota");
    }
}


