using UnityEditor;
using UnityEngine;

public static class LakeTongueArtTools
{
    private const string Root="Assets/Resources/LakeTongueV08/";
    public static void Install()
    {
        AssetDatabase.Refresh();
        Import(Root+"sapo-mouth.png",300,new Vector2(.5f,.38f),false);
        Import(Root+"fly-flight.png",500,Vector2.one*.5f,true);
        Import(Root+"tongue-tip.png",1000,Vector2.one*.5f,true);
        string path=Root+"art.asset";
        var art=AssetDatabase.LoadAssetAtPath<LakeTongueAssets>(path);
        if(art==null) { art=ScriptableObject.CreateInstance<LakeTongueAssets>();AssetDatabase.CreateAsset(art,path); }
        foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(path))if(sub is Sprite)Object.DestroyImmediate(sub,true);
        art.openMouth=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"sapo-mouth.png");
        var sheet=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"fly-flight.png");var pixels=sheet.GetPixels32();
        art.flyFrames=new Sprite[6];
        int w=sheet.width/3,h=sheet.height/2;
        for(int i=0;i<6;i++)
        {
            int x=(i%3)*w,y=(1-i/3)*h;double sx=0,sy=0;int n=0;
            for(int py=0;py<h;py++)for(int px=0;px<w;px++)
            {
                Color32 c=pixels[(y+py)*sheet.width+x+px];
                if(c.a>200 && c.r>150 && c.g<125 && c.b<110) { sx+=px;sy+=py;n++; }
            }
            var pivot=n>0?new Vector2((float)(sx/n)/w-.15f,(float)(sy/n)/h):Vector2.one*.5f;
            var frame=Sprite.Create(sheet,new Rect(x,y,w,h),pivot,500,0,SpriteMeshType.FullRect);
            frame.name="fly_flight_"+i;AssetDatabase.AddObjectToAsset(frame,art);art.flyFrames[i]=frame;
        }
        var tipTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"tongue-tip.png");
        var bounds=OpaqueBounds(tipTexture);
        art.tip=Sprite.Create(tipTexture,bounds,new Vector2(1,.5f),bounds.width/.4f,0,SpriteMeshType.FullRect);
        art.tip.name="sticky_tongue_tip";AssetDatabase.AddObjectToAsset(art.tip,art);
        art.ribbonMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat");
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        Import(Root+"fly-flight.png",500,Vector2.one*.5f,false);
        Import(Root+"tongue-tip.png",1000,Vector2.one*.5f,false);
        var root=PrefabUtility.LoadPrefabContents(LakeStageTools.BossPath);
        try
        {
            var combat=root.GetComponent<LakeTongueAttack>();if(combat==null)combat=root.AddComponent<LakeTongueAttack>();
            JungleStageTools.Set(combat,"art",art);PrefabUtility.SaveAsPrefabAsset(root,LakeStageTools.BossPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        if(System.IO.File.Exists("Assets/Resources/LakeTongueV09/tongue-full.png"))LakeTongueSkinArtTools.Install();
        LakeTongueFieldTools.Apply();
        Debug.Log("LAKE_TONGUE_V08_INSTALLED");
    }
    private static Rect OpaqueBounds(Texture2D texture)
    {
        var pixels=texture.GetPixels32();int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
        for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>160)
        { minX=Mathf.Min(x,minX);minY=Mathf.Min(y,minY);maxX=Mathf.Max(x,maxX);maxY=Mathf.Max(y,maxY); }
        return Rect.MinMaxRect(minX,minY,maxX+1,maxY+1);
    }
    private static void Import(string path,float ppu,Vector2 pivot,bool readable)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
        importer.alphaIsTransparency=true;importer.isReadable=readable;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=9;settings.spritePivot=pivot;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
    }
}
