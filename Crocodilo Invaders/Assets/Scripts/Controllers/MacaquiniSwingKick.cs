using UnityEngine;

// One owned action: build momentum, release, kick through the arena, circle behind the trees, catch.
[DefaultExecutionOrder(20)]
public sealed class MacaquiniSwingKick : MonoBehaviour
{
    public enum Stage { Idle, Windup, Kick, Offscreen, Background, Catch }
    [SerializeField] private Sprite[] poses;
    private const float ReleaseAt=2.6f, KickTime=.95f, AwayTime=.28f, BackTime=2.1f, CatchTime=.8f;
    private Boss boss;
    private Rigidbody2D body;
    private Health target;
    private SpriteRenderer view;
    private Vector2 anchor,aim,exit,backEnd,backStart;
    private Vector3 originalScale;
    private int originalOrder;
    private float age,halfWidth,halfHeight,centerX,centerY;
    private bool locked,hit;
    private bool originalFlip;
    private LineRenderer warning,streak;
    private LineRenderer kickArc,releaseRing;
    private LineRenderer[] wind;
    private readonly Collider2D[] contacts=new Collider2D[16];
    private ContactFilter2D filter;
    private AudioSource audioSource;
    private AudioClip whoosh;
    public bool Running{get;private set;}
    public Stage CurrentStage{get;private set;}
    public float Age=>age;
    public bool Dangerous=>Running&&CurrentStage==Stage.Kick;
    public int DamageHits{get;private set;}
    public int PoseCount=>poses==null?0:poses.Length;
    public Vector2 LockedAim=>aim;
    public Vector2 Anchor=>anchor;
    public float PerspectiveScale{get;private set;}=1;
    public float TotalDuration=>ReleaseAt+KickTime+AwayTime+BackTime+CatchTime;
    private void Awake(){boss=GetComponent<Boss>();body=GetComponent<Rigidbody2D>();view=transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>();}
    public void Begin(Health player,Material material,AudioClip clip)
    {
        Cancel();target=player;anchor=body.position;age=0;locked=hit=false;DamageHits=0;whoosh=clip;
        originalScale=view.transform.localScale;originalOrder=view.sortingOrder;originalFlip=view.flipX;
        var camera=Camera.main;halfHeight=camera.orthographicSize;halfWidth=halfHeight*camera.aspect;centerX=camera.transform.position.x;centerY=camera.transform.position.y;
        boss.Movement.HoldPosition(true);filter=new ContactFilter2D();filter.SetLayerMask(1<<player.gameObject.layer);filter.useTriggers=true;
        warning=Line("KickDirectionWarning",material,.045f,26);warning.positionCount=3;
        streak=Line("KickAirTrail",material,.16f,27);streak.positionCount=2;
        kickArc=Line("KickWindArc",material,.11f,36);kickArc.positionCount=17;
        releaseRing=Line("KickReleaseBurst",material,.07f,28);releaseRing.positionCount=25;
        wind=new LineRenderer[3];
        for(int i=0;i<wind.Length;i++){wind[i]=Line("KickSpeedWind"+i,material,.045f,35);wind[i].positionCount=2;}
        audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;GameAudioSettings.Route(audioSource,AudioChannel.Effects);
        Running=true;CurrentStage=Stage.Windup;
    }
    private LineRenderer Line(string name,Material material,float width,int order)
    {
        var node=new GameObject(name);node.transform.SetParent(transform,false);var line=node.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.startWidth=line.endWidth=width;line.sortingOrder=order;return line;
    }
    private float SwingAngle(float time)=>Mathf.Sin(Mathf.Clamp01(time/ReleaseAt)*Mathf.PI*3)*Mathf.Lerp(.12f,.7f,Mathf.Clamp01(time/ReleaseAt));
    private Vector2 WindupPosition(float time)
    {
        float angle=SwingAngle(time);return anchor+new Vector2(Mathf.Sin(angle)*3,(1-Mathf.Cos(angle))*3);
    }
    private void Lock()
    {
        locked=true;aim=target.transform.position;
        Vector2 direction=(aim-anchor).normalized;if(direction.sqrMagnitude<.01f)direction=Vector2.left;
        float distance=(halfWidth*2+10)/Mathf.Max(.3f,Mathf.Abs(direction.x));
        exit=anchor+direction*distance;
        // The distant silhouette crosses behind canopy and foreground vines.
        float side=Mathf.Sign(exit.x-centerX);backStart=new Vector2(centerX+side*(halfWidth+4),centerY+2);backEnd=new Vector2(centerX-side*(halfWidth+4),centerY+2);
    }
    private void FixedUpdate()
    {
        if(!Running||Time.timeScale<=0)return;
        if(target==null||target.IsDead||!boss.BattleActive||boss.Health.IsDead){Cancel();return;}
        float previous=age;Vector2 previousFoot=FootPosition();age+=Time.fixedDeltaTime;
        if(!locked&&age>=ReleaseAt-.85f)Lock();
        float local=age-ReleaseAt;
        if(local<0){CurrentStage=Stage.Windup;body.position=WindupPosition(age);}
        else if(local<KickTime)
        {
            if(CurrentStage!=Stage.Kick){if(whoosh!=null)audioSource.PlayOneShot(whoosh,.9f);GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.08f,.18f);}
            CurrentStage=Stage.Kick;body.position=Vector2.Lerp(anchor,exit,local/KickTime);
            Physics2D.SyncTransforms();
            if(!hit)CheckHit(previous<ReleaseAt?FootPosition():previousFoot,FootPosition());
        }
        else if(local<KickTime+AwayTime){CurrentStage=Stage.Offscreen;body.position=exit;}
        else if(local<KickTime+AwayTime+BackTime)
        {
            CurrentStage=Stage.Background;float t=(local-KickTime-AwayTime)/BackTime;
            body.position=new Vector2(Mathf.Lerp(backStart.x,backEnd.x,t),centerY+2+Mathf.Sin(t*Mathf.PI)*.75f);
        }
        else if(age<TotalDuration)
        {
            CurrentStage=Stage.Catch;float t=Mathf.SmoothStep(0,1,(age-(TotalDuration-CatchTime))/CatchTime);
            body.position=Vector2.Lerp(backEnd,anchor,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*1.5f;
        }
        else {Cancel();return;}
        boss.Health.SetInvulnerable(this,CurrentStage==Stage.Offscreen||CurrentStage==Stage.Background||CurrentStage==Stage.Catch);
    }
    private float KickRoll()
    {
        Vector2 direction=exit-anchor;return Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-180;
    }
    private Vector2 FootPosition()=>body.position+(Vector2)(Quaternion.Euler(0,0,KickRoll())*new Vector3(-2.3f,.3f))*Mathf.Abs(originalScale.x);
    private void CheckHit(Vector2 from,Vector2 to)
    {
        Vector2 delta=to-from;int count=Physics2D.OverlapCapsule((from+to)*.5f,new Vector2(delta.magnitude+1.05f,1.05f),CapsuleDirection2D.Horizontal,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,filter,contacts);
        for(int i=0;i<count;i++)if(contacts[i]!=null&&contacts[i].GetComponentInParent<Health>()==target)
        {if(target.TakeDamage(3)){hit=true;DamageHits++;}break;}
    }
    public void Present(LineRenderer vine)
    {
        float local=age-ReleaseAt;PerspectiveScale=1;float roll=0;int pose=0;Color tint=Color.white;
        if(CurrentStage==Stage.Windup){pose=0;roll=-SwingAngle(age)*Mathf.Rad2Deg*.7f;}
        else if(CurrentStage==Stage.Kick){pose=local<.035f?1:local<.075f?2:3;roll=KickRoll();}
        else if(CurrentStage==Stage.Background){pose=4;PerspectiveScale=.25f+.07f*Mathf.Sin((local-KickTime-AwayTime)/BackTime*Mathf.PI);roll=12;tint=new Color(.48f,.7f,.61f,.9f);}
        else if(CurrentStage==Stage.Catch){pose=5;float t=Mathf.SmoothStep(0,1,(age-(TotalDuration-CatchTime))/CatchTime);PerspectiveScale=Mathf.Lerp(.25f,1,t);roll=-Mathf.Sin(t*Mathf.PI)*18;}
        else pose=3;
        view.sprite=poses[Mathf.Clamp(pose,0,poses.Length-1)];view.transform.localScale=originalScale*PerspectiveScale;
        view.transform.localPosition=Vector3.zero;view.transform.localRotation=Quaternion.Euler(0,0,roll);view.color=tint;
        view.sortingOrder=CurrentStage==Stage.Background?-80:originalOrder;view.enabled=CurrentStage!=Stage.Offscreen;
        view.flipX=CurrentStage==Stage.Background?backEnd.x>backStart.x:originalFlip;
        vine.enabled=CurrentStage==Stage.Windup||CurrentStage==Stage.Catch&&age>TotalDuration-.13f;
        if(vine.enabled)
        {
            Vector3 hand=view.transform.TransformPoint(CurrentStage==Stage.Windup?new Vector3(.68f,3.07f):new Vector3(.16f,3.20f));Vector3 top=new Vector3(anchor.x+.4f,centerY+halfHeight+3);
            for(int i=0;i<vine.positionCount;i++)vine.SetPosition(i,vine.transform.InverseTransformPoint(Vector3.Lerp(hand,top,i/(float)(vine.positionCount-1))));
            vine.startColor=vine.endColor=new Color(.19f,.42f,.08f);
        }
        warning.enabled=CurrentStage==Stage.Windup&&locked;
        if(warning.enabled){warning.SetPosition(0,anchor);warning.SetPosition(1,aim);warning.SetPosition(2,exit);warning.startColor=warning.endColor=new Color(1,.66f,.14f,.55f+.25f*Mathf.Sin(age*16));}
        streak.enabled=Dangerous;streak.startColor=new Color(.83f,1,.65f,0);streak.endColor=new Color(.83f,1,.65f,.4f);
        if(streak.enabled){Vector2 direction=(exit-anchor).normalized;streak.SetPosition(0,body.position-direction*2.5f);streak.SetPosition(1,body.position);}
        DrawKickEffects(local);
    }
    private void DrawKickEffects(float local)
    {
        kickArc.enabled=Dangerous;releaseRing.enabled=Dangerous&&local<.24f;
        Vector2 direction=(exit-anchor).normalized,normal=Vector2.Perpendicular(direction),foot=FootPosition();
        if(kickArc.enabled)
        {
            float radius=.68f+.07f*Mathf.Sin(local*24);
            kickArc.startWidth=.035f;kickArc.endWidth=.13f;
            kickArc.startColor=new Color(.76f,1,.62f,.1f);kickArc.endColor=new Color(1,.96f,.69f,.9f);
            for(int i=0;i<17;i++){float angle=Mathf.Lerp(-1.25f,1.25f,i/16f);kickArc.SetPosition(i,foot+(direction*Mathf.Cos(angle)+normal*Mathf.Sin(angle))*radius);}
        }
        if(releaseRing.enabled)
        {
            float t=local/.24f,radius=Mathf.Lerp(.25f,1.7f,t);
            releaseRing.startColor=releaseRing.endColor=new Color(.87f,1,.65f,(1-t)*.65f);
            for(int i=0;i<25;i++){float angle=i*Mathf.PI*2/24;releaseRing.SetPosition(i,anchor+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius);}
        }
        for(int i=0;i<wind.Length;i++)
        {
            var line=wind[i];line.enabled=Dangerous;if(!Dangerous)continue;
            Vector2 tip=foot-direction*(.25f+i*.12f)+normal*(i-1)*.32f;
            float length=1.1f+.4f*Mathf.Sin(local*30+i*2);
            line.SetPosition(0,tip-direction*length);line.SetPosition(1,tip);
            line.startColor=new Color(.78f,1,.68f,0);line.endColor=new Color(.96f,1,.81f,.65f);
        }
    }
    public void Cancel()
    {
        if(!Running)return;Running=false;CurrentStage=Stage.Idle;PerspectiveScale=1;
        boss.Health.SetInvulnerable(this,false);body.position=anchor;body.linearVelocity=Vector2.zero;
        if(view!=null){view.transform.localScale=originalScale;view.transform.localRotation=Quaternion.identity;view.transform.localPosition=Vector3.zero;view.color=Color.white;view.sortingOrder=originalOrder;view.flipX=originalFlip;view.enabled=true;}
        if(warning!=null){warning.gameObject.SetActive(false);Destroy(warning.gameObject);}if(streak!=null){streak.gameObject.SetActive(false);Destroy(streak.gameObject);}
        if(audioSource!=null){audioSource.Stop();Destroy(audioSource);}
        if(kickArc!=null){kickArc.gameObject.SetActive(false);Destroy(kickArc.gameObject);}
        if(releaseRing!=null){releaseRing.gameObject.SetActive(false);Destroy(releaseRing.gameObject);}
        if(wind!=null)foreach(var line in wind)if(line!=null){line.gameObject.SetActive(false);Destroy(line.gameObject);}
    }
    private void OnDisable()=>Cancel();
}
