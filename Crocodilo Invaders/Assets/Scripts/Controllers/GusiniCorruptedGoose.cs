using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class GusiniCorruptedGoose : MonoBehaviour
{
    private GusiniPhaseTwoAssets assets;
    private SpriteRenderer visual, beak, streak;
    private Rigidbody2D body;
    private Health health, target;
    private float age, scale, baseY, bobPhase, flashUntil, diveAt = -1f;
    private float diveX, left;
    private bool diver, diving, spent;
    private string variant;
    private Vector2 diveDirection;
    private GusiniAudio audio;
    private bool honkLeader, honked;
    private int previousBeakState;
    public static GusiniCorruptedGoose Create(GusiniPhaseTwoAssets assets, Transform parent, Vector3 position,
        float scale, int slot, Health target, bool diver, Rect arena, GusiniAudio audio = null)
    {
        var root = new GameObject("Corrupted goose " + slot); root.layer = 9; root.transform.SetParent(parent, false);
        root.transform.position = position; root.transform.localScale = Vector3.one * scale * 100f;
        var health = root.AddComponent<Health>(); health.SetMaximumHealth(2, false);
        var visual = root.AddComponent<SpriteRenderer>(); visual.sortingOrder = 25;
        var collider = root.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(.26f, .17f);
        var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.useFullKinematicContacts = true;
        var beakRoot = new GameObject("Animated beak"); beakRoot.transform.SetParent(root.transform, false);
        var beak = beakRoot.AddComponent<SpriteRenderer>(); beak.sortingOrder = 26;
        var streakRoot = new GameObject("Dive streak"); streakRoot.transform.SetParent(root.transform, false);
        streakRoot.transform.localPosition = new Vector3(.25f, 0, 0);
        var streak = streakRoot.AddComponent<SpriteRenderer>(); streak.sortingOrder = 24; streak.enabled = false;
        var goose = root.AddComponent<GusiniCorruptedGoose>();
        goose.assets = assets; goose.health = health; goose.visual = visual; goose.body = body; goose.beak = beak; goose.streak = streak;
        goose.scale = scale; goose.baseY = position.y; goose.target = target; goose.diver = diver;
        goose.audio = audio; goose.honkLeader = slot == 0;
        goose.diveX = arena.xMin + arena.width * .7f; goose.left = arena.xMin - 1f;
        goose.bobPhase = slot * 1.1f; goose.variant = slot % 2 == 0 ? "gansinho_ca" : "gansinho_cb";
        visual.sprite = assets.Get(goose.variant + "_0");
        health.Damaged += goose.OnDamaged; health.Died += goose.OnDied;
        return goose;
    }
    private void OnDamaged() => flashUntil = age + .06f;
    private void OnDied()
    {
        if (spent) return;
        spent = true;
        if (health.IsDead && audio != null) audio.Play(GusiniSound.P2GoosePoof, transform.position, transform.parent);
        GusiniPhaseTwoEffect.Create(assets.Frames("fx_gansinho_poof_", 5), new[] { .06f, .06f, .07f, .08f, .09f },
            transform.parent, transform.position, scale);
        gameObject.SetActive(false); Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        if (spent) return;
        Vector2 point = body.position;
        if (diving) point += diveDirection * (280f * scale * Time.fixedDeltaTime);
        else if (diveAt < 0f)
        {
            point.x -= 130f * scale * Time.fixedDeltaTime;
            point.y = baseY + Mathf.Sin(age * (2f * Mathf.PI / .8f) + bobPhase) * 8f * scale;
            if (diver && point.x <= diveX)
            {
                diveAt = age;
                if (audio != null) audio.Play(GusiniSound.P2DiverWarning, transform.position, transform);
                // Trava a mira no início do aviso de 400ms; nunca persegue depois.
                diveDirection = target != null ? ((Vector2)target.transform.position - point).normalized : Vector2.left;
            }
        }
        body.MovePosition(point);
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (body.position.x < left || age > 14f) { Destroy(gameObject); return; }
        if (!diving && diveAt >= 0f && age - diveAt >= .4f)
        {
            diving = true;
            if (audio != null) audio.Play(GusiniSound.P2DiveWhoosh, transform.position, transform);
        }
        float flight = (age + bobPhase * .02f) % .3f;
        int frame = flight < .08f ? 0 : flight < .15f ? 1 : flight < .23f ? 2 : 3;
        if (diving) frame = 2;
        string prefix = age < flashUntil ? variant + "_hit_white_" :
            diveAt >= 0f && !diving && (int)((age - diveAt) / .08f) % 2 == 0 ? variant + "_flash_green_" : variant + "_";
        visual.sprite = assets.Get(prefix + frame);
        float sing = (age + bobPhase * .08f) % 1.2f;
        int beakState = sing < .06f || sing >= .31f ? 0 : sing < .12f || sing >= .24f ? 1 : 2;
        beak.sprite = assets.Get("gansinho_c_beak_" + beakState);
        if (beakState == 2 && previousBeakState != 2 && honkLeader && !honked)
        {
            honked = true;
            if (audio != null) audio.Play(GusiniSound.P2HonkMid, transform.position, transform);
        }
        previousBeakState = beakState;
        streak.enabled = diving;
        if (diving) streak.sprite = assets.Get("fx_dive_streak_" + ((int)(age / .06f) % 3));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || other.gameObject.layer != 8) return;
        var player = other.GetComponentInParent<Health>();
        if (player == null || (!player.TakeDamage(2) && !player.IsInvulnerable)) return;
        OnDied();
    }
    private void OnDestroy()
    {
        if (health != null) { health.Damaged -= OnDamaged; health.Died -= OnDied; }
    }
}
