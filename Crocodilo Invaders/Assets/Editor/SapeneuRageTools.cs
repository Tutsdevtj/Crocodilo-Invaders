using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class SapeneuRageTools
{
    private const string Root="Assets/Resources/SapererecaV12/";
    [MenuItem("Crocodilo/Lago do Pneu/Install Sapeneu and Saperereca v12")]
    public static void Install()
    {
        AssetDatabase.Refresh();EnsureLayer("LakeForeground",1201220537);EnsureLayer("LakeHUD",1201220538);
        string sheet=Root+"saperereca.png";var importer=(TextureImporter)AssetImporter.GetAtPath(sheet);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency=true;importer.maxTextureSize=4096;importer.isReadable=true;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(sheet);var pixels=texture.GetPixels32();
        string path=Root+"art.asset";var art=AssetDatabase.LoadAssetAtPath<SapererecaAssets>(path);
        if(art==null){art=ScriptableObject.CreateInstance<SapererecaAssets>();AssetDatabase.CreateAsset(art,path);}
        foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(path))if(sub is Sprite)Object.DestroyImmediate(sub,true);
        art.frames=new Sprite[6];int w=texture.width/3,h=texture.height/2;
        for(int i=0;i<6;i++)
        {
            int cx=i%3*w,cy=(1-i/3)*h,minX=w,minY=h,maxX=0,maxY=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[(cy+y)*texture.width+cx+x].a>200)
            {minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
            minX=Mathf.Max(0,minX-2);minY=Mathf.Max(0,minY-2);maxX=Mathf.Min(w-1,maxX+2);maxY=Mathf.Min(h-1,maxY+2);
            art.frames[i]=Sprite.Create(texture,new Rect(cx+minX,cy+minY,maxX-minX+1,maxY-minY+1),new Vector2(.5f,.03f),w/1.85f,0,SpriteMeshType.FullRect);
            art.frames[i].name="Saperereca_"+i;AssetDatabase.AddObjectToAsset(art.frames[i],art);
        }
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();importer.isReadable=false;importer.SaveAndReimport();
        string materialPath=Root+"water-contact.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Crocodilo/LakeWaterContact"));AssetDatabase.CreateAsset(material,materialPath);}
        var prefab=PrefabUtility.LoadPrefabContents(LakeStageTools.BossPath);
        try{JungleStageTools.Set(prefab.GetComponent<Boss>(),"displayName","SAPENEU CURURU");PrefabUtility.SaveAsPrefabAsset(prefab,LakeStageTools.BossPath);}
        finally{PrefabUtility.UnloadPrefabContents(prefab);}
        foreach(string scenePath in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(scenePath);var lake=Object.FindAnyObjectByType<LakeBackdrop>();lake.enabled=false;lake.enabled=true;
            foreach(var boss in Object.FindObjectsByType<Boss>())if(boss.GetComponent<SapoDoPneuCombat>()!=null)JungleStageTools.Set(boss,"displayName","SAPENEU CURURU");
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))if(canvas.isRootCanvas || canvas.overrideSorting)
            {
                var original=PrefabUtility.GetCorrespondingObjectFromSource(canvas);
                canvas.sortingLayerID=original!=null?original.sortingLayerID:0;
                if(canvas.sortingOrder>=32000)canvas.sortingOrder=original!=null?original.sortingOrder:0;
                EditorUtility.SetDirty(canvas);
                PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
            }
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);Debug.Log("SAPENEU_SAPERERECA_V12_INSTALLED");
    }
    private static void EnsureLayer(string name,int id)
    {
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers=tags.FindProperty("m_SortingLayers");
        for(int i=0;i<layers.arraySize;i++)if(layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue==name)return;
        layers.InsertArrayElementAtIndex(layers.arraySize);var layer=layers.GetArrayElementAtIndex(layers.arraySize-1);
        layer.FindPropertyRelative("name").stringValue=name;layer.FindPropertyRelative("uniqueID").intValue=id;
        layer.FindPropertyRelative("locked").boolValue=false;tags.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();
    }
}
