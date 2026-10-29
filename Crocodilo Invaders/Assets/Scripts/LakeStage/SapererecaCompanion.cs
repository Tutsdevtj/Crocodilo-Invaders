using UnityEngine;

// Shares the encounter's health/lifetime; she is an assisting character, not a second boss.
public sealed class SapererecaCompanion : MonoBehaviour
{
    public enum State { InHud, Entering, Rest, Warning, Jump, Landing, Spit, Stopped }
    public const float EntranceDuration=1.25f,WarningDuration=.65f,HopDuration=.9f;
    private SapoDoPneuCombat combat;
    private SapererecaAssets art;
    private Health target;
    private Rigidbody2D body;
    private SpriteRenderer visual,reflection;
    private Transform threats;
    private LakeBossBar hud;
    private LakeBackdrop lake;
    private LakeWaterFX cue;
    private MaterialPropertyBlock waterProperties,reflectionProperties;
    private Vector2 from,landing,lockedAim;
    private float timer,clock,nextAttack,contactCooldown,nextDamage;
    private int berth;
    private readonly Collider2D[] contacts=new Collider2D[8];
    private ContactFilter2D filter;
    public State CurrentState { get; private set; }=State.InHud;
    public bool Busy => CurrentState==State.Entering || CurrentState==State.Warning || CurrentState==State.Jump || CurrentState==State.Landing || CurrentState==State.Spit || ActiveDrops>0;
    public bool Joined { get; private set; }
    public int Entries { get; private set; }
    public int Hops { get; private set; }
    public int Shots { get; private set; }
    public int Hits { get; private set; }
    public int ActiveDrops => threats!=null?threats.GetComponentsInChildren<LakeDroplet>().Length:0;
    public Vector2 Position => body!=null?body.position:Vector2.zero;
    public Vector2 LockedAim => lockedAim;
    public Vector2 Landing => landing;
    public SpriteRenderer Visual => visual;
    public float Clock => clock;
    public float PhaseTime => timer;
    public void Configure(SapoDoPneuCombat owner,Health player)
    {
        combat=owner;target=player;art=Resources.Load<SapererecaAssets>("SapererecaV12/art");lake=FindAnyObjectByType<LakeBackdrop>();
        body=GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.interpolation=RigidbodyInterpolation2D.Interpolate;
        visual=GetComponent<SpriteRenderer>();visual.sortingOrder=19;visual.enabled=false;
        threats=new GameObject("SapererecaWaterThreats").transform;threats.SetParent(transform,false);
        filter=LakeHit.PlayerFilter();
        var material=Resources.Load<Material>("SapererecaV12/water-contact");
        if(material!=null)
        {
            visual.sharedMaterial=material;waterProperties=new MaterialPropertyBlock();reflectionProperties=new MaterialPropertyBlock();
            reflection=new GameObject("SapererecaWaterReflection").AddComponent<SpriteRenderer>();reflection.transform.SetParent(transform,false);
            reflection.sharedMaterial=material;reflection.sortingOrder=9;reflection.enabled=false;
        }
    }
    public void Enter()
    {
        if(CurrentState!=State.InHud || art==null)return;
        Entries++;landing=new Vector2(combat.Arena.xMin+combat.Arena.width*.59f,combat.WaterY+.06f);
        body.position=landing;hud=FindAnyObjectByType<LakeBossBar>();
        if(hud!=null)hud.BeginCompanionJump(landing);SetState(State.Entering);
    }
    private void SetState(State state) { CurrentState=state;timer=0; }
    private void FixedUpdate()
    {
        if(combat==null || !combat.Active){Stop();return;}
        float dt=Time.fixedDeltaTime;clock+=dt;timer+=dt;contactCooldown-=dt;
        switch(CurrentState)
        {
            case State.Entering:
                if(timer>=EntranceDuration)
                {
                    Joined=true;visual.enabled=true;visual.sprite=art.frames[3];body.position=landing;
                    LakeWaterFX.Create(threats,new Vector2(landing.x,combat.WaterY),.9f);nextAttack=clock+1.3f;SetState(State.Landing);
                }
                break;
            case State.Rest:
                body.MovePosition(new Vector2(body.position.x,combat.WaterY+.06f+Mathf.Sin(clock*2.3f)*.025f));
                if(clock>=nextAttack && combat.CurrentState==SapoDoPneuCombat.State.Float && !combat.HasWaterDrops)BeginHop();
                break;
            case State.Warning:
                if(timer>=WarningDuration){from=body.position;if(cue!=null)Destroy(cue.gameObject);SetState(State.Jump);}
                break;
            case State.Jump:
                float t=Mathf.Clamp01(timer/HopDuration);Vector2 point=Vector2.Lerp(from,landing,Mathf.SmoothStep(0,1,t));point.y+=Mathf.Sin(t*Mathf.PI)*1.8f;
                if(contactCooldown<=0 && LakeHit.Touch(target,body.position,point,.35f,filter,contacts))
                {contactCooldown=.8f;TryHit();if(!combat.Active){Stop();return;}}
                body.MovePosition(point);
                if(t>=1){body.position=landing;Hops++;LakeWaterFX.Create(threats,new Vector2(landing.x,combat.WaterY),.9f);SetState(State.Landing);}
                break;
            case State.Landing:
                if(timer>=.2f){if(Hops==0){SetState(State.Rest);}else{Emit();SetState(State.Spit);}}
                break;
            case State.Spit:
                if(timer>=.3f){nextAttack=clock+3.8f;SetState(State.Rest);}
                break;
        }
    }
    private void BeginHop()
    {
        berth++;landing=new Vector2(combat.Arena.xMin+combat.Arena.width*(berth%2==0?.59f:.78f),combat.WaterY+.06f);
        lockedAim=target.transform.position;
        cue=LakeWaterFX.Create(threats,new Vector2(landing.x,combat.WaterY),1.1f,true);SetState(State.Warning);
    }
    private void Emit()
    {
        Vector2 start=body.position+new Vector2(-.42f,.52f);
        // Three widely spaced, aimed arcs with a fixed target established before the hop.
        const float travel=.8f;
        Vector2 aim=(lockedAim-start)/travel+Vector2.up*(.5f*LakeDroplet.Gravity*travel);
        aim.x=Mathf.Clamp(aim.x,-8,8);aim.y=Mathf.Clamp(aim.y,3.4f,7.8f);
        for(int i=-1;i<=1;i++)
        {
            var node=new GameObject("SapererecaWaterDrop",typeof(Rigidbody2D),typeof(SpriteRenderer));node.transform.SetParent(threats,false);
            node.AddComponent<LakeDroplet>().Configure(combat,target,combat.DropletSprite,start,aim+new Vector2(i*1.6f,i*.25f),combat.WaterY,this);Shots++;
        }
    }
    public bool TryHit()
    {
        if(combat==null || !combat.Active || target==null || clock<nextDamage)return false;
        nextDamage=clock+.65f;if(!target.TakeDamage(1))return false;Hits++;return true;
    }
    private void LateUpdate()
    {
        if(art==null || !Joined || CurrentState==State.Stopped)return;
        int frame=CurrentState==State.Warning?1:CurrentState==State.Jump?2:CurrentState==State.Landing?3:CurrentState==State.Spit?4:Mathf.Repeat(clock,3.7f)<.13f?5:0;
        visual.sprite=art.frames[frame];visual.flipX=CurrentState==State.Jump && landing.x>from.x;
        float breath=CurrentState==State.Rest?Mathf.Sin(clock*2.4f)*.018f:0;transform.localScale=new Vector3(1-breath*.5f,1+breath,1);
        if(waterProperties!=null)
        {
            waterProperties.SetFloat("_WaterY",combat.WaterY);waterProperties.SetFloat("_Clock",clock);waterProperties.SetFloat("_Reflection",0);visual.SetPropertyBlock(waterProperties);
            reflection.enabled=visual.enabled;reflection.sprite=visual.sprite;reflection.flipX=visual.flipX;
            reflection.transform.localScale=new Vector3(1,-.42f,1);reflection.transform.position=new Vector3(transform.position.x,combat.WaterY-(transform.position.y-combat.WaterY)*.42f,0);
            reflectionProperties.SetFloat("_WaterY",combat.WaterY);reflectionProperties.SetFloat("_Clock",clock);reflectionProperties.SetFloat("_Reflection",1);reflection.SetPropertyBlock(reflectionProperties);
        }
    }
    public void Stop()
    {
        if(CurrentState==State.Stopped)return;
        SetState(State.Stopped);if(visual!=null)visual.enabled=false;if(reflection!=null)reflection.enabled=false;
        if(cue!=null)Destroy(cue.gameObject);if(threats!=null){threats.gameObject.SetActive(false);Destroy(threats.gameObject);threats=null;}
        if(hud!=null)hud.HideCompanion();if(body!=null)body.linearVelocity=Vector2.zero;
    }
    private void OnDisable(){Stop();}
}
