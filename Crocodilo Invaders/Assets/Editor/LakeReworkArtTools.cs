using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LakeReworkArtTools
{
    private const string Root="Assets/Resources/LakeReworkV26/";
    public static void Install()
    {
        AssetDatabase.Refresh();
        var art=AssetDatabase.LoadAssetAtPath<LakeReworkArt>(Root+"art.asset");
        if(art==null){art=ScriptableObject.CreateInstance<LakeReworkArt>();AssetDatabase.CreateAsset(art,Root+"art.asset");}
        foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(Root+"art.asset").OfType<Sprite>())Object.DestroyImmediate(sprite,true);
        var wings=Import("right-wings.png");var pixels=wings.GetPixels32();art.rightWings=new Sprite[8];
        for(int row=0;row<2;row++)
        {
            int cellW=wings.width/4,cellH=wings.height/2,minX=cellW,minY=cellH,maxX=0,maxY=0;
            for(int col=0;col<4;col++)for(int y=0;y<cellH;y++)for(int x=0;x<cellW;x++)
            {
                if(pixels[((1-row)*cellH+y)*wings.width+col*cellW+x].a<180)continue;
                minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
            }
            int width=maxX-minX+1,height=maxY-minY+1;
            for(int col=0;col<4;col++)
                art.rightWings[row*4+col]=Add(art,wings,new Rect(col*cellW+minX,(1-row)*cellH+minY,width,height),Vector2.one*.5f,width/(row==0?.85f:1.5f),"RightWing_"+row+"_"+col);
        }
        var angry=Import("angry-boss.png");
        var original=AssetDatabase.LoadAssetAtPath<GameObject>(LakeStageTools.BossPath).GetComponentInChildren<SpriteRenderer>().sprite;
        if(angry.width!=original.texture.width || angry.height!=original.texture.height)throw new InvalidOperationException("Angry frame must preserve the source canvas");
        art.angryBoss=Add(art,angry,original.rect,original.pivot/original.rect.size,original.pixelsPerUnit,"SapeneuFurious");
        var wife=Resources.Load<SapererecaAssets>("SapererecaV12/art").frames[0];var r=wife.rect;
        art.surfaceEyes=Add(art,wife.texture,new Rect(r.x+r.width*.055f,r.y+r.height*.68f,r.width*.56f,r.height*.3f),new Vector2(.5f,.10f),wife.pixelsPerUnit,"SapererecaSurfaceEyes");
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("LAKE_REWORK_ART_OK rightWings=8 angry=registered eyes=source_crop");
    }
    private static Sprite Add(LakeReworkArt art,Texture2D texture,Rect rect,Vector2 pivot,float ppu,string name)
    {
        var sprite=Sprite.Create(texture,rect,pivot,ppu,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,art);return sprite;
    }
    private static Texture2D Import(string name)
    {
        var i=(TextureImporter)AssetImporter.GetAtPath(Root+name);i.textureType=TextureImporterType.Default;i.isReadable=true;
        i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
        i.npotScale=TextureImporterNPOTScale.None;i.alphaIsTransparency=true;i.maxTextureSize=2048;i.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(Root+name);
    }
}
