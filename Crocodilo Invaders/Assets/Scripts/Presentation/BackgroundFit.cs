using UnityEngine;

// Ajusta apenas o cenário visual; os limites da arena continuam independentes.
[ExecuteAlways]
[DefaultExecutionOrder(-200)]
public sealed class BackgroundFit : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer background;

    // O parallax calcula as larguras em world space: precisa da escala correta
    // antes de seu Awake/Update, inclusive na troca de resolução.
    private void Awake() => FitToCamera();
    private void Update() => FitToCamera();

    private void LateUpdate()
    {
        FitToCamera();
    }

    public void FitToCamera()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic || background == null || background.sprite == null) return;

        Vector2 imageSize = background.sprite.bounds.size;
        float viewHeight = targetCamera.orthographicSize * 2f;
        float scale = Mathf.Max(viewHeight / imageSize.y, viewHeight * targetCamera.aspect / imageSize.x);

        // Escala uniforme preenche a câmera sem deformar os pixels da arte.
        transform.localScale = Vector3.one * scale;
        transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f);
    }
}
