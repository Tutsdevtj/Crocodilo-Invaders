using UnityEngine;

// Duas pequenas duplas no alto do céu. Tudo é puramente visual e reutilizado.
public sealed class DistantAirBattle : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] ships, shots;
    [SerializeField] private SpriteRenderer explosion, flak;
    [SerializeField] private Sprite[] explosionFrames, flakFrames;
    [SerializeField, Min(0f)] private float parallaxSpeed = .38f;
    private readonly float[] speed = { .45f, .3f, -.5f, -.35f };
    private readonly float[] heights = { 1.65f, 2.65f, 1.8f, 2.5f };
    private float clock;

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        clock += dt;
        float localPerWorldUnit = 1f / Mathf.Max(.01f, transform.lossyScale.x);
        for (int i = 0; i < ships.Length; i++)
        {
            Vector3 p = ships[i].transform.localPosition;
            p.x = Mathf.Repeat(p.x + (speed[i] - parallaxSpeed) * localPerWorldUnit * dt + 8f, 16f) - 8f;
            p.y = heights[i] + Mathf.Sin(clock * .65f + i) * .06f;
            ships[i].transform.localPosition = p;
        }
        for (int i = 0; i < shots.Length; i++)
        {
            float phase = Mathf.Repeat(clock + i * .6f, 1.8f);
            shots[i].enabled = phase < .35f;
            var ship = ships[i == 0 ? 0 : 2];
            float direction = i == 0 ? 1f : -1f;
            shots[i].transform.localPosition = ship.transform.localPosition + new Vector3(direction * (.15f + phase * 1.4f), 0f, 0f);
        }
        float effectAge = Mathf.Repeat(clock, 5.1f);
        explosion.enabled = effectAge < explosionFrames.Length * .1f;
        if (explosion.enabled)
        {
            explosion.transform.localPosition = new Vector2(-1.6f, 2.45f);
            explosion.sprite = explosionFrames[(int)(effectAge / .1f)];
        }
        effectAge = Mathf.Repeat(clock + 2.4f, 6.3f);
        flak.enabled = effectAge < flakFrames.Length * .15f;
        if (flak.enabled)
        {
            flak.transform.localPosition = new Vector2(3.1f, 1.9f);
            flak.sprite = flakFrames[(int)(effectAge / .15f)];
        }
    }
}
