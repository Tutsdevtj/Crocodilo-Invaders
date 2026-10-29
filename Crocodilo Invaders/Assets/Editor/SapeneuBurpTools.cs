using UnityEditor;
using UnityEngine;
public static class SapeneuBurpTools
{
    [MenuItem("Crocodilo/Lago do Pneu/Import burp v13")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        string path="Assets/Resources/SapeneuBurpV13/burp.png";
        var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/LakeStage/sapo-master.png");
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
        importer.spritePixelsPerUnit=original.pixelsPerUnit;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=9;
        settings.spritePivot=new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height);
        importer.SetTextureSettings(settings);importer.SaveAndReimport();AssetDatabase.SaveAssets();
        Debug.Log("SAPENEU_BURP_V13_IMPORTED");
    }
}
