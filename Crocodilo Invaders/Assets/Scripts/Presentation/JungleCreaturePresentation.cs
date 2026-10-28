using UnityEngine;

// A single native sprite per creature. Flight motion is a Transform effect, not invented animation frames.
public sealed class JungleCreaturePresentation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private LineRenderer warning;
    private Health health;
    private EnemyAttack combat;
    private Vector3 basePosition;
    private Color baseColor;
    private float clock, flashUntil;
    private void Awake()
    {
        health = GetComponent<Health>(); combat = GetComponent<EnemyAttack>();
        basePosition = visual.transform.localPosition; baseColor = visual.color;
    }
    private void OnEnable() { health.Damaged += Flash; health.Died += Died; }
    private void OnDisable()
    {
        health.Damaged -= Flash; health.Died -= Died;
        if (warning != null) warning.enabled = false;
        if (visual != null) { visual.color = baseColor; visual.transform.localPosition = basePosition; }
    }
    private void Flash() => flashUntil = Time.time + .12f;
    private void Died() { visual.enabled = false; if (warning != null) warning.enabled = false; }
    private void LateUpdate()
    {
        if (health.IsDead) return;
        clock += Time.deltaTime;
        visual.transform.localPosition = basePosition + Vector3.up * (Mathf.Round(Mathf.Sin(clock * 8f) * 2f) / 100f);
        visual.color = Time.time < flashUntil ? new Color(1f, .7f, .7f) : baseColor;
        if (warning == null) return;
        warning.enabled = combat != null && combat.isActiveAndEnabled && combat.IsTelegraphing;
        if (!warning.enabled) return;
        warning.positionCount = 2; warning.SetPosition(0, transform.position); warning.SetPosition(1, combat.WarningDestination);
    }
}
