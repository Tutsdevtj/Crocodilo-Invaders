using UnityEngine;

// Blips limitados por intervalo e vozes; revelar a fala inteira nunca dispara as letras restantes.
public sealed class DialogueBlipAudio : MonoBehaviour
{
    [SerializeField] private AudioClip blip;
    [SerializeField, Range(0f, 1f)] private float volume = .35f;
    [SerializeField, Min(1)] private int lettersPerBlip = 2;
    [SerializeField, Min(0f)] private float minimumInterval = .07f;
    [SerializeField, Range(0f, 1f), Tooltip("Pontuação toca raramente; espaços nunca.")] private float punctuationChance;
    [SerializeField, Range(0f, .2f)] private float pitchJitter = .03f;
    [SerializeField, Range(1, 4)] private int voices = 2;

    private AudioSource[] sources;
    private AudioSource cueSource;
    private int next, lettersSinceBlip;
    private float lastBlipAt = float.NegativeInfinity;
    private readonly System.Random variation = new System.Random();
    public int PlayedCount { get; private set; }
    public float Volume { get => volume; set => volume = Mathf.Clamp01(value); }

    private void Awake()
    {
        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++) sources[i] = CreateSource("Dialogue blip " + i);
        cueSource = CreateSource("Dialogue cue");
    }

    private AudioSource CreateSource(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);
        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        // Pausa do jogo também pausa a voz da transmissão.
        source.ignoreListenerPause = false;
        GameAudioSettings.Route(source, AudioChannel.Effects);
        return source;
    }

    public void BeginLine() { lettersSinceBlip = lettersPerBlip - 1; }

    public bool TryPlay(char character, AudioClip overrideClip, float pitch)
    {
        if (char.IsWhiteSpace(character) || sources == null) return false;
        bool punctuation = char.IsPunctuation(character) || char.IsSymbol(character);
        if (punctuation && (punctuationChance <= 0f || variation.NextDouble() > punctuationChance)) return false;
        if (!punctuation && ++lettersSinceBlip < lettersPerBlip) return false;
        AudioClip clip = overrideClip != null ? overrideClip : blip;
        if (clip == null || Time.unscaledTime - lastBlipAt < minimumInterval) return false;
        lettersSinceBlip = 0;
        lastBlipAt = Time.unscaledTime;
        AudioSource source = sources[next];
        next = (next + 1) % sources.Length;
        source.Stop();
        source.clip = clip;
        source.volume = volume;
        source.pitch = Mathf.Clamp(pitch, .5f, 2f) * (1f + ((float)variation.NextDouble() * 2f - 1f) * pitchJitter);
        source.Play();
        PlayedCount++;
        return true;
    }

    public void PlayCue(AudioClip clip, float cueVolume)
    {
        if (clip == null || cueSource == null) return;
        cueSource.Stop();
        cueSource.clip = clip;
        cueSource.volume = Mathf.Clamp01(cueVolume);
        cueSource.Play();
    }

    public void StopAll()
    {
        if (sources != null) foreach (AudioSource source in sources) if (source != null) source.Stop();
        if (cueSource != null) cueSource.Stop();
    }

    private void OnDisable() => StopAll();
}
