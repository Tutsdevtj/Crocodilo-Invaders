using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public sealed class LakeTireProjectile : MonoBehaviour
{
    public const float Radius = .40f, Gravity = 9f, MaxAimTime = 1.1f, Restitution = .72f, MinBounceSpeed = 5.2f, MinRollSpeed = 4f;
    private SapoDoPneuCombat owner;
    private LakeTireAttack attack;
    private Health target;
    private Rigidbody2D body;
    private Vector2 velocity;
    private float age, angle;
    private int bounces;
    private bool spent;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    public Vector2 Velocity => velocity;
    public Vector2 LaunchVelocity { get; private set; }
    public float AimTime { get; private set; }
    public int Bounces { get; private set; }
    public bool ContactSpent => spent;
    public float Clock => age;
    public void Configure(SapoDoPneuCombat boss, LakeTireAttack source, Health player, Sprite drawing, Vector2 start, Vector2 aim, float speed, int waterBounces)
    {
        owner = boss;
        attack = source;
        target = player;
        bounces = waterBounces;
        // A ballistic arc through the locked point, so the tire comes down and skips afterwards.
        AimTime = Mathf.Clamp((aim - start).magnitude / speed, .25f, MaxAimTime);
        velocity = LaunchVelocity = LakeDroplet.LaunchThrough(start, aim, AimTime, Gravity);
        transform.position = start;
        angle = transform.eulerAngles.z;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.position = start;
        body.rotation = angle;
        var view = GetComponent<SpriteRenderer>();
        view.sprite = drawing;
        view.sortingOrder = 29;
        filter = LakeHit.PlayerFilter();
        transform.localScale = Vector3.one;
    }
    private void FixedUpdate()
    {
        if (owner == null || !owner.Active || target == null || target.IsDead)
        { Cancel(); return; }
        float dt = Time.fixedDeltaTime;
        age += dt;
        velocity += Vector2.down * Gravity * dt;
        Vector2 previous = body.position, point = previous + velocity * dt;
        if (!spent && LakeHit.Touch(target, previous, point, Radius, filter, contacts))
        {
            // Even a dash consumes this pass. The heavy tire keeps its momentum, with no late hit.
            spent = true;
            bool damaged = target.TakeDamage(owner.Tuning.tireDamage);
            if (attack != null)
                attack.RegisterContact(damaged);
            if (owner == null || !owner.Active)
            { Cancel(); return; }
        }
        if (velocity.y < 0 && point.y <= owner.WaterY)
        {
            LakeWaterFX.Create(new Vector2(point.x, owner.WaterY), .85f);
            if (bounces <= 0)
            { Cancel(); return; }
            bounces--;
            Bounces++;
            LakeAudio.Play(LakeSound.TireBounce);
            point.y = owner.WaterY;
            velocity.y = Mathf.Max(-velocity.y * Restitution, MinBounceSpeed);
            float side = velocity.x < 0 ? -1 : 1;
            velocity.x = side * Mathf.Max(Mathf.Abs(velocity.x), MinRollSpeed);
        }
        body.MovePosition(point);
        angle += dt * (velocity.x < 0 ? 840 : -840);
        body.MoveRotation(angle);
        Rect arena = owner.Arena;
        if (age > 6 || point.x < arena.xMin - 1.1f || point.x > arena.xMax + 1.1f)
            Cancel();
    }
    public void Cancel() { gameObject.SetActive(false); Destroy(gameObject); }
}
