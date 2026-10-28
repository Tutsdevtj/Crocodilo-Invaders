using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniFighterV22Tools
{
    public const string Test="Assets/Scenes/Tests/MacaquiniFighterTest.unity";
    [Serializable] private class Frame {public string name;public float ppu;public float[] pivot;}
    [Serializable] private class Layout {public Frame[] frames;}
    public static void Install()
    {
        AssetDatabase.Refresh();var layout=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/MacaquiniFighter_v22/layout.json"));
        foreach(var frame in layout.frames)
        {
            string path="Assets/Art/JungleStage/Macaquini_v22/"+frame.name;var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=64;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(frame.pivot[0],frame.pivot[1]);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var fighter=root.GetComponent<MacaquiniFighter>();if(fighter==null)fighter=root.AddComponent<MacaquiniFighter>();
            var fields=new SerializedObject(fighter);
            foreach(string group in new[]{"fighter","helper"})
            {
                var array=fields.FindProperty(group=="fighter"?"poses":"helperPoses");array.arraySize=6;
                for(int i=0;i<6;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/JungleStage/Macaquini_v22/"+group+"_"+i.ToString("D2")+".png");
            }
            fields.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        if(!File.Exists(Test))AssetDatabase.CopyAsset(MacaquiniJetpackTools.Test,Test);
        var scene=EditorSceneManager.OpenScene(Test);var controls=UnityEngine.Object.FindAnyObjectByType<MacaquiniJetpackTestControls>();
        var data=new SerializedObject(controls);data.FindProperty("startInFighterForm").boolValue=true;data.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_FIGHTER_V22_INSTALL_OK");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Lutador-v22/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_FIGHTER_V22_BUILD "+report.summary.result);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
