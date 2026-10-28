using System.Collections.Generic;
using UnityEngine;

// Native Tear animation events release physical props exactly when the painted hands let go.
[DefaultExecutionOrder(110)]
public sealed class MacaquiniTransformationPresentation : MonoBehaviour
{
    [SerializeField] private Sprite bananaCap,peelJetpack;
    [SerializeField] private AudioClip roar,rip;
    [SerializeField] private float mapBottom=-8.5f;
    private readonly List<GameObject> debris=new List<GameObject>();
    private MacaquiniFlightPresentation actor;
    private MacaquiniPhaseTwo phase;
    private Boss boss;
    private Transform painted;
    private AudioSource voice;
    private LineRenderer pressure;
    private float waveStart=-10;
    private bool capReleased,packReleased,roared;
    public int CapReleases {get;private set;}
    public int PackReleases {get;private set;}
    public int Roars {get;private set;}
    public int PiecesAtMapBottom {get;private set;}
    public float LastRemovalTop {get;private set;}
    public float MapBottom=>mapBottom;
    public float StrengthScale=>painted.localScale.x;
    public GameObject[] ActiveDebris=>debris.FindAll(g=>g!=null).ToArray();
    private void Awake()
    {
        actor=GetComponent<MacaquiniFlightPresentation>();phase=GetComponent<MacaquiniPhaseTwo>();boss=GetComponent<Boss>();
        painted=transform.Find("CompletePaintedBody");
        voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;GameAudioSettings.Route(voice,AudioChannel.Effects);
        var ring=new GameObject("TransformationPressure");ring.transform.SetParent(transform,false);
        pressure=ring.AddComponent<LineRenderer>();pressure.sharedMaterial=new Material(Shader.Find("Sprites/Default"));
        pressure.useWorldSpace=false;pressure.loop=true;pressure.positionCount=48;pressure.startWidth=pressure.endWidth=.045f;pressure.sortingOrder=28;pressure.enabled=false;
    }
    private void OnEnable(){GetComponent<MacaquiniPhaseTwo>().TransitionStarted+=Begin;GetComponent<Boss>().BattleStopped+=Clear;}
    private void OnDisable(){GetComponent<MacaquiniPhaseTwo>().TransitionStarted-=Begin;GetComponent<Boss>().BattleStopped-=Clear;Clear();}
    private void Begin(){capReleased=packReleased=roared=false;painted.localScale=Vector3.one;}
    public void MacaquiniReleaseCap()
    {
        if(!actor.Transforming||capReleased)return;capReleased=true;CapReleases++;
        actor.AdvanceTearMilestone(1.75f/8);
        Spawn("FallingBananaCap",bananaCap,new Vector3(-1.2f,2.75f),new Vector2(-1.25f,1.4f),95);
        if(rip!=null)voice.PlayOneShot(rip,.65f);
        GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.055f,.16f);
    }
    public void MacaquiniRipEffort()
    {
        if(!actor.Transforming)return;
        if(rip!=null){voice.pitch=.8f;voice.PlayOneShot(rip,.8f);}
        GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.065f,.3f);
    }
    public void MacaquiniReleasePack()
    {
        if(!actor.Transforming||packReleased)return;packReleased=true;PackReleases++;
        actor.AdvanceTearMilestone(5f/8);
        Spawn("FallingPeelAndJetpack",peelJetpack,new Vector3(1.2f,1.35f),new Vector2(.7f,1.1f),-70);
        waveStart=Time.time;GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.11f,.4f);
    }
    public void MacaquiniPowerRoar()
    {
        if(!actor.Transforming||roared)return;roared=true;Roars++;voice.pitch=1;
        actor.AdvanceTearMilestone(5.5f/8);
        if(roar!=null)voice.PlayOneShot(roar,.85f);
        waveStart=Time.time;GusiniPhaseTwoCameraFeedback.Pulse(Camera.main,.10f,.75f);
    }
    private void Spawn(string name,Sprite sprite,Vector3 local,Vector2 velocity,float spin)
    {
        if(sprite==null)return;
        var piece=new GameObject(name);piece.transform.position=painted.TransformPoint(local);
        piece.transform.localScale=painted.lossyScale;piece.transform.rotation=painted.rotation;
        // If a future arena is taller, use its actual lower camera extent too.
        float boundary=Camera.main==null?mapBottom:Mathf.Min(mapBottom,Camera.main.transform.position.y-Camera.main.orthographicSize-2);
        piece.AddComponent<MacaquiniFallingDebris>().Initialize(sprite,velocity,spin,boundary,y=>{PiecesAtMapBottom++;LastRemovalTop=y;});
        debris.Add(piece);
        if(name=="FallingPeelAndJetpack")actor.AttachDiscardedSmoke(piece.transform);
    }
    private void LateUpdate()
    {
        if(Time.timeScale<=0)return;
        float strength=actor.HalfShell?1:actor.Transforming?Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,.80f,actor.TearProgress)):0;
        var kick=GetComponent<MacaquiniSwingKick>();
        if(kick==null||!kick.Running)painted.localScale=Vector3.one*(actor.UniformSpriteScale?1f:1+strength*.065f);
        float age=Time.time-waveStart;pressure.enabled=boss.BattleActive&&age>=0&&age<.65f;
        if(pressure.enabled){float t=age/.65f,radius=Mathf.Lerp(.35f,2.6f,t);pressure.startColor=pressure.endColor=new Color(.65f,.74f,.25f,(1-t)*.55f);for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;pressure.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,1+Mathf.Sin(a)*radius*.6f));}}
        debris.RemoveAll(g=>g==null);
    }
    private void Clear(){foreach(var g in debris)if(g!=null)Destroy(g);debris.Clear();if(voice!=null)voice.Stop();if(pressure!=null)pressure.enabled=false;}
    private void OnDestroy(){if(pressure!=null&&pressure.sharedMaterial!=null)Destroy(pressure.sharedMaterial);}
}
