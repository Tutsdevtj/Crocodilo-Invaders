using UnityEngine;

// Alinha a camada verticalmente à câmera, sem controlar sua rolagem.
public sealed class ViewportAnchor : MonoBehaviour
{
    private enum VerticalEdge { Bottom, Center, Top, Viewport }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer referenceSprite;
    [SerializeField] private VerticalEdge edge;
    [SerializeField] private float offset;
    [SerializeField] private bool anchorHorizontal;
    [SerializeField] private Vector2 viewportPosition = new Vector2(0.5f, 0.8f);

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic || referenceSprite == null || referenceSprite.sprite == null)
        {
            Debug.LogError("Configure uma câmera ortográfica e um sprite de referência para o alinhamento.", this);
            enabled = false;
            return;
        }

        Align();
    }

    private void LateUpdate()
    {
        if (targetCamera != null) Align();
    }

    private void Align()
    {
        Bounds bounds = referenceSprite.bounds;
        float targetY = targetCamera.transform.position.y + offset;
        float spriteY = bounds.center.y;
        if (edge == VerticalEdge.Bottom)
        {
            targetY -= targetCamera.orthographicSize;
            spriteY = bounds.min.y;
        }
        else if (edge == VerticalEdge.Top)
        {
            targetY += targetCamera.orthographicSize;
            spriteY = bounds.max.y;
        }
        else if (edge == VerticalEdge.Viewport)
        {
            targetY += (Mathf.Clamp01(viewportPosition.y) * 2f - 1f) * targetCamera.orthographicSize;
        }

        transform.position += Vector3.up * (targetY - spriteY);
        if (anchorHorizontal)
        {
            float targetX = targetCamera.transform.position.x +
                (Mathf.Clamp01(viewportPosition.x) * 2f - 1f) * targetCamera.orthographicSize * targetCamera.aspect;
            transform.position += Vector3.right * (targetX - bounds.center.x);
        }
    }
}
