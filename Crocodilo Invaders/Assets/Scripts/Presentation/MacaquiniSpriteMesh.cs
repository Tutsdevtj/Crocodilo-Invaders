using UnityEngine;

// Native Sprite assets regenerate their rectangle on reload; restore the slice's pixel-space outline.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MacaquiniSpriteMesh : MonoBehaviour
{
    [SerializeField] private Vector2[] vertices;
    [SerializeField] private int[] triangles;
    private Sprite instance;
    public void Configure(Vector2[] points, ushort[] indices)
    {
        vertices = points; triangles = System.Array.ConvertAll(indices, i => (int)i);
    }
    private void Start()
    {
        var view = GetComponent<SpriteRenderer>(); var source = view.sprite;
        instance = Sprite.Create(source.texture, source.rect, source.pivot / source.rect.size, source.pixelsPerUnit, 0, SpriteMeshType.Tight);
        instance.name = source.name;
        instance.OverrideGeometry(vertices, System.Array.ConvertAll(triangles, i => (ushort)i));
        view.sprite = instance;
        view.enabled = true;
    }
    private void OnDestroy() { if (instance != null) Destroy(instance); }
}
