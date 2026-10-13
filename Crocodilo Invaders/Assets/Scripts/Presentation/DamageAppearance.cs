using UnityEngine;

[RequireComponent(typeof(Health), typeof(SpriteRenderer))]
public sealed class DamageAppearance : MonoBehaviour
{
    [Header("Escurecimento por porcentagem de vida")]
    [SerializeField, Range(0.01f, 1f)] private float darkeningStartHealth = 0.75f;
    [SerializeField, Range(0f, 1f)] private float maximumDarkening = 0.45f;
    [Header("Rachaduras")]
    [SerializeField, Range(0f, 1f)] private float cracksHealthThreshold = 0.5f;
    [SerializeField] private SpriteRenderer cracksRenderer;
    [SerializeField] private Sprite[] crackVariants;
    [SerializeField] private Vector2 cracksSizeLimit = new Vector2(0.36f, 0.18f);

    private Health health;
    private SpriteRenderer bodyRenderer;
    private Color originalTint;
    private MaterialPropertyBlock properties;
    private static readonly System.Random VisualRandom = new System.Random();

    private void Awake()
    {
        health = GetComponent<Health>();
        bodyRenderer = GetComponent<SpriteRenderer>();
        originalTint = bodyRenderer.color;
        properties = new MaterialPropertyBlock();
        ChooseCrack();
    }

    private void OnEnable()
    {
        health.HealthChanged += Refresh;
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void Start() => Refresh(health.CurrentHealth, health.MaxHealth);

    private void OnDisable()
    {
        health.HealthChanged -= Refresh;
        ResetAppearance();
    }

    private void ChooseCrack()
    {
        if (cracksRenderer == null || crackVariants == null || crackVariants.Length == 0) return;
        // Um único sorteio por entidade; cura e novos hits preservam a mesma rachadura.
        cracksRenderer.sprite = crackVariants[VisualRandom.Next(crackVariants.Length)];
        cracksRenderer.sharedMaterial = bodyRenderer.sharedMaterial;
        if (cracksRenderer.sprite == null) return;
        Vector2 size = cracksRenderer.sprite.bounds.size;
        float scale = Mathf.Min(cracksSizeLimit.x / size.x, cracksSizeLimit.y / size.y);
        cracksRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void Refresh(int current, int maximum)
    {
        if (current <= 0 || maximum <= 0)
        {
            ResetAppearance();
            return;
        }

        float healthFraction = health.Fraction;
        float damageAmount = Mathf.InverseLerp(darkeningStartHealth, 0f, healthFraction);
        float brightness = 1f - maximumDarkening * damageAmount;
        bodyRenderer.color = new Color(originalTint.r * brightness, originalTint.g * brightness,
            originalTint.b * brightness, originalTint.a);
        if (cracksRenderer != null)
            cracksRenderer.enabled = healthFraction <= cracksHealthThreshold && cracksRenderer.sprite != null;
    }

    private void LateUpdate()
    {
        if (cracksRenderer == null || !cracksRenderer.enabled) return;
        // A rachadura acompanha o flash do casco, em vez de cobrir o branco com uma marca escura.
        bodyRenderer.GetPropertyBlock(properties);
        cracksRenderer.SetPropertyBlock(properties);
    }

    private void ResetAppearance()
    {
        if (bodyRenderer != null) bodyRenderer.color = originalTint;
        if (cracksRenderer != null) cracksRenderer.enabled = false;
    }
}
