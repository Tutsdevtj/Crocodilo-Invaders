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
    private DeveloperMenuView developerMenu;
    private string selectedGameplayScene;
    private GameObject jungleEntry;

    private void Awake()
    {
        DevelopmentCardSession.End();
        LeaderboardSession.SaveCompletedRun();
        LeaderboardSession.Cancel();
        developerMenu = gameObject.AddComponent<DeveloperMenuView>();
        developerMenu.Initialize(this, playButton != null ? playButton.GetComponentInChildren<TMPro.TMP_Text>(true)?.font : null);
        if (SceneUtility.GetBuildIndexByScenePath(JungleStageScene) >= 0)
        {
            jungleEntry = DevelopmentUI.Canvas("JungleMissionEntry");
            jungleEntry.AddComponent<CanvasGroup>();
            jungleEntry.GetComponent<Canvas>().sortingOrder = 40;
            var button = DevelopmentUI.Button("JungleMission", jungleEntry.transform,
                playButton != null ? playButton.GetComponentInChildren<TMPro.TMP_Text>(true)?.font : null,
                "FASE 2 · SELVA", PlayJungle, 44);
            DevelopmentUI.Corner(button, Vector2.zero, new Vector2(24, 24), new Vector2(286, 44));
        }
    }

    public bool TryOpenDeveloperMenu()
    {
        if (leaving || !menuGroup.interactable) return false;
        menuGroup.interactable = false;
        return true;
    }

    public void CloseDeveloperMenu() => RestoreMenu(playButton);

    public bool BeginDevelopmentCards()
    {
        if (leaving || loadingPrefab == null || SceneUtility.GetBuildIndexByScenePath(gameplayScene) < 0) return false;
        DevelopmentCardSession.Begin();
        LeaderboardSession.Cancel();
        GameplayLoadingController loading = Instantiate(loadingPrefab);
        if (!loading.Begin(gameplayScene))
        {
            Destroy(loading.gameObject);
            DevelopmentCardSession.End();
            return false;
        }
        leaving = true;
        menuGroup.interactable = false;
        return true;
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
        if (jungleEntry != null)
        {
            var group = jungleEntry.GetComponent<CanvasGroup>();
            group.alpha = menuGroup.alpha;
            group.interactable = group.blocksRaycasts = !leaving && menuGroup.interactable;
        }
        if ((Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            && (Gamepad.current == null || !Gamepad.current.buttonEast.wasPressedThisFrame)) return;
        if (developerMenu != null && developerMenu.IsVisible) developerMenu.Close();
        else if (options != null && options.IsVisible) options.Close();
        else if (nameView != null && nameView.IsVisible) nameView.Close();
        else if (leaderboard != null && leaderboard.IsVisible) leaderboard.Close();
    }

    private void OnDestroy()
    {
        if (jungleEntry != null) Destroy(jungleEntry);
        if (options != null) Destroy(options.gameObject);
        if (nameView != null) Destroy(nameView.gameObject);
        if (leaderboard != null) Destroy(leaderboard.gameObject);
    }

    private void OnApplicationQuit() => LeaderboardSession.SaveCompletedRun();

    public void PlayGame()
        => PlayChapter(gameplayScene);

    public const string JungleStageScene = "Assets/Scenes/StageTwoJungle.unity";
    public void PlayJungle() => PlayChapter(JungleStageScene);

    private void PlayChapter(string scenePath)
    {
        if (leaving || !menuGroup.interactable || namePrefab == null) return;
        selectedGameplayScene = scenePath;
        if (nameView == null) nameView = Instantiate(namePrefab);
        menuGroup.interactable = false;
        nameView.Open(BeginNamedGame, () => RestoreMenu(playButton));
    }

    private void BeginNamedGame(string playerName)
    {
        if (leaving) return;
        if (SceneUtility.GetBuildIndexByScenePath(selectedGameplayScene) < 0)
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
        if (loading.Begin(selectedGameplayScene)) { nameView.Hide(); return; }
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
