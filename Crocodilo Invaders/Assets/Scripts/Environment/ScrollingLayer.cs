using System;
using UnityEngine;

// Move elementos visuais horizontalmente: faixa repetida ou passagem única.
public sealed class ScrollingLayer : MonoBehaviour
{
    private enum ScrollMode { RepeatingLayer, SinglePass }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private ScrollMode mode;
    [Tooltip("Unidades por segundo. Positivo move para a esquerda; negativo, para a direita.")]
    [SerializeField] private float scrollSpeed = 0.6f;
    [SerializeField, Min(0f)] private float spacing;
    [SerializeField] private bool alternateMirroring;
    [SerializeField, Min(0f)] private float despawnMargin = 0.2f;
    [Tooltip("Cópias do mesmo sprite, com a mesma escala, como filhos desta camada.")]
    [SerializeField] private SpriteRenderer[] segments;

    private float distance;
    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic || segments == null || segments.Length == 0)
        {
            DisableWithError();
            return;
        }

        foreach (SpriteRenderer segment in segments)
        {
            if (segment == null || segment.sprite == null || segment.transform.parent != transform)
            {
                DisableWithError();
                return;
            }
        }

        if (mode == ScrollMode.RepeatingLayer) LayoutSegments();
    }

    public void Configure(Camera camera, float speed)
    {
        targetCamera = camera;
        scrollSpeed = speed;
    }

    // Called on an inactive layer before Awake, sharing the BattleSky tiling behaviour.
    public void Configure(Camera camera, float speed, SpriteRenderer[] tiles)
    {
        Configure(camera, speed);
        segments = tiles;
    }

    private void Update()
    {
        if (targetCamera == null) return;
        if (mode == ScrollMode.SinglePass)
        {
            MoveSinglePass();
            return;
        }
        float stride = segments[0].bounds.size.x + spacing;
        if (stride <= 0f) return;

        // Duas repetições preservam a ordem do espelhamento após cada volta.
        float cycleWidth = stride * (alternateMirroring ? 2f : 1f);
        distance = Mathf.Repeat(distance + scrollSpeed * Time.deltaTime, cycleWidth);
        LayoutSegments();
    }

    private void MoveSinglePass()
    {
        if (body == null) transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);
        Bounds bounds = segments[0].bounds;
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float cameraX = targetCamera.transform.position.x;
        bool passedLeft = scrollSpeed > 0f && bounds.max.x < cameraX - halfWidth - despawnMargin;
        bool passedRight = scrollSpeed < 0f && bounds.min.x > cameraX + halfWidth + despawnMargin;
        if (passedLeft || passedRight) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (mode == ScrollMode.SinglePass && body != null)
            body.linearVelocity = Vector2.left * scrollSpeed;
    }

    private void OnDisable()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    private void LayoutSegments()
    {
        float width = segments[0].bounds.size.x;
        float stride = width + spacing;
        if (width <= 0f || stride <= 0f) return;

        float viewWidth = targetCamera.orthographicSize * 2f * targetCamera.aspect;
        EnsureCoverage(Mathf.CeilToInt(viewWidth / stride) + 1);
        float offset = Mathf.Repeat(distance, stride);
        int passedSegments = Mathf.FloorToInt(distance / stride);
        float firstCenter = targetCamera.transform.position.x - viewWidth / 2f + width / 2f - offset;

        for (int index = 0; index < segments.Length; index++)
        {
            SpriteRenderer segment = segments[index];
            segment.flipX = alternateMirroring && (index + passedSegments) % 2 != 0;
            Vector3 position = segment.transform.position;
            // Usa o centro visível para também funcionar com pivôs fora do centro.
            position.x += firstCenter + index * stride - segment.bounds.center.x;
            segment.transform.position = position;
        }
    }

    private void EnsureCoverage(int needed)
    {
        if (segments.Length >= needed) return;
        int previousCount = segments.Length;
        Array.Resize(ref segments, needed);
        // Cria cópias só quando a largura da câmera exige mais segmentos.
        for (int index = previousCount; index < needed; index++)
        {
            segments[index] = Instantiate(segments[0], transform);
            segments[index].name = "Segment" + (index + 1);
        }
    }

    private void DisableWithError()
    {
        Debug.LogError("Configure uma câmera ortográfica e sprites filhos da camada para a rolagem.", this);
        enabled = false;
    }
}
