using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class AchievementMenuButtonPreview
{
    const string Key="Crocodilo.AchievementMenu.Medal",Output="Docs/ArtPreviews/AchievementMenuButton_v05";
    static readonly Stack<IEnumerator> Steps=new Stack<IEnumerator>();
    static int checks,errors;static bool finished;static double deadline;
    static AchievementMenuButtonPreview()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(finished&&errors==0?0:1);}
        };
    }
    public static void Run()
    {
        AchievementMenuButtonTools.Install();AchievementIconTools.Validate();
        SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.isPlaying=true;
    }
    static void Begin()
    {
        checks=errors=0;finished=false;deadline=EditorApplication.timeSinceStartup+70;Application.runInBackground=true;
        Directory.CreateDirectory(Output);Achievements.UseDirectory("Logs/Achievements/MenuMedal-"+Guid.NewGuid().ToString("N"));
        CampaignProgress.UseDirectory("Logs/Achievements/MenuDevStage-"+Guid.NewGuid().ToString("N"));
        Steps.Clear();Steps.Push(Scenario());Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;
    }
    static IEnumerator Scenario()
    {
        yield return Frames(5);
        var menu=Object.FindAnyObjectByType<MainMenuController>();
        var achievement=menu.transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        var leaderboard=menu.transform.Find("LeaderboardButton").GetComponent<UnityEngine.UI.Button>();
        yield return Until(()=>achievement.IsInteractable(),"menu entrance");
        double settled=EditorApplication.timeSinceStartup+.25;while(EditorApplication.timeSinceStartup<settled)yield return Frames(1);
        Check(achievement.GetComponent<CanvasGroup>().alpha==1,"medal enters with the menu");
        Check(achievement.GetComponent<UnityEngine.UI.Image>().sprite!=null,"illustrated medal is active");
        Check(achievement.GetComponentsInChildren<TMPro.TMP_Text>().Length==0,"old flat-button label is hidden");
        Check(((RectTransform)achievement.transform).anchorMin==Vector2.zero,"medal sits in lower-left corner");
        Check(GameObject.Find("JungleMissionEntry")==null&&GameObject.Find("JungleMission")==null,"phase-two shortcut removed from normal menu");
        yield return Capture("main-menu-medals.png",1200,720);
        yield return Capture("main-menu-medals-800x600.png",800,600);
        yield return Capture("main-menu-medals-1920x1080.png",1920,1080);
        yield return Frames(4);
        EventSystem.current.SetSelectedGameObject(achievement.gameObject);
        ExecuteEvents.Execute(achievement.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Right,moveVector=Vector2.right},ExecuteEvents.moveHandler);
        var play=menu.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name=="PlayButton");
        Check(EventSystem.current.currentSelectedGameObject==play.gameObject,"right navigates from left medal to central menu");
        ExecuteEvents.Execute(play.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Right,moveVector=Vector2.right},ExecuteEvents.moveHandler);
        Check(EventSystem.current.currentSelectedGameObject==leaderboard.gameObject,"right navigates from central menu to leaderboard");
        ExecuteEvents.Execute(leaderboard.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Left,moveVector=Vector2.left},ExecuteEvents.moveHandler);
        Check(EventSystem.current.currentSelectedGameObject==play.gameObject,"left returns from leaderboard to central menu");
        ExecuteEvents.Execute(play.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Left,moveVector=Vector2.left},ExecuteEvents.moveHandler);
        Check(EventSystem.current.currentSelectedGameObject==achievement.gameObject,"left navigates from central menu to medal");
        yield return Frames(3);yield return Capture("medal-focused.png",1200,720);
        yield return Frames(3);
        var rect=(RectTransform)achievement.transform;
        var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(rect.rect.center.x,rect.rect.yMin+rect.rect.height*.1f,0)));
        var pointer=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Check(hits.Any(hit=>hit.gameObject==achievement.gameObject),"medal plaque receives pointer raycasts");
        ExecuteEvents.Execute(achievement.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        Check(AchievementRuntime.Instance.View.IsVisible,"medal click opens achievements");
        yield return Capture("medal-opens-panel.png",1200,720);
        AchievementRuntime.Instance.View.Close();
        Check(EventSystem.current.currentSelectedGameObject==achievement.gameObject,"closing panel restores medal focus");
        ExecuteEvents.Execute(achievement.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
        Check(AchievementRuntime.Instance.View.IsVisible,"keyboard or controller submit opens achievements");
        AchievementRuntime.Instance.View.Close();
        ExecuteEvents.Execute(leaderboard.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
        var ranking=Object.FindAnyObjectByType<LeaderboardView>();
        Check(ranking!=null&&ranking.IsVisible,"leaderboard still opens normally");ranking.Close();
        Check(Object.FindObjectsByType<MainMenuController>().Length==1,"existing menu preserved");
        var dev=menu.GetComponent<DeveloperMenuView>();
        var devTab=GameObject.Find("DeveloperMenuCanvas/DeveloperTab").GetComponent<UnityEngine.UI.Button>();
        ExecuteEvents.Execute(devTab.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
        Check(dev.IsVisible,"DEV button opens developer menu");
        var stage=GameObject.Find("DeveloperMenuCanvas/DeveloperWindow/DeveloperPanel/Categories/StageTwoLake").GetComponent<UnityEngine.UI.Button>();
        Check(stage.IsInteractable(),"full phase-two stage is available inside DEV");
        yield return Capture("dev-phase-two.png",1200,720);
        ExecuteEvents.Execute(stage.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
        Check(DevelopmentStageSession.IsActive&&!DevelopmentBossSession.IsActive&&!DevelopmentCardSession.IsActive,"stage DEV starts full-stage preview without boss/card shortcuts");
        yield return Until(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="StageTwoLake","DEV stage load");yield return Frames(6);
        var waves=Object.FindAnyObjectByType<WaveController>();
        Check(waves!=null&&waves.CurrentWave<10,"DEV stage starts before boss wave");
        Check(!Achievements.CanRecord&&!Achievements.Award(AchievementCatalog.FirstWave),"DEV stage does not record permanent achievements");
        yield return Capture("dev-lake-stage.png",1200,720);
        finished=true;
    }
    static IEnumerator Capture(string name,int width,int height)
    {
        var camera=Camera.main;var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
        var rt=new RenderTexture(width,height,24);var oldTarget=camera.targetTexture;float aspect=camera.aspect;camera.targetTexture=rt;camera.aspect=width/(float)height;
        yield return Frames(4);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Output+"/"+name,texture.EncodeToPNG());
        RenderTexture.active=old;camera.targetTexture=oldTarget;camera.aspect=aspect;
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
        rt.Release();Object.Destroy(rt);Object.Destroy(texture);
    }
    static IEnumerator Frames(int n){while(n-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Until(Func<bool> condition,string label){double end=EditorApplication.timeSinceStartup+15;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return Frames(1);}}
    static void Check(bool value,string label){if(!value)throw new Exception("MENU_MEDAL_FAILED "+label);checks++;Debug.Log("MENU_MEDAL_CHECK "+label);}
    static void OnLog(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    static void Tick()
    {
        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("menu medal preview");while(Steps.Count>0){var step=Steps.Peek();if(!step.MoveNext()){Steps.Pop();continue;}if(step.Current is IEnumerator child){Steps.Push(child);continue;}return;}}
        catch(Exception e){errors++;Debug.LogException(e);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
        LeaderboardSession.Cancel();DevelopmentStageSession.End();
        File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+",\"finished\":"+finished.ToString().ToLowerInvariant()+"}");
        Debug.Log("MENU_MEDAL_RESULT checks="+checks+" errors="+errors+" finished="+finished);EditorApplication.isPlaying=false;
    }
}
