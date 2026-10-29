using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public sealed class LakeDroplet : MonoBehaviour
{
    private SapoDoPneuCombat owner;
    private Health target;
    private Rigidbody2D body;
    private Vector2 origin, velocity;
    private float age, waterY;
    private bool spent;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    public const float Gravity = 7.4f;
    public const float Radius = .15f;
    public Vector2 Velocity => velocity + Vector2.down * Gravity * age;
    public bool Returning => Velocity.y < 0;
    public void Configure(SapoDoPneuCombat source, Health player, Sprite sprite, Vector2 start, Vector2 launch, float surface)
    {
        owner = source; target = player; origin = start; velocity = launch; waterY = surface;
        body = GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.position = start; filter = LakeHit.PlayerFilter();
        var view = GetComponent<SpriteRenderer>(); view.sprite = sprite; view.sortingOrder = 34;
        float height = sprite != null ? sprite.bounds.size.y : 1;
        view.transform.localScale = Vector3.one * (.72f / Mathf.Max(.01f, height));
    }
    public static Vector2 Point(Vector2 start, Vector2 launch, float time)
        => start + launch * time + Vector2.down * (.5f * Gravity * time * time);
    private void FixedUpdate()
    {
        if (spent) return;
        if (owner == null || !owner.Active || target == null || target.IsDead) { Destroy(gameObject); return; }
        Vector2 previous = body.position;
        age += Time.fixedDeltaTime;
        Vector2 point = Point(origin, velocity, age);
        if (LakeHit.Touch(target, previous, point, Radius, filter, contacts))
        {
            // A drop is spent even against dash invulnerability: it never waits inside the ship.
            spent = true; target.TakeDamage(1); Destroy(gameObject); return;
        }
        body.MovePosition(point);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg - 90);
        if (Returning && point.y <= waterY)
        {
            LakeWaterFX.Create(transform.parent, new Vector2(point.x, waterY), .42f);
            owner.RegisterReturnedDrop(); Destroy(gameObject);
        }
        else if (age > 5 || point.x < owner.Arena.xMin - 1 || point.x > owner.Arena.xMax + 1) Destroy(gameObject);
    }
}
