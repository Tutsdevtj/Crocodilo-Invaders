using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GusiniBossTools
{
    [Serializable] private class SpriteSpec { public string path; public float pivotX,pivotY; public int width,height; }
    [Serializable] private class EventSpec { public string function; public float time; public int value; }
    [Serializable] private class ClipSpec { public string name; public string[] frames; public int[] durations; public bool loop; public EventSpec[] events; }
    [Serializable] private class Manifest { public SpriteSpec[] sprites; public ClipSpec[] clips; }
    private const string AnimationPath = "Assets/Animations/Boss/GusiniPhase1/";

    [MenuItem("Crocodilo/Boss/Open Gusini Phase 1 test")]
    public static void OpenTest()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Tests/GusiniBossTest.unity");
    }

    [MenuItem("Crocodilo/Boss/Validate Gusini Phase 1 assets")]
    public static void Validate()
    {
        Manifest manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("SourceArt/GusiniPhase1/RuntimeManifest.json"));
        Require(manifest.sprites.Length==50,"Expected 50 native sprites");
        foreach (SpriteSpec s in manifest.sprites)
        {
            Sprite sprite=AssetDatabase.LoadAssetAtPath<Sprite>(s.path);
            Require(sprite!=null,s.path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(s.path);
            Require(importer.filterMode==FilterMode.Point && !importer.mipmapEnabled &&
                importer.textureCompression==TextureImporterCompression.Uncompressed && Mathf.Approximately(sprite.pixelsPerUnit,100),"Pixel import: "+s.path);
            Require(sprite.rect.size==new Vector2(s.width,s.height) &&
                Vector2.Distance(sprite.pivot,new Vector2(s.width*s.pivotX,s.height*s.pivotY))<.001f,"Canvas/pivot: "+s.path);
        }
        foreach (ClipSpec c in manifest.clips)
        {
            AnimationClip clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationPath+c.name+".anim");
            Require(clip!=null && AnimationUtility.GetCurveBindings(clip).Length==0,"No transform animation: "+c.name);
            var bindings=AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Require(bindings.Length==1 && bindings[0].path=="Visual" && bindings[0].propertyName=="m_Sprite","Visual-only clip: "+c.name);
            var keys=AnimationUtility.GetObjectReferenceCurve(clip,bindings[0]);
            Require(keys.Length==c.frames.Length+1,"Frame count: "+c.name);
            int ms=0;
            for (int i=0;i<c.frames.Length;i++)
            {
                Require(Mathf.Abs(keys[i].time-ms/1000f)<.0001f && AssetDatabase.GetAssetPath(keys[i].value)==c.frames[i],"Variable frame timing: "+c.name+" / "+i);
                ms+=c.durations[i];
            }
            Require(Mathf.Abs(keys[keys.Length-1].time-ms/1000f)<.0001f,"Final hold: "+c.name);
            Require(AnimationUtility.GetAnimationClipSettings(clip).loopTime==c.loop,"Loop: "+c.name);
            var events=AnimationUtility.GetAnimationEvents(clip);
            var audioEvents=AudioEvents(c.name);
            Require(events.Length==c.events.Length+audioEvents.Length,"Event count: "+c.name);
            foreach (EventSpec e in c.events)
                Require(events.Any(a => a.functionName==e.function && Mathf.Abs(a.time-e.time)<.0001f && a.intParameter==e.value),"Release timestamp: "+c.name+" / "+e.function);
            foreach (EventSpec e in audioEvents)
                Require(events.Any(a => a.functionName==e.function && Mathf.Abs(a.time-e.time)<.0001f),"Audio event: "+c.name+" / "+e.function);
        }
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimationPath+"Gusini.controller");
        Require(controller.layers[0].stateMachine.defaultState.name=="Idle" && controller.layers[0].stateMachine.states.Length==4,"Boss state machine");
        foreach (string name in new[] { "Gusini","GusiniBullet","GusiniSquadron","GusiniSquadronGoose","GusiniParachuteEgg","GusiniYolkLeft","GusiniYolkUpLeft","GusiniYolkDownLeft","ExecutorV9" })
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/"+name+".prefab");
            Require(root!=null,"Prefab "+name);
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)==0,"Missing script "+name+" / "+child.name);
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
            {
                var serialized=new SerializedObject(component); var p=serialized.GetIterator();
                while(p.Next(true))
                    if (p.propertyType==SerializedPropertyType.ObjectReference)
                        Require(p.objectReferenceValue!=null || p.objectReferenceEntityIdValue==EntityId.None,"Missing reference "+name+" / "+p.propertyPath);
            }
        }
        var gusini=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab");
        Require(gusini.GetComponent<Boss>().DisplayName=="GUSINI" && gusini.GetComponent<BossCombat>()==null,"Separate Gusini controller");
        Require(gusini.GetComponent<Boss>().Portrait!=null,"Gusini portrait");
        var legacy=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/ExecutorV9.prefab").GetComponent<Boss>();
        Require(legacy.DisplayName=="EXECUTOR V-9" && legacy.Portrait!=null,"Legacy name/portrait");
        var hud=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/BossHUD.prefab").GetComponent<BossHUD>();
        Require(new SerializedObject(hud).FindProperty("portraitImage").objectReferenceValue!=null,"Generic HUD portrait binding");
        Require(gusini.GetComponent<Animator>().applyRootMotion==false,"No root motion");
        foreach (string origin in new[] { "ShotOrigin","EggDropOrigin","SquadronOrigin" }) Require(gusini.transform.Find(origin)!=null,"Root origin "+origin);
        foreach(string path in new[] { "Assets/Scenes/Tests/PlayerMovementTest.unity", "Assets/Scenes/Tests/StageOneWaves.unity", "Assets/Scenes/Tests/GusiniBossTest.unity" })
        {
            Scene scene=SceneManager.GetSceneByPath(path); bool opened=!scene.IsValid() || !scene.isLoaded;
            if(opened) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var waves=scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<WaveController>(true)).Single();
                Require(new SerializedObject(waves).FindProperty("bossPrefab").objectReferenceValue==gusini.GetComponent<Boss>(),"Gusini scene reference: "+path);
            }
            finally { if(opened) EditorSceneManager.CloseScene(scene,true); }
        }
        GusiniAudioValidation.Validate();
        Debug.Log("GUSINI ASSET VALIDATION PASSED: 50 sprites, 13 variable-timed clips/events; controllers; 9 prefabs including legacy; no missing scripts/references.");
    }
    private static EventSpec[] AudioEvents(string clip)
    {
        switch(clip)
        {
            case "Shoot": return new[] { new EventSpec { function="OnShootWindup",time=.08f } };
            case "EggDrop": return new[] { new EventSpec {function="OnHatchLatch",time=.08f}, new EventSpec {function="OnHatchOpen",time=.18f}, new EventSpec {function="OnHatchClose",time=.49f} };
            case "EggCrack1": return new[] { new EventSpec {function="OnCrack1",time=0} };
            case "EggCrack2": return new[] { new EventSpec {function="OnCrack2",time=0} };
            default: return Array.Empty<EventSpec>();
        }
    }
    private static void Require(bool condition,string detail) { if(!condition) throw new InvalidOperationException(detail); }
}
