using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

// Resultado da run, retorno ao menu e som do resultado
// e ponte entre WaveController e WaveAnnouncer: segura o anúncio até a apresentação terminar.
public sealed class WavesUI : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private WaveAnnouncer announcer;
    [SerializeField, FormerlySerializedAs("startPanel")] private GameObject resultPanel;
    [SerializeField] private Button returnButton;
    [SerializeField] private TMP_Text panelTitle;
    [SerializeField] private TMP_Text panelDetails;
    [SerializeField] private AudioSource resultAudio;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip lossSound;

    private int heldAnnouncement;

    public WaveAnnouncer Announcer => announcer;

    private void OnEnable()
    {
        if (returnButton != null) returnButton.onClick.AddListener(ReturnToMenu);
        waves.WaveAnnounced += ShowWave;
        waves.RunEnded += ShowResult;
        RefreshPanel();
    }

    private void OnDisable()
    {
        if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToMenu);
        waves.WaveAnnounced -= ShowWave;
        waves.RunEnded -= ShowResult;
        // Sem a apresentação, a run não pode ficar presa esperando por ela.
        int token = heldAnnouncement;
        heldAnnouncement = 0;
        if (announcer != null) announcer.Cancel();
        if(waves!=null)
        {
            var lake=waves.GetComponent<LakeBossEntrance>();if(lake!=null)lake.Cancel(false);
            if (token != 0) waves.ReleaseAnnouncement(token);
        }
    }

    private void ShowWave(int current, int total)
    {
        resultPanel.SetActive(false);
        if (announcer == null) return;
        int token = waves.HoldAnnouncement();
        heldAnnouncement = token;
        // A onda do boss é decidida pelo WaveController; o tema é do próprio boss.
        Boss boss = waves.IsBossWave(current) ? waves.BossPrefab : null;
        var lake=waves.GetComponent<LakeBossEntrance>();
        if(boss!=null && lake!=null && boss.GetComponent<SapoDoPneuCombat>()!=null)
        {
            lake.Play(announcer,current,()=>FinishAnnouncement(token));return;
        }
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
        var lake=waves.GetComponent<LakeBossEntrance>();if(lake!=null)lake.Cancel();
        RefreshPanel();
        if (returnButton != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(returnButton.gameObject);
        AudioClip clip = completed ? winSound : lossSound;
        if (resultAudio != null && clip != null) resultAudio.PlayOneShot(clip);
    }

    private void RefreshPanel()
    {
        bool completed = waves.Phase == WaveController.RunPhase.Completed;
        bool ended = completed || waves.Phase == WaveController.RunPhase.Failed;
        resultPanel.SetActive(ended);
        if (returnButton != null) returnButton.gameObject.SetActive(ended);
        panelTitle.text = completed ? "VITÓRIA" : "FIM DE JOGO";
        panelDetails.text = ended ? $"{waves.Score.Points} pontos • {waves.Score.Kills} inimigos" : string.Empty;
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
