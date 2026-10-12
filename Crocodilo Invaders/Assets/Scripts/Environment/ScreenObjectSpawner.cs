using UnityEngine;

// Define quando e onde surgem os elementos visuais; ScrollingLayer cuida do movimento.
public sealed class ScreenObjectSpawner : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private ScrollingLayer objectPrefab;
    [SerializeField] private Vector2 intervalRange = new Vector2(3f, 5f);
    [SerializeField] private Vector2 viewportHeightRange = new Vector2(0.6f, 0.93f);
    [SerializeField] private Vector2 speedRange = new Vector2(0.8f, 1.3f);
    [SerializeField] private Vector2 scaleRange = new Vector2(1.15f, 1.5f);
    [SerializeField, Min(0f)] private float spawnMargin = 0.2f;
    [SerializeField, Min(0f)] private float initialDelay = 0.8f;
    [SerializeField, Min(0)] private int initialVisibleCount = 2;
    [SerializeField, Min(1)] private int maxAlive = 6;

    private float nextSpawn;
    private static readonly System.Random VisualRandom = new System.Random();

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null && targetCamera.orthographic && objectPrefab != null) return;
        Debug.LogError("Configure uma câmera ortográfica e um prefab com ScrollingLayer para o cenário.", this);
        enabled = false;
    }

    private void OnEnable() => nextSpawn = Time.time + initialDelay;

    private void Start()
    {
        if (!enabled) return;
        for (int i = 0; i < Mathf.Min(initialVisibleCount, maxAlive); i++) Spawn(true);
    }

    private void Update()
    {
        if (Time.timeScale == 0f || targetCamera == null || Time.time < nextSpawn) return;
        if (transform.childCount < maxAlive) Spawn(false);
        nextSpawn = Time.time + Mathf.Max(0.1f, Range(intervalRange));
    }

    private void Spawn(bool initiallyVisible)
    {
        ScrollingLayer layer = Instantiate(objectPrefab, transform);
        layer.transform.localScale = objectPrefab.transform.localScale * Mathf.Max(0.01f, Range(scaleRange));
        float speed = Range(speedRange);
        layer.Configure(targetCamera, speed);
        SpriteRenderer sprite = layer.GetComponentInChildren<SpriteRenderer>();
        Bounds bounds = sprite.bounds;
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float viewHeight = targetCamera.orthographicSize * 2f;
        float heightMargin = Mathf.Min(0.5f, bounds.extents.y / viewHeight);
        float height = Mathf.Clamp(Range(viewportHeightRange), heightMargin, 1f - heightMargin);
        float centerY = targetCamera.transform.position.y + (height - 0.5f) * viewHeight;
        float centerX = initiallyVisible
            ? targetCamera.transform.position.x + Range(new Vector2(-0.7f, 0.7f)) * halfWidth
            : targetCamera.transform.position.x + (speed >= 0f ? 1f : -1f) * (halfWidth + bounds.extents.x + spawnMargin);
        layer.transform.position += new Vector3(centerX - bounds.center.x, centerY - bounds.center.y, 0f);
    }

    private static float Range(Vector2 range)
    {
        float minimum = Mathf.Min(range.x, range.y);
        return minimum + (float)VisualRandom.NextDouble() * Mathf.Abs(range.y - range.x);
    }
}
