using System;
using UnityEngine;

[RequireComponent(typeof(EnemySpawner), typeof(RunScore))]
public sealed class WaveController : MonoBehaviour
{
    public enum RunPhase { Waiting, Announcing, Combat, Intermission, Boss, BossDefeated, Completed, Failed }

    [SerializeField] private Player player;
    [SerializeField, Tooltip("Usado quando Wave Composition está vazia.")]
    private int[] enemiesPerWave = { 6, 8, 10 };
    [SerializeField, Tooltip("Grupos de prefabs na ordem em que entram em cada onda.")]
    private EnemyWave[] waveComposition;
    [SerializeField, Min(0f)] private float announcementDuration = 1.6f;
    [SerializeField, Min(0f)] private float intermissionDuration = 2f;
    [SerializeField, Min(0f)] private float rewardTransitionDuration = .35f;
    [SerializeField, Min(0)] private int waveClearBonus = 1000;
    [SerializeField] private Boss bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    private EnemySpawner spawner;
    private RunScore score;
    private Health playerHealth;
    private float phaseEndsAt;

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
        phaseEndsAt = Time.time + rewardTransitionDuration;
        player.SetGameplayActive(true);
    }
    private bool HasComposition => waveComposition != null && waveComposition.Length > 0;
    private int CombatWaveCount => HasComposition ? waveComposition.Length : (enemiesPerWave?.Length ?? 0);
    public int WaveCount => CombatWaveCount + (bossPrefab != null ? 1 : 0);
    public RunScore Score => score;
    public Boss ActiveBoss { get; private set; }
    public event Action<int, int> WaveAnnounced;
    public event Action<int, int> WaveCleared;
    public event Action<bool> RunEnded;
    public event Action<Boss> BossStarted;

    private void Awake()
    {
        spawner = GetComponent<EnemySpawner>();
        score = GetComponent<RunScore>();
        playerHealth = player != null ? player.GetComponent<Health>() : null;
        spawner.StopSpawning();
        bool valid = CombatWaveCount > 0;
        if (HasComposition)
            foreach (EnemyWave wave in waveComposition) valid &= wave != null && wave.IsValid;
        if (playerHealth != null && valid) return;
        Debug.LogError("Configure o Player e pelo menos uma onda no WaveController.", this);
        enabled = false;
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
        if (Phase == RunPhase.Waiting) player.SetGameplayActive(false);
    }

    public void StartRun()
    {
        if (!isActiveAndEnabled || Phase != RunPhase.Waiting || playerHealth.IsDead) return;
        score.ResetScore();
        player.GetComponent<PlayerRunUpgrades>()?.ResetForRun();
        player.SetGameplayActive(true);
        AnnounceNextWave();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (Phase == RunPhase.Announcing && Time.time >= phaseEndsAt)
        {
            if (CurrentWave > CombatWaveCount)
            {
                BeginBoss();
                return;
            }
            Phase = RunPhase.Combat;
            if (HasComposition) spawner.BeginWave(waveComposition[CurrentWave - 1]);
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
        else if (Phase == RunPhase.BossDefeated && Time.time >= phaseEndsAt) EndRun(true);
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
    }

    private void AnnounceNextWave()
    {
        CurrentWave++;
        Phase = RunPhase.Announcing;
        phaseEndsAt = Time.time + announcementDuration;
        WaveAnnounced?.Invoke(CurrentWave, WaveCount);
    }

    private void OnPlayerDied()
    {
        if (Phase == RunPhase.Completed || Phase == RunPhase.Failed) return;
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
        RunEnded?.Invoke(completed);
    }

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
