using UnityEngine;

public sealed class HealthPickupSpawner : MonoBehaviour
{
    [SerializeField] private HealthPickup pickupPrefab;
    [SerializeField] private Player player;
    [SerializeField] private WaveController waves;
    [SerializeField] private Camera targetCamera;
    [Header("Necessidade de cura")]
    [SerializeField, Range(.01f, 1f)] private float lowHealthFraction = .65f;
    [SerializeField, Range(.01f, 1f)] private float criticalHealthFraction = .25f;
    [SerializeField, Min(0f)] private float needDuration = 4f;
    [SerializeField, Min(0f)] private float criticalNeedDuration = 1.5f;
    [SerializeField, Min(0f)] private float collectedCooldown = 20f;
    [SerializeField, Min(0f)] private float missedCooldown = 10f;
    [Header("Passagem pela tela")]
    [SerializeField, Min(.1f)] private float scrollSpeed = 2f;
    [SerializeField] private Vector2 heightRange = new Vector2(-1.5f, 5.1f);
    [SerializeField, Min(0f)] private float heightVariation = 1.2f;
    [SerializeField, Min(0f)] private float spawnMargin = .35f;

    private float needStartedAt = -1f;
    private float nextSpawnAt;
    private HealthPickup activePickup;
    public HealthPickup ActivePickup => activePickup;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (pickupPrefab != null && pickupPrefab.GetComponent<ScrollingLayer>() != null
            && pickupPrefab.GetComponent<Rigidbody2D>() != null && pickupPrefab.GetComponentInChildren<SpriteRenderer>() != null
            && player != null && waves != null && targetCamera != null && targetCamera.orthographic) return;
        Debug.LogError("Configure pickup, player, ondas e câmera ortográfica no HealthPickupSpawner.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        if (waves != null) waves.RunEnded += OnRunEnded;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        WaveController.RunPhase phase = waves.Phase;
        if (player.Health.IsDead || phase == WaveController.RunPhase.Waiting
            || phase == WaveController.RunPhase.Completed || phase == WaveController.RunPhase.Failed
            || phase == WaveController.RunPhase.BossDefeated)
        {
            needStartedAt = -1f;
            ClearPickup();
            return;
        }
        // Uma caixa em passagem ainda pode ser coletada entre ondas.
        if (activePickup != null) return;
        // Durante o diálogo de entrada o player está sem controle: nenhuma caixa passa sem chance de coleta.
        bool bossHeld = phase == WaveController.RunPhase.Boss && waves.ActiveBoss != null && waves.ActiveBoss.CombatHeld;
        if ((phase != WaveController.RunPhase.Combat && phase != WaveController.RunPhase.Boss) || bossHeld)
        {
            needStartedAt = -1f;
            return;
        }

        Health health = player.Health;
        float fraction = health.Fraction;
        if (fraction > lowHealthFraction || health.MaxHealth - health.PreciseCurrentHealth < pickupPrefab.HealAmount)
        {
            needStartedAt = -1f;
            return;
        }
        if (needStartedAt < 0f) needStartedAt = Time.time;
        float delay = fraction <= criticalHealthFraction ? Mathf.Min(needDuration, criticalNeedDuration) : needDuration;
        if (Time.time < nextSpawnAt || Time.time - needStartedAt < delay) return;
        SpawnPickup();
    }

    private void SpawnPickup()
    {
        activePickup = Instantiate(pickupPrefab, transform);
        activePickup.Removed += OnPickupRemoved;
        activePickup.GetComponent<ScrollingLayer>().Configure(targetCamera, scrollSpeed);
        Bounds bounds = activePickup.GetComponentInChildren<SpriteRenderer>().bounds;
        float halfHeight = targetCamera.orthographicSize;
        float bottom = Mathf.Max(Mathf.Min(heightRange.x, heightRange.y), targetCamera.transform.position.y - halfHeight + bounds.extents.y);
        float top = Mathf.Min(Mathf.Max(heightRange.x, heightRange.y), targetCamera.transform.position.y + halfHeight - bounds.extents.y);
        float y = Mathf.Clamp(player.transform.position.y + Random.Range(-heightVariation, heightVariation), bottom, Mathf.Max(bottom, top));
        float x = targetCamera.transform.position.x + halfHeight * targetCamera.aspect + bounds.extents.x + spawnMargin;
        Vector3 position = activePickup.transform.position + new Vector3(x - bounds.center.x, y - bounds.center.y, 0f);
        activePickup.transform.position = position;
        activePickup.GetComponent<Rigidbody2D>().position = position;
    }

    public bool TrySpawnForBossTransition()
    {
        if (!isActiveAndEnabled || activePickup != null || player == null || player.Health.IsDead
            || waves == null || waves.Phase != WaveController.RunPhase.Boss || pickupPrefab == null
            || player.Health.MaxHealth - player.Health.PreciseCurrentHealth < pickupPrefab.HealAmount) return false;
        SpawnPickup();
        // Recompensa única da transição: perto do player, durante a janela sem ataques.
        Vector3 position = activePickup.transform.position;
        float right = targetCamera.transform.position.x + targetCamera.orthographicSize * targetCamera.aspect;
        position.x = Mathf.Min(player.transform.position.x + 2.5f, right - .8f);
        activePickup.transform.position = position;
        activePickup.GetComponent<Rigidbody2D>().position = position;
        return true;
    }

    private void OnPickupRemoved(HealthPickup pickup, bool collected)
    {
        if (pickup != activePickup) return;
        pickup.Removed -= OnPickupRemoved;
        activePickup = null;
        needStartedAt = -1f;
        nextSpawnAt = Time.time + (collected ? collectedCooldown : missedCooldown);
    }

    private void OnRunEnded(bool completed) => ClearPickup();

    private void ClearPickup()
    {
        if (activePickup == null) return;
        activePickup.Removed -= OnPickupRemoved;
        activePickup.gameObject.SetActive(false);
        Destroy(activePickup.gameObject);
        activePickup = null;
    }

    private void OnDisable()
    {
        if (waves != null) waves.RunEnded -= OnRunEnded;
        ClearPickup();
        needStartedAt = -1f;
    }
}
