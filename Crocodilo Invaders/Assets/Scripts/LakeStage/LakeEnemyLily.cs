using UnityEngine;

// A small floating berth. Its body interpolates just like the passenger, keeping feet registered.
[DefaultExecutionOrder(-5)]
public sealed class LakeEnemyLily : MonoBehaviour
{
    private LakeJourneyDirector director;
    private Rigidbody2D body;
    private SpriteRenderer drawing;
    private float anchor,clock,phase,scale,dip,releasedAt=-1;
    private float Drift => Mathf.Sin(Time.fixedTime*.65f)*.10f;
    private Vector2 nextPosition;
    private float nextAngle;
    public Vector2 Seat
    {
        get
        {
            float height=(drawing.sprite.bounds.min.y+drawing.sprite.bounds.size.y*.43f)*scale;
            float angle=nextAngle*Mathf.Deg2Rad;
            return nextPosition+new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle))*height;
        }
    }
    public bool Arrived => Mathf.Abs(body.position.x-(anchor+Drift))<.045f;
    public bool Released => releasedAt>=0;
    public SpriteRenderer Drawing => drawing;
    public static LakeEnemyLily Create(LakeJourneyDirector owner,Sprite sprite,float startX,float targetX,float width,int slot)
    {
        var node=new GameObject("OccupiedLakeLily",typeof(Rigidbody2D),typeof(SpriteRenderer));node.transform.SetParent(owner.transform,false);
        var pad=node.AddComponent<LakeEnemyLily>();pad.director=owner;pad.anchor=targetX;pad.phase=slot*1.8f;
        pad.body=node.GetComponent<Rigidbody2D>();pad.body.bodyType=RigidbodyType2D.Kinematic;pad.body.gravityScale=0;
        pad.body.interpolation=RigidbodyInterpolation2D.Interpolate;pad.body.position=new Vector2(startX,owner.WaterY-.12f);
        pad.nextPosition=pad.body.position;
        pad.drawing=node.GetComponent<SpriteRenderer>();pad.drawing.sprite=sprite;pad.drawing.sortingOrder=14;
        pad.scale=Mathf.Min(width,owner.Arena.width*.13f)/sprite.bounds.size.x;node.transform.localScale=Vector3.one*pad.scale;return pad;
    }
    public void Land(){dip=.055f;}
    public void Release(){if(!Released)releasedAt=clock;}
    private void FixedUpdate()
    {
        if(director==null || !director.CombatActive){Destroy(gameObject);return;}
        float dt=Time.fixedDeltaTime;clock+=dt;dip*=Mathf.Exp(-dt*6);
        float x=Released?body.position.x-dt*.28f:Mathf.MoveTowards(body.position.x,anchor+Drift,dt*3.2f);
        float bob=Mathf.Sin(clock*1.25f+phase)*.022f+Mathf.Sin(clock*2.1f+phase)*.008f;
        nextPosition=new Vector2(x,director.WaterY-.12f+bob-dip);nextAngle=Mathf.Sin(clock*.9f+phase)*1.1f;
        body.MovePosition(nextPosition);body.MoveRotation(nextAngle);
        if(Released)
        {
            float t=clock-releasedAt;drawing.color=new Color(1,1,1,Mathf.Clamp01(1-t/.85f));
            if(t>=.85f)Destroy(gameObject);
        }
    }
}
