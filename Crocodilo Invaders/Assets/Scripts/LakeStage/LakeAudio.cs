using UnityEngine;

public enum LakeSound { Spit, SplashSmall, SplashBig, TongueLash, Gulp, Burp, BossJump, BossLand, TireThrow, TireBounce, EnemyHit, EnemyDie, Slap, BossHit, Croak, FrogCroak, FrogDie, SapererecaCroak, SapeneuGrunt, FlyBuzz, DragonflyDash, BeetleChitter, BeetleShell, BeetleDie, PiranhaChomp }

// One limited voice pool for the whole stage, so many drops and enemies never stack into noise.
public sealed class LakeAudio : MonoBehaviour
{
    private const int Voices = 10;
    private const float AmbienceFade = .35f;
    private static LakeAudio current;
    private LakeAudioBank bank;
    private AudioSource[] voices;
    private AudioSource ambience, mosquitoSwarm;
    private int mosquitoes;
    private const int SwarmFull = 3;
    private float[] nextAllowed;
    private int cursor;
    public static int Played { get; private set; }

    private void Awake()
    {
        current = this;
        bank = GetComponent<LakeBackdrop>().Art.audio;
        if (bank == null)
        { enabled = false; return; }
        nextAllowed = new float[bank.sounds.Length];
        voices = new AudioSource[Voices];
        for (int i = 0; i < Voices; i++)
            voices[i] = NewSource();
        ambience = NewSource();
        ambience.clip = bank.ambience;
        ambience.loop = true;
        ambience.volume = 0;
        ambience.Play();
        // One shared buzz for the whole swarm, never one loop per mosquito.
        mosquitoSwarm = NewSource();
        mosquitoSwarm.clip = bank.mosquitoBuzz;
        mosquitoSwarm.loop = true;
        mosquitoSwarm.volume = 0;
        mosquitoSwarm.Play();
    }
    private AudioSource NewSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        GameAudioSettings.Route(source, AudioChannel.Effects);
        return source;
    }
    private void Update()
    {
        if (ambience != null)
            ambience.volume = Mathf.MoveTowards(ambience.volume, bank.ambienceVolume, Time.unscaledDeltaTime * AmbienceFade);
        if (mosquitoSwarm != null)
            mosquitoSwarm.volume = Mathf.MoveTowards(mosquitoSwarm.volume, bank.mosquitoBuzzVolume * Mathf.Min(mosquitoes, SwarmFull) / SwarmFull, Time.unscaledDeltaTime * .6f);
    }
    private void OnDestroy()
    {
        if (current == this)
            current = null;
    }
    public static int SwarmSize => current != null ? current.mosquitoes : 0;
    public static void AddMosquito(int delta)
    {
        if (current != null)
            current.mosquitoes = Mathf.Max(0, current.mosquitoes + delta);
    }
    public static void Play(LakeSound sound)
    {
        if (current == null || !current.enabled)
            return;
        current.Emit((int)sound);
    }
    private void Emit(int index)
    {
        if (Time.time < nextAllowed[index])
            return;
        var entry = bank.sounds[index];
        nextAllowed[index] = Time.time + entry.cooldown;
        var voice = voices[cursor];
        cursor = (cursor + 1) % Voices;
        voice.pitch = 1 + Random.Range(-entry.pitchVariance, entry.pitchVariance);
        voice.PlayOneShot(entry.clip, entry.volume);
        Played++;
    }
}
