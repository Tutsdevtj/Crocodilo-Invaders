using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MenuIntro : MonoBehaviour
{
    [SerializeField] private AudioSource music;
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private Image curtain;
    [SerializeField] private RectTransform logo;
    [SerializeField] private CanvasGroup logoGroup;
    [SerializeField] private CanvasGroup[] buttonGroups;
    [SerializeField] private Button firstButton;
    [SerializeField, Min(0f)] private float dropTime = 2.59f;
    [SerializeField, Min(.01f)] private float skyDuration = .75f;
    [SerializeField, Min(0f)] private float logoStart = 1.15f;
    [SerializeField, Min(.01f)] private float logoDuration = .8f;
    [SerializeField, Min(.01f)] private float buttonDuration = .32f;
    [SerializeField, Min(0f)] private float buttonStagger = .065f;

    private Vector2 logoPosition;
    private Vector3 logoScale;
    private Vector2[] buttonPositions;

    private void Awake()
    {
        logoPosition = logo.anchoredPosition;
        logoScale = logo.localScale;
        buttonPositions = new Vector2[buttonGroups.Length];
        for (int i = 0; i < buttonGroups.Length; i++)
            buttonPositions[i] = ((RectTransform)buttonGroups[i].transform).anchoredPosition;
        menuGroup.interactable = false;
        curtain.gameObject.SetActive(true);
        ApplyEntrance(0f);
    }

    private IEnumerator Start()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        bool hasMusic = music != null && music.clip != null;
        if (hasMusic)
        {
            music.clip.LoadAudioData();
            while (music.clip.loadState == AudioDataLoadState.Loading) yield return null;
            hasMusic = music.clip.loadState == AudioDataLoadState.Loaded;
        }

        double audioStart = AudioSettings.dspTime + .1;
        float silentStart = Time.unscaledTime;
        if (hasMusic) music.PlayScheduled(audioStart);
        float finishAt = dropTime + Mathf.Max(0, buttonGroups.Length - 1) * buttonStagger + buttonDuration;
        while (true)
        {
            // Segue a música; Time.timeScale não muda a marcação do drop.
            float elapsed = hasMusic ? Mathf.Max(0f, (float)(AudioSettings.dspTime - audioStart))
                : Time.unscaledTime - silentStart;
            ApplyEntrance(elapsed);
            if (elapsed >= finishAt || (elapsed > .15f && WantsToSkip())) break;
            yield return null;
        }
        ApplyEntrance(finishAt);
        logo.anchoredPosition = logoPosition;
        logo.localScale = logoScale;
        curtain.gameObject.SetActive(false);
        // O comando que pula a entrada não pode também apertar Jogar neste frame.
        yield return null;
        menuGroup.interactable = true;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    private void ApplyEntrance(float elapsed)
    {
        Color tint = curtain.color;
        tint.a = 1f - EaseOut(elapsed / skyDuration);
        curtain.color = tint;

        float reveal = EaseOut((elapsed - logoStart) / logoDuration);
        logoGroup.alpha = reveal;
        logo.anchoredPosition = logoPosition + Vector2.up * (34f * (1f - reveal));
        float anticipation = EaseOut((elapsed - dropTime + .08f) / .08f);
        float recovery = EaseOut((elapsed - dropTime) / .24f);
        float pulse = anticipation * (1f - recovery) * .045f;
        logo.localScale = logoScale * (Mathf.Lerp(.94f, 1f, reveal) + pulse);

        for (int i = 0; i < buttonGroups.Length; i++)
        {
            float progress = EaseOut((elapsed - dropTime - i * buttonStagger) / buttonDuration);
            buttonGroups[i].alpha = progress;
            ((RectTransform)buttonGroups[i].transform).anchoredPosition =
                buttonPositions[i] + Vector2.down * (14f * (1f - progress));
        }
    }

    private static float EaseOut(float progress)
    {
        float t = 1f - Mathf.Clamp01(progress);
        return 1f - t * t * t;
    }

    private static bool WantsToSkip() =>
        (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

    private void OnDisable() => StopAllCoroutines();
}
