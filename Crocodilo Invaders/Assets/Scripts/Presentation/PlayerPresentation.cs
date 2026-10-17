using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(PlayerCombat), typeof(Animator), typeof(Rigidbody2D))]
public sealed class PlayerPresentation : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField, Min(.01f)] private float shootClipDuration = .39f;
    [SerializeField, Min(.01f)] private float shakeDuration = .1f;
    [SerializeField, Range(1, 2)] private int shakePixels = 2;

    private PlayerCombat combat;
    private Health health;
    private Rigidbody2D body;
    private Animator animator;
    private Vector3 baseVisualLocalPosition;
    private float shakeStartedAt = -1f;
    private bool hurtPending;
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Special = Animator.StringToHash("Special");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int ShootSpeed = Animator.StringToHash("ShootSpeed");

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<Health>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (visualRoot == null || visualRoot == transform || spriteRenderer == null)
        {
            Debug.LogError("Configure the Player's Visual child and its SpriteRenderer.", this);
            enabled = false;
            return;
        }
        baseVisualLocalPosition = visualRoot.localPosition;
    }

    private void OnEnable()
    {
        if (visualRoot == null || spriteRenderer == null) return;
        combat.ShotStarted += OnShotStarted;
        combat.SpecialStarted += OnSpecialStarted;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
        animator.enabled = true;
        animator.Rebind();
    }

    private void OnDisable()
    {
        combat.ShotStarted -= OnShotStarted;
        combat.SpecialStarted -= OnSpecialStarted;
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
        combat.CancelPendingAttacks();
        ResetFeedback();
        animator.enabled = false;
    }

    private void Update()
    {
        if (health.IsDead) return;
        animator.SetBool(Moving, body.linearVelocity.sqrMagnitude > .0001f);
        // Only Shoot is accelerated, so attack-speed upgrades retain their cadence.
        animator.SetFloat(ShootSpeed, Mathf.Max(1f, shootClipDuration / combat.ShotInterval));
        PlayPendingHurt();
    }

    private void OnShotStarted()
    {
        animator.ResetTrigger(Hurt);
        animator.ResetTrigger(Special);
        animator.ResetTrigger(Shoot);
        animator.SetTrigger(Shoot);
    }

    private void OnSpecialStarted()
    {
        animator.ResetTrigger(Hurt);
        animator.ResetTrigger(Shoot);
        animator.ResetTrigger(Special);
        animator.SetTrigger(Special);
    }

    private void OnDamaged()
    {
        if (health.IsDead) return;
        shakeStartedAt = Time.unscaledTime;
        hurtPending = true;
    }

    private void PlayPendingHurt()
    {
        // Flash and shake start immediately; the pose waits for an accepted shot's release.
        if (!hurtPending || combat.HasPendingAttack) return;
        hurtPending = false;
        animator.ResetTrigger(Hurt);
        animator.SetTrigger(Hurt);
    }

    public void AnimationReleaseNormalShot()
    {
        combat.ReleaseNormalShot();
        if (isActiveAndEnabled && !health.IsDead) PlayPendingHurt();
    }

    public void AnimationReleaseSpecial()
    {
        combat.ReleaseSpecial();
        if (isActiveAndEnabled && !health.IsDead) PlayPendingHurt();
    }

    private void LateUpdate()
    {
        if (shakeStartedAt < 0f) return;
        float elapsed = Time.unscaledTime - shakeStartedAt;
        if (elapsed >= shakeDuration)
        {
            shakeStartedAt = -1f;
            visualRoot.localPosition = baseVisualLocalPosition;
            return;
        }
        int phase = Mathf.Min(2, Mathf.FloorToInt(elapsed / shakeDuration * 3f));
        int pixels = phase == 0 ? -shakePixels : phase == 1 ? shakePixels : -1;
        float onePixel = spriteRenderer.sprite != null
            ? visualRoot.localScale.x / spriteRenderer.sprite.pixelsPerUnit : 0f;
        visualRoot.localPosition = baseVisualLocalPosition + Vector3.right * (pixels * onePixel);
    }

    private void OnDied()
    {
        combat.CancelPendingAttacks();
        ResetFeedback();
        animator.enabled = false;
    }

    private void ResetFeedback()
    {
        hurtPending = false;
        shakeStartedAt = -1f;
        if (visualRoot != null) visualRoot.localPosition = baseVisualLocalPosition;
        animator.ResetTrigger(Shoot);
        animator.ResetTrigger(Special);
        animator.ResetTrigger(Hurt);
    }
}
