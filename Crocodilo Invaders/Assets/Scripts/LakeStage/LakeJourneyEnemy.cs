using UnityEngine;

[RequireComponent(typeof(Enemy))]
public sealed class LakeJourneyEnemy : EnemyAttack
{
    public enum Species { Mosquito, Dragonfly, Frog, Beetle }
    public enum Motion { Arriving, Patrol, Warning, Strike, Exposed, Recover, Stopped }
    [SerializeField] private Species species;
    [SerializeField] private LakeJourneyArt art;
    [SerializeField] private SpriteRenderer visual;
    private Rigidbody2D body;
    private LakeJourneyDirector director;
    private Vector2 berth,from,aim,destination;
    private float clock,timer,nextAttack,flash,patrolAt;
    private int slot;
    private bool fired,contactSpent;
    private LakeEnemyLily lily;
    public LakeEnemyLily Lily => lily;
    public bool UsesLily => species==Species.Frog || species==Species.Beetle;
    private float FootOffset => -art.Frame((int)species,0).bounds.min.y;
    public Vector2 Feet => body.position+Vector2.down*FootOffset;
    private Vector2 LilyRest => lily.Seat+Vector2.up*FootOffset;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts=new Collider2D[8];
    public Species Kind => species;
    public Motion CurrentMotion { get; private set; }=Motion.Arriving;
    public Vector2 LockedAim => aim;
    public int Attacks { get; private set; }
    public int Shots { get; private set; }
    public float Clock => clock;
    public SpriteRenderer Visual => visual;
    public bool Vulnerable => species==Species.Beetle && CurrentMotion==Motion.Exposed;
    protected override void Awake(){base.Awake();body=GetComponent<Rigidbody2D>();filter=LakeHit.PlayerFilter();}
    public void Join(LakeJourneyDirector owner,int number)
    {
        director=owner;slot=number;Rect a=owner.Arena;
        float lane=Mathf.Repeat(number*.381966f,1);
        float x=a.xMin+a.width*(.63f+(number%3)*.12f);
        float y=Mathf.Lerp(owner.WaterY+1.65f,a.yMax-.85f,lane);
        berth=new Vector2(x,species==Species.Frog?owner.WaterY+.15f:y);
        Enemy.Movement.CompleteScriptedArrival(body.position);Enemy.Movement.enabled=false;
        nextAttack=1.3f+(number%3)*.6f;Enemy.Health.Damaged+=Damaged;
        if(UsesLily)
        {
            float anchor=owner.ReserveLily(this);var leaves=owner.GetComponent<LakeLilyPads>();
            lily=LakeEnemyLily.Create(owner,leaves.CreatureLeaf(number),body.position.x,anchor,species==Species.Beetle?1.85f:1.65f,number);
            body.position=LilyRest;berth=LilyRest;
        }
    }
    private void Damaged(){flash=.12f;}
    private void SetMotion(Motion motion){CurrentMotion=motion;timer=0;if(motion==Motion.Patrol)patrolAt=clock;}
    private void FixedUpdate()
    {
        if(director==null)return;
        if(!director.CombatActive || Enemy.Health.IsDead){Stop();return;}
        float dt=Time.fixedDeltaTime;clock+=dt;timer+=dt;
        if(UsesLily && lily!=null)berth=LilyRest;
        Vector2 previous=body.position,point=previous;
        if(CurrentMotion==Motion.Arriving)
        {
            point=UsesLily?berth:Vector2.MoveTowards(previous,berth,3.2f*dt);
            if(UsesLily?lily.Arrived:Vector2.Distance(point,berth)<.03f)SetMotion(Motion.Patrol);
        }
        else if(species==Species.Mosquito)
        {
            // A wave of small bodies, alternating high/low lanes and leaving a broad central gap.
            float x=berth.x-Mathf.PingPong((clock-patrolAt)*.85f,director.Arena.width*.65f);
            point=new Vector2(x,berth.y+Mathf.Sin(clock*1.8f+slot*1.8f)*.55f);
            if(CurrentMotion!=Motion.Patrol)SetMotion(Motion.Patrol);
        }
        else switch(CurrentMotion)
        {
            case Motion.Patrol:
                point=UsesLily?berth:berth+new Vector2(Mathf.Sin(clock*1.1f+slot)*.25f,Mathf.Sin(clock*1.5f+slot)*.18f);
                if(clock>=nextAttack && director.Prepare(this))
                {aim=Target.GetComponent<Rigidbody2D>().position;from=point;BeginTelegraph(aim);SetMotion(Motion.Warning);}
                break;
            case Motion.Warning:
                point=UsesLily?berth:from;
                if(timer>=.8f && director.Strike(this))
                {
                    destination=new Vector2(Mathf.Clamp(aim.x,director.Arena.xMin+.6f,director.Arena.xMax-.6f),Mathf.Clamp(aim.y,director.WaterY+.65f,director.Arena.yMax-.65f));
                    if(UsesLily)from=point;
                    if(species==Species.Beetle){destination=from+Vector2.ClampMagnitude(destination-from,3.2f);lily.Land();}
                    Attacks++;contactSpent=false;fired=false;EndTelegraph();SetMotion(Motion.Strike);
                }
                break;
            case Motion.Strike:
                if(species==Species.Frog)
                {
                    point=berth;
                    if(!fired){fired=true;Spit();}
                    if(timer>=.35f){SetMotion(Motion.Recover);}
                }
                else
                {
                    float duration=species==Species.Dragonfly?.5f:.95f;
                    float t=Mathf.Clamp01(timer/duration);
                    if(species==Species.Beetle)
                    {
                        float arc=Mathf.Sin(t*Mathf.PI);
                        point=berth+new Vector2(Mathf.Clamp(aim.x-from.x,-2.6f,2.6f)*arc,Mathf.Clamp(aim.y-from.y+.35f,1.4f,3.4f)*arc);
                    }
                    else point=Vector2.Lerp(from,destination,Mathf.SmoothStep(0,1,t));
                    if(t>=1){if(species==Species.Beetle)lily.Land();SetMotion(species==Species.Beetle?Motion.Exposed:Motion.Recover);}
                }
                break;
            case Motion.Exposed:
                if(UsesLily)point=berth;
                if(timer>=1.35f)SetMotion(Motion.Recover);
                break;
            case Motion.Recover:
                point=UsesLily?berth:Vector2.MoveTowards(previous,berth,3.5f*dt);
                if(timer>=(species==Species.Frog?1.25f:.8f) && (species==Species.Frog || Vector2.Distance(point,berth)<.05f))
                {director.Release(this);nextAttack=clock+(director.Section==2?1.65f:2.4f);SetMotion(Motion.Patrol);}
                break;
        }
        if(!contactSpent && CurrentMotion!=Motion.Arriving && LakeHit.Touch(Target,previous,point,species==Species.Mosquito?.22f:.35f,filter,contacts))
        {director.Hit();contactSpent=true;if(!director.CombatActive)return;}
        if(CurrentMotion==Motion.Patrol)contactSpent=false;
        body.MovePosition(point);
        if(species==Species.Beetle)
            Enemy.Health.IncomingDamageMultiplier=Vulnerable?1.8f:Target.transform.position.x>body.position.x+.25f?1:.25f;
    }
    private void Spit()
    {
        Vector2 start=body.position+new Vector2(-.4f,.62f);const float flight=.95f;
        Vector2 velocity=(aim-start)/flight+Vector2.up*(.5f*LakeDroplet.Gravity*flight);
        int count=director.Section==2?3:2;
        for(int i=0;i<count;i++)
        {float spread=(i-(count-1)*.5f)*.65f;LakeJourneyDrop.Fire(director,start,velocity+Vector2.up*spread);Shots++;}
        NotifyShotFired();
    }
    private void LateUpdate()
    {
        if(visual==null || director==null || Time.timeScale<=0)return;
        flash=Mathf.Max(0,flash-Time.deltaTime);
        if(Enemy.Health.IsDead){visual.color=new Color(.35f,.4f,.33f,Mathf.Max(0,visual.color.a-Time.deltaTime*3));return;}
        int pose;
        if(species==Species.Mosquito)pose=(int)(clock*14)%4;
        else if(species==Species.Dragonfly)pose=CurrentMotion==Motion.Warning?1:CurrentMotion==Motion.Strike?2:CurrentMotion==Motion.Recover?3:(int)(clock*9)%2;
        else if(species==Species.Frog)pose=CurrentMotion==Motion.Warning?1:CurrentMotion==Motion.Strike?2:Mathf.Repeat(clock,3.8f)<.15f?3:0;
        else pose=CurrentMotion==Motion.Warning || CurrentMotion==Motion.Strike?1:Vulnerable?2:CurrentMotion==Motion.Recover?3:0;
        visual.sprite=art.Frame((int)species,pose);
        Color warningColor=species==Species.Dragonfly?new Color(.45f,.9f,1):species==Species.Beetle?new Color(1,.7f,.3f):new Color(.7f,1,.5f);
        visual.color=flash>0?new Color(1,.55f,.42f):Vulnerable?new Color(.8f,1,.65f):CurrentMotion==Motion.Warning?Color.Lerp(Color.white,warningColor,.3f+.2f*Mathf.Sin(clock*16)):Color.white;
    }
    private void Stop()
    {
        if(CurrentMotion==Motion.Stopped)return;
        SetMotion(Motion.Stopped);EndTelegraph();
        if(lily!=null)lily.Release();if(director!=null)director.ReleaseLily(this);
        if(director!=null)director.Release(this);if(body!=null)body.linearVelocity=Vector2.zero;
    }
    protected override void OnDisable(){base.OnDisable();Stop();if(Enemy!=null && Enemy.Health!=null)Enemy.Health.Damaged-=Damaged;}
}
