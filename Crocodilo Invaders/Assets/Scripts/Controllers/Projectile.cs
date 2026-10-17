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
    private float currentDamage;
    public float BaseDamage => damage;
    public float Damage => currentDamage;

    public bool TargetsLayer(int layer) => (targetLayers.value & (1 << layer)) != 0;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        currentDamage = damage;
    }

    public void Launch(Vector2 direction, Collider2D ownerCollider, float? speedOverride = null, float damageMultiplier = 1f)
    {
        currentDamage = damage * Mathf.Max(0f, damageMultiplier);
        if (ownerCollider != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);

        body.linearVelocity = direction.normalized * Mathf.Max(0f, speedOverride ?? speed);
        // A duração pertence ao projétil, sem depender da câmera ou do input.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Health target = other.GetComponentInParent<Health>();
        // Captura o ponto antes do dano: um acerto fatal pode desativar o collider.
        Vector2 hitPosition = other.ClosestPoint(body.position);
        if (target == null) return;
        bool damaged = target.TakeDamage(currentDamage);
        // Um projétil esquivado também é consumido, sem feedback de acerto ou dano atrasado.
        if (!damaged && !target.IsInvulnerable) return;

        spent = true;
        GetComponent<Collider2D>().enabled = false;
        if (damaged) Hit?.Invoke(hitPosition);
        Destroy(gameObject);
    }
}
