using UnityEngine;

// Trilha contínua das hordas; o anúncio do boss encerra antes da música de Gusini.
[DisallowMultipleComponent]
public sealed class WaveMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float volume = .28f;
    [SerializeField, Min(0f)] private float fadeIn = .6f;
    [SerializeField, Min(0f)] private float fadeOut = .9f;
    private WaveController waves;
    private AudioSource source;
    private float target, fadeSpeed;

    private void Awake()
    {
        waves = GetComponent<WaveController>();
        if (music == null) music = Resources.Load<AudioClip>("Audio/Music/MUS_Crocodilo_Ondas_Loop");
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.priority = 0;
        source.ignoreListenerPause = false;
        source.clip = music;
        source.volume = 0f;
        GameAudioSettings.Route(source, AudioChannel.Music);
    }

    private void OnEnable()
    {
        if (waves == null) return;
        waves.WaveAnnounced += OnWave;
        waves.BossStarted += OnBoss;
        waves.RunEnded += OnRunEnded;
        if (waves.Phase == WaveController.RunPhase.Combat || waves.Phase == WaveController.RunPhase.Intermission || waves.Phase == WaveController.RunPhase.Announcing)
            OnWave(waves.CurrentWave, waves.WaveCount);
    }

    private void OnWave(int wave, int total)
    {
        if (waves.IsBossWave(wave))
        {
            target = 0f;
            fadeSpeed = fadeOut > 0f ? source.volume / fadeOut : float.PositiveInfinity;
            return;
        }
        if (music == null || source.isPlaying) return;
        source.volume = 0f;
        target = volume;
        fadeSpeed = fadeIn > 0f ? volume / fadeIn : float.PositiveInfinity;
        source.Play();
    }

    private void Update()
    {
        if (waves == null || !waves.isActiveAndEnabled) { Stop(); return; }
        if (AudioListener.pause || !source.isPlaying) return;
        // A escolha de cartas pode congelar o tempo; não reinicia nem interrompe a faixa.
        source.volume = Mathf.MoveTowards(source.volume, target, fadeSpeed * Time.unscaledDeltaTime);
        if (target <= 0f && source.volume <= 0f) source.Stop();
    }

    private void Stop() { if (source != null) source.Stop(); }
    private void OnBoss(Boss boss) => Stop();
    private void OnRunEnded(bool completed) => Stop();
    private void OnDisable()
    {
        if (waves != null)
        {
            waves.WaveAnnounced -= OnWave;
            waves.BossStarted -= OnBoss;
            waves.RunEnded -= OnRunEnded;
        }
        Stop();
    }
    private void OnDestroy() { if (source != null) Destroy(source); }
}
