using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class LakeEntranceWater : UnityEngine.UI.MaskableGraphic
{
    private float clock;
    public void Present(float value){clock=value;SetVerticesDirty();}
    protected override void Awake(){base.Awake();raycastTarget=false;}
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();float width=rectTransform.rect.width,height=rectTransform.rect.height;
        for(float x=0;x<width;x+=8)
        {
            float right=Mathf.Min(width,x+8),edge=Mathf.Sin(x*.017f-clock*2.3f)*7+Mathf.Sin(x*.041f+clock*3)*3;
            float top=height-12+edge,bottom=12+Mathf.Sin(x*.023f-clock*1.8f)*6;int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(x,bottom),new Color32(8,37,41,250),Vector2.zero);
            mesh.AddVert(new Vector3(x,top),new Color32(9,69,68,250),Vector2.zero);
            mesh.AddVert(new Vector3(right,top),new Color32(9,69,68,250),Vector2.zero);
            mesh.AddVert(new Vector3(right,bottom),new Color32(8,37,41,250),Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            LakeHudWater.Quad(mesh,x,top-2,right-x,2,new Color32(97,152,103,255));
            LakeHudWater.Quad(mesh,x,bottom,right-x,2,new Color32(20,112,113,255));
        }
        for(int i=0;i<22;i++)
        {
            float x=Mathf.Repeat(i*57-clock*(25+i%4*9),width+40)-40;
            float y=24+(i%4)*8+Mathf.Sin(clock*2+i)*2;
            float left=Mathf.Max(0,x),right=Mathf.Min(width,x+18+i%3*7);
            if(right>left)LakeHudWater.Quad(mesh,left,y,right-left,2,new Color32(42,125,123,110));
        }
    }
}
