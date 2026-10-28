using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniContinuityV13Tools
{
    public const string Art="Assets/Art/JungleStage/Macaquini_v13/";
    [Serializable] private class Cel {public string name,source,vineHand;public int cell;public float ppu,start;public Vector2 pivot,muzzle,vineTop;public bool headExposed,packAttached,hasVine;}
    [Serializable] private class Form {public string form;public Cel[] frames;}
    [Serializable] private class Layout {public Form[] forms;public Motion flightMotion;public Handover handover;}
    [Serializable] private class Handover {public float ratio;}
    [Serializable] private class Motion {public float maxFootPixelsAt80PPU;}
    private static Layout Read()=>JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/Macaquini_v13/layout.json"));
    public static bool ValidateSources()
    {
        var layout=Read();if(layout.flightMotion.maxFootPixelsAt80PPU>4.5f)throw new InvalidOperationException("Unstable flight feet");
        if(layout.handover==null||Mathf.Abs(layout.handover.ratio-1)>.005f)throw new InvalidOperationException("Scale pop at vine handover");
        var sequence=layout.forms.Single(f=>f.form=="transform").frames;bool head=false,packGone=false,vine=false;float last=-1;
        foreach(var cel in sequence){
            if(cel.start<=last||head&&!cel.headExposed||packGone&&cel.packAttached||vine&&!cel.hasVine)throw new InvalidOperationException("Transformation regresses: "+cel.name);
            if(cel.source=="catch"&&(cel.cell==0||cel.cell==11)||!string.IsNullOrEmpty(cel.vineHand)&&cel.vineHand!="screen-right")throw new InvalidOperationException("Rejected/crossed hand pose: "+cel.name);
            last=cel.start;head|=cel.headExposed;packGone|=!cel.packAttached;vine|=cel.hasVine;
        }
        if(!head||!packGone||!vine||sequence.Length!=42)throw new InvalidOperationException("Incomplete transformation");return true;
    }
    public static void Install()
    {
        ValidateSources();AssetDatabase.Refresh();var layout=Read();var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);var data=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());
        foreach(var form in layout.forms){
            bool flight=form.form=="flight";var sprites=data.FindProperty(flight?"flight":"transformation");sprites.arraySize=form.frames.Length;
            var muzzles=data.FindProperty("flightMuzzles");if(flight)muzzles.arraySize=form.frames.Length;
            var starts=data.FindProperty("transformationStarts");var vines=data.FindProperty("transformationVines");if(!flight){starts.arraySize=vines.arraySize=form.frames.Length;data.FindProperty("transformationVineStart").intValue=Array.FindIndex(form.frames,c=>c.hasVine);}
            for(int i=0;i<form.frames.Length;i++){
                var cel=form.frames[i];string path=Art+cel.name;var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=cel.ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(cel.pivot.x/512,1-cel.pivot.y/512);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
                sprites.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(flight)muzzles.GetArrayElementAtIndex(i).vector2Value=cel.muzzle;else{starts.GetArrayElementAtIndex(i).floatValue=cel.start;vines.GetArrayElementAtIndex(i).vector2Value=cel.vineTop;}
            }
        }
        data.FindProperty("chronologicalTransformation").boolValue=true;data.FindProperty("motionFramesPerSecond").floatValue=20;data.FindProperty("closedFramesPerSecond").floatValue=20;data.ApplyModifiedPropertiesWithoutUndo();
        root.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"flight_00.png");
        PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_V13_INSTALL_OK infected banana, 32 chronological flight poses,42 tear poses, same vine hand");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Infeccao-v13/MacaquiniInfeccao.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_V13_BUILD "+report.summary.result+" "+report.summary.totalSize);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
