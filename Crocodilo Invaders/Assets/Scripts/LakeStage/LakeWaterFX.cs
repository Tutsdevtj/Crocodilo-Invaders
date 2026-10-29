using UnityEngine;

// Small square particles and broken ripples remain separate from damaging projectiles.
public sealed class LakeWaterFX : MonoBehaviour
{
    private SpriteRenderer[] particles;
    private Vector3[] origins, velocities;
    private float age, duration;
    private bool warning;
    private float width;
    private static Sprite pixel;
    private LineRenderer[] sheets;
    public static LakeWaterFX Create(Transform parent, Vector2 point, float size, bool cue = false)
    {
        var node = new GameObject(cue ? "LakeLandingWarning" : "LakeSplash");
        node.transform.SetParent(parent, false); node.transform.position = point;
        var fx = node.AddComponent<LakeWaterFX>(); fx.width = size; fx.warning = cue;
        fx.duration = cue ? float.PositiveInfinity : .95f;
        fx.Build();
        if (!cue)
        {
            var lake = FindAnyObjectByType<LakeBackdrop>();
            if (lake != null)
            {
                LakeSurfaceRipple.Create(parent, point, size, lake.WaterFXMaterial);
                fx.BuildSheets(lake.WaterFXMaterial);
            }
        }
        return fx;
    }
    private void BuildSheets(Material material)
    {
        if (material == null) return;
        sheets = new LineRenderer[width < .7f ? 2 : 4];
        for (int i = 0; i < sheets.Length; i++)
        {
            var line = new GameObject("CurvedSplashSheet_" + i).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false); line.useWorldSpace = false;
            line.sharedMaterial = material; line.positionCount = 17; line.sortingOrder = 20;
            line.widthCurve = new AnimationCurve(new Keyframe(0, .2f), new Keyframe(.35f, 1), new Keyframe(.8f, .65f), new Keyframe(1, 0));
            line.widthMultiplier = width < .7f ? .045f : .13f; sheets[i] = line;
        }
    }
    private void Build()
    {
        if (pixel == null) pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        int count = warning ? 16 : width < .7f ? 8 : 20;
        particles = new SpriteRenderer[count]; origins = new Vector3[count]; velocities = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            var view = new GameObject("WaterPixel").AddComponent<SpriteRenderer>(); view.transform.SetParent(transform, false);
            view.sprite = pixel; view.sortingOrder = warning ? 27 : 24; particles[i] = view;
            float side = i % 2 == 0 ? -1 : 1, t = (i / 2f) / (count * .5f);
            origins[i] = new Vector3(side * (.1f + width * .45f * t), 0, 0);
            velocities[i] = new Vector3(side * (.8f + t * 2), .8f + (1 - t) * 3f, 0);
        }
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age >= duration) { Destroy(gameObject); return; }
        if (sheets != null)
        {
            float impulse = Mathf.Sin(Mathf.Clamp01(age / duration) * Mathf.PI);
            for (int s = 0; s < sheets.Length; s++)
            {
                float side = s % 2 == 0 ? -1 : 1;
                float reach = (.3f + age * 1.9f) * Mathf.Min(1.1f, width);
                for (int p = 0; p < 17; p++)
                {
                    float u = p / 16f;
                    sheets[s].SetPosition(p, new Vector3(side * (width * .28f + u * reach),
                        Mathf.Sin(u * Mathf.PI) * impulse * (.35f + width * .18f) * (s < 2 ? 1 : .65f), 0));
                }
                sheets[s].startColor = sheets[s].endColor = new Color(.45f, .8f, .86f, (1 - age / duration) * .8f);
            }
        }
        for (int i = 0; i < particles.Length; i++)
        {
            var view = particles[i]; float side = i % 2 == 0 ? -1 : 1;
            if (warning)
            {
                float angle = i * Mathf.PI * 2 / particles.Length;
                view.transform.localPosition = new Vector3(Mathf.Cos(angle) * width * .5f, Mathf.Sin(angle) * .13f, 0);
                view.transform.localScale = new Vector3(.12f, .045f, 1);
                view.color = new Color(.2f, .85f, 1, .55f + .4f * Mathf.Sin(age * 12));
            }
            else
            {
                Vector3 p = origins[i] + velocities[i] * age + Vector3.down * (4.5f * age * age);
                p.y = Mathf.Max(-.03f, p.y);
                // Opening in the middle keeps the frog and dangerous drops legible.
                p.x += side * age * width * .15f;
                view.transform.localPosition = p;
                view.transform.localScale = new Vector3(.055f + (i % 3) * .022f, .06f + (i % 4) * .025f, 1);
                view.color = new Color(.48f, .89f, .96f, Mathf.Clamp01(1 - age / duration));
            }
        }
    }
}
