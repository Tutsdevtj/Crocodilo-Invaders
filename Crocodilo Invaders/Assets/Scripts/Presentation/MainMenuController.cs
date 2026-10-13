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
    [SerializeField, Min(.01f)] private float exitDuration = .2f;

    private bool leaving;
    private AudioOptionsView options;

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
        if (options == null || !options.IsVisible) return;
        if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)) options.Close();
    }

    private void OnDestroy() { if (options != null) Destroy(options.gameObject); }

    public void PlayGame()
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
        leaving = true;
        menuGroup.interactable = false;
        GameplayLoadingController loading = Instantiate(loadingPrefab);
        if (loading.Begin(gameplayScene)) return;
        Destroy(loading.gameObject);
        leaving = false;
        menuGroup.interactable = true;
    }

    public void QuitGame()
    {
        if (!leaving) StartCoroutine(LeaveMenu());
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
