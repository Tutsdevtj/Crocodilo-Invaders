using System;
using UnityEngine;
using UnityEngine.UI;

// Apresentação genérica "PERIGO! <BOSS>" (guia §4): faixa, aviso, impacto e speedlines são comuns;
// título, anexos, partícula e som vêm do BossAlertTheme. O tempo vem do WaveAnnouncer.
public sealed class BossAlertBanner : MonoBehaviour
{
    public enum AlertState { Hidden, Red, Amber, Flash }

    [Serializable]
    public struct AlertStep
    {
        public float startMs;
        public AlertState state;
        public AlertStep(float startMs, AlertState state) { this.startMs = startMs; this.state = state; }
    }

    [Header("Objetos")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Image speedlines;
    [SerializeField] private Image trail;
    [SerializeField] private Image bandHead;
    [SerializeField] private Image bandMid;
    [SerializeField] private Image bandEnd;
    [SerializeField] private Image front;
    [SerializeField] private Image top;
    [SerializeField] private Image title;
    [SerializeField] private Image alert;
    [SerializeField] private Image impact;
    [SerializeField] private Image[] transitionFeathers = new Image[4];
    [SerializeField] private BannerParticleLayer particles;
    [SerializeField] private AudioSource alertAudio;

    [Header("Peças genéricas")]
    [SerializeField] private Sprite defaultBandHead;
    [SerializeField] private Sprite defaultBandMid;
    [SerializeField] private Sprite defaultBandEnd;
    [SerializeField] private Sprite alertRed;
    [SerializeField] private Sprite alertAmber;
    [SerializeField] private Sprite alertFlash;
    [SerializeField] private Sprite impactBurst;
    [SerializeField, Tooltip("s_0, s_1, m_0, m_1, l_0, l_1")] private Sprite[] speedlineFrames = new Sprite[6];

    [Header("Layout (px nativos; canvas do banner 340 × 120)")]
    [SerializeField] private int pixelScale = 2;
    [SerializeField] private Vector2Int canvasSize = new Vector2Int(340, 120);
    [SerializeField] private float screenCenterX = 600f, screenCenterY = 228f;
    [SerializeField] private Vector2Int bandPosition = new Vector2Int(30, 40);
    [SerializeField, Tooltip("Colunas da ponta (0–60) e largura do fim rasgado.")] private int bandHeadWidth = 61, bandEndWidth = 143;
    [SerializeField, Tooltip("Largura total da faixa = comprimento + este excesso (inclinação + dentes).")] private int bandOverhang = 33;
    [SerializeField] private Vector2Int alertPosition = new Vector2Int(56, 14);
    [SerializeField] private Vector2Int impactPosition = new Vector2Int(40, 0);
    [SerializeField] private Vector2Int speedlinesPosition = new Vector2Int(300, 20);

    [Header("Tempo (ms)")]
    [SerializeField] private BannerKeyframe[] motion =
    {
        new BannerKeyframe(0, 200, 820, 28, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(200, 260, 28, -10, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(260, 320, -10, 0, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(320, 1050, 0, -20, BannerKeyframe.Ease.Linear),
        new BannerKeyframe(1050, 1090, -20, -12, BannerKeyframe.Ease.OutCubic),
        new BannerKeyframe(1090, 1300, -12, -1100, BannerKeyframe.Ease.InCubic),
    };
    [SerializeField] private AlertStep[] alertTimeline =
    {
        new AlertStep(0, AlertState.Hidden), new AlertStep(200, AlertState.Flash), new AlertStep(233, AlertState.Amber),
        new AlertStep(266, AlertState.Red), new AlertStep(420, AlertState.Amber), new AlertStep(480, AlertState.Red),
        new AlertStep(540, AlertState.Amber), new AlertStep(600, AlertState.Red),
    };
    [SerializeField] private Vector2 titleFlashMs = new Vector2(200, 233);
    [SerializeField] private Vector2 impactMs = new Vector2(200, 300);
    [SerializeField, Tooltip("Pular antes deste ponto salta para ele: a saída sempre toca.")] private float skipToMs = 1050f;
    [SerializeField] private float speedlinesMaxLength = 140f;

    [Header("Penas do banner")]
    [SerializeField, Tooltip("Entrada: início, fim (exclusivo) e intervalo.")] private Vector3 entrySpawns = new Vector3(0, 200, 33);
    [SerializeField, Tooltip("Saída: início, fim (exclusivo) e intervalo.")] private Vector3 exitSpawns = new Vector3(1090, 1300, 30);
    [SerializeField] private int exitPuffCount = 4;
    [SerializeField, Tooltip("Penas à esquerda de X_banner + margem não aparecem.")] private float hideMargin = 60f;

    [Header("Transição antes do banner (ms relativos ao início da transição)")]
    [SerializeField] private float transitionMs = 150f;
    [SerializeField] private float[] transitionY = { 190, 262, 150, 310 };
    [SerializeField] private float[] transitionDelay = { 0, 30, 55, 75 };
    [SerializeField] private float transitionTravelMs = 140f, transitionFrameMs = 40f, transitionTailMs = 80f;
    [SerializeField] private Vector3 transitionPath = new Vector3(1260, 1400, 120);

    private const int Spawns = 32;
    private readonly float[] spawnMs = new float[Spawns];
    private readonly int[] spawnVariant = new int[Spawns];
    private readonly Vector2[] spawnOffset = new Vector2[Spawns];
    private readonly BannerSpeedlines lines = new BannerSpeedlines();
    private BossAlertTheme theme;
    private int spawnCount, nextSpawn;
    private float soundFadeSpeed;
    private bool spawnsAllowed, soundAllowed, soundStarted, visible;

    public BossAlertTheme Theme => theme;
    public RectTransform Root => root;
    public Image Title => title;
    public Image Alert => alert;
    public Image Impact => impact;
    public Image Trail => trail;
    public Image Speedlines => speedlines;
    public Image TransitionFeather(int index) => transitionFeathers[index];
    public BannerParticleLayer Particles => particles;
    public AudioSource AlertAudio => alertAudio;
    public float Duration => BannerMotion.Duration(motion);
    public float SkipToMs => skipToMs;
    public float TransitionMs => transitionMs;
    public float TransitionVisibleMs => transitionMs + transitionTailMs;
    public bool Visible => visible;
    public int SoundsPlayed { get; private set; }
    public AlertState CurrentAlert { get; private set; }

    private void Awake()
    {
        if (alertAudio != null)
        {
            alertAudio.playOnAwake = false;
            alertAudio.loop = false;
            alertAudio.spatialBlend = 0f;
            alertAudio.ignoreListenerPause = false;
            GameAudioSettings.Route(alertAudio, AudioChannel.Effects);
        }
        Hide(true);
    }

    private void Update()
    {
        // Fim suave do som somente quando o alerta é pulado; nunca no fluxo normal.
        if (soundFadeSpeed <= 0f || alertAudio == null || !alertAudio.isPlaying || AudioListener.pause) return;
        alertAudio.volume = Mathf.MoveTowards(alertAudio.volume, 0f, soundFadeSpeed * Time.deltaTime);
        if (alertAudio.volume <= 0f) { alertAudio.Stop(); soundFadeSpeed = 0f; }
    }

    public void Prepare(BossAlertTheme next)
    {
        theme = next;
        int scale = pixelScale;
        Sprite head = theme.bandHead != null ? theme.bandHead : defaultBandHead;
        Sprite mid = theme.bandMid != null ? theme.bandMid : defaultBandMid;
        Sprite end = theme.bandEnd != null ? theme.bandEnd : defaultBandEnd;
        int endX = theme.bandLength + bandOverhang - bandEndWidth;
        int midWidth = endX - bandHeadWidth;
        BannerPieces.SetSprite(bandHead, head, scale);
        BannerPieces.Place(bandHead, bandPosition.x, bandPosition.y, scale);
        BannerPieces.SetSprite(bandMid, mid, scale);
        bandMid.rectTransform.sizeDelta = new Vector2(Mathf.Max(0, midWidth) * scale, mid.rect.height * scale);
        BannerPieces.Place(bandMid, bandPosition.x + bandHeadWidth, bandPosition.y, scale);
        BannerPieces.Show(bandMid, midWidth > 0);
        BannerPieces.SetSprite(bandEnd, end, scale);
        BannerPieces.Place(bandEnd, bandPosition.x + endX, bandPosition.y, scale);
        BannerPieces.Show(bandHead, true);
        BannerPieces.Show(bandEnd, true);
        Optional(front, theme.frontAttachment, theme.frontPosition);
        Optional(top, theme.topAccessory, theme.topPosition);
        bool hasTrail = theme.trailFrames != null && theme.trailFrames.Length > 0;
        Optional(trail, hasTrail ? theme.trailFrames[0] : null,
            new Vector2Int(bandPosition.x + theme.bandLength + theme.trailOffset.x, bandPosition.y + theme.trailOffset.y));
        Optional(title, theme.title, theme.titlePosition);
        BannerPieces.SetSprite(alert, alertRed, scale);
        BannerPieces.Place(alert, alertPosition.x, alertPosition.y, scale);
        BannerPieces.SetSprite(impact, impactBurst, scale);
        BannerPieces.Place(impact, impactPosition.x, impactPosition.y, scale);
        lines.image = speedlines;
        lines.frames = speedlineFrames;
        lines.maxLength = speedlinesMaxLength;
        lines.Hide();
        particles.Use(theme.particles);
        BuildSpawns();
        nextSpawn = 0;

        spawnsAllowed = soundAllowed = true;
        soundStarted = false;
        soundFadeSpeed = 0f;
        SoundsPlayed = 0;
        CurrentAlert = AlertState.Hidden;
        foreach (Image feather in transitionFeathers) BannerPieces.Show(feather, false);
    }

    // Após um pulo: as penas vivas terminam a vida, mas não nascem novas; o som não começa mais.
    public void SuppressNewEffects()
    {
        spawnsAllowed = false;
        soundAllowed = false;
        nextSpawn = spawnCount;
    }

    // Som já em curso num pulo: termina sem corte seco junto com a saída.
    public void FadeOutSound(float seconds)
    {
        if (alertAudio == null || !alertAudio.isPlaying) return;
        soundFadeSpeed = alertAudio.volume / Mathf.Max(.01f, seconds);
    }

    // ms = tempo local do banner (negativo durante a transição de penas que o antecede).
    public void Evaluate(float ms)
    {
        if (theme == null) return;
        TryStartSound(ms);
        if (ms < 0f || ms >= Duration) { SetVisible(false); return; }
        SetVisible(true);
        int scale = pixelScale;
        int x = BannerX(ms);
        root.anchoredPosition = new Vector2(x - screenCenterX, -(screenCenterY - canvasSize.y));
        speedlines.rectTransform.anchoredPosition = new Vector2(x - screenCenterX + speedlinesPosition.x * scale,
            -(screenCenterY - canvasSize.y) - speedlinesPosition.y * scale);
        lines.Apply(BannerMotion.Velocity(motion, ms), ms, scale);

        bool flash = ms >= titleFlashMs.x && ms < titleFlashMs.y && theme.titleFlash != null;
        BannerPieces.SetSprite(title, flash ? theme.titleFlash : theme.title, scale);
        BannerPieces.Show(impact, ms >= impactMs.x && ms < impactMs.y);
        SetAlert(StateAt(ms));
        if (theme.trailFrames != null && theme.trailFrames.Length > 0)
            BannerPieces.SetSprite(trail, theme.trailFrames[Mathf.FloorToInt(ms / theme.trailFrameMs) % theme.trailFrames.Length], scale);

        // Penas que "ficam para trás": nascem ao cruzar o instante previsto, com a idade já decorrida.
        while (spawnsAllowed && nextSpawn < spawnCount && spawnMs[nextSpawn] <= ms)
        {
            float at = spawnMs[nextSpawn];
            particles.Spawn(BannerX(at) + spawnOffset[nextSpawn].x, screenCenterY - canvasSize.y + spawnOffset[nextSpawn].y,
                spawnVariant[nextSpawn], ms - at, x + hideMargin);
            nextSpawn++;
        }
    }

    // Avança as partículas pelo tempo real escalado, independente de pulos na linha do tempo.
    public void AdvanceParticles(float deltaMs, float bannerMs)
    {
        float hide = visible ? BannerX(bannerMs) + hideMargin : float.NegativeInfinity;
        particles.Advance(deltaMs, hide);
    }

    // Quatro penas atravessam a tela (in_cubic) entre a saída da onda e a entrada do banner.
    public void EvaluateTransition(float ms)
    {
        bool enabledForTheme = theme != null && theme.transitionParticles && theme.particles != null && theme.particles.Length > 0;
        for (int k = 0; k < transitionFeathers.Length; k++)
        {
            Image image = transitionFeathers[k];
            float u = (ms - transitionDelay[k]) / transitionTravelMs;
            bool show = enabledForTheme && ms >= 0f && ms < TransitionVisibleMs && u >= 0f && u <= 1f;
            BannerPieces.Show(image, show);
            if (!show) continue;
            int x = Mathf.FloorToInt((int)(transitionPath.x - transitionPath.y * u * u * u - transitionPath.z * u) / 2f) * 2;
            Sprite sprite = theme.particles[(k + Mathf.FloorToInt(ms / transitionFrameMs)) % theme.particles.Length].normal;
            BannerPieces.SetSprite(image, sprite, pixelScale);
            image.rectTransform.anchoredPosition = new Vector2(x - screenCenterX, -transitionY[k]);
        }
    }

    public void HideTransition()
    {
        foreach (Image feather in transitionFeathers) BannerPieces.Show(feather, false);
    }

    // stopSound: cancelamento (troca de cena, fim de run). No fim normal a cauda atenuada continua.
    public void Hide(bool stopSound)
    {
        SetVisible(false);
        HideTransition();
        lines.Hide();
        if (!stopSound) return;
        particles.Clear();
        if (alertAudio != null) alertAudio.Stop();
        soundFadeSpeed = 0f;
    }

    public int BannerX(float ms) => BannerMotion.EvenFloor(screenCenterX - canvasSize.x + BannerMotion.Evaluate(motion, ms));

    // Relógio do áudio no tempo local do banner; o diretor usa para corrigir deriva após travadas.
    public bool TryGetSoundClock(out float ms)
    {
        ms = 0f;
        if (!soundStarted || alertAudio == null || !alertAudio.isPlaying || alertAudio.clip != theme.alertSound) return false;
        ms = theme.alertSoundOffsetMs + alertAudio.timeSamples * 1000f / alertAudio.clip.frequency;
        return true;
    }

    public AlertState StateAt(float ms)
    {
        AlertState state = AlertState.Hidden;
        for (int i = 0; i < alertTimeline.Length && alertTimeline[i].startMs <= ms; i++) state = alertTimeline[i].state;
        return state;
    }

    private void TryStartSound(float ms)
    {
        if (soundStarted || !soundAllowed || theme.alertSound == null || alertAudio == null || ms < theme.alertSoundOffsetMs) return;
        soundStarted = true;
        float late = (ms - theme.alertSoundOffsetMs) / 1000f;
        if (late >= theme.alertSound.length) return;
        alertAudio.clip = theme.alertSound;
        alertAudio.volume = theme.alertVolume;
        alertAudio.Play();
        // O quadro raramente cai exatamente no instante: o áudio começa no ponto correspondente, sem pitch/stretch.
        if (late > 0f) alertAudio.timeSamples = Mathf.Min(theme.alertSound.samples - 1, Mathf.RoundToInt(late * theme.alertSound.frequency));
        SoundsPlayed++;
    }

    private void SetAlert(AlertState state)
    {
        CurrentAlert = state;
        BannerPieces.Show(alert, state != AlertState.Hidden);
        if (state == AlertState.Hidden) return;
        BannerPieces.SetSprite(alert, state == AlertState.Flash ? alertFlash : state == AlertState.Amber ? alertAmber : alertRed, pixelScale);
    }

    private void SetVisible(bool value)
    {
        if (visible == value && root.gameObject.activeSelf == value) return;
        visible = value;
        root.gameObject.SetActive(value);
        if (!value) lines.Hide();
    }

    private void Optional(Image image, Sprite sprite, Vector2Int position)
    {
        BannerPieces.Show(image, sprite != null);
        if (sprite == null) return;
        BannerPieces.SetSprite(image, sprite, pixelScale);
        BannerPieces.Place(image, position.x, position.y, pixelScale);
    }

    // Mesma tabela das previews (anim_all.boss_particles): posições pseudo-aleatórias fixas.
    private void BuildSpawns()
    {
        spawnCount = 0;
        int k = 0;
        for (float t = entrySpawns.x; t < entrySpawns.y && spawnCount < Spawns; t += entrySpawns.z, k++)
            AddSpawn(t, k, 300 + k * 37 % 120, 40 + k * 29 % 70);
        k = 0;
        for (float t = exitSpawns.x; t < exitSpawns.y && spawnCount < Spawns; t += exitSpawns.z, k++)
        {
            if (k == 0)
                for (int puff = 0; puff < exitPuffCount && spawnCount < Spawns; puff++)
                    AddSpawn(exitSpawns.x, puff + 30, 380 + puff * 70, 30 + puff * 23 % 60);
            AddSpawn(t, k + 10, 140 + k * 53 % 420, 20 + k * 31 % 80);
        }
    }

    // lx em px de tela a partir do X do banner; ly em px nativos (×2), mais o escalonamento k % 3.
    private void AddSpawn(float at, int variant, int lx, int ly)
    {
        spawnMs[spawnCount] = at;
        spawnVariant[spawnCount] = variant;
        spawnOffset[spawnCount] = new Vector2(lx, ly * pixelScale + variant % 3 * 6);
        spawnCount++;
    }
}
