using UnityEngine;

[RequireComponent(typeof(Health), typeof(Animator), typeof(AudioSource))]
public sealed class EnemyPresentation : MonoBehaviour
{
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip shotSound;
    [Header("Avaria visual")]
    [SerializeField] private SpriteRenderer damageSmoke;
    [SerializeField, Range(0f, 1f)] private float criticalHealthThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float smokeOpacity = 0.65f;
    [SerializeField, Min(0.01f)] private float smokeFadeDuration = 0.2f;

    private Health health;
    private Animator animator;
    private AudioSource source;
    private EnemyAttack combat;
    private Animator smokeAnimator;
    private Color smokeColor;
    private float currentSmokeOpacity;
    private bool smokeWanted;
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly System.Random SmokePhases = new System.Random();

    private void Awake()
    {
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        combat = GetComponent<EnemyAttack>();
        if (damageSmoke != null)
        {
            smokeAnimator = damageSmoke.GetComponent<Animator>();
            smokeColor = damageSmoke.color;
            ResetSmoke();
        }
    }

    private void OnEnable()
    {
        health.Damaged += PlayDamage;
        health.Died += PlayDeath;
        health.HealthChanged += RefreshDamageState;
        if (combat != null) combat.ShotFired += PlayShot;
        RefreshDamageState(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        health.Damaged -= PlayDamage;
        health.Died -= PlayDeath;
        health.HealthChanged -= RefreshDamageState;
        if (combat != null) combat.ShotFired -= PlayShot;
        ResetSmoke();
    }

    // A primeira leitura espera o Awake de Health, independentemente da ordem dos componentes.
    private void Start() => RefreshDamageState(health.CurrentHealth, health.MaxHealth);

    private void RefreshDamageState(int current, int maximum)
    {
        smokeWanted = current > 0 && maximum > 0 && health.Fraction <= criticalHealthThreshold;
        if (!smokeWanted || damageSmoke == null || currentSmokeOpacity > 0f) return;

        if (smokeAnimator != null)
        {
            smokeAnimator.enabled = true;
            // A variação visual não consome o Random usado pelo spawn e pela patrulha.
            smokeAnimator.Play(0, 0, (float)SmokePhases.NextDouble());
        }
    }

    private void Update()
    {
        if (damageSmoke == null) return;
        float target = smokeWanted ? smokeOpacity : 0f;
        currentSmokeOpacity = Mathf.MoveTowards(currentSmokeOpacity, target,
            smokeOpacity * Time.deltaTime / smokeFadeDuration);
        Color color = smokeColor;
        color.a = currentSmokeOpacity;
        damageSmoke.color = color;
        damageSmoke.enabled = currentSmokeOpacity > 0f;
        if (smokeAnimator != null) smokeAnimator.enabled = damageSmoke.enabled;
    }

    private void ResetSmoke()
    {
        smokeWanted = false;
        currentSmokeOpacity = 0f;
        if (damageSmoke == null) return;
        Color color = smokeColor;
        color.a = 0f;
        damageSmoke.color = color;
        damageSmoke.enabled = false;
        if (smokeAnimator != null) smokeAnimator.enabled = false;
    }

    private void PlayShot()
    {
        if (shotSound != null) source.PlayOneShot(shotSound);
    }

    private void PlayDamage()
    {
        if (damageSound != null) source.PlayOneShot(damageSound);
    }

    private void PlayDeath()
    {
        animator.SetTrigger(Die);
        if (deathSound != null) source.PlayOneShot(deathSound);
    }
}
