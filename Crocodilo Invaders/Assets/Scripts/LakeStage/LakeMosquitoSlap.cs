using UnityEngine;

// A brief pixel slap, not a second enemy: no health, hitbox or score.
public sealed class LakeMosquitoSlap : MonoBehaviour
{
    private static Sprite pixel;
    private LakeJourneyDirector owner;
    private SpriteRenderer corpse;
    private SpriteRenderer[] blood, flash;
    private Vector3[] origins, velocities;
    private Vector3 sourceScale;
    private float age;
    public float Clock => age;
    public int BloodCount => blood != null ? blood.Length : 0;
    public SpriteRenderer CrushedSprite => corpse;
    public static LakeMosquitoSlap Create(LakeJourneyDirector director, SpriteRenderer source, bool large)
    {
        if (director == null || source == null)
            return null;
        if (pixel == null)
            pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        var node = new GameObject("MosquitoSlap");
        node.transform.SetParent(director.transform, false);
        node.transform.position = source.transform.position;
        var fx = node.AddComponent<LakeMosquitoSlap>();
        fx.owner = director;
        fx.sourceScale = source.transform.lossyScale;
        fx.corpse = new GameObject("SquashedMosquito").AddComponent<SpriteRenderer>();
        fx.corpse.transform.SetParent(node.transform, false);
        fx.corpse.sprite = source.sprite;
        fx.corpse.flipX = source.flipX;
        fx.corpse.sortingOrder = 35;
        fx.blood = new SpriteRenderer[large ? 26 : 20];
        fx.origins = new Vector3[fx.blood.Length];
        fx.velocities = new Vector3[fx.blood.Length];
        for (int i = 0; i < fx.blood.Length; i++)
        {
            var view = new GameObject("BloodPixel").AddComponent<SpriteRenderer>();
            view.transform.SetParent(node.transform, false);
            view.sprite = pixel;
            view.sortingOrder = 36;
            float angle = i * 2.399963f;
            float speed = (large ? 3.6f : 2.8f) * (.45f + (i % 5) * .13f);
            fx.origins[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * .55f, 0) * .05f;
            fx.velocities[i] = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed * .75f + .8f, 0);
            float size = (large ? .075f : .055f) * (1 + (i % 3) * .25f);
            view.transform.localScale = new Vector3(size * (i % 4 == 0 ? 2 : 1), size, 1);
            view.transform.localRotation = Quaternion.Euler(0, 0, i % 4 * 30);
            fx.blood[i] = view;
        }
        fx.flash = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            var view = new GameObject("SlapImpact").AddComponent<SpriteRenderer>();
            view.transform.SetParent(node.transform, false);
            view.sprite = pixel;
            view.sortingOrder = 37;
            float angle = 45 + i * 90;
            view.transform.localRotation = Quaternion.Euler(0, 0, angle);
            view.transform.localPosition = Quaternion.Euler(0, 0, angle) * Vector3.right * .42f;
            view.transform.localScale = new Vector3(large ? .5f : .35f, .045f, 1);
            fx.flash[i] = view;
        }
        fx.Present();
        return fx;
    }
    private void Update()
    {
        if (owner == null || owner.Target == null || owner.Target.IsDead)
        { Destroy(gameObject); return; }
        if (Time.timeScale <= 0 && owner.CombatActive)
            return;
        age += owner.CombatActive ? Time.deltaTime : Time.unscaledDeltaTime;
        Present();
        if (age >= 1.1f)
            Destroy(gameObject);
    }
    private void Present()
    {
        float hit = 1;
        corpse.transform.localScale = Vector3.Scale(sourceScale, new Vector3(Mathf.Lerp(1, 1.4f, hit), Mathf.Lerp(1, .16f, hit), 1));
        corpse.transform.localRotation = Quaternion.Euler(0, 0, -18 + age * 45);
        corpse.transform.localPosition = Vector3.down * (age * age * 1.6f);
        float alpha = Mathf.Clamp01((1.1f - age) / .35f);
        corpse.color = new Color(.5f, .16f, .1f, alpha);
        for (int i = 0; i < blood.Length; i++)
        {
            blood[i].transform.localPosition = origins[i] + velocities[i] * age + Vector3.down * (3.2f * age * age);
            blood[i].color = i % 3 == 0 ? new Color(.9f, .08f, .06f, alpha) : new Color(.5f, .015f, .025f, alpha);
        }
        foreach (var view in flash)
            view.color = new Color(1, .96f, .8f, Mathf.Clamp01(1 - age / .09f));
    }
}
