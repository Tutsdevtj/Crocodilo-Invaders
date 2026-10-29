using UnityEngine;

// The fly is the warning, never a projectile or an obstacle to the player's shots.
public sealed class LakeTongueFly : MonoBehaviour
{
    private const float VisualScale=.65f;
    private LakeTongueAssets art;
    private LakeTongueAttack owner;
    private SpriteRenderer drawing;
    private Vector2 anchor, offset;
    private float clock, appeared;
    private int index;
    public bool Locked { get; private set; }
    public bool Caught { get; private set; }
    public bool Present => !Caught;
    public Vector2 LockedPoint { get; private set; }
    public int Frame { get; private set; }
    public bool Focused { get; set; }
    public bool IsRearFly => index==0;
    public bool IsAgitating => Focused && !Caught;
    public Vector2 Anchor => anchor;
    public void Configure(LakeTongueAssets assets,LakeTongueAttack attack,Vector2 station,int slot)
    {
        art=assets; owner=attack; index=slot;
        drawing=gameObject.AddComponent<SpriteRenderer>(); drawing.sortingOrder=35;
        drawing.sprite=art.flyFrames[0];anchor=station;Caught=true;drawing.enabled=false;
        transform.localScale=Vector3.one*VisualScale;
    }
    public void Place(Vector2 station)
    {
        anchor=station;offset=Vector2.zero;transform.position=anchor;Revive();
    }
    public void Tick(float dt)
    {
        clock+=dt;
        if(Caught)return;
        appeared+=dt;
        if(Locked) { transform.position=LockedPoint; }
        else if(Focused)
        {
            float flutter=Locked?.008f:.22f;
            Vector2 desired=new Vector2(Mathf.Sin(clock*23+index)*flutter,Mathf.Cos(clock*19+index)*flutter);
            offset=Vector2.Lerp(offset,desired,1-Mathf.Exp(-dt*22));
        }
        else
        {
            offset=new Vector2(Mathf.Sin(clock*1.65f+index*2.1f)*.42f+Mathf.Sin(clock*3.3f+index)*.09f,
                Mathf.Cos(clock*1.3f+index*1.7f)*.24f+Mathf.Sin(clock*2.7f+index)*.06f);
        }
        if(!Locked)transform.position=anchor+offset;
        Frame=(int)(clock*(Focused?30:10)+index*2)%art.flyFrames.Length; drawing.sprite=art.flyFrames[Frame];
        drawing.flipX=owner.Mouth.x<transform.position.x;
        float pulse=Focused?1.06f+Mathf.Sin(clock*20)*.025f:1;
        transform.localScale=Vector3.one*(pulse*VisualScale);
        drawing.color=Focused?Color.white:new Color(.88f,.93f,.90f,Mathf.Clamp01(appeared/.18f));
    }
    public void Lock() { LockedPoint=transform.position; Locked=true; }
    public void ReleaseFocus() { Focused=Locked=false; }
    public void Revive() { Caught=false;Focused=Locked=false;offset=Vector2.zero;appeared=0;if(drawing!=null)drawing.enabled=true; }
    public void Catch() { Caught=true;Focused=false;Locked=false;drawing.enabled=false; }
}
