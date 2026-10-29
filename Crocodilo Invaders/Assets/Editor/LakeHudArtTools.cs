using UnityEditor;
using UnityEngine;

public static class LakeHudArtTools
{
    public static void Import()
    {
        ImportSprite("Assets/Resources/LakeHudV05/hud-frame.png");
        Debug.Log("LAKE_HUD_V05_ART_IMPORTED");
    }
    public static void ImportExpressions()
    {
        Import();
        ImportSprite("Assets/Resources/LakeExpressionsV06/sapo-closed.png");
        ImportSprite("Assets/Resources/LakeExpressionsV06/hud-closed.png");
        var root=PrefabUtility.LoadPrefabContents(LakeStageTools.BossPath);
        try
        {
            if(root.GetComponent<LakeFrogExpressions>()==null)root.AddComponent<LakeFrogExpressions>();
            PrefabUtility.SaveAsPrefabAsset(root,LakeStageTools.BossPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("LAKE_EXPRESSIONS_V06_IMPORTED");
    }
    private static void ImportSprite(string path)
    {
        AssetDatabase.Refresh();
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }
}
