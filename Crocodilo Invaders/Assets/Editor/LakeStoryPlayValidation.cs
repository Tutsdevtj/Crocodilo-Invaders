using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LakeStoryPlayValidation
{
    const string SessionKey="Crocodilo.Lake.StoryQA";
    static Stack<IEnumerator> steps=new Stack<IEnumerator>();
    static int checks,errors;static double deadline;static Keyboard keyboard;
    static LakeStoryController story;static WaveController waves;static Player player;static DialogueRunner runner;
    static LakeStoryPlayValidation(){EditorApplication.playModeStateChanged+=s=>{if(!SessionState.GetBool(SessionKey,false))return;if(s==PlayModeStateChange.EnteredPlayMode)Begin();if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(SessionKey,false);if(errors==0 && SessionState.GetBool(SessionKey+".All",false) && !SessionState.GetBool(SessionKey+".Boss",false)){EditorApplication.delayCall+=RunBoss;return;}SessionState.SetBool(SessionKey+".All",false);EditorApplication.Exit(errors==0?0:1);}};}
    public static void RunAll(){SessionState.SetBool(SessionKey+".All",true);Run();}
    public static void RunSkip(){EditorSceneManager.OpenScene(LakeStageTools.Test);SessionState.SetBool(SessionKey+".Skip",true);SessionState.SetBool(SessionKey+".Boss",true);SessionState.SetBool(SessionKey,true);EditorApplication.isPlaying=true;}
    public static void RunBoss(){SessionState.SetBool(SessionKey+".Skip",false);EditorSceneManager.OpenScene(LakeStageTools.Test);SessionState.SetBool(SessionKey+".Boss",true);SessionState.SetBool(SessionKey,true);EditorApplication.isPlaying=true;}
    public static void Run(){SessionState.SetBool(SessionKey+".Skip",false);SessionState.SetBool(SessionKey+".Boss",false);EditorSceneManager.OpenScene(LakeStageTools.Stage);SessionState.SetBool(SessionKey,true);EditorApplication.isPlaying=true;}
    static void Begin(){checks=errors=0;deadline=EditorApplication.timeSinceStartup+220;Directory.CreateDirectory("Logs/LagoDoPneu/v31");Application.runInBackground=true;InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=InputSystem.AddDevice<Keyboard>("LakeStoryValidationKeyboard");InputSystem.EnableDevice(keyboard);Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;}
    static void Bind(){story=Object.FindAnyObjectByType<LakeStoryController>();waves=Object.FindAnyObjectByType<WaveController>();player=Object.FindAnyObjectByType<Player>();runner=story.Dialogue;player.GetComponent<PlayerInputReader>().enabled=false;LeaderboardSession.Cancel();}
    static IEnumerator Scenario()
    {
        CampaignProgress.UseDirectory("Logs/LagoDoPneu/v31/TestSave/"+Guid.NewGuid().ToString("N"));yield return Frames(5);Bind();
        if(!SessionState.GetBool(SessionKey+".Boss",false))
        {
        Check(story.Playing==LakeStoryController.Scene.Opening && waves.Phase==WaveController.RunPhase.Intro,"Opening owns Intro before wave one");
        Check(!player.Movement.enabled && !player.Combat.enabled && player.Health.IsInvulnerable,"Opening blocks player and protects health");
        Check(!player.Health.TakeDamage(1),"Damage blocked during dialogue");
        yield return Until(()=>runner.AwaitingAdvance,"opening typing");Capture("opening",1280,720);
        var pause=Object.FindAnyObjectByType<PauseController>();pause.PauseGame();int visible=runner.VisibleCharacters;yield return RealWait(.25f);
        Check(pause.IsPaused && Time.timeScale==0 && runner.VisibleCharacters==visible,"Pause freezes dialogue and environment");pause.ResumeGame();
        runner.Skip();yield return Frames(3);Check(story.ScenesCompleted==1 && waves.Phase==WaveController.RunPhase.Announcing && player.Movement.enabled,"Skip opening resumes wave announcement once");
        yield break;
        }
        if(SessionState.GetBool(SessionKey+".Skip",false))
        {
            yield return Until(()=>story.Playing==LakeStoryController.Scene.Encounter,"quick encounter");runner.Skip();yield return Frames(3);
            var quickBoss=waves.ActiveBoss;quickBoss.Health.TakeDamage(quickBoss.Health.PreciseCurrentHealth+100);
            yield return Until(()=>story.Playing==LakeStoryController.Scene.Finale,"quick finale");
            Check(!story.World.ChannelOpen,"Fresh ending starts with blocked outlet");
            DevelopmentBossSession.End();int recorded=0;CampaignProgress.FlagRecorded+=_=>recorded++;
            runner.Skip();
            Check(story.World.ChannelOpen && story.World.NurserySafe,"Early finale skip opens channel and retains nursery");
            Check(waves.Phase==WaveController.RunPhase.ChapterCompleted && Object.FindAnyObjectByType<ChapterFlow>().Completed,"Early skip concludes chapter");
            Check(recorded==2 && CampaignProgress.Has("lago_do_pneu_completed") && CampaignProgress.Has("chapter2_completed"),"Campaign writes both lake flags once in isolated save");
            runner.Skip();Check(recorded==2,"Repeated skip cannot duplicate completion");yield break;
        }
        yield return Until(()=>story.Playing==LakeStoryController.Scene.Encounter,"encounter");var boss=waves.ActiveBoss;var combat=boss.GetComponent<SapoDoPneuCombat>();
        Check(boss.CombatHeld && !boss.Ready && boss.Health.IsInvulnerable,"Encounter holds and protects emerged boss");Check(boss.RetainedAfterDefeat,"Lake boss retained for ending");
        yield return Until(()=>runner.AwaitingAdvance,"Sapeneu first line");Check(runner.RightPortrait.sprite!=null && runner.Sequence.Cast.Find("sapeneu").displayName=="Sapeneu Cururu","Dedicated Sapeneu portrait and full name");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(800,600)})Capture("encounter-"+size.x,size.x,size.y);
        Check(story.World.NurserySafe,"Nursery present before resolution");runner.Skip();yield return Frames(3);
        Check(boss.Ready && player.Movement.enabled && !boss.Health.IsInvulnerable,"Encounter skip restores combat");
        boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth*.51f);
        yield return Until(()=>story.Playing==LakeStoryController.Scene.Fury,"fury dialogue");
        Check(!boss.Ready && combat.Companion.Entries==0,"Fury begins before wife enters");
        Capture("fury-before",1280,720);yield return AdvanceTo(3);yield return Wait(1.4f);
        Check(combat.Companion.Entries==1 && combat.Companion.Joined,"Join beat launches Saperereca exactly once");Capture("fury",1280,720);
        runner.Skip();yield return Frames(3);Check(boss.Ready && combat.Companion.Entries==1,"Fury skip resumes fight without duplicate entry");
        boss.Health.TakeDamage(boss.Health.PreciseCurrentHealth+100);
        yield return Until(()=>story.Playing==LakeStoryController.Scene.Finale,"ending dialogue");
        Check(boss!=null && boss.Health.IsDead && waves.Phase==WaveController.RunPhase.BossDefeated,"Exhausted boss retained and chapter waits for dialogue");
        Check(!player.Movement.enabled && player.Health.IsInvulnerable,"Finale keeps player safe");
        yield return AdvanceTo(3);yield return Wait(.3f);
        Check(story.World.OpeningProgress>0 && story.World.OpeningProgress<1,"Channel opens progressively on Croc line");
        yield return Wait(1.7f);Check(story.World.ChannelOpen && story.World.NurserySafe,"Outlet opens without removing nursery");Capture("channel-open",1280,720);
        runner.Skip();Check(waves.Phase==WaveController.RunPhase.ChapterCompleted && Object.FindAnyObjectByType<ChapterFlow>().Completed,"Finale completes chapter once");
        Check(!CampaignProgress.Has("lago_do_pneu_completed"),"DEV encounter does not write real campaign flags");

    }
    static IEnumerator AdvanceTo(int line)
    {
        int tries=0;
        while(runner.LineIndex<line)
        {
            yield return Until(()=>runner.LineIndex>=line || runner.AwaitingAdvance,"advance ready");
            if(runner.LineIndex>=line)break;
            yield return Press(Key.Enter);
            if(++tries>8)throw new Exception("Input did not advance dialogue");
        }
        yield return Until(()=>runner.VisibleCharacters>0,"line started");
    }

    static IEnumerator Press(Key key){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return Frames(1);InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Frames(1);}
    static void Capture(string label,int width,int height)
    {
        var camera=Camera.main;var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas && c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        float aspect=camera.aspect;camera.aspect=(float)width/height;
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;c.sortingLayerName="LakeHUD";}
        var target=new RenderTexture(width,height,24);camera.targetTexture=target;Object.FindAnyObjectByType<LakeBackdrop>().SendMessage("LateUpdate");Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes("Logs/LagoDoPneu/v31/"+label+".png",texture.EncodeToPNG());
        Check(!runner.Body.isTextOverflowing && !runner.NameText.isTextOverflowing,"Dialogue fits "+label);
        RenderTexture.active=previous;camera.targetTexture=null;camera.aspect=aspect;target.Release();Object.Destroy(target);Object.Destroy(texture);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
    }
    static IEnumerator Frames(int count){while(count-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
    static IEnumerator RealWait(float seconds){double end=EditorApplication.timeSinceStartup+seconds;while(EditorApplication.timeSinceStartup<end)yield return null;}
    static IEnumerator Until(Func<bool> predicate,string label){double end=EditorApplication.timeSinceStartup+35;while(!predicate()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    static void Check(bool condition,string label){if(!condition)throw new Exception("STORY_CHECK_FAILED "+label);checks++;Debug.Log("STORY_CHECK "+label);}
    static void OnLog(string message,string trace,LogType type){if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors++;}
    static void Tick()
    {

        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Story QA deadline");while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}Debug.Log("LAKE_STORY_PLAY_OK checks="+checks+" errors="+errors);}
        catch(Exception e){errors++;Debug.LogException(e);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;InputSystem.RemoveDevice(keyboard);LeaderboardSession.Cancel();File.WriteAllText("Logs/LagoDoPneu/v31/"+(SessionState.GetBool(SessionKey+".Skip",false)?"skip-validation.json":SessionState.GetBool(SessionKey+".Boss",false)?"boss-validation.json":"opening-validation.json"),"{\"checks\":"+checks+",\"errors\":"+errors+"}");Time.timeScale=1;EditorApplication.isPlaying=false;
    }
}
