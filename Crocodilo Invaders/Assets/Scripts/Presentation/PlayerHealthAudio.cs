using UnityEngine;

[RequireComponent(typeof(Health), typeof(AudioSource))]
public sealed class PlayerHealthAudio : MonoBehaviour
{
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;

    private Health health;
    private AudioSource source;

    private void Awake()
    {
        health = GetComponent<Health>();
        source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        health.Damaged += PlayDamage;
        health.Healed += PlayHeal;
    }

    private void OnDisable()
    {
        health.Damaged -= PlayDamage;
        health.Healed -= PlayHeal;
    }

    private void PlayDamage()
    {
        if (damageSound != null) source.PlayOneShot(damageSound);
    }

    private void PlayHeal()
    {
        if (healSound != null) source.PlayOneShot(healSound);
    }
}
