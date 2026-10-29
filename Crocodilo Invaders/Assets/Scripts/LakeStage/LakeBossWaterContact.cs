using UnityEngine;

// One surface plane clips the submerged feet, reflection and the tire's wake together.
[DefaultExecutionOrder(100)]
public sealed class LakeBossWaterContact : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Material contactMaterial;
    private SapoDoPneuCombat combat;
    private LakeBackdrop lake;
    private SpriteRenderer reflection;
    private LineRenderer frontWake, backWake;
    private Material originalMaterial;
    private MaterialPropertyBlock mainProperties, reflectionProperties;
    private float contact;
    private Vector2 wakePoint;
    public float ContactWeight => contact;
    public SpriteRenderer Reflection => reflection;
    public float SurfaceY { get; private set; }
    private void Start()
    {
        combat = GetComponent<SapoDoPneuCombat>(); lake = FindAnyObjectByType<LakeBackdrop>();
        if (visual == null || lake == null || contactMaterial == null) { enabled = false; return; }
        originalMaterial = visual.sharedMaterial; visual.sharedMaterial = contactMaterial;
        mainProperties = new MaterialPropertyBlock(); reflectionProperties = new MaterialPropertyBlock();
        reflection = new GameObject("SapoWaterReflection").AddComponent<SpriteRenderer>(); reflection.transform.SetParent(transform, false);
        reflection.sprite = visual.sprite; reflection.sharedMaterial = contactMaterial; reflection.sortingOrder = 10;
        frontWake = Wake("TireFrontMeniscus", 18); backWake = Wake("TireBackWake", 12);
    }
    private LineRenderer Wake(string id, int order)
    {
        var line = new GameObject(id).AddComponent<LineRenderer>(); line.transform.SetParent(transform, false);
        line.useWorldSpace = true; line.positionCount = 33; line.sharedMaterial = lake.WaterFXMaterial;
        line.startWidth = line.endWidth = order == 18 ? .045f : .025f; line.sortingOrder = order;
        return line;
    }
    private void LateUpdate()
    {
        if (reflection == null || Time.timeScale <= 0) return;
        float clock = lake.MotionClock;
        SurfaceY = combat.WaterY;
        float altitude = Mathf.Max(0, transform.position.y - (SurfaceY + SapoDoPneuCombat.FloatHeight));
        float desired = combat.Active ? Mathf.Clamp01(1 - altitude / .65f) : 0;
        contact = Mathf.Lerp(contact, desired, 1 - Mathf.Exp(-Time.deltaTime * 14));
        float roll = contact * (Mathf.Sin(clock * 1.7f) * 1.5f + Mathf.Sin(clock * 3.1f) * .4f);
        visual.transform.localRotation = Quaternion.Euler(0, 0, roll);
        visual.GetPropertyBlock(mainProperties); mainProperties.SetFloat("_WaterY", SurfaceY);
        mainProperties.SetFloat("_Clock", clock); mainProperties.SetFloat("_Reflection", 0); visual.SetPropertyBlock(mainProperties);
        reflection.transform.position = new Vector3(visual.transform.position.x, SurfaceY * 1.42f - visual.transform.position.y * .42f, 0);
        reflection.transform.localScale = Vector3.Scale(visual.transform.localScale, new Vector3(1, -.42f, 1));
        reflection.transform.rotation = Quaternion.Euler(0, 0, -roll);
        reflection.color = new Color(1, 1, 1, contact);
        reflectionProperties.SetFloat("_WaterY", SurfaceY); reflectionProperties.SetFloat("_Clock", clock);
        reflectionProperties.SetFloat("_Reflection", 1); reflection.SetPropertyBlock(reflectionProperties);
        if (desired > .7f) wakePoint = new Vector2(transform.position.x, SurfaceY);
        for (int p = 0; p < 33; p++)
        {
            float angle = p / 32f * Mathf.PI;
            float x = Mathf.Cos(angle) * (1.38f + Mathf.Sin(clock * 2) * .04f);
            float y = Mathf.Sin(angle) * .095f;
            frontWake.SetPosition(p, new Vector3(wakePoint.x + x, wakePoint.y - y - .025f, 0));
            backWake.SetPosition(p, new Vector3(wakePoint.x + x, wakePoint.y + y, 0));
        }
        frontWake.startColor = frontWake.endColor = new Color(.45f, .81f, .8f, contact * .8f);
        backWake.startColor = backWake.endColor = new Color(.22f, .6f, .61f, contact * .5f);
    }
    private void OnDisable()
    {
        if (visual != null && originalMaterial != null) visual.sharedMaterial = originalMaterial;
        if (reflection != null) reflection.enabled = false;
        if (frontWake != null) frontWake.enabled = false;
        if (backWake != null) backWake.enabled = false;
    }
}
