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
public static class MacaquiniMapVinesV29Validation
{
    private const string Key="Crocodilo.MapVines29.Validation",Output="Docs/ArtPreviews/MacaquiniMapVines_v29";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors;private static double deadline;
    private static Boss boss;private static MacaquiniCombat combat;private static MacaquiniFlightPresentation actor;private static Player player;
    static MacaquiniMapVinesV29Validation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);bool build=SessionState.GetBool(Key+".Build",false);SessionState.SetBool(Key+".Build",false);if(build&&errors==0)EditorApplication.delayCall+=MacaquiniMapVinesV29Tools.Build;else EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run(){MacaquiniMapVinesV29Tools.Import();EditorSceneManager.OpenScene(MacaquiniFighterV22Tools.Test);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    public static void RunAndBuild(){SessionState.SetBool(Key+".Build",true);Run();}
    private static void Begin(){checks=errors=0;deadline=EditorApplication.timeSinceStartup+140;Application.runInBackground=true;Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;}
    private static JungleVineHazard Spawn(Vector2 from,Vector2 to,float warning=.65f,float active=.6f)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<JungleVineHazard>("Assets/Prefabs/Boss/JungleVine.prefab");
        var vine=Object.Instantiate(prefab);vine.Configure(from,to,player.Health,warning,active);return vine;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(4);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/JungleStage/MapVines29Save");
        player=Object.FindAnyObjectByType<Player>();player.GetComponent<PlayerInputReader>().enabled=false;player.Health.SetInvulnerable(typeof(MacaquiniMapVinesV29Validation),true);
        var waves=Object.FindAnyObjectByType<WaveController>();yield return Until(()=>waves.ActiveBoss!=null&&waves.ActiveBoss.GetComponent<MacaquiniCombat>().PhaseTwo&&waves.ActiveBoss.Ready,"second form ready");
        boss=waves.ActiveBoss;combat=boss.GetComponent<MacaquiniCombat>();actor=boss.GetComponent<MacaquiniFlightPresentation>();combat.StopCombat();boss.Movement.HoldPosition(true);yield return Frames(4);
        var upper=Object.FindObjectsByType<MacaquiniVineRibbon>().Single(v=>v.Visible&&v.UsesPoseJoin);
        Check(Vector3.Distance(upper.JoinPosition,actor.VineGripPosition)<.001f,"Emenda aprovada da mão preservada");
        player.GetComponent<Rigidbody2D>().position=Vector2.zero;player.Health.SetInvulnerable(typeof(MacaquiniMapVinesV29Validation),false);int hp=player.Health.CurrentHealth;
        var vine=Spawn(new Vector2(8,0),new Vector2(-8,0));yield return Frames(3);
        var ribbon=vine.GetComponentInChildren<MacaquiniVineRibbon>();
        Check(!vine.Dangerous&&!ribbon.Visible&&vine.GetComponent<LineRenderer>().enabled,"Aviso dourado é separado do cipó ativo");
        yield return Wait(.2f);Check(player.Health.CurrentHealth==hp,"Aviso continua sem dano");Snapshot("aviso-horizontal");
        yield return Until(()=>vine.Dangerous,"horizontal activates");yield return Frames(3);
        Check(ribbon.Visible&&ribbon.DrawnSegments==1,"Cipó horizontal usa uma faixa contínua");
        Check(vine.GetComponentsInChildren<SpriteRenderer>().Length==0,"Sem pedaços sobrepostos ou tremendo separadamente");
        Check(!vine.GetComponent<LineRenderer>().enabled,"Aviso desaparece quando a textura ativa entra");
        var renderer=ribbon.GetComponentInChildren<MeshRenderer>();
        Check(renderer.sharedMaterial.mainTexture==upper.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture,"Ataques usam a mesma casca aprovada na mão");
        var mesh=ribbon.GetComponentInChildren<MeshFilter>().sharedMesh;
        Check(mesh.uv.Max(v=>v.y)>30,"Casca se repete ao longo do mapa sem ser esticada");
        Check(player.Health.CurrentHealth==hp-2,"Contato real do horizontal mantém dois de dano");
        Snapshot("horizontal-continuo");var frozen=mesh.vertices;Time.timeScale=0;yield return WallWait(.2f);
        Check(mesh.vertices.SequenceEqual(frozen)&&vine.Dangerous,"Pausa congela malha e duração do ataque");Time.timeScale=1;
        yield return Wait(.15f);Check(player.Health.CurrentHealth==hp-2,"Mesmo cipó não repete dano");
        yield return Until(()=>vine==null,"horizontal expires");yield return Frames(2);
        Check(!Object.FindObjectsByType<MacaquiniVineRibbon>().Any(v=>!v.UsesPoseJoin),"Horizontal expira sem deixar malha órfã");
        hp=player.Health.CurrentHealth;var top=Spawn(new Vector2(8,1.5f),new Vector2(-8,1.5f),.3f,.5f);var bottom=Spawn(new Vector2(8,-1.5f),new Vector2(-8,-1.5f),.3f,.5f);
        yield return Wait(.4f);Snapshot("duas-faixas-corredor");Check(player.Health.CurrentHealth==hp,"Corredor livre entre os cipós mantém esquiva segura");
        yield return Until(()=>top==null&&bottom==null,"lanes expire");
        combat.SetTarget(player.Health);combat.ForceAttack(MacaquiniCombat.Attack.Pendulum);yield return Frames(5);
        var swing=combat.Pendulum;var attack=Object.FindObjectsByType<MacaquiniVineRibbon>().Single(v=>!v.UsesPoseJoin);
        Check(attack.name=="PendulumBark"&&attack.Visible,"Pêndulo também usa casca contínua durante a preparação");
        Check(swing.GetComponentsInChildren<SpriteRenderer>().Length==0&&swing.PaintedSegments==32,"Pêndulo sem pilha de sprites repetidos");
        Check(Object.FindObjectsByType<MacaquiniVineRibbon>().Count(v=>v.Visible&&v.UsesPoseJoin)==1,"Ligação da mão permanece única e íntegra");
        player.Health.SetInvulnerable(typeof(MacaquiniMapVinesV29Validation),false);hp=player.Health.CurrentHealth;
        yield return Until(()=>swing.Dangerous,"swing active");yield return Frames(2);
        Check(attack.Visible&&attack.Tint.a==1,"Pêndulo ativo tem textura opaca, sem núcleo luminoso");Snapshot("pendulo-continuo");
        float end=Time.time+.45f;int index=0;
        while(Time.time<end){Snapshot("pendulo-frame_"+index.ToString("D3"));index++;yield return Wait(.04f);}
        yield return Until(()=>player.Health.CurrentHealth<hp,"swing real collision");Check(player.Health.CurrentHealth==hp-2,"Varredura do pêndulo mantém dano e colisão reais");
        player.Health.SetInvulnerable(typeof(MacaquiniMapVinesV29Validation),true);
        yield return Frames(3);var control=Object.FindObjectsByType<MacaquiniVineRibbon>().Single(v=>v.Visible&&v.UsesPoseJoin);
        Check(Vector3.Distance(control.JoinPosition,actor.VineGripPosition)<.001f,"Junção da mão permanece alinhada durante a varredura");
        combat.StopCombat();yield return Frames(3);Check(Object.FindObjectsByType<MacaquiniVineRibbon>().All(v=>v.UsesPoseJoin),"Cancelar remove o cipó do mapa e preserva o da mão");
        combat.SetTarget(player.Health);combat.ForceAttack(MacaquiniCombat.Attack.Vines);yield return Wait(1.2f);Snapshot("padrao-cipos");
        Check(Object.FindObjectsByType<JungleVineHazard>().Length==2&&Object.FindObjectsByType<MacaquiniVineRibbon>().Count(v=>v.Visible&&!v.UsesPoseJoin)==2,"Padrão real do boss usa duas faixas contínuas e uma faixa livre");
        boss.StopBattle();yield return Frames(3);Check(Object.FindObjectsByType<MacaquiniVineRibbon>().All(v=>!v.Visible),"Fim da batalha limpa todos os cipós ativos");
    }
    private static void Snapshot(string name)
    {
        var camera=Camera.main;var texture=new RenderTexture(1200,720,24);var old=camera.targetTexture;var active=RenderTexture.active;
        var overlays=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
        camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;var pixels=new Texture2D(1200,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1200,720),0,0);pixels.Apply();File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=old;RenderTexture.active=active;foreach(var canvas in overlays){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}texture.Release();Object.Destroy(texture);Object.Destroy(pixels);
    }
    private static IEnumerator Frames(int n){while(n-->0)yield return null;}
    private static IEnumerator Wait(float n){float end=Time.time+n;while(Time.time<end)yield return null;}
    private static IEnumerator WallWait(float n){double end=EditorApplication.timeSinceStartup+n;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static IEnumerator Until(Func<bool> p,string label){double end=EditorApplication.timeSinceStartup+30;while(!p()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    private static void Check(bool value,string label){if(!value)throw new InvalidOperationException("MAPVINES29_FAIL "+label);checks++;Debug.Log("MAPVINES29_CHECK "+label);}
    private static void OnLog(string m,string t,LogType type){if(t.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick(){EditorApplication.QueuePlayerLoopUpdate();try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("MapVines29 deadline");while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}Debug.Log("MACAQUINI_MAPVINES29_PLAY_OK checks="+checks+" errors="+errors);}catch(Exception e){errors++;Debug.LogException(e);}EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;Time.timeScale=1;File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+"}");EditorApplication.isPlaying=false;}
}
