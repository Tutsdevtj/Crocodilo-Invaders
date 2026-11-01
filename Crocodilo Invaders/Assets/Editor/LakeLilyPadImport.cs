using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class LakeLilyPadImport
{
    public static void Install()
    {
        const string path="Assets/Art/LakeStage/LilyPadsV16/lilypads.png";
        AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=4096;importer.spritePixelsPerUnit=256;importer.alphaIsTransparency=true;importer.isReadable=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        var factories=new SpriteDataProviderFactories();factories.Init();
        var provider=factories.GetSpriteEditorDataProviderFromObject(importer);
        if(provider==null)throw new InvalidOperationException("Sprite provider unavailable; slicing aborted");
        provider.InitSpriteEditorDataProvider();
        var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
        if(capability==null)throw new InvalidOperationException("Edit capabilities unavailable; slicing aborted");
        var caps=capability.GetEditCapability();
        foreach(var required in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteRect,EEditCapability.EditSpriteName,EEditCapability.EditPivot})
            if(!caps.HasCapability(required))throw new InvalidOperationException("Missing capability "+required+"; slicing aborted");
        var nameIds=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if(nameIds==null)throw new InvalidOperationException("Name/file ID provider unavailable; slicing aborted");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();
        if(texture.width!=1536 || texture.height!=1024)throw new InvalidOperationException("Unexpected master dimensions");
        string[] names={"lilypad_small_notched","lilypad_large_rim","lilypad_nibbled","lilypad_pink_bud","lilypad_pink_bloom","lilypad_double_cluster"};
        int[] left={0,470,1080},right={470,1080,1536};
        var previous=provider.GetSpriteRects();var rects=new SpriteRect[6];
        for(int i=0;i<6;i++)
        {
            int col=i%3,loY=i<3?512:0,hiY=loY+512,minX=right[col],maxX=left[col],minY=hiY,maxY=loY;
            for(int y=loY;y<hiY;y++)for(int x=left[col];x<right[col];x++)if(pixels[y*texture.width+x].a>180)
            {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            if(maxX<=minX || maxY<=minY)throw new InvalidOperationException("Empty region "+names[i]);
            minX=Mathf.Max(left[col],minX-4);maxX=Mathf.Min(right[col]-1,maxX+4);minY=Mathf.Max(loY,minY-4);maxY=Mathf.Min(hiY-1,maxY+4);
            var old=previous.FirstOrDefault(s=>s.name==names[i]);
            rects[i]=new SpriteRect{name=names[i],rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,.12f),spriteID=old!=null?old.spriteID:GUID.Generate()};
        }
        provider.SetSpriteRects(rects);nameIds.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply();importer.SaveAndReimport();importer.isReadable=false;importer.SaveAndReimport();AssetDatabase.SaveAssets();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if(sprites.Length!=6 || sprites.Any(s=>s.pixelsPerUnit!=256))throw new InvalidOperationException("Unexpected imported sprites");
        Directory.CreateDirectory("Logs/LagoDoPneu/v16");
        File.WriteAllLines("Logs/LagoDoPneu/v16/sprites.txt",sprites.Select(s=>s.name+" rect="+s.rect+" pivot="+s.pivot+" PPU="+s.pixelsPerUnit));
        Debug.Log("LAKE_LILYPADS_V16_IMPORTED sprites="+sprites.Length+" Point RGBA FullRect");
    }
}
