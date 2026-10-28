using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class CardRarityAudio : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip rare;
    [SerializeField] private AudioClip epic;
    [SerializeField] private AudioClip legendary;
    [SerializeField] private AudioClip mythic;
    public float OfferGain { get; private set; } = 1f;
    public void ConfigureOverlap(int highRarityCount, UpgradeRarity rarity)
    {
        OfferGain = rarity >= UpgradeRarity.Legendary ? 1f / Mathf.Max(1, highRarityCount) : 1f;
    }
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
        AudioClip clip = rarity == UpgradeRarity.Rare ? rare : rarity == UpgradeRarity.Epic ? epic
            : rarity == UpgradeRarity.Legendary ? legendary
            : rarity == UpgradeRarity.Mythic ? mythic : null;
        if (clip == null) return;
        RevealCount++;
        LastRevealClip = clip;
        source.pitch = 1f; // Cada raridade tem composição própria, com notas e camadas distintas.
        float gain = rarity == UpgradeRarity.Rare ? .60f : rarity == UpgradeRarity.Epic ? .72f
            : rarity == UpgradeRarity.Legendary ? .84f : .96f;
        source.PlayOneShot(clip, gain * OfferGain);
    }

    public void Stop() { if (source != null) source.Stop(); }
    public void SetPaused(bool value)
    {
        if (source == null) return;
        if (value) source.Pause(); else source.UnPause();
    }
    private void OnDisable() => Stop();
}
