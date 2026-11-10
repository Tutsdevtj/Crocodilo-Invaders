using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class AchievementIconPreview
{
    const string Key="Crocodilo.AchievementIcons.Preview",Output="Docs/ArtPreviews/AchievementIcons_v03";
    static readonly Stack<IEnumerator> Steps=new Stack<IEnumerator>();
    static bool finished;static int checks,errors;static double deadline;
    static AchievementIconPreview()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(finished&&errors==0?0:1);}
        };
    }
    public static void Run(){AchievementIconTools.Install();SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.isPlaying=true;}
    static void Begin()
    {
        finished=false;checks=errors=0;deadline=EditorApplication.timeSinceStartup+90;
        Directory.CreateDirectory(Output);Application.runInBackground=true;
        Achievements.UseDirectory("Logs/Achievements/Icons-"+Guid.NewGuid().ToString("N"));LeaderboardSession.Cancel();
        CampaignProgress.UseDirectory("Logs/Achievements/IconsCampaign-"+Guid.NewGuid().ToString("N"));
        Steps.Clear();Steps.Push(Scenario());Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;
    }
    static IEnumerator Scenario()
    {
        yield return Frames(6);var menu=Object.FindAnyObjectByType<MainMenuController>();
        var button=menu.transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        yield return Until(()=>button.IsInteractable(),"menu intro");menu.ShowAchievements();
        var runtime=AchievementRuntime.Instance;var view=runtime.View;
        Check(view.IsVisible,"achievement panel opens");
        view.SetPage(2);yield return Frames(2);
        var hidden=runtime.GetComponentsInChildren<AchievementRow>().Single(r=>r.HidesDetails);
        Check(hidden.DisplayedTitle=="CONQUISTA OCULTA"&&!hidden.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().enabled,"secret icon remains hidden until unlocked");
        yield return Capture("hidden-locked.png");
        foreach(var definition in AchievementCatalog.All)
        {
            if(definition.Id==AchievementCatalog.Cards)
                for(int kind=0;kind<5;kind++)Achievements.Card(new UpgradeCard{kind=(UpgradeKind)kind,rarity=UpgradeRarity.Common,percent=1});
            else Achievements.Award(definition.Id);
        }
        Check(Achievements.Store.UnlockedCount==10,"isolated preview has all ten icons unlocked");
        for(int page=0;page<3;page++)
        {
            view.SetPage(page);yield return Frames(3);
            foreach(var row in runtime.GetComponentsInChildren<AchievementRow>())
            {
                var icon=row.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
                Check(icon.enabled&&icon.sprite!=null&&AssetDatabase.GetAssetPath(icon.sprite).StartsWith(AchievementIconTools.Icons+"/"),"dedicated icon visible: "+row.DisplayedTitle);
            }
            yield return Capture("panel-icons-"+(page+1)+".png");
        }
        view.Close();SceneManager.LoadScene("Assets/Scenes/Tests/PlayerMovementTest.unity");yield return Frames(8);
        var player=Object.FindAnyObjectByType<Player>();player.Health.SetInvulnerable(true);player.GetComponent<PlayerInputReader>().enabled=false;
        var waves=Object.FindAnyObjectByType<WaveController>();
        yield return Until(()=>waves.Phase==WaveController.RunPhase.Combat,"gameplay begins",true);
        waves.enabled=false;waves.GetComponent<EnemySpawner>().StopCombat();player.SetGameplayActive(true);
        Check(AssetDatabase.GetAssetPath(player.GetComponent<Animator>().runtimeAnimatorController)=="Assets/Animations/Player/Player.controller","original airplane controller in gameplay");
        Check(AssetDatabase.GetAssetPath(player.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite).StartsWith("Assets/Art/Player/Bombardiro/"),"original airplane sprite in gameplay");
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.FirstWave&&view.ToastClock>.4f,"new icon popup");
        yield return Capture("original-ship-new-popup.png");
        Check(runtime.transform.Find("UnlockNotification/Icon").GetComponent<UnityEngine.UI.Image>().sprite.name==AchievementCatalog.FirstWave,"new icon used by compact popup");
        finished=true;
    }
    static IEnumerator Capture(string name)
    {
        var camera=Camera.main;var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
        var rt=new RenderTexture(1200,720,24);var previousTarget=camera.targetTexture;float aspect=camera.aspect;camera.targetTexture=rt;camera.aspect=5f/3f;
        yield return Frames(4);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(1200,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1200,720),0,0);texture.Apply();File.WriteAllBytes(Output+"/"+name,texture.EncodeToPNG());
        RenderTexture.active=old;camera.targetTexture=previousTarget;camera.aspect=aspect;
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
        rt.Release();Object.Destroy(rt);Object.Destroy(texture);
    }
    static IEnumerator Frames(int n){while(n-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Until(Func<bool> condition,string label,bool skip=false)
    {double end=EditorApplication.timeSinceStartup+30;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);if(skip){var dialogue=Object.FindAnyObjectByType<DialogueRunner>();if(dialogue!=null&&dialogue.IsPlaying)dialogue.Skip();}yield return Frames(1);}}
    static void Check(bool value,string label){if(!value)throw new Exception("ICON_PREVIEW_FAILED "+label);checks++;Debug.Log("ICON_CHECK "+label);}
    static void OnLog(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    static void Tick()
    {
        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("icon preview deadline");while(Steps.Count>0){var step=Steps.Peek();if(!step.MoveNext()){Steps.Pop();continue;}if(step.Current is IEnumerator child){Steps.Push(child);continue;}return;}}
        catch(Exception e){errors++;Debug.LogException(e);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;LeaderboardSession.Cancel();Time.timeScale=1;
        File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+",\"finished\":"+finished.ToString().ToLowerInvariant()+"}");
        Debug.Log("ICON_PREVIEW_RESULT checks="+checks+" errors="+errors+" finished="+finished);EditorApplication.isPlaying=false;
    }
}
