using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AchievementRuntime : MonoBehaviour
{
    static AchievementRuntime instance;
    public static AchievementRuntime Instance => instance;
    public AchievementView View { get; private set; }
    AchievementTracker tracker;
    DialogueRunner dialogue;
    WaveController waves;
    public bool PresentationHeld => Time.timeScale <= 0f || AudioListener.pause || View.IsVisible ||
        (dialogue != null && dialogue.IsPlaying) || (waves != null &&
        (waves.Phase == WaveController.RunPhase.Intro || waves.Phase == WaveController.RunPhase.BossDefeated ||
         waves.Phase == WaveController.RunPhase.ChapterCompleted ||
         (waves.ActiveBoss != null && (waves.ActiveBoss.Transitioning || waves.ActiveBoss.CombatHeld))));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        var prefab = Resources.Load<GameObject>("Achievements/Achievements");
        if (prefab != null) Instantiate(prefab);
    }
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this; DontDestroyOnLoad(gameObject);
        View = GetComponent<AchievementView>(); tracker = GetComponent<AchievementTracker>();
        SceneManager.sceneLoaded += Loaded;
    }
    void Loaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        View.HideImmediately();
        Achievements.RetrySave();
        dialogue = FindAnyObjectByType<DialogueRunner>(); waves = FindAnyObjectByType<WaveController>();
        tracker.BindScene(scene);
    }
    void OnApplicationQuit() => Achievements.RetrySave();
    void OnDestroy() { if (instance == this) { SceneManager.sceneLoaded -= Loaded; instance = null; } }
}
