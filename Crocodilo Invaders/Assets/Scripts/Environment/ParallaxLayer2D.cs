using UnityEngine;

// Camada de profundidade horizontal. scrollFactor é a velocidade aparente em relação ao mundo:
// 0 = infinitamente distante (acompanha a câmera), 1 = mundo, > 1 = primeiro plano.
// A posição autoral vale quando a câmera está em referenceCameraX; não acumula erro nem aloca.
[DefaultExecutionOrder(1000)]
public sealed class ParallaxLayer2D : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Tooltip("0 = parado na tela, 1 = mundo, > 1 = primeiro plano mais rápido.")]
    private float scrollFactor = .1f;
    [SerializeField, Tooltip("X da câmera em que a camada está na posição montada na cena.")]
    private float referenceCameraX;
    [SerializeField, Tooltip("Somente para arte realmente repetível: largura de um ciclo, em unidades. 0 = não repete.")]
    private float repeatWidth;
    [SerializeField, Tooltip("Arredonda para a grade de pixels da tela (unidades por pixel de referência). 0 = desligado.")]
    private float snapPixelsPerUnit;

    private Transform cached;
    private Transform cameraTransform;
    private float baseX;

    public float ScrollFactor => scrollFactor;
    public float BaseX => baseX;

    private void Awake()
    {
        cached = transform;
        baseX = cached.position.x;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null) cameraTransform = targetCamera.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        float offset = (cameraTransform.position.x - referenceCameraX) * (1f - scrollFactor);
        if (repeatWidth > 0f)
        {
            // Mantém o ciclo perto da câmera: só faz sentido para texturas sem emenda.
            float drift = (cameraTransform.position.x - referenceCameraX) * scrollFactor;
            offset += Mathf.Floor(drift / repeatWidth) * repeatWidth;
        }
        float x = baseX + offset;
        if (snapPixelsPerUnit > 0f) x = Mathf.Round(x * snapPixelsPerUnit) / snapPixelsPerUnit;
        Vector3 position = cached.position;
        position.x = x;
        cached.position = position;
    }
}
