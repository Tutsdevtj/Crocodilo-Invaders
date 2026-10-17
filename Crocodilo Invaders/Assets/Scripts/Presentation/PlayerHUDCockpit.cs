using UnityEngine;
using UnityEngine.UI;

// Apresentação apenas: recebe HP/SP do PlayerHUD, sem modificar o gameplay.
public sealed class PlayerHUDCockpit : MonoBehaviour
{
    [Header("Integridade")]
    [SerializeField] private Image healthFill, damageLag, healFlash, emblem, dialCrack, lowHealthGlow;
    [SerializeField] private Sprite healthNormal, healthLow, emblemNormal, emblemLow;
    [Header("Reservatório")]
    [SerializeField] private Image specialFill, refillFlash, wave, fan, lamp, fullGlow;
    [SerializeField] private RectTransform liquidMask;
    [SerializeField] private Image[] bubbles;
    [SerializeField] private Sprite[] chargeSprites, waveFrames, bubbleFrames, fanFrames, lampSprites;
    [Header("Turbina")]
    [SerializeField] private Image exhaust, activation;
    [SerializeField] private Sprite[] flameFrames, activationFrames, shutdownFrames;
    [SerializeField] private Image[] sparks;
    [SerializeField] private Sprite[] sparkSprites;

    private readonly System.Random visualRandom = new System.Random();
    private readonly Vector2[] sparkVelocities = new Vector2[9];
    private readonly float[] sparkAges = new float[9], sparkLifetimes = new float[9];
    private float hp, sp, clock, lagHoldUntil, healUntil, refillUntil, rechargeUntil;
    private float fullSince, shutdownSince = float.NegativeInfinity, sparkAccumulator;
    private const float LiquidWidth = 206f;

    public void ResetDisplay(float health, float charge)
    {
        hp = Mathf.Clamp01(health); sp = Mathf.Clamp01(charge);
        clock = lagHoldUntil = healUntil = refillUntil = rechargeUntil = sparkAccumulator = 0f;
        fullSince = -.3f; shutdownSince = float.NegativeInfinity;
        healthFill.fillAmount = damageLag.fillAmount = healFlash.fillAmount = hp;
        specialFill.fillAmount = refillFlash.fillAmount = sp;
        foreach (var image in sparks) image.enabled = false;
        foreach (var image in bubbles) image.enabled = false;
        RefreshHealth(); RefreshCharge(); Animate(0f);
    }

    public void SetHealth(float fraction)
    {
        float next = Mathf.Clamp01(fraction);
        if (next < hp)
        {
            lagHoldUntil = clock + .35f;
            healUntil = 0f;
        }
        else if (next > hp)
        {
            damageLag.fillAmount = next;
            healUntil = clock + .25f;
        }
        hp = next;
        healthFill.fillAmount = healFlash.fillAmount = hp;
        RefreshHealth();
    }

    public void SetCharge(float fraction)
    {
        float next = Mathf.Clamp01(fraction);
        if (next > sp)
        {
            refillUntil = clock + .2f;
            rechargeUntil = clock + .35f;
        }
        if (sp < 1f && next >= 1f)
        {
            fullSince = clock;
            shutdownSince = float.NegativeInfinity;
            sparkAccumulator = 0f;
            for (int i = 0, count = visualRandom.Next(6, 10); i < count; i++) SpawnSpark();
        }
        else if (sp >= 1f && next < 1f)
        {
            shutdownSince = clock;
            refillUntil = rechargeUntil = 0f;
        }
        sp = next;
        specialFill.fillAmount = refillFlash.fillAmount = sp;
        RefreshCharge();
        Animate(0f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        clock += dt;
        if (clock >= lagHoldUntil)
            damageLag.fillAmount = Mathf.MoveTowards(damageLag.fillAmount, hp, .9f * dt);
        Animate(dt);
    }

    private void RefreshHealth()
    {
        bool critical = hp <= .25f;
        healthFill.sprite = critical ? healthLow : healthNormal;
        emblem.sprite = critical ? emblemLow : emblemNormal;
        dialCrack.enabled = critical;
        lowHealthGlow.enabled = critical;
    }

    private void RefreshCharge()
    {
        int state = sp >= 1f ? 2 : sp >= 1f / 3f ? 1 : 0;
        specialFill.sprite = chargeSprites[state];
        lamp.sprite = lampSprites[state];
        liquidMask.sizeDelta = new Vector2(LiquidWidth * sp, 14f);
        wave.enabled = sp > 0f;
        fullGlow.enabled = sp >= 1f;
    }

    private void Animate(float dt)
    {
        Alpha(healFlash, Mathf.Clamp01((healUntil - clock) / .25f));
        Alpha(refillFlash, Mathf.Clamp01((refillUntil - clock) / .2f));
        Alpha(lowHealthGlow, Pulse(.9f, .25f));
        Alpha(fullGlow, Pulse(1.2f, .35f));
        wave.sprite = waveFrames[(int)(clock / .1f) % waveFrames.Length];
        fan.sprite = fanFrames[sp < 1f / 3f ? 0 : (int)(clock / (sp >= 1f ? .05f : .08f)) % fanFrames.Length];

        // Um pool fixo de três bolhas, visível somente no ganho recente de carga.
        bool recharging = sp > 0f && sp < 1f && clock < rechargeUntil;
        for (int i = 0; i < bubbles.Length; i++)
        {
            var image = bubbles[i]; image.enabled = recharging;
            if (!recharging) continue;
            float age = Mathf.Repeat(clock + i * .07f, .22f);
            float x = Mathf.Repeat(i * 63f + clock * 80f, Mathf.Max(1f, LiquidWidth * sp));
            image.rectTransform.anchoredPosition = new Vector2(x, age * 80f - 3f);
            image.sprite = bubbleFrames[(int)(age / .1f) % bubbleFrames.Length];
        }

        float fullAge = clock - fullSince, shutdownAge = clock - shutdownSince;
        activation.enabled = sp >= 1f && fullAge < .3f;
        if (activation.enabled) activation.sprite = activationFrames[Mathf.Min(activationFrames.Length - 1, (int)(fullAge / .1f))];
        if (sp >= 1f && fullAge >= .3f)
        {
            exhaust.enabled = true;
            exhaust.sprite = flameFrames[(int)((fullAge - .3f) / .08f) % flameFrames.Length];
        }
        else if (sp < 1f && shutdownAge < .4f)
        {
            exhaust.enabled = true;
            exhaust.sprite = shutdownFrames[Mathf.Min(shutdownFrames.Length - 1, (int)(shutdownAge / .1f))];
        }
        else exhaust.enabled = false;

        for (int i = 0; i < sparks.Length; i++)
        {
            if (!sparks[i].enabled) continue;
            sparkAges[i] += dt;
            if (sparkAges[i] >= sparkLifetimes[i]) { sparks[i].enabled = false; continue; }
            sparks[i].rectTransform.anchoredPosition += sparkVelocities[i] * dt;
            Alpha(sparks[i], 1f - sparkAges[i] / sparkLifetimes[i]);
        }
        if (sp >= 1f)
        {
            sparkAccumulator += dt * 2.5f;
            if (sparkAccumulator >= 1f) { sparkAccumulator -= 1f; SpawnSpark(); }
        }
    }

    private void SpawnSpark()
    {
        for (int i = 0; i < sparks.Length; i++)
        {
            if (sparks[i].enabled) continue;
            sparks[i].sprite = sparkSprites[visualRandom.Next(sparkSprites.Length)];
            sparks[i].SetNativeSize();
            sparks[i].rectTransform.anchoredPosition = new Vector2(356f, 26f);
            float angle = Mathf.Lerp(-25f, 25f, (float)visualRandom.NextDouble()) * Mathf.Deg2Rad;
            float speed = Mathf.Lerp(50f, 120f, (float)visualRandom.NextDouble());
            sparkVelocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            sparkAges[i] = 0f; sparkLifetimes[i] = Mathf.Lerp(.25f, .5f, (float)visualRandom.NextDouble());
            Alpha(sparks[i], 1f); sparks[i].enabled = true;
            return;
        }
    }

    private float Pulse(float period, float minimum)
        => Mathf.Lerp(minimum, 1f, .5f - .5f * Mathf.Cos(clock * 2f * Mathf.PI / period));

    private static void Alpha(Image image, float value)
    {
        var color = image.color; color.a = value; image.color = color;
    }
}
