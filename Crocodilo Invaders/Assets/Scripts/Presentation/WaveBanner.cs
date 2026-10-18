using UnityEngine;
using UnityEngine.UI;

// Anúncio "ONDA N" montado com os PNGs aprovados. Não tem relógio próprio: WaveAnnouncer chama
// Evaluate com o tempo da sequência, assim pausa, pulo e sincronização ficam num único lugar.
public sealed class WaveBanner : MonoBehaviour
{
    [SerializeField] private WaveBannerStyle style;
    [SerializeField] private RectTransform root;
    [SerializeField] private Image speedlines;
    [SerializeField] private Image barHead;
    [SerializeField] private Image barMid;
    [SerializeField] private Image barEnd;
    [SerializeField] private Image word;
    [SerializeField] private RectTransform digitsRoot;
    [SerializeField] private Image[] digitImages = new Image[0];
    [SerializeField] private Image burst;

    private readonly int[] digitValues = new int[WaveBannerStyle.MaxDigits];
    private readonly int[] digitX = new int[WaveBannerStyle.MaxDigits];
    private readonly BannerSpeedlines lines = new BannerSpeedlines();
    private WaveBannerStyle.Layout layout;
    private int pose = -1;

    public WaveBannerStyle Style => style;
    public RectTransform Root => root;
    public WaveBannerStyle.Layout CurrentLayout => layout;
    public float Duration => style != null ? style.Duration : 0f;
    public bool Visible => root != null && root.gameObject.activeSelf;
    public Image DigitImage(int index) => digitImages[index];
    public Image Burst => burst;
    public Image Speedlines => speedlines;
    public Image BarMid => barMid;
    public Image BarEnd => barEnd;

    private void Awake() => Hide();

    public void Prepare(int wave)
    {
        int scale = style.pixelScale;
        layout = style.Compose(wave, digitValues, digitX);
        EnsureDigits(layout.digitCount);
        BannerPieces.SetSprite(barHead, style.barHead, scale);
        BannerPieces.Place(barHead, style.barPosition.x, style.barPosition.y, scale);
        // A coluna central estica só em X; os algarismos nunca são comprimidos.
        int midX = style.barPosition.x + style.barHeadWidth;
        int midWidth = style.EndX(layout.barLength) - midX;
        BannerPieces.SetSprite(barMid, style.barMid, scale);
        barMid.rectTransform.sizeDelta = new Vector2(Mathf.Max(0, midWidth) * scale, style.barMid.rect.height * scale);
        BannerPieces.Place(barMid, midX, style.barPosition.y, scale);
        BannerPieces.Show(barMid, midWidth > 0);
        BannerPieces.SetSprite(barEnd, style.barEnd, scale);
        BannerPieces.Place(barEnd, style.EndX(layout.barLength), style.barPosition.y, scale);
        BannerPieces.SetSprite(word, style.word, scale);
        BannerPieces.Place(word, style.wordPosition.x, style.wordPosition.y, scale);
        BannerPieces.SetSprite(burst, style.burst, scale);
        BannerPieces.Show(barHead, true);
        BannerPieces.Show(barEnd, true);
        BannerPieces.Show(word, true);
        speedlines.rectTransform.anchoredPosition = new Vector2(layout.speedlinesX * scale, 0f);
        for (int i = 0; i < digitImages.Length; i++) BannerPieces.Show(digitImages[i], i < layout.digitCount);
        lines.image = speedlines;
        lines.frames = style.speedlines;
        lines.maxLength = style.speedlinesMaxLength;
        lines.Hide();
        pose = -1;
        root.gameObject.SetActive(true);
        Evaluate(0f);
    }

    public void Evaluate(float ms)
    {
        int scale = style.pixelScale;
        float x = BannerMotion.Evaluate(style.motion, ms);
        // X_banner = 600 − centro·2 + deslocamento; relativo à âncora no centro superior do Canvas.
        int screenX = BannerMotion.EvenFloor(style.screenCenterX - layout.CenterX * scale + x);
        root.anchoredPosition = new Vector2(screenX - style.screenCenterX, -style.screenTop);
        lines.Apply(BannerMotion.Velocity(style.motion, ms), ms, scale);
        int nextPose = ms >= style.flashStartMs && ms < style.pop1StartMs ? 2 : ms >= style.pop1StartMs && ms < style.settledMs ? 1 : 0;
        bool burstVisible = ms >= style.burstStartMs;
        if (nextPose != pose) ApplyPose(nextPose);
        BannerPieces.Show(burst, burstVisible);
    }

    public void Hide()
    {
        if (root != null) root.gameObject.SetActive(false);
        lines.Hide();
    }

    // Ênfase do assentamento: troca de sprite (flash +6 px, pop1 +3 px), com o deslocamento do manifesto.
    private void ApplyPose(int next)
    {
        pose = next;
        int scale = style.pixelScale;
        Vector2Int variant = Vector2Int.zero;
        for (int i = 0; i < layout.digitCount; i++)
        {
            WaveBannerStyle.Digit digit = style.digits[digitValues[i]];
            Sprite sprite = next == 2 ? digit.flash : next == 1 ? digit.pop1 : digit.normal;
            variant = next == 2 ? digit.flashOffset : next == 1 ? digit.pop1Offset : Vector2Int.zero;
            BannerPieces.SetSprite(digitImages[i], sprite, scale);
            BannerPieces.Place(digitImages[i], digitX[i] + variant.x, style.digitsTop + variant.y, scale);
        }
        // O estalo acompanha o topo do último algarismo; o x compensa o alargamento da variante.
        int burstY = Mathf.Max(0, layout.burstY + variant.y);
        BannerPieces.Place(burst, layout.burstX - variant.x, burstY, scale);
    }

    private void EnsureDigits(int count)
    {
        if (digitImages.Length >= count) return;
        int previous = digitImages.Length;
        System.Array.Resize(ref digitImages, count);
        for (int i = previous; i < count; i++)
        {
            var image = Instantiate(digitImages[0], digitsRoot);
            image.name = "Digit" + i;
            digitImages[i] = image;
        }
    }
}
