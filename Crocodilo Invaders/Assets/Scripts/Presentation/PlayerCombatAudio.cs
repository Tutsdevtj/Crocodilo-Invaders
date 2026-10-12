using UnityEngine;

[RequireComponent(typeof(PlayerCombat), typeof(AudioSource))]
public sealed class PlayerCombatAudio : MonoBehaviour
{
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip specialSound;

    private PlayerCombat combat;
    private AudioSource source;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        combat.ShotFired += PlayShot;
        combat.SpecialFired += PlaySpecial;
    }

    private void OnDisable()
    {
        combat.ShotFired -= PlayShot;
        combat.SpecialFired -= PlaySpecial;
    }

    private void PlayShot()
    {
        if (shotSound != null) source.PlayOneShot(shotSound);
    }

    private void PlaySpecial()
    {
        if (specialSound != null) source.PlayOneShot(specialSound);
    }
}
