using UnityEngine;

// Objetos reais da cena. 100 pixels da arte correspondem a uma unidade da Unity.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CrocodiloVisual : MonoBehaviour
{
    public Sprite[] frames;
    [Min(0.001f)] public float frameSeconds = 0.1f;
    public BoxCollider2D hitbox;
    [Header("Jogabilidade (pixels por segundo)")]
    [Min(0)] public float moveSpeed = 333.33334f;
    [Min(1)] public int maxHealth = 4;
    [Min(0)] public int damage = 1;
    [Tooltip("Marcador de nascimento. Sem marcador, usa a posição salva deste objeto.")]
    public Transform spawnPoint;
    public Vector2 respawnY = new Vector2(250, 500);
    private SpriteRenderer cachedRenderer;

    public SpriteRenderer Renderer
    {
        get { if (cachedRenderer == null) cachedRenderer = GetComponent<SpriteRenderer>(); return cachedRenderer; }
    }

    public Vector2 GamePosition
    {
        get { return new Vector2(transform.position.x * 100f, -transform.position.y * 100f); }
        set { transform.position = new Vector3(value.x / 100f, -value.y / 100f, transform.position.z); }
    }

    public Vector2 SpawnPosition
    {
        get { return spawnPoint == null ? GamePosition : new Vector2(spawnPoint.position.x * 100f, -spawnPoint.position.y * 100f); }
    }

    public void Show(bool visible, float elapsed)
    {
        Renderer.enabled = visible;
        if (frames == null || frames.Length <= 1) return;
        int frame = Mathf.FloorToInt(Mathf.Max(0, elapsed) / Mathf.Max(0.001f, frameSeconds)) % frames.Length;
        if (Renderer.sprite != frames[frame]) Renderer.sprite = frames[frame];
    }

    public void FitRect(Rect rect)
    {
        GamePosition = rect.position;
        Sprite sprite = Renderer.sprite;
        if (sprite == null) return;
        transform.localScale = new Vector3(rect.width / sprite.rect.width, rect.height / sprite.rect.height, 1);
    }

    public Rect CollisionRectAt(float x, float y)
    {
        if (hitbox != null) return ColliderRectAt(hitbox, transform, x, y);
        Bounds bounds = Renderer.sprite != null ? Renderer.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
        return LocalRectAt(transform, bounds.min, bounds.max, transform, x, y);
    }

    public static Rect ColliderRectAt(BoxCollider2D collider, Transform owner, float x, float y)
    {
        Vector2 half = collider.size / 2;
        return LocalRectAt(collider.transform, collider.offset - half, collider.offset + half, owner, x, y);
    }

    private static Rect LocalRectAt(Transform source, Vector3 min, Vector3 max, Transform owner, float x, float y)
    {
        Vector3 shift = new Vector3(x / 100f, -y / 100f, owner.position.z) - owner.position;
        Vector3 a = source.TransformPoint(min) + shift;
        Vector3 b = source.TransformPoint(new Vector3(min.x, max.y, 0)) + shift;
        Vector3 c = source.TransformPoint(max) + shift;
        Vector3 d = source.TransformPoint(new Vector3(max.x, min.y, 0)) + shift;
        float left = Mathf.Min(a.x, b.x, c.x, d.x) * 100f;
        float right = Mathf.Max(a.x, b.x, c.x, d.x) * 100f;
        float top = -Mathf.Max(a.y, b.y, c.y, d.y) * 100f;
        float bottom = -Mathf.Min(a.y, b.y, c.y, d.y) * 100f;
        return Rect.MinMaxRect(left, top, right, bottom);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && Renderer.sprite == null && frames != null && frames.Length > 0) Renderer.sprite = frames[0];
    }
}
