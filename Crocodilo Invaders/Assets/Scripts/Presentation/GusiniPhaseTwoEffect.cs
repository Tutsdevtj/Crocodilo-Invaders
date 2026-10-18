using UnityEngine;

// FX curtos compartilham a raiz de ameaças: pausa e encerramento limpam tudo juntos.
public sealed class GusiniPhaseTwoEffect : MonoBehaviour
{
    private SpriteRenderer visual;
    private Sprite[] frames;
    private float[] durations;
    private float age, lifetime, total;
    private bool loop;
    public static GusiniPhaseTwoEffect Create(Sprite[] frames, float[] durations, Transform parent,
        Vector3 position, float pixelScale, bool loop = false, float lifetime = 0f)
    {
        var root = new GameObject("Gusini F2 effect"); root.transform.SetParent(parent, false);
        root.transform.position = position; root.transform.localScale = Vector3.one * pixelScale * 100f;
        var renderer = root.AddComponent<SpriteRenderer>(); renderer.sortingOrder = 30; renderer.sprite = frames[0];
        var effect = root.AddComponent<GusiniPhaseTwoEffect>();
        effect.visual = renderer; effect.frames = frames; effect.durations = durations; effect.loop = loop;
        foreach (float duration in durations) effect.total += duration;
        effect.lifetime = lifetime > 0f ? lifetime : effect.total;
        return effect;
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime) { Destroy(gameObject); return; }
        float t = loop ? age % total : age;
        for (int i = 0; i < frames.Length; i++)
        {
            if (t < durations[i]) { visual.sprite = frames[i]; return; }
            t -= durations[i];
        }
    }
}
