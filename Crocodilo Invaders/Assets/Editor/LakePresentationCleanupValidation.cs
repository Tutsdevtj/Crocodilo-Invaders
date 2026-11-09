using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LakePresentationCleanupValidation
{
    const string Key="Crocodilo.Lake.CleanupV32";
    static Stack<IEnumerator> steps=new Stack<IEnumerator>();static int checks,errors;static double deadline;
    static LakePresentationCleanupValidation(){EditorApplication.playModeStateChanged+=s=>{if(!SessionState.GetBool(Key,false))return;if(s==PlayModeStateChange.EnteredPlayMode)Begin();if(s==PlayModeStateChange.EnteredEditMode){if(errors==0 && !SessionState.GetBool(Key+".Boss",false)){EditorApplication.delayCall+=RunBoss;return;}SessionState.SetBool(Key,false);EditorApplication.Exit(errors==0?0:1);}};}
    public static void Run(){SessionState.SetBool(Key,true);SessionState.SetBool(Key+".Boss",false);EditorSceneManager.OpenScene(LakeStageTools.Stage);EditorApplication.isPlaying=true;}
    static void RunBoss(){SessionState.SetBool(Key+".Boss",true);EditorSceneManager.OpenScene(LakeStageTools.Test);EditorApplication.isPlaying=true;}
    static void Begin(){checks=errors=0;deadline=EditorApplication.timeSinceStartup+100;Application.runInBackground=true;Directory.CreateDirectory("Logs/LagoDoPneu/v32");Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;}
    static IEnumerator Scenario()
    {
        yield return Frames(5);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/LagoDoPneu/v32/TestSave-"+Guid.NewGuid().ToString("N"));
        var waves=Object.FindAnyObjectByType<WaveController>();var lake=Object.FindAnyObjectByType<LakeBackdrop>();var player=Object.FindAnyObjectByType<Player>();var runner=Object.FindAnyObjectByType<DialogueRunner>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Health.SetInvulnerable(true);
        Check(Object.FindAnyObjectByType<LakeStoryController>()==null && Object.FindAnyObjectByType<LakeStoryWorld>()==null,"Rejected story components absent");
        foreach(string name in new[]{"SideChannel","TadpoleNursery","DryIntake","ChannelDebris","RestoredWater"})Check(GameObject.Find(name)==null,"No stationary landmark: "+name);
        Check(!runner.IsPlaying,"No water-story dialogue starts");
        if(!SessionState.GetBool(Key+".Boss",false))
        {
            Check(waves.CurrentWave==1 && waves.Phase==WaveController.RunPhase.Announcing,"Campaign starts at wave one without dialogue gate");
            Check(player.Movement.enabled && player.Combat.enabled,"Player controls available at campaign start");
            float distance=lake.ScrollDistance(4);yield return Wait(.4f);Check(lake.ScrollDistance(4)>distance,"Background continues scrolling");
            yield return Capture("campaign-clean.png");
            yield return Until(()=>waves.Phase==WaveController.RunPhase.Combat,"wave one combat");
            Check(!runner.IsPlaying && player.Movement.enabled,"Wave one progresses normally without narrative interruption");yield break;
        }
        var entrance=Object.FindAnyObjectByType<LakeBossEntrance>();
        Check(waves.CurrentWave==10 && entrance.IsPlaying,"Round ten keeps its existing boss presentation");
        yield return Until(()=>entrance.CurrentStage==LakeBossEntrance.Stage.Finished,"boss entrance");yield return Frames(3);
        var boss=waves.ActiveBoss;var combat=boss.GetComponent<SapoDoPneuCombat>();
        Check(boss.Ready && !boss.CombatHeld && player.Movement.enabled,"Entrance releases combat immediately");
        Check(!runner.IsPlaying && !boss.RetainedAfterDefeat,"Boss is not retained for rejected finale");
        yield return Capture("boss-clean.png");
        boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth*.51f);
        yield return Until(()=>combat.Companion.Entries==1,"Saperereca entry");
        Check(!runner.IsPlaying && player.Movement.enabled,"Rage and wife entry have no dialogue interruption");
        yield return Until(()=>combat.Companion.Joined,"Saperereca landing");
        Check(combat.Companion.Entries==1 && boss.Ready,"Saperereca joins once and fight continues");
        var flow=Object.FindAnyObjectByType<ChapterFlow>();boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth+100);
        yield return Until(()=>flow.Completed,"normal chapter completion");
        Check(waves.Phase==WaveController.RunPhase.ChapterCompleted && !runner.IsPlaying,"Defeat completes chapter without canal scene");
        Check(flow.Transition.Current==SceneTransition.State.FadingOut,"Normal return to base starts");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"DEV preview does not write campaign progress");
    }
    static IEnumerator Capture(string name)
    {
        Directory.CreateDirectory("Docs/ArtPreviews/LakeSceneCleanup_v32");var camera=Camera.main;float aspect=camera.aspect;
        var overlays=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas && c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;c.sortingLayerName="LakeHUD";}
        var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.aspect=16f/9;yield return Frames(4);Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;
        var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes("Docs/ArtPreviews/LakeSceneCleanup_v32/"+name,pixels.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;camera.aspect=aspect;rt.Release();Object.Destroy(rt);Object.Destroy(pixels);
        foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
    }
    static IEnumerator Frames(int count){while(count-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
    static IEnumerator Until(Func<bool> condition,string label){double end=EditorApplication.timeSinceStartup+40;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    static void Check(bool success,string label){if(!success)throw new Exception("CLEANUP_FAILED "+label);checks++;Debug.Log("CLEANUP_CHECK "+label);}
    static void OnLog(string m,string t,LogType type){if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors++;}
    static void Tick()
    {
        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Cleanup deadline");while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}Debug.Log("LAKE_CLEANUP_PLAY_OK checks="+checks+" errors="+errors);}
        catch(Exception e){errors++;Debug.LogException(e);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;LeaderboardSession.Cancel();File.WriteAllText("Logs/LagoDoPneu/v32/"+(SessionState.GetBool(Key+".Boss",false)?"boss":"campaign")+"-validation.json","{\"checks\":"+checks+",\"errors\":"+errors+"}");Time.timeScale=1;EditorApplication.isPlaying=false;
    }
}
