using UnityEngine;

// Health is readable as length: the water keeps its depth while its right edge recedes.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LakeHudWater : UnityEngine.UI.MaskableGraphic
{
    private float clock, level, hit;
    private static readonly int LevelId = Shader.PropertyToID("_Level"), HitId = Shader.PropertyToID("_Hit"), SizeId = Shader.PropertyToID("_Size");
    private Material instance;
    public float Level => level;
    public float FilledWidth => rectTransform.rect.width * level;
    public float SurfaceSample => Surface(61);
    public override Texture mainTexture => Texture2D.whiteTexture;
    protected override void Awake() { base.Awake(); raycastTarget = false; maskable = false; }
    protected override void OnDestroy() { base.OnDestroy(); if (instance != null) Destroy(instance); }
    public void UseMaterial(Material source)
    {
        instance = new Material(source);
        instance.SetVector(SizeId, rectTransform.rect.size);
        material = instance;
    }
    public void Present(float time, float fraction, float impact)
    {
        clock = time;
        level = Mathf.Clamp01(fraction);
        hit = impact;
        if (instance == null)
            return;
        instance.SetFloat(LakeShaderIds.Clock, clock);
        instance.SetFloat(LevelId, level);
        instance.SetFloat(HitId, hit);
    }
    private float Surface(float x)
    {
        float ripple = (Mathf.Sin(x * .10f + clock * 4.8f) * .48f + Mathf.Sin(x * .23f - clock * 7.2f) * .25f) * (1 + hit * 1.6f);
        return Mathf.Clamp(16.5f + ripple, .5f, 17.7f);
    }
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = rectTransform.rect;
        Color32 white = Color.white;
        mesh.AddVert(new Vector3(r.xMin, r.yMin), white, new Vector2(0, 0));
        mesh.AddVert(new Vector3(r.xMin, r.yMax), white, new Vector2(0, 1));
        mesh.AddVert(new Vector3(r.xMax, r.yMax), white, new Vector2(1, 1));
        mesh.AddVert(new Vector3(r.xMax, r.yMin), white, new Vector2(1, 0));
        mesh.AddTriangle(0, 1, 2);
        mesh.AddTriangle(0, 2, 3);
    }
    public static void Quad(UnityEngine.UI.VertexHelper mesh, float x, float y, float w, float h, Color32 c)
    {
        int n = mesh.currentVertCount;
        mesh.AddVert(new Vector3(x, y), c, Vector2.zero);
        mesh.AddVert(new Vector3(x, y + h), c, Vector2.zero);
        mesh.AddVert(new Vector3(x + w, y + h), c, Vector2.zero);
        mesh.AddVert(new Vector3(x + w, y), c, Vector2.zero);
        mesh.AddTriangle(n, n + 1, n + 2);
        mesh.AddTriangle(n, n + 2, n + 3);
    }
}
