using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BossBarFX))]
public sealed class BossBarPhaseTwo : MonoBehaviour
{
    [SerializeField] private GusiniPhaseTwoAssets assets;
    [SerializeField] private Image eye, takeover, nameImage;
    [SerializeField] private Image[] motes;
    [SerializeField] private Material silhouetteMaterial;
    private float clock;
    private bool active, changed;
    private BossBarFX bar;
    private Sprite[] originalSprites;
    private Image[] images;
    private Material[] originalMaterials;
    private bool flashing;
    private float health = 1f;

    private void Awake()
    {
        bar = GetComponent<BossBarFX>();
        images = GetComponentsInChildren<Image>(true);
        originalSprites = new Sprite[images.Length];
        originalMaterials = new Material[images.Length];
        for (int i = 0; i < images.Length; i++) originalSprites[i] = images[i].sprite;
        for (int i = 0; i < images.Length; i++) originalMaterials[i] = images[i].material;
    }
    public void ResetPhase()
    {
        active = changed = false;
        Flash(false);
        if (images != null) for (int i = 0; i < images.Length; i++) images[i].sprite = originalSprites[i];
        eye.enabled = takeover.enabled = false;
        foreach (Image mote in motes) mote.enabled = false;
    }
    public void Begin(float fraction, float elapsed = 0f)
    {
        active = true; changed = false; clock = Mathf.Max(0f, elapsed); health = fraction;
        bar.BeginPhaseTwoCharge(fraction, elapsed);
    }
    public void SetHealth(float value) => health = value;
    private void LateUpdate()
    {
        if (!active) return;
        clock += Time.deltaTime;
        takeover.enabled = clock < .36f;
        if (clock < .24f) takeover.sprite = assets.Get("takeover_" + Mathf.Min(3, (int)(clock / .06f)));
        else if (clock >= .3f && !changed)
        {
            changed = true; bar.ChangePhaseTwoSprites(assets);
            takeover.sprite = assets.Get("takeover_3");
        }
        // Flash da silhueta, sem cobrir a tela com um retângulo opaco.
        float alpha = clock >= .24f ? Mathf.Clamp01(1f - (clock - .24f) / .12f) : 1f;
        takeover.color = new Color(1f, 1f, 1f, alpha);
        Flash(clock >= .24f && clock < .36f);
        if (!changed) return;
        bool critical = clock >= .98f && health > 0f && health <= .3f;
        float blink = clock % 2.6f;
        int frame = blink < .08f ? 1 : blink < .16f ? 2 : blink < .24f ? 1 : 0;
        if (clock < .98f) frame = clock < .6f ? 2 : clock < .78f ? 1 : 0;
        if (critical && Mathf.Sin(clock * Mathf.PI * 2f / .7f) > .3f) frame = 3;
        eye.enabled = true; eye.sprite = assets.Get("emblem_eye_" + frame);
        float glitch = clock % (critical ? 1.2f : 3.2f);
        nameImage.sprite = glitch < .05f ? assets.Get("name_text_glitch_1") : glitch < .1f ? assets.Get("name_text_glitch_2") : assets.Get("name_text");
        for (int i = 0; i < motes.Length; i++)
        {
            float t = (clock / 1.1f + i / (float)motes.Length) % 1f;
            motes[i].enabled = clock > .98f && health > 0f;
            motes[i].sprite = assets.Get("fx_mote_" + Mathf.Min(2, (int)(t * 3f)));
            float along = (i * .173f + .07f) % 1f;
            motes[i].rectTransform.anchoredPosition = new Vector2(32f + 332f * (1f - health + health * along), 43f + 22f * t);
            motes[i].color = new Color(1, 1, 1, 1f - t);
        }
    }
    private void Flash(bool value)
    {
        if (flashing == value || images == null) return;
        flashing = value;
        for (int i = 0; i < images.Length; i++)
            if (images[i] != takeover) images[i].material = value ? silhouetteMaterial : originalMaterials[i];
    }
    private void OnDisable() { active = false; Flash(false); }
}
