using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

// A cena editável acompanha o projeto; o setup apenas configura o build.
public static class CrocodiloProjectSetup
{
    private const string ScenePath = CrocodiloSceneBuilder.ScenePath;

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

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    [MenuItem("Crocodilo/Preparar cena")]
    public static void PrepareScene()
    {
        EnsureSceneAndSettings();
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Crocodilo/Build WebGL")]
    public static void BuildWebGL()
    {
        EnsureSceneAndSettings();
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("Cena editável ausente. Use Crocodilo > Criar ou abrir cena editável.");
            return;
        }
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Debug.LogError("Instale o módulo WebGL Build Support no Unity Hub.");
            return;
        }

        const string output = "Builds/WebGL";
        Directory.CreateDirectory(output);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
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
