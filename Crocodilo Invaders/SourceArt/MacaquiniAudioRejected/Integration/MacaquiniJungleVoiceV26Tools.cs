using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniJungleVoiceV26Tools
{
    public static void Import()
    {
        AssetDatabase.Refresh();
        foreach (string file in Directory.GetFiles("Assets/Resources/Audio/MacaquiniJungleVoiceV26", "*.wav"))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            importer.forceToMono = false; importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings; settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad; settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true; importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
    }
    public static void Build()
    {
        string path = Path.GetFullPath("Builds/Windows-Macaquini-Eco-Selva-v26/MacaquiniSelva.exe"); Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { MacaquiniJetpackTools.Test }.Concat(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)).Distinct().ToArray(),
            locationPathName = path, target = BuildTarget.StandaloneWindows64, options = BuildOptions.CompressWithLz4HC
        });
        Debug.Log("MACAQUINI_JUNGLE_VOICE_V26_BUILD " + report.summary.result);
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
