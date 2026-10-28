using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniTransformationV12Tools
{
    public const string Art="Assets/Art/JungleStage/Macaquini_v12/";
    [Serializable] private class Cel {public string name;public Vector2 muzzle;}
    [Serializable] private class Form {public string form;public int canvas,ppu;public Vector2 pivot;public Cel[] frames;}
    [Serializable] private class Debris {public string name;public int ppu,canvas;}
    [Serializable] private class Layout {public Form[] forms;public Debris[] debris;}
    [Serializable] private class Quality {public int[] flightOrder;public MotionStats v12;}
    [Serializable] private class MotionStats {public float meanAdjacentFootPixels,maxAdjacentFootPixels;}
    public static void Install()
    {
        AssetDatabase.Refresh();var layout=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/Macaquini_v12/layout.json"));
        var quality=JsonUtility.FromJson<Quality>(File.ReadAllText("SourceArt/Macaquini_v12/quality.json"));
        if(quality.flightOrder.Length!=32||quality.flightOrder.Distinct().Count()!=32||quality.v12.maxAdjacentFootPixels>4.5f)throw new InvalidOperationException("Flight continuity quality gate failed");
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);var data=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());
        foreach(var form in layout.forms){var frames=form.form=="flight"?quality.flightOrder.Select(i=>form.frames[i]).ToArray():form.frames;var array=data.FindProperty(form.form=="flight"?"flight":"transformation");array.arraySize=frames.Length;var muzzles=data.FindProperty("flightMuzzles");if(form.form=="flight")muzzles.arraySize=frames.Length;
            for(int i=0;i<frames.Length;i++){string path=Art+frames[i].name;Import(path,form.ppu,new Vector2(form.pivot.x/form.canvas,1-form.pivot.y/form.canvas));array.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(form.form=="flight")muzzles.GetArrayElementAtIndex(i).vector2Value=frames[i].muzzle;}}
        data.FindProperty("motionFramesPerSecond").floatValue=20;data.FindProperty("closedFramesPerSecond").floatValue=20;data.FindProperty("chronologicalTransformation").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
        var component=root.GetComponent<MacaquiniTransformationPresentation>();if(component==null)component=root.AddComponent<MacaquiniTransformationPresentation>();
        foreach(var prop in layout.debris)Import(Art+prop.name,prop.ppu,new Vector2(.5f,.5f));
        JungleStageTools.Set(component,"bananaCap",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"banana_cap.png"));
        JungleStageTools.Set(component,"peelJetpack",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"peel_jetpack.png"));
        JungleStageTools.Set(component,"roar",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Boss/Macaquini/power_roar_v12.wav"));
        JungleStageTools.Set(component,"rip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Boss/Macaquini/whip.wav"));
        root.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"flight_"+quality.flightOrder[0].ToString("D2")+".png");
        PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);PrefabUtility.UnloadPrefabContents(root);
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Boss/Macaquini/Tear.anim");
        AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(MacaquiniPresentation),"tearProgress"),AnimationCurve.Linear(0,0,8,1));
        AnimationUtility.SetAnimationEvents(clip,new[]{
            new AnimationEvent{functionName="MacaquiniReleaseCap",time=1.75f},
            new AnimationEvent{functionName="MacaquiniRipEffort",time=3.25f},
            new AnimationEvent{functionName="MacaquiniReleasePack",time=5},
            new AnimationEvent{functionName="MacaquiniPowerRoar",time=5.5f},
            new AnimationEvent{functionName="MacaquiniTearComplete",time=7.999f}});
        EditorUtility.SetDirty(clip);AssetDatabase.SaveAssets();Debug.Log("MACAQUINI_V12_INSTALL_OK native debris events, continuous costume, expressive face, power roar");
    }
    private static void Import(string path,int ppu,Vector2 pivot)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Transformacao-v12/MacaquiniTransformacao.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_V12_BUILD "+report.summary.result+" "+report.summary.totalSize);if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
