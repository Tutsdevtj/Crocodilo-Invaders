using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class MenuAmbientMotion : MonoBehaviour
{
    [SerializeField] private Vector2 amplitude = new Vector2(16f, 5f);
    [SerializeField] private Vector2 period = new Vector2(14f, 10f);
    [SerializeField] private float phase;

    private RectTransform rect;
    private Vector2 restingPosition;
    private float elapsed;

    private void Awake()
    {
        rect = (RectTransform)transform;
        restingPosition = rect.anchoredPosition;
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        // Oscila ao redor da posição da arte: não faz spawn nem atravessa a tela.
        Vector2 offset = new Vector2(
            Mathf.Sin(elapsed * Mathf.PI * 2f / Mathf.Max(.1f, period.x) + phase) * amplitude.x,
            Mathf.Sin(elapsed * Mathf.PI * 2f / Mathf.Max(.1f, period.y) + phase * 1.7f) * amplitude.y);
        rect.anchoredPosition = restingPosition + offset;
    }

    private void OnDisable()
    {
        if (rect != null) rect.anchoredPosition = restingPosition;
    }
}
