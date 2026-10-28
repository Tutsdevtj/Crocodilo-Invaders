using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniVineV28Tools
{
    public static void Import()
    {
        AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Art/MacaquiniVineV28/vine_bark.png");
        importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.wrapModeU=TextureWrapMode.Clamp;importer.wrapModeV=TextureWrapMode.Repeat;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();
    }
    public static void Build()
    {
        string path=Path.GetFullPath("Builds/Windows-Macaquini-Cipo-v28/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{MacaquiniFighterV22Tools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray(),
            locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_VINE_V28_BUILD "+report.summary.result);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
