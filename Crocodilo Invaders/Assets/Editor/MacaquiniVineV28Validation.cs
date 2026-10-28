using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class MacaquiniVineV28Validation
{
    private const string Key="Crocodilo.Vine28.Validation",Output="Docs/ArtPreviews/MacaquiniVine_v28";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors;private static double deadline;
    private static Boss boss;private static MacaquiniCombat combat;private static MacaquiniFlightPresentation actor;private static Player player;
    static MacaquiniVineV28Validation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);bool build=SessionState.GetBool(Key+".Build",false);SessionState.SetBool(Key+".Build",false);if(build&&errors==0)EditorApplication.delayCall+=MacaquiniVineV28Tools.Build;else EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run(){MacaquiniVineV28Tools.Import();EditorSceneManager.OpenScene(MacaquiniFighterV22Tools.Test);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    public static void RunAndBuild(){SessionState.SetBool(Key+".Build",true);Run();}
    private static void Begin(){checks=errors=0;deadline=EditorApplication.timeSinceStartup+140;Application.runInBackground=true;Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;}
    private static IEnumerator Scenario()
    {
        yield return Frames(4);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/JungleStage/Vine28Save");
        player=Object.FindAnyObjectByType<Player>();player.Health.SetInvulnerable(typeof(MacaquiniVineV28Validation),true);player.GetComponent<PlayerInputReader>().enabled=false;
        var waves=Object.FindAnyObjectByType<WaveController>();yield return Until(()=>waves.ActiveBoss!=null&&waves.ActiveBoss.GetComponent<MacaquiniCombat>().PhaseTwo&&waves.ActiveBoss.Ready,"second form ready");
        boss=waves.ActiveBoss;combat=boss.GetComponent<MacaquiniCombat>();actor=boss.GetComponent<MacaquiniFlightPresentation>();combat.StopCombat();boss.Movement.HoldPosition(true);yield return Frames(4);
        Check(actor.HalfShell&&actor.VineVisible,"Inspeção usa a segunda forma original");
        var ribbon=Object.FindObjectsByType<MacaquiniVineRibbon>().Single(v=>v.Visible);
        var line=ribbon.GetComponent<LineRenderer>();
        Check(line.forceRenderingOff&&ribbon.DrawnSegments==17,"Linha lisa substituída pela faixa texturizada");
        var material=ribbon.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        Check(material.mainTexture!=null&&material.mainTexture.filterMode==FilterMode.Point&&material.mainTexture.wrapModeV==TextureWrapMode.Repeat,"Textura original repetida sem esticar ou borrar");
        Check(MacaquiniVineRibbon.For(actor.CurrentSprite)!=null,"Pose atual possui encaixe medido na arte");
        Vector3 canopy=actor.CanopyAnchor;var draws=new HashSet<Sprite>();float end=Time.time+2.7f;int n=0;float next=Time.time;
        while(Time.time<end)
        {
            draws.Add(actor.CurrentSprite);
            Check(Vector3.Distance(ribbon.JoinPosition,actor.VineGripPosition)<.001f,"Emenda acompanha exatamente a ponta pintada");
            Vector3 tangent=(line.transform.TransformPoint(line.GetPosition(1))-line.transform.TransformPoint(line.GetPosition(0))).normalized;
            Check(Vector3.Dot(tangent,actor.VineJoinDirection)>.91f,"Curva sai na direção da ponta original");
            Check(actor.CanopyAnchor==canopy,"Ponta da copa permanece ancorada no balanço");
            if(Time.time>=next){Snapshot("frame_"+n.ToString("D3"),1200,720);n++;next=Time.time+.09f;}
            yield return null;
        }
        Check(draws.Count>=12,"Conferência cobre vários desenhos de balanço");
        Snapshot("cipo-1200x720",1200,720);Snapshot("cipo-800x600",800,600);Snapshot("cipo-1920x1080",1920,1080);
        boss.GetComponent<Rigidbody2D>().position+=(Vector2)new Vector3(.7f,.2f);yield return Wait(.35f);
        Check(actor.CanopyAnchor==canopy,"Deslocamento do macaco não arrasta o topo da copa");
        Vector3 join=ribbon.JoinPosition;var mesh=ribbon.GetComponentInChildren<MeshFilter>().sharedMesh;var vertices=mesh.vertices;
        Time.timeScale=0;yield return WallWait(.22f);Check(ribbon.JoinPosition==join&&mesh.vertices.SequenceEqual(vertices),"Pausa congela emenda e textura");Time.timeScale=1;
        combat.SetTarget(player.Health);combat.ForceAttack(MacaquiniCombat.Attack.SwingKick);yield return Frames(4);
        var kick=boss.GetComponent<MacaquiniSwingKick>();
        Check(kick.Running&&kick.CurrentStage==MacaquiniSwingKick.Stage.Windup&&ribbon.Visible,"Chute usa a mesma extensão pintada na preparação");
        Check(Vector3.Distance(ribbon.JoinPosition,actor.VineGripPosition)<.001f,"Pose do chute também conecta na ponta medida");Snapshot("chute-preparacao",1200,720);
        yield return Until(()=>kick.CurrentStage==MacaquiniSwingKick.Stage.Kick,"kick releases vine");yield return Frames(3);
        Check(!ribbon.Visible,"Cipó solta sem sobra durante o chute");
        yield return Until(()=>kick.CurrentStage==MacaquiniSwingKick.Stage.Catch&&ribbon.Visible,"kick catch");Snapshot("chute-retomada",1200,720);
        Check(Vector3.Distance(ribbon.JoinPosition,actor.VineGripPosition)<.001f,"Retomada do chute fecha a emenda sem salto");
        combat.StopCombat();yield return Frames(3);combat.SetTarget(player.Health);combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);yield return Frames(5);
        var visible=Object.FindObjectsByType<MacaquiniVineRibbon>().Where(v=>v.Visible&&v.UsesPoseJoin).ToArray();
        Check(visible.Length==1&&visible[0].name=="MacaquiniControlVine","Pêndulo exibe uma única ligação superior, sem cipó duplicado");
        Check(Vector3.Distance(visible[0].JoinPosition,actor.VineGripPosition)<.001f,"Controle do pêndulo mantém encaixe pintado");Snapshot("pendulo-encaixe",1200,720);
        boss.StopBattle();yield return Frames(3);Check(Object.FindObjectsByType<MacaquiniVineRibbon>().All(v=>!v.Visible),"Fim da luta remove as extensões sem restos");
    }
    private static void Snapshot(string name,int width,int height)
    {
        var camera=Camera.main;var texture=new RenderTexture(width,height,24);var old=camera.targetTexture;var active=RenderTexture.active;
        var overlays=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
        camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=old;RenderTexture.active=active;foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}texture.Release();Object.Destroy(texture);Object.Destroy(pixels);
    }
    private static IEnumerator Frames(int n){while(n-->0)yield return null;}
    private static IEnumerator Wait(float n){float end=Time.time+n;while(Time.time<end)yield return null;}
    private static IEnumerator WallWait(float n){double end=EditorApplication.timeSinceStartup+n;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static IEnumerator Until(Func<bool> p,string label){double end=EditorApplication.timeSinceStartup+30;while(!p()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    private static void Check(bool value,string label){if(!value)throw new InvalidOperationException("VINE28_FAIL "+label);checks++;Debug.Log("VINE28_CHECK "+label);}
    private static void OnLog(string m,string t,LogType type){if(t.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Vine28 deadline");while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}Debug.Log("MACAQUINI_VINE28_PLAY_OK checks="+checks+" errors="+errors);}
        catch(Exception e){errors++;Debug.LogException(e);}EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;Time.timeScale=1;File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+"}");EditorApplication.isPlaying=false;
    }
}
