using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MacaquiniFramePreviewTools
{
    public const string ScenePath="Assets/Scenes/Tests/MacaquiniFrameAnimationTest.unity";
    public const string Art="Assets/Art/JungleStage/Macaquini_v08/";
    public const string Prefab="Assets/Prefabs/Boss/MacaquiniFullFramesPreview.prefab";
    [MenuItem("Crocodilo/Fase 2/Quadros/Install preview")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(Art,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=60;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,372f/384);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var root=new GameObject("MacaquiniFullFrames");root.transform.localScale=Vector3.one*1.25f;
        var view=root.AddComponent<SpriteRenderer>();view.sortingOrder=30;view.sprite=Load("closed_00");
        var actor=root.AddComponent<MacaquiniFrameAnimation>();var data=new SerializedObject(actor);
        foreach(var entry in new[]{("closedSwing","closed",16),("halfSwing","half",16),("closedAttack","closed_attack",8),("halfAttack","half_attack",8),("tearing","tear",8)})
        {
            var values=data.FindProperty(entry.Item1);values.arraySize=entry.Item3;
            for(int i=0;i<entry.Item3;i++)values.GetArrayElementAtIndex(i).objectReferenceValue=Load(entry.Item2+"_"+i.ToString("D2"));
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        WriteClip("ClosedSwing","closed",16,MacaquiniFrameAnimation.Motion.Swing);
        WriteClip("HalfSwing","half",16,MacaquiniFrameAnimation.Motion.Swing);
        WriteClip("ClosedAttack","closed_attack",8,MacaquiniFrameAnimation.Motion.Attack);
        WriteClip("HalfAttack","half_attack",8,MacaquiniFrameAnimation.Motion.Attack);
        WriteClip("Tear","tear",8,MacaquiniFrameAnimation.Motion.Tear);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Prefab);UnityEngine.Object.DestroyImmediate(root);
        var source=EditorSceneManager.OpenScene(JungleStageTools.Stage);var backdrop=JungleStageTools.Find<JungleBackdrop>(source);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        var cameraNode=new GameObject("MainCamera");cameraNode.tag="MainCamera";cameraNode.transform.position=new Vector3(0,2,-10);
        var camera=cameraNode.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.02f,.07f,.06f);cameraNode.AddComponent<AudioListener>();
        var jungle=UnityEngine.Object.Instantiate(backdrop.gameObject);jungle.name="JungleBackdrop";JungleStageTools.Set(jungle.GetComponent<JungleBackdrop>(),"gameplayCamera",camera);
        var body=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);body.transform.position=new Vector3(1.1f,6.9f,0);
        var ui=new GameObject("FramePreviewControls").AddComponent<MacaquiniFramePreviewUI>();JungleStageTools.Set(ui,"actor",body.GetComponent<MacaquiniFrameAnimation>());
        EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(source,true);AssetDatabase.SaveAssets();Validate();Debug.Log("MACAQUINI_FULL_FRAMES_INSTALL_OK 56 drawings");
    }
    public static void Validate()
    {
        if(Directory.GetFiles(Art,"*.png").Length!=56)throw new InvalidOperationException("Expected56fullframes");
        var actor=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<MacaquiniFrameAnimation>();
        if(actor.TotalDrawings!=56||actor.GetComponentsInChildren<SpriteRenderer>().Length!=1||actor.GetComponent<MacaquiniPresentation>()!=null)throw new InvalidOperationException("Preview must use one complete sprite, no modular rig");
        if(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath))throw new InvalidOperationException("Preview must remain outside campaign build settings");
        if(new DirectoryInfo(Art).GetFiles("*.png").Select(f=>Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(File.ReadAllBytes(f.FullName)))).Distinct().Count()!=56)throw new InvalidOperationException("Duplicate animation cels");
        Debug.Log("MACAQUINI_FULL_FRAMES_ASSETS_OK 56 distinct cels, one body renderer, campaign unchanged");
    }
    [MenuItem("Crocodilo/Fase 2/Quadros/Build Windows preview")]
    public static void Build()
    {
        try
        {
            Validate();string output=Path.GetFullPath("Builds/Windows-Macaquini-Quadros-v08/MacaquiniQuadros.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneWindows64,subtarget=(int)StandaloneBuildSubtarget.Player,options=BuildOptions.CompressWithLz4HC});
            if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("Preview build failed");
            Debug.Log("MACAQUINI_FRAME_PREVIEW_BUILD_OK "+report.summary.totalSize+" bytes");if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        catch(Exception error){Debug.LogException(error);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
    }
    private static Sprite Load(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
    private static void WriteClip(string name,string prefix,int count,MacaquiniFrameAnimation.Motion motion)
    {
        const string folder="Assets/Animations/Boss/MacaquiniFullFrames_v08/";Directory.CreateDirectory(folder);
        string path=folder+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}
        clip.frameRate=60;
        var indices=Enumerable.Range(0,count).ToList();bool loop=motion==MacaquiniFrameAnimation.Motion.Swing;
        if(loop)indices.AddRange(Enumerable.Range(1,count-2).Reverse());
        var keys=new System.Collections.Generic.List<ObjectReferenceKeyframe>();float time=0;
        foreach(int index in indices){keys.Add(new ObjectReferenceKeyframe{time=time,value=Load(prefix+"_"+index.ToString("D2"))});time+=MacaquiniFrameAnimation.Hold(motion,index);}
        keys.Add(new ObjectReferenceKeyframe{time=time,value=Load(prefix+"_"+(loop?0:count-1).ToString("D2"))});
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys.ToArray());
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);
    }
}
