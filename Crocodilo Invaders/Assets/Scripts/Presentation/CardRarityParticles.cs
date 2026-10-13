using UnityEngine;
using UnityEngine.UI;

// Um único mesh de UI: evita instanciar um GameObject para cada partícula.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class CardRarityParticles : MaskableGraphic
{
    private float elapsed;
    private int tier;
    private float opacity;

    public void Draw(float time, int intensity, Color tint, float alpha)
    {
        elapsed = time;
        tier = intensity;
        color = tint;
        opacity = alpha;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        int count = 12 + tier * 6;
        for (int i = 0; i < count; i++)
        {
            float seed = Fraction(i * .6180339f + .23f);
            float life = Fraction(elapsed * (.21f + seed * .14f) + i * .371f);
            float x = (i % 2 == 0 ? -1f : 1f) * (132f + seed * 36f);
            float y = Mathf.Lerp(-218f, 228f, life);
            x += Mathf.Sin(elapsed * 1.2f + i * 2.1f) * 7f;
            float alpha = Mathf.Sin(life * Mathf.PI) * opacity * (.5f + tier * .13f);
            float size = i % 4 == 0 ? 4f : 2f;
            Color particleColor = Color.Lerp(color, Color.white, i % 3 == 0 ? .65f : .1f);
            particleColor.a = alpha;
            Square(mesh, new Vector2(Mathf.Round(x / 2f) * 2f, Mathf.Round(y / 2f) * 2f), size, particleColor);
        }
        // Pequena explosão de pixels que se dissipa após a revelação.
        if (elapsed >= .6f) return;
        float progress = elapsed / .6f;
        for (int i = 0; i < 10 + tier * 4; i++)
        {
            float angle = i * 2.39996f;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = new Vector2(direction.x * (140f + progress * 55f), direction.y * (205f + progress * 45f));
            Color flash = Color.Lerp(color, Color.white, .45f);
            flash.a = (1f - progress) * opacity;
            Square(mesh, new Vector2(Mathf.Round(position.x / 2f) * 2f, Mathf.Round(position.y / 2f) * 2f), 3f, flash);
        }
    }

    private static float Fraction(float value) => value - Mathf.Floor(value);
    private static void Square(VertexHelper mesh, Vector2 center, float size, Color tint)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(center + new Vector2(-size, -size), tint, Vector2.zero);
        mesh.AddVert(center + new Vector2(-size, size), tint, Vector2.up);
        mesh.AddVert(center + new Vector2(size, size), tint, Vector2.one);
        mesh.AddVert(center + new Vector2(size, -size), tint, Vector2.right);
        mesh.AddTriangle(index, index + 1, index + 2);
        mesh.AddTriangle(index, index + 2, index + 3);
    }
}
