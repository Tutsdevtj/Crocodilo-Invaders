using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LakeStageDataTools
{
    private const string Folder="Assets/Data/Lake/";
    private const string Runtime="Assets/Art/LakeRuntime/";
    [MenuItem("Crocodilo/Lago do Pneu/Install stage data")]
    public static void Install()
    {
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
        var difficulty=GetOrCreate<LakeDifficulty>(Folder+"LakeDifficulty.asset");
        var art=GetOrCreate<LakeStageArt>(Folder+"LakeStageArt.asset");
        art.tongue=Load<LakeTongueAssets>("LakeTongueV08/art.asset");
        art.tire=Load<LakeTireArt>("LakeTireV25/art.asset");
        art.rework=Load<LakeReworkArt>("LakeReworkV26/art.asset");
        art.wildlife=Load<LakeWildlifeAssets>("LakeWildlifeV11/art.asset");
        art.companion=Load<SapererecaAssets>("SapererecaV12/art.asset");
        art.waterContact=Load<Material>("SapererecaV12/water-contact.mat");
        const string hudWaterPath="Assets/Materials/LakeHudWater.mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(hudWaterPath)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("Crocodilo/Lake HUD Water")),hudWaterPath);
        art.hudWater=AssetDatabase.LoadAssetAtPath<Material>(hudWaterPath);
        art.waterDrop=Load<Sprite>("LakeJourneyV22/water-drop.png");
        art.burp=Load<Sprite>("SapeneuBurpV13/burp.png");
        art.bossClosedEyes=Load<Sprite>("LakeExpressionsV06/sapo-closed.png");
        art.hudFrame=Load<Sprite>("LakeHudV05/hud-frame.png");
        art.hudClosedEyes=Load<Sprite>("LakeExpressionsV06/hud-closed.png");
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        foreach(string path in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(path);var lake=Object.FindAnyObjectByType<LakeBackdrop>();
            // Opening a scene unloads loose assets, so the references must be fetched after it.
            JungleStageTools.Set(lake,"difficulty",AssetDatabase.LoadAssetAtPath<LakeDifficulty>(Folder+"LakeDifficulty.asset"));
            JungleStageTools.Set(lake,"art",AssetDatabase.LoadAssetAtPath<LakeStageArt>(Folder+"LakeStageArt.asset"));
            if(new SerializedObject(lake).FindProperty("difficulty").objectReferenceValue==null)throw new InvalidOperationException("Lake difficulty did not persist");
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("LAKE_STAGE_DATA_OK");
    }
    [MenuItem("Crocodilo/Lago do Pneu/Compress lake textures")]
    public static void CompressTextures()
    {
        int count=0;
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Art/LakeRuntime","Assets/Art/LakeStage"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if(path.Contains("LilyPadsV14") || !(AssetImporter.GetAtPath(path) is TextureImporter importer))continue;
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            // Block compression needs sides divisible by four; other sizes would just fall back to raw RGBA.
            if(width%4!=0 || height%4!=0){importer.ClearPlatformTextureSettings("Standalone");importer.SaveAndReimport();continue;}
            var settings=importer.GetPlatformTextureSettings("Standalone");
            settings.overridden=true;settings.format=TextureImporterFormat.BC7;settings.compressionQuality=100;settings.maxTextureSize=importer.maxTextureSize;
            importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();count++;
        }
        Debug.Log("LAKE_TEXTURES_COMPRESSED "+count);
    }
    private static T GetOrCreate<T>(string path) where T:ScriptableObject
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}
        return asset;
    }
    private static T Load<T>(string relative) where T:Object
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(Runtime+relative);
        if(asset==null)throw new InvalidOperationException("Missing lake asset "+relative);
        return asset;
    }
}
