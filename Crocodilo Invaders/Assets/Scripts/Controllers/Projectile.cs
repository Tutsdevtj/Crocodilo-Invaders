using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Projectile : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 3f;
    [SerializeField, Min(0.01f)] private float lifetime = 5f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField] private LayerMask targetLayers = 1 << 9;
    public event Action<Vector2> Hit;

    private Rigidbody2D body;
    private bool spent;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, Collider2D ownerCollider)
    {
        if (ownerCollider != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);

        body.linearVelocity = direction.normalized * speed;
        // A duração pertence ao projétil, sem depender da câmera ou do input.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Health target = other.GetComponentInParent<Health>();
        // Captura o ponto antes do dano: um acerto fatal pode desativar o collider.
        Vector2 hitPosition = other.ClosestPoint(body.position);
        if (target == null || !target.TakeDamage(damage)) return;

        spent = true;
        GetComponent<Collider2D>().enabled = false;
        Hit?.Invoke(hitPosition);
        Destroy(gameObject);
    }
}
