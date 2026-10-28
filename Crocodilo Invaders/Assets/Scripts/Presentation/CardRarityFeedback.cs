using UnityEngine;

public sealed class CardRarityFeedback : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image glow;
    [SerializeField] private CardRarityParticles particles;
    [SerializeField] private UnityEngine.UI.Image lightningLeft;
    [SerializeField] private UnityEngine.UI.Image lightningRight;
    [SerializeField] private Sprite[] lightningFrames;
    [SerializeField] private CardRarityAudio sound;
    [SerializeField, Min(1f)] private float lightningFramesPerSecond = 40f;
    private UpgradeRarity rarity;
    private Color tint;
    private bool paused;
    private float burstAge, focus;
    public bool IsRevealed { get; private set; }
    public float Elapsed { get; private set; }
    public bool HasAura => IsRevealed && rarity >= UpgradeRarity.Rare;
    public bool LightningVisible => lightningLeft != null && lightningLeft.enabled;
    public float Impact => IsRevealed ? Mathf.Sin(Mathf.Clamp01(burstAge / .7f) * Mathf.PI)
        * Mathf.Exp(-burstAge * 4f) * (int)rarity : 0f;

    public void Configure(Vector2 cardSize)
    {
        particles.SetCardSize(cardSize);
        particles.raycastTarget = false;
        glow.rectTransform.sizeDelta = cardSize + new Vector2(100f, 20f);
        lightningLeft.rectTransform.sizeDelta = lightningRight.rectTransform.sizeDelta = new Vector2(80f, cardSize.y + 24f);
        lightningLeft.rectTransform.anchoredPosition = Vector2.left * (cardSize.x * .5f + 20f);
        lightningRight.rectTransform.anchoredPosition = Vector2.right * (cardSize.x * .5f + 20f);
    }
    public void Bind(UpgradeRarity value)
    {
        Hide();
        rarity = value;
        tint = CardOptionView.ColorFor(value);
        Elapsed = burstAge = focus = 0f;
        paused = false;
    }
    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;
        Elapsed = burstAge = 0f;
        sound.Reveal(rarity);
        glow.enabled = HasAura;
        particles.enabled = true;
        Animate();
    }
    public void Celebrate()
    {
        if (!IsRevealed) return;
        burstAge = 0f;
        Animate();
    }
    public void SetFocus(float value) => focus = Mathf.Clamp01(value);
    public void SetPaused(bool value)
    {
        paused = value;
        if (sound != null) sound.SetPaused(value);
    }
    private void Update() => Advance(Time.unscaledDeltaTime);

    // Um relógio para congelar todos os efeitos junto com a pausa.
    public void Advance(float delta)
    {
        if (!IsRevealed || paused) return;
        Elapsed += Mathf.Max(0f, delta);
        burstAge += Mathf.Max(0f, delta);
        Animate();
    }
    private void Animate()
    {
        int tier = (int)rarity;
        float arrival = Mathf.Clamp01(Elapsed / .045f);
        float burst = Mathf.Exp(-burstAge * 4f);
        float pulse = Mathf.Sin(Elapsed * (2.6f + tier * .3f));
        float resting = tier == 1 ? .055f : tier == 2 ? .15f : tier == 3 ? .26f : .36f;
        float reveal = tier == 1 ? .08f : tier == 2 ? .17f : tier == 3 ? .26f : .34f;
        glow.color = new Color(tint.r, tint.g, tint.b,
            arrival * (resting + pulse * .012f * tier + burst * reveal + focus * .025f * tier));
        glow.rectTransform.localScale = Vector3.one * (1f + pulse * .012f + burst * tier * .018f);
        particles.Draw(Elapsed, tier, tint, arrival, burstAge, focus);
        // Native short mesh arcs now hug the border; do not stretch the old
        // vertical thunder sprites around an unrelated card silhouette.
        lightningLeft.enabled = lightningRight.enabled = false;
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
