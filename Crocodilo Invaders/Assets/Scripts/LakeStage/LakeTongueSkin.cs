using UnityEngine;

// One connected, textured surface. Root and sticky bulb keep their proportions.
public sealed class LakeTongueSkin : MonoBehaviour
{
    private const int RootSteps=8,BodySteps=48,TipSteps=12,Rings=RootSteps+BodySteps+TipSteps+1;
    private readonly Vector3[] vertices=new Vector3[Rings*2];
    private readonly Vector2[] uvs=new Vector2[Rings*2];
    private readonly Color32[] colors=new Color32[Rings*2];
    private readonly int[] triangles=new int[(Rings-1)*6];
    private Mesh mesh;
    private MeshRenderer drawing;
    private LakeTongueSkinData data;
    private Rect uvRect;
    private Color32 tint=new Color32(255,255,255,255);
    public bool Visible => drawing!=null && drawing.enabled;
    public Vector2 RootPoint => transform.TransformPoint((vertices[0]+vertices[1])*.5f);
    public Vector2 EndPoint => transform.TransformPoint((vertices[vertices.Length-2]+vertices[vertices.Length-1])*.5f);
    public int SurfaceCount => drawing!=null?1:0;
    public void Configure(LakeTongueSkinData source,int layer,int order)
    {
        data=source;var filter=gameObject.AddComponent<MeshFilter>();drawing=gameObject.AddComponent<MeshRenderer>();
        drawing.sharedMaterial=data.material;drawing.sortingLayerID=layer;drawing.sortingOrder=order;drawing.enabled=false;
        mesh=new Mesh { name="ContinuousFrogTongue" };mesh.MarkDynamic();filter.sharedMesh=mesh;
        Rect rect=data.drawing.textureRect;var texture=data.drawing.texture;
        uvRect=new Rect(rect.x/texture.width,rect.y/texture.height,rect.width/texture.width,rect.height/texture.height);
        for(int i=0;i<Rings-1;i++)
        {
            int p=i*2,t=i*6;triangles[t]=p;triangles[t+1]=p+1;triangles[t+2]=p+2;
            triangles[t+3]=p+2;triangles[t+4]=p+1;triangles[t+5]=p+3;
        }
        mesh.vertices=vertices;mesh.uv=uvs;mesh.colors32=colors;mesh.triangles=triangles;
    }
    public void Draw(Vector2 start,Vector2 end,float time,bool retracting,bool mirrored,Color color)
    {
        if(data==null)return;
        tint=color;
        float length=Vector2.Distance(start,end);if(length<.12f) { Hide();return; }
        Vector2 direction=(end-start)/length;
        Vector2 normal=new Vector2(direction.y,-direction.x)*(mirrored?-1:1);
        float root=Mathf.Min(data.rootLength,length*.35f),tip=Mathf.Min(data.tipLength,length*.45f),body=length-root-tip;
        int ring=0;
        WriteRing(ring++,0,1,start,end,normal,length,time,retracting);
        for(int i=1;i<=RootSteps;i++)WriteRing(ring++,root*i/RootSteps,Mathf.Lerp(1,data.rootUv,i/(float)RootSteps),start,end,normal,length,time,retracting);
        for(int i=1;i<=BodySteps;i++)WriteRing(ring++,root+body*i/BodySteps,Mathf.Lerp(data.rootUv,data.tipUv,i/(float)BodySteps),start,end,normal,length,time,retracting);
        for(int i=1;i<=TipSteps;i++)WriteRing(ring++,root+body+tip*i/TipSteps,Mathf.Lerp(data.tipUv,0,i/(float)TipSteps),start,end,normal,length,time,retracting);
        mesh.vertices=vertices;mesh.uv=uvs;mesh.colors32=colors;mesh.RecalculateBounds();drawing.enabled=true;
    }
    private void WriteRing(int ring,float distance,float u,Vector2 start,Vector2 end,Vector2 normal,float length,float time,bool retracting)
    {
        float t=distance/length;
        float bend=Mathf.Sin(t*Mathf.PI)*Mathf.Sin(time*11)*(retracting?.085f:.018f);
        Vector2 point=Vector2.Lerp(start,end,t)+normal*bend;
        float height=data.height*(retracting?1.035f:1);
        vertices[ring*2]=transform.InverseTransformPoint(point-normal*height*.5f);
        vertices[ring*2+1]=transform.InverseTransformPoint(point+normal*height*.5f);
        float x=uvRect.x+u*uvRect.width;
        uvs[ring*2]=new Vector2(x,uvRect.y);uvs[ring*2+1]=new Vector2(x,uvRect.yMax);
        byte alpha=(byte)(tint.a*Mathf.Clamp01(distance/.075f));
        colors[ring*2]=colors[ring*2+1]=new Color32(tint.r,tint.g,tint.b,alpha);
    }
    public void Hide() { if(drawing!=null)drawing.enabled=false; }
    private void OnDisable() { Hide(); }
    private void OnDestroy() { if(mesh!=null)Destroy(mesh); }
}
