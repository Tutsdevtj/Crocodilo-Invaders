using UnityEngine;

// The telegraph and damage share one segment, with a single hit per activation.
public sealed class MacaquiniHeatHazard : MonoBehaviour
{
    private LineRenderer line;
    private Vector2 from, to;
    private Health target;
    private float fireAt, endAt;
    private float radius;
    private bool hit;
    private LineRenderer canister;
    private Vector2 launch;
    private float startedAt;
    public bool Dangerous => Time.time >= fireAt && Time.time < endAt;
    public void Configure(Vector2 a, Vector2 b, Health player, Material material, float warning, float duration, float width)
    {
        from=a;to=b;target=player;radius=width*.5f;
        fireAt=Time.time+warning;endAt=fireAt+duration;
        line=gameObject.AddComponent<LineRenderer>();line.sharedMaterial=material;
        line.sortingOrder=9;line.positionCount=25;line.numCapVertices=0;
        Draw();
    }
    public void LaunchFuel(Vector2 source)
    {
        launch=source;startedAt=Time.time;
        var node=new GameObject("FuelPod");node.transform.SetParent(transform,false);canister=node.AddComponent<LineRenderer>();
        canister.sharedMaterial=line.sharedMaterial;canister.sortingOrder=38;canister.positionCount=2;canister.startWidth=canister.endWidth=.22f;
        canister.startColor=new Color(.98f,.77f,.12f);canister.endColor=new Color(.25f,.65f,.12f);
    }
    private void Draw()
    {
        bool active=Dangerous;
        if(canister!=null){float u=Mathf.Clamp01((Time.time-startedAt)/Mathf.Max(.01f,fireAt-startedAt));Vector2 p=Vector2.Lerp(launch,from,u)+Vector2.up*Mathf.Sin(u*Mathf.PI)*1.2f;canister.SetPosition(0,p);canister.SetPosition(1,p+Vector2.down*.27f);canister.enabled=!active;}
        line.startWidth=line.endWidth=active?radius*2:.045f;
        Color color=active?new Color(1,.32f,.04f,.9f):new Color(1,.72f,.22f,.6f+.3f*Mathf.Sin(Time.time*15));
        line.startColor=line.endColor=color;
        Vector2 normal=Vector2.Perpendicular((to-from).normalized);
        for(int i=0;i<25;i++){
            float t=i/24f;
            Vector2 p=Vector2.Lerp(from,to,t);
            if(active)p+=normal*Mathf.Sin(i*2+Time.time*35)*.035f;
            line.SetPosition(i,p);
        }
    }
    private void Update()
    {
        if(target==null||target.IsDead||Time.time>=endAt){gameObject.SetActive(false);Destroy(gameObject);return;}
        Draw();if(Time.timeScale<=0||!Dangerous||hit)return;
        Vector2 point=target.transform.position,delta=to-from;
        float t=Mathf.Clamp01(Vector2.Dot(point-from,delta)/Mathf.Max(.0001f,delta.sqrMagnitude));
        foreach(var collider in target.GetComponentsInChildren<Collider2D>()){
            if(!collider.enabled)continue;
            Vector2 closest=collider.ClosestPoint(Vector2.Lerp(from,to,t));
            if(Vector2.Distance(closest,Vector2.Lerp(from,to,t))<=radius){hit=target.TakeDamage(2);if(hit)break;}
        }
    }
}
