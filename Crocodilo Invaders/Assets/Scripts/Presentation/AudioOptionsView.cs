using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class AudioOptionsView : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Slider[] sliders;
    [SerializeField] private TMP_Text[] percentages;
    [SerializeField] private Button back;
    [SerializeField] private AudioSource previewAudio;
    [SerializeField] private AudioClip previewSound;
    private Action closed;
    private bool closing;
    private Coroutine animationRoutine;
    private float lastPreview = -1f;
    public bool IsVisible => content.activeSelf;
    public Slider[] Sliders => sliders;

    private void Awake()
    {
        previewAudio.ignoreListenerPause = true;
        for (int i = 0; i < sliders.Length; i++)
        {
            int index = i;
            sliders[i].onValueChanged.AddListener(value => OnValue(index, value));
        }
        back.onClick.AddListener(Close);
        content.SetActive(false);
    }

    private void OnEnable() => GameAudioSettings.Changed += Refresh;
    private void OnDisable() { GameAudioSettings.Changed -= Refresh; HideImmediately(); }

    public void Open(Action onClosed)
    {
        if (IsVisible) return;
        closed = onClosed;
        closing = false;
        Refresh();
        content.SetActive(true);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;
        animationRoutine = StartCoroutine(Animate(true));
    }

    private void Refresh()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            int percent = Mathf.RoundToInt(GameAudioSettings.Get((AudioChannel)i) * 100f);
            sliders[i].SetValueWithoutNotify(percent);
            percentages[i].text = percent + "%";
        }
    }

    private void OnValue(int index, float percent)
    {
        GameAudioSettings.Set((AudioChannel)index, percent / 100f);
        if (index == (int)AudioChannel.Music || previewSound == null || Time.unscaledTime - lastPreview < .12f) return;
        lastPreview = Time.unscaledTime;
        previewAudio.PlayOneShot(previewSound, .25f);
    }

    public void Close()
    {
        if (!IsVisible || closing) return;
        closing = true;
        GameAudioSettings.Save();
        group.interactable = false;
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(Animate(false));
    }

    private IEnumerator Animate(bool opening)
    {
        float alpha = group.alpha;
        float duration = opening ? .16f : .12f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
            float visibility = opening ? ease : 1f - ease;
            group.alpha = Mathf.Lerp(alpha, opening ? 1f : 0f, ease);
            panel.localScale = Vector3.one * Mathf.Lerp(.97f, 1f, visibility);
            yield return null;
        }
        animationRoutine = null;
        if (opening)
        {
            panel.localScale = Vector3.one;
            group.alpha = 1f;
            group.interactable = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(sliders[0].gameObject);
        }
        else
        {
            Action callback = closed;
            HideImmediately();
            callback?.Invoke();
        }
    }

    public void HideImmediately()
    {
        GameAudioSettings.Save();
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = null;
        closed = null;
        content.SetActive(false);
        group.blocksRaycasts = false;
        previewAudio.Stop();
    }
}
