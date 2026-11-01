using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class LakeStoryTools
{
    const string Data="Assets/Data/Dialogue/LakeStoryV31/";
    [Serializable] class CastPayload { public DialogueCast.Speaker[] speakers; }
    [Serializable] class SequencePayload { public DialogueCast cast; public float openDelay=.25f,closeDelay=.2f;public DialogueSequence.Line[] lines; }
    public static void Install()
    {
        AssetDatabase.Refresh();
        var art=AssetDatabase.LoadAssetAtPath<LakeStoryAssets>("Assets/Resources/LakeStoryV31/art.asset");
        if(art==null){art=ScriptableObject.CreateInstance<LakeStoryAssets>();AssetDatabase.CreateAsset(art,"Assets/Resources/LakeStoryV31/art.asset");}
        foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(art)).OfType<Sprite>())Object.DestroyImmediate(sub,true);
        art.sapeneuPortraits=Slice("Assets/Art/Portraits/LakeStoryV31/sapeneu.png",2,2,art,true);
        art.sapererecaPortraits=Slice("Assets/Art/Portraits/LakeStoryV31/saperereca.png",2,1,art,true);
        art.props=Slice("Assets/Resources/LakeStoryV31/props.png",3,1,art,false);EditorUtility.SetDirty(art);
        var original=AssetDatabase.LoadAssetAtPath<DialogueCast>("Assets/Data/Dialogue/DialogueCast.asset");
        var cast=AssetDatabase.LoadAssetAtPath<DialogueCast>(Data+"Cast.asset");
        if(cast==null){cast=ScriptableObject.CreateInstance<DialogueCast>();AssetDatabase.CreateAsset(cast,Data+"Cast.asset");}
        var speakers=original.Speakers.ToList();
        speakers.Add(Speaker("sapeneu","Sapeneu Cururu",new Color(.52f,.69f,.25f),.78f,new[]{"proud","serious","furious","vulnerable"},art.sapeneuPortraits));
        speakers.Add(Speaker("saperereca","Saperereca",new Color(.83f,.48f,.6f),1.05f,new[]{"firm","relieved"},art.sapererecaPortraits));
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(new CastPayload{speakers=speakers.ToArray()}),cast);EditorUtility.SetDirty(cast);
        var opening=Sequence("Opening",cast,new[]{
            L("crocodilo","Lá embaixo não enche um balde…","worried"),
            L("crocodilo","…e aqui fizeram uma represa.","serious"),
            L("crocodilo","Tá. Tem alguém cuidando do almoço.","alert")});
        var encounter=Sequence("Encounter",cast,new[]{
            L("sapeneu","Desliga essa geringonça. Tá espantando minha comida.","proud"),
            L("crocodilo","Tu tá segurando a água que abastece a base.","serious"),
            L("sapeneu","Tô segurando coisa pior.","serious"),
            L("sapeneu","A última máquina que passou deixou isso aqui.","serious"),
            L("crocodilo","Tem gente precisando dessa água.","sincere"),
            L("sapeneu","Aqui também tem.","serious"),
            L("crocodilo","O canal do lado tá entupido. Dá pra abrir uma passagem.","determined"),
            L("sapeneu","Começa abrindo um buraco. Quando eu vejo, levaram minha margem inteira.","furious"),
            L("crocodilo","O rio já tá escolhendo onde vai ceder.","worried"),
            L("sapeneu","Então é bom tu sair de cima dele.","proud"),
            L("crocodilo","Ô! A mosca tá ATRÁS de mim!","alert")});
        var fury=Sequence("Fury",cast,new[]{
            L("sapeneu","Chega. Tu vai afundar antes de encostar nessa margem!","furious"),
            L("saperereca","Sapeneu. O raso tá abrindo.","firm"),
            L("crocodilo","Por isso eu tô tentando chegar no canal!","alert"),
            L("saperereca","Daqui eu cuido.","firm","join"),
            L("saperereca","Tu segura o alto. Eu fecho a passagem por baixo.","firm"),
            L("crocodilo","Maravilha. Agora o lago tem dois donos.","worried")});
        var finale=Sequence("Finale",cast,new[]{
            L("saperereca","Já chega.","firm"),
            L("crocodilo","Então deixa eu abrir o canal.","determined"),
            L("sapeneu","Segura os pequenos!","vulnerable"),
            L("crocodilo","Segura aí. Eu abro do lado!","determined","channel",1.5f),
            L("saperereca","Tá baixando. Devagar.","relieved"),
            L("sapeneu","Tu podia ter levado a barragem inteira.","vulnerable"),
            L("crocodilo","A gente precisa dessa água viva.","sincere"),
            L("sapeneu","O canal fica aberto.","serious"),
            L("saperereca","E vocês seguram a sucata lá de cima.","firm"),
            L("crocodilo","Vou cobrar dos pilotos.","neutral"),
            L("sapeneu","Cobra com juros.","proud"),
            L("crocodilo","Essa eu deixei pra tu.","neutral")});
        AssetDatabase.SaveAssets();
        foreach(string path in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(path);
            art=AssetDatabase.LoadAssetAtPath<LakeStoryAssets>("Assets/Resources/LakeStoryV31/art.asset");
            opening=AssetDatabase.LoadAssetAtPath<DialogueSequence>(Data+"Opening.asset");encounter=AssetDatabase.LoadAssetAtPath<DialogueSequence>(Data+"Encounter.asset");fury=AssetDatabase.LoadAssetAtPath<DialogueSequence>(Data+"Fury.asset");finale=AssetDatabase.LoadAssetAtPath<DialogueSequence>(Data+"Finale.asset");
            if(opening==null || encounter==null || fury==null || finale==null)throw new Exception("Missing persisted sequence");
            var waves=Object.FindAnyObjectByType<WaveController>();var player=Object.FindAnyObjectByType<Player>();var pause=Object.FindAnyObjectByType<PauseController>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();
            var runner=Object.FindAnyObjectByType<DialogueRunner>();
            if(runner==null){var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Cinematics.prefab"),scene);runner=obj.GetComponent<DialogueRunner>();
                var extra=obj.GetComponent<ChapterFlow>();if(Object.FindObjectsByType<ChapterFlow>(FindObjectsSortMode.None).Length>1)Object.DestroyImmediate(extra);}
            foreach(var legacy in Object.FindObjectsByType<RunCutscenes>(FindObjectsInactive.Include,FindObjectsSortMode.None))legacy.enabled=false;
            JungleStageTools.Set(runner,"pause",pause);
            var world=lake.GetComponent<LakeStoryWorld>();if(world==null)world=lake.gameObject.AddComponent<LakeStoryWorld>();
            JungleStageTools.Set(world,"art",art);JungleStageTools.Set(world,"lake",lake);JungleStageTools.Set(world,"waves",waves);
            var story=waves.GetComponent<LakeStoryController>();if(story==null)story=waves.gameObject.AddComponent<LakeStoryController>();
            JungleStageTools.Set(story,"dialogue",runner);JungleStageTools.Set(story,"player",player);JungleStageTools.Set(story,"pause",pause);JungleStageTools.Set(story,"waves",waves);JungleStageTools.Set(story,"world",world);
            JungleStageTools.Set(story,"opening",opening);JungleStageTools.Set(story,"encounter",encounter);JungleStageTools.Set(story,"fury",fury);JungleStageTools.Set(story,"finale",finale);JungleStageTools.Set(waves,"lakeStory",story);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);
        Debug.Log("LAKE_STORY_V31_INSTALL_OK: 32 lines, 4 scenes, 6 portraits, 3 landmarks.");
    }
    static DialogueCast.Speaker Speaker(string id,string name,Color color,float pitch,string[] ids,Sprite[] frames)=>new DialogueCast.Speaker{id=id,displayName=name,side=DialogueCast.Side.Right,accent=color,typingPitch=pitch,expressions=ids.Select((s,i)=>new DialogueCast.Expression{id=s,portrait=frames[i]}).ToArray()};
    static DialogueSequence.Line L(string who,string text,string expression,string beat="",float after=.15f)=>new DialogueSequence.Line{speakerId=who,text=text,expressionId=expression,beat=beat,pauseAfter=after};
    static DialogueSequence Sequence(string name,DialogueCast cast,DialogueSequence.Line[] lines)
    {
        var sequence=AssetDatabase.LoadAssetAtPath<DialogueSequence>(Data+name+".asset");if(sequence==null){sequence=ScriptableObject.CreateInstance<DialogueSequence>();AssetDatabase.CreateAsset(sequence,Data+name+".asset");}
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(new SequencePayload{cast=cast,lines=lines}),sequence);EditorUtility.SetDirty(sequence);
        if(!sequence.Validate(out string error))throw new Exception(name+": "+error);return sequence;
    }
    static Sprite[] Slice(string path,int columns,int rows,LakeStoryAssets owner,bool portrait)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.isReadable=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var frames=new Sprite[columns*rows];
        int w=texture.width/columns,h=texture.height/rows;
        for(int i=0;i<frames.Length;i++)
        {
            // Native sprite rectangles keep the source unchanged; common cell scale prevents expression jitter.
            Rect rect=new Rect((i%columns)*w,texture.height-(i/columns+1)*h,w,h);
            frames[i]=Sprite.Create(texture,rect,new Vector2(.5f,portrait?0:.12f),portrait?100:300,0,SpriteMeshType.FullRect);frames[i].name=Path.GetFileNameWithoutExtension(path)+"_"+i;AssetDatabase.AddObjectToAsset(frames[i],owner);
        }
        return frames;
    }
}
