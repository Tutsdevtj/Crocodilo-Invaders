using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniBananaV25Tools
{
    public static void Import() => MacaquiniHelpersV24Tools.Import();
    public static void Build()
    {
        string path = Path.GetFullPath("Builds/Windows-Macaquini-Combate-v25/MacaquiniSelva.exe"); Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { MacaquiniJetpackTools.Test }.Concat(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)).Distinct().ToArray(),
            locationPathName = path, target = BuildTarget.StandaloneWindows64, options = BuildOptions.CompressWithLz4HC
        });
        Debug.Log("MACAQUINI_BANANA_V25_BUILD " + report.summary.result);
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
