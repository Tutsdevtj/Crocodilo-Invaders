using System.Collections.Generic;
using UnityEngine;

// Separate transparent art planes reuse the same seamless scroller as BattleSky.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class JungleBackdrop : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Sprite vineSprite, distantTrees, middleTrees, canopySprite, fernSprite;
    [SerializeField] private Material effectMaterial;
    private SpriteRenderer sprite;
    private GameObject windRoot;
    private readonly List<Transform> vines = new List<Transform>();
    private readonly List<LineRenderer> motes = new List<LineRenderer>();
    private readonly List<Transform> planes = new List<Transform>();
    private readonly List<Sprite> planeSprites = new List<Sprite>();
    private readonly float[] anchors = { .13f, .32f, .69f, .88f, .015f, .985f, -.055f, 1.055f };
    private float clock;
    public float WindClock => clock;
    public int VineCount => vines.Count;
    public int ParallaxLayerCount => planes.Count;
    public Transform ParallaxRoot => windRoot != null ? windRoot.transform : null;
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        windRoot = new GameObject("JungleWindLayers");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(windRoot, gameObject.scene);
        AddPlane("RemoteTrunks", distantTrees, .16f, -104, new Color(.55f, .79f, .77f, .72f));
        AddPlane("MiddleTrunks", middleTrees, .58f, -96, new Color(.62f, .78f, .64f, .85f));
        AddPlane("OverheadCanopy", canopySprite, 1.25f, -65, Color.white);
        AddPlane("ForegroundFerns", fernSprite, 2.15f, 21, new Color(.72f, .85f, .65f));
        FitPlanes();
        // Awake must see the fitted tile width, exactly as in BattleSky.
        foreach (var plane in planes) plane.gameObject.SetActive(true);
        for (int i = 0; i < 8; i++)
        {
            var node = new GameObject("SwayingVine_" + i); node.transform.SetParent(windRoot.transform, false);
            var view = node.AddComponent<SpriteRenderer>(); view.sprite = vineSprite; view.sortingOrder = i < 4 ? -70 : i < 6 ? -35 : 20;
            view.color = i < 4 ? new Color(.4f, .67f, .59f, .65f) : i < 6 ? new Color(.72f, .83f, .56f) : new Color(.4f, .57f, .34f);
            view.flipX = i % 2 == 0; vines.Add(node.transform);
        }
        for (int i = 0; i < 30; i++)
        {
            var node = new GameObject("LeafMote_" + i); node.transform.SetParent(windRoot.transform, false);
            var line = node.AddComponent<LineRenderer>(); line.sharedMaterial = effectMaterial; line.sortingOrder = i % 3 == 0 ? 19 : -25;
            line.positionCount = 3; line.startWidth = line.endWidth = i % 3 == 0 ? .035f : .02f;
            line.startColor = line.endColor = i % 3 == 0 ? new Color(.59f, .75f, .23f, .6f) : new Color(.56f, .88f, .53f, .3f); motes.Add(line);
        }
    }
    private void AddPlane(string name, Sprite art, float speed, int order, Color tint)
    {
        if (art == null) return;
        var node = new GameObject(name); node.SetActive(false); node.transform.SetParent(windRoot.transform, false);
        var tiles = new SpriteRenderer[3];
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = new GameObject("Segment" + i); tile.transform.SetParent(node.transform, false);
            tiles[i] = tile.AddComponent<SpriteRenderer>(); tiles[i].sprite = art; tiles[i].sortingOrder = order; tiles[i].color = tint;
        }
        node.AddComponent<ScrollingLayer>().Configure(gameplayCamera, speed, tiles);
        planes.Add(node.transform); planeSprites.Add(art);
    }
    private void FitPlanes()
    {
        if (gameplayCamera == null) return;
        float height = gameplayCamera.orthographicSize * 2;
        for (int i = 0; i < planes.Count; i++)
        {
            float scale = height / planeSprites[i].bounds.size.y;
            planes[i].localScale = new Vector3(scale, scale, 1);
            planes[i].position = new Vector3(0, gameplayCamera.transform.position.y, 0);
        }
    }
    private void LateUpdate()
    {
        if (gameplayCamera == null || sprite.sprite == null) return;
        clock += Time.deltaTime; float height = gameplayCamera.orthographicSize * 2, width = height * gameplayCamera.aspect;
        Vector2 size = sprite.sprite.bounds.size; float scale = Mathf.Max(height / size.y, width / size.x) * 1.06f;
        transform.localScale = new Vector3(scale, scale, 1); Vector3 center = gameplayCamera.transform.position;
        transform.position = new Vector3(center.x + Mathf.Sin(clock * .2f) * .14f, center.y + Mathf.Sin(clock * .31f) * .06f, 5);
        FitPlanes();
        for (int i = 0; i < vines.Count; i++)
        {
            float depth = i < 4 ? .85f : i < 6 ? 1.8f : 2.8f;
            float x = center.x + (anchors[i] - .5f) * width, y = center.y + height * .5f + (i < 4 ? .7f + (i % 3) * .18f : .2f);
            vines[i].position = new Vector3(x + Mathf.Sin(clock * .75f + i) * .12f * depth, y, 0);
            vines[i].localScale = Vector3.one * depth;
            vines[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * (.72f + i * .035f) + i * 1.7f) * (i < 4 ? 7 : 4.5f));
        }
        for (int i = 0; i < motes.Count; i++)
        {
            float u = Mathf.Repeat(i * .173f - clock * (i % 3 == 0 ? .065f : .025f), 1), v = Mathf.Repeat(i * .379f - clock * .018f, 1);
            Vector3 p = new Vector3(center.x + (u - .5f) * width, center.y + (v - .5f) * height + Mathf.Sin(clock * 2 + i) * .12f, 0);
            motes[i].SetPosition(0, p); motes[i].SetPosition(1, p + new Vector3(.045f, .035f)); motes[i].SetPosition(2, p + new Vector3(.09f, 0));
        }
    }
    private void OnDestroy() { if (windRoot != null) Destroy(windRoot); }
}