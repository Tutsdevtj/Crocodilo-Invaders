using UnityEngine;

public sealed class LakePiranha : MonoBehaviour
{
    public enum State { Hidden, Gathering, Leaping, Returning }
    private LakeWildlife owner;
    private LakeWildlifeAssets art;
    private Rigidbody2D body;
    private SpriteRenderer drawing;
    private Vector2 origin, velocity;
    private float age;
    private int slot;
    private bool spent;
    private readonly Collider2D[] contacts=new Collider2D[8];
    private ContactFilter2D filter;
    public State CurrentState { get; private set; }
    public Vector2 Position => body.position;
    public Vector2 Velocity => velocity+Vector2.down*14*age;
    public bool Active => CurrentState!=State.Hidden;
    public void Configure(LakeWildlife source,LakeWildlifeAssets assets,int index)
    {
        owner=source;art=assets;slot=index;body=GetComponent<Rigidbody2D>();drawing=GetComponent<SpriteRenderer>();
        body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.interpolation=RigidbodyInterpolation2D.None;
        filter=LakeHit.PlayerFilter();Park();
    }
    public void Gather(Vector2 point)
    {
        CurrentState=State.Gathering;age=0;spent=false;body.interpolation=RigidbodyInterpolation2D.None;
        body.position=point;drawing.enabled=true;drawing.sortingOrder=-182;drawing.color=new Color(.40f,.74f,.73f,.72f);
        transform.rotation=Quaternion.identity;
    }
    public void Launch(Vector2 aim)
    {
        origin=body.position;age=0;spent=false;
        const float arrival=.30f;
        velocity=(aim-origin)/arrival+Vector2.up*(7*arrival);
        velocity.x=Mathf.Clamp(velocity.x,-7,7);velocity.y=Mathf.Clamp(velocity.y,3.5f,8.5f);
        body.interpolation=RigidbodyInterpolation2D.Interpolate;CurrentState=State.Leaping;
    }
    public void Retreat() { spent=true;if(CurrentState==State.Gathering)Park();else if(Active)CurrentState=State.Returning; }
    public void Park() { CurrentState=State.Hidden;if(drawing!=null)drawing.enabled=false;if(body!=null)body.linearVelocity=Vector2.zero; }
    public void Tick(float dt)
    {
        if(!Active)return;
        age+=dt;
        if(CurrentState==State.Gathering)
        {
            Vector2 desired=owner.GatherPoint(slot);
            body.MovePosition(Vector2.Lerp(body.position,desired,1-Mathf.Exp(-dt*9)));
            drawing.sprite=art.piranhaFrames[(int)(owner.Clock*10+slot)%3];drawing.flipX=owner.PlayerPosition.x<body.position.x;
            return;
        }
        Vector2 previous=body.position,oldVelocity=velocity+Vector2.down*14*(age-dt);
        Vector2 point=origin+velocity*age+Vector2.down*(7*age*age);
        if(!spent && owner.CanHurt && LakeHit.Touch(owner.PlayerHealth,previous+oldVelocity.normalized*.32f,point+Velocity.normalized*.32f,.16f,filter,contacts))
        {
            spent=true;owner.Bite();if(CurrentState==State.Hidden)return;
        }
        body.MovePosition(point);
        if(previous.y<owner.WaterY && point.y>=owner.WaterY)owner.Splash(point.x,.5f);
        if(previous.y>owner.WaterY && point.y<=owner.WaterY && Velocity.y<0)owner.Splash(point.x,.38f);
        bool right=Velocity.x>=0;drawing.flipX=!right;
        transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(Velocity.y,Velocity.x)*Mathf.Rad2Deg-(right?0:180));
        int frame=(int)(age*15+slot)%3;drawing.sprite=art.piranhaFrames[(CurrentState==State.Leaping && Velocity.y>0?3:0)+frame];
        float air=Mathf.InverseLerp(owner.WaterY-.35f,owner.WaterY+.12f,point.y);
        drawing.color=Color.Lerp(new Color(.42f,.76f,.75f,.78f),Color.white,air);drawing.sortingOrder=point.y<owner.WaterY-.05f?-182:32;
        if(age>1.4f || (age>.45f && Velocity.y<0 && point.y<Mathf.Min(owner.WaterY-.35f,origin.y-.15f)) || point.y<owner.Arena.yMin-.5f)Park();
    }
}
