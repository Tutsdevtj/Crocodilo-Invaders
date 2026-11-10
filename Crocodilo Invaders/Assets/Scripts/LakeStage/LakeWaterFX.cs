using UnityEngine;

// Small square particles and broken ripples remain separate from damaging projectiles.
public sealed class LakeWaterFX : MonoBehaviour
{
    public const float PixelGravity = 9f;
    private const float Duration = .95f;
    private const int SheetPoints = 17;
    private static readonly AnimationCurve SheetWidth = new AnimationCurve(new Keyframe(0, .2f), new Keyframe(.35f, 1), new Keyframe(.8f, .65f), new Keyframe(1, 0));
    private readonly Vector3[] points = new Vector3[SheetPoints];
    private LineRenderer[] sheets;
    private int activeSheets;
    private float age, width;

    public static void Create(Vector2 point, float size)
    {
        var pool = LakeFxPool.Instance;
        pool.TakeSplash().Begin(pool, point, size);
        LakeSurfaceRipple.Create(point, size);
        LakeAudio.Play(size < .7f ? LakeSound.SplashSmall : LakeSound.SplashBig);
    }
    public void Build(Material material)
    {
        if (material == null)
            return;
        sheets = new LineRenderer[4];
        for (int i = 0; i < sheets.Length; i++)
        {
            var line = new GameObject("CurvedSplashSheet_" + i).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.positionCount = SheetPoints;
            line.sortingOrder = 20;
            line.widthCurve = SheetWidth;
            sheets[i] = line;
        }
    }
    private void Begin(LakeFxPool pool, Vector2 point, float size)
    {
        transform.position = point;
        width = size;
        age = 0;
        bool small = width < .7f;
        activeSheets = sheets == null ? 0 : small ? 2 : 4;
        if (sheets != null)
            for (int i = 0; i < sheets.Length; i++)
            {
                sheets[i].enabled = i < activeSheets;
                sheets[i].widthMultiplier = small ? .045f : .13f;
            }
        EmitPixels(pool, point, small ? 8 : 20);
    }
    private void EmitPixels(LakeFxPool pool, Vector2 point, int count)
    {
        var color = new Color(.48f, .89f, .96f, 1);
        for (int i = 0; i < count; i++)
        {
            float side = i % 2 == 0 ? -1 : 1, t = (i / 2f) / (count * .5f);
            // Opening in the middle keeps the frog and dangerous drops legible.
            float speedX = side * (.8f + t * 2 + width * .15f), speedY = .8f + (1 - t) * 3f;
            float toWater = (speedY + Mathf.Sqrt(speedY * speedY + 2 * PixelGravity * .03f)) / PixelGravity;
            pool.EmitPixel(point + Vector2.right * (side * (.1f + width * .45f * t)), new Vector2(speedX, speedY),
                new Vector2(.055f + (i % 3) * .022f, .06f + (i % 4) * .025f), Mathf.Min(Duration, toWater), color);
        }
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age >= Duration)
        { LakeFxPool.Return(this); return; }
        float impulse = Mathf.Sin(Mathf.Clamp01(age / Duration) * Mathf.PI);
        var tint = new Color(.45f, .8f, .86f, (1 - age / Duration) * .8f);
        for (int s = 0; s < activeSheets; s++)
        {
            float side = s % 2 == 0 ? -1 : 1;
            float reach = (.3f + age * 1.9f) * Mathf.Min(1.1f, width);
            float height = impulse * (.35f + width * .18f) * (s < 2 ? 1 : .65f);
            for (int p = 0; p < SheetPoints; p++)
            {
                float u = p / (SheetPoints - 1f);
                points[p] = new Vector3(side * (width * .28f + u * reach), Mathf.Sin(u * Mathf.PI) * height, 0);
            }
            sheets[s].SetPositions(points);
            sheets[s].startColor = sheets[s].endColor = tint;
        }
    }
}
