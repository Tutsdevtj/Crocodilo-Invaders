using UnityEngine;

// A stage projectile owns its lifetime independently of the frog that fired it.
[RequireComponent(typeof(Rigidbody2D),typeof(SpriteRenderer))]
public sealed class LakeJourneyDrop : MonoBehaviour
{
    private LakeJourneyDirector director;
    private Rigidbody2D body;
    private Vector2 origin,launch;
    private float age;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts=new Collider2D[8];
    public Vector2 Velocity => launch+Vector2.down*LakeDroplet.Gravity*age;
    public static LakeJourneyDrop Fire(LakeJourneyDirector owner,Vector2 start,Vector2 velocity)
    {
        var node=new GameObject("SapinhoWaterDrop",typeof(Rigidbody2D),typeof(SpriteRenderer));node.transform.SetParent(owner.Threats,false);
        var drop=node.AddComponent<LakeJourneyDrop>();drop.director=owner;drop.origin=start;drop.launch=velocity;
        drop.body=node.GetComponent<Rigidbody2D>();drop.body.bodyType=RigidbodyType2D.Kinematic;drop.body.gravityScale=0;drop.body.position=start;
        drop.body.interpolation=RigidbodyInterpolation2D.Interpolate;drop.filter=LakeHit.PlayerFilter();
        var view=node.GetComponent<SpriteRenderer>();view.sprite=owner.DropSprite;view.sortingOrder=34;
        node.transform.localScale=Vector3.one*(.40f/Mathf.Max(.01f,view.sprite.bounds.size.y));
        drop.Rotate();return drop;
    }
    private void Rotate(){transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(Velocity.y,Velocity.x)*Mathf.Rad2Deg+90);}
    private void FixedUpdate()
    {
        if(director==null || !director.CombatActive){Destroy(gameObject);return;}
        Vector2 previous=body.position;age+=Time.fixedDeltaTime;
        Vector2 point=LakeDroplet.Point(origin,launch,age);
        if(LakeHit.Touch(director.Target,previous,point,.13f,filter,contacts)){director.Hit();Destroy(gameObject);return;}
        body.MovePosition(point);Rotate();
        if(Velocity.y<0 && point.y<=director.WaterY)
        {LakeWaterFX.Create(director.Threats,new Vector2(point.x,director.WaterY),.3f);Destroy(gameObject);}
        else if(age>4 || point.x<director.Arena.xMin-.8f || point.x>director.Arena.xMax+.8f)Destroy(gameObject);
    }
}
