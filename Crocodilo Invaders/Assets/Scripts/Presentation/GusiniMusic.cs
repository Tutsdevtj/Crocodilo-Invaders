using UnityEngine;
using UnityEngine.SceneManagement;

// Uma única fonte de música: F1 depois da liberação do combate, silêncio na falsa derrota, F2 no fim da transformação.
[RequireComponent(typeof(Boss))]
public sealed class GusiniMusic : MonoBehaviour
{
    public enum Track { None, PhaseOne, PhaseTwo }
    [SerializeField] private AudioClip phaseOneMusic;
    [SerializeField] private AudioClip phaseTwoMusic;
    [SerializeField, Range(0f, 1f)] private float phaseOneVolume = .28f;
    [SerializeField, Range(0f, 1f)] private float phaseTwoVolume = .24f;
    [SerializeField, Min(0f)] private float fadeIn = .6f;
    [SerializeField, Min(0f), Tooltip("Silencia a F1 ao zerar a barra, antes do quadro de falsa derrota.")] private float transitionFadeOut = .35f;
    [SerializeField, Min(0f)] private float stopFadeOut = 1.2f;

    private Boss boss;
    private GusiniPhaseTwo phaseTwo;
    private AudioSource source;
    private float target, fadeSpeed;
    private bool phaseOneStarted, phaseTwoStarted;
    public Track Current { get; private set; }
    public AudioSource Source => source;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        phaseTwo = GetComponent<GusiniPhaseTwo>();
        // Fora da hierarquia do boss: não entra no pool de dez vozes de efeitos do GusiniAudio.
        var child = new GameObject("Gusini music");
        SceneManager.MoveGameObjectToScene(child, gameObject.scene);
        source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.priority = 0;
        source.ignoreListenerPause = false;
        GameAudioSettings.Route(source, AudioChannel.Music);
    }

    private void OnEnable()
    {
        boss.BattleStopped += OnStopped;
        if (phaseTwo == null) return;
        phaseTwo.TransitionStarted += OnTransition;
        phaseTwo.PhaseTwoStarted += OnPhaseTwo;
    }

    private void OnDisable()
    {
        boss.BattleStopped -= OnStopped;
        if (phaseTwo != null)
        {
            phaseTwo.TransitionStarted -= OnTransition;
            phaseTwo.PhaseTwoStarted -= OnPhaseTwo;
        }
        if (source != null) source.Stop();
        Current = Track.None;
    }

    private void OnDestroy()
    {
        if (source != null) Destroy(source.gameObject);
    }

    private void Update()
    {
        // Liberar o combate (após o diálogo de entrada) é o que inicia a F1, uma única vez.
        if (!phaseOneStarted && boss.Ready && (phaseTwo == null || phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.PhaseOne))
        {
            phaseOneStarted = true;
            Begin(Track.PhaseOne);
        }
        if (Current == Track.None || source == null || AudioListener.pause) return;
        // A falsa derrota congela o tempo do jogo; o fade da música não fica preso nela.
        source.volume = Mathf.MoveTowards(source.volume, target, fadeSpeed * Time.unscaledDeltaTime);
        if (target <= 0f && source.volume <= 0f)
        {
            source.Stop();
            Current = Track.None;
        }
    }

    private void Begin(Track track)
    {
        AudioClip clip = track == Track.PhaseOne ? phaseOneMusic : phaseTwoMusic;
        if (clip == null || source == null) return;
        source.Stop();
        source.clip = clip;
        source.volume = 0f;
        target = track == Track.PhaseOne ? phaseOneVolume : phaseTwoVolume;
        fadeSpeed = fadeIn > 0f ? target / fadeIn : float.PositiveInfinity;
        source.Play();
        Current = track;
    }

    private void FadeOut(float duration)
    {
        // No unload a fonte (raiz própria da cena) pode sumir antes do StopBattle do boss.
        if (Current == Track.None || source == null) return;
        target = 0f;
        fadeSpeed = duration > 0f ? Mathf.Max(.01f, source.volume) / duration : float.PositiveInfinity;
    }

    private void OnTransition()
    {
        phaseOneStarted = true;
        FadeOut(transitionFadeOut);
    }

    private void OnPhaseTwo()
    {
        if (phaseTwoStarted) return;
        phaseTwoStarted = true;
        Begin(Track.PhaseTwo);
    }

    private void OnStopped() => FadeOut(stopFadeOut);
}
