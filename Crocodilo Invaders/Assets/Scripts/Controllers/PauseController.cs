using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class PauseController : MonoBehaviour
{
    [SerializeField, Tooltip("Componente de input da cena: PlayerInputReader no combate ou HubPlayerInput na base.")]
    private Behaviour playerInput;
    [SerializeField] private PauseMenuView view;
    [SerializeField] private string menuScene = "Assets/Scenes/MainMenu.unity";

    private InputAction pauseAction;
    private InputAction backAction;
    private float previousTimeScale;
    private bool previousAudioPause;
    private bool inputWasEnabled;
    private bool leaving;
    private bool allowFrozenPause;
    public void AllowPauseWhileFrozen(bool allow) => allowFrozenPause = allow;
    public bool IsPaused { get; private set; }
    public event Action<bool> PauseChanged;
    public event Action MenuReturnRequested;

    private void Awake()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        backAction = new InputAction("PauseBack", InputActionType.Button, "<Gamepad>/buttonEast");
    }

    private void OnEnable() { pauseAction.Enable(); backAction.Enable(); }

    private void Update()
    {
        if (leaving) return;
        if (!pauseAction.WasPressedThisFrame() && !(IsPaused && backAction.WasPressedThisFrame())) return;
        if (IsPaused && view.ShowingOptions) view.ShowMain();
        else if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused || leaving || (Time.timeScale <= 0f && !allowFrozenPause) || playerInput == null) return;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        inputWasEnabled = playerInput != null && playerInput.enabled;
        if (playerInput != null) playerInput.enabled = false;
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseChanged?.Invoke(true);
    }

    public void ResumeGame()
    {
        if (!IsPaused || leaving) return;
        RestoreGame();
        PauseChanged?.Invoke(false);
    }

    public void ReturnToMenu()
    {
        if (!IsPaused || leaving) return;
        int index = SceneUtility.GetBuildIndexByScenePath(menuScene);
        if (index < 0) { Debug.LogError("Inclua MainMenu nas cenas do build.", this); return; }
        leaving = true;
        LeaderboardSession.SaveCompletedRun();
        LeaderboardSession.Cancel();
        MenuReturnRequested?.Invoke();
        view.HideForSceneChange();
        StartCoroutine(LoadMenu(index));
    }

    private IEnumerator LoadMenu(int index)
    {
        yield return new WaitForSecondsRealtime(.12f);
        // O jogo permanece congelado até a troca de cena. OnDisable restaura os globais.
        SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
    }

    private void RestoreGame()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = leaving ? 1f : previousTimeScale;
        AudioListener.pause = leaving ? false : previousAudioPause;
        if (playerInput != null && !leaving) playerInput.enabled = inputWasEnabled;
    }

    private void OnDisable()
    {
        pauseAction.Disable(); backAction.Disable();
        RestoreGame();
    }

    private void OnDestroy() { pauseAction.Dispose(); backAction.Dispose(); }
}
