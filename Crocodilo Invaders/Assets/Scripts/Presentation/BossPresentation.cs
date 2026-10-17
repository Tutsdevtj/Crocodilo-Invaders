using UnityEngine;

[RequireComponent(typeof(Boss), typeof(AudioSource))]
public sealed class BossPresentation : MonoBehaviour
{
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip missileSound;
    [SerializeField] private AudioClip redShotSound;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private SpriteRenderer charge;
    [SerializeField] private LineRenderer aimGuide;

    private Boss boss;
    private BossCombat combat;
    private AudioSource sounds;
    private SpriteRenderer body;
    private bool telegraphing;
    private BossCombat.Attack attack;
    private Vector2 aimPoint;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        combat = GetComponent<BossCombat>();
        sounds = GetComponent<AudioSource>();
        body = GetComponent<SpriteRenderer>();
        HideTelegraph();
    }

    private void OnEnable()
    {
        boss.Health.Damaged += OnDamage;
        boss.Health.Died += OnDeath;
        boss.BattleStopped += StopMusic;
        combat.TelegraphStarted += ShowTelegraph;
        combat.TelegraphFinished += HideTelegraph;
        combat.ShotFired += OnShot;
        combat.EnrageStarted += OnEnrage;
    }

    private void Start()
    {
        if (boss.BattleActive && music != null) music.Play();
    }

    private void OnDisable()
    {
        boss.Health.Damaged -= OnDamage;
        boss.Health.Died -= OnDeath;
        boss.BattleStopped -= StopMusic;
        combat.TelegraphStarted -= ShowTelegraph;
        combat.TelegraphFinished -= HideTelegraph;
        combat.ShotFired -= OnShot;
        combat.EnrageStarted -= OnEnrage;
        HideTelegraph();
        StopMusic();
    }

    private void ShowTelegraph(BossCombat.Attack nextAttack, Vector2 target)
    {
        attack = nextAttack;
        aimPoint = target;
        telegraphing = true;
        charge.enabled = true;
        aimGuide.enabled = attack != BossCombat.Attack.Lasers;
    }

    private void HideTelegraph()
    {
        telegraphing = false;
        if (charge != null) charge.enabled = false;
        if (aimGuide != null) aimGuide.enabled = false;
    }

    private void LateUpdate()
    {
        if (boss.Health.IsDead)
        {
            Color color = body.color;
            color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime / 1.5f);
            body.color = color;
        }
        if (!telegraphing) return;
        Vector3 origin = combat.ShotOrigin.position;
        if (attack == BossCombat.Attack.Missiles)
        {
            aimGuide.positionCount = 2;
            aimGuide.SetPosition(0, origin);
            aimGuide.SetPosition(1, aimPoint);
        }
        else if (attack == BossCombat.Attack.Sweep)
        {
            aimGuide.positionCount = 2;
            float angle = (180f + Mathf.Sin(Time.time * 7f) * 54f) * Mathf.Deg2Rad;
            aimGuide.SetPosition(0, origin);
            aimGuide.SetPosition(1, origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 1.5f);
        }
        else
        {
            aimGuide.positionCount = 13;
            for (int i = 0; i < 13; i++)
            {
                float angle = Mathf.Lerp(126f, 234f, i / 12f) * Mathf.Deg2Rad;
                aimGuide.SetPosition(i, origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 1.1f);
            }
        }
    }

    private void OnShot(BossCombat.Attack fired)
    {
        AudioClip clip = fired == BossCombat.Attack.Missiles ? missileSound : redShotSound;
        if (clip != null) sounds.PlayOneShot(clip);
    }

    private void OnDamage()
    {
        if (damageSound != null) sounds.PlayOneShot(damageSound, .45f);
    }

    private void OnDeath()
    {
        HideTelegraph();
        if (deathSound != null) sounds.PlayOneShot(deathSound);
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

    private void OnEnrage()
    {
        if (missileSound != null) sounds.PlayOneShot(missileSound, .8f);
    }

    private void StopMusic()
    {
        if (music != null) music.Stop();
    }
}
