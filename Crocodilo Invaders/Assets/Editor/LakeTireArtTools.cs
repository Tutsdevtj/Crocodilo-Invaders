using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LakeTireArtTools
{
    public static void Install()
    {
        const string root="Assets/Art/LakeRuntime/LakeTireV25/";AssetDatabase.Refresh();
        var importer=(TextureImporter)AssetImporter.GetAtPath(root+"tire.png");
        importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.isReadable=true;
        importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"tire.png");var pixels=texture.GetPixels32();
        int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
        for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>180)
        {minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
        if(minX>=maxX || minY>=maxY)throw new InvalidOperationException("Empty tire sprite");
        minX=Mathf.Max(0,minX-2);minY=Mathf.Max(0,minY-2);maxX=Mathf.Min(texture.width-1,maxX+2);maxY=Mathf.Min(texture.height-1,maxY+2);
        var art=AssetDatabase.LoadAssetAtPath<LakeTireArt>(root+"art.asset");
        if(art==null){art=ScriptableObject.CreateInstance<LakeTireArt>();AssetDatabase.CreateAsset(art,root+"art.asset");}
        foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(root+"art.asset").OfType<Sprite>())Object.DestroyImmediate(sprite,true);
        int width=maxX-minX+1,height=maxY-minY+1;
        art.drawing=Sprite.Create(texture,new Rect(minX,minY,width,height),Vector2.one*.5f,width/1.15f,0,SpriteMeshType.FullRect);
        art.drawing.name="ThrowableSwampTire";AssetDatabase.AddObjectToAsset(art.drawing,art);
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();importer.isReadable=false;importer.SaveAndReimport();
        Debug.Log("LAKE_TIRE_ART_INSTALLED rect="+art.drawing.rect+" width="+art.drawing.bounds.size.x);
    }
}
