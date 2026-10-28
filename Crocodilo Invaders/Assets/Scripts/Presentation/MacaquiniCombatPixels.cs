using UnityEngine;

// Tiny code-native pixel marks. No screen-filling beam, additive glow or new texture dependency.
public static class MacaquiniCombatPixels
{
    private static Sprite diamond, streak;
    public static Sprite Diamond => diamond == null ? diamond = Make(false) : diamond;
    public static Sprite Streak => streak == null ? streak = Make(true) : streak;
    private static Sprite Make(bool elongated)
    {
        int width = elongated ? 12 : 5, height = 5;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "MacaquiniPixelMark" };
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            bool fill = elongated ? Mathf.Abs(y - 2) <= (x < 4 ? 0 : x < 9 ? 1 : 2) : Mathf.Abs(x - 2) + Mathf.Abs(y - 2) <= 2;
            pixels[y * width + x] = fill ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
        }
        texture.SetPixels32(pixels); texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 32);
    }
    public static SpriteRenderer Mark(Transform parent, string name, bool longMark, int order)
    {
        var node = new GameObject(name); node.transform.SetParent(parent, false);
        var view = node.AddComponent<SpriteRenderer>(); view.sprite = longMark ? Streak : Diamond; view.sortingOrder = order; return view;
    }
}
