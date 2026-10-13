using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(AudioSource))]
public sealed class BossLaser : MonoBehaviour
{
    [SerializeField] private SpriteRenderer beam;
    [SerializeField] private Animator beamAnimator;
    [SerializeField] private LineRenderer warning;
    [SerializeField] private AudioClip fireSound;
    [SerializeField, Min(1)] private int damage = 5;
    [SerializeField] private LayerMask targetLayers = 1 << 8;
    [SerializeField, Min(.01f)] private float visualThickness = 1.5f;
    [SerializeField, Min(.01f)] private float damageThickness = .45f;

    private BoxCollider2D hitbox;
    private readonly HashSet<Health> damagedTargets = new HashSet<Health>();
    public bool IsFiring => hitbox.enabled;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        Disarm();
    }

    public void Begin(Vector2 center, bool vertical, float length, float warningSeconds, float activeSeconds)
    {
        transform.position = center;
        transform.rotation = Quaternion.Euler(0, 0, vertical ? 90f : 0f);
        hitbox.size = new Vector2(length, damageThickness);
        beam.transform.localScale = new Vector3(length / beam.sprite.bounds.size.x, visualThickness / beam.sprite.bounds.size.y, 1f);
        warning.startWidth = warning.endWidth = damageThickness;
        warning.SetPosition(0, Vector3.left * length * .5f);
        warning.SetPosition(1, Vector3.right * length * .5f);
        warning.enabled = true;
        StartCoroutine(Fire(warningSeconds, activeSeconds));
    }

    private IEnumerator Fire(float warningSeconds, float activeSeconds)
    {
        float elapsed = 0f;
        while (elapsed < warningSeconds)
        {
            Color color = new Color(1f, .72f, .15f, .35f + .35f * Mathf.Abs(Mathf.Sin(elapsed * 12f)));
            warning.startColor = warning.endColor = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        warning.enabled = false;
        beam.enabled = true;
        beamAnimator.enabled = true;
        hitbox.enabled = true;
        if (fireSound != null) GetComponent<AudioSource>().PlayOneShot(fireSound);
        yield return new WaitForSeconds(activeSeconds);
        hitbox.enabled = false;
        float fade = 0f;
        while (fade < .15f)
        {
            Color color = beam.color;
            color.a = 1f - fade / .15f;
            beam.color = color;
            fade += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Disarm()
    {
        StopAllCoroutines();
        if (hitbox != null) hitbox.enabled = false;
        if (beam != null) beam.enabled = false;
        if (beamAnimator != null) beamAnimator.enabled = false;
        if (warning != null) warning.enabled = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Disarm();
    }

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (!hitbox.enabled || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Health health = other.GetComponentInParent<Health>();
        if (health != null && !damagedTargets.Contains(health) && health.TakeDamage(damage))
            damagedTargets.Add(health);
    }
}
