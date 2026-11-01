using UnityEngine;

// Small surface bubbles mark the emergence without drawing a target over the ship.
public sealed class LakeDiveBubbles : MonoBehaviour
{
    private readonly LineRenderer[] rings=new LineRenderer[6];
    private float clock;
    private Vector2 surface;
    public static LakeDiveBubbles Create(Transform parent,Vector2 point,Material material)
    {
        var node=new GameObject("SapererecaDiveBubbles");node.transform.SetParent(parent,false);
        var fx=node.AddComponent<LakeDiveBubbles>();fx.surface=point;
        for(int i=0;i<fx.rings.Length;i++)
        {
            var ring=new GameObject("RisingBubble").AddComponent<LineRenderer>();
            ring.transform.SetParent(node.transform,false);ring.sharedMaterial=material;ring.useWorldSpace=true;
            ring.positionCount=13;ring.startWidth=ring.endWidth=.018f;ring.sortingOrder=28;
            fx.rings[i]=ring;
        }
        return fx;
    }
    private void Update()
    {
        clock+=Time.deltaTime;
        for(int i=0;i<rings.Length;i++)
        {
            var ring=rings[i];if(ring==null)continue;
            float t=Mathf.Repeat(clock*1.2f+i*.17f,1);
            Vector2 center=surface+new Vector2(Mathf.Sin(i*2.4f+clock*2)*.35f,-.65f+t*.85f);
            float radius=.035f+t*.045f;
            ring.startColor=ring.endColor=new Color(.5f,.92f,1,Mathf.Sin(t*Mathf.PI)*.65f);
            for(int p=0;p<13;p++){float a=p*Mathf.PI/6;ring.SetPosition(p,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
        }
    }
}
