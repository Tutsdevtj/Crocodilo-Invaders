using UnityEngine;
using UnityEngine.UI;

public sealed class PauseMenuParticles : MonoBehaviour
{
    [SerializeField] private RectTransform area;
    [SerializeField] private Image[] particles;
    private float[] phases;
    private float[] speeds;
    private float[] horizontal;
    private static readonly System.Random VisualRandom = new System.Random();

    private void Awake()
    {
        phases = new float[particles.Length];
        speeds = new float[particles.Length];
        horizontal = new float[particles.Length];
        for (int i = 0; i < particles.Length; i++)
        {
            phases[i] = (float)VisualRandom.NextDouble();
            horizontal[i] = (float)VisualRandom.NextDouble() - .5f;
            speeds[i] = 1f / Mathf.Lerp(18f, 32f, (float)VisualRandom.NextDouble());
        }
    }

    private void Update()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            phases[i] = Mathf.Repeat(phases[i] + Time.unscaledDeltaTime * speeds[i], 1f);
            particles[i].rectTransform.anchoredPosition = new Vector2(
                horizontal[i] * area.rect.width + Mathf.Sin(phases[i] * 6f + i) * 9f,
                (phases[i] - .5f) * area.rect.height);
            particles[i].color = new Color(1f, .86f, .5f, Mathf.Sin(phases[i] * Mathf.PI) * .25f);
        }
    }
}
