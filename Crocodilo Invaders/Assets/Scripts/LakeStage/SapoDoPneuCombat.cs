using UnityEngine;

[RequireComponent(typeof(Boss), typeof(Rigidbody2D))]
public sealed class SapoDoPneuCombat : MonoBehaviour, IBossCombatController
{
    public enum State { Entering, Float, Prepare, Rise, Fall, Impact, Recover, Stopped, Tongue, Rage, Burp }
    public const float BurpDuration=1.4f;
    private Sprite restingSprite,burpSprite;
    private float normalDamageMultiplier;
    public bool IsVulnerable => state==State.Burp && Active;
    public int Burps { get; private set; }
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite dropletSprite;
    [SerializeField] private TMPro.TMP_FontAsset hudFont;
    public TMPro.TMP_FontAsset HudFont => hudFont;
    [SerializeField, Min(.6f)] private float jumpDuration = 1.45f;
    [SerializeField, Min(.5f)] private float warningDuration = .9f;
    [SerializeField, Min(1)] private float restDuration = 2.4f;
    private LakeTongueAttack tongue;
    private int attackCycle;
    private SapererecaCompanion companion;
    private bool rageTriggered;
    public SapererecaCompanion Companion => companion;
    public Sprite DropletSprite => dropletSprite;
    public bool HasWaterDrops => highWaveDelay>0 || (effects!=null && effects.GetComponentInChildren<LakeDroplet>()!=null);
    private float highWaveDelay;
    public int LowWaves { get; private set; }
    public int HighWaves { get; private set; }
    public LakeTongueAttack Tongue => tongue;
    private Boss boss;
    private Rigidbody2D body;
    private Health target;
    private LakeBackdrop backdrop;
    private Camera arenaCamera;
    private Transform effects;
    private LakeWaterFX cue;
    private State state = State.Entering;
    private Vector2 from, landing;
    private Vector3 visualScale;
    private float timer, jumpHeight, hitFlash, contactCooldown;
    private bool stopped, tookMovement;
    private int sequence;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    public State CurrentState => state;
    public Vector2 LockedLanding => landing;
    public int Landings { get; private set; }
    public int DropsLaunched { get; private set; }
    public int DropsReturned { get; private set; }
    public bool Active => !stopped && boss != null && boss.BattleActive && !boss.Health.IsDead && target != null && !target.IsDead;
    public bool Enraged => rageTriggered;
    public Rect Arena
    {
        get
        {
            var camera = arenaCamera != null ? arenaCamera : Camera.main;
            if (camera == null) return new Rect(-9, -3, 18, 10);
            Vector2 lo = camera.ViewportToWorldPoint(Vector3.zero), hi = camera.ViewportToWorldPoint(Vector3.one);
            return Rect.MinMaxRect(lo.x, lo.y, hi.x, hi.y);
        }
    }
    public float WaterY => backdrop != null ? backdrop.WaterY : Arena.yMin + Arena.height * .28f;
    public const float FloatHeight = .43f;
    private float RestY => WaterY + FloatHeight;
    private void Awake()
    {
        boss = GetComponent<Boss>(); body = GetComponent<Rigidbody2D>(); tongue=GetComponent<LakeTongueAttack>();
        backdrop = FindAnyObjectByType<LakeBackdrop>(); arenaCamera = Camera.main;
        visualScale = visual != null ? visual.transform.localScale : Vector3.one;
        restingSprite=visual!=null?visual.sprite:null;burpSprite=Resources.Load<Sprite>("SapeneuBurpV13/burp");
        filter = LakeHit.PlayerFilter();
    }
    private void OnEnable() { GetComponent<Health>().Damaged += Flash; }
    private void OnDisable() { GetComponent<Health>().Damaged -= Flash; StopCombat(); }
    private void Flash() { hitFlash = .12f; }
    public void SetTarget(Health player)
    {
        target = player; stopped = false;
        if (effects == null) { effects = new GameObject("SapoWaterThreats").transform; effects.SetParent(transform.parent, false); }
        if(tongue!=null)tongue.PrepareField(player,effects,Arena);
        if(companion==null)
        {
            var node=new GameObject("Saperereca, a mulher do sapo",typeof(Rigidbody2D),typeof(SpriteRenderer));
            node.transform.SetParent(transform.parent,false);companion=node.AddComponent<SapererecaCompanion>();companion.Configure(this,player);
        }
    }
    private void FixedUpdate()
    {
        if (!Active) { if (!stopped && state != State.Entering) StopCombat(); return; }
        if (!boss.Ready) return;
        if (!tookMovement)
        {
            boss.Movement.enabled = false; body.linearVelocity = Vector2.zero;
            tookMovement = true; body.position = new Vector2(Mathf.Clamp(body.position.x, Arena.center.x + 1, Arena.xMax - 1.7f), RestY);
            SetState(State.Float);
        }
        float dt = Time.fixedDeltaTime; timer += dt; contactCooldown -= dt;
        if(highWaveDelay>0 && (highWaveDelay-=dt)<=0)EmitHighWave();
        if(!rageTriggered && boss.Health.Fraction<=.5f && state!=State.Burp)BeginRage();
        if(state==State.Float && tongue!=null && tongue.MealPending && !HasWaterDrops && (companion==null || !companion.Busy))BeginBurp();
        switch (state)
        {
            case State.Burp:
                body.MovePosition(new Vector2(body.position.x,RestY+Mathf.Sin(timer*5)*.035f));
                if(timer>=BurpDuration){EndBurp();SetState(State.Float);}
                break;
            case State.Rage:
                body.MovePosition(new Vector2(body.position.x,RestY+Mathf.Sin(timer*28)*.035f));
                if(timer>=SapererecaCompanion.EntranceDuration+.35f)SetState(State.Float);
                break;
            case State.Float:
                float clock = backdrop != null ? backdrop.MotionClock : Time.time;
                body.MovePosition(new Vector2(body.position.x, RestY + Mathf.Sin(clock * 1.7f) * .065f + Mathf.Sin(clock * 3.1f) * .018f));
                if ((companion==null || !companion.Busy) && timer >= (attackCycle==0?1f:(Enraged ? restDuration * .72f : restDuration)))
                {
                    if(attackCycle%2==1 || tongue==null || !tongue.ArtReady || tongue.AliveFlyCount==0)BeginJump();
                    else TryBeginTongue(Enraged?4:3);
                }
                break;
            case State.Tongue:
                body.linearVelocity=Vector2.zero;
                if(tongue.Step(dt) && Active)
                {
                    if(tongue.MealPending && (companion==null || !companion.Busy))BeginBurp();
                    else {SetState(State.Float);timer=Mathf.Max(0,(Enraged?restDuration*.72f:restDuration)-.8f);}
                }
                break;
            case State.Prepare:
                if (timer >= warningDuration)
                {
                    from = body.position;
                    LakeWaterFX.Create(effects, new Vector2(from.x, WaterY), .85f);
                    SetState(State.Rise);
                }
                break;
            case State.Rise:
            case State.Fall:
                float t = Mathf.Clamp01(timer / jumpDuration);
                Vector2 point = Vector2.Lerp(from, landing, Mathf.SmoothStep(0, 1, t));
                point.y = RestY + Mathf.Sin(t * Mathf.PI) * jumpHeight;
                if (t >= .5f) state = State.Fall;
                if (contactCooldown <= 0 && LakeHit.Touch(target, body.position, point, .7f, filter, contacts))
                { target.TakeDamage(2); contactCooldown = .8f;if(!Active)return; }
                body.MovePosition(point);
                if (t >= 1) Land();
                break;
            case State.Impact:
                if (timer >= .25f) SetState(State.Recover);
                break;
            case State.Recover:
                if (timer >= .7f) { SetState(State.Float);timer=Mathf.Max(0,(Enraged?restDuration*.72f:restDuration)-1f); }
                break;
        }
    }
    public bool TryBeginTongue(int flies)
    {
        if(!Active || !boss.Ready || state!=State.Float || tongue==null || effects==null || HasWaterDrops || (companion!=null && companion.Busy))return false;
        if(!tongue.Begin(flies,target,effects,Arena))return false;
        attackCycle++;SetState(State.Tongue);
        if(Enraged && companion!=null)companion.PrepareTongueCombo();
        return true;
    }
    private void BeginJump()
    {
        attackCycle++;
        var area = Arena;
        // Snapshot before the warning: moving away during the cue always changes the outcome.
        Vector2 aim=target.GetComponent<Rigidbody2D>().position;
        landing = new Vector2(Mathf.Clamp(aim.x, area.xMin + 1.7f, area.xMax - 1.7f), RestY);
        jumpHeight = Mathf.Clamp(aim.y-RestY+.65f,2.1f,Mathf.Max(2.1f,area.yMax-RestY-1.3f));
        cue = LakeWaterFX.Create(effects, new Vector2(landing.x, WaterY), 2.35f, true);
        SetState(State.Prepare);
    }
    private void BeginRage()
    {
        rageTriggered=true;
        highWaveDelay=0;
        if(cue!=null){Destroy(cue.gameObject);cue=null;}
        if(tongue!=null)tongue.CancelSequence();
        if(effects!=null)foreach(var drop in effects.GetComponentsInChildren<LakeDroplet>()){drop.gameObject.SetActive(false);Destroy(drop.gameObject);}
        body.position=new Vector2(body.position.x,RestY);body.linearVelocity=Vector2.zero;
        SetState(State.Rage);if(companion!=null)companion.Enter();
        LakeWaterFX.Create(effects,new Vector2(body.position.x,WaterY),1.4f);
    }
    private void BeginBurp()
    {
        if(IsVulnerable)return;
        Burps++;normalDamageMultiplier=boss.Health.IncomingDamageMultiplier;boss.Health.IncomingDamageMultiplier=normalDamageMultiplier*2;
        SetState(State.Burp);body.linearVelocity=Vector2.zero;
        if(visual!=null && burpSprite!=null)visual.sprite=burpSprite;
    }
    private void EndBurp()
    {
        if(state!=State.Burp)return;
        boss.Health.IncomingDamageMultiplier=normalDamageMultiplier;
        if(visual!=null && restingSprite!=null)visual.sprite=restingSprite;
        if(tongue!=null)tongue.FinishMeal();
    }
    private void Land()
    {
        body.position = landing; Landings++; sequence++;
        if (cue != null) Destroy(cue.gameObject);
        LakeWaterFX.Create(effects, new Vector2(landing.x, WaterY), 2.1f);
        EmitDrops(); SetState(State.Impact);
    }
    private void EmitDrops()
    {
        int count = Enraged ? 4 : 3;LowWaves++;
        // Fast, low crossing arcs first. The second, higher wave arrives after a clear gap.
        for (int i = 0; i < count; i++)
        {
            float direction = i % 2 == 0 ? -1 : 1;
            float speedX = direction * (4.7f + i * .55f);
            float speedY = 3.8f + (i % 2) * .35f;
            Vector2 start = new Vector2(landing.x + direction * (.8f + i * .09f), WaterY + .04f);
            SpawnDrop(start, new Vector2(speedX, speedY));
        }
        highWaveDelay=.32f;
    }
    private void EmitHighWave()
    {
        HighWaves++;int count=Enraged?3:2;
        LakeWaterFX.Create(effects,new Vector2(landing.x,WaterY),.75f);
        for(int i=0;i<count;i++)
        {
            float direction=i%2==0?-1:1;
            SpawnDrop(new Vector2(landing.x+direction*(.7f+i*.18f),WaterY+.04f),new Vector2(direction*(1.5f+i*.5f),8.7f+i*.35f));
        }
    }
    private void SpawnDrop(Vector2 start, Vector2 velocity)
    {
        var node = new GameObject("DangerousLakeDrop"); node.transform.SetParent(effects, false);
        node.AddComponent<Rigidbody2D>(); node.AddComponent<SpriteRenderer>();
        node.AddComponent<LakeDroplet>().Configure(this, target, dropletSprite, start, velocity, WaterY);
        DropsLaunched++;
    }
    public void RegisterReturnedDrop() { DropsReturned++; }
    private void SetState(State next) { state = next; timer = 0; }
    private void LateUpdate()
    {
        if (visual == null || Time.timeScale <= 0) return;
        hitFlash = Mathf.Max(0, hitFlash - Time.deltaTime);
        visual.color = hitFlash > 0 ? new Color(1, .65f, .65f) : state==State.Burp ? new Color(1,1,.82f) : state==State.Rage ? Color.Lerp(new Color(1,.5f,.32f),new Color(1,.83f,.52f),.5f+.5f*Mathf.Sin(timer*24)) : Enraged ? new Color(1, .82f, .64f) : Color.white;
        Vector2 stretch = Vector2.one;
        if(state==State.Burp)stretch=new Vector2(1.02f+Mathf.Sin(timer*9)*.018f,.99f+Mathf.Cos(timer*9)*.015f);
        else if(state==State.Rage)stretch=new Vector2(1.08f+Mathf.Sin(timer*24)*.02f,.92f);
        else if (state == State.Float)
        {
            float breath = Mathf.Sin((backdrop != null ? backdrop.MotionClock : Time.time)*2.2f);
            stretch = new Vector2(1 + breath*.012f,1 + breath*.024f);
        }
        else if (state == State.Tongue && tongue!=null && tongue.MouthOpen) stretch=new Vector2(1.025f,.98f);
        else if (state == State.Prepare) stretch = Vector2.Lerp(Vector2.one, new Vector2(1.12f, .82f), Mathf.Clamp01(timer / warningDuration));
        else if (state == State.Rise) stretch = new Vector2(.93f, 1.1f);
        else if (state == State.Fall) stretch = new Vector2(1.03f, 1.03f);
        else if (state == State.Impact) stretch = new Vector2(1.16f, .78f);
        else if (state == State.Recover) stretch = Vector2.Lerp(new Vector2(1.16f, .78f), Vector2.one, Mathf.Clamp01(timer / .7f));
        visual.transform.localScale = Vector3.Scale(visualScale, new Vector3(stretch.x, stretch.y, 1));
    }
    public void StopCombat()
    {
        EndBurp();
        highWaveDelay=0;
        stopped = true; state = State.Stopped;
        if(tongue!=null)tongue.Stop();
        if(companion!=null)companion.Stop();
        if (body != null) body.linearVelocity = Vector2.zero;
        if (effects != null) { effects.gameObject.SetActive(false); Destroy(effects.gameObject); effects = null; }
        cue = null;
        if (visual != null) { visual.transform.localScale = visualScale; visual.color = Color.white; }
    }
}
