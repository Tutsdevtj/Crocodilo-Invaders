using UnityEngine;

[RequireComponent(typeof(Boss), typeof(Rigidbody2D))]
public sealed class SapoDoPneuCombat : MonoBehaviour, IBossCombatController
{
    public enum State { Entering, Float, Prepare, Rise, Fall, Impact, Recover, Stopped }
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite dropletSprite;
    [SerializeField] private TMPro.TMP_FontAsset hudFont;
    public TMPro.TMP_FontAsset HudFont => hudFont;
    [SerializeField, Min(.6f)] private float jumpDuration = 1.45f;
    [SerializeField, Min(.5f)] private float warningDuration = .9f;
    [SerializeField, Min(1)] private float restDuration = 2.4f;
    private Boss boss;
    private Rigidbody2D body;
    private Health target;
    private LakeBackdrop backdrop;
    private Camera arenaCamera;
    private Transform effects;
    private LakeWaterFX cue;
    private State state = State.Entering;
    private Vector2 from, landing;
    private Vector3 visualScale;
    private float timer, jumpHeight, hitFlash, contactCooldown;
    private bool stopped, tookMovement;
    private int sequence;
    private ContactFilter2D filter;
    private readonly Collider2D[] contacts = new Collider2D[8];
    public State CurrentState => state;
    public Vector2 LockedLanding => landing;
    public int Landings { get; private set; }
    public int DropsLaunched { get; private set; }
    public int DropsReturned { get; private set; }
    public bool Active => !stopped && boss != null && boss.BattleActive && !boss.Health.IsDead && target != null && !target.IsDead;
    public bool Enraged => boss != null && boss.Health.Fraction <= .4f;
    public Rect Arena
    {
        get
        {
            var camera = arenaCamera != null ? arenaCamera : Camera.main;
            if (camera == null) return new Rect(-9, -3, 18, 10);
            Vector2 lo = camera.ViewportToWorldPoint(Vector3.zero), hi = camera.ViewportToWorldPoint(Vector3.one);
            return Rect.MinMaxRect(lo.x, lo.y, hi.x, hi.y);
        }
    }
    public float WaterY => backdrop != null ? backdrop.WaterY : Arena.yMin + Arena.height * .28f;
    private float RestY => WaterY + .78f;
    private void Awake()
    {
        boss = GetComponent<Boss>(); body = GetComponent<Rigidbody2D>();
        backdrop = FindAnyObjectByType<LakeBackdrop>(); arenaCamera = Camera.main;
        visualScale = visual != null ? visual.transform.localScale : Vector3.one;
        filter = LakeHit.PlayerFilter();
    }
    private void OnEnable() { GetComponent<Health>().Damaged += Flash; }
    private void OnDisable() { GetComponent<Health>().Damaged -= Flash; StopCombat(); }
    private void Flash() { hitFlash = .12f; }
    public void SetTarget(Health player)
    {
        target = player; stopped = false;
        if (effects == null) { effects = new GameObject("SapoWaterThreats").transform; effects.SetParent(transform.parent, false); }
    }
    private void FixedUpdate()
    {
        if (!Active) { if (!stopped && state != State.Entering) StopCombat(); return; }
        if (!boss.Ready) return;
        if (!tookMovement)
        {
            boss.Movement.enabled = false; body.linearVelocity = Vector2.zero;
            tookMovement = true; body.position = new Vector2(Mathf.Clamp(body.position.x, Arena.center.x + 1, Arena.xMax - 1.7f), RestY);
            SetState(State.Float);
        }
        float dt = Time.fixedDeltaTime; timer += dt; contactCooldown -= dt;
        switch (state)
        {
            case State.Float:
                body.MovePosition(new Vector2(body.position.x, RestY + Mathf.Sin(timer * 3) * .06f));
                if (timer >= (Enraged ? restDuration * .72f : restDuration)) BeginJump();
                break;
            case State.Prepare:
                if (timer >= warningDuration) { from = body.position; SetState(State.Rise); }
                break;
            case State.Rise:
            case State.Fall:
                float t = Mathf.Clamp01(timer / jumpDuration);
                Vector2 point = Vector2.Lerp(from, landing, Mathf.SmoothStep(0, 1, t));
                point.y = RestY + Mathf.Sin(t * Mathf.PI) * jumpHeight;
                if (t >= .5f) state = State.Fall;
                if (contactCooldown <= 0 && LakeHit.Touch(target, body.position, point, .7f, filter, contacts))
                { target.TakeDamage(2); contactCooldown = .8f; }
                body.MovePosition(point);
                if (t >= 1) Land();
                break;
            case State.Impact:
                if (timer >= .25f) SetState(State.Recover);
                break;
            case State.Recover:
                if (timer >= .7f) SetState(State.Float);
                break;
        }
    }
    private void BeginJump()
    {
        var area = Arena;
        // Alternate right-side berths; no last-frame tracking or landing on the player's corridor.
        float x = area.xMin + area.width * (sequence % 2 == 0 ? .64f : .82f);
        landing = new Vector2(Mathf.Clamp(x, area.center.x + .8f, area.xMax - 1.7f), RestY);
        jumpHeight = Mathf.Min(3.4f, Mathf.Max(1.2f, area.yMax - RestY - 1.8f));
        cue = LakeWaterFX.Create(effects, new Vector2(landing.x, WaterY), 2.35f, true);
        SetState(State.Prepare);
    }
    private void Land()
    {
        body.position = landing; Landings++; sequence++;
        if (cue != null) Destroy(cue.gameObject);
        LakeWaterFX.Create(effects, new Vector2(landing.x, WaterY), 2.1f);
        EmitDrops(); SetState(State.Impact);
    }
    private void EmitDrops()
    {
        int count = Enraged ? 7 : 5;
        // A staggered fan: velocity gaps remain wide enough for the ship plus a dash.
        for (int i = 0; i < count; i++)
        {
            float direction = i % 2 == 0 ? -1 : 1;
            float speedX = direction * (1.35f + i * .48f);
            float speedY = 6.6f + (i % 3) * .65f;
            Vector2 start = new Vector2(landing.x + direction * (.8f + i * .09f), WaterY + .04f);
            SpawnDrop(start, new Vector2(speedX, speedY));
        }
    }
    private void SpawnDrop(Vector2 start, Vector2 velocity)
    {
        var node = new GameObject("DangerousLakeDrop"); node.transform.SetParent(effects, false);
        node.AddComponent<Rigidbody2D>(); node.AddComponent<SpriteRenderer>();
        node.AddComponent<LakeDroplet>().Configure(this, target, dropletSprite, start, velocity, WaterY);
        DropsLaunched++;
    }
    public void RegisterReturnedDrop() { DropsReturned++; }
    private void SetState(State next) { state = next; timer = 0; }
    private void LateUpdate()
    {
        if (visual == null || Time.timeScale <= 0) return;
        hitFlash = Mathf.Max(0, hitFlash - Time.deltaTime);
        visual.color = hitFlash > 0 ? new Color(1, .65f, .65f) : Enraged ? new Color(1, .92f, .8f) : Color.white;
        Vector2 stretch = Vector2.one;
        if (state == State.Prepare) stretch = Vector2.Lerp(Vector2.one, new Vector2(1.12f, .82f), Mathf.Clamp01(timer / warningDuration));
        else if (state == State.Rise) stretch = new Vector2(.93f, 1.1f);
        else if (state == State.Fall) stretch = new Vector2(1.03f, 1.03f);
        else if (state == State.Impact) stretch = new Vector2(1.16f, .78f);
        else if (state == State.Recover) stretch = Vector2.Lerp(new Vector2(1.16f, .78f), Vector2.one, Mathf.Clamp01(timer / .7f));
        visual.transform.localScale = Vector3.Scale(visualScale, new Vector3(stretch.x, stretch.y, 1));
    }
    public void StopCombat()
    {
        stopped = true; state = State.Stopped;
        if (body != null) body.linearVelocity = Vector2.zero;
        if (effects != null) { effects.gameObject.SetActive(false); Destroy(effects.gameObject); effects = null; }
        cue = null;
        if (visual != null) { visual.transform.localScale = visualScale; visual.color = Color.white; }
    }
}
