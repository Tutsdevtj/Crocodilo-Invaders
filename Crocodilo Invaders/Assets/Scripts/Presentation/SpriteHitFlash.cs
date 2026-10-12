using UnityEngine;

[RequireComponent(typeof(Health), typeof(SpriteRenderer))]
public sealed class SpriteHitFlash : MonoBehaviour
{
    [SerializeField, Min(0f)] private float holdDuration = 0.045f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.065f;

    private Health health;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock properties;
    private float startedAt;
    private bool flashing;
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

    private void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        properties = new MaterialPropertyBlock();
    }

    private void OnEnable() => health.Damaged += Flash;

    private void OnDisable()
    {
        health.Damaged -= Flash;
        flashing = false;
        SetAmount(0f);
    }

    private void Flash()
    {
        startedAt = Time.time;
        flashing = true;
        SetAmount(1f);
    }

    private void Update()
    {
        if (!flashing) return;
        float amount = 1f - Mathf.Clamp01((Time.time - startedAt - holdDuration) / fadeDuration);
        SetAmount(amount);
        if (amount <= 0f) flashing = false;
    }

    private void SetAmount(float amount)
    {
        // Cada nave pisca individualmente, usando o mesmo material compartilhado.
        spriteRenderer.GetPropertyBlock(properties);
        properties.SetFloat(FlashAmount, amount);
        spriteRenderer.SetPropertyBlock(properties);
    }
}
