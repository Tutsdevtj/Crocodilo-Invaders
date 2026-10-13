using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplayScene = "Assets/Scenes/Tests/PlayerMovementTest.unity";
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private AudioSource music;
    [SerializeField] private GameplayLoadingController loadingPrefab;
    [SerializeField] private AudioOptionsView optionsPrefab;
    [SerializeField] private Button optionsButton;
    [SerializeField] private PlayerNameView namePrefab;
    [SerializeField] private LeaderboardView leaderboardPrefab;
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField, Min(.01f)] private float exitDuration = .2f;

    private bool leaving;
    private AudioOptionsView options;
    private PlayerNameView nameView;
    private LeaderboardView leaderboard;

    private void Awake()
    {
        LeaderboardSession.SaveCompletedRun();
        LeaderboardSession.Cancel();
    }

    public void ShowLeaderboard()
    {
        if (leaving || !menuGroup.interactable || leaderboardPrefab == null) return;
        if (leaderboard == null) leaderboard = Instantiate(leaderboardPrefab);
        menuGroup.interactable = false;
        leaderboard.Open(() => RestoreMenu(leaderboardButton));
    }

    private void RestoreMenu(Button selected)
    {
        if (leaving) return;
        menuGroup.interactable = true;
        if (EventSystem.current != null && selected != null) EventSystem.current.SetSelectedGameObject(selected.gameObject);
    }

    public void ShowOptions()
    {
        if (leaving || !menuGroup.interactable || optionsPrefab == null) return;
        if (options == null) options = Instantiate(optionsPrefab);
        menuGroup.interactable = false;
        options.Open(() =>
        {
            if (leaving) return;
            menuGroup.interactable = true;
            if (EventSystem.current != null && optionsButton != null) EventSystem.current.SetSelectedGameObject(optionsButton.gameObject);
        });
    }

    private void Update()
    {
        if ((Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            && (Gamepad.current == null || !Gamepad.current.buttonEast.wasPressedThisFrame)) return;
        if (options != null && options.IsVisible) options.Close();
        else if (nameView != null && nameView.IsVisible) nameView.Close();
        else if (leaderboard != null && leaderboard.IsVisible) leaderboard.Close();
    }

    private void OnDestroy()
    {
        if (options != null) Destroy(options.gameObject);
        if (nameView != null) Destroy(nameView.gameObject);
        if (leaderboard != null) Destroy(leaderboard.gameObject);
    }

    private void OnApplicationQuit() => LeaderboardSession.SaveCompletedRun();

    public void PlayGame()
    {
        if (leaving || !menuGroup.interactable || namePrefab == null) return;
        if (nameView == null) nameView = Instantiate(namePrefab);
        menuGroup.interactable = false;
        nameView.Open(BeginNamedGame, () => RestoreMenu(playButton));
    }

    private void BeginNamedGame(string playerName)
    {
        if (leaving) return;
        if (SceneUtility.GetBuildIndexByScenePath(gameplayScene) < 0)
        {
            Debug.LogError("Inclua a cena de gameplay na lista de cenas do build.", this);
            return;
        }
        if (loadingPrefab == null)
        {
            Debug.LogError("Configure o prefab de loading no MainMenuController.", this);
            return;
        }
        if (!LeaderboardSession.Prepare(playerName))
        {
            nameView.ShowError("NÃO FOI POSSÍVEL SALVAR O RANKING ANTERIOR");
            return;
        }
        leaving = true;
        menuGroup.interactable = false;
        GameplayLoadingController loading = Instantiate(loadingPrefab);
        if (loading.Begin(gameplayScene)) { nameView.Hide(); return; }
        Destroy(loading.gameObject);
        leaving = false;
        LeaderboardSession.Cancel();
        nameView.ShowError("NÃO FOI POSSÍVEL INICIAR. TENTE NOVAMENTE.");
    }

    public void QuitGame()
    {
        if (!leaving && menuGroup.interactable) StartCoroutine(LeaveMenu());
    }

    private IEnumerator LeaveMenu()
    {
        leaving = true;
        menuGroup.interactable = false;
        float volume = music != null ? music.volume : 0f;
        for (float elapsed = 0f; elapsed < exitDuration; elapsed += Time.unscaledDeltaTime)
        {
            float fade = 1f - Mathf.Clamp01(elapsed / exitDuration);
            menuGroup.alpha = fade;
            if (music != null) music.volume = volume * fade;
            yield return null;
        }
        menuGroup.alpha = 0f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
