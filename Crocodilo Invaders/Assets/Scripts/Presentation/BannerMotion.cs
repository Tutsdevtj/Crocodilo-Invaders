using System;
using UnityEngine;
using UnityEngine.UI;

// Trecho aprovado do movimento horizontal dos banners (wave_boss_ui.json), em px de tela de referência.
[Serializable]
public struct BannerKeyframe
{
    public enum Ease { Linear, OutCubic, InCubic }
    public float startMs, endMs, fromX, toX;
    public Ease ease;

    public BannerKeyframe(float startMs, float endMs, float fromX, float toX, Ease ease)
    {
        this.startMs = startMs; this.endMs = endMs; this.fromX = fromX; this.toX = toX; this.ease = ease;
    }
}

public static class BannerMotion
{
    // Mesmo cálculo do gerador das previews: x(t) por trechos, sem extrapolar além do último.
    public static float Evaluate(BannerKeyframe[] keys, float ms)
    {
        if (keys == null || keys.Length == 0) return 0f;
        for (int i = 0; i < keys.Length; i++)
        {
            BannerKeyframe key = keys[i];
            if (ms >= key.endMs && i < keys.Length - 1) continue;
            float span = key.endMs - key.startMs;
            float u = span > 0f ? Mathf.Clamp01((ms - key.startMs) / span) : 1f;
            return key.fromX + (key.toX - key.fromX) * Apply(key.ease, u);
        }
        return keys[keys.Length - 1].toX;
    }

    // px/ms, medido como nas previews: x(t + 1 ms) − x(t).
    public static float Velocity(BannerKeyframe[] keys, float ms) => Evaluate(keys, ms + 1f) - Evaluate(keys, ms);

    public static float Duration(BannerKeyframe[] keys) => keys == null || keys.Length == 0 ? 0f : keys[keys.Length - 1].endMs;

    public static float Apply(BannerKeyframe.Ease ease, float u)
    {
        switch (ease)
        {
            case BannerKeyframe.Ease.OutCubic: { float v = 1f - u; return 1f - v * v * v; }
            case BannerKeyframe.Ease.InCubic: return u * u * u;
            default: return u;
        }
    }

    // round() do Python (meio para par) seguido de "X -= X % 2": nunca quebra o pixel nativo 2x.
    public static int EvenFloor(float value)
    {
        int rounded = (int)Math.Round(value, MidpointRounding.ToEven);
        return rounded - ((rounded % 2) + 2) % 2;
    }

    public static int EvenFloor(int value) => value - ((value % 2) + 2) % 2;
}

// Linhas de velocidade s/m/l com dois quadros; o comprimento vem da velocidade do banner.
[Serializable]
public sealed class BannerSpeedlines
{
    [Tooltip("s_0, s_1, m_0, m_1, l_0, l_1")] public Sprite[] frames = new Sprite[6];
    public Image image;
    [Min(1f)] public float maxLength = 130f;
    public float hideBelow = 6f, smallBelow = 60f, mediumBelow = 105f, frameMs = 33f;

    public void Apply(float velocity, float ms, int pixelScale)
    {
        if (image == null) return;
        float length = Mathf.Min(maxLength, Mathf.Abs(velocity) * 10f);
        bool visible = length >= hideBelow;
        if (image.enabled != visible) image.enabled = visible;
        if (!visible) return;
        int size = length < smallBelow ? 0 : length < mediumBelow ? 1 : 2;
        int frame = Mathf.FloorToInt(ms / frameMs) & 1;
        BannerPieces.SetSprite(image, frames[size * 2 + frame], pixelScale);
    }

    public void Hide() { if (image != null) image.enabled = false; }
}

// Imagens UI de pixel art: pivô e âncora no canto superior esquerdo, tamanho nativo × escala.
public static class BannerPieces
{
    public static void SetSprite(Image image, Sprite sprite, int pixelScale)
    {
        if (image.sprite == sprite) return;
        image.sprite = sprite;
        if (sprite != null) image.rectTransform.sizeDelta = sprite.rect.size * pixelScale;
    }

    public static void Place(Image image, float nativeX, float nativeY, int pixelScale)
    {
        image.rectTransform.anchoredPosition = new Vector2(nativeX * pixelScale, -nativeY * pixelScale);
    }

    public static void Show(Image image, bool visible)
    {
        if (image != null && image.enabled != visible) image.enabled = visible;
    }
}
