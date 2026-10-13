using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class WavesUI : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private TMP_Text panelTitle;
    [SerializeField] private TMP_Text panelDetails;
    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private CanvasGroup bannerGroup;
    [SerializeField, Min(0.01f)] private float enterDuration = .22f;
    [SerializeField, Min(0f)] private float holdDuration = .65f;
    [SerializeField, Min(0.01f)] private float exitDuration = .55f;
    [SerializeField] private AudioSource resultAudio;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip lossSound;

    private Coroutine animationRoutine;
    private bool suppressStartPanel;

    public void SuppressStartPanel(bool suppress)
    {
        suppressStartPanel = suppress;
        RefreshPanel();
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(StartRun);
        if (returnButton != null) returnButton.onClick.AddListener(ReturnToMenu);
        waves.WaveAnnounced += ShowWave;
        waves.WaveCleared += ShowCleared;
        waves.RunEnded += ShowResult;
        waves.BossStarted += ShowBoss;
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;
        RefreshPanel();
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartRun);
        if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToMenu);
        waves.WaveAnnounced -= ShowWave;
        waves.WaveCleared -= ShowCleared;
        waves.RunEnded -= ShowResult;
        waves.BossStarted -= ShowBoss;
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = null;
        bannerGroup.alpha = 0f;
    }

    private void StartRun()
    {
        waves.StartRun();
        RefreshPanel();
    }

    private void ShowWave(int current, int total)
    {
        startPanel.SetActive(false);
        waveCounter.text = $"ONDA {current} / {total}";
        ShowBanner($"ONDA {current}", Color.white);
    }

    private void ShowCleared(int current, int bonus)
    {
        string reward = bonus > 0 ? $"\n<size=24>+{bonus} pontos</size>" : string.Empty;
        ShowBanner($"ONDA {current} CONCLUÍDA{reward}", new Color(.68f, 1f, .45f));
    }

    private void ShowBoss(Boss boss)
    {
        waveCounter.text = $"ONDA {waves.CurrentWave} / {waves.WaveCount} • BOSS";
        ShowBanner("EXECUTOR V-9", new Color(1f, .55f, .35f));
    }

    private void ShowResult(bool completed)
    {
        RefreshPanel();
        if (returnButton != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(returnButton.gameObject);
        AudioClip clip = completed ? winSound : lossSound;
        if (resultAudio != null && clip != null) resultAudio.PlayOneShot(clip);
    }

    private void RefreshPanel()
    {
        bool waiting = waves.Phase == WaveController.RunPhase.Waiting;
        bool completed = waves.Phase == WaveController.RunPhase.Completed;
        bool ended = completed || waves.Phase == WaveController.RunPhase.Failed;
        startPanel.SetActive((waiting && !suppressStartPanel) || ended);
        startButton.gameObject.SetActive(waiting);
        if (returnButton != null) returnButton.gameObject.SetActive(ended);
        panelTitle.text = waiting ? "PRONTO?" : completed ? "VITÓRIA" : "FIM DE JOGO";
        panelDetails.text = waiting ? "Elimine os inimigos para avançar." : $"{waves.Score.Points} pontos • {waves.Score.Kills} inimigos";
        if (waiting) waveCounter.text = string.Empty;
        if (ended && animationRoutine != null) StopCoroutine(animationRoutine);
        if (ended) bannerGroup.alpha = 0f;
    }

    private void ReturnToMenu()
    {
        if (waves.Phase != WaveController.RunPhase.Completed && waves.Phase != WaveController.RunPhase.Failed) return;
        var pause = FindAnyObjectByType<PauseController>();
        if (pause == null) return;
        pause.PauseGame();
        pause.ReturnToMenu();
    }

    private void ShowBanner(string text, Color color)
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        bannerText.text = text;
        bannerText.color = color;
        animationRoutine = StartCoroutine(AnimateBanner());
    }

    private IEnumerator AnimateBanner()
    {
        RectTransform rect = bannerText.rectTransform;
        RectTransform parent = (RectTransform)rect.parent;
        float outside = (parent.rect.width + rect.rect.width) * .5f + 40f;
        bannerGroup.alpha = 1f;

        // Entrada freia ao chegar ao centro; saída ganha velocidade.
        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            float t = Mathf.Clamp01(elapsed / enterDuration);
            SetBannerX(rect, Mathf.Lerp(outside, 0f, 1f - Mathf.Pow(1f - t, 3f)));
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetBannerX(rect, 0f);
        yield return new WaitForSeconds(holdDuration);
        elapsed = 0f;
        while (elapsed < exitDuration)
        {
            float t = Mathf.Clamp01(elapsed / exitDuration);
            SetBannerX(rect, Mathf.Lerp(0f, -outside, t * t * t));
            bannerGroup.alpha = 1f - t * t;
            elapsed += Time.deltaTime;
            yield return null;
        }
        bannerGroup.alpha = 0f;
        animationRoutine = null;
    }

    private static void SetBannerX(RectTransform rect, float x)
    {
        Vector2 position = rect.anchoredPosition;
        position.x = x;
        rect.anchoredPosition = position;
    }
}
