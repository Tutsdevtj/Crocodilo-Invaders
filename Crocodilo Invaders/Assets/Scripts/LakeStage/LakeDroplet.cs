using UnityEngine;

// Whoever fires a drop decides what a hit means and how long the drop may live.
public interface ILakeDropOwner
{
    bool DropsLive { get; }
    Health DropTarget { get; }
    float DropWaterY { get; }
    Rect DropArena { get; }
    void DropStarted();
    void DropEnded();
    void DropHit();
    void DropReturned();
}

public readonly struct LakeDropStyle
{
    public readonly float Height, Radius, SplashSize, ArenaMargin, MaxAge;
    public readonly bool Trail, Stretch;
    private LakeDropStyle(float height, float radius, float splashSize, float arenaMargin, float maxAge, bool trail, bool stretch)
    {
        Height = height;
        Radius = radius;
        SplashSize = splashSize;
        ArenaMargin = arenaMargin;
        MaxAge = maxAge;
        Trail = trail;
        Stretch = stretch;
    }
    public static readonly LakeDropStyle Boss = new LakeDropStyle(.76f, .15f, .42f, 1, 5, true, true);
    public static readonly LakeDropStyle Helper = new LakeDropStyle(.40f, .15f, .42f, 1, 5, false, true);
    public static readonly LakeDropStyle Journey = new LakeDropStyle(.40f, .13f, .3f, .8f, 4, false, false);
}

// The arc is analytic, so no Rigidbody2D is needed: physics only samples it for contacts.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class LakeDroplet : MonoBehaviour
{
    public const float Gravity = 7.4f;
    private static readonly Collider2D[] Contacts = new Collider2D[8];
    private ILakeDropOwner owner;
    private Object ownerObject;
    private Health target;
    private LakeDropStyle style;
    private Vector2 origin, launch, lastPoint;
    private float age, waterY, drawingScale;
    private ContactFilter2D filter;
    private SpriteRenderer drawing;
    private LineRenderer trail;
    private readonly Vector3[] history = new Vector3[4];

    public bool Live { get; private set; }
    public ILakeDropOwner Owner => owner;
    public Sprite Drawing => drawing != null ? drawing.sprite : null;
    public Vector2 FrontDirection => -transform.up;
    public Vector2 Velocity => launch + Vector2.down * Gravity * age;
    public bool Returning => Velocity.y < 0;
    private bool OwnerAlive => ownerObject != null;

    public static Vector2 Point(Vector2 start, Vector2 launch, float time)
        => start + launch * time + Vector2.down * (.5f * Gravity * time * time);
    public static Vector2 LaunchThrough(Vector2 start, Vector2 aim, float time, float gravity = Gravity)
        => (aim - start) / time + Vector2.up * (.5f * gravity * time);
    public static LakeDroplet Fire(ILakeDropOwner owner, Sprite sprite, Vector2 start, Vector2 velocity, LakeDropStyle style)
    {
        var drop = LakeFxPool.Instance.TakeDrop();
        drop.Launch(owner, sprite, start, velocity, style);
        return drop;
    }
    private void Awake()
    {
        drawing = GetComponent<SpriteRenderer>();
        drawing.sortingOrder = 34;
        filter = LakeHit.PlayerFilter();
    }
    private void Launch(ILakeDropOwner source, Sprite sprite, Vector2 start, Vector2 velocity, LakeDropStyle dropStyle)
    {
        owner = source;
        ownerObject = source as Object;
        style = dropStyle;
        target = source.DropTarget;
        waterY = source.DropWaterY;
        origin = lastPoint = start;
        launch = velocity;
        age = 0;
        Live = true;
        source.DropStarted();
        drawing.sprite = sprite;
        drawingScale = style.Height / Mathf.Max(.01f, sprite != null ? sprite.bounds.size.y : 1);
        transform.localScale = Vector3.one * drawingScale;
        Present(start, velocity);
        if (trail == null && style.Trail)
            BuildTrail();
        if (trail != null)
        {
            trail.enabled = style.Trail;
            for (int i = 0; i < history.Length; i++)
                history[i] = start;
            trail.SetPositions(history);
        }
    }
    private void BuildTrail()
    {
        var material = LakeBackdrop.Current != null ? LakeBackdrop.Current.WaterFXMaterial : null;
        if (material == null)
            return;
        trail = new GameObject("LiquidWaterStreak").AddComponent<LineRenderer>();
        trail.transform.SetParent(transform, false);
        trail.sharedMaterial = material;
        trail.useWorldSpace = true;
        trail.positionCount = history.Length;
        trail.startWidth = .035f;
        trail.endWidth = 0;
        trail.sortingOrder = 33;
        trail.startColor = new Color(.35f, .74f, .76f, .5f);
        trail.endColor = new Color(.35f, .74f, .76f, 0);
    }
    private void FixedUpdate()
    {
        if (!Live)
            return;
        if (!OwnerAlive || !owner.DropsLive)
        { Release(); return; }
        age += Time.fixedDeltaTime;
        Vector2 point = Point(origin, launch, age);
        if (LakeHit.Touch(target, lastPoint, point, style.Radius, filter, Contacts))
        {
            // A drop is spent even against dash invulnerability: it never waits inside the ship.
            owner.DropHit();
            Release();
            return;
        }
        lastPoint = point;
        if (trail != null && style.Trail)
        {
            for (int i = history.Length - 1; i > 0; i--)
                history[i] = history[i - 1];
            history[0] = point;
            trail.SetPositions(history);
        }
        if (Returning && point.y <= waterY)
        {
            LakeWaterFX.Create(new Vector2(point.x, waterY), style.SplashSize);
            owner.DropReturned();
            Release();
            return;
        }
        Rect arena = owner.DropArena;
        if (age > style.MaxAge || point.x < arena.xMin - style.ArenaMargin || point.x > arena.xMax + style.ArenaMargin)
            Release();
    }
    private void LateUpdate()
    {
        if (!Live || Time.timeScale <= 0)
            return;
        float renderAge = age + Mathf.Clamp(Time.time - Time.fixedTime, 0, Time.fixedDeltaTime);
        Present(Point(origin, launch, renderAge), launch + Vector2.down * Gravity * renderAge);
    }
    private void Present(Vector2 point, Vector2 velocity)
    {
        transform.SetPositionAndRotation(point, Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + 90));
        if (!style.Stretch)
            return;
        float stretch = Mathf.SmoothStep(.78f, 1.16f, Mathf.Clamp01(velocity.magnitude / 8));
        transform.localScale = new Vector3(drawingScale / Mathf.Sqrt(stretch), drawingScale * stretch, drawingScale);
    }
    public void Release()
    {
        if (!Live)
            return;
        Live = false;
        if (OwnerAlive)
            owner.DropEnded();
        owner = null;
        ownerObject = null;
        LakeFxPool.Return(this);
    }
}
