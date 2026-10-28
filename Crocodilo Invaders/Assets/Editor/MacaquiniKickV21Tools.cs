using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniKickV21Tools
{
    public static void Validate(){SessionState.SetBool("Crocodilo.Jetpack.Validation.Effects21",true);MacaquiniJetpackValidation.Run();}
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Chute-v21/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_KICK_V21_BUILD "+report.summary.result);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
