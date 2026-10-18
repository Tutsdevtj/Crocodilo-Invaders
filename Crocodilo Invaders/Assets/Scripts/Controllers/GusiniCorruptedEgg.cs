using System;
using UnityEngine;

public sealed class GusiniCorruptedEgg : MonoBehaviour
{
    private GusiniPhaseTwoAssets assets;
    private SpriteRenderer egg, parachute;
    private GusiniAudio audio;
    private float age, scale;
    private Vector3 flightOrigin, burstPosition;
    private float travelDuration;
    public event Action Burst;
    private bool popped, burst;
    private int pulseSounds;
    private bool firstCrack, secondCrack;
    private static readonly float[] PulseTimes = { .2f, .55f, .82f, 1f };
    public static GusiniCorruptedEgg Create(GusiniPhaseTwoAssets assets, Transform parent, Vector3 position,
        float scale, GusiniAudio audio, int bossOrder, Vector3 burstPosition, float travelDuration)
    {
        var root = new GameObject("Indestructible corrupted egg"); root.transform.SetParent(parent, false);
        root.transform.position = position; root.transform.localScale = Vector3.one * scale * 100f;
        var egg = root.AddComponent<SpriteRenderer>(); egg.sortingOrder = bossOrder - 1; egg.sprite = assets.Get("egg_p2_idle");
        var canopy = new GameObject("Torn parachute"); canopy.transform.SetParent(root.transform, false);
        // Attach (18,25) sobre o topo do ovo (12,4): centro do paraquedas 22px acima.
        canopy.transform.localPosition = new Vector3(0f, .22f, 0f);
        var parachute = canopy.AddComponent<SpriteRenderer>(); parachute.sortingOrder = 24; parachute.enabled = false;
        var component = root.AddComponent<GusiniCorruptedEgg>();
        component.assets = assets; component.egg = egg; component.parachute = parachute; component.scale = scale; component.audio = audio;
        component.flightOrigin = position; component.burstPosition = burstPosition;
        component.travelDuration = Mathf.Max(.1f, travelDuration);
        // Sem Health nem collider atingível: os tiros do player passam pelo ovo.
        return component;
    }
    private void Update()
    {
        age += Time.deltaTime;
        if (age > .02f) egg.sortingOrder = 24;
        if (!burst)
        {
            float dropTime = Mathf.Min(age, .46f);
            Vector3 drop = new Vector3(-15f * dropTime, -70f * Mathf.Min(dropTime, .25f) - 23f * Mathf.Max(0f, dropTime - .25f), 0f) * scale;
            Vector3 deployedPosition = flightOrigin + drop;
            float flight = Mathf.Clamp01((age - .46f) / travelDuration);
            transform.position = Vector3.Lerp(deployedPosition, burstPosition, Mathf.SmoothStep(0f, 1f, flight));
        }
        if (age < .25f) return;
        parachute.enabled = !burst;
        if (!popped)
        {
            popped = true;
            GusiniPhaseTwoEffect.Create(assets.Frames("fx_parachute_pop_", 2), new[] { .06f, .07f }, transform.parent,
                transform.position + Vector3.up * 22f * scale, scale);
            if (audio != null) audio.Play(GusiniSound.P2ParachutePop, transform.position, transform);
        }
        if (age < .46f)
        {
            float deploy = age - .25f;
            parachute.sprite = assets.Get("parachute_p2_deploy_" + (deploy < .06f ? 0 : deploy < .13f ? 1 : 2));
            return;
        }
        parachute.sprite = assets.Get("parachute_p2_" + ((int)((age - .46f) / .16f) % 2));
        // O aviso de rachadura/estouro só começa após alcançar o ponto sorteado.
        float t = age - .46f - travelDuration;
        if (t < 0f) { egg.sprite = assets.Get("egg_p2_idle"); return; }
        while (pulseSounds < PulseTimes.Length && t >= PulseTimes[pulseSounds])
        {
            if (audio != null) audio.Play(GusiniSound.P2EggPulse, transform.position, transform, .85f + .05f * pulseSounds, 1f + .02f * pulseSounds);
            pulseSounds++;
        }
        if (!firstCrack && t >= 1.15f)
        {
            firstCrack = true;
            if (audio != null) audio.Play(GusiniSound.P2EggCrack, transform.position, transform);
        }
        if (!secondCrack && t >= 1.35f)
        {
            secondCrack = true;
            if (audio != null) audio.Play(GusiniSound.P2EggCrack, transform.position, transform, 1f, 1.035f);
        }
        if (t < 1.15f)
        {
            int pulse = (t >= .2f && t < .36f) || (t >= .55f && t < .68f) ? 1 :
                (t >= .82f && t < .92f) || (t >= 1f && t < 1.08f) ? 2 : 0;
            egg.sprite = assets.Get(pulse == 0 ? "egg_p2_idle" : "egg_p2_pulse_" + pulse);
        }
        else if (t < 1.35f) egg.sprite = assets.Get("egg_p2_crack_1");
        else if (t < 1.51f) egg.sprite = assets.Get("egg_p2_crack_2");
        else
        {
            if (!burst)
            {
                burst = true; parachute.enabled = false; egg.sprite = assets.Get("egg_p2_burst");
                string[] names = { "up", "up_left", "left", "down_left", "down" };
                Vector2[] directions = { Vector2.up, new Vector2(-1, 1).normalized, Vector2.left, new Vector2(-1, -1).normalized, Vector2.down };
                for (int i = 0; i < 5; i++) GusiniAcidYolk.Create(assets.Get("yolk_p2_" + names[i]), transform.parent, transform.position, scale, directions[i]);
                if (audio != null) audio.Play(GusiniSound.P2EggBurst, transform.position, transform.parent);
                Burst?.Invoke();
            }
            if (t >= 1.63f) Destroy(gameObject);
        }
    }
}
