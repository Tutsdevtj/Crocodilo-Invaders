using UnityEngine;
using UnityEngine.UI;

// Dedicated jungle skin. Health and form changes still come from the boss controllers.
public sealed class MacaquiniBossBar : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image fill, lag, sheen, portraitOne, portraitTwo, accent, frameTwo;
    [SerializeField] private TMPro.TMP_Text formLabel;
    [SerializeField] private CanvasGroup entrance;
    private MacaquiniFlightPresentation actor;
    private float clock, target, shown, lagDelay, blend, chargeStart;
    private bool held, charging;
    private Sprite meterPixel;
    public float FormBlend => blend;
    public float ShownHealth => shown;
    public float PortraitSize => portraitOne.rectTransform.sizeDelta.x;
    public bool HoldingEmpty => held;
    public Sprite FirstPortrait => portraitOne.sprite;
    public Sprite SecondPortrait => portraitTwo.sprite;
    public Color FillColor => fill.color;
    public RectTransform Bounds => (RectTransform)transform;
    private static readonly Color Infected = new Color(.33f, .85f, .16f);
    private static readonly Color Primal = new Color(.94f, .66f, .16f);

    private void Awake()
    {
        // A flat native UI primitive avoids the rounded button texture previously stretching inside the track.
        meterPixel = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,100);
        foreach(var image in new[]{fill,lag,sheen,accent}) image.sprite=meterPixel;
    }

    public void Bind(MacaquiniFlightPresentation source)
    {
        actor = source; blend = 0f;
        SetBlend(source != null && source.HalfShell ? 1f : 0f);
    }
    public void Show(float fraction)
    {
        clock = 0f; target = Mathf.Clamp01(fraction); shown = 0f;
        held = false; charging = true; chargeStart = .35f; lagDelay = 0f;
        fill.fillAmount = lag.fillAmount = 0f; entrance.alpha = 0f;
    }
    public void SetHealth(float fraction)
    {
        if (held) return;
        target = Mathf.Clamp01(fraction);
        if (!charging) { if (target < shown) lagDelay = .16f; shown = target; fill.fillAmount = shown; }
    }
    public void HoldEmpty()
    {
        held = true; charging = false; target = shown = 0f;
        fill.fillAmount = lag.fillAmount = 0f;
    }
    public void Reveal(float fraction)
    {
        held = false; charging = true; target = Mathf.Clamp01(fraction);
        shown = 0f; fill.fillAmount = lag.fillAmount = 0f; chargeStart = clock + .15f;
        SetBlend(1f);
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        clock += dt; entrance.alpha = Mathf.Clamp01(clock / .28f);
        if (actor != null)
        {
            // Follow the protected chronological tear progress; never restart the health entrance.
            float desired = actor.HalfShell ? 1f : actor.Transforming
                ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.24f, .73f, actor.TearProgress)) : 0f;
            SetBlend(Mathf.Max(blend, desired));
        }
        if (charging)
        {
            float t = Mathf.Clamp01((clock - chargeStart) / .65f);
            shown = target * (1f - Mathf.Pow(1f - t, 3f));
            fill.fillAmount = lag.fillAmount = shown;
            if (t >= 1f) charging = false;
        }
        lagDelay -= dt;
        if (!charging && lagDelay <= 0f) lag.fillAmount = Mathf.MoveTowards(lag.fillAmount, shown, dt * 1.8f);
        Color tint = Color.Lerp(Infected, Primal, blend);
        bool critical = shown > 0f && shown <= .3f;
        fill.color = critical ? new Color(.92f, .27f, .12f) : tint;
        accent.color = Color.Lerp(fill.color,Color.black,.4f); accent.fillAmount=shown;
        // Small light variation on the sap, never a full portrait flash or scale pulse.
        Color light=Color.Lerp(fill.color,Color.white,.4f);
        sheen.color = new Color(light.r,light.g,light.b,shown>0f?.65f:0f);
        sheen.fillAmount = shown;
    }
    private void SetBlend(float value)
    {
        blend = Mathf.Clamp01(value);
        // Opaque first layer beneath the second keeps the portrait from dimming halfway through.
        portraitOne.color = Color.white;
        portraitTwo.color = new Color(1f, 1f, 1f, blend);
        frameTwo.color = new Color(1f, 1f, 1f, blend);
        formLabel.text = blend < .5f ? "I  ·  CASCA INFECTADA" : "II  ·  FÚRIA DA SELVA";
    }
    private void OnDisable() { actor = null; }
    private void OnDestroy(){if(meterPixel!=null)Destroy(meterPixel);}
}
