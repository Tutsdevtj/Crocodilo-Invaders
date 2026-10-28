using UnityEngine;
using UnityEngine.UI;

// Uma malha reutilizada para toda a energia externa, sem objetos por partícula.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class CardRarityParticles : MaskableGraphic
{
    private float elapsed, burstAge, opacity, focus;
    private int tier;
    private Vector2 halfSize = new Vector2(125f, 201f);
    public void SetCardSize(Vector2 size) => halfSize = size * .5f;
    public void Draw(float time, int intensity, Color tint, float alpha, float impactAge, float highlighted)
    {
        elapsed = time; tier = intensity; color = tint; opacity = alpha;
        burstAge = impactAge; focus = highlighted;
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (opacity <= 0f) return;
        if (tier > 0)
        {
            Sparks(mesh);
            if (tier >= 2) Crown(mesh);
            if (tier >= 3) Rays(mesh);
            if (tier == 4) Lightning(mesh);
        }
        Impact(mesh);
    }
    private void Sparks(VertexHelper mesh)
    {
        for (int i = 0; i < 6 + tier * 7; i++)
        {
            float seed = Fraction(i * .6180339f + .23f);
            float life = Fraction(elapsed * (.15f + tier * .025f + seed * .12f) + i * .371f);
            float side = i % 2 == 0 ? -1f : 1f;
            Vector2 position = new Vector2(side * (halfSize.x + 7f + seed * (9f + tier * 4f)),
                Mathf.Lerp(-halfSize.y - 5f, halfSize.y + 10f, life));
            position.x += Mathf.Sin(elapsed * 2f + i * 2.1f) * (2f + tier);
            Color tint = Tint(Mathf.Sin(life * Mathf.PI) * (.45f + tier * .1f), i % 3 == 0 ? .8f : .15f);
            float size = i % 5 == 0 ? 2.8f : 1.4f;
            if (tier >= 3 && i % 4 == 0) Star(mesh, position, size * 2f, tint);
            else Quad(mesh, position, new Vector2(size, size + tier * .4f), tint);
        }
        if (tier == 1) Star(mesh, Edge(elapsed * 1.8f, 7f), 4f + focus * 2f, Tint(.9f, .8f));
    }
    private void Crown(VertexHelper mesh)
    {
        int count = tier == 4 ? 24 : 16;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float wave = .5f + .5f * Mathf.Sin(elapsed * (2f + tier) - i * .8f);
            Vector2 start = Edge(angle, 5f);
            Vector2 end = Edge(angle, 12f + wave * (tier * 4f + focus * 7f));
            Beam(mesh, start, end, 2f + tier, Tint(.16f + wave * .28f + focus * .12f, .12f));
            if (tier >= 3 && i % 2 == 0) Star(mesh, end, 2f + wave * 2f, Tint(wave * .65f, .7f));
        }
    }
    private void Rays(VertexHelper mesh)
    {
        int count = tier == 4 ? 20 : 12;
        float impact = Mathf.Exp(-burstAge * 3.4f);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count + .08f * Mathf.Sin(elapsed * .6f);
            float wave = .5f + .5f * Mathf.Sin(elapsed * 1.7f + i * 2.4f);
            Color tint = Tint(.08f + wave * .12f + impact * .45f + focus * .08f, .18f);
            Beam(mesh, Edge(angle, 3f), Edge(angle, 22f + wave * 10f + impact * 17f),
                (tier == 4 ? 10f : 6f) + impact * 9f, tint);
        }
    }
    private void Lightning(VertexHelper mesh)
    {
        if (burstAge > .55f && Fraction(elapsed / 2.7f) > .12f) return;
        int phase = Mathf.FloorToInt(elapsed * 18f);
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 previous = new Vector2(side * (halfSize.x + 10f), -halfSize.y - 5f);
            for (int j = 1; j <= 12; j++)
            {
                Vector2 next = new Vector2(side * (halfSize.x + 10f + Fraction(j * .733f + phase * .37f) * 22f),
                    Mathf.Lerp(-halfSize.y - 5f, halfSize.y + 8f, j / 12f));
                Segment(mesh, previous, next, 4f, Tint(.25f, .1f));
                Segment(mesh, previous, next, 1.25f, Tint(.95f, .86f));
                previous = next;
            }
        }
    }
    private void Impact(VertexHelper mesh)
    {
        float duration = .35f + tier * .14f;
        if (burstAge >= duration) return;
        float t = Mathf.Clamp01(burstAge / duration);
        for (int i = 0; i < 8 + tier * 8; i++)
        {
            float angle = i * 2.39996f;
            Vector2 start = Edge(angle, 3f + t * (12f + tier * 7f));
            Vector2 end = Edge(angle, 7f + t * (18f + tier * 8f));
            Segment(mesh, start, end, (1f - t) * (1.2f + tier * .5f), Tint((1f - t) * .95f, .75f));
            if (tier >= 2 && i % 4 == 0) Star(mesh, end, (1f - t) * 6f, Tint(1f - t, .9f));
        }
    }
    private Vector2 Edge(float angle, float margin)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        // Expansão lateral livre; limite vertical protege título e comparações.
        Vector2 size = halfSize + new Vector2(margin, Mathf.Min(margin, 12f));
        return direction / Mathf.Max(Mathf.Abs(direction.x) / size.x, Mathf.Abs(direction.y) / size.y);
    }
    private Color Tint(float alpha, float white)
    {
        Color tint = Color.Lerp(color, Color.white, white);
        tint.a = Mathf.Clamp01(alpha * opacity);
        return tint;
    }
    private static float Fraction(float value) => value - Mathf.Floor(value);
    private static void Star(VertexHelper mesh, Vector2 center, float size, Color tint)
    {
        Quad(mesh, center, new Vector2(size * .22f, size), tint);
        Quad(mesh, center, new Vector2(size, size * .22f), tint);
    }
    private static void Beam(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized * width;
        Color transparent = tint; transparent.a = 0f;
        AddQuad(mesh, start - normal, start + normal, end + normal * .12f, end - normal * .12f, tint, transparent);
    }
    private static void Segment(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized * width;
        AddQuad(mesh, start - normal, start + normal, end + normal, end - normal, tint, tint);
    }
    private static void Quad(VertexHelper mesh, Vector2 center, Vector2 size, Color tint) =>
        AddQuad(mesh, center - size, center + new Vector2(-size.x, size.y), center + size,
            center + new Vector2(size.x, -size.y), tint, tint);
    private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color inner, Color outer)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(a, inner, Vector2.zero); mesh.AddVert(b, inner, Vector2.up);
        mesh.AddVert(c, outer, Vector2.one); mesh.AddVert(d, outer, Vector2.right);
        mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
    }
}
