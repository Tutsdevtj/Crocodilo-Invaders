using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniEyesV18Tools
{
    [Serializable] private class Cel {public string form,source,path;public int frame;}
    [Serializable] private class Layout {public Cel[] entries;}
    public static void Install()
    {
        AssetDatabase.Refresh();var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var fields=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());
            foreach(var cel in JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/MacaquiniEyes_v18/processing.json")).entries)
            {
                var source=(TextureImporter)AssetImporter.GetAtPath(cel.source);var importer=(TextureImporter)AssetImporter.GetAtPath(cel.path);
                var settings=new TextureImporterSettings();source.ReadTextureSettings(settings);importer.SetTextureSettings(settings);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=source.spritePixelsPerUnit;
                importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();
                var original=AssetDatabase.LoadAssetAtPath<Sprite>(cel.source);var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(cel.path);
                if(original.pivot!=sprite.pivot||original.pixelsPerUnit!=sprite.pixelsPerUnit||original.rect!=sprite.rect)throw new InvalidOperationException("Registration changed: "+cel.path);
                fields.FindProperty(cel.form).GetArrayElementAtIndex(cel.frame).objectReferenceValue=sprite;
            }
            fields.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_EYES_V18_INSTALL_OK 12 localized emerald-eye corrections; chest, registration and 80PPU preserved");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Olhos-v18/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_EYES_V18_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
