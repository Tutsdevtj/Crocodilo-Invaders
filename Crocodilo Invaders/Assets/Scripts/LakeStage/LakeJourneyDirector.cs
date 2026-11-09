using System.Collections.Generic;
using UnityEngine;

// Coordinates only lake enemies; WaveController and the card controller still own the run.
[DefaultExecutionOrder(-20)]
public sealed class LakeJourneyDirector : MonoBehaviour, ILakeDropOwner
{
    private WaveController waves;
    private EnemySpawner spawner;
    private LakeBackdrop lake;
    private Player player;
    private readonly HashSet<LakeJourneyEnemy> striking = new HashSet<LakeJourneyEnemy>();
    private readonly HashSet<LakeJourneyEnemy> preparing = new HashSet<LakeJourneyEnemy>();
    private readonly Queue<LakeJourneyEnemy> preparationOrder = new Queue<LakeJourneyEnemy>();
    private readonly LakeJourneyEnemy[] lilyOwners = new LakeJourneyEnemy[9];
    private Transform threats;
    private float clock, nextStrike, nextDamage;
    private int ordinal;
    public int Wave => waves != null ? waves.CurrentWave : 0;
    public int Section => Wave <= 3 ? 0 : Wave <= 6 ? 1 : 2;
    public bool CombatActive => waves != null && waves.Phase == WaveController.RunPhase.Combat && player != null && !player.Health.IsDead;
    public bool PiranhasEnabled => Wave >= 5 || (waves != null && waves.IsBossWave(Wave));
    public bool Calm => waves != null && waves.IsBossWave(Wave) && waves.Phase == WaveController.RunPhase.Announcing;
    public float Clock => clock;
    public float WaterY => lake.WaterY;
    public Health Target => player != null ? player.Health : null;
    public Rigidbody2D TargetBody { get; private set; }
    public Transform Threats => threats;
    public Material WaterMaterial => lake.WaterFXMaterial;
    public Sprite DropSprite { get; private set; }
    public int Attacks { get; private set; }
    public int Hits { get; private set; }
    public int Spawned { get; private set; }
    public int ActivePreparations => preparing.Count;
    public int ActiveStrikes => striking.Count;
    public int MaxStrikes => Section >= 1 ? lake.Difficulty.lateStrikes : lake.Difficulty.earlyStrikes;
    private float StrikeGap => Section >= 1 ? lake.Difficulty.lateStrikeGap : lake.Difficulty.earlyStrikeGap;
    public int ActiveDrops { get; private set; }
    public bool DropsLive => CombatActive;
    public Health DropTarget => Target;
    public float DropWaterY => WaterY;
    public Rect DropArena => Arena;
    public void DropStarted() { ActiveDrops++; }
    public void DropEnded() { ActiveDrops--; }
    public void DropHit() { Hit(); }
    public void DropReturned() { }
    public Rect Arena => lake.ViewRect;
    private void Awake()
    {
        lake = GetComponent<LakeBackdrop>();
        waves = FindAnyObjectByType<WaveController>();
        spawner = waves != null ? waves.GetComponent<EnemySpawner>() : null;
        player = FindAnyObjectByType<Player>();
        TargetBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        DropSprite = lake.Art.waterDrop;
        threats = new GameObject("LakeJourneyThreats").transform;
        threats.SetParent(transform, false);
    }
    private void OnEnable()
    {
        if (spawner != null)
            spawner.EnemySpawned += SpawnedEnemy;
        if (waves != null)
        { waves.WaveAnnounced += Announced; waves.RunEnded += Ended; }
    }
    private void Update()
    {
        clock += Time.deltaTime;
        if (!CombatActive && (ActiveDrops > 0 || (threats != null && threats.childCount > 0)))
            ClearThreats();
    }
    private void SpawnedEnemy(Enemy enemy)
    {
        var actor = enemy.GetComponent<LakeJourneyEnemy>();
        if (actor == null)
            return;
        actor.Join(this, ordinal++);
        Spawned++;
    }
    private void Announced(int wave, int total) { ordinal = 0; System.Array.Clear(lilyOwners, 0, lilyOwners.Length); preparing.Clear(); preparationOrder.Clear(); striking.Clear(); nextStrike = clock + .9f; ClearThreats(); }
    public float ReserveLily(LakeJourneyEnemy enemy)
    {
        for (int i = 0; i < lilyOwners.Length; i++)
        if (lilyOwners[i] == null)
        { lilyOwners[i] = enemy; return Arena.xMin + Arena.width * (i < 7 ? .89f - i * .135f : .89f - .135f * (1.5f + (i - 7) * 3)); }
        return Arena.center.x;
    }
    public void ReleaseLily(LakeJourneyEnemy enemy)
    { for (int i = 0; i < lilyOwners.Length; i++) if (lilyOwners[i] == enemy) lilyOwners[i] = null; }
    public bool Prepare(LakeJourneyEnemy enemy)
    {
        if (!CombatActive || enemy == null)
            return false;
        if (preparing.Contains(enemy))
            return true;
        if (preparing.Count >= 2)
            return false;
        preparing.Add(enemy);
        preparationOrder.Enqueue(enemy);
        return true;
    }
    public bool Strike(LakeJourneyEnemy enemy)
    {
        while (preparationOrder.Count > 0 && (preparationOrder.Peek() == null || !preparing.Contains(preparationOrder.Peek())))
            preparationOrder.Dequeue();
        if (!CombatActive || !preparing.Contains(enemy) || preparationOrder.Count == 0 || preparationOrder.Peek() != enemy || striking.Count >= MaxStrikes || clock < nextStrike || (Section == 0 && ActiveDrops > 0))
            return false;
        preparationOrder.Dequeue();
        striking.Add(enemy);
        preparing.Remove(enemy);
        nextStrike = clock + StrikeGap;
        Attacks++;
        return true;
    }
    public void Release(LakeJourneyEnemy enemy)
    {
        preparing.Remove(enemy);
        if (striking.Remove(enemy))
            nextStrike = Mathf.Max(nextStrike, clock + .25f);
    }
    public bool Hit()
    {
        if (!CombatActive || clock < nextDamage)
            return false;
        nextDamage = clock + lake.Difficulty.journeyDamageGate;
        if (!player.Health.TakeDamage(1))
            return false;
        Hits++;
        return true;
    }
    private void ClearThreats()
    {
        LakeFxPool.ReleaseDrops(this);
        if (threats == null)
            return;
        foreach (Transform child in threats)
        { child.gameObject.SetActive(false); Destroy(child.gameObject); }
    }
    private void Ended(bool won) { striking.Clear(); preparing.Clear(); preparationOrder.Clear(); ClearThreats(); }
    private void OnDisable()
    {
        if (spawner != null)
            spawner.EnemySpawned -= SpawnedEnemy;
        if (waves != null)
        { waves.WaveAnnounced -= Announced; waves.RunEnded -= Ended; }
        Ended(false);
    }
}
