using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PauseMenuView : MonoBehaviour
{
    [SerializeField] private PauseController pause;
    [SerializeField] private PauseBackdrop backdrop;
    [SerializeField] private GameObject content;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform logo;
    [SerializeField] private GameObject mainPage;
    [SerializeField] private Button continueButton;
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioOptionsView optionsPrefab;
    [SerializeField, Min(.01f)] private float entryDuration = .18f;
    [SerializeField, Min(.01f)] private float exitDuration = .1f;

    private Coroutine animationRoutine;
    private GameObject previousSelection;
    private Vector2 panelPosition;
    private Vector2 logoPosition;
    private AudioOptionsView options;
    public bool ShowingOptions { get; private set; }

    private void Awake()
    {
        panelPosition = panel.anchoredPosition;
        logoPosition = logo.anchoredPosition;
        if (uiAudio != null) uiAudio.ignoreListenerPause = true;
        content.SetActive(false);
    }

    private void OnEnable() => pause.PauseChanged += OnPauseChanged;

    private void OnPauseChanged(bool paused)
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        if (!paused && options != null) { options.HideImmediately(); RestoreMain(); }
        if (paused)
        {
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            backdrop.Capture();
            content.SetActive(true);
            group.alpha = 0f;
            ShowMain();
        }
        group.interactable = false;
        group.blocksRaycasts = true;
        animationRoutine = StartCoroutine(Animate(paused));
    }

    public void ShowOptions()
    {
        if (!pause.IsPaused || ShowingOptions || optionsPrefab == null) return;
        ShowingOptions = true;
        mainPage.SetActive(false);
        panel.gameObject.SetActive(false);
        logo.gameObject.SetActive(false);
        if (options == null) options = Instantiate(optionsPrefab);
        options.Open(RestoreMain);
    }

    public void ShowMain()
    {
        if (ShowingOptions && options != null && options.IsVisible) { options.Close(); return; }
        RestoreMain();
    }

    private void RestoreMain()
    {
        ShowingOptions = false;
        panel.gameObject.SetActive(true);
        logo.gameObject.SetActive(true);
        mainPage.SetActive(true);
        Select(continueButton);
    }

    public void HideForSceneChange()
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        group.interactable = false;
        animationRoutine = StartCoroutine(Animate(false));
    }

    private IEnumerator Animate(bool opening)
    {
        float initialAlpha = group.alpha;
        float duration = opening ? entryDuration : exitDuration;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float ease = 1f - Mathf.Pow(1f - progress, 3f);
            group.alpha = Mathf.Lerp(initialAlpha, opening ? 1f : 0f, ease);
            float visibility = opening ? ease : 1f - ease;
            panel.localScale = Vector3.one * Mathf.Lerp(.965f, 1f, visibility);
            panel.anchoredPosition = panelPosition + Vector2.down * (12f * (1f - visibility));
            logo.anchoredPosition = logoPosition + Vector2.up * (8f * (1f - visibility));
            yield return null;
        }
        group.alpha = opening ? 1f : 0f;
        group.interactable = opening;
        if (opening)
        {
            panel.localScale = Vector3.one;
            panel.anchoredPosition = panelPosition;
            logo.anchoredPosition = logoPosition;
            if (!ShowingOptions) Select(continueButton);
        }
        else
        {
            content.SetActive(false);
            backdrop.Release();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
        }
        animationRoutine = null;
    }

    private static void Select(Button button)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void OnDisable()
    {
        pause.PauseChanged -= OnPauseChanged;
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = null;
        backdrop.Release();
        if (options != null) options.HideImmediately();
    }
    private void OnDestroy() { if (options != null) Destroy(options.gameObject); }
}
