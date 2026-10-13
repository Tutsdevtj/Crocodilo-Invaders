using UnityEngine;
using UnityEngine.UI;

public sealed class CardRarityFeedback : MonoBehaviour
{
    [SerializeField] private Image glow;
    [SerializeField] private CardRarityParticles particles;
    [SerializeField] private Image lightningLeft;
    [SerializeField] private Image lightningRight;
    [SerializeField] private Sprite[] lightningFrames;
    [SerializeField] private CardRarityAudio sound;
    [SerializeField, Min(1f)] private float lightningFramesPerSecond = 40f;
    private UpgradeRarity rarity;
    private Color tint;
    private bool paused;
    public bool IsRevealed { get; private set; }
    public float Elapsed { get; private set; }
    public bool HasAura => IsRevealed && rarity >= UpgradeRarity.Epic;
    public bool LightningVisible => lightningLeft != null && lightningLeft.enabled;

    public void Bind(UpgradeRarity value)
    {
        Hide();
        rarity = value;
        tint = CardOptionView.ColorFor(value);
        Elapsed = 0f;
        paused = false;
    }

    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;
        Elapsed = 0f;
        if (rarity < UpgradeRarity.Epic) return;
        sound.Reveal(rarity);
        glow.enabled = true;
        particles.enabled = true;
        Animate();
    }

    public void SetPaused(bool value) => paused = value;

    private void Update()
    {
        if (!HasAura || paused) return;
        Elapsed += Time.unscaledDeltaTime;
        Animate();
    }

    private void Animate()
    {
        int tier = (int)rarity - (int)UpgradeRarity.Epic;
        float arrival = Mathf.Clamp01(Elapsed / .12f);
        float burst = Mathf.Exp(-Elapsed * 5f);
        float pulse = Mathf.Sin(Elapsed * (2.6f + tier * .3f));
        glow.color = new Color(tint.r, tint.g, tint.b,
            arrival * (.55f + tier * .1f + pulse * .055f + burst * .25f));
        glow.rectTransform.localScale = Vector3.one * (1f + pulse * .018f + burst * .08f);
        particles.Draw(Elapsed, tier, tint, arrival);
        int frame = Mathf.FloorToInt(Elapsed * lightningFramesPerSecond);
        bool thunder = rarity == UpgradeRarity.Mythic && lightningFrames != null && frame < lightningFrames.Length;
        lightningLeft.enabled = lightningRight.enabled = thunder;
        if (thunder)
        {
            lightningLeft.sprite = lightningRight.sprite = lightningFrames[frame];
            lightningLeft.color = lightningRight.color = new Color(1f, 1f, 1f, arrival);
        }
    }

    public void Hide()
    {
        IsRevealed = false;
        if (glow != null) glow.enabled = false;
        if (particles != null) particles.enabled = false;
        if (lightningLeft != null) lightningLeft.enabled = false;
        if (lightningRight != null) lightningRight.enabled = false;
        if (sound != null) sound.Stop();
    }

    private void OnDisable() => Hide();
}
