using TMPro;
using UnityEngine;

// The route progresses with real waves; decorative objects never block or damage the ship.
public sealed class LakeJourneyScenery : MonoBehaviour
{
    [SerializeField] private LakeJourneyArt art;
    [SerializeField] private Sprite bossHint;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private AudioClip distantCroak;
    private LakeJourneyDirector director;
    private LakeBackdrop lake;
    private WaveController waves;
    private SpriteRenderer[] props;
    private float[] positions;
    private SpriteRenderer silhouette,fly;
    private LakeTongueAssets flies;
    private CanvasGroup heading;
    private TMP_Text title,detail;
    private AudioSource sound;
    private int section=-1,lastWave;
    private float clock,headingAt=-100,hintAt=-100,calmAt=-100;
    private bool wasCalm;
    private static readonly string[] Names={"ENTRADA DO BREJO","LAGO FECHADO","TERRITORIO DO CURURU"};
    public int Section => section;
    public int PropCount => props!=null?props.Length:0;
    public float Clock => clock;
    public bool HintVisible => silhouette!=null && silhouette.enabled;
    public SpriteRenderer Prop(int i) => props[i];
    private void Start()
    {
        director=GetComponent<LakeJourneyDirector>();lake=GetComponent<LakeBackdrop>();waves=FindAnyObjectByType<WaveController>();
        lake.JourneyDensity=DevelopmentBossSession.IsActive?1:.1f;
        flies=Resources.Load<LakeTongueAssets>("LakeTongueV08/art");
        props=new SpriteRenderer[12];positions=new float[props.Length];
        for(int i=0;i<props.Length;i++)
        {
            props[i]=new GameObject("JourneyBankProp_"+i).AddComponent<SpriteRenderer>();props[i].transform.SetParent(transform,false);
            props[i].sprite=art.props[i%4];props[i].sortingOrder=-175;
            props[i].color=new Color(.8f,.91f,.82f,.95f);props[i].transform.localScale=Vector3.one*(i%4==0?1.3f:i%4==2?.8f:1);
            positions[i]=director.Arena.xMin+director.Arena.width*(i/12f);
        }
        silhouette=new GameObject("DistantCururuHint").AddComponent<SpriteRenderer>();silhouette.transform.SetParent(transform,false);
        silhouette.sprite=bossHint;silhouette.sortingOrder=-191;silhouette.color=new Color(.2f,.42f,.39f,.65f);silhouette.transform.localScale=Vector3.one*.55f;silhouette.enabled=false;
        fly=new GameObject("BossArrivalFly").AddComponent<SpriteRenderer>();fly.transform.SetParent(transform,false);fly.sortingOrder=30;fly.transform.localScale=Vector3.one*.55f;fly.enabled=false;
        sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=0;GameAudioSettings.Route(sound,AudioChannel.Effects);
        BuildHeading();
    }
    private void BuildHeading()
    {
        var node=new GameObject("LakeRouteHeading",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(CanvasGroup));
        node.transform.SetParent(transform,false);var canvas=node.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=55;
        var scaler=node.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1200,720);scaler.matchWidthOrHeight=.5f;
        heading=node.GetComponent<CanvasGroup>();heading.blocksRaycasts=heading.interactable=false;heading.alpha=0;
        var panel=new GameObject("RoutePlate",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(node.transform,false);
        var rect=panel.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-18);rect.sizeDelta=new Vector2(470,52);
        var background=panel.GetComponent<UnityEngine.UI.Image>();background.color=new Color(.035f,.14f,.13f,.87f);background.raycastTarget=false;
        title=Text(panel.transform,"Section",13,new Vector2(0,10));detail=Text(panel.transform,"Hint",8,new Vector2(0,-12));detail.color=new Color(.61f,.84f,.76f);
    }
    private TMP_Text Text(Transform parent,string name,float size,Vector2 offset)
    {
        var node=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));node.transform.SetParent(parent,false);
        var rect=node.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(455,22);rect.anchoredPosition=offset;
        var text=node.GetComponent<TextMeshProUGUI>();text.font=font;text.fontSize=size;text.alignment=TextAlignmentOptions.Center;text.color=new Color(.95f,.85f,.48f);text.raycastTarget=false;return text;
    }
    private void Update()
    {
        if(director==null)return;float dt=Time.deltaTime;clock+=dt;
        int desired=director.Section;
        if(desired!=section)
        {
            section=desired;headingAt=clock;title.text=Names[section];
            detail.text=section==0?"ENXAMES NO CEU":section==1?"ATENCAO AS MARGENS E A SUPERFICIE":"ALGO GRANDE SE APROXIMA";
            if(section==2 && distantCroak!=null)sound.PlayOneShot(distantCroak,.22f);
        }
        if(lastWave!=director.Wave)
        {lastWave=director.Wave;if(lastWave==7 || lastWave==9)hintAt=clock;}
        lake.JourneyDensity=Mathf.MoveTowards(lake.JourneyDensity,section*.45f+.1f,dt*.15f);
        lake.TravelScale=Mathf.MoveTowards(lake.TravelScale,director.Calm?.15f:1,dt*.6f);
        float t=clock-headingAt;heading.alpha=waves!=null && waves.Phase!=WaveController.RunPhase.Failed && director.Wave<10?Mathf.Clamp01(t/.3f)*Mathf.Clamp01((4-t)/.7f):0;
        Rect arena=director.Arena;
        for(int i=0;i<props.Length;i++)
        {
            bool visible=section==0?i%4==0 || i%4==3:section==1?i%4!=1:true;
            positions[i]-=.42f*dt*lake.TravelScale;
            if(positions[i]<arena.xMin-2.7f)positions[i]=arena.xMax+2.7f;
            props[i].enabled=visible;
            props[i].transform.position=new Vector3(positions[i],lake.WaterY+(i%4==2?.03f:-.025f)+Mathf.Sin(clock*1.1f+i)*.015f,1);
        }
        float hintTime=clock-hintAt;silhouette.enabled=hintTime>=0 && hintTime<3.4f;
        if(silhouette.enabled)
            silhouette.transform.position=new Vector3(arena.xMax-2.7f,lake.WaterY+.18f-Mathf.SmoothStep(0,1,Mathf.Clamp01((hintTime-1.8f)/1.6f))*1.3f,3);
        if(director.Calm && !wasCalm){calmAt=clock;if(distantCroak!=null)sound.PlayOneShot(distantCroak,.28f);}
        wasCalm=director.Calm;
        float flyTime=clock-calmAt;fly.enabled=director.Calm && flyTime<1.7f && flies!=null;
        if(fly.enabled){fly.sprite=flies.flyFrames[(int)(clock*14)%6];fly.transform.position=new Vector3(Mathf.Lerp(arena.xMin+1,arena.xMax-1,flyTime/1.7f),lake.WaterY+3+Mathf.Sin(flyTime*5)*.3f,0);}
    }
    private void OnDisable()
    {
        if(sound!=null)sound.Stop();if(heading!=null)heading.alpha=0;
        if(silhouette!=null)silhouette.enabled=false;if(fly!=null)fly.enabled=false;
        if(lake!=null){lake.TravelScale=1;lake.JourneyDensity=1;}
    }
}
