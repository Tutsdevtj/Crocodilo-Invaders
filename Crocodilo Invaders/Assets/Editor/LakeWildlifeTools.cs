using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LakeWildlifeTools
{
    private const string Root="Assets/Resources/LakeWildlifeV11/";
    [MenuItem("Crocodilo/Lago do Pneu/Install wildlife v11")]
    public static void Install()
    {
        AssetDatabase.Refresh();Import("fish-swim.png",true);Import("piranha.png",true);
        string path=Root+"art.asset";var art=AssetDatabase.LoadAssetAtPath<LakeWildlifeAssets>(path);
        if(art==null){art=ScriptableObject.CreateInstance<LakeWildlifeAssets>();AssetDatabase.CreateAsset(art,path);}
        foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(path))if(sub is Sprite)Object.DestroyImmediate(sub,true);
        art.fishFrames=Slice(art,"fish-swim.png",2,2,1.4f);
        art.piranhaFrames=Slice(art,"piranha.png",3,2,.9f);
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        Import("fish-swim.png",false);Import("piranha.png",false);
        foreach(string scenePath in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(scenePath);var lake=Object.FindAnyObjectByType<LakeBackdrop>();
            if(lake==null)throw new System.InvalidOperationException("Missing lake in "+scenePath);
            var wildlife=lake.GetComponent<LakeWildlife>();if(wildlife==null)wildlife=lake.gameObject.AddComponent<LakeWildlife>();
            JungleStageTools.Set(wildlife,"art",art);lake.enabled=false;lake.enabled=true;
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);Debug.Log("LAKE_WILDLIFE_V11_INSTALLED");
    }
    private static Sprite[] Slice(LakeWildlifeAssets art,string name,int cols,int rows,float width)
    {
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+name);var pixels=texture.GetPixels32();
        int w=texture.width/cols,h=texture.height/rows;var frames=new Sprite[cols*rows];
        for(int i=0;i<frames.Length;i++)
        {
            int x=i%cols*w,y=(rows-1-i/cols)*h,n=0;double sx=0,sy=0;
            for(int py=0;py<h;py++)for(int px=0;px<w;px++)
            {
                var c=pixels[(y+py)*texture.width+x+px];
                if(c.a>200 && c.r>150 && c.g>105 && c.g<230 && c.b<130 && c.r>c.g*1.2f){sx+=px;sy+=py;n++;}
            }
            Vector2 pivot=n>0?new Vector2((float)(sx/n)/w-.22f,(float)(sy/n)/h):Vector2.one*.5f;
            frames[i]=Sprite.Create(texture,new Rect(x,y,w,h),pivot,w/width,0,SpriteMeshType.FullRect);
            frames[i].name=name.Replace(".png","")+"_"+i;AssetDatabase.AddObjectToAsset(frames[i],art);
        }
        return frames;
    }
    private static void Import(string name,bool readable)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(Root+name);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=4096;importer.alphaIsTransparency=true;importer.isReadable=readable;importer.SaveAndReimport();
    }
}
