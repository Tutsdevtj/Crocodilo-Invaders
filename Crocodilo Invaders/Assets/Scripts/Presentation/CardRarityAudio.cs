using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class CardRarityAudio : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip epic;
    [SerializeField] private AudioClip legendary;
    [SerializeField] private AudioClip mythic;
    public int RevealCount { get; private set; }
    public AudioClip LastRevealClip { get; private set; }

    private void Awake()
    {
        if (source == null) source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = false;
    }

    public void Reveal(UpgradeRarity rarity)
    {
        AudioClip clip = rarity == UpgradeRarity.Epic ? epic
            : rarity == UpgradeRarity.Legendary ? legendary
            : rarity == UpgradeRarity.Mythic ? mythic : null;
        if (clip == null) return;
        RevealCount++;
        LastRevealClip = clip;
        source.PlayOneShot(clip);
    }

    public void Stop() { if (source != null) source.Stop(); }
    private void OnDisable() => Stop();
}
