using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniChestV17Tools
{
    public static void Install()
    {
        AssetDatabase.Refresh();var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var fields=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());var frames=fields.FindProperty("transformation");
            for(int i=18;i<=27;i++)
            {
                string name="transform_"+i.ToString("D2")+".png",source="Assets/Art/JungleStage/Macaquini_v13/"+name,path="Assets/Art/JungleStage/Macaquini_v17/"+name;
                var old=(TextureImporter)AssetImporter.GetAtPath(source);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                var settings=new TextureImporterSettings();old.ReadTextureSettings(settings);importer.SetTextureSettings(settings);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=old.spritePixelsPerUnit;
                importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
                importer.SaveAndReimport();frames.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var a=AssetDatabase.LoadAssetAtPath<Sprite>(source);var b=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(a.pivot!=b.pivot||a.pixelsPerUnit!=b.pixelsPerUnit||a.rect!=b.rect)throw new InvalidOperationException("Sprite registration changed: "+name);
            }
            fields.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_CHEST_V17_INSTALL_OK 10 local palette edits; original pivots, 80PPU, dimensions and motion preserved");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Peito-v17/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_CHEST_V17_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
