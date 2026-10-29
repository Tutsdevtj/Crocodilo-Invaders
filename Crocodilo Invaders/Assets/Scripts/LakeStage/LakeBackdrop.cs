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
    private readonly float[] drift = { 0, .025f, .045f, .07f, .09f, 0, .02f, .12f };
    private SpriteRenderer[] views;
    private float clock;
    private MaterialPropertyBlock properties;
    public float MotionClock => clock;
    public Material WaterFXMaterial => waterFXMaterial;
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
            if (motionMaterial != null && (i >= 3)) views[i].sharedMaterial = motionMaterial;
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
            float sway = i == 0 ? 0 : Mathf.Sin(clock * (i == 1 ? .11f : drift[i] * 2)) * (i == 1 ? .3f : .08f);
            float extra = i == 0 ? 1f : 1.045f;
            view.transform.localScale = new Vector3(width / view.sprite.bounds.size.x * extra, height / view.sprite.bounds.size.y, 1);
            view.transform.position = new Vector3(gameplayCamera.transform.position.x + sway,
                gameplayCamera.transform.position.y - offsets[i] / 941f * height, 4f);
            if (motionMaterial != null && i >= 3)
            {
                if (properties == null) properties = new MaterialPropertyBlock();
                view.GetPropertyBlock(properties);
                properties.SetFloat("_Clock", clock);
                properties.SetFloat("_Mode", i == 5 ? 0 : i == 6 ? 1 : 2);
                properties.SetFloat("_Root", i == 5 ? .325f : i == 7 ? 0 : .26f);
                properties.SetFloat("_Strength", i == 3 ? .75f : i == 4 ? 1.15f : i == 7 ? 1.5f : 1);
                view.SetPropertyBlock(properties);
            }
        }
    }
}
