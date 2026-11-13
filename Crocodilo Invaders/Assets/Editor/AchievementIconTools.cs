using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AchievementIconTools
{
    public const string Icons="Assets/Resources/Achievements/Icons_v03";
    [MenuItem("Crocodilo/Achievements/Install unique icons and restore original ship")]
    public static void Install()
    {
        CrocsSkinTools.EquipOriginal();
        AssetDatabase.Refresh();
        foreach(var definition in AchievementCatalog.All)
        {
            string path=Icons+"/"+definition.Id+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new Exception("Missing generated icon: "+path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var root=PrefabUtility.LoadPrefabContents(AchievementTools.Prefab);
        try
        {
            var serialized=new SerializedObject(root.GetComponent<AchievementView>());
            var icons=serialized.FindProperty("icons");icons.arraySize=AchievementCatalog.All.Count;
            for(int i=0;i<icons.arraySize;i++)icons.GetArrayElementAtIndex(i).objectReferenceValue=
                AssetDatabase.LoadAssetAtPath<Sprite>(Icons+"/"+AchievementCatalog.All[i].Id+".png");
            serialized.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,AchievementTools.Prefab);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();Validate();
    }
    public static void Validate()
    {
        var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if(AssetDatabase.GetAssetPath(player.GetComponent<Animator>().runtimeAnimatorController)!="Assets/Animations/Player/Player.controller")
            throw new Exception("Original airplane controller must be active");
        if(AssetDatabase.GetAssetPath(player.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite)!=
            "Assets/Art/Player/Bombardiro/Idle/bombardiro_idle_0.png")throw new Exception("Original airplane idle must be active");
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(AchievementTools.Prefab);
        var array=new SerializedObject(root.GetComponent<AchievementView>()).FindProperty("icons");
        if(array.arraySize!=10)throw new Exception("Expected ten unique icons");
        for(int i=0;i<array.arraySize;i++)
        {
            var sprite=array.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
            string path=Icons+"/"+AchievementCatalog.All[i].Id+".png";
            if(sprite==null||AssetDatabase.GetAssetPath(sprite)!=path||sprite.rect.size!=new Vector2(64,64))
                throw new Exception("Invalid icon mapping: "+AchievementCatalog.All[i].Id);
        }
        var roots=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)
            .Concat(AssetDatabase.FindAssets("",new[]{"Assets/Resources"}).Select(AssetDatabase.GUIDToAssetPath)
                // The Hangar catalog is the only allowed path to Crocs: an opt-in cosmetic chosen by the player.
                .Where(p=>!p.StartsWith("Assets/Resources/Wardrobe")))
            .Append("Assets/Prefabs/Player.prefab").ToArray();
        foreach(string dependency in AssetDatabase.GetDependencies(roots,true))
            if(dependency.StartsWith(CrocsSkinTools.Art+"/")||dependency.StartsWith(CrocsSkinTools.Animations+"/"))
                throw new Exception("Saved Crocs skin is still used by playable content: "+dependency);
        if(Directory.GetFiles(CrocsSkinTools.Art,"*.png",SearchOption.AllDirectories).Length!=26||!File.Exists(CrocsSkinTools.Controller))
            throw new Exception("Archived Crocs assets must remain saved");
        Debug.Log("ACHIEVEMENT_ICONS_OK: ten dedicated 64px icons; original airplane active; Crocs reachable only through the Hangar catalog.");
    }
}
