using System;
using UnityEditor;
using UnityEngine;

public static class LakeMosquitoRegistrationTools
{
    public static void Install()
    {
        const string artPath="Assets/Art/LakeRuntime/LakeJourneyV22/art.asset";
        const string texturePath="Assets/Art/LakeRuntime/LakeJourneyV22/enemies.png";
        var art=AssetDatabase.LoadAssetAtPath<LakeJourneyArt>(artPath);
        if(art==null || art.enemies.Length<4)throw new InvalidOperationException("Missing original mosquito frames");
        var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);bool wasReadable=importer.isReadable;
        try
        {
            importer.isReadable=true;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);var pixels=texture.GetPixels32();
            var centers=new Vector2[4];art.mosquitoRegistration=new Vector2[4];
            for(int i=0;i<4;i++)
            {
                var sprite=art.enemies[i];Rect rect=sprite.rect;Vector2 sum=Vector2.zero;float weight=0;
                for(int y=Mathf.CeilToInt(rect.y);y<rect.yMax;y++)for(int x=Mathf.CeilToInt(rect.x);x<rect.xMax;x++)
                {
                    Color32 c=pixels[y*texture.width+x];
                    if(c.a<200 || c.r<135 || c.r<c.g*1.6f || c.r<c.b*1.6f)continue;
                    float w=c.r-Mathf.Max(c.g,c.b);sum+=new Vector2(x+.5f,y+.5f)*w;weight+=w;
                }
                if(weight<1)throw new InvalidOperationException("No red eye anchor in mosquito frame "+i);
                centers[i]=(sum/weight-rect.position-sprite.pivot)/sprite.pixelsPerUnit;
                art.mosquitoRegistration[i]=i==0?Vector2.zero:centers[0]-centers[i];
            }
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("MOSQUITO_REGISTRATION_OK "+string.Join(" / ",Array.ConvertAll(art.mosquitoRegistration,p=>p.ToString("F4"))));
        }
        finally{importer.isReadable=wasReadable;importer.SaveAndReimport();}
    }
}
