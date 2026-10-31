using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(SpriteRenderer))]
public sealed class LakeTireProjectile : MonoBehaviour
{
    public const float Radius=.40f;
    private SapoDoPneuCombat owner;
    private LakeTireAttack attack;
    private Health target;
    private Rigidbody2D body;
    private Vector2 direction;
    private float speed,age,angle;
    private bool spent;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts=new Collider2D[8];
    public Vector2 Velocity => direction*speed;
    public bool ContactSpent => spent;
    public float Clock => age;
    public void Configure(SapoDoPneuCombat boss,LakeTireAttack source,Health player,Sprite drawing,Vector2 start,Vector2 aim,float velocity)
    {
        owner=boss;attack=source;target=player;speed=velocity;direction=(aim-start).normalized;
        if(direction.sqrMagnitude<.5f)direction=Vector2.left;
        transform.position=start;angle=transform.eulerAngles.z;
        body=GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.position=start;body.rotation=angle;
        var view=GetComponent<SpriteRenderer>();view.sprite=drawing;view.sortingOrder=29;
        filter=LakeHit.PlayerFilter();transform.localScale=Vector3.one;
    }
    private void FixedUpdate()
    {
        if(owner==null || !owner.Active || target==null || target.IsDead){Cancel();return;}
        float dt=Time.fixedDeltaTime;age+=dt;Vector2 previous=body.position,point=previous+Velocity*dt;
        if(!spent && LakeHit.Touch(target,previous,point,Radius,filter,contacts))
        {
            // Even a dash consumes this pass. The heavy tire keeps its momentum, with no late hit.
            spent=true;bool damaged=target.TakeDamage(2);if(attack!=null)attack.RegisterContact(damaged);
            if(owner==null || !owner.Active){Cancel();return;}
        }
        body.MovePosition(point);angle+=dt*(direction.x<0?840:-840);body.MoveRotation(angle);
        Rect arena=owner.Arena;
        if(direction.y<0 && point.y<=owner.WaterY)
        {LakeWaterFX.Create(transform.parent,new Vector2(point.x,owner.WaterY),.85f);Cancel();return;}
        if(age>4 || point.x<arena.xMin-1.1f || point.x>arena.xMax+1.1f || point.y<arena.yMin-1.1f || point.y>arena.yMax+1.1f)Cancel();
    }
    public void Cancel(){gameObject.SetActive(false);Destroy(gameObject);}
}
