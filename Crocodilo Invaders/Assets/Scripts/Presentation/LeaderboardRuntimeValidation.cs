#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Only loaded by the explicit validation command; omitted entirely from player builds.
public sealed class LeaderboardRuntimeValidation : MonoBehaviour
{
    private Keyboard keyboard;
    private LeaderboardStore store;
    private string folder;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (Environment.GetCommandLineArgs().Contains("CrocodiloLeaderboardValidation.Runtime"))
            new GameObject("LeaderboardValidation").AddComponent<LeaderboardRuntimeValidation>();
    }

    private void Awake() => DontDestroyOnLoad(gameObject);

    private IEnumerator Start()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard = InputSystem.AddDevice<Keyboard>();
        folder = Path.GetFullPath("Logs/LeaderboardRuntime/" + Guid.NewGuid().ToString("N"));
        store = new LeaderboardStore(folder);
        typeof(LeaderboardSession).GetField("store", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, store);
        var checks = Checks();
        while (true)
        {
            object step;
            try { if (!checks.MoveNext()) break; step = checks.Current; }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); yield break; }
            yield return step;
        }
        InputSystem.RemoveDevice(keyboard);
        Debug.Log("LEADERBOARD RUNTIME VALIDATION PASSED");
        EditorApplication.Exit(0);
    }

    private IEnumerator Checks()
    {
        yield return new WaitForSecondsRealtime(4f);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Check(menu != null, "Main menu loads");
        Preview("leaderboard-menu.png", 1200, 720);
        menu.ShowLeaderboard(); yield return null;
        var ranking = Object.FindAnyObjectByType<LeaderboardView>();
        Check(ranking != null && ranking.IsVisible && ranking.PageCount == 1, "Menu trophy opens empty ranking");
        Check(!menu.GetComponent<CanvasGroup>().interactable, "Menu input is blocked behind ranking");
        Check(!Button(ranking, "NextButton").interactable, "Empty ranking disables next page");
        Preview("leaderboard-empty.png", 1200, 720);
        Keys(Key.Escape); yield return null; Keys(); yield return null;
        Check(!ranking.IsVisible && menu.GetComponent<CanvasGroup>().interactable, "Escape returns to main menu");
        Button(menu, "PlayButton").onClick.Invoke(); yield return null;
        var name = Object.FindAnyObjectByType<PlayerNameView>();
        var input = name.GetComponentInChildren<TMP_InputField>(true);
        Check(name.IsVisible && EventSystem.current.currentSelectedGameObject == input.gameObject, "Play asks for the name and focuses the input");
        menu.PlayGame(); Check(Object.FindObjectsByType<PlayerNameView>().Length == 1, "Repeated Play cannot duplicate name dialog");
        name.Confirm(); Check(name.IsVisible && Object.FindAnyObjectByType<GameplayLoadingController>() == null, "Empty name cannot enter loading");
        Preview("leaderboard-name.png", 1200, 720);
        input.text = "João Silva";
        Keys(Key.Enter); yield return null; yield return null;
        var loading = Object.FindAnyObjectByType<GameplayLoadingController>();
        Check(loading != null, "Enter submits the name and opens loading");
        float deadline = Time.realtimeSinceStartup + 60;
        while (loading != null && loading.State == GameplayLoadingController.LoadingState.Loading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loading != null && loading.State == GameplayLoadingController.LoadingState.Ready, "Gameplay is ready behind loading gate");
        yield return new WaitForSecondsRealtime(.15f);
        var waves = Object.FindAnyObjectByType<WaveController>();
        Check(waves.CurrentWave == 0, "Name submission Enter does not skip the loading gate");
        Keys(); yield return null; yield return null;
        Keys(Key.Enter); yield return null; Keys(); yield return new WaitForSecondsRealtime(.6f);
        Check(waves.CurrentWave == 1 && waves.Score.Points == 0, "Fresh Enter starts a normal zero-score run");
        waves.Score.RecordDefeat(1250);
        Object.FindAnyObjectByType<Player>().GetComponent<Health>().TakeDamage(100000);
        Check(waves.Phase == WaveController.RunPhase.Failed, "Player death ends the run");
        waves.Score.AwardPoints(999);
        Check(store.Entries.Count == 0, "Completed score waits until returning to menu");
        var ui = Object.FindAnyObjectByType<WavesUI>();
        Check(Button(ui, "ReturnToMenuButton").gameObject.activeInHierarchy, "Result shows a direct return button");
        Preview("leaderboard-defeat.png", 1200, 720);
        Button(ui, "ReturnToMenuButton").onClick.Invoke();
        yield return WaitMenu();
        Check(store.Entries.Count == 1 && store.Entries[0].points == 1250 && store.Entries[0].playerName == "João Silva", "Returning records the final snapshot with the entered name");
        Check(Time.timeScale == 1 && !AudioListener.pause, "Time and audio restore at menu");
        yield return new WaitForSecondsRealtime(4f);
        menu = Object.FindAnyObjectByType<MainMenuController>();
        Keys(Key.Enter); menu.PlayGame(); yield return null; yield return null;
        name = Object.FindAnyObjectByType<PlayerNameView>(); input = name.GetComponentInChildren<TMP_InputField>(true);
        Check(input.text == "João Silva", "Next run remembers the last name");
        Check(name.IsVisible && Object.FindAnyObjectByType<GameplayLoadingController>() == null, "Held menu Enter cannot auto-submit a remembered name");
        Keys(); yield return null;
        input.text = "Arthur"; Button(name, "StartButton").onClick.Invoke(); yield return null;
        loading = Object.FindAnyObjectByType<GameplayLoadingController>();
        deadline = Time.realtimeSinceStartup + 60;
        while (loading != null && loading.State == GameplayLoadingController.LoadingState.Loading && Time.realtimeSinceStartup < deadline) yield return null;
        Keys(); yield return null; yield return null; Keys(Key.Enter); yield return null; Keys(); yield return new WaitForSecondsRealtime(.6f);
        waves = Object.FindAnyObjectByType<WaveController>();
        Check(waves.Score.Points == 0 && waves.CurrentWave == 1, "A second named run resets the score");
        waves.Score.AwardPoints(5400);
        typeof(WaveController).GetMethod("EndRun", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(waves, new object[] { true });
        Check(waves.Phase == WaveController.RunPhase.Completed, "Victory uses the completed-result path");
        var pause = Object.FindAnyObjectByType<PauseController>(); pause.PauseGame(); pause.ReturnToMenu();
        yield return WaitMenu();
        Check(store.Entries.Count == 2 && store.Entries[0].won && store.Entries[0].points == 5400, "Victory saves from pause and sorts ahead of defeat");
        LeaderboardSession.SaveCompletedRun(); Check(store.Entries.Count == 2, "Repeated menu save does not duplicate the victory");
        for (int i = 0; i < 14; i++) store.Add(new LeaderboardEntry { runId = "page" + i, playerName = "JOGADOR " + i,
            points = 5000 - i * 50, kills = 0, finishedUtcTicks = DateTime.UtcNow.Ticks + i });
        yield return new WaitForSecondsRealtime(4f);
        menu = Object.FindAnyObjectByType<MainMenuController>(); menu.ShowLeaderboard(); yield return null;
        ranking = Object.FindAnyObjectByType<LeaderboardView>();
        Check(ranking.Page == 0 && ranking.PageCount == 3, "Sixteen entries produce three pages");
        Preview("leaderboard-filled.png", 1200, 720);
        Preview("leaderboard-filled-wide.png", 1920, 1080);
        Preview("leaderboard-filled-small.png", 800, 600);
        Button(ranking, "NextButton").onClick.Invoke(); Check(ranking.Page == 1, "Next moves to second page");
        Button(ranking, "NextButton").onClick.Invoke(); Check(ranking.Page == 2 && !Button(ranking, "NextButton").interactable, "Last page disables next");
        var allText = ranking.GetComponentsInChildren<TMP_Text>(true);
        Check(allText.First(t => t.name == "Position0").text == "15" && allText.First(t => t.name == "Name2").text == "", "Last page has global positions and clears unused rows");
        Button(ranking, "PreviousPage").onClick.Invoke(); Check(ranking.Page == 1, "Previous arrow returns to second page");
        Button(ranking, "BackButton").onClick.Invoke(); Check(!ranking.IsVisible, "Voltar returns to menu from any page");
        Check(new LeaderboardStore(folder).Entries.Count == 16, "All runtime results persist across reload");
        menu.PlayGame(); yield return null; name = Object.FindAnyObjectByType<PlayerNameView>(); name.Close();
        Check(menu.GetComponent<CanvasGroup>().interactable && store.Entries.Count == 16, "Cancel restores menu without creating a result");
        // Check abandoned runs independently of the completed-result path.
        Check(LeaderboardSession.Prepare("Abandono"), "Another run can be prepared");
        LeaderboardSession.StartRun(); LeaderboardSession.Cancel(); LeaderboardSession.Complete(99999, 100, false);
        LeaderboardSession.SaveCompletedRun(); Check(store.Entries.Count == 16, "Abandoned run cannot create a score");
    }

    private static IEnumerator WaitMenu()
    {
        float deadline = Time.realtimeSinceStartup + 30;
        while (SceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity" && Time.realtimeSinceStartup < deadline) yield return null;
        if (SceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity") throw new Exception("Menu return timed out");
        yield return null;
    }
    private static Button Button(Component root, string name) => root.GetComponentsInChildren<Button>(true).First(button => button.name == name);
    private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("LEADERBOARD CHECK FAILED: " + description);
        Debug.Log("PASS: " + description);
    }
    private static void Preview(string filename, int width, int height)
    {
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>().Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
        var target = new RenderTexture(width, height, 24);
        var oldTarget = camera.targetTexture; float oldAspect = camera.aspect; var oldActive = RenderTexture.active;
        camera.targetTexture = target; camera.aspect = (float)width / height;
        Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
        File.WriteAllBytes(Path.GetFullPath("Logs/" + filename), image.EncodeToPNG());
        camera.targetTexture = oldTarget; camera.aspect = oldAspect; RenderTexture.active = oldActive;
        foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
        Object.Destroy(image); Object.Destroy(target);
    }
}
#endif
