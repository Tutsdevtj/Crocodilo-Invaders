using UnityEngine;

// Shares the encounter's health/lifetime; she is an assisting character, not a second boss.
public sealed class SapererecaCompanion : MonoBehaviour, ILakeDropOwner
{
    public enum State { InHud, Entering, Rest, Warning, Landing, Spit, Stopped, Dive, Swim, Surface, Ready }
    public const float EntranceDuration = 1.25f, WarningDuration = .65f;
    public const float WorldScale = 1.65f;
    private LakeDifficulty tuning;
    private SapoDoPneuCombat combat;
    private SapererecaAssets art;
    private Health target;
    private Rigidbody2D targetBody;
    private Sprite dropSprite;
    private int activeDrops;
    private Rigidbody2D body;
    private SpriteRenderer visual, reflection;
    private Transform threats;
    private LakeBossBar hud;
    private LakeBackdrop lake;
    private float standingFraction = .46f, landingFraction = .46f;
    private Vector2 WaterBerth(float fraction) => new Vector2(combat.Arena.xMin + combat.Arena.width * fraction, combat.WaterY + .06f + Mathf.Sin((lake != null ? lake.MotionClock : Time.time) * 1.7f + fraction * 8) * .025f);
    private SpriteRenderer surfaceEyes;
    public bool EyesVisible => surfaceEyes != null && surfaceEyes.enabled;
    public Vector2 EyesPosition => surfaceEyes != null ? (Vector2)surfaceEyes.transform.position : Vector2.zero;
    private MaterialPropertyBlock waterProperties, reflectionProperties;
    private Vector2 from, landing, lockedAim;
    private float timer, clock, nextAttack, nextDamage;
    private int berth;
    private int burstShots;
    private bool tongueCombo;

    public int Dives { get; private set; }
    public int Combos { get; private set; }
    public int Volleys { get; private set; }
    public Vector2 LastLaunch { get; private set; }
    public Vector2 LastVelocity { get; private set; }
    public float LastTravel { get; private set; }
    public State CurrentState { get; private set; } = State.InHud;
    public bool Busy => (Joined && CurrentState != State.Rest && CurrentState != State.Stopped) || CurrentState == State.Entering || ActiveDrops > 0;
    public bool Joined { get; private set; }
    public int Entries { get; private set; }
    public int Shots { get; private set; }
    public int Hits { get; private set; }
    public int ActiveDrops => activeDrops;
    public bool DropsLive => combat != null && combat.Active;
    public Health DropTarget => target;
    public float DropWaterY => combat.WaterY;
    public Rect DropArena => combat.Arena;
    public void DropStarted() { activeDrops++; }
    public void DropEnded() { activeDrops--; }
    public void DropHit() { TryHit(); }
    public void DropReturned() { combat.DropReturned(); }
    public Vector2 Position => body != null ? body.position : Vector2.zero;
    public Vector2 LockedAim => lockedAim;
    public Vector2 Landing => landing;
    public SpriteRenderer Visual => visual;
    public float Clock => clock;
    public float PhaseTime => timer;
    public void Configure(SapoDoPneuCombat owner, Health player)
    {
        combat = owner;
        target = player;
        targetBody = player.GetComponent<Rigidbody2D>();
        tuning = owner.Tuning;
        art = LakeBackdrop.Current.Art.companion;
        dropSprite = LakeBackdrop.Current.Art.waterDrop;
        lake = LakeBackdrop.Current;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        visual = GetComponent<SpriteRenderer>();
        visual.sortingOrder = 19;
        visual.enabled = false;
        threats = new GameObject("SapererecaWaterThreats").transform;
        threats.SetParent(transform, false);
        transform.localScale = Vector3.one * WorldScale;
        var rework = LakeBackdrop.Current.Art.rework;
        if (rework != null && rework.surfaceEyes != null)
        {
            surfaceEyes = new GameObject("SapererecaWatchingEyes").AddComponent<SpriteRenderer>();
            surfaceEyes.transform.SetParent(threats, false);
            surfaceEyes.sprite = rework.surfaceEyes;
            surfaceEyes.sortingOrder = 25;
            surfaceEyes.transform.localScale = Vector3.one;
            surfaceEyes.enabled = false;
        }
        var material = LakeBackdrop.Current.Art.waterContact;
        if (material != null)
        {
            visual.sharedMaterial = material;
            if (surfaceEyes != null)
                surfaceEyes.sharedMaterial = material;
            waterProperties = new MaterialPropertyBlock();
            reflectionProperties = new MaterialPropertyBlock();
            reflection = new GameObject("SapererecaWaterReflection").AddComponent<SpriteRenderer>();
            reflection.transform.SetParent(transform, false);
            reflection.sharedMaterial = material;
            reflection.sortingOrder = 9;
            reflection.enabled = false;
        }
    }
    public void Enter()
    {
        if (CurrentState != State.InHud || art == null)
            return;
        Entries++;
        landingFraction = standingFraction = .46f;
        landing = WaterBerth(landingFraction);
        body.position = landing;
        hud = FindAnyObjectByType<LakeBossBar>();
        if (hud != null)
            hud.BeginCompanionJump(landing);
        SetState(State.Entering);
    }
    private void SetState(State state) { CurrentState = state; timer = 0; }
    private void FixedUpdate()
    {
        if (combat == null || !combat.Active)
        { Stop(); return; }
        if (combat.IsVulnerable)
        { if (Joined) body.MovePosition(WaterBerth(standingFraction)); return; }
        float dt = Time.fixedDeltaTime;
        clock += dt;
        timer += dt;
        if (CurrentState == State.Entering)
            landing = WaterBerth(landingFraction);
        switch (CurrentState)
        {
            case State.Entering:
                if (timer >= EntranceDuration)
                {
                    Joined = true;
                    visual.enabled = true;
                    visual.sprite = art.frames[3];
                    body.position = landing;
                    LandingRipple();
                    nextAttack = clock + tuning.helperFirstAttack;
                    SetState(State.Landing);
                }
                break;
            case State.Rest:
                body.MovePosition(WaterBerth(standingFraction));
                if (clock >= nextAttack && ActiveDrops == 0 && !combat.Tongue.Busy && !combat.Tongue.MealPending)
                    BeginDive(false);
                break;
            case State.Dive:
                body.MovePosition(Vector2.Lerp(from, from + Vector2.down * 1.9f, Mathf.SmoothStep(0, 1, timer / .42f)));
                if (timer >= .42f)
                { visual.enabled = false; from = body.position; SetState(State.Swim); }
                break;
            case State.Swim:
                body.MovePosition(Vector2.Lerp(from, landing + Vector2.down * 1.9f, Mathf.SmoothStep(0, 1, timer / .72f)));
                if (timer >= .72f)
                {
                    lockedAim = targetBody.position;
                    SetState(State.Warning);
                }
                break;
            case State.Warning:
                if (timer >= WarningDuration)
                { visual.enabled = true; from = body.position; LandingRipple(); SetState(State.Surface); LakeAudio.Play(LakeSound.SapererecaCroak); }
                break;
            case State.Surface:
                body.MovePosition(Vector2.Lerp(from, landing, Mathf.SmoothStep(0, 1, timer / .25f)));
                if (timer >= .25f)
                { body.position = landing; standingFraction = landingFraction; SetState(State.Ready); }
                break;
            case State.Ready:
                body.MovePosition(WaterBerth(standingFraction));
                // Preparation overlaps the tongue, but damaging shots wait until it has fully retracted.
                if (combat.Tongue.Busy)
                    timer = 0;
                if (timer <= tuning.helperReadyTime - tuning.helperAimLock)
                    lockedAim = PredictedAim();
                if (timer >= tuning.helperReadyTime)
                { burstShots = 0; Volleys++; SetState(State.Spit); }
                break;
            case State.Landing:
                if (timer >= .2f)
                    SetState(State.Rest);
                break;
            case State.Spit:
                if (burstShots < tuning.helperShots && timer >= burstShots * tuning.helperShotInterval)
                { Emit(burstShots - 1); burstShots++; }
                if (timer >= .65f)
                {
                    if (surfaceEyes != null)
                        surfaceEyes.enabled = false;
                    nextAttack = clock + (tongueCombo ? tuning.helperComboCooldown : tuning.helperCooldown);
                    tongueCombo = false;
                    SetState(State.Rest);
                }
                break;
        }
    }
    public void PrepareTongueCombo()
    {
        if (Joined && CurrentState == State.Rest && ActiveDrops == 0)
            BeginDive(true);
    }
    private Vector2 PredictedAim()
    {
        return targetBody.position + Vector2.ClampMagnitude(targetBody.linearVelocity * tuning.helperAimLead, tuning.helperMaxAimLead);
    }
    private void BeginDive(bool combo)
    {
        berth++;
        Dives++;
        tongueCombo = combo;
        if (combo)
            Combos++;
        float playerFraction = (target.transform.position.x - combat.Arena.xMin) / combat.Arena.width;
        float fraction = Mathf.Clamp(playerFraction + (berth % 2 == 0 ? .22f : -.22f), .15f, .85f);
        if (Mathf.Abs(WaterBerth(fraction).x - combat.transform.position.x) < 2.7f)
            fraction = combat.transform.position.x < combat.Arena.center.x ? .78f : .22f;
        landingFraction = fraction;
        landing = WaterBerth(landingFraction);
        from = body.position;
        LakeWaterFX.Create(new Vector2(from.x, combat.WaterY), .75f);

        SetState(State.Dive);
    }
    private void LandingRipple()
    {
        if (lake != null)
            LakeSurfaceRipple.Create(new Vector2(landing.x, combat.WaterY), .45f);
    }
    private void Emit(int spread)
    {
        float side = lockedAim.x < body.position.x ? -1 : 1;
        Vector2 start = body.position + new Vector2(side * .42f, .52f) * WorldScale;
        // The same compact arcs and vertical spacing as the small frogs.
        float travel = tuning.helperShotFlight;
        Vector2 velocity = LakeDroplet.LaunchThrough(start, lockedAim, travel) + Vector2.up * (spread * tuning.helperShotSpread);
        LastLaunch = start;
        LastVelocity = velocity;
        LastTravel = travel;
        LakeDroplet.Fire(this, dropSprite, start, velocity, LakeDropStyle.Helper);
        LakeAudio.Play(LakeSound.Spit);
        Shots++;
    }
    public bool TryHit()
    {
        if (combat == null || !combat.Active || target == null || clock < nextDamage)
            return false;
        nextDamage = clock + tuning.helperDamageGate;
        if (!target.TakeDamage(tuning.helperDamage))
            return false;
        Hits++;
        return true;
    }
    private void LateUpdate()
    {
        if (art == null || !Joined || CurrentState == State.Stopped)
            return;
        if (surfaceEyes != null)
        {
            surfaceEyes.enabled = CurrentState == State.Warning || CurrentState == State.Swim;
            surfaceEyes.transform.position = new Vector3(body.position.x, combat.WaterY + .015f, 0);
            surfaceEyes.flipX = target != null && target.transform.position.x > body.position.x;
            surfaceEyes.color = new Color(.85f, 1, .82f, CurrentState == State.Warning ? 1 : Mathf.Clamp01(timer / .25f) * .65f);
        }
        int frame = CurrentState == State.Dive || CurrentState == State.Warning ? 1 : CurrentState == State.Surface ? 2 : CurrentState == State.Landing ? 3 : CurrentState == State.Spit || CurrentState == State.Ready ? 4 : Mathf.Repeat(clock, 3.7f) < .13f ? 5 : 0;
        visual.sprite = art.frames[frame];
        visual.flipX = (CurrentState == State.Spit || CurrentState == State.Ready) && lockedAim.x > body.position.x;
        float breath = CurrentState == State.Rest ? Mathf.Sin(clock * 2.4f) * .018f : 0;
        transform.localScale = new Vector3(1 - breath * .5f, 1 + breath, 1) * WorldScale;
        if (waterProperties != null)
        {
            waterProperties.SetFloat(LakeShaderIds.WaterY, combat.WaterY);
            waterProperties.SetFloat(LakeShaderIds.Clock, clock);
            waterProperties.SetFloat(LakeShaderIds.Reflection, 0);
            visual.SetPropertyBlock(waterProperties);
            if (surfaceEyes != null)
                surfaceEyes.SetPropertyBlock(waterProperties);
            reflection.enabled = visual.enabled;
            reflection.sprite = visual.sprite;
            reflection.flipX = visual.flipX;
            reflection.transform.localScale = new Vector3(1, -.42f, 1);
            reflection.transform.position = new Vector3(transform.position.x, combat.WaterY - (transform.position.y - combat.WaterY) * .42f, 0);
            reflectionProperties.SetFloat(LakeShaderIds.WaterY, combat.WaterY);
            reflectionProperties.SetFloat(LakeShaderIds.Clock, clock);
            reflectionProperties.SetFloat(LakeShaderIds.Reflection, 1);
            reflection.SetPropertyBlock(reflectionProperties);
        }
    }
    public void Stop()
    {
        if (CurrentState == State.Stopped)
            return;
        SetState(State.Stopped);
        if (visual != null)
            visual.enabled = false;
        if (reflection != null)
            reflection.enabled = false;
        if (surfaceEyes != null)
            surfaceEyes.enabled = false;
        LakeFxPool.ReleaseDrops(this);
        if (threats != null)
        { threats.gameObject.SetActive(false); Destroy(threats.gameObject); threats = null; }
        if (hud != null)
            hud.HideCompanion();
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
    private void OnDisable() { Stop(); }
}
