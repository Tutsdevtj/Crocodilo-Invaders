using UnityEngine;
using UnityEngine.UI;

// Partículas de sprite reaproveitadas: depois de nascer, ficam paradas na tela (não seguem o banner),
// derivam devagar e se dissolvem por troca de quadro, sem alpha parcial.
public sealed class BannerParticleLayer : MonoBehaviour
{
    private struct Live
    {
        public bool active;
        public float x, y, age;
        public int variant;
    }

    [SerializeField] private Image[] pool = new Image[0];
    [SerializeField] private int pixelScale = 2;
    [SerializeField] private float screenCenterX = 600f;
    [SerializeField, Min(1f)] private float lifeMs = 260f;
    [SerializeField, Min(1f)] private float frameMs = 66f;
    [SerializeField, Range(0f, 1f)] private float dissolve1At = .62f;
    [SerializeField, Range(0f, 1f)] private float dissolve2At = .82f;
    [SerializeField] private Vector2 driftPerMs = new Vector2(.18f, .06f);

    private Live[] live = new Live[0];
    private BossAlertTheme.Particle[] sprites;

    public int ActiveCount { get; private set; }
    public float LifeMs => lifeMs;
    public Image PoolImage(int index) => pool[index];
    public int PoolSize => pool.Length;

    private void Awake()
    {
        live = new Live[pool.Length];
        Clear();
    }

    public void Use(BossAlertTheme.Particle[] set) => sprites = set;

    // x/y em px de tela de referência (origem no canto superior esquerdo, y para baixo).
    public void Spawn(float screenX, float screenY, int variant, float age, float hideLeftOf)
    {
        if (sprites == null || sprites.Length == 0 || age >= lifeMs) return;
        if (live.Length != pool.Length) live = new Live[pool.Length];
        for (int i = 0; i < live.Length; i++)
        {
            if (live[i].active) continue;
            live[i] = new Live { active = true, x = screenX, y = screenY, age = Mathf.Max(0f, age), variant = variant };
            ActiveCount++;
            Draw(i, hideLeftOf);
            return;
        }
    }

    // hideLeftOf: como nas previews, penas à esquerda da frente do banner não aparecem.
    public void Advance(float deltaMs, float hideLeftOf)
    {
        if (ActiveCount == 0) return;
        for (int i = 0; i < live.Length; i++)
        {
            if (!live[i].active) continue;
            live[i].age += deltaMs;
            if (live[i].age >= lifeMs) { Release(i); continue; }
            Draw(i, hideLeftOf);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < live.Length; i++) live[i].active = false;
        for (int i = 0; i < pool.Length; i++) BannerPieces.Show(pool[i], false);
        ActiveCount = 0;
    }

    private void Draw(int index, float hideLeftOf)
    {
        Live particle = live[index];
        Image image = pool[index];
        int px = (int)(particle.x + particle.age * driftPerMs.x) / 2 * 2;
        int py = (int)(particle.y + particle.age * driftPerMs.y) / 2 * 2;
        if (px < hideLeftOf) { BannerPieces.Show(image, false); return; }
        BossAlertTheme.Particle set = sprites[(particle.variant + Mathf.FloorToInt(particle.age / frameMs)) % sprites.Length];
        float life = particle.age / lifeMs;
        Sprite sprite = life > dissolve2At ? set.dissolve2 : life > dissolve1At ? set.dissolve1 : set.normal;
        BannerPieces.SetSprite(image, sprite, pixelScale);
        image.rectTransform.anchoredPosition = new Vector2(px - screenCenterX, -py);
        BannerPieces.Show(image, true);
    }

    private void Release(int index)
    {
        live[index].active = false;
        BannerPieces.Show(pool[index], false);
        ActiveCount--;
    }
}
