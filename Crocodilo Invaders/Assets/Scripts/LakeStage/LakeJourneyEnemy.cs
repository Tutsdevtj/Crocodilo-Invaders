using UnityEngine;

[RequireComponent(typeof(Enemy))]
public sealed class LakeJourneyEnemy : EnemyAttack
{
    public enum Species { Mosquito, Dragonfly, Frog, Beetle }
    public enum Motion { Arriving, Patrol, Warning, Strike, Exposed, Recover, Stopped, Queued, WaitingLily, BankJump }
    [SerializeField] private Species species;
    [SerializeField] private LakeJourneyArt art;
    [SerializeField] private SpriteRenderer visual;
    private Rigidbody2D body;
    private LakeJourneyDirector director;
    private Vector2 berth, from, aim, destination;
    private float clock, timer, nextAttack, flash;
    private int slot;
    private bool fired, contactSpent, slapped;
    private Vector2 pursuitVelocity;
    public bool IsEnteringBank => UsesLily && (CurrentMotion == Motion.Arriving || CurrentMotion == Motion.WaitingLily || CurrentMotion == Motion.BankJump);
    private LakeEnemyLily lily;
    private bool facingRight;
    private float horizontalSpeed;
    public LakeEnemyLily Lily => lily;
    public bool UsesLily => species == Species.Frog || species == Species.Beetle;
    private float FootOffset => -art.Frame((int)species, 0).bounds.min.y;
    public Vector2 Feet => body.position + Vector2.down * FootOffset;
    private Vector2 LilyRest => lily.Seat + Vector2.up * FootOffset;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    public Species Kind => species;
    public Motion CurrentMotion { get; private set; } = Motion.Arriving;
    public Vector2 LockedAim => aim;
    public int Attacks { get; private set; }
    public int Shots { get; private set; }
    public float Clock => clock;
    public SpriteRenderer Visual => visual;
    public bool Vulnerable => species == Species.Beetle && CurrentMotion == Motion.Exposed;
    protected override void Awake() { base.Awake(); body = GetComponent<Rigidbody2D>(); filter = LakeHit.PlayerFilter(); }
    public void Join(LakeJourneyDirector owner, int number)
    {
        director = owner;
        slot = number;
        Rect a = owner.Arena;
        float lane = Mathf.Repeat(number * .381966f, 1);
        float x = a.xMin + a.width * (.63f + (number % 3) * .12f);
        float y = Mathf.Lerp(owner.WaterY + 1.65f, a.yMax - .85f, lane);
        berth = new Vector2(x, species == Species.Frog ? owner.WaterY + .15f : y);
        Enemy.Movement.CompleteScriptedArrival(body.position);
        Enemy.Movement.enabled = false;
        nextAttack = 1.3f + (number % 3) * .6f;
        Enemy.Health.Damaged += Damaged;
        if (UsesLily)
        {
            float anchor = owner.ReserveLily(this);
            var leaves = owner.GetComponent<LakeLilyPads>();
            lily = LakeEnemyLily.Create(owner, leaves.CreatureLeaf(number), body.position.x, anchor, species == Species.Beetle ? 1.85f : 1.65f, number);
            berth = LilyRest;
            visual.enabled = false;
            OwnerCollider.enabled = false;
            Enemy.Health.SetInvulnerable(this, true);
        }
    }
    private void Damaged()
    {
        flash = .12f;
        if (Enemy.Health.IsDead && !slapped && (species == Species.Mosquito || species == Species.Dragonfly))
        {
            slapped = true;
            LakeMosquitoSlap.Create(director, visual, species == Species.Dragonfly);
            visual.enabled = false;
        }
    }
    private void SetMotion(Motion motion) { CurrentMotion = motion; timer = 0; }
    private void FixedUpdate()
    {
        if (director == null)
            return;
        if (!director.CombatActive || Enemy.Health.IsDead)
        { Stop(); return; }
        float dt = Time.fixedDeltaTime;
        clock += dt;
        timer += dt;
        if (UsesLily && lily != null)
            berth = LilyRest;
        Vector2 previous = body.position, point = previous;
        if (CurrentMotion == Motion.Arriving)
        {
            if (UsesLily)
            {
                if (lily.Arrived)
                    SetMotion(Motion.WaitingLily);
            }
            else
            {
                point = Vector2.MoveTowards(previous, berth, 3.2f * dt);
                if (Vector2.Distance(point, berth) < .03f)
                    SetMotion(Motion.Patrol);
            }
        }
        else if (species == Species.Mosquito)
        {
            Vector2 player = director.TargetBody.position;
            player.y = Mathf.Clamp(player.y, director.WaterY + .85f, director.Arena.yMax - .55f);
            Vector2 desired = (player - previous).normalized * 1.4f;
            desired.y = Mathf.Clamp(desired.y, -.8f, .8f);
            pursuitVelocity = Vector2.Lerp(pursuitVelocity, desired, 1 - Mathf.Exp(-dt * 2.6f));
            point = previous + pursuitVelocity * dt;
            point.x = Mathf.Clamp(point.x, director.Arena.xMin + .25f, director.Arena.xMax - .25f);
            point.y = Mathf.Clamp(point.y, director.WaterY + .75f, director.Arena.yMax - .45f);
            if (CurrentMotion != Motion.Patrol)
                SetMotion(Motion.Patrol);
        }
        else
            switch (CurrentMotion)
            {
                case Motion.WaitingLily:
                    if (timer >= .4f)
                    {
                        float side = slot % 2 == 0 ? 1 : -1;
                        from = new Vector2(Mathf.Clamp(berth.x + side * .9f, director.Arena.xMin + .55f, director.Arena.xMax - .55f), director.WaterY + .02f);
                        body.interpolation = RigidbodyInterpolation2D.None;
                        body.position = from;
                        transform.position = from;
                        point = from;
                        visual.enabled = true;
                        visual.sortingOrder = -205;
                        SetMotion(Motion.BankJump);
                    }
                    break;
                case Motion.BankJump:
                    float entryDuration = species == Species.Frog ? .9f : 1f;
                    float entry = Mathf.Clamp01(timer / entryDuration);
                    point = Vector2.Lerp(from, berth, Mathf.SmoothStep(0, 1, entry)) + Vector2.up * (Mathf.Sin(entry * Mathf.PI) * (species == Species.Frog ? 1.65f : 1.45f));
                    if (timer >= dt * 2)
                        body.interpolation = RigidbodyInterpolation2D.Interpolate;
                    if (entry >= .3f)
                        visual.sortingOrder = 20;
                    if (entry >= 1)
                    {
                        point = berth;
                        visual.sortingOrder = 20;
                        OwnerCollider.enabled = true;
                        Enemy.Health.SetInvulnerable(this, false);
                        lily.Land();
                        LakeWaterFX.Create(new Vector2(berth.x, director.WaterY), .35f);
                        nextAttack = clock + 1.1f;
                        SetMotion(Motion.Patrol);
                    }
                    break;
                case Motion.Patrol:
                case Motion.Queued:
                    point = UsesLily ? berth : berth + new Vector2(Mathf.Sin(clock * 1.5f + slot) * .85f, Mathf.Sin(clock * 2.2f + slot) * .36f);
                    if (CurrentMotion == Motion.Patrol && clock >= nextAttack && director.Prepare(this))
                        SetMotion(Motion.Queued);
                    if (CurrentMotion == Motion.Queued && director.Strike(this))
                    { aim = director.TargetBody.position; from = point; BeginTelegraph(aim); SetMotion(Motion.Warning); }
                    break;
                case Motion.Warning:
                    point = UsesLily ? berth : from + new Vector2(Mathf.Sign(from.x - aim.x) * Mathf.SmoothStep(0, .32f, timer / .58f), Mathf.Sin(timer * 13) * .06f);
                    if (timer >= (species == Species.Dragonfly ? .58f : .7f))
                    {
                        destination = new Vector2(Mathf.Clamp(aim.x, director.Arena.xMin + .6f, director.Arena.xMax - .6f), Mathf.Clamp(aim.y, director.WaterY + .65f, director.Arena.yMax - .65f));
                        from = point;
                        if (species == Species.Beetle)
                        { destination = from + Vector2.ClampMagnitude(destination - from, 3.2f); lily.Land(); }
                        Attacks++;
                        contactSpent = false;
                        fired = false;
                        EndTelegraph();
                        SetMotion(Motion.Strike);
                    }
                    break;
                case Motion.Strike:
                    if (species == Species.Frog)
                    {
                        point = berth;
                        if (!fired)
                        { fired = true; Spit(); }
                        if (timer >= .35f)
                        { SetMotion(Motion.Recover); }
                    }
                    else
                    {
                        float duration = species == Species.Dragonfly ? .5f : .95f;
                        float t = Mathf.Clamp01(timer / duration);
                        if (species == Species.Beetle)
                        {
                            float arc = Mathf.Sin(t * Mathf.PI);
                            if (t >= .43f && !fired)
                            { fired = true; BeetleSpit(previous); }
                            point = berth + new Vector2(Mathf.Clamp(aim.x - from.x, -5.8f, 5.8f) * arc, Mathf.Clamp(aim.y - from.y + .35f, 1.6f, director.Arena.yMax - from.y - .5f) * arc);
                        }
                        else
                            point = Vector2.Lerp(from, destination, Mathf.SmoothStep(0, 1, t));
                        if (t >= 1)
                        { if (species == Species.Beetle) lily.Land(); SetMotion(species == Species.Beetle ? Motion.Exposed : Motion.Recover); }
                    }
                    break;
                case Motion.Exposed:
                    if (UsesLily)
                        point = berth;
                    if (timer >= 1.35f)
                        SetMotion(Motion.Recover);
                    break;
                case Motion.Recover:
                    point = UsesLily ? berth : Vector2.MoveTowards(previous, berth, 5.6f * dt);
                    if (timer >= (species == Species.Frog ? 1.25f : .8f) && (species == Species.Frog || Vector2.Distance(point, berth) < .05f))
                    { director.Release(this); nextAttack = clock + (director.Section == 2 ? 1.65f : 2.4f); SetMotion(Motion.Patrol); }
                    break;
            }
        if (!contactSpent && CurrentMotion != Motion.Arriving && !IsEnteringBank && LakeHit.Touch(Target, previous, point, species == Species.Mosquito ? .22f : .35f, filter, contacts))
        { director.Hit(); contactSpent = true; if (!director.CombatActive) return; }
        if (CurrentMotion == Motion.Patrol)
            contactSpent = false;
        if (species == Species.Mosquito || species == Species.Dragonfly)
        {
            float dx = point.x - previous.x;
            horizontalSpeed = Mathf.Lerp(horizontalSpeed, dx / dt, 1 - Mathf.Exp(-dt * 10));
            if (Mathf.Abs(horizontalSpeed) > .2f)
                facingRight = horizontalSpeed > 0;
        }
        body.MovePosition(point);
        if (species == Species.Beetle)
            Enemy.Health.IncomingDamageMultiplier = Vulnerable ? 1.8f : Target.transform.position.x > body.position.x + .25f ? 1 : .55f;
    }
    private void BeetleSpit(Vector2 start)
    {
        const float travel = .85f;
        Vector2 velocity = LakeDroplet.LaunchThrough(start, aim, travel);
        for (int i = 0; i < 2; i++)
        { LakeDroplet.Fire(director, director.DropSprite, start, velocity + Vector2.up * (i == 0 ? -.45f : .45f), LakeDropStyle.Journey); Shots++; }
        NotifyShotFired();
    }
    private void Spit()
    {
        Vector2 start = body.position + new Vector2(-.4f, .62f);
        const float flight = .95f;
        Vector2 velocity = LakeDroplet.LaunchThrough(start, aim, flight);
        int count = director.Section == 2 ? 3 : 2;
        for (int i = 0; i < count; i++)
        { float spread = (i - (count - 1) * .5f) * .65f; LakeDroplet.Fire(director, director.DropSprite, start, velocity + Vector2.up * spread, LakeDropStyle.Journey); Shots++; }
        NotifyShotFired();
    }
    private void LateUpdate()
    {
        if (visual == null || director == null || Time.timeScale <= 0)
            return;
        flash = Mathf.Max(0, flash - Time.deltaTime);
        if (Enemy.Health.IsDead)
        { visual.color = new Color(.35f, .4f, .33f, Mathf.Max(0, visual.color.a - Time.deltaTime * 3)); return; }
        int pose;
        if (species == Species.Mosquito)
            pose = (int)(clock * 14 + slot * .7f) % 4;
        else if (species == Species.Dragonfly)
            pose = (int)(clock * 12 + slot * .7f) % 4;
        else if (species == Species.Frog)
            pose = CurrentMotion == Motion.Warning ? 1 : CurrentMotion == Motion.Strike ? 2 : Mathf.Repeat(clock, 3.8f) < .15f ? 3 : 0;
        else
            pose = CurrentMotion == Motion.Warning || CurrentMotion == Motion.Strike || CurrentMotion == Motion.BankJump ? 1 : Vulnerable ? 2 : CurrentMotion == Motion.Recover ? 3 : 0;
        visual.sprite = art.Frame((int)species, pose);
        if (species == Species.Mosquito || species == Species.Dragonfly)
        {
            // Keep both directions registered to the same approved drawings.
            visual.flipX = facingRight;
            if (species == Species.Mosquito && art.mosquitoRegistration != null && art.mosquitoRegistration.Length == 4)
            {
                Vector2 offset = art.mosquitoRegistration[pose];
                visual.transform.localPosition = new Vector3(facingRight ? -offset.x : offset.x, offset.y, 0);
            }
            visual.transform.localScale = Vector3.one;
            float bank = Mathf.Clamp(-horizontalSpeed * .5f, -3, 3);
            visual.transform.localRotation = Quaternion.Euler(0, 0, Mathf.MoveTowardsAngle(visual.transform.localEulerAngles.z, bank, Time.deltaTime * 30));
        }
        else if (species == Species.Beetle || (species == Species.Frog && CurrentMotion == Motion.BankJump))
        {
            float crouch = CurrentMotion == Motion.Warning ? Mathf.SmoothStep(0, .14f, timer / .7f) : 0;
            visual.transform.localScale = new Vector3(1 + crouch, 1 - crouch, 1);
        }
        Color warningColor = species == Species.Dragonfly ? new Color(.45f, .9f, 1) : species == Species.Beetle ? new Color(1, .7f, .3f) : new Color(.7f, 1, .5f);
        visual.color = flash > 0 ? new Color(1, .55f, .42f) : Vulnerable ? new Color(.8f, 1, .65f) : CurrentMotion == Motion.Warning ? Color.Lerp(Color.white, warningColor, .28f) : Color.white;
    }
    private void Stop()
    {
        if (CurrentMotion == Motion.Stopped)
            return;
        SetMotion(Motion.Stopped);
        EndTelegraph();
        if (Enemy != null && Enemy.Health != null)
            Enemy.Health.SetInvulnerable(this, false);
        if (lily != null)
            lily.Release();
        if (director != null)
            director.ReleaseLily(this);
        if (director != null)
            director.Release(this);
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
    protected override void OnDisable() { base.OnDisable(); Stop(); if (Enemy != null && Enemy.Health != null) Enemy.Health.Damaged -= Damaged; }
}
