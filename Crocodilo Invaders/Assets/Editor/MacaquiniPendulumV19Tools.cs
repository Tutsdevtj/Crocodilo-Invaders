using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniPendulumV19Tools
{
    public static void Install()
    {
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var fields=new SerializedObject(root.GetComponent<MacaquiniCombat>());
            fields.FindProperty("pendulumEnabled").boolValue=true;
            fields.FindProperty("pendulumWhip").objectReferenceValue=new SerializedObject(root.GetComponent<MacaquiniPresentation>()).FindProperty("whipSound").objectReferenceValue;
            fields.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_PENDULUM_V19_INSTALL_OK phase two adds warned whole-arena pendulum, dash crossing, low-health return");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Cipos-v19/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_PENDULUM_V19_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
