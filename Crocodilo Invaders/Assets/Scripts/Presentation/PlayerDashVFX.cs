using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Somente apresentação. Movimento e proteção pertencem ao PlayerDash/Health.
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(PlayerDash), typeof(Health))]
public sealed class PlayerDashVFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ship;
    [SerializeField] private SpriteRenderer[] composition;
    [SerializeField] private Sprite[] ignitionHorizontal, ignitionDiagonal, streakHorizontal, streakDiagonal, endHorizontal, endDiagonal;
    [SerializeField] private Material afterimageMaterial;
    [SerializeField] private AudioClip dashSound;
    [SerializeField, Range(0f, 1f)] private float dashVolume = .25f;
    [SerializeField, Min(.01f)] private float afterimageInterval = .03f;
    [SerializeField, Min(.01f)] private float afterimageLifetime = .13f;
    [SerializeField, HideInInspector] private Sprite[] silhouetteSprites = System.Array.Empty<Sprite>();
    [SerializeField, HideInInspector] private float[] shadeThresholds = System.Array.Empty<float>();
    private readonly Dictionary<Sprite,float> shadeBySprite = new Dictionary<Sprite,float>();
    private sealed class Puff { public SpriteRenderer image; public Sprite[] frames; public float born, frameTime; public bool active; }
    private sealed class Ghost { public SpriteRenderer[] images; public bool[] visible; public float born; public bool active; }
    private readonly Ghost[] ghosts = new Ghost[6];
    private readonly Puff[] ignition = new Puff[2], ending = new Puff[2];
    private MaterialPropertyBlock block;
    private PlayerDash dash;
    private Health health;
    private Rigidbody2D body;
    private Transform worldRoot;
    private SpriteRenderer streak;
    private AudioSource audioSource;
    private float streakStart, streakEnd = float.NegativeInfinity, nextGhost;
    private bool diagonal, flipX, flipY, emitting;
    private float angle;
    private Vector2 visualDirection;
    private static readonly int Outline = Shader.PropertyToID("_DashOutline");
    private static readonly int OutlineColor = Shader.PropertyToID("_DashOutlineColor");
    private static readonly int Age = Shader.PropertyToID("_DashAge");
    private static readonly int Shade = Shader.PropertyToID("_DashShade");
    public int ActiveAfterimages { get { int n = 0; foreach (var ghost in ghosts) if (ghost != null && ghost.active) n++; return n; } }
    public bool StreakVisible => streak != null && streak.enabled;
    public bool OutlineVisible { get; private set; }
    public Transform WorldRoot => worldRoot;

    private void Awake()
    {
        dash = GetComponent<PlayerDash>(); health = GetComponent<Health>(); body = GetComponent<Rigidbody2D>();
        block = new MaterialPropertyBlock();
        for (int i = 0; i < silhouetteSprites.Length; i++) shadeBySprite[silhouetteSprites[i]] = shadeThresholds[i];
        var root = new GameObject("Player dash VFX pool"); SceneManager.MoveGameObjectToScene(root, gameObject.scene); worldRoot = root.transform;
        for (int i = 0; i < ghosts.Length; i++)
        {
            var ghost = ghosts[i] = new Ghost { images = new SpriteRenderer[composition.Length], visible = new bool[composition.Length] };
            for (int j = 0; j < composition.Length; j++) ghost.images[j] = Image("Afterimage " + i + "/" + j, worldRoot, afterimageMaterial);
        }
        for (int i = 0; i < 2; i++) { ignition[i] = new Puff { image = Image("Ignition " + i, worldRoot) }; ending[i] = new Puff { image = Image("End " + i, worldRoot) }; }
        streak = Image("Dash speed streak", ship.transform);
        if (dashSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; audioSource.spatialBlend = 0f;
            GameAudioSettings.Route(audioSource, AudioChannel.Effects);
        }
    }
    private static SpriteRenderer Image(string name, Transform parent, Material material = null)
    {
        var root = new GameObject(name); root.transform.SetParent(parent, false);
        var image = root.AddComponent<SpriteRenderer>(); image.enabled = false;
        if (material != null) image.sharedMaterial = material;
        return image;
    }
    private void OnEnable()
    {
        dash.DashStarted += Started; dash.DashEnded += Ended; dash.DashCancelled += Cancelled;
        dash.InvulnerabilityEnded += RemoveOutline; health.Died += OnDeath;
    }
    private void OnDisable()
    {
        dash.DashStarted -= Started; dash.DashEnded -= Ended; dash.DashCancelled -= Cancelled;
        dash.InvulnerabilityEnded -= RemoveOutline; health.Died -= OnDeath;
        ClearAll();
    }
    // Arredonda somente a arte. O vetor físico permanece o vetor analógico original.
    public static void Orientation(Vector2 direction, out bool diagonal, out bool flipX, out bool flipY, out float angle)
    {
        int sector = ((Mathf.RoundToInt(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg / 45f) % 8) + 8) % 8;
        diagonal = (sector & 1) != 0; angle = sector == 2 ? 90f : sector == 6 ? -90f : 0f;
        flipX = sector == 3 || sector == 4 || sector == 5; flipY = sector == 5 || sector == 7;
    }
    public Vector3 VisualCenter
    {
        get
        {
            Sprite sprite = ship.sprite;
            Vector2 pivot = sprite.pivot;
            Vector3 local = new Vector3((72f - pivot.x) / sprite.pixelsPerUnit, (sprite.rect.height - 59f - pivot.y) / sprite.pixelsPerUnit);
            if (ship.flipX) local.x = -local.x;
            if (ship.flipY) local.y = -local.y;
            return ship.transform.TransformPoint(local);
        }
    }
    private Vector3 Rear()
    {
        Vector3 scale = ship.transform.lossyScale;
        float radius = diagonal ? 52f : Mathf.Abs(visualDirection.y) > .5f ? 47f : 56f;
        Vector2 offset = dash.Direction * radius / ship.sprite.pixelsPerUnit;
        offset = new Vector2(offset.x * Mathf.Abs(scale.x), offset.y * Mathf.Abs(scale.y));
        return VisualCenter + PhysicsDelta - (Vector3)offset;
    }
    private Vector3 PhysicsDelta => body != null ? (Vector3)body.position - transform.position : Vector3.zero;
    private void Started(Vector2 direction)
    {
        Orientation(direction, out diagonal, out flipX, out flipY, out angle);
        visualDirection = new Vector2(Mathf.Cos(Mathf.Atan2(direction.y, direction.x)), Mathf.Sin(Mathf.Atan2(direction.y, direction.x)));
        emitting = true; streakStart = Time.time; streakEnd = float.NegativeInfinity;
        SpawnPuff(ignition, diagonal ? ignitionDiagonal : ignitionHorizontal, .02f, ship.sortingOrder - 3);
        streak.transform.localRotation = Quaternion.Euler(0, 0, angle); streak.transform.localScale = Vector3.one;
        streak.flipX = flipX; streak.flipY = flipY; streak.sortingLayerID = ship.sortingLayerID; streak.sortingOrder = ship.sortingOrder - 2;
        Snapshot(); nextGhost = Time.time + afterimageInterval;
        SetOutline(dash.HasInvulnerability);
        if (audioSource != null && dashSound != null) audioSource.PlayOneShot(dashSound, dashVolume);
    }
    private void Ended()
    {
        emitting = false; streakEnd = Time.time;
        SpawnPuff(ending, diagonal ? endDiagonal : endHorizontal, .025f, ship.sortingOrder + 1);
    }
    private void Cancelled()
    {
        if (health.IsDead || !dash.isActiveAndEnabled) { ClearAll(); return; }
        Ended();
    }
    private void OnDeath() => ClearAll();
    private void RemoveOutline() => SetOutline(false);
    private void SetOutline(bool enabled)
    {
        OutlineVisible = enabled;
        Color color = ((int)(Time.time / .04f) & 1) == 0 ? new Color32(170, 234, 255, 255) : new Color32(240, 252, 255, 255);
        foreach (var image in composition)
        {
            if (image == null) continue;
            image.GetPropertyBlock(block); block.SetFloat(Outline, enabled ? 1f : 0f); block.SetColor(OutlineColor, color); image.SetPropertyBlock(block);
        }
    }
    private void SpawnPuff(Puff[] pool, Sprite[] frames, float frameTime, int sorting)
    {
        Puff puff = pool[0]; foreach (var candidate in pool) if (!candidate.active) { puff = candidate; break; }
        puff.frames = frames; puff.born = Time.time; puff.frameTime = frameTime; puff.active = true;
        var image = puff.image; image.transform.position = Rear(); image.transform.rotation = Quaternion.Euler(0, 0, angle);
        Vector3 scale = ship.transform.lossyScale;
        image.transform.localScale = Mathf.Abs(angle) > .1f ? new Vector3(Mathf.Abs(scale.y), Mathf.Abs(scale.x), 1f) : new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);
        image.flipX = flipX; image.flipY = flipY; image.sortingLayerID = ship.sortingLayerID; image.sortingOrder = sorting;
        image.sprite = frames[0]; image.color = Color.white; image.enabled = true;
    }
    private void FixedUpdate()
    {
        if (!emitting || !dash.IsDashing || Time.time < nextGhost) return;
        Snapshot(); nextGhost += afterimageInterval;
    }
    private void Snapshot()
    {
        Ghost ghost = ghosts[0];
        foreach (var candidate in ghosts) if (!candidate.active || Time.time - candidate.born >= afterimageLifetime) { ghost = candidate; break; }
        ghost.active = true; ghost.born = Time.time;
        for (int j = 0; j < composition.Length; j++)
        {
            var source = composition[j]; var copy = ghost.images[j];
            copy.sprite = source.sprite; copy.transform.SetPositionAndRotation(source.transform.position + PhysicsDelta, source.transform.rotation);
            copy.transform.localScale = source.transform.lossyScale; copy.flipX = source.flipX; copy.flipY = source.flipY;
            copy.sortingLayerID = source.sortingLayerID; copy.sortingOrder = source.sortingOrder - 1;
            copy.color = Color.white; ghost.visible[j] = source.enabled && source.gameObject.activeInHierarchy; copy.enabled = ghost.visible[j];
            copy.GetPropertyBlock(block); block.SetFloat(Age, 0f);
            block.SetFloat(Shade, source.sprite != null && shadeBySprite.TryGetValue(source.sprite, out float threshold) ? threshold : .38f);
            copy.SetPropertyBlock(block);
        }
    }
    private void LateUpdate()
    {
        if (!dash.isActiveAndEnabled || health.IsDead) { ClearAll(); return; }
        SetOutline(dash.HasInvulnerability);
        AnimatePuffs(ignition); AnimatePuffs(ending);
        foreach (var ghost in ghosts)
        {
            if (!ghost.active) continue;
            float age = Time.time - ghost.born;
            ghost.active = age < afterimageLifetime;
            for (int i = 0; i < ghost.images.Length; i++)
            {
                var image = ghost.images[i]; image.enabled = ghost.active && ghost.visible[i];
                image.GetPropertyBlock(block); block.SetFloat(Age, age / afterimageLifetime * .13f); image.SetPropertyBlock(block);
            }
        }
        float fade = emitting ? 1f : Mathf.Clamp01(1f - (Time.time - streakEnd) / .04f);
        streak.enabled = Time.time - streakStart >= .02f && fade > 0f;
        if (streak.enabled)
        {
            streak.transform.position = VisualCenter;
            Sprite[] frames = diagonal ? streakDiagonal : streakHorizontal;
            streak.sprite = frames[(int)(Mathf.Max(0f, Time.time - streakStart - .02f) / .03f) % frames.Length];
            streak.color = new Color(1f, 1f, 1f, fade);
        }
    }
    private static void AnimatePuffs(Puff[] pool)
    {
        foreach (var puff in pool)
        {
            if (!puff.active) continue;
            int frame = (int)((Time.time - puff.born) / puff.frameTime);
            puff.active = frame < puff.frames.Length; puff.image.enabled = puff.active;
            if (puff.active) puff.image.sprite = puff.frames[frame];
        }
    }
    private static void ClearPuffs(Puff[] pool)
    {
        foreach (var puff in pool) if (puff != null) { puff.active = false; if (puff.image != null) puff.image.enabled = false; }
    }
    private void ClearAll()
    {
        emitting = false; streakEnd = float.NegativeInfinity;
        if (streak != null) streak.enabled = false;
        ClearPuffs(ignition); ClearPuffs(ending);
        foreach (var ghost in ghosts) if (ghost != null) { ghost.active = false; foreach (var image in ghost.images) if (image != null) image.enabled = false; }
        if (ship != null) SetOutline(false);
        if (audioSource != null) audioSource.Stop();
    }
    private void OnDestroy() { if (worldRoot != null) Destroy(worldRoot.gameObject); }
}
