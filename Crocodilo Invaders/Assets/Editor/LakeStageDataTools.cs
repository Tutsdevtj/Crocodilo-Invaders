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
        art.dropHalo=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteHitFlash.mat");
        art.audio=BuildAudioBank();
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
    private static LakeAudioBank BuildAudioBank()
    {
        const string audio="Assets/Audio/Lake/";
        foreach(string guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio/Lake","Assets/Resources/Audio/Ambience"}))ConfigureClip(AssetDatabase.GUIDToAssetPath(guid));
        var bank=GetOrCreate<LakeAudioBank>(Folder+"LakeAudioBank.asset");
        var names=(LakeSound[])Enum.GetValues(typeof(LakeSound));
        float[] volumes={.42f,.28f,.55f,.45f,.4f,.7f,.45f,.7f,.5f,.5f,.22f,.4f,.45f,.22f,.5f,.42f,.45f,.5f,.55f,.22f,.4f,.38f,.3f,.5f,.42f};
        float[] cooldowns={.04f,.09f,.12f,.05f,.05f,.3f,.2f,.2f,.2f,.08f,.07f,.06f,.06f,.14f,1f,.5f,.06f,.8f,.6f,.3f,.15f,.4f,.1f,.06f,.2f};
        // Volumes already tuned in the Inspector survive a reinstall.
        var previous=bank.sounds??new LakeAudioBank.Entry[0];
        var entries=new LakeAudioBank.Entry[names.Length];
        for(int i=0;i<names.Length;i++)
        {
            entries[i]=Array.Find(previous,e=>e.name==names[i].ToString())??new LakeAudioBank.Entry{name=names[i].ToString(),volume=volumes[i],cooldown=cooldowns[i],pitchVariance=.05f};
            string clip=names[i]==LakeSound.Croak?"Assets/Art/LakeRuntime/LakeEntranceV17/cururu-croak.wav":audio+"SFX_Lake_"+names[i]+".wav";
            entries[i].clip=AssetDatabase.LoadAssetAtPath<AudioClip>(clip);
            if(entries[i].clip==null)throw new InvalidOperationException("Missing lake sound "+clip);
        }
        bank.sounds=entries;
        bank.mosquitoBuzz=AssetDatabase.LoadAssetAtPath<AudioClip>(audio+"SFX_Lake_MosquitoBuzz.wav");
        bank.ambience=AssetDatabase.LoadAssetAtPath<AudioClip>(audio+"AMB_Lake_Loop.wav");
        EditorUtility.SetDirty(bank);
        return bank;
    }
    private static void ConfigureClip(string path)
    {
        var importer=(AudioImporter)AssetImporter.GetAtPath(path);
        var settings=importer.defaultSampleSettings;
        settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
        importer.defaultSampleSettings=settings;importer.loadInBackground=false;importer.SaveAndReimport();
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
