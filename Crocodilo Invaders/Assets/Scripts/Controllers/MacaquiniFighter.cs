using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(25)]
public sealed class MacaquiniFighter : MonoBehaviour
{
    public enum Move { Jab, Cross, Uppercut, Hammer }
    public enum State { Idle, Approach, Cue, Strike, Recovery, Helpers, Finisher }
    [SerializeField] private Sprite[] poses,helperPoses;
    private Boss boss;
    private Rigidbody2D body;
    private SpriteRenderer view;
    private Health target;
    private Material material;
    private AudioClip sound;
    private AudioSource voice;
    private Transform effects;
    private LineRenderer path,cue,wind;
    private Vector2 origin,end,home,aim,previousFist;
    private Vector3 originalScale;
    private int originalOrder;
    private bool originalFlip,flip,hit;
    private float started,duration,nextThreatAt,approachArc,carryDrawingUntil=-1;
    private int completedCombos;
    private Vector2 guardOrigin;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts=new Collider2D[16];
    private readonly List<MacaquiniFighterMinion> helpers=new List<MacaquiniFighterMinion>();
    private readonly int[] bag={0,1,2,3,4,5};
    private int bagAt,lastCombo=-1;
    private static readonly Move[][] combos={
        new[]{Move.Jab,Move.Cross},new[]{Move.Uppercut,Move.Cross,Move.Jab},
        new[]{Move.Jab,Move.Cross,Move.Hammer},new[]{Move.Cross,Move.Uppercut,Move.Cross},
        new[]{Move.Jab,Move.Uppercut,Move.Hammer},new[]{Move.Cross,Move.Jab,Move.Cross,Move.Hammer}
    };
    public bool Running {get;private set;}
    public State CurrentState {get;private set;}
    public Move CurrentMove {get;private set;}
    public bool ControlsDrawing=>Running&&CurrentState!=State.Helpers&&CurrentState!=State.Finisher||!Running&&Time.time<carryDrawingUntil&&boss.BattleActive&&!boss.Transitioning;
    public bool Enraged=>boss!=null&&boss.Health.Fraction<=.45f;
    public float HelperCueDuration=>Enraged?.56f:.70f;
    public int ComboLength {get;private set;}
    public int ComboStrikeIndex {get;private set;}
    public int InterleavedStrikes {get;private set;}
    public float RecoveryDuration {get;private set;}
    public bool Dangerous=>Running&&CurrentState==State.Strike&&Time.time-started>=.065f&&Time.time-started<duration-.025f;
    public int CompletedRounds {get;private set;}
    public int StrikesStarted {get;private set;}
    public int DamageHits {get;private set;}
    public int HelpersSpawned {get;private set;}
    public int ComboId {get;private set;}
    public float CueDuration {get;private set;}
    public float LastDangerGap {get;private set;}=10;
    public float LastThreatEnded {get;private set;}=-10;
    public float NextThreatAt=>nextThreatAt;
    public Vector2 LockedAim=>aim;
    public Vector2 FistPosition=>body.position+Offset(CurrentMove);
    public int LivingHelpers=>helpers.FindAll(h=>h!=null&&h.Alive).Count;
    public bool CanContinue=>Running&&isActiveAndEnabled&&boss.BattleActive&&!boss.Health.IsDead&&target!=null&&!target.IsDead;
    private void Awake(){boss=GetComponent<Boss>();body=GetComponent<Rigidbody2D>();view=transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>();}
    public IEnumerator Round(Health player,Material fx,AudioClip clip,int forced=-1)
    {
        Cancel();target=player;material=fx;sound=clip;Running=true;home=body.position;
        ComboLength=0;ComboStrikeIndex=0;
        originalScale=view.transform.localScale;originalOrder=view.sortingOrder;originalFlip=view.flipX;
        filter=new ContactFilter2D();filter.SetLayerMask(1<<player.gameObject.layer);filter.useTriggers=true;
        effects=new GameObject("MacaquiniFighterEffects").transform;effects.SetParent(transform,false);
        path=MakeLine(effects,"FighterLockedPath",material,.035f,28,2);
        cue=MakeLine(effects,"FighterPostureCue",material,.055f,36,25);
        wind=MakeLine(effects,"FighterStrikeWind",material,.08f,35,3);
        voice=effects.gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;GameAudioSettings.Route(voice,AudioChannel.Effects);
        boss.Movement.HoldPosition(true);
        if(forced==-2 || forced<0&&CompletedRounds%3==2)yield return Reinforcements();
        else if(forced>=0){ComboLength=1;ComboStrikeIndex=0;yield return Perform((Move)forced);}
        else
        {
            ComboId=NextCombo();ComboLength=Mathf.Min(combos[ComboId].Length,Enraged?4:3);
            for(int i=0;i<ComboLength;i++){if(!CanContinue)break;ComboStrikeIndex=i;yield return Perform(combos[ComboId][i]);}
            if(CanContinue)completedCombos++;
            if(CanContinue&&completedCombos%(Enraged?2:3)==0)
            {
                CurrentState=State.Finisher;HideLines();RestoreDrawing();
                var kick=GetComponent<MacaquiniSwingKick>();kick.Begin(target,material,sound);
                while(CanContinue&&kick.Running)yield return null;
                ThreatEnded();
            }
        }
        if(CanContinue)
        {
            CurrentState=State.Recovery;started=Time.time;duration=RecoveryDuration=Enraged?.4f:.55f;origin=body.position;
            float side=body.position.x>=target.transform.position.x?1:-1;
            end=ClampRoot((Vector2)target.transform.position+new Vector2(side*(Enraged?3.6f:4.2f),CompletedRounds%2==0?.6f:-.7f));
            while(CanContinue&&Time.time-started<duration)yield return null;
            CompletedRounds++;
        }
        bool carry=CanContinue;Cancel();if(carry)carryDrawingUntil=Time.time+.24f;
    }
    private int NextCombo()
    {
        if(bagAt>=bag.Length)
        {
            for(int i=bag.Length-1;i>0;i--){int j=Random.Range(0,i+1);(bag[i],bag[j])=(bag[j],bag[i]);}
            if(bag[0]==lastCombo)(bag[0],bag[1])=(bag[1],bag[0]);bagAt=0;
        }
        lastCombo=bag[bagAt++];return lastCombo;
    }
    private IEnumerator Perform(Move move)
    {
        if(!CanContinue)yield break;
        CurrentMove=move;CurrentState=State.Approach;origin=body.position;aim=target.transform.position;flip=aim.x>origin.x;
        float side=flip?-1:1;
        if(move==Move.Cross&&ComboStrikeIndex>0)side=-side;
        end=ClampRoot(new Vector2(aim.x+side*(move==Move.Uppercut?2.9f:move==Move.Hammer?3.2f:4.2f),aim.y+(move==Move.Uppercut?-3.2f:move==Move.Hammer?2.8f:-1.25f)));
        approachArc=move==Move.Uppercut?-.35f:move==Move.Hammer?.42f:.25f;
        started=Time.time;duration=Mathf.Max(.22f,Vector2.Distance(origin,end)/(Enraged?16f:13f));
        while(CanContinue&&Time.time-started<duration)yield return null;
        if(!CanContinue)yield break;
        body.position=end;CurrentState=State.Cue;origin=guardOrigin=body.position;aim=target.transform.position;flip=aim.x>origin.x;
        float cueTime=ComboStrikeIndex==0?(Enraged?.48f:.62f):(Enraged?.40f:.48f);
        CueDuration=Mathf.Max(cueTime,nextThreatAt-Time.time);
        started=Time.time;duration=CueDuration;end=ClampRoot(aim-Offset(move));
        while(CanContinue&&Time.time-started<duration)yield return null;
        if(!CanContinue)yield break;
        origin=body.position;CurrentState=State.Strike;started=Time.time;duration=move==Move.Hammer?.39f:move==Move.Uppercut?.34f:.30f;hit=false;
        previousFist=FistPosition;LastDangerGap=Time.time+.065f-LastThreatEnded;StrikesStarted++;
        if(sound!=null){voice.pitch=move==Move.Hammer?.8f:1.2f;voice.PlayOneShot(sound,.48f);}
        while(CanContinue&&Time.time-started<duration)yield return null;
        ThreatEnded();CurrentState=State.Recovery;origin=body.position;
        end=ClampRoot(origin+((Vector2)body.position-aim).normalized*.38f+Vector2.up*.06f);started=Time.time;duration=.18f;
        if(CanContinue)yield return new WaitForSeconds(.18f);
    }
    public void ThreatEnded()
    {
        LastThreatEnded=Time.time;
        var dash=target==null?null:target.GetComponent<PlayerDash>();
        nextThreatAt=Time.time+Mathf.Max(1.14f,dash==null?1.14f:dash.Cooldown+.04f);
    }
    private IEnumerator Reinforcements()
    {
        CurrentState=State.Helpers;HideLines();RestoreDrawing();
        var camera=Camera.main;float right=camera.transform.position.x+camera.orthographicSize*camera.aspect;
        int count=boss.Health.Fraction<.5f?3:2;
        for(int i=0;i<count;i++)
        {
            int role=(CompletedRounds+i)%3;
            var node=new GameObject("MacaquiniHelper_"+role);int enemyLayer=LayerMask.NameToLayer("Enemy");node.layer=enemyLayer<0?9:enemyLayer;
            node.transform.SetParent(effects,false);node.AddComponent<Health>().SetMaximumHealth(role==2?3:2,true);
            var helper=node.AddComponent<MacaquiniFighterMinion>();
            helper.Configure(this,target,role,helperPoses,material,new Vector2(right+2+i,camera.transform.position.y+2-i*1.4f),new Vector2(right-1.4f-i*.8f,camera.transform.position.y+1-i*1.5f));
            helpers.Add(helper);HelpersSpawned++;
        }
        yield return new WaitForSeconds(.65f);
        var order=helpers.ToArray();
        for(int i=0;i<order.Length;i++)
        {
            if(!CanContinue)yield break;
            CurrentState=State.Helpers;RestoreDrawing();
            if(order[i]!=null&&order[i].Alive)yield return order[i].Assault();
            if(i<order.Length-1&&CanContinue)
            {
                ComboStrikeIndex=0;InterleavedStrikes++;yield return Perform(Enraged?Move.Cross:Move.Jab);
            }
        }
    }
    private Vector2 Offset(Move move)
    {
        Vector2 local=move==Move.Uppercut?new Vector2(-1.27f,2.27f):move==Move.Hammer?new Vector2(-1.82f,-1.05f):move==Move.Cross?new Vector2(-2.32f,1.21f):new Vector2(-2.21f,1.25f);
        if(flip)local.x=-local.x;
        return Vector2.Scale(local,(Vector2)transform.lossyScale);
    }
    private Vector2 ClampRoot(Vector2 p)
    {
        var c=Camera.main;float h=c.orthographicSize,w=h*c.aspect;
        return new Vector2(Mathf.Clamp(p.x,c.transform.position.x-w+1,c.transform.position.x+w-1),Mathf.Clamp(p.y,c.transform.position.y-h+.7f,c.transform.position.y+h-1.3f));
    }
    private void FixedUpdate()
    {
        if(!CanContinue||Time.timeScale<=0)return;
        float age=Time.time-started;
        if(CurrentState==State.Strike)
        {
            float u=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.065f)/(duration-.09f)));
            body.position=Vector2.Lerp(origin,end,u);
            if(CurrentMove==Move.Uppercut)body.position+=Vector2.down*Mathf.Sin(u*Mathf.PI)*.3f;
            Physics2D.SyncTransforms();Vector2 point=FistPosition;
            if(Dangerous&&!hit&&SweptHit(target,previousFist,point,CurrentMove==Move.Hammer?.62f:.34f,CurrentMove==Move.Jab||CurrentMove==Move.Cross?1:2,filter,contacts)){hit=true;DamageHits++;GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.06f,.13f);}
            previousFist=point;
        }
        else if(CurrentState==State.Recovery||CurrentState==State.Approach)
        {
            float u=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/duration));body.position=Vector2.Lerp(origin,end,u);
            if(CurrentState==State.Approach)body.position+=Vector2.up*Mathf.Sin(u*Mathf.PI)*approachArc;
        }
        else if(CurrentState==State.Cue)
        {
            float u=Mathf.Clamp01(age/duration);Vector2 away=(guardOrigin-aim).normalized;
            body.position=guardOrigin+away*(Mathf.Sin(u*Mathf.PI)*.16f)+Vector2.up*Mathf.Sin(u*Mathf.PI*2)*.025f;
        }
    }
    public void Present(LineRenderer vine)
    {
        if(!ControlsDrawing)return;
        if(!Running){view.sprite=poses[0];view.flipX=flip;view.transform.localRotation=Quaternion.identity;vine.enabled=false;return;}
        bool striking=CurrentState==State.Strike&&Time.time-started>.055f;
        int frame=striking?(int)CurrentMove+1:CurrentMove==Move.Hammer?4:CurrentMove==Move.Uppercut?3:0;
        if(striking&&CurrentMove==Move.Hammer)frame=5;
        if(CurrentState==State.Recovery)frame=0;
        view.sprite=poses[Mathf.Clamp(frame,0,5)];view.transform.localPosition=Vector3.zero;view.transform.localScale=originalScale;
        float poseProgress=Mathf.Clamp01((Time.time-started)/Mathf.Max(.01f,duration));
        view.transform.localRotation=Quaternion.Euler(0,0,CurrentState==State.Cue?Mathf.Sin(poseProgress*Mathf.PI)*(CurrentMove==Move.Uppercut?-1.5f:1.5f):0);
        view.flipX=flip;view.color=Color.white;view.sortingOrder=originalOrder;view.enabled=true;vine.enabled=false;
        Color color=CurrentMove==Move.Uppercut?new Color(.18f,.85f,1):CurrentMove==Move.Hammer?new Color(1,.3f,.13f):new Color(1,.78f,.16f);
        path.enabled=CurrentState==State.Cue;cue.enabled=CurrentState==State.Cue||CurrentState==State.Recovery;wind.enabled=Dangerous;
        if(path.enabled)
        {
            path.positionCount=CurrentMove==Move.Uppercut?17:2;
            for(int i=0;i<path.positionCount;i++){float t=i/(float)(path.positionCount-1);Vector2 point=Vector2.Lerp(origin+Offset(CurrentMove),aim,t);if(CurrentMove==Move.Uppercut)point.x+=Mathf.Sin(t*Mathf.PI)*.25f;path.SetPosition(i,point);}
            path.startColor=path.endColor=new Color(color.r,color.g,color.b,.6f);
        }
        if(cue.enabled)
        {
            Vector2 center=CurrentState==State.Recovery?body.position+Vector2.up:CurrentMove==Move.Hammer?aim:body.position+Offset(CurrentMove);
            float radius=CurrentState==State.Recovery?1.2f:CurrentMove==Move.Hammer?.78f:.33f+.05f*Mathf.Sin(Time.time*18);
            Color glow=CurrentState==State.Recovery?new Color(.48f,.8f,.28f,.45f):color;cue.startColor=cue.endColor=glow;
            for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;cue.SetPosition(i,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
        }
        if(wind.enabled){Vector2 fist=FistPosition,d=(end-origin).normalized,n=Vector2.Perpendicular(d);wind.SetPosition(0,fist-d*.8f+n*.18f);wind.SetPosition(1,fist);wind.SetPosition(2,fist-d*.8f-n*.18f);wind.startColor=wind.endColor=color;}
    }
    internal static LineRenderer MakeLine(Transform parent,string name,Material material,float width,int order,int points)
    {
        var node=new GameObject(name);node.transform.SetParent(parent,false);var line=node.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=points;line.startWidth=line.endWidth=width;line.sortingOrder=order;line.enabled=false;return line;
    }
    internal static bool SweptHit(Health target,Vector2 from,Vector2 to,float radius,int damage,ContactFilter2D filter,Collider2D[] results)
    {
        Vector2 d=to-from;int count=Physics2D.OverlapCapsule((from+to)*.5f,new Vector2(d.magnitude+2*radius,2*radius),CapsuleDirection2D.Horizontal,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,filter,results);
        for(int i=0;i<count;i++)if(results[i]!=null&&results[i].GetComponentInParent<Health>()==target)return target.TakeDamage(damage);
        return false;
    }
    private void HideLines(){if(path!=null)path.enabled=false;if(cue!=null)cue.enabled=false;if(wind!=null)wind.enabled=false;}
    private void RestoreDrawing(){view.transform.localScale=originalScale;view.transform.localPosition=Vector3.zero;view.transform.localRotation=Quaternion.identity;view.flipX=originalFlip;view.sortingOrder=originalOrder;view.color=Color.white;view.enabled=true;}
    public void Cancel()
    {
        bool held=Time.time<carryDrawingUntil;carryDrawingUntil=-1;
        if(!Running){if(held&&view!=null)RestoreDrawing();return;}Running=false;CurrentState=State.Idle;HideLines();RestoreDrawing();
        if(effects!=null){effects.gameObject.SetActive(false);Destroy(effects.gameObject);}helpers.Clear();
    }
    private void OnDisable()=>Cancel();
}
