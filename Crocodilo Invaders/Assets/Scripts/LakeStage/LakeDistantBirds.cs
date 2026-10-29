using UnityEngine;

// Tiny distant silhouettes; no colliders, gameplay events or sounds.
public sealed class LakeDistantBirds : MonoBehaviour
{
    private sealed class Bird
    {
        public LineRenderer line;
        public Vector2 origin;
        public float start,span,phase;
        public bool active;
    }
    private readonly Bird[] birds=new Bird[5];
    private readonly Vector3[] points=new Vector3[5];
    private LakeBackdrop lake;
    private float clock,nextFlock=1.2f,direction=1,speed=.8f;
    private int flock;
    private void Start()
    {
        lake=GetComponent<LakeBackdrop>();if(lake==null || lake.WaterFXMaterial==null){enabled=false;return;}
        for(int i=0;i<birds.Length;i++)
        {
            var line=new GameObject("DistantLakeBird_"+i).AddComponent<LineRenderer>();line.transform.SetParent(transform,false);
            line.sharedMaterial=lake.WaterFXMaterial;line.useWorldSpace=false;line.positionCount=5;
            line.sortingOrder=-225;line.startWidth=line.endWidth=.018f;line.numCapVertices=line.numCornerVertices=0;
            line.startColor=line.endColor=new Color(.19f,.36f,.41f,.62f);line.enabled=false;
            birds[i]=new Bird{line=line};
        }
    }
    private void Update()
    {
        if(lake==null || birds[0]==null)return;
        float dt=Time.deltaTime;if(dt<=0)return;clock+=dt;
        var camera=lake.GameplayCamera;float height=camera.orthographicSize*2,width=height*camera.aspect;
        float left=camera.transform.position.x-width*.5f,right=left+width;
        bool any=false;
        foreach(var bird in birds)if(bird.active)
        {
            float age=clock-bird.start;float x=bird.origin.x+direction*speed*age;
            if((direction>0 && x>right+.8f) || (direction<0 && x<left-.8f)){bird.active=false;bird.line.enabled=false;continue;}
            any=true;bird.line.enabled=true;float y=bird.origin.y+Mathf.Sin(age*.5f+bird.phase)*.045f;
            bird.line.transform.position=new Vector3(x,y,3.8f);
            float wing=Mathf.Sin(age*7.8f+bird.phase)*.065f;
            float pixel=height/Mathf.Max(1,camera.pixelHeight);
            points[0]=Snap(new Vector3(-bird.span,wing),pixel);
            points[1]=Snap(new Vector3(-bird.span*.48f,wing*.64f+.018f),pixel);
            points[2]=Vector3.zero;
            points[3]=Snap(new Vector3(bird.span*.48f,wing*.64f+.018f),pixel);
            points[4]=Snap(new Vector3(bird.span,wing),pixel);bird.line.SetPositions(points);
        }
        if(any){nextFlock=clock+7+(flock%3)*2;return;}
        if(clock<nextFlock)return;
        direction=flock%2==0?1:-1;speed=.72f+(flock%3)*.1f;
        float origin=direction>0?left-.35f:right+.35f;
        float altitude=camera.transform.position.y+height*(.22f+(flock%3)*.045f);
        int count=3+flock%3;
        for(int i=0;i<count;i++)
        {
            var bird=birds[i];bird.active=true;bird.start=clock;
            bird.origin=new Vector2(origin-direction*i*.25f,altitude+(i%2==0?1:-1)*(i*.09f));
            bird.span=.07f+(i%3)*.012f;bird.phase=i*1.7f;bird.line.enabled=true;
        }
        flock++;
    }
    private static Vector3 Snap(Vector3 p,float pixel) => new Vector3(Mathf.Round(p.x/pixel)*pixel,Mathf.Round(p.y/pixel)*pixel,0);
    private void OnDisable(){foreach(var bird in birds)if(bird!=null && bird.line!=null)bird.line.enabled=false;}
}
