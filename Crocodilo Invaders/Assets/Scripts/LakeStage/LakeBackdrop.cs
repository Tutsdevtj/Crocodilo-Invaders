using UnityEngine;

// Full-frame registered layers: horizon stays at 72% at every aspect ratio.
[ExecuteAlways]
public sealed class LakeBackdrop : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Sprite[] layers;
    private readonly int[] offsets = { 0, 0, -12, -23, -31, 42, 11, 0 };
    private readonly float[] drift = { 0, .025f, .045f, .07f, .09f, 0, .02f, .12f };
    private SpriteRenderer[] views;
    private float clock;
    public float WaterY => gameplayCamera != null ? gameplayCamera.ViewportToWorldPoint(new Vector3(.5f, .28f, 0)).y : -2.2f;
    public Camera GameplayCamera => gameplayCamera;

    private void OnEnable() { Build(); Fit(); }
    private void Build()
    {
        if (layers == null || layers.Length != 8) return;
        views = new SpriteRenderer[8];
        for (int i = 0; i < views.Length; i++)
        {
            var child = transform.Find("Layer_" + i);
            if (child == null) { child = new GameObject("Layer_" + i).transform; child.SetParent(transform, false); }
            views[i] = child.GetComponent<SpriteRenderer>();
            if (views[i] == null) views[i] = child.gameObject.AddComponent<SpriteRenderer>();
            views[i].sprite = layers[i]; views[i].sortingOrder = -180 + i;
        }
    }
    private void LateUpdate()
    {
        if (Application.isPlaying) clock += Time.deltaTime;
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
            // Tiny bounded drift avoids a hard wrap seam in the first playable map.
            float sway = i == 0 ? 0 : Mathf.Sin(clock * drift[i]) * .06f;
            float extra = i == 0 ? 1f : 1.015f;
            view.transform.localScale = new Vector3(width / view.sprite.bounds.size.x * extra, height / view.sprite.bounds.size.y, 1);
            view.transform.position = new Vector3(gameplayCamera.transform.position.x + sway,
                gameplayCamera.transform.position.y - offsets[i] / 941f * height, 4f);
        }
    }
}
