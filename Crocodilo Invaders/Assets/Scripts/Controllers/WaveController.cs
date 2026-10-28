using System;
using UnityEngine;

[RequireComponent(typeof(EnemySpawner), typeof(RunScore))]
public sealed class WaveController : MonoBehaviour
{
    // Intro: abertura narrativa da run, antes do anúncio da primeira horda.
    // ChapterCompleted: boss resgatado, cena final concluída; aguarda a troca para o HUB (sem tela de vitória).
    public enum RunPhase { Waiting, Announcing, Combat, Intermission, Boss, BossDefeated, Completed, Failed, Intro, ChapterCompleted }

    [SerializeField] private Player player;
    [SerializeField, Tooltip("Usado quando Wave Composition está vazia.")]
    private int[] enemiesPerWave = { 6, 8, 10 };
    [SerializeField, Tooltip("Grupos de prefabs na ordem em que entram em cada onda.")]
    private EnemyWave[] waveComposition;
    [SerializeField, Tooltip("Um pool por horda. Quando preenchido, substitui Wave Composition.")]
    private EnemyWavePool[] wavePools;
    [SerializeField, Tooltip("Reproduz somente a seleção das composições, sem alterar os outros sorteios do jogo.")]
    private bool useDebugSeed;
    [SerializeField] private int debugSeed = 12345;
    [SerializeField, Min(0f), Tooltip("Só sem apresentação registrada. Com WavesUI, o combate espera o fim do anúncio.")]
    private float announcementDuration = 1.6f;
    [SerializeField, Min(0f)] private float intermissionDuration = 2f;
    [SerializeField, Min(0f)] private float rewardTransitionDuration = .35f;
    [SerializeField, Min(0)] private int waveClearBonus = 1000;
    [SerializeField] private Boss bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField, Tooltip("Capítulo sem recuperação específica: conclui após a apresentação de derrota do boss.")]
    private bool completeChapterOnBossDefeat;
    [SerializeField, Tooltip("Opcional: cenas narrativas da abertura e do boss.")] private RunCutscenes cutscenes;

    private EnemySpawner spawner;
    private RunScore score;
    private Health playerHealth;
    private float phaseEndsAt;
    private EnemyWave[] runComposition;
    private bool chapterEnding;
    private int announcementId, announcementHolds;
    private bool announcementHeld;

    public RunPhase Phase { get; private set; } = RunPhase.Waiting;
    public int CurrentWave { get; private set; }
    public bool AwaitingReward { get; private set; }

    public bool RequestWaveReward()
    {
        if (Phase != RunPhase.Intermission || CurrentWave >= WaveCount || AwaitingReward) return false;
        AwaitingReward = true;
        player.SetGameplayActive(false);
        return true;
    }

    public void CompleteWaveReward()
    {
        if (Phase != RunPhase.Intermission || !AwaitingReward) return;
        AwaitingReward = false;
        phaseEndsAt = DevelopmentCardSession.IsActive ? float.PositiveInfinity : Time.time + rewardTransitionDuration;
        player.SetGameplayActive(true);
    }
    private bool HasComposition => waveComposition != null && waveComposition.Length > 0;
    private bool HasPools => wavePools != null && wavePools.Length > 0;
    public int CombatWaveCount => HasPools ? wavePools.Length : HasComposition ? waveComposition.Length : (enemiesPerWave?.Length ?? 0);
    public EnemyWave CurrentComposition => runComposition != null && CurrentWave > 0 && CurrentWave <= runComposition.Length
        ? runComposition[CurrentWave - 1] : null;
    public int WaveCount => CombatWaveCount + (bossPrefab != null ? 1 : 0);
    public RunScore Score => score;
    public Boss ActiveBoss { get; private set; }
    public Boss BossPrefab => bossPrefab;
    // A onda do boss vem depois de todas as hordas configuradas, qualquer que seja o número.
    public bool IsBossWave(int wave) => bossPrefab != null && wave > CombatWaveCount;

    // Uma apresentação segura o anúncio atual até terminar. O token identifica o anúncio:
    // liberações atrasadas de um anúncio cancelado não avançam a run.
    public int HoldAnnouncement()
    {
        if (Phase != RunPhase.Announcing) return 0;
        announcementHeld = true;
        announcementHolds++;
        return announcementId;
    }

    public void ReleaseAnnouncement(int token)
    {
        if (token == 0 || token != announcementId || Phase != RunPhase.Announcing || announcementHolds == 0) return;
        announcementHolds--;
    }

    private bool AnnouncementFinished => announcementHeld ? announcementHolds == 0 : Time.time >= phaseEndsAt;
    public event Action<int, int> WaveAnnounced;
    public event Action<int, int> WaveCleared;
    public event Action<bool> RunEnded;
    public event Action<Boss> BossStarted;
    public event Action ChapterCompleted;

    private void Awake()
    {
        spawner = GetComponent<EnemySpawner>();
        score = GetComponent<RunScore>();
        playerHealth = player != null ? player.GetComponent<Health>() : null;
        spawner.StopSpawning();
        bool valid = ValidateWaves(out string error);
        if (playerHealth != null && valid)
        {
            if (GetComponent<WaveMusic>() == null) gameObject.AddComponent<WaveMusic>();
            return;
        }
        Debug.LogError(playerHealth == null ? "Configure o Player no WaveController." : error, this);
        enabled = false;
    }

    public bool ValidateWaves(out string error)
    {
        error = null;
        if (CombatWaveCount == 0) error = "Configure pelo menos uma horda no WaveController.";
        else if (HasPools)
        {
            for (int i = 0; i < wavePools.Length; i++)
            {
                string detail = "o pool é nulo.";
                if (wavePools[i] != null && wavePools[i].Validate(out detail)) continue;
                error = $"Onda {i + 1}: {detail}";
                break;
            }
        }
        else if (HasComposition)
        {
            for (int i = 0; i < waveComposition.Length; i++)
                if (waveComposition[i] == null || !waveComposition[i].IsValid)
                { error = $"Onda {i + 1}: composição inválida."; break; }
        }
        return error == null;
    }

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
        if (spawner != null) spawner.StopSpawning();
        if (ActiveBoss != null) ActiveBoss.Defeated -= OnBossDefeated;
    }

    private void Start()
    {
        if (Phase != RunPhase.Waiting) return;
        player.SetGameplayActive(false);
        if (DevelopmentCardSession.IsActive) return;
#if UNITY_EDITOR
        if (FindAnyObjectByType<BossDevelopmentStart>() != null) return;
#endif
        // Loading confirma pelo fluxo existente; abrir a cena diretamente inicia a run.
        if (FindAnyObjectByType<GameplayLoadingController>() == null) StartRun();
    }

    public void StartRun() => BeginRun(true);

    // Uses the actual post-wave event; CardRewardController still owns freezing,
    // reveal, reroll, choice, upgrades and release of player input.
    public bool StartCardDevelopment()
    {
        if (!DevelopmentCardSession.IsActive || !isActiveAndEnabled || Phase != RunPhase.Waiting || playerHealth.IsDead) return false;
        spawner.StopCombat();
        score.ResetScore();
        player.GetComponent<PlayerRunUpgrades>()?.ResetForRun();
        Phase = RunPhase.Intermission;
        phaseEndsAt = float.PositiveInfinity;
        player.SetGameplayActive(true);
        return RequestDevelopmentCardReward();
    }

    public bool RequestDevelopmentCardReward()
    {
        if (!DevelopmentCardSession.IsActive || Phase != RunPhase.Intermission || AwaitingReward || playerHealth.IsDead) return false;
        int rewardCount = Mathf.Min(CombatWaveCount, WaveCount - 1);
        if (rewardCount < 1) return false;
        CurrentWave = Mathf.Clamp(DevelopmentCardSession.CompletedWave, 1, rewardCount);
        phaseEndsAt = float.PositiveInfinity;
        WaveCleared?.Invoke(CurrentWave, 0);
        return AwaitingReward;
    }

    private void BeginRun(bool playIntro, int completedWaves = 0)
    {
        if (!isActiveAndEnabled || Phase != RunPhase.Waiting || playerHealth.IsDead) return;
        if (!ValidateWaves(out string error)) { Debug.LogError(error, this); return; }
        // Sorteia uma vez por run; EnemySpawner recebe a composição inteira, sem alterá-la.
        runComposition = HasComposition ? waveComposition : null;
        if (HasPools)
        {
            var random = useDebugSeed ? new System.Random(debugSeed) : new System.Random();
            runComposition = new EnemyWave[wavePools.Length];
            for (int i = 0; i < wavePools.Length; i++) runComposition[i] = wavePools[i].Pick(random);
        }
        CurrentWave = completedWaves;
        score.ResetScore();
        LeaderboardSession.StartRun();
        player.GetComponent<PlayerRunUpgrades>()?.ResetForRun();
        // Uma vez por run nova; as ondas só começam quando a abertura termina ou é pulada.
        if (playIntro && cutscenes != null && cutscenes.PlayRunIntro(FinishIntro))
        {
            Phase = RunPhase.Intro;
            player.SetGameplayActive(false);
            return;
        }
        player.SetGameplayActive(true);
        AnnounceNextWave();
    }

    private void FinishIntro()
    {
        if (Phase != RunPhase.Intro || playerHealth.IsDead) return;
        player.SetGameplayActive(true);
        AnnounceNextWave();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (Phase == RunPhase.Announcing && AnnouncementFinished)
        {
            if (IsBossWave(CurrentWave))
            {
                BeginBoss();
                return;
            }
            Phase = RunPhase.Combat;
            if (runComposition != null) spawner.BeginWave(CurrentComposition);
            else spawner.BeginWave(Mathf.Max(1, enemiesPerWave[CurrentWave - 1]));
        }
        else if (Phase == RunPhase.Combat && spawner.RemainingToSpawn == 0 && spawner.AliveCount == 0)
        {
            // A onda acaba quando não há vivos nem inimigos aguardando spawn.
            Phase = RunPhase.Intermission;
            phaseEndsAt = Time.time + intermissionDuration;
            score.AwardPoints(waveClearBonus);
            ClearProjectiles(true);
            WaveCleared?.Invoke(CurrentWave, waveClearBonus);
        }
        else if (Phase == RunPhase.Intermission && !AwaitingReward && Time.time >= phaseEndsAt)
        {
            if (CurrentWave == WaveCount) EndRun(true);
            else AnnounceNextWave();
        }
        else if (Phase == RunPhase.BossDefeated && !chapterEnding && Time.time >= phaseEndsAt)
        {
            if (completeChapterOnBossDefeat) { chapterEnding = true; CompleteChapter(); }
            else EndRun(true);
        }
    }

    public void StartBossForDevelopment()
    {
        if (Phase != RunPhase.Waiting || bossPrefab == null) return;
        // Mesmo caminho da run: a onda do boss é anunciada (ONDA N → alerta) antes da entrada.
        BeginRun(false, CombatWaveCount);
    }

    private void BeginBoss()
    {
        Phase = RunPhase.Boss;
        ClearProjectiles(false);
        Vector3 position = bossSpawnPoint != null ? bossSpawnPoint.position : new Vector3(11.5f, 2f, 0f);
        ActiveBoss = Instantiate(bossPrefab, position, Quaternion.identity);
        ActiveBoss.Defeated += OnBossDefeated;
        BossReinforcements reinforcements = ActiveBoss.GetComponent<BossReinforcements>();
        if (reinforcements != null) reinforcements.Configure(spawner);
        // Antes de BeginBattle: a cena segura só o início do combate, nunca a entrada.
        if (cutscenes != null) cutscenes.PrepareBoss(ActiveBoss);
        ActiveBoss.BeginBattle(playerHealth);
        BossStarted?.Invoke(ActiveBoss);
    }

    private void OnBossDefeated(Boss boss)
    {
        if (Phase != RunPhase.Boss) return;
        Phase = RunPhase.BossDefeated;
        phaseEndsAt = Time.time + boss.DefeatDuration;
        score.RecordDefeat(boss.PointsOnDefeat);
        ClearProjectiles(false);
        if (!boss.RetainedAfterDefeat) return;
        // Boss resgatado: o fim de run antigo (RunEnded/painel de vitória) dá lugar à cena final e ao HUB.
        chapterEnding = true;
        player.SetGameplayActive(false);
        if (cutscenes == null || !cutscenes.PlayBossFinale(boss, CompleteChapter)) CompleteChapter();
    }

    private void CompleteChapter()
    {
        if (Phase != RunPhase.BossDefeated || !chapterEnding) return;
        Phase = RunPhase.ChapterCompleted;
        spawner.StopCombat();
        if (ActiveBoss != null) ActiveBoss.Defeated -= OnBossDefeated;
        player.SetGameplayActive(false);
        ClearProjectiles(false);
        LeaderboardSession.Complete(score.Points, score.Kills, true);
        ChapterCompleted?.Invoke();
    }

    private void AnnounceNextWave()
    {
        CurrentWave++;
        Phase = RunPhase.Announcing;
        phaseEndsAt = Time.time + announcementDuration;
        announcementId++;
        announcementHolds = 0;
        announcementHeld = false;
        WaveAnnounced?.Invoke(CurrentWave, WaveCount);
    }

    private void OnPlayerDied()
    {
        if (Phase == RunPhase.Completed || Phase == RunPhase.Failed || Phase == RunPhase.ChapterCompleted) return;
        EndRun(false);
    }

    private void EndRun(bool completed)
    {
        Phase = completed ? RunPhase.Completed : RunPhase.Failed;
        AwaitingReward = false;
        spawner.StopCombat();
        if (ActiveBoss != null)
        {
            ActiveBoss.Defeated -= OnBossDefeated;
            ActiveBoss.StopBattle();
        }
        player.SetGameplayActive(false);
        ClearProjectiles(false);
        LeaderboardSession.Complete(score.Points, score.Kills, completed);
        RunEnded?.Invoke(completed);
    }

    private void OnApplicationQuit() => LeaderboardSession.SaveCompletedRun();

    private void ClearProjectiles(bool hostileOnly)
    {
        foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
        {
            if (projectile.gameObject.scene != gameObject.scene) continue;
            if (hostileOnly && !projectile.TargetsLayer(player.gameObject.layer)) continue;
            // Destroy é adiado; desativa já para impedir um último acerto na transição.
            projectile.gameObject.SetActive(false);
            Destroy(projectile.gameObject);
        }
    }
}
