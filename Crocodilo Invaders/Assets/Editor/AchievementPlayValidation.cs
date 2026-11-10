using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class AchievementPlayValidation
{
    const string Key="Crocodilo.Achievements.Validation";
    const string Output="Docs/ArtPreviews/Achievements_v01";
    static readonly Stack<IEnumerator> Steps=new Stack<IEnumerator>();
    static double deadline;static int checks,errors;static bool finished;
    static AchievementPlayValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);if(Application.isBatchMode)EditorApplication.Exit(finished&&errors==0?0:1);}
        };
    }
    public static void Run()
    {
        SessionState.SetBool(Key+".Visual",false);
        AchievementTools.Install();
        SessionState.SetInt(Key+".Data",AchievementDataValidation.Run("Logs/Achievements/Data-"+Guid.NewGuid().ToString("N")));
        SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.isPlaying=true;
    }
    public static void RunVisual()
    {
        AchievementTools.Install();SessionState.SetBool(Key+".Visual",true);
        SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.isPlaying=true;
    }
    static void Begin()
    {
        checks=errors=0;finished=false;deadline=EditorApplication.timeSinceStartup+240;
        Directory.CreateDirectory(Output);Application.runInBackground=true;
        Achievements.UseDirectory("Logs/Achievements/Play-"+Guid.NewGuid().ToString("N"));LeaderboardSession.Cancel();
        CampaignProgress.UseDirectory("Logs/Achievements/Campaign-"+Guid.NewGuid().ToString("N"));
        Application.logMessageReceived+=OnLog;Steps.Clear();Steps.Push(SessionState.GetBool(Key+".Visual",false)?VisualScenario():Scenario());EditorApplication.update+=Tick;
    }
    static IEnumerator VisualScenario()
    {
        yield return Frames(6);var runtime=AchievementRuntime.Instance;var view=runtime.View;
        var button=Object.FindAnyObjectByType<MainMenuController>().transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        yield return Until(()=>button.IsInteractable(),"menu intro",12);
        Check(Achievements.Award(AchievementCatalog.LastTooth),"isolated visual unlock");
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.LastTooth&&view.ToastClock>.4f,"visual popup",8);
        var popup=(RectTransform)runtime.transform.Find("UnlockNotification");
        Check(popup.rect.size==new Vector2(300,68),"compact popup is 300 by 68");
        Check(!popup.Find("Description").gameObject.activeSelf,"condition stays in panel instead of compact popup");
        var text=popup.Find("Name").GetComponent<TMP_Text>();text.ForceMeshUpdate();
        yield return Capture("hidden-unlocked-popup.png",1200,720);
        yield return Capture("popup-800x600.png",800,600);
        foreach(var definition in AchievementCatalog.All)
        {text.text=definition.Name;text.ForceMeshUpdate();Check(!text.isTextOverflowing&&text.mesh.vertexCount>0,"compact popup title fits: "+definition.Id);}
        finished=true;
    }
    static IEnumerator Scenario()
    {
        yield return Frames(6);
        var runtime=AchievementRuntime.Instance;Check(runtime!=null,"bootstrap creates runtime");var view=runtime.View;
        Check(Object.FindObjectsByType<AchievementRuntime>().Length==1,"one persistent achievement UI");
        Check(Object.FindObjectsByType<EventSystem>().Length==1,"one EventSystem");
        var menuButton=Object.FindAnyObjectByType<MainMenuController>().transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        yield return Until(()=>menuButton.IsInteractable(),"menu intro completed",12);
        yield return Capture("main-menu.png",1200,720);
        Click(menuButton);
        Check(view.IsVisible,"main-menu button opens panel");
        Check(EventSystem.current.currentSelectedGameObject.name=="Close","keyboard/controller focus starts inside modal");
        yield return Capture("panel-locked.png",1200,720);
        foreach(var row in runtime.GetComponentsInChildren<AchievementRow>())
        {var title=row.transform.Find("Name").GetComponent<TMP_Text>();title.ForceMeshUpdate();Check(title.mesh.vertexCount>0,"achievement title has visible glyph geometry");}
        view.SetPage(2);yield return Frames(2);
        var hidden=runtime.GetComponentsInChildren<AchievementRow>().Single(r=>r.HidesDetails);
        Check(hidden.DisplayedTitle=="CONQUISTA OCULTA"&&!hidden.DisplayedDescription.Contains("10%"),"hidden title and condition concealed");
        Check(!runtime.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Por Um Dente")),"hidden name absent from displayed UI");
        yield return Capture("hidden-locked.png",1200,720);
        yield return Capture("panel-800x600.png",800,600);
        view.SetFilter(1);Check(view.PageCount==1,"empty unlocked filter");
        yield return Capture("empty-unlocked.png",1200,720);
        view.SetFilter(0);Achievements.Set(AchievementCatalog.Dodge,4);view.SetPage(0);
        Check(!Achievements.Store.Get(AchievementCatalog.Dodge).Unlocked,"partial progress stays locked");
        yield return Capture("panel-progress.png",1200,720);
        Click(runtime.transform.Find("Panel/MedalBoard/Close").GetComponent<UnityEngine.UI.Button>());
        Check(!view.IsVisible&&EventSystem.current.currentSelectedGameObject.name=="AchievementsButton","closing restores menu selection");

        SceneManager.LoadScene("Assets/Scenes/Tests/PlayerMovementTest.unity");yield return Frames(8);
        Check(AchievementRuntime.Instance==runtime&&Object.FindObjectsByType<AchievementRuntime>().Length==1,"UI survives scene change without duplication");
        var player=Object.FindAnyObjectByType<Player>();var waves=Object.FindAnyObjectByType<WaveController>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Health.SetInvulnerable(true);
        yield return Until(()=>waves.Phase==WaveController.RunPhase.Combat,"first wave combat",45,true);
        // Clear actual spawned enemies; wave completion still comes from native spawner/waves events.
        double waveDeadline=EditorApplication.timeSinceStartup+50;
        while(!Achievements.Store.Get(AchievementCatalog.FirstWave).Unlocked)
        {
            if(EditorApplication.timeSinceStartup>waveDeadline)throw new TimeoutException("first wave clear");
            foreach(var enemy in Object.FindObjectsByType<Enemy>())if(!enemy.Health.IsDead)enemy.Health.TakeDamage(100000);
            yield return Frames(1);
        }
        Check(Achievements.Store.Get(AchievementCatalog.FirstWave).Unlocked,"native first-wave completion unlocks");
        Check(Achievements.Store.Get(AchievementCatalog.CleanWave).Unlocked,"native undamaged wave unlocks");
        var rewards=Object.FindAnyObjectByType<CardRewardController>();
        yield return Until(()=>rewards.CanInteract,"reward ready",12);
        Check(!view.ToastVisible&&view.PendingCount>=2,"notifications queue during frozen reward selection");
        Check(rewards.TryChoose(0),"native reward choice accepted");
        yield return Until(()=>Time.timeScale>0,"reward unfreezes game",12);
        waves.enabled=false;waves.GetComponent<EnemySpawner>().StopCombat();player.SetGameplayActive(true);
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.FirstWave&&view.ToastClock>.4f,"first popup",12);
        Check(view.ToastVisible&&view.SoundPlayCount==1,"popup visible and sound triggered once");
        Check(view.GetComponent<AudioSource>().isPlaying,"native audio source is actually playing the unlock sound");
        Check(view.GetComponent<AudioSource>().outputAudioMixerGroup!=null,"achievement sound routed through game mixer");
        yield return Capture("popup-in-game.png",1200,720);
        var toastName=runtime.transform.Find("UnlockNotification/Name").GetComponent<TMP_Text>();toastName.ForceMeshUpdate();
        Check(toastName.mesh.vertexCount>0,"popup achievement name has visible glyph geometry");
        var pause=Object.FindAnyObjectByType<PauseController>();pause.PauseGame();
        float clock=view.ToastClock;yield return Frames(8);
        Check(!view.ToastVisible&&Mathf.Approximately(clock,view.ToastClock),"pause hides and freezes popup");
        yield return Capture("pause-menu.png",1200,720);
        var pauseView=Object.FindAnyObjectByType<PauseMenuView>();pauseView.ShowAchievements();
        Check(view.IsVisible&&pause.IsPaused&&pauseView.ShowingOptions,"pause opens panel without resuming gameplay");
        yield return Capture("panel-from-pause.png",1200,720);
        pauseView.ShowMain();Check(!view.IsVisible&&pause.IsPaused,"back returns to pause");pause.ResumeGame();
        yield return Until(()=>view.CurrentToastId==null&&view.PendingCount==0,"queued popups drain",18);
        Check(view.SoundPlayCount==2,"queued achievements each play exactly one sound");
        Check(!Achievements.Award(AchievementCatalog.FirstWave)&&view.PendingCount==0,"duplicate achievement never requeues");
        player.Health.SetInvulnerable(false);
        Check(player.Dash.TryDash(),"empty dash accepted");yield return Wait(.3f);
        Check(Achievements.Store.Get(AchievementCatalog.Dodge).progress==4,"dash without avoided damage does not count");
        player.Dash.SetUpgradeMultipliers(.1f,.1f,1);
        for(int i=0;i<6;i++)
        {
            yield return Until(()=>player.Dash.IsReady,"dash ready",3);
            Check(player.Dash.TryDash(),"protected dash "+i);
            Check(!player.Health.TakeDamage(1),"real damage prevented by dash "+i);
            player.Health.TakeDamage(1);
            Check(Achievements.Store.Get(AchievementCatalog.Dodge).progress==5+i,"one count per successful dash "+i);
            yield return Wait(.23f);
        }
        Check(Achievements.Store.Get(AchievementCatalog.Dodge).Unlocked,"ten successful dodges unlock sport mode");
        var upgrades=player.GetComponent<PlayerRunUpgrades>();
        var mythic=new UpgradeCard{kind=UpgradeKind.AttackDamage,rarity=UpgradeRarity.Mythic,percent=1};
        DevelopmentBossSession.Begin(1);upgrades.Apply(mythic);
        Check(!Achievements.Award(AchievementCatalog.LastTooth)&&!Achievements.Store.Get(AchievementCatalog.Mythic).Unlocked,"boss DEV blocks achievements and applied-card progress");
        DevelopmentBossSession.End();DevelopmentCardSession.Begin();upgrades.Apply(mythic);
        Check(!Achievements.Store.Get(AchievementCatalog.Mythic).Unlocked,"card DEV blocks achievements");DevelopmentCardSession.End();
        for(int i=0;i<5;i++)upgrades.Apply(new UpgradeCard{kind=(UpgradeKind)i,rarity=UpgradeRarity.Common,percent=1});
        upgrades.Apply(mythic);
        Check(Achievements.Store.Get(AchievementCatalog.Cards).Unlocked&&Achievements.Store.Get(AchievementCatalog.Mythic).Unlocked,"real card applications record collection and mythic");

        void BossStart(Scene scene,LoadSceneMode mode)
        { SceneManager.sceneLoaded-=BossStart;Object.FindAnyObjectByType<WaveController>().StartBossForDevelopment(); }
        SceneManager.sceneLoaded+=BossStart;SceneManager.LoadScene("Assets/Scenes/Tests/PlayerMovementTest.unity");yield return Frames(8);
        player=Object.FindAnyObjectByType<Player>();waves=Object.FindAnyObjectByType<WaveController>();player.GetComponent<PlayerInputReader>().enabled=false;player.Health.SetInvulnerable(true);
        yield return Until(()=>waves.ActiveBoss!=null&&waves.ActiveBoss.Ready,"Gusini ready",45,true);
        var boss=waves.ActiveBoss;boss.Movement.HoldPosition(true);boss.transform.position=player.transform.position+Vector3.right*2.5f;
        Check(!Achievements.Store.Get(AchievementCatalog.Gusini).Unlocked,"boss encounter alone grants nothing");
        Check(player.Combat.TryShoot(),"normal shot for boss test");yield return Wait(.6f);
        Check(!Achievements.Store.Get(AchievementCatalog.Special).Unlocked,"normal hit does not unlock special achievement");
        player.Combat.ConfigureBossTestSpecial(1);Check(player.Combat.TrySpecialAttack(),"special launched at boss");
        yield return Until(()=>Achievements.Store.Get(AchievementCatalog.Special).Unlocked,"actual special-boss collision",10);
        Check(Achievements.Store.Get(AchievementCatalog.Special).Unlocked,"damaging boss with special unlocks");
        boss.Health.TakeDamage(100000);yield return Frames(2);
        Check(!Achievements.Store.Get(AchievementCatalog.Gusini).Unlocked&&!Achievements.Store.Get(AchievementCatalog.LastTooth).Unlocked,"first form is not final victory");
        yield return Until(()=>boss.GetComponent<GusiniPhaseTwo>().CurrentStage==GusiniPhaseTwo.Stage.PhaseTwo&&boss.Ready&&!boss.Health.IsInvulnerable,"second form ready",50,true);
        player.Health.SetInvulnerable(false);player.Health.TakeDamage(player.Health.PreciseCurrentHealth-player.Health.MaxHealth*.1f);player.Health.SetInvulnerable(true);
        Check(!player.Health.IsDead&&player.Health.Fraction<=.100001f,"player has exactly ten percent HP");
        boss.Health.TakeDamage(100000);yield return Frames(2);
        Check(Achievements.Store.Get(AchievementCatalog.Gusini).Unlocked,"final boss defeat unlocks Gusini");
        Check(Achievements.Store.Get(AchievementCatalog.LastTooth).Unlocked,"final boss at low HP unlocks hidden achievement");
        Check(!view.ToastVisible,"victory cinematic defers notification");
        SceneManager.LoadScene("Assets/Scenes/MainMenu.unity");yield return Frames(8);
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.LastTooth&&view.ToastClock>.4f,"hidden achievement popup",35);
        yield return Capture("hidden-unlocked-popup.png",1200,720);
        Object.FindAnyObjectByType<MainMenuController>().ShowAchievements();view.SetPage(2);yield return Frames(3);
        Check(runtime.GetComponentsInChildren<AchievementRow>().Any(r=>r.DisplayedTitle=="Por Um Dente"&&!r.HidesDetails),"hidden name and condition revealed only after unlock");
        yield return Capture("hidden-unlocked-panel.png",1200,720);
        yield return Capture("panel-1920x1080.png",1920,1080);
        view.SetFilter(1);yield return Capture("panel-unlocked.png",1200,720);view.Close();
        Check(Object.FindObjectsByType<AchievementRuntime>().Length==1,"persistent singleton remains unique after multiple loads");
        finished=true;
    }
    static void Click(UnityEngine.UI.Button button)
    {
        Check(button!=null&&button.IsInteractable(),"button interactable");
        var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    static IEnumerator Capture(string name,int width,int height)
    {
        var camera=Camera.main;bool created=camera==null;if(created)camera=new GameObject("CaptureCamera").AddComponent<Camera>();
        var overlays=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
        var rt=new RenderTexture(width,height,24);var oldTarget=camera.targetTexture;float oldAspect=camera.aspect;camera.targetTexture=rt;camera.aspect=width/(float)height;
        yield return Frames(4);Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Output+"/"+name,texture.EncodeToPNG());
        RenderTexture.active=old;camera.targetTexture=oldTarget;camera.aspect=oldAspect;
        foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
        rt.Release();Object.Destroy(rt);Object.Destroy(texture);if(created)Object.Destroy(camera.gameObject);
    }
    static IEnumerator Frames(int n){while(n-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Wait(float time){float end=Time.time+time;while(Time.time<end)yield return null;}
    static IEnumerator Until(Func<bool> test,string label,float seconds,bool skip=false)
    {double end=EditorApplication.timeSinceStartup+seconds;while(!test()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);if(skip){var dialogue=Object.FindAnyObjectByType<DialogueRunner>();if(dialogue!=null&&dialogue.IsPlaying)dialogue.Skip();}yield return Frames(1);}}
    static void Check(bool value,string label){if(!value)throw new Exception("ACHIEVEMENT_PLAY_FAILED "+label);checks++;Debug.Log("ACHIEVEMENT_CHECK "+label);}
    static void OnLog(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("achievement test deadline");
            while(Steps.Count>0){var step=Steps.Peek();if(!step.MoveNext()){Steps.Pop();continue;}if(step.Current is IEnumerator child){Steps.Push(child);continue;}return;}
        }
        catch(Exception error){errors++;Debug.LogException(error);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;Time.timeScale=1;AudioListener.pause=false;LeaderboardSession.Cancel();DevelopmentBossSession.End();DevelopmentCardSession.End();
        bool visual=SessionState.GetBool(Key+".Visual",false);
        File.WriteAllText(Output+(visual?"/visual-validation.json":"/validation.json"),"{\"dataChecks\":"+(visual?0:SessionState.GetInt(Key+".Data",0))+",\"playChecks\":"+checks+",\"errors\":"+errors+",\"finished\":"+finished.ToString().ToLowerInvariant()+"}");
        Debug.Log("ACHIEVEMENTS_PLAY_RESULT checks="+checks+" errors="+errors+" finished="+finished);EditorApplication.isPlaying=false;
    }
}
