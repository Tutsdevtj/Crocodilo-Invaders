using UnityEngine;

// Small surface bubbles mark the emergence; the pulsing ring marks a locked shot, never a hitbox.
public sealed class LakeDiveBubbles : MonoBehaviour
{
    private readonly LineRenderer[] rings=new LineRenderer[7];
    private float clock;
    private Vector2 surface,aim;
    private bool aiming;
    public static LakeDiveBubbles Create(Transform parent,Vector2 point,Material material)
    {
        var node=new GameObject("SapererecaDiveBubbles");node.transform.SetParent(parent,false);
        var fx=node.AddComponent<LakeDiveBubbles>();fx.surface=point;
        for(int i=0;i<fx.rings.Length;i++)
        {
            var ring=new GameObject(i==6?"LockedWaterAim":"RisingBubble").AddComponent<LineRenderer>();
            ring.transform.SetParent(node.transform,false);ring.sharedMaterial=material;ring.useWorldSpace=true;
            ring.positionCount=13;ring.startWidth=ring.endWidth=i==6?.025f:.018f;ring.sortingOrder=28;
            ring.enabled=i<6;fx.rings[i]=ring;
        }
        return fx;
    }
    public void ShowAim(Vector2 point){aim=point;aiming=true;}
    private void Update()
    {
        clock+=Time.deltaTime;
        for(int i=0;i<rings.Length;i++)
        {
            var ring=rings[i];if(ring==null)continue;ring.enabled=i<6 || aiming;
            float t=Mathf.Repeat(clock*1.2f+i*.17f,1);
            Vector2 center=i==6?aim:surface+new Vector2(Mathf.Sin(i*2.4f+clock*2)*.35f,-.65f+t*.85f);
            float radius=i==6?.32f+Mathf.Sin(clock*14)*.045f:.035f+t*.045f;
            ring.startColor=ring.endColor=new Color(.5f,.92f,1,i==6?.7f:Mathf.Sin(t*Mathf.PI)*.65f);
            for(int p=0;p<13;p++){float a=p*Mathf.PI/6;ring.SetPosition(p,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
        }
    }
}
