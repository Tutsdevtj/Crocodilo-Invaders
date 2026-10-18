using UnityEngine;

public sealed class GusiniAcidYolk : MonoBehaviour
{
    private bool spent;
    private float age;
    public static void Create(Sprite sprite, Transform parent, Vector3 position, float scale, Vector2 direction)
    {
        var root = new GameObject("Acid yolk"); root.transform.SetParent(parent, false);
        root.transform.position = position; root.transform.localScale = Vector3.one * scale * 100f;
        var visual = root.AddComponent<SpriteRenderer>(); visual.sortingOrder = 25; visual.sprite = sprite;
        // Cada sprite já aponta na direção correta; não rotacionar a arte.
        var hitbox = root.AddComponent<CircleCollider2D>(); hitbox.isTrigger = true; hitbox.radius = .04f;
        var body = root.AddComponent<Rigidbody2D>(); body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.linearVelocity = direction * (145f * scale);
        root.AddComponent<GusiniAcidYolk>();
    }
    private void Update() { age += Time.deltaTime; if (age > 7f) Destroy(gameObject); }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || other.gameObject.layer != 8) return;
        var health = other.GetComponentInParent<Health>();
        if (health == null || (!health.TakeDamage(2) && !health.IsInvulnerable)) return;
        spent = true; GetComponent<Collider2D>().enabled = false; Destroy(gameObject);
    }
}
