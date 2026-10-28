using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniKickV20Tools
{
    public static void Install()
    {
        AssetDatabase.Refresh();var poses=new Sprite[6];
        for(int i=0;i<poses.Length;i++)
        {
            string path="Assets/Art/JungleStage/Macaquini_v20/kick_"+i.ToString("D2")+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=80;importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(270f/512,1-322f/512);importer.SetTextureSettings(settings);importer.SaveAndReimport();
            poses[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var kick=root.GetComponent<MacaquiniSwingKick>();if(kick==null)kick=root.AddComponent<MacaquiniSwingKick>();
            var data=new SerializedObject(kick);var frames=data.FindProperty("poses");frames.arraySize=6;
            for(int i=0;i<6;i++)frames.GetArrayElementAtIndex(i).objectReferenceValue=poses[i];
            data.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();Debug.Log("MACAQUINI_KICK_V20_INSTALL_OK");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Chute-v20/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_KICK_V20_BUILD "+report.summary.result);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
