using UnityEngine;

public sealed class GusiniEnergyBall : MonoBehaviour
{
    private GusiniPhaseTwoAssets assets;
    private SpriteRenderer visual;
    private CircleCollider2D hitbox;
    private Rigidbody2D body;
    private float age, popAge, verticalSpeed, top, bottom, left, scale;
    private bool popping;
    private GusiniAudio audio;
    public static GusiniEnergyBall Create(GusiniPhaseTwoAssets assets, Transform parent, Vector3 position, float scale, int index, GusiniAudio audio = null)
    {
        var root = new GameObject("Corrupted energy ball"); root.transform.SetParent(parent, false);
        root.transform.position = position; root.transform.localScale = Vector3.one * scale * 100f;
        var visual = root.AddComponent<SpriteRenderer>(); visual.sortingOrder = 25; visual.sprite = assets.Get("gusini_p2_energy_ball_spawn_0");
        var collider = root.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = .23f;
        var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.useFullKinematicContacts = true;
        var ball = root.AddComponent<GusiniEnergyBall>();
        ball.assets = assets; ball.visual = visual; ball.hitbox = collider; ball.body = body; ball.scale = scale;
        ball.audio = audio;
        Camera camera = Camera.main;
        float half = camera != null ? camera.orthographicSize : 4.8f;
        float center = camera != null ? camera.transform.position.y : 1.8f;
        ball.bottom = center - half + .5f; ball.top = center + half - .5f;
        ball.left = camera != null ? camera.transform.position.x - half * camera.aspect - .8f : -9f;
        ball.verticalSpeed = (index % 2 == 0 ? 1 : -1) * (5.6f + index * .65f);
        return ball;
    }
    private void FixedUpdate()
    {
        if (popping || age < .18f) return;
        Vector2 point = body.position + new Vector2(-185f * scale, verticalSpeed) * Time.fixedDeltaTime;
        // Reflexão independente, começando exatamente no release (sem salto vertical).
        if (point.y > top) { point.y = 2f * top - point.y; verticalSpeed = -Mathf.Abs(verticalSpeed); }
        if (point.y < bottom) { point.y = 2f * bottom - point.y; verticalSpeed = Mathf.Abs(verticalSpeed); }
        body.MovePosition(point);
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (popping)
        {
            popAge += Time.deltaTime;
            int frame = (int)(popAge / .05f);
            if (frame >= 5) { Destroy(gameObject); return; }
            visual.sprite = assets.Get("gusini_p2_energy_ball_pop_" + frame); return;
        }
        visual.sprite = age < .18f ? assets.Get("gusini_p2_energy_ball_spawn_" + Mathf.Min(2, (int)(age / .06f))) :
            assets.Get("gusini_p2_energy_ball_" + ((int)((age - .18f) / .07f) % 6));
        if (body.position.x < left || age > 8f) Pop();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (popping || other.gameObject.layer != 8) return;
        var health = other.GetComponentInParent<Health>();
        if (health != null && (health.TakeDamage(3) || health.IsInvulnerable)) Pop();
    }
    private void Pop()
    {
        if (popping) return;
        popping = true; hitbox.enabled = false; body.linearVelocity = Vector2.zero;
        if (audio != null) audio.Play(GusiniSound.P2EnergyPop, transform.position, transform.parent);
    }
}
