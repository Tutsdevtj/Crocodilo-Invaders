using System.Collections;
using TMPro;
using UnityEngine;

public sealed class GameplayLoadingView : MonoBehaviour
{
    [SerializeField] private CanvasGroup screen;
    [SerializeField] private CanvasGroup content;
    [SerializeField] private RectTransform ship;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text dots;
    [SerializeField] private TMP_Text prompt;
    [SerializeField, Min(.01f)] private float enterDuration = .12f;
    [SerializeField, Min(.01f)] private float exitDuration = .45f;
    private Vector2 shipPosition;
    private float startedAt;
    private bool ready;
    private int lastDotCount = -1;
    private static readonly string[] DotFrames = { "", ".", "..", "..." };

    private void Awake() => shipPosition = ship.anchoredPosition;

    public void ShowLoading()
    {
        startedAt = Time.unscaledTime;
        ready = false;
        screen.alpha = 0f;
        content.alpha = 1f;
        label.text = "LOADING";
        prompt.gameObject.SetActive(false);
        dots.gameObject.SetActive(true);
    }

    public void ShowReady()
    {
        ready = true;
        label.text = "PRONTO";
        label.rectTransform.anchoredPosition = new Vector2(0f, label.rectTransform.anchoredPosition.y);
        dots.gameObject.SetActive(false);
        prompt.text = "Pressione <b>ENTER</b> para jogar";
        prompt.gameObject.SetActive(true);
    }

    public void ShowError()
    {
        ready = true;
        label.text = "Não foi possível preparar a fase";
        label.rectTransform.sizeDelta = new Vector2(740f, label.rectTransform.sizeDelta.y);
        label.rectTransform.anchoredPosition = new Vector2(0f, label.rectTransform.anchoredPosition.y);
        dots.gameObject.SetActive(false);
        prompt.text = "Pressione <b>ENTER</b> para voltar ao menu";
        prompt.gameObject.SetActive(true);
    }

    private void Update()
    {
        float time = Time.unscaledTime - startedAt;
        ship.anchoredPosition = shipPosition + Vector2.up * (Mathf.Sin(time * 2f) * 5f);
        if (ready)
        {
            Color color = prompt.color;
            color.a = .75f + .25f * (.5f + .5f * Mathf.Sin(time * 3f));
            prompt.color = color;
        }
        else
        {
            int count = Mathf.FloorToInt(time / .3f) % DotFrames.Length;
            if (count == lastDotCount) return;
            lastDotCount = count;
            dots.text = DotFrames[count];
        }
    }

    public IEnumerator FadeIn()
    {
        for (float time = 0f; time < enterDuration; time += Time.unscaledDeltaTime)
        {
            screen.alpha = Mathf.Clamp01(time / enterDuration);
            yield return null;
        }
        screen.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        for (float time = 0f; time < exitDuration; time += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(time / exitDuration);
            content.alpha = 1f - Mathf.Clamp01(t * 4f);
            screen.alpha = 1f - t * t * (3f - 2f * t);
            yield return null;
        }
        screen.alpha = 0f;
    }
}
