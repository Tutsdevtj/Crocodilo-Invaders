using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LakeJourneyTools
{
    private const string Root="Assets/Resources/LakeJourneyV22/";
    private const string Prefabs="Assets/Prefabs/Enemies/LakeV22/";
    [MenuItem("Crocodilo/Lago do Pneu/Install full journey v22")]
    public static void Install()
    {
        AssetDatabase.Refresh();Directory.CreateDirectory(Prefabs);
        var art=AssetDatabase.LoadAssetAtPath<LakeJourneyArt>(Root+"art.asset");
        if(art==null){art=ScriptableObject.CreateInstance<LakeJourneyArt>();AssetDatabase.CreateAsset(art,Root+"art.asset");}
        foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(Root+"art.asset").OfType<Sprite>())Object.DestroyImmediate(sprite,true);
        var texture=Import("enemies.png",true);Color32[] pixels=texture.GetPixels32();art.enemies=new Sprite[16];
        float[] widths={.85f,1.5f,1.25f,1.45f};
        for(int row=0;row<4;row++)
        {
            // The tall beetle horns extend above the nominal last grid row. Keep that silhouette
            // out of the frogs' shared rectangle, so their feet stay registered on the logs.
            float[] rowEdges={0,.25f,.52f,.72f,1};
            int y0=Mathf.RoundToInt((1-rowEdges[row+1])*texture.height),y1=Mathf.RoundToInt((1-rowEdges[row])*texture.height);
            int minX=texture.width/4,minY=y1-y0,maxX=0,maxY=0;
            for(int col=0;col<4;col++)
            {
                int x0=Mathf.RoundToInt(col*texture.width/4f),x1=Mathf.RoundToInt((col+1)*texture.width/4f);
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>160)
                {minX=Mathf.Min(minX,x-x0);maxX=Mathf.Max(maxX,x-x0);minY=Mathf.Min(minY,y-y0);maxY=Mathf.Max(maxY,y-y0);}
            }
            minX=Mathf.Max(0,minX-1);minY=Mathf.Max(0,minY-1);maxY=Mathf.Min(y1-y0-1,maxY+1);
            for(int col=0;col<4;col++)
            {
                int x0=Mathf.RoundToInt(col*texture.width/4f),x1=Mathf.RoundToInt((col+1)*texture.width/4f);
                int w=Mathf.Min(x1-x0-minX,maxX-minX+2),h=maxY-minY+1;
                var sprite=Sprite.Create(texture,new Rect(x0+minX,y0+minY,w,h),new Vector2(.5f,row==2?.07f:.5f),w/widths[row],0,SpriteMeshType.FullRect);
                sprite.name=((LakeJourneyEnemy.Species)row)+"_"+col;AssetDatabase.AddObjectToAsset(sprite,art);art.enemies[row*4+col]=sprite;
            }
        }
        var props=Import("props.png",true);var pp=props.GetPixels32();art.props=new Sprite[4];
        // The generated log is wider than half the sheet. These non-overlapping source regions keep it whole.
        Rect[] regions={new Rect(.01f,.5f,.58f,.5f),new Rect(.59f,.5f,.41f,.5f),new Rect(0,0,.51f,.5f),new Rect(.52f,0,.48f,.5f)};
        float[] propWidths={2.05f,1.6f,1.7f,1.45f};
        for(int i=0;i<4;i++)
        {
            var r=regions[i];int left=(int)(r.x*props.width),bottom=(int)(r.y*props.height),right=(int)(r.xMax*props.width),top=(int)(r.yMax*props.height);
            int minX=right,minY=top,maxX=left,maxY=bottom;
            for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(pp[y*props.width+x].a>160)
            {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            int w=maxX-minX+1,h=maxY-minY+1;
            var sprite=Sprite.Create(props,new Rect(minX,minY,w,h),new Vector2(.5f,.05f),w/propWidths[i],0,SpriteMeshType.FullRect);
            sprite.name=new[]{"SunkenLog","DiscardedTire","CattailBank","LotusFlowers"}[i];AssetDatabase.AddObjectToAsset(sprite,art);art.props[i]=sprite;
        }
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();Import("enemies.png",false);Import("props.png",false);
        Import("water-drop.png",false,1000);
        for(int i=0;i<4;i++)BuildEnemy((LakeJourneyEnemy.Species)i,art);
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        var scene=EditorSceneManager.OpenScene(LakeStageTools.Stage);
        art=AssetDatabase.LoadAssetAtPath<LakeJourneyArt>(Root+"art.asset");
        var prefabs=new Enemy[4];
        for(int i=0;i<4;i++)
        {
            prefabs[i]=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+((LakeJourneyEnemy.Species)i)+".prefab").GetComponent<Enemy>();
            if(prefabs[i]==null)throw new InvalidOperationException("Missing saved enemy "+i);
        }
        var waves=Object.FindAnyObjectByType<WaveController>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
        var director=lake.GetComponent<LakeJourneyDirector>();if(director==null)director=lake.gameObject.AddComponent<LakeJourneyDirector>();
        var scenery=lake.GetComponent<LakeJourneyScenery>();if(scenery==null)scenery=lake.gameObject.AddComponent<LakeJourneyScenery>();
        JungleStageTools.Set(scenery,"art",art);
        JungleStageTools.Set(scenery,"bossHint",AssetDatabase.LoadAssetAtPath<GameObject>(LakeStageTools.BossPath).GetComponentInChildren<SpriteRenderer>().sprite);
        JungleStageTools.Set(scenery,"font",AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset"));
        JungleStageTools.Set(scenery,"distantCroak",Resources.Load<AudioClip>("LakeEntranceV17/cururu-croak"));
        var data=new SerializedObject(waves);var pools=data.FindProperty("wavePools");pools.arraySize=9;
        // Each row introduces a threat before mixing it with the next. Three compositions per wave.
        int[][] counts={new[]{6,0,0,0},new[]{6,2,0,0},new[]{6,4,0,0},new[]{6,2,2,0},new[]{6,2,4,0},new[]{6,4,4,0},new[]{6,4,2,2},new[]{6,4,4,2},new[]{6,4,4,4}};
        for(int wave=0;wave<9;wave++)
        {
            var options=pools.GetArrayElementAtIndex(wave).FindPropertyRelative("options");options.arraySize=3;
            for(int option=0;option<3;option++)
            {
                var config=options.GetArrayElementAtIndex(option);var groups=config.FindPropertyRelative("groups");groups.arraySize=0;
                // Alternate small groups, so a late wave does not exhaust all mosquitoes before the other roles arrive.
                int[] remaining=(int[])counts[wave].Clone();remaining[0]+=option;
                while(remaining.Any(n=>n>0))for(int kind=0;kind<4;kind++)if(remaining[kind]>0)
                {
                    int index=groups.arraySize++;var group=groups.GetArrayElementAtIndex(index);int amount=Mathf.Min(2,remaining[kind]);remaining[kind]-=amount;
                    group.FindPropertyRelative("prefab").objectReferenceValue=prefabs[kind];group.FindPropertyRelative("count").intValue=amount;
                }
                config.FindPropertyRelative("spawnInterval").vector2Value=new Vector2(Mathf.Lerp(2.3f,1.55f,wave/8f),Mathf.Lerp(3.1f,2.25f,wave/8f));
                config.FindPropertyRelative("maxAlive").intValue=wave<2?4:wave<4?5:wave<6?6:7;
            }
        }
        data.FindProperty("waveComposition").arraySize=0;data.ApplyModifiedPropertiesWithoutUndo();
        JungleStageTools.Set(waves.GetComponent<EnemySpawner>(),"enemyPrefab",prefabs[0]);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        if(!waves.ValidateWaves(out string error))throw new InvalidOperationException(error);
        if(waves.CombatWaveCount!=9 || waves.WaveCount!=10)throw new InvalidOperationException("Lake journey must have nine waves and one boss");
        Debug.Log("LAKE_JOURNEY_V22_INSTALLED: 4 enemies / 27 compositions / 9 waves / 1 boss / 16 animation frames / 4 props");
    }
    private static Texture2D Import(string name,bool readable,float ppu=100)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(Root+name);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
        importer.spritePixelsPerUnit=ppu;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;importer.isReadable=readable;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(Root+name);
    }
    private static Enemy BuildEnemy(LakeJourneyEnemy.Species species,LakeJourneyArt art)
    {
        int index=(int)species;var root=new GameObject("Lake"+species);root.layer=LayerMask.NameToLayer("Enemy");
        var body=root.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.interpolation=RigidbodyInterpolation2D.Interpolate;
        var hitbox=root.AddComponent<CapsuleCollider2D>();hitbox.isTrigger=true;hitbox.size=new Vector2(index==0?.45f:index==3?.95f:.75f,index==0?.35f:.65f);
        hitbox.direction=CapsuleDirection2D.Horizontal;hitbox.offset=index==2?new Vector2(0,.45f):Vector2.zero;
        var health=root.AddComponent<Health>();JungleStageTools.Set(health,"maxHealth",new[]{2,4,5,8}[index]);
        root.AddComponent<EnemyMovement>();var enemy=root.AddComponent<Enemy>();JungleStageTools.Set(enemy,"contactDamage",0);JungleStageTools.Set(enemy,"pointsOnDefeat",new[]{250,600,650,1000}[index]);JungleStageTools.Set(enemy,"deathLifetime",.28f);
        var view=new GameObject("Visual").AddComponent<SpriteRenderer>();view.transform.SetParent(root.transform,false);view.sprite=art.Frame(index,0);view.sortingOrder=20;
        var combat=root.AddComponent<LakeJourneyEnemy>();JungleStageTools.Set(combat,"species",index);JungleStageTools.Set(combat,"art",art);JungleStageTools.Set(combat,"visual",view);
        string path=Prefabs+species+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);Object.DestroyImmediate(root);return prefab.GetComponent<Enemy>();
    }
}
