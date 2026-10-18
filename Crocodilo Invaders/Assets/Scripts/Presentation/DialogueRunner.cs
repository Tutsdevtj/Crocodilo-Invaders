using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Caixa modal "Rádio de Piloto". Usa tempo não escalado e congela junto com o menu de pausa.
[RequireComponent(typeof(DialogueBlipAudio))]
public sealed class DialogueRunner : MonoBehaviour
{
    public enum SkipMode { Disabled, Press, Hold }
    private enum RadioState { Idle, Static, Click, Transmitting, Ended }

    [Header("Estrutura")]
    [SerializeField] private GameObject content;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panelMotion;
    [SerializeField] private TMP_Text body;
    [SerializeField] private RectTransform nameplate;
    [SerializeField] private Image nameplateAccent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image radio;
    [SerializeField] private Image continueIcon;
    [SerializeField] private Image highlight;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private TMP_Text skipHint;
    [Header("Sprites")]
    [SerializeField] private Sprite radioIdle;
    [SerializeField] private Sprite[] radioActive = new Sprite[3];
    [SerializeField] private Sprite[] continueFrames = new Sprite[3];
    [Header("Digitação")]
    [SerializeField, Min(.005f)] private float characterDelay = .035f;
    [SerializeField, Min(0f)] private float sentencePause = .12f;
    [SerializeField, Min(0f)] private float commaPause = .06f;
    [SerializeField, Min(0f)] private float textDelay = .08f;
    [Header("Apresentação")]
    [SerializeField, Min(.01f)] private float entranceDuration = .24f;
    [SerializeField, Min(.01f)] private float exitDuration = .14f;
    [SerializeField, Min(.01f)] private float portraitEntrance = .2f;
    [SerializeField, Min(.01f)] private float highlightDuration = .25f;
    [SerializeField, Min(.01f)] private float interferenceDuration = .48f;
    [SerializeField] private Color inactivePortrait = new Color(.46f, .48f, .54f, 1f);
    [SerializeField, Min(0f)] private float plateInset = 24f;
    [Header("Controles")]
    [SerializeField] private SkipMode skipMode = SkipMode.Hold;
    [SerializeField, Min(0f)] private float skipHoldDuration = .5f;
    [SerializeField] private string skipLabel = "SEGURE TAB: PULAR";
    [SerializeField] private string skippingLabel = "PULANDO...";
    [SerializeField, Tooltip("{0} recebe o rótulo; mark escurece o fundo sem arte extra.")] private string skipFormat = "<mark=#12161CC8> {0} </mark>";
    [SerializeField] private PauseController pause;

    private const float RadioFrame = .12f, ContinueFrame = .14f, ClickDuration = .08f;
    private static readonly int[] LoopFrames = { 0, 1, 2, 1 };
    private DialogueBlipAudio blips;
    private InputAction advanceAction, skipAction;
    private DialogueSequence sequence;
    private Action finished;
    private Coroutine routine;
    private RadioState radioState;
    private float radioClock, continueClock, highlightClock = 99f, popClock = 99f, skipHeld;
    private int ignoreInputUntilFrame = -1;
    private bool advanceRequested, leftShown, rightShown;
    private DialogueCast.Speaker currentSpeaker;
    private Vector2 panelHome, leftHome, rightHome;
    private TMP_FontAsset nameFont;
    private float nameFontSize;
    private TextAlignmentOptions nameAlignment;

    public bool IsPlaying { get; private set; }
    public DialogueSequence Sequence => sequence;
    public int LineIndex { get; private set; } = -1;
    public bool AwaitingAdvance { get; private set; }
    public int VisibleCharacters { get; private set; }
    public int TotalCharacters { get; private set; }
    public Image LeftPortrait => leftPortrait;
    public Image RightPortrait => rightPortrait;
    public TMP_Text NameText => nameText;
    public TMP_Text Body => body;
    public DialogueBlipAudio Blips => blips;
    public event Action<int, DialogueSequence.Line> LineStarted;
    private bool Paused => pause != null && pause.IsPaused;

    private void Awake()
    {
        blips = GetComponent<DialogueBlipAudio>();
        nameFont = nameText.font;
        nameFontSize = nameText.fontSize;
        nameAlignment = nameText.alignment;
        advanceAction = new InputAction("DialogueAdvance", InputActionType.Button);
        advanceAction.AddBinding("<Keyboard>/enter");
        advanceAction.AddBinding("<Keyboard>/numpadEnter");
        advanceAction.AddBinding("<Mouse>/leftButton");
        advanceAction.AddBinding("<Gamepad>/buttonSouth");
        skipAction = new InputAction("DialogueSkip", InputActionType.Button);
        skipAction.AddBinding("<Keyboard>/tab");
        skipAction.AddBinding("<Gamepad>/select");
        panelHome = panelMotion.anchoredPosition;
        leftHome = leftPortrait.rectTransform.anchoredPosition;
        rightHome = rightPortrait.rectTransform.anchoredPosition;
        if (skipHint != null) skipHint.gameObject.SetActive(skipMode != SkipMode.Disabled);
        Hide();
    }

    private void OnEnable()
    {
        advanceAction.Enable(); skipAction.Enable();
        if (pause != null) pause.PauseChanged += OnPauseChanged;
    }

    private void OnDisable()
    {
        advanceAction.Disable(); skipAction.Disable();
        if (pause != null) pause.PauseChanged -= OnPauseChanged;
        // Descarregar a cena não chama de volta um dono que também está sendo destruído.
        if (IsPlaying) Finish(false);
    }

    private void OnDestroy() { advanceAction?.Dispose(); skipAction?.Dispose(); }

    // O mesmo ENTER/clique que fecha a pausa não chega à fala.
    private void OnPauseChanged(bool paused) => ignoreInputUntilFrame = Time.frameCount;

    public bool Play(DialogueSequence dialogue, Action onFinished)
    {
        if (IsPlaying || dialogue == null || !isActiveAndEnabled) return false;
        if (!dialogue.Validate(out string error)) { Debug.LogError(error, dialogue); return false; }
        sequence = dialogue;
        finished = onFinished;
        IsPlaying = true;
        ignoreInputUntilFrame = Time.frameCount;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        routine = StartCoroutine(Run());
        return true;
    }

    public void Skip()
    {
        if (IsPlaying) Finish(true);
    }

    public void Cancel()
    {
        if (IsPlaying) Finish(false);
    }

    private void Update()
    {
        advanceRequested = false;
        if (!IsPlaying) return;
        if (Paused || Time.frameCount <= ignoreInputUntilFrame) { skipHeld = 0f; UpdateSkipHint(); return; }
        advanceRequested = advanceAction.WasPressedThisFrame();
        if (skipMode == SkipMode.Press && skipAction.WasPressedThisFrame()) { Skip(); return; }
        if (skipMode == SkipMode.Hold)
        {
            skipHeld = skipAction.IsPressed() ? skipHeld + Time.unscaledDeltaTime : 0f;
            if (skipAction.IsPressed() && skipHeld >= skipHoldDuration) { Skip(); return; }
        }
        UpdateSkipHint();
    }

    private bool ConsumeAdvance()
    {
        if (!advanceRequested) return false;
        advanceRequested = false;
        return true;
    }

    private IEnumerator Run()
    {
        Prepare();
        yield return Wait(sequence.OpenDelay);
        content.SetActive(true);
        yield return Entrance();
        DialogueSequence.Line[] lines = sequence.Lines;
        for (int i = 0; i < lines.Length; i++)
        {
            DialogueSequence.Line line = lines[i];
            DialogueCast.Speaker speaker = sequence.Cast.Find(line.speakerId);
            LineIndex = i;
            VisibleCharacters = TotalCharacters = 0;
            if (line.pauseBefore > 0f)
            {
                body.text = string.Empty;
                SetRadio(line.pauseStyle == DialogueSequence.PauseStyle.RadioStatic ? RadioState.Static : RadioState.Idle);
                yield return Wait(line.pauseBefore);
            }
            ShowSpeaker(line, speaker);
            LineStarted?.Invoke(i, line);
            if (line.startSound != null) blips.PlayCue(line.startSound, line.startSoundVolume);
            if (line.effect == DialogueSequence.LineEffect.Interference) yield return Interference(line, speaker);
            yield return Type(line, speaker, i == 0 ? textDelay : 0f);
            AwaitingAdvance = true;
            SetRadio(RadioState.Ended);
            continueIcon.enabled = true;
            continueClock = 0f;
            while (!ConsumeAdvance()) yield return null;
            AwaitingAdvance = false;
            continueIcon.enabled = false;
            if (line.pauseAfter > 0f)
            {
                SetRadio(RadioState.Idle);
                yield return Wait(line.pauseAfter);
            }
        }
        SetRadio(RadioState.Idle);
        yield return Wait(sequence.CloseDelay);
        yield return Exit();
        routine = null;
        Finish(true);
    }

    private IEnumerator Type(DialogueSequence.Line line, DialogueCast.Speaker speaker, float delay)
    {
        body.text = line.text;
        body.maxVisibleCharacters = 0;
        body.ForceMeshUpdate();
        // characterCount ignora tags de rich text e conta acentos como um caractere visível.
        TotalCharacters = body.textInfo.characterCount;
        VisibleCharacters = 0;
        blips.BeginLine();
        SetRadio(RadioState.Transmitting);
        float clock = -delay, nextAt = 0f;
        while (VisibleCharacters < TotalCharacters)
        {
            if (ConsumeAdvance())
            {
                // Revela o resto em silêncio; a próxima pressão é que avança.
                VisibleCharacters = TotalCharacters;
                body.maxVisibleCharacters = TotalCharacters;
                blips.StopAll();
                yield break;
            }
            if (!Paused) clock += Time.unscaledDeltaTime;
            bool sounded = false;
            while (VisibleCharacters < TotalCharacters && clock >= nextAt)
            {
                char character = body.textInfo.characterInfo[VisibleCharacters].character;
                VisibleCharacters++;
                // Um engasgo de frame revela várias letras, mas toca no máximo uma.
                if (!sounded) sounded = blips.TryPlay(character, line.soundOverride, speaker.typingPitch);
                nextAt += characterDelay + PunctuationPause(character);
            }
            body.maxVisibleCharacters = VisibleCharacters;
            yield return null;
        }
    }

    private float PunctuationPause(char character)
    {
        if (character == '.' || character == '!' || character == '?') return sentencePause;
        if (character == ',' || character == '…' || character == ';' || character == ':') return commaPause;
        return 0f;
    }

    private IEnumerator Interference(DialogueSequence.Line line, DialogueCast.Speaker speaker)
    {
        DialogueCast.Speaker glimpse = sequence.Cast.Find(line.glimpseSpeakerId);
        Sprite glimpseSprite = DialogueCast.Portrait(glimpse, line.glimpseExpressionId);
        Image slot = speaker.side == DialogueCast.Side.Left ? leftPortrait : rightPortrait;
        Sprite normal = slot.sprite;
        Color accent = speaker.accent;
        SetRadio(RadioState.Static);
        float elapsed = 0f;
        while (elapsed < interferenceDuration)
        {
            if (!Paused) elapsed += Time.unscaledDeltaTime;
            bool on = glimpseSprite != null && (int)(elapsed / .06f) % 2 == 0 && elapsed < interferenceDuration * .8f;
            slot.sprite = on ? glimpseSprite : normal;
            nameplateAccent.color = on && glimpse != null ? glimpse.accent : accent;
            float strength = 1f - elapsed / interferenceDuration;
            panelMotion.anchoredPosition = panelHome + new Vector2(Even(Mathf.Sin(elapsed * 97f) * 4f * strength), Even(Mathf.Cos(elapsed * 71f) * 2f * strength));
            yield return null;
        }
        slot.sprite = normal;
        nameplateAccent.color = accent;
        panelMotion.anchoredPosition = panelHome;
    }

    private void Prepare()
    {
        LineIndex = -1;
        AwaitingAdvance = false;
        VisibleCharacters = TotalCharacters = 0;
        leftShown = rightShown = false;
        currentSpeaker = null;
        skipHeld = 0f;
        body.text = string.Empty;
        nameText.text = string.Empty;
        continueIcon.enabled = false;
        leftPortrait.enabled = rightPortrait.enabled = false;
        leftPortrait.rectTransform.anchoredPosition = leftHome;
        rightPortrait.rectTransform.anchoredPosition = rightHome;
        nameplate.gameObject.SetActive(false);
        SetHighlight(0f);
        group.alpha = 0f;
        panelMotion.anchoredPosition = panelHome;
        UpdateSkipHint();
    }

    private void ShowSpeaker(DialogueSequence.Line line, DialogueCast.Speaker speaker)
    {
        bool changed = currentSpeaker != speaker;
        currentSpeaker = speaker;
        // Falante sem retrato (RÁDIO): placa à esquerda e quem está em cena só escurece.
        bool left = speaker.side != DialogueCast.Side.Right;
        if (speaker.side == DialogueCast.Side.None)
        {
            leftPortrait.color = rightPortrait.color = inactivePortrait;
        }
        else
        {
            Image slot = left ? leftPortrait : rightPortrait;
            Image other = left ? rightPortrait : leftPortrait;
            slot.sprite = DialogueCast.Portrait(speaker, line.expressionId);
            slot.rectTransform.localScale = new Vector3(speaker.flipX ? -1f : 1f, 1f, 1f);
            slot.color = Color.white;
            other.color = inactivePortrait;
            if (left && !leftShown) { leftShown = true; StartCoroutine(PortraitEntrance(slot, leftHome, -1f)); }
            if (!left && !rightShown) { rightShown = true; StartCoroutine(PortraitEntrance(slot, rightHome, 1f)); }
        }

        nameplate.gameObject.SetActive(true);
        bool customFont = speaker.nameFont != null;
        nameText.font = customFont ? speaker.nameFont : nameFont;
        nameText.fontSize = customFont && speaker.nameFontSize > 0f ? speaker.nameFontSize : nameFontSize;
        nameText.alignment = customFont ? TextAlignmentOptions.MidlineLeft : nameAlignment;
        nameText.text = string.IsNullOrEmpty(line.speakerName) ? speaker.displayName : line.speakerName;
        nameplateAccent.color = speaker.accent;
        float width = Even(20f + nameText.GetPreferredValues(nameText.text).x + 22f);
        Vector2 corner = new Vector2(left ? 0f : 1f, 1f);
        nameplate.anchorMin = nameplate.anchorMax = corner;
        nameplate.pivot = new Vector2(left ? 0f : 1f, .5f);
        nameplate.sizeDelta = new Vector2(width, nameplate.sizeDelta.y);
        nameplate.anchoredPosition = new Vector2(left ? plateInset : -plateInset, 2f);
        RectTransform radioRect = radio.rectTransform;
        radioRect.anchorMin = radioRect.anchorMax = new Vector2(left ? 1f : 0f, 1f);
        radioRect.pivot = new Vector2(left ? 1f : 0f, 0f);
        radioRect.anchoredPosition = new Vector2(left ? -plateInset : plateInset, -16f);
        if (!changed) return;
        highlightClock = 0f;
        popClock = 0f;
        SetRadio(RadioState.Click);
    }

    private IEnumerator PortraitEntrance(Image portrait, Vector2 home, float direction)
    {
        RectTransform rect = portrait.rectTransform;
        portrait.enabled = true;
        float elapsed = 0f;
        while (elapsed < portraitEntrance && IsPlaying)
        {
            if (!Paused) elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / portraitEntrance), 3f);
            rect.anchoredPosition = home + new Vector2(Even((1f - t) * 24f * direction), 0f);
            Color color = portrait.color; color.a = t; portrait.color = color;
            yield return null;
        }
        rect.anchoredPosition = home;
        Color final = portrait.color; final.a = 1f; portrait.color = final;
    }

    private IEnumerator Entrance()
    {
        float elapsed = 0f;
        while (elapsed < entranceDuration + .06f)
        {
            if (!Paused) elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / entranceDuration), 3f);
            group.alpha = t;
            panelMotion.anchoredPosition = panelHome + new Vector2(0f, Even((1f - t) * -24f));
            yield return null;
        }
        group.alpha = 1f;
        panelMotion.anchoredPosition = panelHome;
    }

    private IEnumerator Exit()
    {
        body.text = string.Empty;
        continueIcon.enabled = false;
        float elapsed = 0f;
        while (elapsed < exitDuration)
        {
            if (!Paused) elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / exitDuration);
            group.alpha = 1f - t * t;
            panelMotion.anchoredPosition = panelHome + new Vector2(0f, Even(t * t * -12f));
            yield return null;
        }
    }

    private void Finish(bool notify)
    {
        if (routine != null) StopCoroutine(routine);
        StopAllCoroutines();
        routine = null;
        blips.StopAll();
        IsPlaying = false;
        AwaitingAdvance = false;
        Hide();
        Action callback = finished;
        finished = null;
        sequence = null;
        if (notify) callback?.Invoke();
    }

    private void Hide()
    {
        if (group != null) group.alpha = 0f;
        if (panelMotion != null) panelMotion.anchoredPosition = panelHome;
        if (content != null) content.SetActive(false);
    }

    private IEnumerator Wait(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!Paused) elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SetRadio(RadioState state)
    {
        radioState = state;
        radioClock = 0f;
    }

    private void LateUpdate()
    {
        if (!IsPlaying || Paused) return;
        float delta = Time.unscaledDeltaTime;
        radioClock += delta;
        continueClock += delta;
        if (radioState == RadioState.Click && radioClock >= ClickDuration) SetRadio(RadioState.Transmitting);
        Sprite radioSprite = radioState switch
        {
            RadioState.Idle => radioIdle,
            RadioState.Static => Mathf.PerlinNoise(radioClock * 9f, .37f) > .55f ? radioActive[2] : radioIdle,
            RadioState.Click => radioActive[2],
            RadioState.Transmitting => radioActive[LoopFrames[(int)(radioClock / RadioFrame) % LoopFrames.Length]],
            _ => radioActive[0]
        };
        radio.sprite = radioSprite;
        radio.color = new Color(1f, 1f, 1f, radioState == RadioState.Idle ? .6f : 1f);
        if (continueIcon.enabled) continueIcon.sprite = continueFrames[LoopFrames[(int)(continueClock / ContinueFrame) % LoopFrames.Length]];
        highlightClock += delta;
        SetHighlight(Mathf.Pow(1f - Mathf.Clamp01(highlightClock / highlightDuration), 2f));
        popClock += delta;
        float pop = popClock < .12f ? 1f + .06f * Mathf.Sin(popClock / .12f * Mathf.PI) : 1f;
        nameplate.localScale = new Vector3(pop, pop, 1f);
    }

    private void SetHighlight(float alpha)
    {
        if (highlight == null) return;
        highlight.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
    }

    private void UpdateSkipHint()
    {
        if (skipHint == null || skipMode == SkipMode.Disabled) return;
        float progress = skipMode == SkipMode.Hold && skipHoldDuration > 0f ? Mathf.Clamp01(skipHeld / skipHoldDuration) : 0f;
        skipHint.text = string.Format(skipFormat, progress > 0f ? skippingLabel : skipLabel);
        skipHint.alpha = Mathf.Lerp(.7f, 1f, progress);
    }

    private static float Even(float value) => Mathf.Round(value * .5f) * 2f;
}
