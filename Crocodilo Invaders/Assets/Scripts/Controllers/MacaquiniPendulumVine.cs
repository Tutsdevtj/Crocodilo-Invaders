using UnityEngine;

// Legacy Physics2D: the painted vine and its swept capsule share the same pendulum angle.
public sealed class MacaquiniPendulumVine : MonoBehaviour
{
    private const float Radius=.18f, SweepTime=1.65f, TurnTime=1.4f;
    private Health target;
    private MacaquiniFlightPresentation actor;
    private Vector2 pivot;
    private float length,age,warning,amplitude;
    private int direction,passes,previousPass=-1;
    private bool hit;
    private LineRenderer bark,core,guide,handle,trail,arrow;
    private Transform[] pieces;
    private AudioSource voice;
    private AudioClip whip;
    private readonly Collider2D[] contacts=new Collider2D[16];
    private ContactFilter2D filter;
    public bool Dangerous{get;private set;}
    public int PassIndex{get;private set;}
    public int PassCount=>passes;
    public float Age=>age;
    public float Angle{get;private set;}
    public float Duration=>warning+passes*SweepTime+(passes-1)*TurnTime+.25f;
    public float WarningRemaining=>Mathf.Max(0,warning-age);
    public Vector2 Pivot=>pivot;
    public int DamageHits{get;private set;}
    public int PaintedSegments=>pieces==null?0:pieces.Length;
    public void Configure(Health player,MacaquiniFlightPresentation source,Material material,Sprite sprite,AudioClip sound,bool fromLeft,bool returns,float prepare=1.15f)
    {
        target=player;actor=source;warning=prepare;direction=fromLeft?1:-1;passes=returns?2:1;whip=sound;
        var camera=Camera.main;float halfHeight=camera==null?5:camera.orthographicSize;
        float halfWidth=camera==null?9:halfHeight*camera.aspect;
        amplitude=Mathf.Min(86*Mathf.Deg2Rad,Mathf.Max(74*Mathf.Deg2Rad,Mathf.Atan2(halfWidth+1,3)+.04f));
        pivot=camera==null?new Vector2(0,8):new Vector2(camera.transform.position.x,camera.transform.position.y+halfHeight+3);
        length=Mathf.Sqrt(Mathf.Pow(halfHeight*2+3,2)+halfWidth*halfWidth)+2;
        filter=new ContactFilter2D();filter.SetLayerMask(1<<player.gameObject.layer);filter.useTriggers=true;
        bark=Line("PendulumBark",material,.48f,22);core=Line("PendulumCore",material,.1f,24);
        guide=Line("SweepWarning",material,.055f,21);guide.positionCount=65;
        handle=Line("MacaquiniControlVine",material,.075f,29);
        trail=Line("SwingAfterimage",material,.16f,20);
        arrow=Line("SwingDirection",material,.09f,25);arrow.positionCount=3;
        voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;GameAudioSettings.Route(voice,AudioChannel.Effects);
        if(sprite!=null)
        {
            float scale=.9f/Mathf.Max(.01f,sprite.bounds.size.x),step=sprite.bounds.size.y*scale*.85f;
            pieces=new Transform[Mathf.CeilToInt(length/step)+1];
            for(int i=0;i<pieces.Length;i++)
            {
                var node=new GameObject("SwingVineSegment");node.transform.SetParent(transform,false);var view=node.AddComponent<SpriteRenderer>();view.sprite=sprite;view.sortingOrder=23;
                node.transform.localScale=Vector3.one*scale;pieces[i]=node.transform;
            }
        }
        Angle=-amplitude*direction;Draw();
    }
    private LineRenderer Line(string name,Material material,float width,int order)
    {
        var node=new GameObject(name);node.transform.SetParent(transform,false);var line=node.AddComponent<LineRenderer>();
        line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=33;line.sortingOrder=order;line.startWidth=line.endWidth=width;line.numCapVertices=0;return line;
    }
    public float FirstCrossingIn(Vector2 point)
    {
        float angle=Mathf.Atan2(point.x-pivot.x,pivot.y-point.y);
        return warning+Mathf.Acos(Mathf.Clamp(-angle/(amplitude*direction),-1,1))/Mathf.PI*SweepTime-age;
    }
    private void State(float time,out int pass,out float angle,out bool active)
    {
        float t=time-warning;pass=0;active=t>=0&&t<SweepTime;
        if(t<SweepTime){angle=-direction*amplitude*Mathf.Cos(Mathf.Clamp01(t/SweepTime)*Mathf.PI);return;}
        if(passes>1&&t>=SweepTime+TurnTime)
        {
            pass=1;float u=(t-SweepTime-TurnTime)/SweepTime;active=u>=0&&u<1;angle=direction*amplitude*Mathf.Cos(Mathf.Clamp01(u)*Mathf.PI);return;
        }
        angle=direction*amplitude;
    }
    private void FixedUpdate()
    {
        if(target==null||target.IsDead||age>=Duration){gameObject.SetActive(false);Destroy(gameObject);return;}
        if(Time.timeScale<=0)return;
        float old=age;age+=Time.fixedDeltaTime;State(age,out int pass,out float angle,out bool active);
        if(pass!=previousPass){hit=false;previousPass=pass;}
        if(active&&!Dangerous&&whip!=null){voice.pitch=pass==0?1:.93f;voice.PlayOneShot(whip,.85f);}
        Dangerous=active;PassIndex=pass;Angle=angle;
        if(!active||hit)return;
        State(Mathf.Max(old,warning+pass*(SweepTime+TurnTime)),out _,out float before,out _);
        int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(angle-before)/(.6f*Mathf.Deg2Rad)));
        for(int i=0;i<=steps&&!hit;i++)CheckContact(Mathf.Lerp(before,angle,i/(float)steps));
    }
    private void CheckContact(float angle)
    {
        Vector2 end=pivot+new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle))*length;
        Vector2 delta=end-pivot;
        int count=Physics2D.OverlapCapsule((pivot+end)*.5f,new Vector2(length+Radius*2,Radius*2),CapsuleDirection2D.Horizontal,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,filter,contacts);
        for(int i=0;i<count;i++)if(contacts[i]!=null&&contacts[i].GetComponentInParent<Health>()==target)
        {
            if(target.TakeDamage(2)){hit=true;DamageHits++;}break;
        }
    }
    private void Update(){if(target!=null&&Time.timeScale>0)Draw();}
    private void Draw()
    {
        bool returning=age>=warning+SweepTime&&age<warning+SweepTime+TurnTime;
        bool preparing=age<warning||returning;
        bark.enabled=core.enabled=preparing||pieces==null;
        bark.startColor=bark.endColor=new Color(.045f,.095f,.025f,preparing?.65f:1);
        core.startColor=core.endColor=preparing?new Color(1,.74f,.15f):new Color(.42f,.83f,.12f);
        handle.startColor=handle.endColor=new Color(.32f,.65f,.08f,.85f);
        guide.enabled=preparing;guide.startColor=guide.endColor=new Color(1,.67f,.12f,.35f+.2f*Mathf.Sin(age*13));
        for(int i=0;i<65;i++)
        {
            float a=Mathf.Lerp(-amplitude,amplitude,i/64f);
            if(guide.enabled)guide.SetPosition(i,pivot+new Vector2(Mathf.Sin(a),-Mathf.Cos(a))*Mathf.Min(length,Camera.main==null?9:Camera.main.orthographicSize+3));
        }
        arrow.enabled=preparing;arrow.startColor=arrow.endColor=new Color(1,.85f,.2f,.9f);
        if(preparing)
        {
            float a=Mathf.Lerp(-.7f,.7f,Mathf.Repeat(age*.8f,1))*(returning?-direction:direction);
            float radius=Camera.main==null?8:Camera.main.orthographicSize+3;
            Vector2 point=pivot+new Vector2(Mathf.Sin(a),-Mathf.Cos(a))*radius;
            Vector2 tangent=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(returning?-direction:direction),normal=Vector2.Perpendicular(tangent);
            arrow.SetPosition(0,point-tangent*.35f+normal*.2f);arrow.SetPosition(1,point);arrow.SetPosition(2,point-tangent*.35f-normal*.2f);
        }
        Vector2 end=pivot+new Vector2(Mathf.Sin(Angle),-Mathf.Cos(Angle))*length;
        for(int i=0;i<33;i++)
        {
            float t=i/32f;Vector3 p=Vector2.Lerp(pivot,end,t);p.x+=Mathf.Sin(t*Mathf.PI*3-age*9)*.018f*Mathf.Sin(t*Mathf.PI);
            bark.SetPosition(i,p);core.SetPosition(i,p);
            float prior=Angle-(Dangerous?.09f*(PassIndex==0?direction:-direction):0);
            trail.SetPosition(i,pivot+new Vector2(Mathf.Sin(prior),-Mathf.Cos(prior))*length*t);
            Vector3 hand=actor==null?pivot:(Vector2)actor.VineGripPosition;
            Vector3 pull=Vector3.Lerp(hand,pivot,t);pull.x+=Mathf.Sin(t*Mathf.PI)*.25f;handle.SetPosition(i,pull);
        }
        trail.enabled=Dangerous;trail.startColor=trail.endColor=new Color(.56f,.85f,.19f,.17f);
        if(pieces!=null)for(int i=0;i<pieces.Length;i++)
        {
            pieces[i].gameObject.SetActive(!preparing);pieces[i].position=Vector2.Lerp(pivot,end,i/(float)(pieces.Length-1));
            pieces[i].rotation=Quaternion.Euler(0,0,Angle*Mathf.Rad2Deg);
        }
    }
    private void OnDisable(){Dangerous=false;if(voice!=null)voice.Stop();}
}
