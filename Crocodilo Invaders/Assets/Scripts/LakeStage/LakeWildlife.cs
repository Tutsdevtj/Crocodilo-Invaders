using UnityEngine;

[DefaultExecutionOrder(30)]
public sealed class LakeWildlife : MonoBehaviour
{
    public const float EntryMargin = .08f, ClearMargin = .55f, WarningTime = .65f;
    [SerializeField] private LakeWildlifeAssets art;
    private LakeBackdrop lake;
    private Player player;
    private Rigidbody2D playerBody;
    private Collider2D hull;
    private WaveController waves;
    private LakeJourneyDirector journey;
    private Transform effects;
    private readonly SpriteRenderer[] fish = new SpriteRenderer[8];
    private readonly float[] fishX = new float[8];
    private readonly LakePiranha[] predators = new LakePiranha[10];
    private readonly LakePiranha[] queue = new LakePiranha[5];
    private float nextAmbient = 6;
    public int AmbientJumps { get; private set; }
    private float clock, nextPack, warningAt, nextLaunch, nextBite;
    private int launched, queued;
    private bool winding, launching;
    public float Clock => clock;
    public float WaterY => lake.WaterY;
    public Health PlayerHealth => player != null ? player.Health : null;
    public Vector2 PlayerPosition => playerBody != null ? playerBody.position : Vector2.zero;
    public bool WaterDangerActive { get; private set; }
    public bool CanHurt => Playable;
    public int Warnings { get; private set; }
    public int Packs { get; private set; }
    public int Launches { get; private set; }
    public int Bites { get; private set; }
    public int ActivePiranhas { get { int count = 0; foreach (var p in predators) if (p != null && p.Active) count++; return count; } }
    public int AmbientFishCount => fish[0] != null ? fish.Length : 0;
    public SpriteRenderer AmbientFish(int index) => fish[index];
    public LakePiranha Piranha(int index) => predators[index];
    public float FishDirection(int index) => index % 2 == 0 ? 1 : -1;
    public Rect Arena => lake.ViewRect;
    private bool Playable => (journey == null || journey.PiranhasEnabled) && player != null && !player.Health.IsDead && player.Movement.isActiveAndEnabled && waves != null &&
        (waves.Phase == WaveController.RunPhase.Combat || waves.Phase == WaveController.RunPhase.Boss || waves.Phase == WaveController.RunPhase.Announcing);
    private void Start()
    {
        lake = GetComponent<LakeBackdrop>();
        if (art == null)
            art = lake.Art.wildlife;
        if (lake == null || art == null || art.fishFrames.Length != 4 || art.piranhaFrames.Length != 6)
        { enabled = false; return; }
        waves = FindAnyObjectByType<WaveController>();
        journey = GetComponent<LakeJourneyDirector>();
        if (waves != null)
            waves.RunEnded += Ended;
        effects = new GameObject("LakeWildlifeEffects").transform;
        effects.SetParent(transform, false);
        for (int i = 0; i < fish.Length; i++)
        {
            fish[i] = new GameObject("AmbientLakeFish_" + i).AddComponent<SpriteRenderer>();
            fish[i].transform.SetParent(transform, false);
            fish[i].sprite = art.fishFrames[i % 4];
            fish[i].sortingOrder = -185;
            fish[i].color = new Color(.43f, .75f, .73f, .62f);
            fish[i].transform.localScale = Vector3.one * (.58f + i % 3 * .16f);
            fishX[i] = Arena.xMin + Mathf.Repeat(i * .381966f, 1) * Arena.width;
        }
        for (int i = 0; i < predators.Length; i++)
        {
            var node = new GameObject("LakePiranha_" + i, typeof(Rigidbody2D), typeof(SpriteRenderer));
            node.transform.SetParent(transform, false);
            predators[i] = node.AddComponent<LakePiranha>();
            predators[i].Configure(this, art, i);
        }
        FindPlayer();
    }
    private void FindPlayer()
    {
        player = FindAnyObjectByType<Player>();
        if (player == null)
            return;
        playerBody = player.GetComponent<Rigidbody2D>();
        hull = player.GetComponent<Collider2D>();
    }
    private void FixedUpdate()
    {
        if (lake == null || art == null)
            return;
        float dt = Time.fixedDeltaTime;
        clock += dt;
        if (player == null)
            FindPlayer();
        if (!Playable)
        { StopPredators(false); foreach (var p in predators) if (p != null) p.Tick(dt); return; }
        float bottom = hull != null ? hull.bounds.min.y : playerBody.position.y - .4f;
        if (!WaterDangerActive && bottom <= WaterY + EntryMargin)
        { WaterDangerActive = true; nextPack = clock; }
        else if (WaterDangerActive && bottom > WaterY + ClearMargin)
            StopPredators(false);
        if (WaterDangerActive)
        {
            if (!winding && !launching && clock >= nextPack)
                BeginPack();
            if (winding && clock - warningAt >= WarningTime)
            {
                winding = false;
                launching = true;
                launched = 0;
                nextLaunch = clock;

            }
            if (launching && clock >= nextLaunch)
            {
                Vector2 aim = PlayerPosition + Vector2.right * Mathf.Clamp(playerBody.linearVelocity.x * .1f, -.5f, .5f);
                aim.y = Mathf.Min(aim.y, WaterY + .8f);
                if (aim.y > WaterY - .18f)
                { queue[launched].Launch(aim); Launches++; }
                else
                    queue[launched].Retreat();
                launched++;
                nextLaunch = clock + .3f;
                if (launched >= queued)
                    launching = false;
            }
        }
        if (!WaterDangerActive && clock >= nextAmbient)
        {
            nextAmbient = clock + Random.Range(7f, 11f);
            foreach (var p in predators)
            if (p.Available && Mathf.Abs(p.Position.x - PlayerPosition.x) > 2.4f)
            { p.Launch(new Vector2(p.Position.x + (p.Velocity.x < 0 ? -1.2f : 1.2f), WaterY + .35f), true); AmbientJumps++; break; }
        }
        foreach (var p in predators)
        if (p != null)
            p.Tick(dt);
    }
    private void BeginPack()
    {
        queued = 0;
        for (int k = 0; k < 2; k++)
        {
            LakePiranha nearest = null;
            float distance = 4;
            foreach (var p in predators)
            if (p.Available && Vector2.Distance(p.Position, PlayerPosition) < distance)
            { nearest = p; distance = Vector2.Distance(p.Position, PlayerPosition); }
            if (nearest == null)
                break;
            queue[queued++] = nearest;
            nearest.Gather();
        }
        if (queued == 0)
        { nextPack = clock + .2f; return; }
        warningAt = clock;
        nextPack = clock + 1.85f;
        winding = true;
        Warnings++;
        Packs++;

    }
    public void Bite()
    {
        if (!CanHurt || clock < nextBite)
            return;
        nextBite = clock + .65f;
        if (player.Health.TakeDamage(1))
            Bites++;
    }
    public void Splash(float x, float size) { LakeWaterFX.Create(new Vector2(x, WaterY), size); }
    private void StopPredators(bool immediate)
    {
        WaterDangerActive = false;
        winding = launching = false;

        foreach (var p in predators)
        if (p != null)
        { if (immediate) p.Park(); else p.Retreat(); }
    }
    private void Ended(bool completed) { StopPredators(true); }
    private void LateUpdate()
    {
        if (fish[0] == null)
            return;
        Rect area = Arena;
        for (int i = 0; i < fish.Length; i++)
        {
            fishX[i] += FishDirection(i) * (.36f + i % 4 * .13f) * Time.deltaTime;
            if (fishX[i] > area.xMax + 1)
                fishX[i] = area.xMin - 1;
            if (fishX[i] < area.xMin - 1)
                fishX[i] = area.xMax + 1;
            float depth = .6f + (i % 4) * .38f;
            fish[i].transform.position = new Vector3(fishX[i], Mathf.Max(area.yMin + .35f, WaterY - depth) + Mathf.Sin(clock * 1.3f + i) * .045f, 2);
            fish[i].flipX = FishDirection(i) < 0;
            fish[i].sprite = art.fishFrames[(int)(clock * 7 + i) % 4];
        }
    }
    private void OnDisable() { if (waves != null) waves.RunEnded -= Ended; StopPredators(true); }
}
