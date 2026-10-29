using UnityEngine;

// A shallow animated reservoir, with real vertical level rather than a shrinking strip.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LakeHudWater : UnityEngine.UI.MaskableGraphic
{
    private float clock, level, hit;
    public float Level => level;
    public float SurfaceSample => Surface(61);
    public override Texture mainTexture => Texture2D.whiteTexture;
    protected override void Awake() { base.Awake(); raycastTarget=false; }
    public void Present(float time,float fraction,float impact)
    {
        clock=time; level=Mathf.Clamp01(fraction); hit=impact; SetVerticesDirty();
    }
    private float Surface(float x)
    {
        float ripple=(Mathf.Sin(x*.10f+clock*4.8f)*.48f+Mathf.Sin(x*.23f-clock*7.2f)*.25f)*(1+hit*1.6f);
        return Mathf.Clamp(.5f+16f*level+ripple*Mathf.Min(1,level*5),.5f,17.7f);
    }
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear(); if(level<=0)return;
        float width=rectTransform.rect.width;
        for(float x=0;x<width;x+=2)
        {
            float top=Surface(x), right=Mathf.Min(width,x+2);
            int n=mesh.currentVertCount;
            var deep=new Color32(8,81,94,255); var aqua=new Color32(26,164,158,255);
            mesh.AddVert(new Vector3(x,.3f),deep,Vector2.zero); mesh.AddVert(new Vector3(x,top),aqua,Vector2.zero);
            mesh.AddVert(new Vector3(right,Surface(right)),aqua,Vector2.zero); mesh.AddVert(new Vector3(right,.3f),deep,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2); mesh.AddTriangle(n,n+2,n+3);
            Quad(mesh,x,Mathf.Max(.3f,top-1),right-x,1,new Color32(147,239,214,255));
        }
        for(int i=0;i<19;i++)
        {
            float x=Mathf.Repeat(i*19-clock*(21+i%3*5),width+18)-18;
            float y=Surface(Mathf.Max(0,x))*(.22f+(i%3)*.22f);
            float length=7+i%4*3, left=Mathf.Max(0,x), right=Mathf.Min(width,x+length);
            if(right>left && level>.035f)Quad(mesh,left,Mathf.Floor(y),right-left,1,new Color32(59,190,184,210));
        }
        for(int i=0;i<7;i++)
        {
            float x=Mathf.Repeat(i*43-clock*12,width);
            float y=Mathf.Repeat(clock*5+i*2.7f,18);
            if(y+2<Surface(x))Quad(mesh,x,y,1,2,new Color32(138,228,211,190));
        }
    }
    public static void Quad(UnityEngine.UI.VertexHelper mesh,float x,float y,float w,float h,Color32 c)
    {
        int n=mesh.currentVertCount;
        mesh.AddVert(new Vector3(x,y),c,Vector2.zero); mesh.AddVert(new Vector3(x,y+h),c,Vector2.zero);
        mesh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero); mesh.AddVert(new Vector3(x+w,y),c,Vector2.zero);
        mesh.AddTriangle(n,n+1,n+2); mesh.AddTriangle(n,n+2,n+3);
    }
}
