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
            BorderTrace(mesh);
            if (tier >= 2) Crown(mesh);
            if (tier >= 3) Rays(mesh);
            if (tier == 4) Lightning(mesh);
        }
        Impact(mesh);
    }
    private void Sparks(VertexHelper mesh)
    {
        int count = tier == 1 ? 4 : tier == 2 ? 12 : tier == 3 ? 20 : 28;
        for (int i = 0; i < count; i++)
        {
            float seed = Fraction(i * .6180339f + .23f);
            float life = Fraction(elapsed * (.15f + tier * .025f + seed * .12f) + i * .371f);
            float side = i % 2 == 0 ? -1f : 1f;
            Vector2 position = new Vector2(side * (halfSize.x + 7f + seed * (9f + tier * 4f)),
                Mathf.Lerp(-halfSize.y - 5f, halfSize.y + 10f, life));
            position.x += Mathf.Sin(elapsed * 2f + i * 2.1f) * (2f + tier);
            Color tint = Tint(Mathf.Sin(life * Mathf.PI) * (.14f + tier * .12f), i % 3 == 0 ? .8f : .15f);
            float size = i % 5 == 0 ? 2.1f : 1.1f;
            if (tier >= 3 && i % 4 == 0) Star(mesh, position, size * 2f, tint);
            else Quad(mesh, position, new Vector2(size, size + tier * .4f), tint);
        }
        if (tier == 1) Star(mesh, Edge(elapsed * .55f, 2f), 2.4f + focus, Tint(.45f, .7f));
    }
    private void BorderTrace(VertexHelper mesh)
    {
        int count = tier;
        for (int i = 0; i < count; i++)
            for (int tail = 0; tail < 3; tail++)
            {
                float angle = elapsed * (.34f + tier * .055f) + i * Mathf.PI * 2f / count - tail * .05f;
                Vector2 a = Edge(angle, 2f), b = Edge(angle + .035f, 2f);
                float alpha = (1f - tail * .27f) * (.18f + tier * .11f + focus * .08f);
                Segment(mesh, a, b, 2.1f, Tint(alpha * .15f, .2f));
                Segment(mesh, a, b, .7f, Tint(alpha, .75f));
            }
    }
    private void Crown(VertexHelper mesh)
    {
        int count = tier == 4 ? 16 : tier == 3 ? 12 : 8;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float wave = .5f + .5f * Mathf.Sin(elapsed * (2f + tier) - i * .8f);
            Vector2 start = Edge(angle, 5f);
            Vector2 end = Edge(angle, 8f + wave * (tier * 3f + focus * 4f));
            Beam(mesh, start, end, 1f + tier * .65f, Tint(.055f + wave * .13f + focus * .04f, .12f));
            if (tier >= 3 && i % 2 == 0) Star(mesh, end, 2f + wave * 2f, Tint(wave * .65f, .7f));
        }
    }
    private void Rays(VertexHelper mesh)
    {
        int count = tier == 4 ? 12 : 8;
        float impact = Mathf.Exp(-burstAge * 3.4f);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count + .08f * Mathf.Sin(elapsed * .6f);
            float wave = .5f + .5f * Mathf.Sin(elapsed * 1.7f + i * 2.4f);
            Color tint = Tint(.025f + wave * .045f + impact * .25f + focus * .035f, .18f);
            Beam(mesh, Edge(angle, 3f), Edge(angle, 16f + wave * 8f + impact * 12f),
                (tier == 4 ? 6f : 4f) + impact * 5f, tint);
        }
    }
    private void Lightning(VertexHelper mesh)
    {
        if (burstAge > .42f && Fraction(elapsed / 4.8f) > .06f) return;
        int phase = Mathf.FloorToInt(elapsed * 14f);
        for (int side = -1; side <= 1; side += 2)
        {
            float center = Mathf.Sin(phase * .27f + side) * halfSize.y * .6f;
            Vector2 previous = new Vector2(side * (halfSize.x + 2f), center - 32f);
            for (int j = 1; j <= 5; j++)
            {
                Vector2 next = new Vector2(side * (halfSize.x + 2f + Fraction(j * .733f + phase * .37f) * 11f),
                    center + Mathf.Lerp(-32f, 32f, j / 5f));
                Segment(mesh, previous, next, 2.8f, Tint(.15f, .1f));
                Segment(mesh, previous, next, .8f, Tint(.7f, .86f));
                previous = next;
            }
        }
    }
    private void Impact(VertexHelper mesh)
    {
        float duration = .18f + tier * .14f;
        if (burstAge >= duration) return;
        float t = Mathf.Clamp01(burstAge / duration);
        for (int i = 0; i < 4 + tier * 6; i++)
        {
            float angle = i * 2.39996f;
            Vector2 start = Edge(angle, 2f + t * (6f + tier * 4f));
            Vector2 end = Edge(angle, 5f + t * (9f + tier * 5f));
            Segment(mesh, start, end, (1f - t) * (.7f + tier * .4f), Tint((1f - t) * (.18f + tier * .16f), .65f));
            if (tier >= 2 && i % 4 == 0) Star(mesh, end, (1f - t) * 6f, Tint(1f - t, .9f));
        }
    }
    private Vector2 Edge(float angle, float margin)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        // Expansão lateral livre; limite vertical protege título e comparações.
        Vector2 size = halfSize + new Vector2(margin, Mathf.Min(margin, 6f));
        return direction / Mathf.Max(Mathf.Abs(direction.x) / size.x, Mathf.Abs(direction.y) / size.y);
    }
    private Color Tint(float alpha, float white)
    {
        Color tint = Color.Lerp(color, Color.white, white);
        tint.a = Mathf.Clamp01(alpha * opacity);
        return tint;
    }
    private static float Fraction(float value) => value - Mathf.Floor(value);
    private void Star(VertexHelper mesh, Vector2 center, float size, Color tint)
    {
        Quad(mesh, center, new Vector2(size * .22f, size), tint);
        Quad(mesh, center, new Vector2(size, size * .22f), tint);
    }
    private void Beam(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized * width;
        Color transparent = tint; transparent.a = 0f;
        AddQuad(mesh, start - normal, start + normal, end + normal * .12f, end - normal * .12f, tint, transparent);
    }
    private void Segment(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized * width;
        AddQuad(mesh, start - normal, start + normal, end + normal, end - normal, tint, tint);
    }
    private void Quad(VertexHelper mesh, Vector2 center, Vector2 size, Color tint) =>
        AddQuad(mesh, center - size, center + new Vector2(-size.x, size.y), center + size,
            center + new Vector2(size.x, -size.y), tint, tint);
    private Color ProtectedColor(Color tint, Vector2 vertex)
    {
        tint.a *= 1f - Mathf.Clamp01((Mathf.Abs(vertex.y) - halfSize.y - 1f) / 8f);
        return tint;
    }
    private void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color inner, Color outer)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(a, ProtectedColor(inner, a), Vector2.zero); mesh.AddVert(b, ProtectedColor(inner, b), Vector2.up);
        mesh.AddVert(c, ProtectedColor(outer, c), Vector2.one); mesh.AddVert(d, ProtectedColor(outer, d), Vector2.right);
        mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
    }
}
