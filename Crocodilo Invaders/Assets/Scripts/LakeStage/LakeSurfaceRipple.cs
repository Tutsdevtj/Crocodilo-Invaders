using UnityEngine;

public sealed class LakeSurfaceRipple : MonoBehaviour
{
    private LineRenderer[] rings;
    private float age, strength;
    private const float Duration = 1.65f;
    public float Radius { get; private set; }
    public static void Create(Transform parent, Vector2 point, float size, Material material)
    {
        if (material == null) return;
        var root = new GameObject("LakeExpandingRipple"); root.transform.SetParent(parent, false); root.transform.position = point;
        var ripple = root.AddComponent<LakeSurfaceRipple>(); ripple.strength = size;
        ripple.rings = new LineRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            var line = new GameObject("RippleRing_" + i).AddComponent<LineRenderer>(); line.transform.SetParent(root.transform, false);
            line.sharedMaterial = material; line.useWorldSpace = false; line.positionCount = 49;
            line.startWidth = line.endWidth = i == 0 ? .035f : .02f;
            line.sortingOrder = 17; ripple.rings[i] = line;
        }
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age >= Duration) { Destroy(gameObject); return; }
        float expansion = Mathf.Lerp(.35f, 1.5f, Mathf.Clamp01(strength / 2.1f));
        Radius = .12f + age * expansion;
        for (int i = 0; i < rings.Length; i++)
        {
            float local = Mathf.Max(0, age - i * .12f);
            float radius = .12f + local * expansion;
            for (int p = 0; p < 49; p++)
            {
                float angle = p * Mathf.PI * 2 / 48;
                rings[i].SetPosition(p, new Vector3(Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius * .105f - .025f, 0));
            }
            float alpha = Mathf.Clamp01(1 - age / Duration) * (i == 0 ? .72f : .4f);
            rings[i].startColor = rings[i].endColor = new Color(.37f, .77f, .8f, alpha);
        }
    }
}
