using UnityEngine;

// Um único Update para todos os halos que tremulam: variação lenta e discreta do alpha.
public sealed class BaseLightFlicker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] halos = new SpriteRenderer[0];
    [SerializeField, Range(0f, .5f)] private float amount = .12f;
    [SerializeField, Min(.1f)] private float speed = 2.2f;

    private float[] baseAlpha = new float[0];

    private void Awake()
    {
        baseAlpha = new float[halos.Length];
        for (int i = 0; i < halos.Length; i++) baseAlpha[i] = halos[i] != null ? halos[i].color.a : 0f;
    }

    private void Update()
    {
        float t = Time.time * speed;
        for (int i = 0; i < halos.Length; i++)
        {
            SpriteRenderer halo = halos[i];
            if (halo == null) continue;
            float noise = Mathf.PerlinNoise(t, i * 7.31f) - .5f;
            Color color = halo.color;
            color.a = baseAlpha[i] * (1f + noise * 2f * amount);
            halo.color = color;
        }
    }
}
