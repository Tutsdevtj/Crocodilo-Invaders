using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniJetpackTools
{
    public const string Art="Assets/Art/JungleStage/Macaquini_v10/";
    public const string Test="Assets/Scenes/Tests/MacaquiniJetpackTest.unity";
    [Serializable] private class Offset {public string name;public Vector2 offset;}
    [Serializable] private class Layout {public Offset[] muzzles;}
    [MenuItem("Crocodilo/Fase 2/Jetpack/Install")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(Art,"*.png")){
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            if(Path.GetFileName(path).StartsWith("fx_")){importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();continue;}
            bool half=Path.GetFileName(path).StartsWith("half");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=half?60:80;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=half?new Vector2(178f/384,184f/384):Path.GetFileName(path).StartsWith("transform")?new Vector2(.5f,.375f):new Vector2(230f/384,144f/384);
            settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        var component=root.GetComponent<MacaquiniFlightPresentation>();
        if(component==null)component=root.AddComponent<MacaquiniFlightPresentation>();
        Transform painted=root.transform.Find("CompletePaintedBody");
        if(painted==null){painted=new GameObject("CompletePaintedBody").transform;painted.SetParent(root.transform,false);}
        var view=painted.GetComponent<SpriteRenderer>();if(view==null)view=painted.gameObject.AddComponent<SpriteRenderer>();
        view.sortingOrder=30;view.sprite=Load("flight",0);
        Transform origin=root.transform.Find("PaintedBananaMuzzle");if(origin==null){origin=new GameObject("PaintedBananaMuzzle").transform;origin.SetParent(root.transform,false);}
        JungleStageTools.Set(component,"view",view);JungleStageTools.Set(component,"shotOrigin",origin);
        JungleStageTools.Set(component,"effectsMaterial",AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));
        JungleStageTools.Set(component,"engineLoop",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Boss/Macaquini/jetpack_loop_v10.wav"));
        var layout=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/Macaquini_v10/layout.json"));
        var data=new SerializedObject(component);
        data.FindProperty("chronologicalTransformation").boolValue=false;
        var materials=data.FindProperty("particleMaterials");materials.arraySize=4;
        string[] effectNames={"fx_flame","fx_blue","fx_smoke","fx_sparks"};
        for(int i=0;i<4;i++){
            string materialPath="Assets/Materials/Macaquini_"+effectNames[i]+"_v10.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));AssetDatabase.CreateAsset(material,materialPath);}
            material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+effectNames[i]+".png");EditorUtility.SetDirty(material);materials.GetArrayElementAtIndex(i).objectReferenceValue=material;
        }
        foreach(var pair in new[]{("flight","flight",16),("transformation","transform",16),("swinging","half",16),("shooting","half_attack",8)}){
            var array=data.FindProperty(pair.Item1);array.arraySize=pair.Item3;
            for(int i=0;i<pair.Item3;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=Load(pair.Item2,pair.Item2=="transform"&&i==10?11:i);
        }
        foreach(var pair in new[]{("flightMuzzles","flight",16),("swingMuzzles","half",16),("shootMuzzles","half_attack",8)}){
            var array=data.FindProperty(pair.Item1);array.arraySize=pair.Item3;
            for(int i=0;i<pair.Item3;i++)array.GetArrayElementAtIndex(i).vector2Value=layout.muzzles.First(m=>m.name==pair.Item2+"_"+i.ToString("D2")+".png").offset;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        JungleStageTools.Set(root.GetComponent<MacaquiniCombat>(),"muzzle",origin);
        var collider=root.GetComponent<BoxCollider2D>();collider.size=new Vector2(1.3f,2.2f);collider.offset=new Vector2(-.05f,.55f);
        var bossData=new SerializedObject(root.GetComponent<Boss>());bossData.FindProperty("patrolArea").rectValue=new Rect(2.6f,-.5f,1.6f,2.7f);bossData.ApplyModifiedPropertiesWithoutUndo();
        root.transform.Find("Visual").gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);PrefabUtility.UnloadPrefabContents(root);
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Boss/Macaquini/Tear.anim");
        if(clip==null)throw new InvalidOperationException("Missing native Tear clip");
        AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(MacaquiniPresentation),"tearProgress"),AnimationCurve.Linear(0,0,6.2f,1));
        AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{functionName="MacaquiniTearComplete",time=6.199f}});EditorUtility.SetDirty(clip);
        var scene=EditorSceneManager.OpenScene(JungleStageTools.Test);
        foreach(var old in UnityEngine.Object.FindObjectsByType<BossDevelopmentStart>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(old);
        foreach(var old in UnityEngine.Object.FindObjectsByType<MacaquiniDebugControls>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(old);
        new GameObject("JetpackTestControls").AddComponent<MacaquiniJetpackTestControls>();
        EditorSceneManager.SaveScene(scene,Test);AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_JETPACK_INSTALL_OK 56 complete cels; original v08 second form preserved");
    }
    private static Sprite Load(string prefix,int index)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+prefix+"_"+index.ToString("D2")+".png");
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Jetpack-v10/MacaquiniJetpack.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_JETPACK_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
