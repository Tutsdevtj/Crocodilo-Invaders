using System.Collections;
using UnityEngine;

public sealed class MacaquiniFighterMinion : MonoBehaviour
{
    private enum State { Waiting, Cue, Strike, Exit, Dead }
    private MacaquiniFighter director;
    private Health health,target;
    private Rigidbody2D body;
    private SpriteRenderer view;
    private BoxCollider2D hitbox;
    private Sprite[] poses;
    private int role;
    private Vector2 from,home,aim,end,previous;
    private float age,started,activeDuration;
    private State state;
    private bool hit,scored;
    private LineRenderer warning,vine;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts=new Collider2D[8];
    public bool Alive=>health!=null&&!health.IsDead;
    public bool Dangerous=>Alive&&state==State.Strike;
    public int Role=>role;
    public void Configure(MacaquiniFighter owner,Health player,int type,Sprite[] sprites,Material material,Vector2 spawn,Vector2 waiting)
    {
        director=owner;target=player;role=type;poses=sprites;from=spawn;home=waiting;transform.position=spawn;health=GetComponent<Health>();
        view=gameObject.AddComponent<SpriteRenderer>();view.sortingOrder=32;view.sprite=poses[role];
        hitbox=gameObject.AddComponent<BoxCollider2D>();hitbox.isTrigger=true;hitbox.size=new Vector2(.72f,.85f);hitbox.offset=new Vector2(0,.25f);
        body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.useFullKinematicContacts=true;
        filter=new ContactFilter2D();filter.SetLayerMask(1<<target.gameObject.layer);filter.useTriggers=true;
        warning=MacaquiniFighter.MakeLine(transform,"HelperWarning",material,.025f,30,3);
        vine=MacaquiniFighter.MakeLine(transform,"HelperVine",material,.035f,29,2);
        health.Died+=Die;health.Damaged+=Flash;
    }
    private void Flash(){if(view!=null)view.color=new Color(1,.85f,.7f);}
    private void Die()
    {
        if(scored)return;scored=true;state=State.Dead;started=Time.time;hitbox.enabled=false;warning.enabled=vine.enabled=false;
        FindAnyObjectByType<RunScore>()?.RecordDefeat(200);
    }
    private Vector2 HandOffset=>role==0?new Vector2(-.82f,.18f):role==1?new Vector2(-.9f,.25f):Vector2.zero;
    public IEnumerator Assault()
    {
        if(!Alive)yield break;state=State.Cue;aim=target.transform.position;from=body.position;end=aim-HandOffset;
        float tell=Mathf.Max(director.HelperCueDuration,director.NextThreatAt-Time.time);started=Time.time;
        while(Alive&&director.CanContinue&&Time.time-started<tell)yield return null;
        if(!Alive||!director.CanContinue)yield break;
        state=State.Strike;started=Time.time;activeDuration=role==0?.4f:.55f;hit=false;previous=body.position+HandOffset;
        while(Alive&&director.CanContinue&&Time.time-started<activeDuration)yield return null;
        director.ThreatEnded();if(!Alive)yield break;
        state=State.Exit;started=Time.time;from=body.position;
        var c=Camera.main;end=new Vector2(c.transform.position.x-c.orthographicSize*c.aspect-2,c.transform.position.y+3);
        // Departure is harmless and runs in FixedUpdate while the boss begins its next cue.
    }
    private void FixedUpdate()
    {
        if(Time.timeScale<=0||body==null||state==State.Dead)return;
        if(target==null||target.IsDead||director==null||!director.CanContinue){Destroy(gameObject);return;}
        age+=Time.fixedDeltaTime;
        if(state==State.Waiting)body.position=Vector2.Lerp(from,home,Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.7f)));
        else if(state==State.Strike)
        {
            float u=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-started)/activeDuration));body.position=Vector2.Lerp(from,end,u);
            if(role==1)body.position+=Vector2.down*Mathf.Sin(u*Mathf.PI)*.5f;
            if(role==2)body.position+=Vector2.up*Mathf.Sin(u*Mathf.PI)*.7f;
            Physics2D.SyncTransforms();Vector2 point=body.position+HandOffset;
            if(!hit&&MacaquiniFighter.SweptHit(target,previous,point,role==2?.42f:.23f,1,filter,contacts))hit=true;
            previous=point;
        }
        else if(state==State.Exit){float u=Mathf.Clamp01((Time.time-started)/.65f);body.position=Vector2.Lerp(from,end,Mathf.SmoothStep(0,1,u));if(u>=1){gameObject.SetActive(false);Destroy(gameObject);}}
    }
    private void LateUpdate()
    {
        if(view==null||Time.timeScale<=0)return;
        if(state==State.Dead){float t=(Time.time-started)/.2f;view.color=new Color(1,1,1,1-t);transform.localScale=Vector3.one*(1-t*.7f);if(t>=1)Destroy(gameObject);return;}
        view.sprite=poses[role+(state==State.Strike?3:0)];view.color=Color.Lerp(view.color,Color.white,Time.deltaTime*14);
        view.transform.localRotation=Quaternion.Euler(0,0,role==2&&state==State.Strike?-(Time.time-started)/activeDuration*540:Mathf.Sin(age*3+role)*2);
        warning.enabled=state==State.Cue;
        if(warning.enabled){warning.SetPosition(0,from+HandOffset);warning.SetPosition(1,(from+aim)*.5f);warning.SetPosition(2,aim);warning.startColor=warning.endColor=new Color(1,.72f,.16f,.75f);}
        vine.enabled=role==1&&state!=State.Exit;
        if(vine.enabled){vine.SetPosition(0,new Vector3(home.x+.8f,Camera.main.transform.position.y+Camera.main.orthographicSize+1));vine.SetPosition(1,body.position+new Vector2(-.15f,1));vine.startColor=vine.endColor=new Color(.28f,.52f,.11f);}
    }
    private void OnDestroy(){if(health!=null){health.Died-=Die;health.Damaged-=Flash;}}
}
