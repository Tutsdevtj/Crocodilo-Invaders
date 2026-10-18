using System;
using UnityEngine;

// Relógio único dos anúncios: "ONDA N" e, na onda do boss, transição de penas + PERIGO! <BOSS>.
// Tempo escalado (pausa congela tudo); o callback de conclusão roda uma única vez por anúncio.
// Quem decide o que vem depois continua sendo o WaveController.
public sealed class WaveAnnouncer : MonoBehaviour
{
    [SerializeField] private WaveBanner waveBanner;
    [SerializeField] private BossAlertBanner bossBanner;
    [SerializeField, Min(1f), Tooltip("Uma nova onda antes do fim: a saída restante é comprimida para este tempo.")]
    private float compressedExitMs = 120f;
    [SerializeField, Min(1f), Tooltip("Diferença entre animação e áudio que dispara a correção (ex.: travada de frame).")]
    private float soundResyncMs = 50f;

    private Action completed;
    private BossAlertTheme theme;
    private int pendingWave;
    private BossAlertTheme pendingTheme;
    private Action pendingCompleted;
    private bool hasPending;
    private float clock, bossStart, rate = 1f, transitionClock = -1f;
    private bool bossSkipped, skipped;

    public bool IsPlaying { get; private set; }
    public float Clock => clock;
    public bool HasBoss => theme != null;
    public float WaveDuration => waveBanner.Duration;
    public float BossStart => bossStart;
    public float Duration => HasBoss ? bossStart + bossBanner.Duration : WaveDuration;
    public WaveBanner Wave => waveBanner;
    public BossAlertBanner Boss => bossBanner;
    public int WaveNumber { get; private set; }

    private void OnDisable() => Cancel();

    // theme nulo: só o anúncio da onda. Com tema: ONDA N (1050) → penas (150) → banner do boss (1300).
    public void Play(int wave, BossAlertTheme bossTheme, Action onCompleted)
    {
        if (IsPlaying)
        {
            // Nunca dois anúncios juntos: o atual sai depressa e o novo começa em seguida.
            CompressExit();
            pendingWave = wave; pendingTheme = bossTheme; pendingCompleted = onCompleted; hasPending = true;
            completed = null;
            return;
        }
        Begin(wave, bossTheme, onCompleted);
    }

    // Regras do guia §6: a saída sempre toca; partículas vivas terminam a vida e não nascem novas.
    public void Skip()
    {
        if (!IsPlaying) return;
        skipped = true;
        if (clock < WaveDuration)
        {
            clock = Mathf.Max(clock, waveBanner.Style.skipToMs);
            if (!HasBoss) return;
            // Pular a sequência inteira: depois da saída da onda, o boss aparece já em t = 1050 (só a saída).
            bossSkipped = true;
            bossStart = WaveDuration - bossBanner.SkipToMs;
            bossBanner.SuppressNewEffects();
            bossBanner.FadeOutSound((WaveDuration - clock + bossBanner.Duration - bossBanner.SkipToMs) / 1000f);
            return;
        }
        if (!HasBoss) return;
        float local = clock - bossStart;
        if (local < bossBanner.SkipToMs) bossStart = clock - bossBanner.SkipToMs;
        bossBanner.SuppressNewEffects();
        bossBanner.FadeOutSound((bossBanner.Duration - bossBanner.SkipToMs) / 1000f);
    }

    // Cancelamento (fim de run, disable, troca de cena): limpa tudo e descarta o callback.
    public void Cancel()
    {
        completed = null;
        hasPending = false;
        pendingCompleted = null;
        IsPlaying = false;
        theme = null;
        if (waveBanner != null) waveBanner.Hide();
        if (bossBanner != null) bossBanner.Hide(true);
    }

#if UNITY_EDITOR
    // Somente validação/capturas: posiciona a linha do tempo num instante exato.
    public void SeekForValidation(float ms)
    {
        if (!IsPlaying) return;
        float delta = Mathf.Max(0f, ms - clock);
        clock = ms;
        Evaluate(delta);
    }
#endif

    private void Begin(int wave, BossAlertTheme bossTheme, Action onCompleted)
    {
        WaveNumber = wave;
        theme = bossTheme;
        completed = onCompleted;
        clock = 0f;
        rate = 1f;
        bossSkipped = skipped = false;
        transitionClock = -1f;
        IsPlaying = true;
        waveBanner.Prepare(wave);
        if (HasBoss)
        {
            bossStart = WaveDuration + bossBanner.TransitionMs;
            bossBanner.Prepare(theme);
        }
        else bossBanner.Hide(false);
        Evaluate(0f);
    }

    private void Update()
    {
        float delta = Time.deltaTime * 1000f;
        if (!IsPlaying)
        {
            // A cauda das penas continua depois da conclusão, sem segurar o WaveController.
            if (bossBanner != null) bossBanner.AdvanceParticles(delta, 0f);
            return;
        }
        if (delta <= 0f) return;
        clock += delta * rate;
        ResyncWithSound();
        Evaluate(delta);
    }

    private void Evaluate(float deltaMs)
    {
        if (clock < WaveDuration) waveBanner.Evaluate(clock);
        else if (waveBanner.Visible) waveBanner.Hide();

        if (HasBoss)
        {
            float local = clock - bossStart;
            bool started = bossSkipped ? clock >= WaveDuration : true;
            // Primeiro envelhece as penas vivas; as que nascem agora já recebem a idade exata.
            bossBanner.AdvanceParticles(deltaMs, local);
            if (started) bossBanner.Evaluate(local);
            if (!bossSkipped && clock >= WaveDuration)
            {
                transitionClock = transitionClock < 0f ? clock - WaveDuration : transitionClock + deltaMs;
                if (transitionClock < bossBanner.TransitionVisibleMs) bossBanner.EvaluateTransition(transitionClock);
                else bossBanner.HideTransition();
            }
        }

        if (clock >= Duration) Finish();
    }

    // O áudio corre no relógio DSP; uma travada longa de frame não pode separar o slam visual do sonoro.
    private void ResyncWithSound()
    {
        if (!HasBoss || skipped || !bossBanner.TryGetSoundClock(out float soundMs)) return;
        float local = clock - bossStart;
        if (Mathf.Abs(soundMs - local) > soundResyncMs) clock = bossStart + soundMs;
    }

    private void CompressExit()
    {
        Skip();
        float remaining = Duration - clock;
        rate = Mathf.Max(1f, remaining / compressedExitMs);
    }

    private void Finish()
    {
        IsPlaying = false;
        waveBanner.Hide();
        // Fim normal: banner fora da tela; a cauda atenuada do WAV e as penas vivas terminam sozinhas.
        bossBanner.Hide(false);
        Action callback = completed;
        completed = null;
        if (hasPending)
        {
            hasPending = false;
            Action next = pendingCompleted;
            pendingCompleted = null;
            Begin(pendingWave, pendingTheme, next);
        }
        callback?.Invoke();
    }
}
