using UnityEngine;
using UnityEngine.UI;

public sealed class PauseBackdrop : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RawImage background;
    [SerializeField] private Material blurMaterial;
    [SerializeField, Range(1f, 5f)] private float blurRadius = 2.2f;

    private RenderTexture snapshot;
    private RenderTexture intermediate;
    private RenderTexture blurred;
    private Material blur;

    public void Capture()
    {
        Release();
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null || blurMaterial == null) return;
        int width = Mathf.Max(1, Screen.width / 3);
        int height = Mathf.Max(1, Screen.height / 3);
        snapshot = CreateTexture(width, height, 16);
        intermediate = CreateTexture(width, height, 0);
        blurred = CreateTexture(width, height, 0);
        RenderTexture previousTarget = gameCamera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        try
        {
            gameCamera.targetTexture = snapshot;
            gameCamera.Render();
            blur = new Material(blurMaterial);
            blur.SetFloat("_Radius", blurRadius);
            Graphics.Blit(snapshot, intermediate, blur, 0);
            Graphics.Blit(intermediate, blurred, blur, 1);
            background.texture = blurred;
        }
        finally
        {
            gameCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
        }
    }

    private static RenderTexture CreateTexture(int width, int height, int depth)
    {
        var texture = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32)
            { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        texture.Create();
        return texture;
    }

    public void Release()
    {
        if (background != null) background.texture = null;
        foreach (var texture in new[] { snapshot, intermediate, blurred })
            if (texture != null) { texture.Release(); Destroy(texture); }
        snapshot = intermediate = blurred = null;
        if (blur != null) Destroy(blur);
        blur = null;
    }

    private void OnDisable() => Release();
}
