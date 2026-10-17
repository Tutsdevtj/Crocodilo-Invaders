using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class GusiniPresentation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(.1f)] private float animationSpeed = 2f;
    private Animator animator;
    private GusiniAudio audio;
    private GusiniCombat combat;
    private Boss boss;
    private bool whistlePlayed;
    private bool windupPlayed, latchPlayed, openPlayed, closePlayed;
    public bool IsIdle => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0);

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = animationSpeed;
        audio = GetComponent<GusiniAudio>();
        combat = GetComponent<GusiniCombat>();
        boss = GetComponent<Boss>();
    }

    private void OnEnable()
    {
        combat.AttackStarted += OnAttack;
        boss.Health.Damaged += OnDamage;
    }
    private void OnDisable()
    {
        combat.AttackStarted -= OnAttack;
        boss.Health.Damaged -= OnDamage;
    }
    private void OnAttack(GusiniCombat.Attack attack)
    {
        windupPlayed = latchPlayed = openPlayed = closePlayed = false;
    }
    private void OnDamage() { if (audio != null) audio.Play(GusiniSound.Hit, transform.position); }
    public void OnShootWindup()
    {
        if (windupPlayed || !combat.IsPending(GusiniCombat.Attack.Burst)) return;
        windupPlayed = true;
        if (audio != null) audio.Play(GusiniSound.Cock, transform.position);
    }

    public void Play(GusiniCombat.Attack attack)
    {
        ResetTriggers();
        whistlePlayed = false;
        animator.SetTrigger(attack == GusiniCombat.Attack.Burst ? "Shoot" :
            attack == GusiniCombat.Attack.Squadron ? "Squadron" : "EggDrop");
    }

    public void OnShootRelease()
    {
        if (combat.TryRelease(GusiniCombat.Attack.Burst) && audio != null) audio.Play(GusiniSound.Cannon, transform.position);
    }
    public void OnSquadronRelease()
    {
        if (combat.TryRelease(GusiniCombat.Attack.Squadron) && audio != null) audio.Play(GusiniSound.Command, transform.position);
    }
    public void OnEggRelease()
    {
        if (combat.TryRelease(GusiniCombat.Attack.EggParachute) && audio != null) audio.Play(GusiniSound.EggDrop, transform.position);
    }
    public void OnHatchLatch() => HatchSound(ref latchPlayed, GusiniSound.HatchLatch);
    public void OnHatchOpen() => HatchSound(ref openPlayed, GusiniSound.HatchOpen);
    public void OnHatchClose() => HatchSound(ref closePlayed, GusiniSound.HatchClose);
    private void HatchSound(ref bool played, GusiniSound sound)
    {
        if (played || !combat.IsPending(GusiniCombat.Attack.EggParachute)) return;
        played = true;
        if (audio != null) audio.Play(sound, transform.position);
    }
    public void OnWhistle()
    {
        if (whistlePlayed || !combat.IsPending(GusiniCombat.Attack.Squadron)) return;
        whistlePlayed = true;
        if (audio != null) audio.Play(GusiniSound.Whistle, transform.position);
    }
    public void OnAttackFinished(int attack) => combat.FinishAnimation((GusiniCombat.Attack)attack);

    public void Cancel()
    {
        if (animator == null) return;
        ResetTriggers();
        if (animator.gameObject.activeInHierarchy && animator.isActiveAndEnabled) animator.Play("Idle", 0, 0f);
        if (audio != null) audio.StopAll();
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger("Shoot"); animator.ResetTrigger("Squadron"); animator.ResetTrigger("EggDrop");
    }
    private void LateUpdate()
    {
        if (boss == null || !boss.Health.IsDead || visual == null) return;
        Color color = visual.color;
        color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime / boss.DefeatDuration);
        visual.color = color;
    }
}
