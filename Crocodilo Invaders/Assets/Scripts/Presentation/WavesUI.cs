using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Elementos auxiliares da run (painel de início/resultado, retorno ao menu, som do resultado)
// e ponte entre WaveController e WaveAnnouncer: segura o anúncio até a apresentação terminar.
public sealed class WavesUI : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private WaveAnnouncer announcer;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private TMP_Text panelTitle;
    [SerializeField] private TMP_Text panelDetails;
    [SerializeField] private AudioSource resultAudio;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip lossSound;

    private bool suppressStartPanel;
    private int heldAnnouncement;

    public WaveAnnouncer Announcer => announcer;

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
        waves.RunEnded += ShowResult;
        RefreshPanel();
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartRun);
        if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToMenu);
        waves.WaveAnnounced -= ShowWave;
        waves.RunEnded -= ShowResult;
        // Sem a apresentação, a run não pode ficar presa esperando por ela.
        int token = heldAnnouncement;
        heldAnnouncement = 0;
        if (announcer != null) announcer.Cancel();
        if (token != 0) waves.ReleaseAnnouncement(token);
    }

    private void StartRun()
    {
        waves.StartRun();
        RefreshPanel();
    }

    private void ShowWave(int current, int total)
    {
        startPanel.SetActive(false);
        if (announcer == null) return;
        int token = waves.HoldAnnouncement();
        heldAnnouncement = token;
        // A onda do boss é decidida pelo WaveController; o tema é do próprio boss.
        Boss boss = waves.IsBossWave(current) ? waves.BossPrefab : null;
        announcer.Play(current, boss != null ? boss.AlertTheme : null, () => FinishAnnouncement(token));
    }

    private void FinishAnnouncement(int token)
    {
        if (heldAnnouncement == token) heldAnnouncement = 0;
        waves.ReleaseAnnouncement(token);
    }

    private void ShowResult(bool completed)
    {
        heldAnnouncement = 0;
        if (announcer != null) announcer.Cancel();
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
    }

    private void ReturnToMenu()
    {
        if (waves.Phase != WaveController.RunPhase.Completed && waves.Phase != WaveController.RunPhase.Failed) return;
        var pause = FindAnyObjectByType<PauseController>();
        if (pause == null) return;
        pause.PauseGame();
        pause.ReturnToMenu();
    }
}
