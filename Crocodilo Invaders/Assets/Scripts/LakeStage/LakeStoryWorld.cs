using UnityEngine;

// Scenic story landmarks have no collider, damage or score ownership.
public sealed class LakeStoryWorld : MonoBehaviour
{
    [SerializeField] private LakeStoryAssets art;
    [SerializeField] private LakeBackdrop lake;
    [SerializeField] private WaveController waves;
    private SpriteRenderer intake,outlet,nursery,plug,wife;
    private LineRenderer stream;
    private bool opening,finale;
    private float progress,clock,nextSplash;
    public bool ChannelOpen => progress>=1;
    public bool NurserySafe => nursery!=null && nursery.enabled;
    public float OpeningProgress => progress;
    private SpriteRenderer Make(string name,Sprite sprite,float width,int order)
    {
        var view=new GameObject(name).AddComponent<SpriteRenderer>();view.transform.SetParent(transform,false);
        view.sprite=sprite;view.sortingOrder=order;
        view.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);return view;
    }
    private void Start()
    {
        if(art==null || art.props.Length<3)return;
        intake=Make("DryIntake",art.props[0],2.5f,-174);
        outlet=Make("SideChannel",art.props[1],2.9f,-173);
        nursery=Make("TadpoleNursery",art.props[2],2.1f,-172);
        var journey=Resources.Load<LakeJourneyArt>("LakeJourneyV22/art");
        if(journey!=null)plug=Make("ChannelDebris",journey.props[1],1.1f,-171);
        stream=new GameObject("RestoredWater").AddComponent<LineRenderer>();stream.transform.SetParent(transform,false);
        stream.sharedMaterial=lake.WaterFXMaterial;stream.positionCount=24;stream.sortingOrder=-170;
        stream.widthMultiplier=.17f;stream.startColor=new Color(.2f,.65f,.75f,1);stream.endColor=new Color(.55f,.87f,.92f,0);
    }
    private void LateUpdate()
    {
        if(intake==null || lake==null)return;
        clock+=Time.deltaTime;
        if(opening)progress=Mathf.MoveTowards(progress,1,Time.deltaTime/1.7f);
        var camera=lake.GameplayCamera;
        float width=camera.orthographicSize*2*camera.aspect,left=camera.transform.position.x-width*.5f;
        intake.enabled=waves.CurrentWave<=3 && !finale;
        outlet.enabled=waves.CurrentWave>=4 || finale;nursery.enabled=waves.CurrentWave>=7 || finale;
        intake.transform.position=new Vector3(left+width*.2f,lake.WaterY+.05f,1);
        outlet.transform.position=new Vector3(left+width*.56f,lake.WaterY+.05f,1);
        nursery.transform.position=new Vector3(left+width*.89f,lake.WaterY+.06f+Mathf.Sin(clock*1.4f)*.025f,1);
        if(plug!=null){plug.enabled=outlet.enabled && progress<1;plug.transform.position=outlet.transform.position+new Vector3(progress*1.4f,-.02f-progress*.65f,-.01f);plug.transform.rotation=Quaternion.Euler(0,0,-progress*110);}
        stream.enabled=progress>0;
        if(stream.enabled)
        {
            for(int i=0;i<24;i++){float u=i/23f;stream.SetPosition(i,outlet.transform.position+new Vector3(-u*3*progress,-.1f-u*.18f+Mathf.Sin(u*14-clock*5)*.035f,0));}
            if(progress<1 && clock>nextSplash){nextSplash=clock+.3f;LakeWaterFX.Create(transform,outlet.transform.position,.35f);}
        }
        if(wife!=null)wife.transform.position=new Vector3(left+width*.82f,lake.WaterY+.08f+Mathf.Sin(clock*1.8f)*.025f,0);
    }
    public void ShowFinale(Boss boss)
    {
        finale=true;
        var companion=Resources.Load<SapererecaAssets>("SapererecaV12/art");
        if(wife==null && companion!=null)wife=Make("SapererecaGuardsNursery",companion.frames[0],2.1f,19);
    }
    public void OpenChannel(bool instant){opening=true;if(instant)progress=1;}
}
