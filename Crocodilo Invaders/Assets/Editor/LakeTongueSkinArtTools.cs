using UnityEditor;
using UnityEngine;

public static class LakeTongueSkinArtTools
{
    private const string Root="Assets/Resources/LakeTongueV09/";
    public static void Install()
    {
        AssetDatabase.Refresh();string imagePath=Root+"tongue-full.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(imagePath);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.maxTextureSize=4096;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.isReadable=true;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);var pixels=texture.GetPixels32();
        int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
        for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>160)
        { minX=Mathf.Min(x,minX);minY=Mathf.Min(y,minY);maxX=Mathf.Max(x,maxX);maxY=Mathf.Max(y,maxY); }
        Rect crop=Rect.MinMaxRect(minX,minY,maxX+1,maxY+1);
        var data=AssetDatabase.LoadAssetAtPath<LakeTongueSkinData>(Root+"skin.asset");
        if(data==null) { data=ScriptableObject.CreateInstance<LakeTongueSkinData>();AssetDatabase.CreateAsset(data,Root+"skin.asset"); }
        foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(Root+"skin.asset"))if(sub is Sprite)Object.DestroyImmediate(sub,true);
        data.drawing=Sprite.Create(texture,crop,Vector2.one*.5f,crop.height/.38f,0,SpriteMeshType.FullRect);
        data.drawing.name="complete_frog_tongue";AssetDatabase.AddObjectToAsset(data.drawing,data);
        var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"tongue-full.mat");
        if(material==null) { material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,Root+"tongue-full.mat"); }
        material.mainTexture=texture;material.color=Color.white;EditorUtility.SetDirty(material);
        data.material=material;data.height=.38f;data.rootLength=.32f;data.tipLength=.48f;data.rootUv=.84f;data.tipUv=.24f;
        EditorUtility.SetDirty(data);
        var art=AssetDatabase.LoadAssetAtPath<LakeTongueAssets>("Assets/Resources/LakeTongueV08/art.asset");
        if(art==null)throw new System.InvalidOperationException("Install the v08 tongue assets first.");
        art.skin=data;EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        importer.isReadable=false;importer.SaveAndReimport();AssetDatabase.SaveAssets();
        Debug.Log("LAKE_TONGUE_FIT_V09_INSTALLED");
    }
}
