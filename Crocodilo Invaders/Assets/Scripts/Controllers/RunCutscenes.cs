using System;
using System.Collections;
using UnityEngine;

// Só orquestra apresentação: WaveController, Boss e GusiniPhaseTwo continuam donos das regras.
public sealed class RunCutscenes : MonoBehaviour
{
    public enum Scene { None, RunIntro, BossIntro, PhaseTransition, BossFinale }
    // Encerramento do capítulo: BossDefeated → AlienRecovery → FinalDialogue → RadioTransmission → Farewell → SceneTransition.
    public enum FinaleStage { None, AlienRecovery, FinalDialogue, RadioTransmission, Farewell, SceneTransition }
    [SerializeField] private WaveController waves;
    [SerializeField] private Player player;
    [SerializeField] private PauseController pause;
    [SerializeField] private DialogueRunner dialogue;
    [Header("Cenas")]
    [SerializeField] private DialogueSequence runIntro;
    [SerializeField] private DialogueSequence bossIntro;
    [SerializeField] private DialogueSequence phaseTransition;
    [SerializeField] private bool playRunIntro = true;
    [SerializeField] private bool playBossIntro = true;
    [SerializeField] private bool playPhaseTransition = true;
    [SerializeField] private DialogueSequence bossFinale;
    [SerializeField] private bool playBossFinale = true;

    private Boss boss;
    private GusiniPhaseTwo phaseTwo;
    private GusiniDefeat defeat;
    private Action finaleCompleted;
    private Coroutine bossRoutine;
    private bool introPlayed, bossIntroPlayed, transitionPlayed, finalePlayed, ownsFreeze;
    private float frozenTimeScale = 1f;
    public Scene Playing { get; private set; }
    public FinaleStage Finale { get; private set; }
    public DialogueRunner Dialogue => dialogue;
    public event Action<Scene> SceneStarted;
    public event Action<Scene> SceneFinished;
    public event Action<FinaleStage> FinaleStageChanged;

    private void OnEnable()
    {
        if (waves != null) waves.RunEnded += OnRunEnded;
        if (dialogue != null) dialogue.LineStarted += OnLineStarted;
    }

    private void OnDisable()
    {
        if (waves != null) waves.RunEnded -= OnRunEnded;
        if (dialogue != null) dialogue.LineStarted -= OnLineStarted;
        Abort();
        Unbind();
    }

    // Chamado por WaveController quando um boss resgatável é derrotado. A conversa espera o fim
    // da animação nativa de recuperação (GusiniDefeat.RecoveryCompleted), não um tempo fixo.
    public bool PlayBossFinale(Boss target, Action completed)
    {
        if (finalePlayed || !playBossFinale || bossFinale == null || dialogue == null || target == null) return false;
        var recovery = target.GetComponent<GusiniDefeat>();
        if (recovery == null) return false;
        finalePlayed = true;
        finaleCompleted = completed;
        player.SetGameplayActive(false);
        SetFinale(FinaleStage.AlienRecovery);
        if (recovery.Recovered) { StartFinaleDialogue(); return true; }
        defeat = recovery;
        defeat.RecoveryCompleted += StartFinaleDialogue;
        return true;
    }

    private void StartFinaleDialogue()
    {
        if (defeat != null) defeat.RecoveryCompleted -= StartFinaleDialogue;
        defeat = null;
        if (Finale != FinaleStage.AlienRecovery) return;
        SetFinale(FinaleStage.FinalDialogue);
        if (!Begin(Scene.BossFinale, bossFinale, FinishFinale)) FinishFinale();
    }

    private void FinishFinale()
    {
        if (Finale == FinaleStage.SceneTransition || Finale == FinaleStage.None) return;
        SetFinale(FinaleStage.SceneTransition);
        Action completed = finaleCompleted;
        finaleCompleted = null;
        completed?.Invoke();
    }

    private void OnLineStarted(int index, DialogueSequence.Line line)
    {
        if (Playing != Scene.BossFinale || string.IsNullOrEmpty(line.beat)) return;
        if (line.beat == "radio") SetFinale(FinaleStage.RadioTransmission);
        else if (line.beat == "farewell") SetFinale(FinaleStage.Farewell);
    }

    private void SetFinale(FinaleStage stage)
    {
        if (Finale == stage) return;
        Finale = stage;
        FinaleStageChanged?.Invoke(stage);
    }

    // Chamado por WaveController.StartRun. Retorna false quando a run deve seguir sem cena.
    public bool PlayRunIntro(Action completed)
    {
        if (introPlayed || !playRunIntro || runIntro == null || dialogue == null || Playing != Scene.None) return false;
        introPlayed = true;
        return Begin(Scene.RunIntro, runIntro, completed);
    }

    // Chamado antes de BeginBattle: a entrada continua, só o início do combate espera o diálogo.
    public void PrepareBoss(Boss target)
    {
        Unbind();
        if (target == null || target.GetComponent<GusiniCombat>() == null) return;
        boss = target;
        phaseTwo = target.GetComponent<GusiniPhaseTwo>();
        if (phaseTwo != null) phaseTwo.ApparentDefeatHeld += OnApparentDefeat;
        if (bossIntroPlayed || !playBossIntro || bossIntro == null || dialogue == null) return;
        bossIntroPlayed = true;
        boss.HoldCombat(true);
        player.SetGameplayActive(false);
        bossRoutine = StartCoroutine(BossIntro());
    }

    private IEnumerator BossIntro()
    {
        // PrepareBoss roda antes de BeginBattle; a batalha só está ativa a partir do retorno.
        yield return null;
        while (boss != null && boss.BattleActive && !boss.Movement.HasArrived) yield return null;
        bossRoutine = null;
        if (boss == null || !boss.BattleActive || !Begin(Scene.BossIntro, bossIntro, ReleaseBoss)) ReleaseBoss();
    }

    private void ReleaseBoss()
    {
        if (boss != null) boss.HoldCombat(false);
        if (RunActive) player.SetGameplayActive(true);
    }

    // A falsa derrota congela o mundo no quadro do clip original; o restante da transformação segue intacto.
    private void OnApparentDefeat()
    {
        if (transitionPlayed || !playPhaseTransition || phaseTransition == null || dialogue == null || Playing != Scene.None) return;
        transitionPlayed = true;
        Freeze(true);
        player.SetGameplayActive(false);
        if (!Begin(Scene.PhaseTransition, phaseTransition, ResumeTransformation)) ResumeTransformation();
    }

    private void ResumeTransformation()
    {
        Freeze(false);
        if (RunActive) player.SetGameplayActive(true);
    }

    private bool Begin(Scene scene, DialogueSequence sequence, Action completed)
    {
        Playing = scene;
        if (!dialogue.Play(sequence, () => Complete(scene, completed)))
        {
            Playing = Scene.None;
            return false;
        }
        SceneStarted?.Invoke(scene);
        return true;
    }

    private void Complete(Scene scene, Action completed)
    {
        Playing = Scene.None;
        completed?.Invoke();
        SceneFinished?.Invoke(scene);
    }

    private bool RunActive => waves == null || (waves.Phase != WaveController.RunPhase.Completed && waves.Phase != WaveController.RunPhase.Failed);

    private void Freeze(bool frozen)
    {
        if (frozen == ownsFreeze) return;
        ownsFreeze = frozen;
        if (frozen)
        {
            frozenTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else Time.timeScale = frozenTimeScale;
        if (pause != null) pause.AllowPauseWhileFrozen(frozen);
    }

    private void OnRunEnded(bool completed) => Abort();

    private void Abort()
    {
        if (bossRoutine != null) StopCoroutine(bossRoutine);
        bossRoutine = null;
        if (dialogue != null) dialogue.Cancel();
        Playing = Scene.None;
        if (boss != null) boss.HoldCombat(false);
        Freeze(false);
    }

    private void Unbind()
    {
        if (defeat != null) defeat.RecoveryCompleted -= StartFinaleDialogue;
        defeat = null;
        if (phaseTwo != null) phaseTwo.ApparentDefeatHeld -= OnApparentDefeat;
        phaseTwo = null;
        boss = null;
    }
}
