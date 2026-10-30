using UnityEngine;

[DefaultExecutionOrder(120)]
public sealed class LakeTongueAttack : MonoBehaviour
{
    public enum Phase { Idle, Tracking, Locked, Extend, Retract, Gap, Recover }
    public const float TrackingTime=.46f, LockTime=.16f, ExtendTime=.13f, RetractTime=.19f, HoldTime=.04f;
    [SerializeField] private LakeTongueAssets art;
    private SpriteRenderer visual;
    private LakeTongueSkin skin;
    public LakeTongueSkin Skin => skin;
    private Sprite resting;
    private Health target;
    private PlayerDash dash;
    [SerializeField] private Vector2[] stationPositions={new Vector2(.13f,.66f),new Vector2(.27f,.48f),new Vector2(.36f,.58f),new Vector2(.46f,.44f),new Vector2(.51f,.51f)};
    private readonly LakeTongueFly[] flies=new LakeTongueFly[3];
    private readonly bool[] introduced=new bool[3];
    private readonly int[] plan=new int[4];
    private Transform fieldRoot;
    private Rigidbody2D playerBody;
    private SapoDoPneuCombat combat;
    private Rect arena;
    private int sequenceLength, mapCursor,selectionCursor;
    private float nextSpawn;
    private bool rearIntroduced;
    private Vector2 campingAnchor;
    private float campingTime;
    public int CampingFlies { get; private set; }
    public bool MealPending { get; private set; }
    public int AliveFlyCount { get { int n=0;for(int i=0;i<count;i++)if(flies[i]!=null && flies[i].Present)n++;return n; } }
    public int FliesSpawned { get; private set; }
    private readonly Collider2D[] contacts=new Collider2D[8];
    private ContactFilter2D filter;
    private int count, current;
    private float timer, clock, progress;
    private bool spent, restingFlip;
    public Phase CurrentPhase { get; private set; }
    public bool Busy => CurrentPhase!=Phase.Idle;
    public bool MouthOpen => Busy && (CurrentPhase==Phase.Extend || CurrentPhase==Phase.Retract);
    public bool ArtReady => art!=null && art.openMouth!=null && art.skin!=null && art.skin.drawing!=null && art.skin.material!=null && art.flyFrames!=null && art.flyFrames.Length==6;
    public Vector2 Mouth
    {
        get
        {
            if(visual==null || resting==null)return transform.position;
            Vector2 point=(new Vector2(698,625)-resting.pivot)/resting.pixelsPerUnit;
            if(visual.flipX)point.x=-point.x;
            return visual.transform.TransformPoint(point);
        }
    }
    public Vector2 Aim { get; private set; }
    public Vector2 TipPoint => Vector2.Lerp(Mouth,Aim,progress);
    public float Clock => clock;
    public float PhaseTime => timer;
    public float Extension => progress;
    public int FlyCount => count;
    public int SequenceLength => sequenceLength;
    public LakeTongueFly TargetFly => Busy && current<sequenceLength?flies[plan[current]]:null;
    public int AgitatingCount
    {
        get { int active=0;for(int i=0;i<count;i++)if(flies[i]!=null && flies[i].IsAgitating)active++;return active; }
    }
    public int Strikes { get; private set; }
    public int Hits { get; private set; }
    public int DamageAttempts { get; private set; }
    public int CapturedFlies { get; private set; }
    public float LastStrikeTime { get; private set; }
    public float PreviousStrikeTime { get; private set; }
    public LakeTongueFly Fly(int slot) => slot>=0 && slot<count?flies[slot]:null;
    private void Awake()
    {
        if(art==null)art=Resources.Load<LakeTongueAssets>("LakeTongueV08/art");
        visual=GetComponentInChildren<SpriteRenderer>(); resting=visual!=null?visual.sprite:null;restingFlip=visual!=null && visual.flipX;
        filter=LakeHit.PlayerFilter();combat=GetComponent<SapoDoPneuCombat>();
        if(!ArtReady)return;
        skin=new GameObject("TongueSkin").AddComponent<LakeTongueSkin>();skin.transform.SetParent(transform,false);
        skin.Configure(art.skin,visual.sortingLayerID,24);EnsureField();
    }
    public void PrepareField(Health player,Transform effects,Rect bounds)
    {
        target=player;playerBody=player!=null?player.GetComponent<Rigidbody2D>():null;
        dash=player!=null?player.GetComponent<PlayerDash>():null;fieldRoot=effects;arena=bounds;
        if(count==0 && playerBody!=null)campingAnchor=playerBody.position;
        EnsureField();
    }
    private Vector2 BehindPlayer()
    {
        Vector2 direction=(playerBody.position-Mouth).normalized;if(direction.sqrMagnitude<.1f)direction=Vector2.left;
        Vector2 candidate=playerBody.position+direction*.9f;
        for(int attempt=0;attempt<3;attempt++)
        {
            Vector2 p=playerBody.position+direction*(.9f+attempt*.7f);
            candidate=new Vector2(Mathf.Clamp(p.x,arena.xMin+.25f,arena.xMax-.25f),Mathf.Clamp(p.y,arena.yMin+.3f,arena.yMax-.3f));
            bool clear=true;
            for(int i=0;i<stationPositions.Length;i++)
            {
                Vector2 station=new Vector2(arena.xMin+stationPositions[i].x*arena.width,arena.yMin+stationPositions[i].y*arena.height);
                if(Vector2.Distance(candidate,station)<.7f)clear=false;
            }
            if(clear)break;
        }
        return candidate;
    }
    private void EnsureField()
    {
        if(count>0 || !ArtReady || visual==null || fieldRoot==null || playerBody==null)return;
        count=flies.Length;
        for(int i=0;i<count;i++)
        {
            Vector2 station=Station(i);
            var node=new GameObject("RoamingLakeFly_"+i);node.transform.SetParent(fieldRoot,false);
            flies[i]=node.AddComponent<LakeTongueFly>();flies[i].Configure(art,this,station,i);
        }
        SpawnFly(0);nextSpawn=clock+1.35f;
    }
    private Vector2 Station(int slot)
    {
        Vector2 point=stationPositions.Length>0?stationPositions[slot%stationPositions.Length]:new Vector2(.3f,.65f);
        return new Vector2(arena.xMin+point.x*arena.width,arena.yMin+point.y*arena.height);
    }
    private void SpawnFly(int slot)
    {
        Vector2 point=!rearIntroduced?BehindPlayer():Station(mapCursor++);
        if(rearIntroduced && campingTime>=1.5f && mapCursor%2==0)
        {
            // Approach from either side of a camp, including high lanes; only the first fly is forced behind.
            Vector2 offset=new Vector2(mapCursor%4==0?1.1f:-1.1f,.25f);
            point=playerBody.position+offset;
            point.x=Mathf.Clamp(point.x,arena.xMin+.7f,arena.xMax-.7f);
            point.y=Mathf.Clamp(point.y,combat.WaterY+.9f,arena.yMax-.6f);CampingFlies++;
        }
        rearIntroduced=true;introduced[slot]=true;flies[slot].Place(point);FliesSpawned++;
    }
    public void FinishMeal()
    {
        MealPending=false;System.Array.Clear(introduced,0,introduced.Length);nextSpawn=clock+3.5f;
    }
    public void CancelSequence() { FinishSequence(); }
    private void FixedUpdate()
    {
        if(combat==null || !combat.Active)return;
        clock+=Time.fixedDeltaTime;
        if(playerBody!=null)
        {
            if(Vector2.Distance(playerBody.position,campingAnchor)>.85f){campingAnchor=playerBody.position;campingTime=0;}
            else campingTime+=Time.fixedDeltaTime;
        }
        for(int i=0;i<count;i++)if(flies[i]!=null)flies[i].Tick(Time.fixedDeltaTime);
        if(!MealPending && combat.CurrentState!=SapoDoPneuCombat.State.Burp && clock>=nextSpawn)
        {
            for(int i=0;i<count;i++)if(flies[i].Caught && !introduced[i])
            {
                SpawnFly(i);int spawned=0;foreach(bool value in introduced)if(value)spawned++;
                nextSpawn=clock+(spawned==1?1.35f:3.1f);break;
            }
        }
    }
    public bool Begin(int number,Health player,Transform effects,Rect bounds)
    {
        if(Busy || !ArtReady || player==null || player.IsDead)return false;
        PrepareField(player,effects,bounds);if(AliveFlyCount==0 || MealPending)return false;
        sequenceLength=Mathf.Clamp(number,1,Mathf.Min(plan.Length,AliveFlyCount));current=0;progress=0;
        for(int i=0;i<count;i++)flies[i].ReleaseFocus();
        for(int i=0;i<sequenceLength;i++)
        {
            int chosen=-1;
            for(int attempt=0;attempt<count;attempt++)
            {
                int candidate=selectionCursor++%count;bool used=false;
                for(int p=0;p<i;p++)if(plan[p]==candidate)used=true;
                if(!used && !flies[candidate].Caught) { chosen=candidate;break; }
            }
            plan[i]=chosen;
        }
        StartWarning();return true;
    }
    private void StartWarning()
    {
        for(int i=0;i<count;i++)flies[i].ReleaseFocus();
        var fly=flies[plan[current]];fly.Focused=true;Aim=fly.Anchor;Change(Phase.Tracking);
    }
    private bool AdvanceStrike()
    {
        progress=1-Mathf.Pow(1-Mathf.Clamp01(timer/ExtendTime),3);
        if(!spent && progress>0 && LakeHit.Touch(target,Mouth,TipPoint,.13f,filter,contacts))
        {
            spent=true;DamageAttempts++;if(target.TakeDamage(2))Hits++;
            if(!Busy || target==null || target.IsDead) { Stop();return true; }
        }
        var fly=TargetFly;
        if(timer>=ExtendTime && fly!=null && !fly.Caught) { fly.Catch();CapturedFlies++;if(AliveFlyCount==0)MealPending=true; }
        if(timer>=ExtendTime+HoldTime)Change(Phase.Retract);
        return false;
    }
    public bool Step(float dt)
    {
        if(!Busy)return true;
        timer+=dt;
        switch(CurrentPhase)
        {
            case Phase.Tracking:
                if(timer>=TrackingTime) { TargetFly.Lock(); Aim=TargetFly.LockedPoint; Change(Phase.Locked); }
                break;
            case Phase.Locked:
                if(timer>=LockTime)
                {
                    spent=false; Strikes++; PreviousStrikeTime=LastStrikeTime;LastStrikeTime=Time.time;Change(Phase.Extend);
                    timer=dt;if(AdvanceStrike())return true;
                }
                break;
            case Phase.Extend:
                if(AdvanceStrike())return true;
                break;
            case Phase.Retract:
                progress=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(timer/RetractTime));
                if(timer>=RetractTime)
                {
                    progress=0;
                    if(current+1<sequenceLength) { current++;Change(Phase.Gap); }
                    else Change(Phase.Recover);
                }
                break;
            case Phase.Gap:
                float gap=Mathf.Max(.12f,(dash!=null?dash.Cooldown:1.1f)+.06f-TrackingTime-LockTime-ExtendTime-HoldTime-RetractTime);
                if(timer>=gap)StartWarning();
                break;
            case Phase.Recover:
                if(timer>=.25f) { FinishSequence();return true; }
                break;
        }
        return false;
    }
    private void Change(Phase phase)
    {
        CurrentPhase=phase;timer=0;
        if(visual!=null && resting!=null) { visual.sprite=MouthOpen?art.openMouth:resting;visual.flipX=MouthOpen?Aim.x>visual.transform.position.x:restingFlip; }
    }
    private void LateUpdate()
    {
        if(skin==null || Time.timeScale<=0)return;
        bool showing=Busy && (CurrentPhase==Phase.Extend || CurrentPhase==Phase.Retract) && Vector2.Distance(Mouth,TipPoint)>.12f;
        if(showing)skin.Draw(Mouth,TipPoint,clock,CurrentPhase==Phase.Retract,visual.flipX,visual.color);
        else skin.Hide();
    }
    private void FinishSequence()
    {
        CurrentPhase=Phase.Idle;progress=0;
        if(visual!=null && resting!=null) { visual.sprite=resting;visual.flipX=restingFlip; }
        if(skin!=null)skin.Hide();
        for(int i=0;i<count;i++)if(flies[i]!=null)flies[i].ReleaseFocus();
    }
    public void Stop()
    {
        FinishSequence();
        for(int i=0;i<flies.Length;i++)if(flies[i]!=null) { flies[i].gameObject.SetActive(false);Destroy(flies[i].gameObject);flies[i]=null; }
        count=sequenceLength=0;fieldRoot=null;MealPending=false;System.Array.Clear(introduced,0,introduced.Length);
    }
    private void OnDisable() { Stop(); }
}
