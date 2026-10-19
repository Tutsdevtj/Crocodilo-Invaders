using UnityEngine;

// As faces internas das paredes coincidem com o viewport, sem uma barreira no meio da arena.
[DefaultExecutionOrder(1000)]
public sealed class ArenaViewportBounds2D : MonoBehaviour
{
    [SerializeField] private Camera arenaCamera;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D topWall;
    [SerializeField] private BoxCollider2D bottomWall;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField, Min(.1f)] private float wallThickness = .2f;

    private Rect previousViewport;
    private bool initialized;

    private void OnEnable() { initialized = false; RefreshBounds(); }
    private void FixedUpdate() => RefreshBounds();
    private void LateUpdate() => RefreshBounds();

    public void RefreshBounds()
    {
        if (arenaCamera == null || !arenaCamera.orthographic || leftWall == null || rightWall == null || topWall == null || bottomWall == null) return;
        Vector2 min = arenaCamera.ViewportToWorldPoint(Vector3.zero);
        Vector2 max = arenaCamera.ViewportToWorldPoint(Vector3.one);
        var viewport = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        if (initialized && viewport == previousViewport) return;

        float thickness = Mathf.Max(.1f, wallThickness);
        Vector2 center = viewport.center;
        Place(leftWall, new Vector2(min.x - thickness * .5f, center.y), new Vector2(thickness, viewport.height + 2f * thickness));
        Place(rightWall, new Vector2(max.x + thickness * .5f, center.y), new Vector2(thickness, viewport.height + 2f * thickness));
        Place(topWall, new Vector2(center.x, max.y + thickness * .5f), new Vector2(viewport.width + 2f * thickness, thickness));
        Place(bottomWall, new Vector2(center.x, min.y - thickness * .5f), new Vector2(viewport.width + 2f * thickness, thickness));

        // Ao estreitar a janela, recoloca a nave dentro da nova área: a parede não pode
        // atravessá-la e deixá-la presa do lado de fora. Shake não reposiciona o player.
        if (Application.isPlaying && (!initialized || viewport.size != previousViewport.size)) ConfineAfterResize(viewport);
        previousViewport = viewport;
        initialized = true;
    }

    private static void Place(BoxCollider2D wall, Vector2 center, Vector2 size)
    {
        wall.transform.position = new Vector3(center.x, center.y, wall.transform.position.z);
        wall.offset = Vector2.zero;
        wall.size = size;
    }

    private void ConfineAfterResize(Rect viewport)
    {
        if (playerCollider == null || playerCollider.attachedRigidbody == null) return;
        Bounds bounds = playerCollider.bounds;
        Vector2 correction = Vector2.zero;
        if (bounds.min.x < viewport.xMin) correction.x = viewport.xMin - bounds.min.x;
        else if (bounds.max.x > viewport.xMax) correction.x = viewport.xMax - bounds.max.x;
        if (bounds.min.y < viewport.yMin) correction.y = viewport.yMin - bounds.min.y;
        else if (bounds.max.y > viewport.yMax) correction.y = viewport.yMax - bounds.max.y;
        if (correction != Vector2.zero) playerCollider.attachedRigidbody.position += correction;
    }
}
