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
    public static LakeWaterFX Create(Transform parent, Vector2 point, float size, bool cue = false)
    {
        var node = new GameObject(cue ? "LakeLandingWarning" : "LakeSplash");
        node.transform.SetParent(parent, false); node.transform.position = point;
        var fx = node.AddComponent<LakeWaterFX>(); fx.width = size; fx.warning = cue;
        fx.duration = cue ? float.PositiveInfinity : .72f;
        fx.Build(); return fx;
    }
    private void Build()
    {
        if (pixel == null) pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        int count = warning ? 16 : 28;
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
                // Opening in the middle keeps the frog and dangerous drops legible.
                p.x += side * age * width * .15f;
                view.transform.localPosition = p;
                view.transform.localScale = new Vector3(.055f + (i % 3) * .022f, .06f + (i % 4) * .025f, 1);
                view.color = new Color(.48f, .89f, .96f, Mathf.Clamp01(1 - age / duration));
            }
        }
    }
}
