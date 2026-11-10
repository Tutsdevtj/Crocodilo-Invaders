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

[LakePlaySuite(200)]
public static class LakeBossEntrancePlayValidation
{
    public static void Run() => Launch(typeof(LakeBossEntrancePlayValidation), LakeStageTools.Test);
    public static void RunFromMenu() => Launch(typeof(LakeBossEntrancePlayValidation), "Assets/Scenes/MainMenu.unity");
    private static Player player;
    private static Boss boss;
    private static SapoDoPneuCombat combat;
    private static WaveController waves;
    private static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v17/TestSave");
        yield return EnterLake();
        var intro=Object.FindAnyObjectByType<LakeBossEntrance>();var announcer=Object.FindAnyObjectByType<WaveAnnouncer>();
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.GetComponent<Rigidbody2D>().position=new Vector2(-3,1);
        Check(waves.CurrentWave==10 && waves.IsBossWave(10),"Onda dez continua sendo a rodada do boss");
        Check(intro.CurrentStage==LakeBossEntrance.Stage.Round && announcer.IsPlaying && announcer.WaveNumber==10,"Anuncio original da onda dez precede apresentacao propria");
        Check(waves.ActiveBoss==null && waves.Phase==WaveController.RunPhase.Announcing,"Boss nao aparece antes do aviso");
        yield return Wait(.3f);Capture("round-10.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Warning,"aviso do lago");yield return Wait(.15f);
        Check(!player.Movement.enabled && !player.Combat.enabled && player.Health.IsInvulnerable,"Jogador protegido e sem atacar durante apresentacao");
        int hp=player.Health.CurrentHealth;Check(!player.Health.TakeDamage(2) && player.Health.CurrentHealth==hp,"Protecao da abertura impede dano real");
        Check(intro.PresentationCanvas.sortingOrder==240 && !intro.PresentationCanvas.GetComponent<CanvasGroup>().blocksRaycasts,"Apresentacao fica abaixo da pausa e nao bloqueia interface");Capture("lake-warning.png");
        float clock=intro.Clock,waterClock=lake.MotionClock;Time.timeScale=0;AudioListener.pause=true;
        double pause=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<pause)yield return null;
        Check(intro.Clock==clock && lake.MotionClock==waterClock && intro.CurrentStage==LakeBossEntrance.Stage.Warning,"Pausa congela aviso e ambiente juntos");Time.timeScale=1;AudioListener.pause=false;
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Reveal,"revelacao do nome");yield return Wait(.5f);
        Check(waves.Phase==WaveController.RunPhase.Announcing && waves.ActiveBoss==null,"Nome e retrato seguram anuncio sem iniciar luta");
        var title=intro.PresentationCanvas.GetComponentsInChildren<TMPro.TMP_Text>().First(t=>t.name=="BossTitle");title.ForceMeshUpdate();
        Check(title.text=="SAPENEU CURURU" && !title.isTextOverflowing,"Nome completo legivel na apresentacao propria");Capture("sapeneu-title.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Arrival && waves.ActiveBoss!=null,"sapo emergindo");
        boss=waves.ActiveBoss;combat=boss.GetComponent<SapoDoPneuCombat>();var body=boss.GetComponent<Rigidbody2D>();
        Check(boss.CombatHeld && !boss.Ready && body.position.y<lake.WaterY,"Sapeneu comeca submerso e ainda sem combate");
        float startY=body.position.y;yield return Wait(.7f);
        Check(body.position.y>startY && combat.CurrentState==SapoDoPneuCombat.State.Entering && !combat.Tongue.Busy,"Boss emerge sem atacar");
        Check(boss.Health.IsInvulnerable && !player.Movement.enabled,"Entrada fisica mantem protecao e controle bloqueado");Capture("water-emergence.png");
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Finished,"fim da abertura");yield return Frames(4);
        Check(boss.Ready && !boss.CombatHeld && boss.Movement.HasArrived,"Fim da abertura libera boss pronto exatamente uma vez");
        Check(player.Movement.enabled && player.Combat.enabled && !player.Health.IsInvulnerable,"Controle e vulnerabilidade do jogador retornam");
        Check(!boss.Health.IsInvulnerable && !intro.PresentationCanvas.gameObject.activeSelf,"Boss pode receber dano e tela cinematica desaparece");
        yield return Wait(.1f);Check(intro.Entrances==1 && combat.CurrentState==SapoDoPneuCombat.State.Float,"Sem repeticao da abertura ou ataque antecipado");Capture("battle-ready.png");
        player.Health.SetInvulnerable(typeof(LakeBossEntrancePlayValidation),true);yield return Until(()=>combat.Tongue.Busy,"combate apos abertura");
        Check(combat.Tongue.AliveFlyCount>0 && Object.FindObjectsByType<LakeLilyPads>().Length==1,"Moscas e folhas continuam apos a entrada");yield return Wait(1.2f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"novo menu");yield return EnterLake();
        intro=Object.FindAnyObjectByType<LakeBossEntrance>();waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();player.GetComponent<PlayerInputReader>().enabled=false;player.GetComponent<Rigidbody2D>().position=new Vector2(-3,1);
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Warning,"aviso para cancelamento");intro.enabled=false;yield return Frames(3);
        Check(!intro.IsPlaying && player.Movement.enabled && !player.Health.IsInvulnerable,"Desativar apresentacao restaura jogador e remove protecao");
        yield return Until(()=>waves.ActiveBoss?.Ready==true,"boss sem apresentacao apos cancelamento");
        Check(waves.Phase==WaveController.RunPhase.Boss && !waves.ActiveBoss.CombatHeld,"Cancelamento libera token e nao prende a fase");

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");yield return Until(()=>Object.FindAnyObjectByType<DeveloperMenuView>()!=null,"terceiro menu");yield return EnterLake();
        intro=Object.FindAnyObjectByType<LakeBossEntrance>();waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();
        yield return Until(()=>intro.CurrentStage==LakeBossEntrance.Stage.Reveal,"titulo para encerramento");
        player.Health.SetInvulnerable(intro,false);player.Health.TakeDamage(9999);yield return Frames(4);
        Check(waves.Phase==WaveController.RunPhase.Failed && !intro.IsPlaying && waves.ActiveBoss==null,"Encerrar run durante titulo cancela entrada sem spawn atrasado");
        Check(!player.Movement.enabled && !intro.PresentationCanvas.gameObject.activeSelf,"Derrota nao reativa controles nem deixa sobreposicao");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"Progresso real preservado");
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

