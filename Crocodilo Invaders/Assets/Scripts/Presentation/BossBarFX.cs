using UnityEngine;
using UnityEngine.UI;

// Barra das duas fases do Gusini. Somente apresentação; HP continua no Health.
public sealed class BossBarFX : MonoBehaviour
{
    [SerializeField] private RectTransform motion, barGroup, emblemGroup, scarfGroup, nameGroup;
    [SerializeField] private CanvasGroup barAlpha, emblemAlpha, scarfAlpha, nameAlpha;
    [SerializeField] private Image healthFill, damageLag, hitFlash, lowGlow, emblemAlert, scarf;
    [SerializeField] private Image shimmer, fillEdge, glint;
    [SerializeField] private RectTransform shimmerMask;
    [SerializeField] private Image[] hitSparks;
    [SerializeField] private Sprite normalFill, criticalFill;
    [SerializeField] private Sprite[] scarfFrames, hitFrames, glintFrames;

    private MacaquiniBossBar jungle;
    public MacaquiniBossBar JungleView => jungle != null ? jungle : (jungle = GetComponent<MacaquiniBossBar>());
    private readonly float[] sparkStarted = new float[3];
    private float clock, targetHp, initialHp, shownHp, lagHoldUntil;
    private float flashStarted, shakeStarted, nextHitAt, shimmerStarted, nextShimmerAt, glintStarted;
    private bool chargeFinished, shimmerActive, alternateGlint;
    private bool phaseTwo, transformationHold;
    private static readonly Vector2 EmblemOrigin = new Vector2(418f, 46f);
    private const float FillWidth = 332f;
    private Sprite phaseOneFill, phaseOneCritical;
    private Sprite[] phaseOneScarf, phaseOneHit, phaseOneGlint;
    private void Awake()
    {
        phaseOneFill = normalFill; phaseOneCritical = criticalFill;
        phaseOneScarf = scarfFrames; phaseOneHit = hitFrames; phaseOneGlint = glintFrames;
    }

    // A cloned instance keeps the exact frame, anchoring and health feedback of the original HUD.
    public void SetIdentity(Sprite title, Sprite portrait)
    {
        if (JungleView != null) { return; }
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.name == "name_text" && title != null)
            {
                image.rectTransform.anchoredPosition += Vector2.right * (image.rectTransform.sizeDelta.x - title.rect.width) * .5f;
                image.rectTransform.sizeDelta = title.rect.size;
                image.sprite = title;
            }
            if (image.name == "emblem_roundel" && portrait != null) image.sprite = portrait;
        }
    }

    public void Show(float fraction)
    {
        if (JungleView != null) { JungleView.Show(fraction); return; }
        phaseTwo = transformationHold = false;
        if (phaseOneFill != null)
        {
            normalFill = phaseOneFill; criticalFill = phaseOneCritical;
            scarfFrames = phaseOneScarf; hitFrames = phaseOneHit; glintFrames = phaseOneGlint;
        }
        clock = 0f; targetHp = initialHp = Mathf.Clamp01(fraction); shownHp = 0f;
        chargeFinished = shimmerActive = alternateGlint = false;
        lagHoldUntil = nextHitAt = 0f; nextShimmerAt = 5.8f;
        flashStarted = shakeStarted = shimmerStarted = glintStarted = float.NegativeInfinity;
        healthFill.fillAmount = damageLag.fillAmount = hitFlash.fillAmount = 0f;
        foreach (var image in hitSparks) image.enabled = false;
        motion.anchoredPosition = Vector2.zero;
        Animate();
    }

    public void SetHealth(float fraction)
    {
        if (JungleView != null) { JungleView.SetHealth(fraction); return; }
        if (transformationHold) return;
        targetHp = Mathf.Clamp01(fraction);
        // Guarda dano durante a carga de entrada, sem perder o valor real.
        if (chargeFinished) ApplyHealth();
    }

    public void HoldEmpty()
    {
        if (JungleView != null) { JungleView.HoldEmpty(); return; }
        transformationHold = true; chargeFinished = true;
        targetHp = shownHp = 0f;
        healthFill.fillAmount = damageLag.fillAmount = hitFlash.fillAmount = 0f;
        shimmerActive = false;
    }

    public void BeginPhaseTwoCharge(float fraction, float elapsed = 0f)
    {
        if (JungleView != null) { JungleView.Reveal(fraction); return; }
        Show(fraction);
        phaseTwo = true;
        clock = Mathf.Max(0f, elapsed);
        nextShimmerAt = 3.5f;
    }

    public void ChangePhaseTwoSprites(GusiniPhaseTwoAssets assets)
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
            if (image.sprite != null && assets.Get(image.sprite.name) != null) image.sprite = assets.Get(image.sprite.name);
        normalFill = assets.Get("hp_fill"); criticalFill = assets.Get("hp_fill_low");
        scarfFrames = assets.Frames("orn_scarf_tail_", 4);
        hitFrames = assets.Frames("fx_hit_spark_", 3);
        glintFrames = assets.Frames("fx_glint_", 4);
    }

    private void ApplyHealth()
    {
        if (targetHp < shownHp)
        {
            lagHoldUntil = clock + .4f;
            if (clock >= nextHitAt)
            {
                flashStarted = shakeStarted = clock;
                nextHitAt = clock + .08f;
                for (int i = 0; i < hitSparks.Length; i++)
                {
                    if (hitSparks[i].enabled) continue;
                    sparkStarted[i] = clock;
                    hitSparks[i].rectTransform.anchoredPosition = new Vector2(Edge(targetHp), 43f);
                    hitSparks[i].sprite = hitFrames[0]; hitSparks[i].enabled = true;
                    break;
                }
            }
        }
        else if (targetHp > shownHp) damageLag.fillAmount = targetHp;
        shownHp = targetHp;
        healthFill.fillAmount = hitFlash.fillAmount = shownHp;
        RefreshFill();
    }

    private void Update()
    {
        if (JungleView != null) { return; }
        clock += Time.deltaTime;
        if (!chargeFinished)
        {
            float chargeStart = phaseTwo ? .4f : .5f, chargeEnd = phaseTwo ? .98f : 1.1f;
            shownHp = initialHp * OutCubic(Mathf.InverseLerp(chargeStart, chargeEnd, clock));
            healthFill.fillAmount = damageLag.fillAmount = hitFlash.fillAmount = shownHp;
            if (clock >= chargeEnd)
            {
                chargeFinished = true;
                shownHp = initialHp;
                ApplyHealth();
                BeginGlint(true);
            }
        }
        else if (clock >= lagHoldUntil)
            damageLag.fillAmount = Mathf.MoveTowards(damageLag.fillAmount, shownHp, .8f * Time.deltaTime);
        Animate();
    }

    private void Animate()
    {
        float barT = Mathf.Clamp01(clock / .28f);
        barGroup.anchoredPosition = new Vector2(120f * (1f - OutCubic(barT)), 0f);
        barAlpha.alpha = barT;
        float emblemT = Mathf.InverseLerp(.18f, .4f, clock);
        Vector2 drop = new Vector2(0f, 20f * (1f - OutBack(emblemT)));
        emblemGroup.anchoredPosition = EmblemOrigin + drop;
        scarfGroup.anchoredPosition = drop;
        emblemAlpha.alpha = scarfAlpha.alpha = emblemT;
        float nameT = Mathf.InverseLerp(.32f, .46f, clock);
        nameGroup.anchoredPosition = new Vector2(0f, 12f * (1f - OutCubic(nameT)));
        nameAlpha.alpha = nameT;
        if (phaseTwo || transformationHold)
        {
            barGroup.anchoredPosition = nameGroup.anchoredPosition = scarfGroup.anchoredPosition = Vector2.zero;
            emblemGroup.anchoredPosition = EmblemOrigin;
            barAlpha.alpha = emblemAlpha.alpha = scarfAlpha.alpha = nameAlpha.alpha = 1f;
        }

        bool critical = chargeFinished && shownHp > 0f && shownHp <= .3f;
        healthFill.sprite = critical ? criticalFill : normalFill;
        lowGlow.enabled = emblemAlert.enabled = critical;
        float pulse = .5f - .5f * Mathf.Cos(clock * 2f * Mathf.PI / (phaseTwo ? .7f : .8f));
        Alpha(lowGlow, Mathf.Lerp(.15f, .9f, pulse));
        Alpha(emblemAlert, Mathf.Lerp(.25f, 1f, pulse));
        scarf.sprite = scarfFrames[(int)(Mathf.Max(0f, clock - .18f) / (phaseTwo ? (critical ? .07f : .1f) : (critical ? .08f : .12f))) % scarfFrames.Length];
        fillEdge.enabled = clock >= (phaseTwo ? .4f : .5f) && !chargeFinished && shownHp > 0f;
        fillEdge.rectTransform.anchoredPosition = new Vector2(Edge(shownHp), 34f);
        float flashAt = phaseTwo ? .98f : 1.1f;
        float entranceFlash = clock >= flashAt && clock < flashAt + .2f ? .9f * (1f - (clock - flashAt) / .2f) : 0f;
        Alpha(hitFlash, Mathf.Max(entranceFlash, .85f * Mathf.Clamp01(1f - (clock - flashStarted) / .12f)));

        float shakeAge = clock - shakeStarted;
        motion.anchoredPosition = shakeAge < .1f
            ? new Vector2(Mathf.Sin(shakeAge * 170f), Mathf.Cos(shakeAge * 210f)) * (2f * (1f - shakeAge / .1f))
            : Vector2.zero;
        for (int i = 0; i < hitSparks.Length; i++)
        {
            if (!hitSparks[i].enabled) continue;
            int frame = (int)((clock - sparkStarted[i]) / .05f);
            if (frame >= hitFrames.Length) hitSparks[i].enabled = false;
            else hitSparks[i].sprite = hitFrames[frame];
        }

        if (clock >= nextShimmerAt)
        {
            nextShimmerAt = clock + (phaseTwo ? 3.5f : 4.5f);
            if (!critical && shownHp > 0f) { shimmerStarted = clock; shimmerActive = true; }
        }
        float shimmerAge = clock - shimmerStarted;
        if (shimmerActive && (critical || shownHp <= 0f)) shimmerActive = false;
        float shimmerDuration = phaseTwo ? .45f : .55f;
        if (shimmerActive && shimmerAge >= shimmerDuration)
        {
            shimmerActive = false; BeginGlint(!alternateGlint); alternateGlint = !alternateGlint;
        }
        shimmer.enabled = shimmerActive;
        shimmerMask.anchoredPosition = new Vector2(Edge(shownHp), 34f);
        shimmerMask.sizeDelta = new Vector2(FillWidth * shownHp, 18f);
        shimmer.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(FillWidth * shownHp, -18f, Mathf.Clamp01(shimmerAge / shimmerDuration)), 0f);
        int glintFrame = (int)((clock - glintStarted) / .06f);
        glint.enabled = clock - glintStarted < glintFrames.Length * .06f;
        if (glint.enabled) glint.sprite = glintFrames[Mathf.Clamp(glintFrame, 0, glintFrames.Length - 1)];
    }

    private void RefreshFill()
    {
        // A máscara acompanha também danos que chegam entre dois Updates.
        shimmerMask.anchoredPosition = new Vector2(Edge(shownHp), 34f);
        shimmerMask.sizeDelta = new Vector2(FillWidth * shownHp, 18f);
    }

    private void BeginGlint(bool onEmblem)
    {
        glintStarted = clock;
        glint.rectTransform.anchoredPosition = onEmblem ? new Vector2(404f, 62f) : new Vector2(10f, 52f);
    }

    private void OnDisable()
    {
        if (JungleView != null) { return; }
        shimmerActive = false;
        motion.anchoredPosition = Vector2.zero;
        shimmer.enabled = fillEdge.enabled = glint.enabled = false;
        foreach (var image in hitSparks) image.enabled = false;
    }

    private static float Edge(float hp) => 32f + FillWidth * (1f - hp);
    private static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float OutBack(float t)
    {
        const float overshoot = 1.9f;
        float x = t - 1f; return 1f + (overshoot + 1f) * x * x * x + overshoot * x * x;
    }
    private static void Alpha(Image image, float alpha)
    {
        var color = image.color; color.a = alpha; image.color = color;
    }
}
