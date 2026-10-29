using UnityEngine;

// Damage drains horizontally from the moving right edge, with a bright continuous jet.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LakeHudSpill : UnityEngine.UI.MaskableGraphic
{
    private struct Drop { public Vector2 origin,velocity; public float born,life,size; }
    private readonly Drop[] drops=new Drop[128];
    private int cursor;
    private float clock,level,until,start,strength;
    public int Bursts { get; private set; }
    public float LostVolume { get; private set; }
    public bool IsPouring => clock<until;
    public Vector2 Outlet => Origin(1);
    public override Texture mainTexture => Texture2D.whiteTexture;
    protected override void Awake() { base.Awake(); raycastTarget=false; }
    public void ResetWater() { Bursts=cursor=0; LostVolume=clock=until=strength=0; System.Array.Clear(drops,0,drops.Length); SetVerticesDirty(); }
    public void Release(float amount,float time,float surfaceLevel)
    {
        clock=time; level=surfaceLevel; Bursts++; LostVolume+=amount;
        start=clock; until=Mathf.Max(until,clock+.6f+amount*1.6f); strength=Mathf.Clamp(strength+amount*3.2f,.25f,1);
        int count=Mathf.Clamp(10+Mathf.CeilToInt(amount*110),10,90);
        for(int i=0;i<count;i++)
        {
            float noise=Mathf.Repeat((i+Bursts*17)*.6180339f,1);
            drops[cursor]=new Drop { origin=Origin(1),velocity=new Vector2(32+noise*46,2+noise*9),born=clock+i*.005f,life=.45f+noise*.55f,size=1.3f+noise*2.3f };
            cursor=(cursor+1)%drops.Length;
        }
        SetVerticesDirty();
    }
    public void Present(float time,float surfaceLevel) { clock=time; level=surfaceLevel; if(!IsPouring)strength=0; SetVerticesDirty(); }
    private Vector2 Origin(float direction) => new Vector2(43+292*level,48);
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        float fade=Mathf.Clamp01((until-clock)/.25f)*Mathf.Clamp01((clock-start)/.07f);
        if(fade>0)
        {
            for(int side=1;side<=1;side++)
            {
                var origin=Origin(side);
                for(int i=0;i<22;i++)
                {
                    float t=i/22f, next=(i+1)/22f;
                    Vector2 a=Jet(origin,side,t), b=Jet(origin,side,next);
                    float width=(1.8f+strength*3.8f)*(1-t*.72f)*fade;
                    Ribbon(mesh,a,b,width,new Color32(23,141,153,230));
                    Ribbon(mesh,a,b,width*.45f,new Color32(160,246,227,245));
                }
            }
        }
        foreach(var drop in drops)
        {
            float age=clock-drop.born; if(drop.life<=0 || age<0 || age>drop.life)continue;
            Vector2 p=drop.origin+drop.velocity*age+Vector2.down*(65*age*age);
            byte alpha=(byte)(255*Mathf.Clamp01((drop.life-age)/.2f));
            LakeHudWater.Quad(mesh,Mathf.Round(p.x),Mathf.Round(p.y),drop.size,drop.size*1.5f,new Color32(52,194,195,alpha));
            LakeHudWater.Quad(mesh,Mathf.Round(p.x),Mathf.Round(p.y)+drop.size,1,1,new Color32(190,255,237,alpha));
        }
    }
    private Vector2 Jet(Vector2 origin,float side,float t)
    {
        return origin+new Vector2((24+strength*38)*t,-5*t*t-14*t*t*t)+Vector2.up*(Mathf.Sin(t*19-clock*22)*.65f*t);
    }
    private static void Ribbon(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,float width,Color32 color)
    {
        Vector2 normal=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
        int n=mesh.currentVertCount;
        mesh.AddVert(a-normal,color,Vector2.zero);mesh.AddVert(a+normal,color,Vector2.zero);
        mesh.AddVert(b+normal,color,Vector2.zero);mesh.AddVert(b-normal,color,Vector2.zero);
        mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
    }
}
