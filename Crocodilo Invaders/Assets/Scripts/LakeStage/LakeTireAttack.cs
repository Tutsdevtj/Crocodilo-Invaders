using UnityEngine;

// One clearly shown tire, aim locked just before the throw, a single damaging pass that skips on the water. No destination marker.
public sealed class LakeTireAttack : MonoBehaviour
{
    public enum Phase { Idle, Preparing, Flying, Recover }
    private LakeDifficulty tuning;
    public float PreparationTime => tuning.tirePreparation;
    private SapoDoPneuCombat combat;
    private Health target;
    private Rigidbody2D targetBody;
    private Transform effects;
    private SpriteRenderer bossVisual, held;
    private Sprite sprite;
    private float timer, side;
    private MaterialPropertyBlock waterProperties;
    public Phase CurrentPhase { get; private set; }
    public bool Busy => CurrentPhase != Phase.Idle;
    public bool Preparing => CurrentPhase == Phase.Preparing;
    public bool ArtReady => sprite != null;
    public float PhaseTime => timer;
    public Vector2 LockedAim { get; private set; }
    public Vector2 LaunchPoint { get; private set; }
    public LakeTireProjectile Projectile { get; private set; }
    public SpriteRenderer Held => held;
    public int Throws { get; private set; }
    public int DamageAttempts { get; private set; }
    public int Hits { get; private set; }
    public void Configure(SapoDoPneuCombat owner, Health player, Transform parent, SpriteRenderer visual)
    {
        combat = owner;
        target = player;
        targetBody = player.GetComponent<Rigidbody2D>();
        effects = parent;
        bossVisual = visual;
        tuning = owner.Tuning;
        var art = LakeBackdrop.Current.Art.tire;
        sprite = art != null ? art.drawing : null;
        if (held == null)
        {
            held = new GameObject("SapeneuHeldTire").AddComponent<SpriteRenderer>();
            held.transform.SetParent(effects, false);
            held.sortingOrder = 24;
            held.sprite = sprite;
            held.enabled = false;
            var water = LakeBackdrop.Current.Art.waterContact;
            if (water != null)
            { held.sharedMaterial = water; waterProperties = new MaterialPropertyBlock(); }
        }
    }
    public bool Begin()
    {
        if (Busy || !ArtReady || target == null || target.IsDead)
            return false;
        LockedAim = targetBody.position;
        side = LockedAim.x > transform.position.x ? 1 : -1;
        Projectile = null;
        CurrentPhase = Phase.Preparing;
        LakeAudio.Play(LakeSound.SapeneuGrunt);
        timer = 0;
        held.enabled = true;
        PoseHeld();
        LakeWaterFX.Create(new Vector2(transform.position.x + side, combat.WaterY), .65f);
        return true;
    }
    public bool Step(float dt)
    {
        if (!Busy)
            return true;
        timer += dt;
        switch (CurrentPhase)
        {
            case Phase.Preparing:
                PoseHeld();
                if (timer <= PreparationTime - tuning.tireAimLock)
                    LockedAim = targetBody.position;
                if (timer >= PreparationTime)
                {
                    PoseHeld();
                    LaunchPoint = held.transform.position;
                    held.enabled = false;
                    var node = new GameObject("FlyingLakeTire");
                    node.transform.SetParent(effects, false);
                    node.transform.position = LaunchPoint;
                    node.transform.rotation = held.transform.rotation;
                    node.AddComponent<Rigidbody2D>();
                    node.AddComponent<SpriteRenderer>();
                    Projectile = node.AddComponent<LakeTireProjectile>();
                    Projectile.Configure(combat, this, target, sprite, LaunchPoint, LockedAim, combat.Enraged ? tuning.tireSpeedFury : tuning.tireSpeed, combat.Enraged ? tuning.tireBouncesFury : tuning.tireBounces);
                    Throws++;
                    LakeAudio.Play(LakeSound.TireThrow);
                    CurrentPhase = Phase.Flying;
                    timer = 0;
                }
                break;
            case Phase.Flying:
                // The tire keeps skipping by itself; the frog only watches the throw.
                if (Projectile == null || !Projectile.gameObject.activeSelf || timer >= tuning.tireFlightHold)
                { CurrentPhase = Phase.Recover; timer = 0; }
                break;
            case Phase.Recover:
                if (timer >= tuning.tireRecovery)
                { CurrentPhase = Phase.Idle; return true; }
                break;
        }
        return false;
    }
    private void PoseHeld()
    {
        Vector2 hand = side < 0 ? new Vector2(-1.28f, .48f) : new Vector2(1.12f, .28f);
        Vector2 start = new Vector2(hand.x, -.52f);
        Vector2 pose = Vector2.Lerp(start, hand + Vector2.up * .32f, Mathf.SmoothStep(0, 1, Mathf.Clamp01(timer / .8f)));
        held.transform.position = bossVisual.transform.TransformPoint(pose);
        held.transform.localScale = Vector3.one;
        held.transform.rotation = Quaternion.Euler(0, 0, side * Mathf.Sin(timer / PreparationTime * Mathf.PI) * 35);
        held.color = Color.Lerp(new Color(.55f, .8f, .76f), Color.white, Mathf.Clamp01(timer / .5f));
        if (waterProperties != null)
        {
            waterProperties.SetFloat(LakeShaderIds.WaterY, combat.WaterY);
            waterProperties.SetFloat(LakeShaderIds.Clock, timer);
            waterProperties.SetFloat(LakeShaderIds.Reflection, 0);
            held.SetPropertyBlock(waterProperties);
        }
    }
    private void LateUpdate() { if (Preparing && Time.timeScale > 0) PoseHeld(); }
    public void RegisterContact(bool damaged) { DamageAttempts++; if (damaged) Hits++; }
    public void Cancel()
    {
        if (held != null)
            held.enabled = false;
        if (Projectile != null)
        { Projectile.Cancel(); Projectile = null; }
        CurrentPhase = Phase.Idle;
        timer = 0;
    }
    private void OnDisable() { Cancel(); }
}
