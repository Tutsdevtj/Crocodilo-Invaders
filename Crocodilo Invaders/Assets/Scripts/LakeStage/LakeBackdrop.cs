using UnityEngine;

// Full-frame registered layers: horizon stays at 72% at every aspect ratio.
[ExecuteAlways]
[DefaultExecutionOrder(-100)]
public sealed class LakeBackdrop : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Sprite[] layers;
    [SerializeField] private Material motionMaterial;
    [SerializeField] private Material waterFXMaterial;
    private readonly int[] offsets = { 0, 0, -12, -23, -31, 42, 11, 0 };
    // World units per second: every layer advances left, nearer banks move faster.
    private readonly float[] scrollSpeed = { 0, .12f, .28f, .52f, .86f, .24f, .43f, 1.22f };
    private SpriteRenderer[] views;
    private SpriteRenderer[,] tiles;
    private float clock;
    public float JourneyDensity { get; set; }=1;
    public float TravelScale { get; set; }=1;
    private MaterialPropertyBlock properties;
    public float MotionClock => clock;
    public Material WaterFXMaterial => waterFXMaterial;
    public int LayerCount => views != null ? views.Length : 0;
    public int TileCount => views != null ? 1 + (views.Length - 1) * 3 : 0;
    public float ScrollDistance(int layer) => clock * scrollSpeed[Mathf.Clamp(layer, 0, scrollSpeed.Length - 1)];
    public float ScrollSpeed(int layer) => scrollSpeed[Mathf.Clamp(layer, 0, scrollSpeed.Length - 1)];
    public float WaterY => gameplayCamera != null ? gameplayCamera.ViewportToWorldPoint(new Vector3(.5f, .28f, 0)).y : -2.2f;
    public Camera GameplayCamera => gameplayCamera;

    private void OnEnable() { Build(); Fit(); }
    private void Build()
    {
        if (layers == null || layers.Length != 8) return;
        views = new SpriteRenderer[8];
        tiles = new SpriteRenderer[8, 3];
        for (int i = 0; i < views.Length; i++)
        {
            var child = transform.Find("Layer_" + i);
            if (child == null) { child = new GameObject("Layer_" + i).transform; child.SetParent(transform, false); }
            views[i] = child.GetComponent<SpriteRenderer>();
            if (views[i] == null) views[i] = child.gameObject.AddComponent<SpriteRenderer>();
            views[i].sprite = layers[i]; views[i].sortingOrder = i == 7 ? 32000 : -240 + i * 10;
            views[i].sortingLayerName = i == 7 ? "LakeForeground" : "Default";
            if (motionMaterial != null && (i >= 3)) views[i].sharedMaterial = motionMaterial;
            tiles[i, 1] = views[i];
            if (i == 0) continue;
            for (int t = 0; t < 3; t += 2)
            {
                var tile = child.Find(t == 0 ? "RepeatLeft" : "RepeatRight");
                if (tile == null) { tile = new GameObject(t == 0 ? "RepeatLeft" : "RepeatRight").transform; tile.SetParent(child, false); }
                var copy = tile.GetComponent<SpriteRenderer>();
                if (copy == null) copy = tile.gameObject.AddComponent<SpriteRenderer>();
                copy.sprite = layers[i]; copy.sortingOrder = views[i].sortingOrder;
                copy.sortingLayerID = views[i].sortingLayerID;
                copy.sharedMaterial = views[i].sharedMaterial;
                tiles[i, t] = copy;
            }
        }
    }
    private void LateUpdate()
    {
        if (Application.isPlaying) clock += Time.deltaTime*TravelScale;
        Fit();
    }
    private void Fit()
    {
        if (gameplayCamera == null || views == null) return;
        float height = gameplayCamera.orthographicSize * 2f;
        float width = height * gameplayCamera.aspect;
        for (int i = 0; i < views.Length; i++)
        {
            var view = views[i]; if (view == null || view.sprite == null) continue;
            float densityAlpha=i==3?Mathf.Lerp(.62f,1,JourneyDensity):i==4?Mathf.Lerp(.32f,1,JourneyDensity):1;
            view.color=new Color(1,1,1,densityAlpha);
            float extra = i == 0 ? 1f : 1.025f;
            float tileWidth = width * extra;
            double travelled = (double)clock * scrollSpeed[i];
            long cycle = (long)System.Math.Floor(travelled / tileWidth);
            float offset = (float)(travelled - cycle * tileWidth);
            view.transform.localScale = new Vector3(width / view.sprite.bounds.size.x * extra, height / view.sprite.bounds.size.y, 1);
            view.transform.position = new Vector3(gameplayCamera.transform.position.x - offset,
                gameplayCamera.transform.position.y - offsets[i] / 941f * height, 4f);
            // Mirrored neighbors meet on the same source edge; no cross-fade or empty gap at wrap.
            view.flipX = (cycle & 1) != 0;
            if (i > 0)
            {
                for (int t = 0; t < 3; t += 2)
                {
                    var copy = tiles[i, t];
                    copy.transform.localPosition = new Vector3((t - 1) * view.sprite.bounds.size.x, 0, 0);
                    copy.transform.localScale = Vector3.one;
                    copy.flipX = ((cycle + t - 1) & 1) != 0;
                    copy.color=view.color;
                }
            }
            if (motionMaterial != null && i >= 3)
            {
                if (properties == null) properties = new MaterialPropertyBlock();
                view.GetPropertyBlock(properties);
                properties.SetFloat("_Clock", clock);
                properties.SetFloat("_Mode", i == 5 ? 0 : i == 6 ? 1 : 2);
                properties.SetFloat("_Root", i == 5 ? .325f : i == 7 ? 0 : .26f);
                properties.SetFloat("_Strength", i == 3 ? .75f : i == 4 ? 1.15f : i == 7 ? 1.5f : 1);
                view.SetPropertyBlock(properties);
                if (i > 0) { tiles[i, 0].SetPropertyBlock(properties); tiles[i, 2].SetPropertyBlock(properties); }
            }
        }
    }
}
