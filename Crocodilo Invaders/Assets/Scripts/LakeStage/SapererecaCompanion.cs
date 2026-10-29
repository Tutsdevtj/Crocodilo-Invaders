using UnityEngine;

// Shares the encounter's health/lifetime; she is an assisting character, not a second boss.
public sealed class SapererecaCompanion : MonoBehaviour
{
    public enum State { InHud, Entering, Rest, Warning, Jump, Landing, Spit, Stopped, Dive, Swim, Surface, Ready }
    public const float EntranceDuration=1.25f,WarningDuration=.65f,HopDuration=.9f;
    public const float WorldScale=1.65f;
    private SapoDoPneuCombat combat;
    private SapererecaAssets art;
    private Health target;
    private Rigidbody2D body;
    private SpriteRenderer visual,reflection;
    private Transform threats;
    private LakeBossBar hud;
    private LakeBackdrop lake;
    private float standingFraction=.46f,landingFraction=.46f;
    private Vector2 WaterBerth(float fraction) => new Vector2(combat.Arena.xMin+combat.Arena.width*fraction,combat.WaterY+.06f+Mathf.Sin((lake!=null?lake.MotionClock:Time.time)*1.7f+fraction*8)*.025f);
    private LakeWaterFX cue;
    private MaterialPropertyBlock waterProperties,reflectionProperties;
    private Vector2 from,landing,lockedAim;
    private float timer,clock,nextAttack,contactCooldown,nextDamage;
    private int berth;
    private int burstShots;
    private bool tongueCombo;
    private LakeDiveBubbles bubbles;
    public int Dives { get; private set; }
    public int Combos { get; private set; }
    public int Volleys { get; private set; }
    public Vector2 LastLaunch { get; private set; }
    public Vector2 LastVelocity { get; private set; }
    public float LastTravel { get; private set; }
    private readonly Collider2D[] contacts=new Collider2D[8];
    private ContactFilter2D filter;
    public State CurrentState { get; private set; }=State.InHud;
    public bool Busy => (Joined && CurrentState!=State.Rest && CurrentState!=State.Stopped) || CurrentState==State.Entering || ActiveDrops>0;
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
        transform.localScale=Vector3.one*WorldScale;
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
        Entries++;landingFraction=standingFraction=.46f;landing=WaterBerth(landingFraction);
        body.position=landing;hud=FindAnyObjectByType<LakeBossBar>();
        if(hud!=null)hud.BeginCompanionJump(landing);SetState(State.Entering);
    }
    private void SetState(State state) { CurrentState=state;timer=0; }
    private void FixedUpdate()
    {
        if(combat==null || !combat.Active){Stop();return;}
        if(combat.IsVulnerable){if(Joined)body.MovePosition(WaterBerth(standingFraction));return;}
        float dt=Time.fixedDeltaTime;clock+=dt;timer+=dt;contactCooldown-=dt;
        if(CurrentState==State.Entering)landing=WaterBerth(landingFraction);
        switch(CurrentState)
        {
            case State.Entering:
                if(timer>=EntranceDuration)
                {
                    Joined=true;visual.enabled=true;visual.sprite=art.frames[3];body.position=landing;
                    LandingRipple();nextAttack=clock+1.3f;SetState(State.Landing);
                }
                break;
            case State.Rest:
                body.MovePosition(WaterBerth(standingFraction));
                if(clock>=nextAttack && ActiveDrops==0 && combat.CurrentState==SapoDoPneuCombat.State.Float && !combat.HasWaterDrops && !combat.Tongue.MealPending)BeginDive(false);
                break;
            case State.Dive:
                body.MovePosition(Vector2.Lerp(from,from+Vector2.down*1.9f,Mathf.SmoothStep(0,1,timer/.42f)));
                if(timer>=.42f){visual.enabled=false;from=body.position;SetState(State.Swim);}
                break;
            case State.Swim:
                body.MovePosition(Vector2.Lerp(from,landing+Vector2.down*1.9f,Mathf.SmoothStep(0,1,timer/.72f)));
                if(timer>=.72f)
                {
                    lockedAim=target.GetComponent<Rigidbody2D>().position;
                    bubbles.ShowAim(lockedAim);SetState(State.Warning);
                }
                break;
            case State.Warning:
                if(timer>=WarningDuration){visual.enabled=true;from=body.position;LandingRipple();SetState(State.Surface);}
                break;
            case State.Surface:
                body.MovePosition(Vector2.Lerp(from,landing,Mathf.SmoothStep(0,1,timer/.25f)));
                if(timer>=.25f){body.position=landing;standingFraction=landingFraction;SetState(State.Ready);}
                break;
            case State.Ready:
                body.MovePosition(WaterBerth(standingFraction));
                // Preparation overlaps the tongue, but damaging shots wait until it has fully retracted.
                if(timer>=.28f && combat.CurrentState==SapoDoPneuCombat.State.Float && !combat.Tongue.Busy)
                {burstShots=0;Volleys++;SetState(State.Spit);}
                break;
            case State.Jump:
                float t=Mathf.Clamp01(timer/HopDuration);Vector2 point=Vector2.Lerp(from,landing,Mathf.SmoothStep(0,1,t));point.y+=Mathf.Sin(t*Mathf.PI)*1.8f;
                if(contactCooldown<=0 && LakeHit.Touch(target,body.position,point,.35f*WorldScale,filter,contacts))
                {contactCooldown=.8f;TryHit();if(!combat.Active){Stop();return;}}
                body.MovePosition(point);
                if(t>=1){body.position=landing;standingFraction=landingFraction;Hops++;LandingRipple();SetState(State.Landing);}
                break;
            case State.Landing:
                if(timer>=.2f)SetState(State.Rest);
                break;
            case State.Spit:
                if(burstShots<3 && timer>=burstShots*.11f){Emit(burstShots-1);burstShots++;}
                if(timer>=.5f)
                {
                    if(cue!=null)Destroy(cue.gameObject);if(bubbles!=null)Destroy(bubbles.gameObject);
                    nextAttack=clock+(tongueCombo?2.6f:2.2f);tongueCombo=false;SetState(State.Rest);
                }
                break;
        }
    }
    public void PrepareTongueCombo()
    {
        if(Joined && CurrentState==State.Rest && ActiveDrops==0)BeginDive(true);
    }
    private void BeginDive(bool combo)
    {
        berth++;Dives++;tongueCombo=combo;if(combo)Combos++;
        float playerFraction=(target.transform.position.x-combat.Arena.xMin)/combat.Arena.width;
        float fraction=Mathf.Clamp(playerFraction+(berth%2==0?.22f:-.22f),.15f,.85f);
        if(Mathf.Abs(WaterBerth(fraction).x-combat.transform.position.x)<2.7f)
            fraction=combat.transform.position.x<combat.Arena.center.x?.78f:.22f;
        landingFraction=fraction;landing=WaterBerth(landingFraction);
        from=body.position;LakeWaterFX.Create(threats,new Vector2(from.x,combat.WaterY),.75f);
        cue=LakeWaterFX.Create(threats,new Vector2(landing.x,combat.WaterY),1.1f,true);
        bubbles=LakeDiveBubbles.Create(threats,new Vector2(landing.x,combat.WaterY),lake!=null?lake.WaterFXMaterial:null);
        SetState(State.Dive);
    }
    private void LandingRipple()
    {
        if(lake!=null)LakeSurfaceRipple.Create(threats,new Vector2(landing.x,combat.WaterY),.45f,lake.WaterFXMaterial);
    }
    private void Emit(int spread)
    {
        float side=lockedAim.x<body.position.x?-1:1;
        Vector2 start=body.position+new Vector2(side*.42f,.52f)*WorldScale;
        // Solve the actual ballistic intercept, including the top of the arena. No vertical clamp.
        float travel=Mathf.Clamp(Vector2.Distance(start,lockedAim)/11f,.65f,1.15f);
        Vector2 aim=(lockedAim-start)/travel+Vector2.up*(.5f*LakeDroplet.Gravity*travel);
        Vector2 velocity=aim+new Vector2(-aim.y,aim.x).normalized*(spread*.85f);
        LastLaunch=start;LastVelocity=velocity;LastTravel=travel;
        var node=new GameObject("SapererecaWaterDrop",typeof(Rigidbody2D),typeof(SpriteRenderer));node.transform.SetParent(threats,false);
        node.AddComponent<LakeDroplet>().Configure(combat,target,combat.DropletSprite,start,velocity,combat.WaterY,this);Shots++;
    }
    public bool TryHit()
    {
        if(combat==null || !combat.Active || target==null || clock<nextDamage)return false;
        nextDamage=clock+.65f;if(!target.TakeDamage(1))return false;Hits++;return true;
    }
    private void LateUpdate()
    {
        if(art==null || !Joined || CurrentState==State.Stopped)return;
        int frame=CurrentState==State.Dive || CurrentState==State.Warning?1:CurrentState==State.Jump || CurrentState==State.Surface?2:CurrentState==State.Landing?3:CurrentState==State.Spit || CurrentState==State.Ready?4:Mathf.Repeat(clock,3.7f)<.13f?5:0;
        visual.sprite=art.frames[frame];visual.flipX=CurrentState==State.Spit || CurrentState==State.Ready?lockedAim.x>body.position.x:CurrentState==State.Jump && landing.x>from.x;
        float breath=CurrentState==State.Rest?Mathf.Sin(clock*2.4f)*.018f:0;transform.localScale=new Vector3(1-breath*.5f,1+breath,1)*WorldScale;
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
