using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class CleanCardUpgradeValidation
{
    const string SessionKey="Crocodilo.CleanCards.Validation",Output="Docs/ArtPreviews/CleanCardsTab_v02";
    static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    static int checks,errors;static bool finished;static double deadline,ready;static Keyboard keyboard;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static CleanCardUpgradeValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(SessionKey,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(SessionKey,false);if(Application.isBatchMode)EditorApplication.Exit(finished&&errors==0?0:1);}
        };
    }
    public static void Run(){ready=EditorApplication.timeSinceStartup+5;EditorApplication.update+=StartWhenReady;}
    static void StartWhenReady()
    {
        if(EditorApplication.timeSinceStartup<ready||EditorApplication.isUpdating||EditorApplication.isCompiling)return;
        EditorApplication.update-=StartWhenReady;CleanCardUpgradeTools.Install();
        EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");Object.FindAnyObjectByType<WaveController>().enabled=false;
        SessionState.SetBool(SessionKey,true);EditorApplication.isPlaying=true;
    }
    static void Begin()
    {
        checks=errors=0;finished=false;deadline=EditorApplication.timeSinceStartup+160;Application.runInBackground=true;Directory.CreateDirectory(Output);
        Achievements.UseDirectory("Logs/CleanCards/Achievements-"+Guid.NewGuid().ToString("N"));CampaignProgress.UseDirectory("Logs/CleanCards/Campaign-"+Guid.NewGuid().ToString("N"));LeaderboardSession.Cancel();
        oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard=InputSystem.AddDevice<Keyboard>("CleanCardsKeyboard");InputSystem.EnableDevice(keyboard);
        DevelopmentCardSession.Begin();DevelopmentCardSession.Mode=DevelopmentCardSession.OfferMode.FixedRarity;DevelopmentCardSession.Rarity=UpgradeRarity.Common;
        steps.Clear();steps.Push(Scenario());Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;
    }
    static IEnumerator Scenario()
    {
        yield return Frames(5);
        var summary=Object.FindAnyObjectByType<RunUpgradeView>();
        Check(summary!=null&&!summary.IsVisible,"Tab HUD starts hidden without a permanent shortcut");
        Check(summary.GetComponentsInChildren<UnityEngine.UI.Button>().Length==0,"no visible HUD buttons");
        Check(!summary.GetComponent<CanvasGroup>().blocksRaycasts,"status HUD does not intercept gameplay pointer");
        var waves=Object.FindAnyObjectByType<WaveController>();waves.enabled=true;yield return Frames(2);Check(waves.StartCardDevelopment(),"real post-wave card selection");
        var rewards=Object.FindAnyObjectByType<CardRewardController>();var cards=Object.FindAnyObjectByType<CardRewardView>();
        yield return Until(()=>rewards.CanInteract,"cards ready");yield return Frames(2);
        var texts=cards.GetComponentsInChildren<TMP_Text>(true);
        Check(texts.Single(t=>t.name=="Title").text=="ESCOLHA UMA MELHORIA","one clear heading");
        Check(new[]{"Hint","Controls","Totals"}.All(n=>!texts.Single(t=>t.name==n).gameObject.activeSelf),"redundant hint, controls and totals removed");
        Check(texts.Count(t=>t.name=="Comparison"&&t.gameObject.activeInHierarchy)==1,"only focused card displays comparison");
        Check(texts.Where(t=>t.name=="Comparison").All(t=>!t.text.Contains("Total:")&&!t.text.Contains("\n")),"one-line comparison without duplicate totals");
        EventSystem.current.SetSelectedGameObject(cards.Options[0].Button.gameObject);yield return Frames(2);
        Check(cards.Options[0].GetComponentsInChildren<TMP_Text>().Any(t=>t.name=="Comparison"),"comparison follows selection");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));yield return Frames(3);Check(!summary.IsVisible,"Tab does not cover card choice");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        foreach(var size in new[]{new Vector2Int(1200,720),new Vector2Int(800,600),new Vector2Int(1920,1080)})
        {yield return Capture("cards-clean-"+size.x+"x"+size.y+".png",size.x,size.y);CheckText(cards);}
        DevelopmentCardSession.Rarity=UpgradeRarity.Legendary;Check(rewards.TryReroll(),"legendary reroll");yield return Until(()=>rewards.CanInteract,"legendary ready");
        yield return Capture("cards-legendary.png",1200,720);
        DevelopmentCardSession.Rarity=UpgradeRarity.Mythic;Check(rewards.TryReroll(),"mythic reroll");yield return Until(()=>rewards.CanInteract,"mythic ready");
        Check(cards.Options.All(o=>o.Rarity==UpgradeRarity.Mythic&&o.HasRarityFeedback),"rarity feedback preserved");
        DevelopmentCardSession.End();Check(rewards.TryReroll(),"one normal reroll");yield return Until(()=>rewards.CanInteract,"normal reroll ready");
        Check(rewards.RerollsRemaining==0&&!rewards.TryReroll(),"normal reroll stays limited to one");
        Check(texts.Single(t=>t.name=="Label"&&t.transform.parent.name=="Reroll").text=="TROCAR CARTAS (0)","clear disabled reroll count");
        Check(rewards.TryChoose(0),"real choice still applies upgrade");yield return Until(()=>rewards.State==CardRewardController.RewardState.Closed,"choice complete");
        Check(Object.FindAnyObjectByType<PlayerRunUpgrades>().AcquiredCount==1,"exactly one chosen card applied");
        // Start the normal first wave in a fresh scene; skip only its dialogue for QA.
        DevelopmentCardSession.Begin();var load=SceneManager.LoadSceneAsync("Assets/Scenes/Tests/PlayerMovementTest.unity");yield return Until(()=>load.isDone,"fresh combat scene");yield return Frames(5);
        Check(summary==null,"previous scene status HUD cleaned up");
        summary=Object.FindAnyObjectByType<RunUpgradeView>();waves=Object.FindAnyObjectByType<WaveController>();
        waves.enabled=true;yield return Frames(2);DevelopmentCardSession.End();
        typeof(WaveController).GetMethod("BeginRun",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(waves,new object[]{false,0});
        yield return Until(()=>waves.Phase==WaveController.RunPhase.Combat,"normal first-wave combat");
        var upgrades=Object.FindAnyObjectByType<PlayerRunUpgrades>();
        foreach(var card in new[]{new UpgradeCard{kind=UpgradeKind.AttackDamage,rarity=UpgradeRarity.Rare,percent=15},new UpgradeCard{kind=UpgradeKind.AttackDamage,rarity=UpgradeRarity.Epic,percent=20},
            new UpgradeCard{kind=UpgradeKind.MaximumHealth,rarity=UpgradeRarity.Epic,percent=20},new UpgradeCard{kind=UpgradeKind.AttackSpeed,rarity=UpgradeRarity.Legendary,percent=30},
            new UpgradeCard{kind=UpgradeKind.MoveSpeed,rarity=UpgradeRarity.Rare,percent=6},new UpgradeCard{kind=UpgradeKind.SpecialDamage,rarity=UpgradeRarity.Mythic,percent=50}})
            Check(upgrades.Apply(card),"applied "+card.kind);
        var pause=Object.FindAnyObjectByType<PauseController>();var input=upgrades.GetComponent<PlayerInputReader>();var combat=upgrades.GetComponent<PlayerCombat>();
        Check(summary.CanShow&&input.enabled,"Tab available during normal combat");
        yield return Capture("combat-no-shortcut.png",1200,720);
        Vector3 position=upgrades.transform.position;float time=Time.time;int shots=0;Action shot=()=>shots++;combat.ShotFired+=shot;
        var selected=EventSystem.current.currentSelectedGameObject;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab,Key.RightArrow,Key.Space));
        double until=EditorApplication.timeSinceStartup+.4;while(EditorApplication.timeSinceStartup<until)yield return Frames(1);
        Check(summary.IsVisible,"holding Tab opens status HUD");
        Check(Time.timeScale==1&&!pause.IsPaused&&!AudioListener.pause&&input.enabled,"combat, sound and player input remain live");
        Check(Time.time>time+.1f&&Vector3.Distance(position,upgrades.transform.position)>.05f,"game time and player movement advance under the HUD; dt="+(Time.time-time)+", movement="+Vector3.Distance(position,upgrades.transform.position));
        Check(shots>0,"player can shoot with Tab held");combat.ShotFired-=shot;
        Check(EventSystem.current.currentSelectedGameObject==selected,"Tab does not steal UI focus");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));yield return Frames(2);
        Check(summary.GetComponentsInChildren<TMP_Text>().All(t=>!t.text.Contains("TAB")&&!t.text.Contains("VOLTAR")),"only status text, without input hints or modal controls");
        var panel=summary.transform.Find("Content/Panel");Check(panel.GetComponent<UnityEngine.UI.Image>().color.a<=.2f,"panel background almost transparent");
        Check(panel.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i=>i.name.StartsWith("Pip"))==100,"five segmented status bars");
        foreach(var size in new[]{new Vector2Int(1200,720),new Vector2Int(800,600),new Vector2Int(1920,1080)})
        {yield return Capture("tab-status-"+size.x+"x"+size.y+".png",size.x,size.y);CheckText(summary);}
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Frames(3);Check(!summary.IsVisible,"release Tab closes immediately");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));yield return Frames(3);pause.PauseGame();yield return Frames(3);
        Check(!summary.IsVisible&&pause.IsPaused,"real pause hides the live status HUD");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());pause.ResumeGame();yield return Frames(3);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));yield return Frames(3);upgrades.ResetForRun();yield return Frames(2);
        Check(upgrades.AcquiredCount==0&&summary.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="EquippedCount").text=="0 MELHORIAS","new run clears status immediately");
        upgrades.GetComponent<Health>().TakeDamage(99999);yield return Frames(3);Check(!summary.IsVisible,"death hides status HUD");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());finished=true;
    }
    static void CheckText(Component root){foreach(var text in root.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"text fits: "+text.name);}}
    private static IEnumerator Capture(string name,int width,int height)
    {
        var camera=Camera.main;var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
        var rt=new RenderTexture(width,height,24);var old=camera.targetTexture;float aspect=camera.aspect;camera.targetTexture=rt;camera.aspect=width/(float)height;
        yield return Frames(4);Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Output+"/"+name,texture.EncodeToPNG());
        RenderTexture.active=active;camera.targetTexture=old;camera.aspect=aspect;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
        rt.Release();Object.Destroy(rt);Object.Destroy(texture);
    }
    private static IEnumerator Frames(int count){while(count-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    private static IEnumerator Until(Func<bool> condition,string label){double end=EditorApplication.timeSinceStartup+20;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return Frames(1);}}
    private static void Check(bool value,string label){if(!value)throw new Exception("CLEAN_CARDS_FAILED "+label);checks++;Debug.Log("CLEAN_CARDS_CHECK "+label);}
    private static void OnLog(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick()
    {
        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("clean cards validation");while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}}
        catch(Exception e){errors++;Debug.LogException(e);}
        EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
        if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
        InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
        DevelopmentCardSession.End();LeaderboardSession.Cancel();
        File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+",\"finished\":"+finished.ToString().ToLowerInvariant()+"}");
        Debug.Log("CLEAN_CARDS_RESULT checks="+checks+" errors="+errors+" finished="+finished);EditorApplication.isPlaying=false;
    }
}
