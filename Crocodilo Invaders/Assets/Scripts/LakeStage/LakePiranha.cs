using UnityEngine;

public sealed class LakePiranha : MonoBehaviour
{
    public enum State { Hidden, Gathering, Leaping, Returning, Swimming }
    private LakeWildlife owner;
    private LakeWildlifeAssets art;
    private Rigidbody2D body;
    private SpriteRenderer drawing;
    private Vector2 origin,velocity;
    private float age,direction,cruiseY,cooldown,visible;
    private int slot;
    private bool spent,ambient;
    private readonly Collider2D[] contacts=new Collider2D[8];
    private ContactFilter2D filter;
    public State CurrentState { get; private set; }
    public Vector2 Position => body.position;
    public Vector2 Velocity => CurrentState==State.Leaping || CurrentState==State.Returning?velocity+Vector2.down*14*age:velocity;
    public bool Active => CurrentState!=State.Hidden;
    public bool Available => CurrentState==State.Swimming && owner.Clock>=cooldown;
    public SpriteRenderer Drawing => drawing;
    public void Configure(LakeWildlife source,LakeWildlifeAssets assets,int index)
    {
        owner=source;art=assets;slot=index;body=GetComponent<Rigidbody2D>();drawing=GetComponent<SpriteRenderer>();
        body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.interpolation=RigidbodyInterpolation2D.Interpolate;
        direction=index%2==0?1:-1;cruiseY=source.WaterY-.42f-(index%4)*.27f;
        Vector2 start=new Vector2(source.Arena.xMin+source.Arena.width*Mathf.Repeat(.12f+index*.381966f,1),cruiseY);
        transform.position=start;body.position=start;filter=LakeHit.PlayerFilter();CurrentState=State.Swimming;drawing.sortingOrder=-182;
    }
    public void Gather(Vector2 point)
    {if(!Available)return;CurrentState=State.Gathering;age=0;spent=false;ambient=false;}
    public void Launch(Vector2 aim,bool decoration=false)
    {
        origin=body.position;age=0;spent=false;ambient=decoration;
        float peak=Mathf.Max(owner.WaterY+1.65f+(slot%3)*.18f,aim.y+.5f);
        peak=Mathf.Min(peak,owner.Arena.yMax-.6f);
        float up=Mathf.Sqrt(28*Mathf.Max(.1f,peak-origin.y));
        float arrival=(up+Mathf.Sqrt(Mathf.Max(0,up*up-28*(aim.y-origin.y))))/14;
        velocity=new Vector2(Mathf.Clamp((aim.x-origin.x)/Mathf.Max(.2f,arrival),-7,7),up);
        CurrentState=State.Leaping;
    }
    public void Retreat(){spent=true;if(CurrentState==State.Gathering)Swim();else if(CurrentState==State.Leaping)CurrentState=State.Returning;}
    private void Swim(){CurrentState=State.Swimming;age=0;cooldown=owner.Clock+1.2f;transform.rotation=Quaternion.identity;}
    public void Park(){CurrentState=State.Hidden;if(drawing!=null)drawing.enabled=false;}
    public void Tick(float dt)
    {
        if(!Active)return;age+=dt;visible=Mathf.MoveTowards(visible,1,dt*.8f);drawing.enabled=true;
        Vector2 previous=body.position,point=previous;
        if(CurrentState==State.Swimming || CurrentState==State.Gathering)
        {
            bool hunting=owner.WaterDangerActive && owner.CanHurt;
            Vector2 desired=hunting?owner.PlayerPosition+new Vector2((slot%2==0?-1:1)*(.45f+slot%3*.22f),-.12f):
                new Vector2(previous.x+direction,cruiseY+Mathf.Sin(owner.Clock*1.4f+slot)*.09f);
            desired.y=Mathf.Clamp(desired.y,owner.Arena.yMin+.3f,owner.WaterY-.18f);
            if(!hunting){if(previous.x>owner.Arena.xMax-.35f)direction=-1;else if(previous.x<owner.Arena.xMin+.35f)direction=1;}
            if(!hunting)desired.x=previous.x+direction;
            float speed=hunting?2.5f:1.05f+(slot%3)*.15f;
            velocity=(desired-previous).normalized*speed;point=Vector2.MoveTowards(previous,desired,speed*dt);
            drawing.flipX=velocity.x<0;transform.rotation=Quaternion.Euler(0,0,Mathf.Clamp(velocity.y*7,-12,12)*(drawing.flipX?-1:1));
            drawing.sprite=art.piranhaFrames[(hunting?3:0)+(int)(owner.Clock*11+slot)%3];
            drawing.color=new Color(hunting?.7f:.48f,hunting?.92f:.8f,.78f,visible*.88f);drawing.sortingOrder=-182;
            if(hunting && owner.PlayerPosition.y<owner.WaterY-.15f && !spent && LakeHit.Touch(owner.PlayerHealth,previous,point,.27f,filter,contacts))
            {spent=true;owner.Bite();cooldown=owner.Clock+.8f;}
            if(owner.Clock>=cooldown)spent=false;
            body.MovePosition(point);return;
        }
        point=origin+velocity*age+Vector2.down*(7*age*age);
        if(!spent && !ambient && owner.CanHurt && LakeHit.Touch(owner.PlayerHealth,previous,point,.22f,filter,contacts))
        {spent=true;owner.Bite();if(!owner.CanHurt){Retreat();return;}}
        body.MovePosition(point);
        if(previous.y<owner.WaterY && point.y>=owner.WaterY)owner.Splash(point.x,.45f);
        if(previous.y>owner.WaterY && point.y<=owner.WaterY && Velocity.y<0)owner.Splash(point.x,.35f);
        bool right=Velocity.x>=0;drawing.flipX=!right;
        transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(Velocity.y,Velocity.x)*Mathf.Rad2Deg-(right?0:180));
        drawing.sprite=art.piranhaFrames[(Velocity.y>0?3:0)+(int)(age*15+slot)%3];
        float air=Mathf.InverseLerp(owner.WaterY-.35f,owner.WaterY+.12f,point.y);
        drawing.color=Color.Lerp(new Color(.48f,.8f,.78f,.88f),Color.white,air);drawing.sortingOrder=point.y<owner.WaterY-.05f?-182:32;
        if(age>.5f && Velocity.y<0 && point.y<owner.WaterY-.28f){direction=right?1:-1;Swim();}
    }
}
