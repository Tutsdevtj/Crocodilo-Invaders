using System;
using UnityEngine;

// Peças e regras do anúncio "ONDA N". Valores importados de SourceArt/WaveBossUI/wave_boss_ui.json
// (Crocodilo/UI/Install wave and boss announcements); coordenadas em px nativos, y para baixo.
[CreateAssetMenu(menuName = "Crocodilo/UI/Wave Banner Style")]
public sealed class WaveBannerStyle : ScriptableObject
{
    [Serializable]
    public sealed class Digit
    {
        public Sprite normal, pop1, flash;
        [Tooltip("x do algarismo quando é o primeiro do número.")] public int firstX;
        [Tooltip("Comprimento da faixa a partir deste algarismo quando ele é o último.")] public int barEndOffset;
        public Vector2Int burstOffset, pop1Offset, flashOffset;
    }

    public struct Layout
    {
        public int digitCount, barLength, burstX, burstY, bboxMin, bboxMax, speedlinesX;
        public float CenterX => (bboxMin + bboxMax) * .5f;
    }

    public const int MaxDigits = 10;

    [Header("Peças")]
    public Sprite barHead;
    public Sprite barMid;
    public Sprite barEnd;
    public Sprite word;
    public Sprite burst;
    [Tooltip("s_0, s_1, m_0, m_1, l_0, l_1")] public Sprite[] speedlines = new Sprite[6];
    public Digit[] digits = new Digit[10];
    [Tooltip("Avanço do par AB em kerning[A * 10 + B].")] public int[] kerning = new int[100];

    [Header("Montagem (px nativos)")]
    public Vector2Int barPosition = new Vector2Int(14, 24);
    public int barHeadWidth = 138, barEndBack = 12, minBarLength = 150;
    public Vector2Int wordPosition = new Vector2Int(40, 32);
    public int digitsTop = 14, speedlinesGap = 50;

    [Header("Tela de referência")]
    public int pixelScale = 2;
    public float screenCenterX = 600f, screenTop = 160f;

    [Header("Tempo (ms)")]
    public BannerKeyframe[] motion =
    {
        new BannerKeyframe(0, 180, 760, 24, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(180, 230, 24, -8, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(230, 280, -8, 0, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(280, 800, 0, -16, BannerKeyframe.Ease.Linear),
        new BannerKeyframe(800, 840, -16, -8, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(840, 1050, -8, -1000, BannerKeyframe.Ease.InCubic),
    };
    public float flashStartMs = 180f, pop1StartMs = 213f, settledMs = 246f, burstStartMs = 180f;
    [Tooltip("Pular antes deste ponto salta para ele: a saída sempre toca.")] public float skipToMs = 800f;
    public float speedlinesMaxLength = 130f;

    public float Duration => BannerMotion.Duration(motion);

    // Separa os algarismos sem alocar; o mais significativo fica em digitsOut[0].
    public static int Split(int value, int[] digitsOut)
    {
        value = Mathf.Max(0, value);
        int count = 0;
        do { digitsOut[count++] = value % 10; value /= 10; } while (value > 0 && count < MaxDigits);
        Array.Reverse(digitsOut, 0, count);
        return count;
    }

    // Regras do guia §3.2: o primeiro algarismo usa first_x, os seguintes o kerning do par,
    // a faixa cresce (nunca comprime algarismos) e o centro é o da caixa do conjunto.
    public Layout Compose(int wave, int[] digitValues, int[] digitX)
    {
        var layout = new Layout { digitCount = Split(wave, digitValues) };
        int x = digits[digitValues[0]].firstX;
        for (int i = 0; i < layout.digitCount; i++)
        {
            digitX[i] = x;
            if (i + 1 < layout.digitCount) x += kerning[digitValues[i] * 10 + digitValues[i + 1]];
        }
        int last = digitValues[layout.digitCount - 1];
        int lastX = digitX[layout.digitCount - 1];
        Digit lastDigit = digits[last];
        layout.barLength = Mathf.Max(minBarLength, lastX + lastDigit.barEndOffset - barPosition.x);
        layout.burstX = lastX + lastDigit.burstOffset.x;
        layout.burstY = digitsTop + lastDigit.burstOffset.y;
        layout.speedlinesX = barPosition.x + layout.barLength + speedlinesGap;

        int min = Mathf.Min(barPosition.x, wordPosition.x, layout.burstX);
        int max = Mathf.Max(EndX(layout.barLength) + Width(barEnd), wordPosition.x + Width(word), layout.burstX + Width(burst));
        for (int i = 0; i < layout.digitCount; i++)
        {
            min = Mathf.Min(min, digitX[i]);
            max = Mathf.Max(max, digitX[i] + Width(digits[digitValues[i]].normal));
        }
        layout.bboxMin = min;
        layout.bboxMax = max;
        return layout;
    }

    public int EndX(int barLength) => barPosition.x + barLength - barEndBack;
    public static int Width(Sprite sprite) => sprite != null ? Mathf.RoundToInt(sprite.rect.width) : 0;
}
