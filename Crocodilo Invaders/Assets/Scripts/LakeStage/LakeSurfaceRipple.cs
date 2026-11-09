using UnityEngine;

public sealed class LakeSurfaceRipple : MonoBehaviour
{
    private const float Duration = 1.65f;
    private const int RingPoints = 49;
    private readonly Vector3[] points = new Vector3[RingPoints];
    private LineRenderer[] rings;
    private float age, strength;
    public float Radius { get; private set; }

    public static void Create(Vector2 point, float size)
    {
        var ripple = LakeFxPool.Instance.TakeRipple();
        ripple.transform.position = point;
        ripple.strength = size;
        ripple.age = 0;
        ripple.Radius = 0;
    }
    public void Build(Material material)
    {
        if (material == null)
            return;
        rings = new LineRenderer[3];
        for (int i = 0; i < rings.Length; i++)
        {
            var line = new GameObject("RippleRing_" + i).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.positionCount = RingPoints;
            line.startWidth = line.endWidth = i == 0 ? .035f : .02f;
            line.sortingOrder = 17;
            rings[i] = line;
        }
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age >= Duration || rings == null)
        { LakeFxPool.Return(this); return; }
        float expansion = Mathf.Lerp(.35f, 1.5f, Mathf.Clamp01(strength / 2.1f));
        Radius = .12f + age * expansion;
        for (int i = 0; i < rings.Length; i++)
        {
            float local = Mathf.Max(0, age - i * .12f);
            float radius = .12f + local * expansion;
            for (int p = 0; p < RingPoints; p++)
            {
                float angle = p * Mathf.PI * 2 / (RingPoints - 1);
                points[p] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * .105f - .025f, 0);
            }
            rings[i].SetPositions(points);
            float alpha = Mathf.Clamp01(1 - age / Duration) * (i == 0 ? .72f : .4f);
            rings[i].startColor = rings[i].endColor = new Color(.37f, .77f, .8f, alpha);
        }
    }
}
