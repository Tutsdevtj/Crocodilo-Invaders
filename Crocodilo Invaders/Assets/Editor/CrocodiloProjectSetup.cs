using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// A Unity cria a cena versionada na primeira importação e inclui-a no build.
// Assim o projeto pode ser aberto direto pelo Hub, sem passos manuais de configuração.
public static class CrocodiloProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Jogo.unity";
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/Tests/PlayerMovementTest.unity";

    [InitializeOnLoadMethod]
    private static void ScheduleSetup()
    {
        EditorApplication.delayCall += EnsureSceneAndSettings;
    }

    private static void EnsureSceneAndSettings()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        PlayerSettings.companyName = "Arthur Reis";
        PlayerSettings.productName = "Crocodilo Invaders";
        PlayerSettings.defaultScreenWidth = 1200;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.WebGL.template = "PROJECT:Crocodilo";
        PlayerSettings.WebGL.decompressionFallback = true;

        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject cameraObject = new GameObject("Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0, 0, -10);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
        }

        var scenes = new List<EditorBuildSettingsScene>();
        foreach (string path in new[] { MenuScenePath, GameplayScenePath, ScenePath })
            if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
        // Preserva outras cenas registradas pelo projeto, sem voltar ao build só do port.
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (!scenes.Any(existing => existing.path == scene.path)) scenes.Add(scene);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    [MenuItem("Crocodilo/Preparar cena")]
    public static void PrepareScene()
    {
        EnsureSceneAndSettings();
        EditorSceneManager.OpenScene(File.Exists(MenuScenePath) ? MenuScenePath : ScenePath);
    }

    [MenuItem("Crocodilo/Build WebGL")]
    public static void BuildWebGL()
    {
        EnsureSceneAndSettings();
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Debug.LogError("Instale o módulo WebGL Build Support no Unity Hub.");
            return;
        }

        const string output = "Builds/WebGL";
        Directory.CreateDirectory(output);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("WebGL gerado em unity/Builds/WebGL");
        else
            Debug.LogError("Build WebGL falhou; verifique o Console da Unity.");
    }
}
