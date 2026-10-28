using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class CrocodiloWindowsBuild
{
    private const string Menu = "Assets/Scenes/MainMenu.unity";
    private const string Gameplay = "Assets/Scenes/Tests/PlayerMovementTest.unity";

    // O mesmo método é usado localmente e pelo GameCI.
    [MenuItem("Crocodilo/Build Windows")]
    public static void Build()
    {
        var previousBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone);
        try
        {
            ValidateProject();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            string version = Argument("-buildVersion");
            if (!string.IsNullOrWhiteSpace(version)) PlayerSettings.bundleVersion = version;
            string output = Argument("-customBuildPath") ?? Path.Combine("Builds", "Windows", "CrocodiloInvaders.exe");
            if (!output.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) output = Path.Combine(output, "CrocodiloInvaders.exe");
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Player,
                options = BuildOptions.CompressWithLz4HC
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed: " + report.summary.result + " / errors: " + report.summary.totalErrors);
            if (!File.Exists(output) || !File.Exists(Path.Combine(Path.GetDirectoryName(output), "UnityPlayer.dll")))
                throw new BuildFailedException("Windows build is missing the executable or UnityPlayer.dll.");
            Debug.Log("WINDOWS BUILD SUCCEEDED: " + output + " / " + report.summary.totalSize + " bytes");
            // GameCI checks this exact signal after Unity exits; emit it only after validating the player.
            Debug.Log("Build succeeded!");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
        finally { PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, previousBackend); }
    }

    public static void ValidateProject()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length < 2 || scenes[0] != Menu || scenes[1] != Gameplay)
            throw new BuildFailedException("Build must start with MainMenu followed by PlayerMovementTest.");
        foreach (string path in new[] { "Assets/Prefabs/UI/Leaderboard.prefab", "Assets/Prefabs/UI/PlayerName.prefab" })
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) throw new BuildFailedException("Leaderboard UI is missing: " + path);
        var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCardCatalog>("Assets/Data/UpgradeCards.asset");
        GusiniPhaseTwoTools.Validate();
        GusiniPhaseTwoAudioTools.Validate();
        RogueliteValidation.ValidateCatalog(catalog);
        CardAudioPackageTools.Validate();
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Resources/Audio/GameAudio.mixer");
        if (mixer == null || Enumerable.Range(0, 3).Any(i => !mixer.GetFloat(GameAudioSettings.Parameter((AudioChannel)i), out _)))
            throw new BuildFailedException("Audio mixer must expose all three volume parameters.");
        foreach (string path in scenes)
        {
            if (!File.Exists(path)) throw new BuildFailedException("Build scene not found: " + path);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    ValidateScripts(root, path);
                    foreach (var waves in root.GetComponentsInChildren<WaveController>(true)) RogueliteValidation.ValidatePools(waves);
                }
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
        foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try { ValidateScripts(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        Debug.Log("BUILD VALIDATION PASSED: scenes, prefab scripts, cards, odds and audio mixer.");
    }

    private static void ValidateScripts(GameObject root, string path)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                throw new BuildFailedException("Missing script in " + path + " / " + child.name);
    }

    private static string Argument(string name)
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = arguments.Length - 2; i >= 0; i--)
            if (arguments[i] == name) return arguments[i + 1];
        return null;
    }
}
