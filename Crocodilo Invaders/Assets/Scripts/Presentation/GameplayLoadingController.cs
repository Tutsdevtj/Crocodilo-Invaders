using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameplayLoadingView))]
public sealed class GameplayLoadingController : MonoBehaviour
{
    public enum LoadingState { Loading, Ready, Fading, Error }
    [SerializeField] private string menuScene = "Assets/Scenes/MainMenu.unity";
    private static GameplayLoadingController active;
    private readonly List<Behaviour> blockedInputs = new List<Behaviour>();
    private GameplayLoadingView view;
    private AsyncOperation operation;
    private WaveController waves;
    private string targetScene;
    private float previousTimeScale;
    private bool previousAudioPause;
    private bool ownsFreeze;
    private bool confirmArmed;
    public LoadingState State { get; private set; }

    public bool Begin(string scenePath)
    {
        int index = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (active != null || index < 0) return false;
        active = this;
        targetScene = scenePath;
        view = GetComponent<GameplayLoadingView>();
        DontDestroyOnLoad(gameObject);
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        ownsFreeze = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        State = LoadingState.Loading;
        view.ShowLoading();
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(Load(index));
        return true;
    }

    private IEnumerator Load(int index)
    {
        // Deixa o loading desenhar antes de iniciar o trabalho da troca de cena.
        yield return view.FadeIn();
        operation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        while (operation.progress < .9f) yield return null;
        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;
        // Awake, Start e o primeiro render da fase acontecem atrás do fundo preto.
        yield return null;
        yield return null;
        if (waves == null || !waves.isActiveAndEnabled)
        {
            Debug.LogError("A fase carregada precisa de um WaveController ativo.", this);
            State = LoadingState.Error;
            view.ShowError();
            yield break;
        }
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
            if (source.clip != null && source.clip.loadState == AudioDataLoadState.Unloaded) source.clip.LoadAudioData();
        bool loadingAudio;
        do
        {
            loadingAudio = false;
            foreach (AudioSource source in sources)
                loadingAudio |= source != null && source.clip != null && source.clip.loadState == AudioDataLoadState.Loading;
            if (loadingAudio) yield return null;
        } while (loadingAudio);
        State = LoadingState.Ready;
        view.ShowReady();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.path != targetScene) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (waves == null) waves = root.GetComponentInChildren<WaveController>();
            foreach (PlayerInputReader input in root.GetComponentsInChildren<PlayerInputReader>(true)) Block(input);
            foreach (EventSystem input in root.GetComponentsInChildren<EventSystem>(true)) Block(input);
        }
    }

    private void Block(Behaviour input)
    {
        if (!input.enabled) return;
        blockedInputs.Add(input);
        input.enabled = false;
    }

    private void Update()
    {
        if (State != LoadingState.Ready && State != LoadingState.Error) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        // O ENTER usado para selecionar Jogar não confirma o loading por acidente.
        if (!confirmArmed)
        {
            confirmArmed = !keyboard.enterKey.isPressed && !keyboard.numpadEnterKey.isPressed;
            return;
        }
        if (!keyboard.enterKey.wasPressedThisFrame && !keyboard.numpadEnterKey.wasPressedThisFrame) return;
        StartCoroutine(State == LoadingState.Error ? ReturnToMenu() : StartGame());
    }

    private IEnumerator StartGame()
    {
        State = LoadingState.Fading;
        yield return view.FadeOut();
        RestoreGame();
        if (DevelopmentCardSession.IsActive)
        {
            var development = waves.gameObject.AddComponent<DevelopmentCardTest>();
            development.Initialize(waves, menuScene);
        }
        else waves.StartRun();
        Destroy(gameObject);
    }

    private IEnumerator ReturnToMenu()
    {
        State = LoadingState.Fading;
        int index = SceneUtility.GetBuildIndexByScenePath(menuScene);
        if (index >= 0) yield return SceneManager.LoadSceneAsync(index);
        RestoreGame();
        Destroy(gameObject);
    }

    private void RestoreGame()
    {
        if (!ownsFreeze) return;
        ownsFreeze = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        foreach (Behaviour input in blockedInputs) if (input != null) input.enabled = true;
        blockedInputs.Clear();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (operation != null && !operation.isDone) operation.allowSceneActivation = true;
        RestoreGame();
        if (active == this) active = null;
    }
}
