using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public sealed class LakeDroplet : MonoBehaviour
{
    private SapoDoPneuCombat owner;
    private Health target;
    private SapererecaCompanion helper;
    private Rigidbody2D body;
    private Vector2 origin, velocity;
    private float age, waterY;
    private bool spent;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    private SpriteRenderer drawing;
    private LineRenderer trail;
    private readonly Vector3[] history = new Vector3[4];
    private float drawingScale;
    public Sprite Drawing => drawing != null ? drawing.sprite : null;
    public Vector2 FrontDirection => -transform.up;
    public const float Gravity = 7.4f;
    public const float Radius = .15f;
    public Vector2 Velocity => velocity + Vector2.down * Gravity * age;
    public bool Returning => Velocity.y < 0;
    public void Configure(SapoDoPneuCombat source, Health player, Sprite sprite, Vector2 start, Vector2 launch, float surface, SapererecaCompanion assistant = null)
    {
        owner = source; target = player; origin = start; velocity = launch; waterY = surface; helper = assistant;
        body = GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.position = start; filter = LakeHit.PlayerFilter();
        // The rounded mass is drawn below the tip, so local down is the leading side.
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(launch.y, launch.x) * Mathf.Rad2Deg + 90);
        drawing = GetComponent<SpriteRenderer>(); drawing.sprite = sprite; drawing.sortingOrder = 34;
        float height = sprite != null ? sprite.bounds.size.y : 1;
        drawingScale = .76f / Mathf.Max(.01f, height);
        transform.localScale = Vector3.one * drawingScale;
        var lake = FindAnyObjectByType<LakeBackdrop>();
        if (lake != null && lake.WaterFXMaterial != null)
        {
            trail = new GameObject("LiquidWaterStreak").AddComponent<LineRenderer>(); trail.transform.SetParent(transform, false);
            trail.sharedMaterial = lake.WaterFXMaterial; trail.useWorldSpace = true; trail.positionCount = history.Length;
            trail.startWidth = .035f; trail.endWidth = 0; trail.sortingOrder = 33;
            trail.startColor = new Color(.35f, .74f, .76f, .5f); trail.endColor = new Color(.35f, .74f, .76f, 0);
            for (int i = 0; i < history.Length; i++) history[i] = start;
            trail.SetPositions(history);
        }
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
            spent = true; if(helper!=null)helper.TryHit();else target.TakeDamage(1); Destroy(gameObject); return;
        }
        body.MovePosition(point);
        if (trail != null)
        {
            for (int i = history.Length - 1; i > 0; i--) history[i] = history[i - 1];
            history[0] = point; trail.SetPositions(history);
        }
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg + 90);
        if (Returning && point.y <= waterY)
        {
            LakeWaterFX.Create(transform.parent, new Vector2(point.x, waterY), .42f);
            owner.RegisterReturnedDrop(); Destroy(gameObject);
        }
        else if (age > 5 || point.x < owner.Arena.xMin - 1 || point.x > owner.Arena.xMax + 1) Destroy(gameObject);
    }
    private void LateUpdate()
    {
        if (drawing == null || Time.timeScale <= 0) return;
        float stretch = Mathf.SmoothStep(.78f, 1.16f, Mathf.Clamp01(Velocity.magnitude / 8));
        transform.localScale = new Vector3(drawingScale / Mathf.Sqrt(stretch), drawingScale * stretch, drawingScale);
    }
}
