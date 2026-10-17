using UnityEngine;

[RequireComponent(typeof(Health), typeof(Animator))]
public sealed class GusiniSquadronGoose : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController variantA;
    [SerializeField] private RuntimeAnimatorController variantB;
    [SerializeField] private SpriteRenderer beak;
    [SerializeField] private Sprite[] beakSprites;
    [SerializeField] private float[] singDurations = { .06f, .06f, .12f, .07f };
    [SerializeField, Min(1)] private int contactDamage = 1;
    [SerializeField] private LayerMask targetLayers = 1 << 8;
    private Health health;
    private float singAt = float.PositiveInfinity;
    private bool spent;
    public int BeakState { get; private set; }

    private void Awake() { health = GetComponent<Health>(); }
    private void OnEnable() => health.Died += OnDied;
    private void OnDisable() => health.Died -= OnDied;
    private void OnDied() { gameObject.SetActive(false); Destroy(gameObject); }

    public void Configure(bool useVariantB, float startNormalizedTime)
    {
        Animator animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = useVariantB ? variantB : variantA;
        animator.Play("Flight", 0, startNormalizedTime);
    }

    public void Sing(float delay)
    {
        singAt = Time.time + Mathf.Max(0f, delay);
    }

    private void Update()
    {
        float t = Time.time - singAt;
        float semi = singDurations[0], open = semi + singDurations[1];
        float closing = open + singDurations[2], end = closing + singDurations[3];
        BeakState = t < semi || t >= end ? 0 : t < open || t >= closing ? 1 : 2;
        beak.sprite = beakSprites[BeakState];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Health target = other.GetComponentInParent<Health>();
        if (target == null || (!target.TakeDamage(contactDamage) && !target.IsInvulnerable)) return;
        spent = true;
        OnDied();
    }
}
