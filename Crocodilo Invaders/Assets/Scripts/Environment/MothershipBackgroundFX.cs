using UnityEngine;

// Ambientação distante: pool fixo, sem colisão, dano, áudio ou sorteios do gameplay.
public sealed class MothershipBackgroundFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] smoke, glitches, debris;
    [SerializeField] private SpriteRenderer explosion, flash;
    [SerializeField] private Sprite[] smokeFrames, explosionFrames, flashFrames, glitchFrames, debrisFrames;
    [SerializeField] private Vector2[] damagePoints;
    private readonly System.Random visualRandom = new System.Random();
    private readonly Vector2[] debrisVelocity = new Vector2[6];
    private readonly float[] debrisAge = new float[6], glitchStarted = { float.NegativeInfinity, float.NegativeInfinity };
    private float clock, nextExplosion, nextGlitch, explosionStarted = float.NegativeInfinity;

    private void Awake()
    {
        nextExplosion = Range(1.2f, 2.5f); nextGlitch = Range(.8f, 1.5f);
        Animate(0f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        clock += dt;
        if (clock >= nextExplosion)
        {
            Vector2 point = damagePoints[visualRandom.Next(damagePoints.Length)];
            explosion.transform.localPosition = flash.transform.localPosition = point;
            explosionStarted = clock; nextExplosion = clock + Range(1.2f, 2.5f);
            int count = visualRandom.Next(2, 4);
            for (int i = 0; i < debris.Length && count > 0; i++)
            {
                if (debris[i].enabled) continue;
                debris[i].transform.localPosition = point; debris[i].enabled = true;
                debrisAge[i] = 0f; debrisVelocity[i] = new Vector2(Range(-.25f, .25f), Range(.06f, .14f)); count--;
            }
        }
        if (clock >= nextGlitch)
        {
            glitchStarted[visualRandom.Next(glitches.Length)] = clock;
            nextGlitch = clock + Range(.8f, 1.5f);
        }
        Animate(dt);
    }

    private void Animate(float dt)
    {
        for (int i = 0; i < smoke.Length; i++) smoke[i].sprite = smokeFrames[((int)(clock / .11f) + i * 3) % smokeFrames.Length];
        Frame(explosion, explosionFrames, clock - explosionStarted, .1f);
        Frame(flash, flashFrames, clock - explosionStarted, .05f);
        for (int i = 0; i < glitches.Length; i++) Frame(glitches[i], glitchFrames, clock - glitchStarted[i], .07f);
        for (int i = 0; i < debris.Length; i++)
        {
            if (!debris[i].enabled) continue;
            debrisAge[i] += dt;
            if (debrisAge[i] >= 1.2f) { debris[i].enabled = false; continue; }
            debrisVelocity[i] += Vector2.down * (.9f * dt);
            debris[i].transform.localPosition += (Vector3)(debrisVelocity[i] * dt);
            debris[i].sprite = debrisFrames[(int)(debrisAge[i] / .08f) % debrisFrames.Length];
        }
    }

    private static void Frame(SpriteRenderer renderer, Sprite[] frames, float age, float duration)
    {
        renderer.enabled = age >= 0f && age < frames.Length * duration;
        if (renderer.enabled) renderer.sprite = frames[(int)(age / duration)];
    }
    private float Range(float min, float max) => Mathf.Lerp(min, max, (float)visualRandom.NextDouble());
}
