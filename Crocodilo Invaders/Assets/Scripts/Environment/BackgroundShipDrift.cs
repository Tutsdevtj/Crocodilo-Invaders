using UnityEngine;

// A nave distante atravessa o cenário sem virar entidade de gameplay.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BackgroundShipDrift : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(0f)] private float scrollSpeed = .08f;
    [SerializeField, Min(0f)] private float loopMargin = .35f;
    private SpriteRenderer ship;

    private void Awake()
    {
        ship = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f || targetCamera == null || !targetCamera.orthographic) return;
        // Velocidade em world space, independente da escala usada para encaixar o céu.
        transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);
        float halfView = targetCamera.orthographicSize * targetCamera.aspect;
        float left = targetCamera.transform.position.x - halfView - loopMargin;
        Bounds bounds = ship.bounds;
        if (bounds.max.x >= left) return;

        // A troca acontece fora da tela; fumaça e avarias viajam junto como filhos.
        float right = targetCamera.transform.position.x + halfView + loopMargin;
        transform.position += Vector3.right * (right - bounds.min.x);
    }
}
