using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniMotionV11Tools
{
    public const string Art="Assets/Art/JungleStage/Macaquini_v11/";
    [Serializable] private class Cel {public string name;public Vector2 muzzle,vineTop;}
    [Serializable] private class Form {public string form;public int canvas,ppu;public Vector2 pivot;public Cel[] frames;}
    [Serializable] private class Layout {public Form[] forms;}
    [MenuItem("Crocodilo/Fase 2/Jetpack/Install natural motion v11")]
    public static void Install()
    {
        AssetDatabase.Refresh();var layout=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/Macaquini_v11/layout.json"));
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);var data=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());
        foreach(var form in layout.forms){
            string spriteField=form.form=="flight"?"flight":"swinging",muzzleField=form.form=="flight"?"flightMuzzles":"swingMuzzles";
            var sprites=data.FindProperty(spriteField);sprites.arraySize=form.frames.Length;var muzzles=data.FindProperty(muzzleField);muzzles.arraySize=form.frames.Length;
            var vines=data.FindProperty("swingVines");if(form.form=="half")vines.arraySize=form.frames.Length;
            for(int i=0;i<form.frames.Length;i++){
                string path=Art+form.frames[i].name;var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=form.ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(form.pivot.x/form.canvas,1-form.pivot.y/form.canvas);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
                sprites.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(path);muzzles.GetArrayElementAtIndex(i).vector2Value=form.frames[i].muzzle;
                if(form.form=="half")vines.GetArrayElementAtIndex(i).vector2Value=form.frames[i].vineTop;
            }
        }
        data.FindProperty("naturalMotion").boolValue=true;data.FindProperty("motionFramesPerSecond").floatValue=20;
        var nozzles=data.FindProperty("jetNozzles");nozzles.arraySize=2;nozzles.GetArrayElementAtIndex(0).vector2Value=new Vector2(.65f,1);nozzles.GetArrayElementAtIndex(1).vector2Value=new Vector2(.94f,.83f);
        data.ApplyModifiedPropertiesWithoutUndo();root.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"flight_00.png");
        PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_V11_INSTALL_OK 32 flight +24 hanging drawings; continuous motion phase");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Movimento-v11/MacaquiniMovimento.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_V11_BUILD "+report.summary.result+" "+report.summary.totalSize);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
