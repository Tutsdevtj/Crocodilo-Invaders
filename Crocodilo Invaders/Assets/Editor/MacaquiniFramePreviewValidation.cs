using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class MacaquiniFramePreviewValidation
{
    private const string Key="Crocodilo.Macaquini.FullFrames.Test";
    private const string Output="Docs/ArtPreviews/Macaquini_v08";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors;
    private static bool running;
    private static double deadline;
    private static MacaquiniFrameAnimation actor;
    private static MacaquiniFramePreviewUI ui;
    static MacaquiniFramePreviewValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run()
    {
        MacaquiniFramePreviewTools.Validate();EditorSceneManager.OpenScene(MacaquiniFramePreviewTools.ScenePath);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    private static void Begin()
    {
        checks=errors=0;running=true;deadline=EditorApplication.timeSinceStartup+180;Application.runInBackground=true;Directory.CreateDirectory(Output);
        Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(5);actor=Object.FindAnyObjectByType<MacaquiniFrameAnimation>();ui=Object.FindAnyObjectByType<MacaquiniFramePreviewUI>();
        Check(actor!=null&&ui.PreviewCanvas!=null,"Cena abre diretamente na bancada de animação");
        Check(actor.TotalDrawings==56,"56 desenhos completos disponíveis");
        Check(actor.GetComponentsInChildren<SpriteRenderer>().Length==1&&actor.transform.childCount==0,"Um único sprite para corpo, rosto, arma e cipó; nenhum membro montado");
        Check(Object.FindAnyObjectByType<Boss>()==null&&Object.FindAnyObjectByType<WaveController>()==null&&Object.FindAnyObjectByType<ChapterFlow>()==null,"Teste isolado de combate e progresso da campanha");
        Check(EventSystem.current.currentInputModule is InputSystemUIInputModule module&&module.point.action!=null,"Mouse e teclado configurados no Input System");
        Canvas.ForceUpdateCanvases();var button=FindButton("Attack");var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Check(hits.Any(h=>h.gameObject.GetComponentInParent<UnityEngine.UI.Button>()==button),"Botão de disparo recebe raycast real do mouse");
        var jungle=Object.FindAnyObjectByType<JungleBackdrop>();ui.Pause(true);yield return Frames(3);float clock=jungle.WindClock;int frame=actor.Frame;yield return Wait(.2f);
        Check(actor.Frame==frame&&jungle.WindClock==clock,"Pausa congela quadros e selva");
        Click("Next");Check(actor.Frame==(frame+1)%16&&actor.Paused,"Avança um quadro sem reproduzir");Click("Previous");Check(actor.Frame==frame,"Volta um quadro");
        Click("Frame7");Check(actor.Frame==7&&actor.Paused,"Clique na tira de quadros seleciona a pose exata");
        Click("Slower");Check(actor.Speed==.75f,"Controle reduz velocidade");Click("Faster");Check(actor.Speed==1,"Controle restaura velocidade");
        Click("Forma2");Check(actor.HalfShell&&actor.CurrentMotion==MacaquiniFrameAnimation.Motion.Swing,"Forma2 abre o macaco parcialmente libertado");
        Click("Forma1");Check(!actor.HalfShell,"Forma1 volta à banana fechada");
        Click("Loop");Check(actor.RepeatAttack,"Repetição de ataque pode ser ligada");Click("Loop");
        yield return Snapshot(1200,720,"preview_closed");ui.Pause(false);
        yield return Record();
        Click("Forma2");actor.Seek(5);ui.Pause(true);
        yield return Snapshot(1200,720,"preview_half");yield return Snapshot(800,600,"preview_800x600");yield return Snapshot(1920,1080,"preview_1920x1080");ui.Pause(false);
    }
    private static IEnumerator Record()
    {
        const string directory="Logs/JungleStage/FullFrameMotion";Directory.CreateDirectory(directory);
        var camera=Camera.main;var canvas=ui.PreviewCanvas;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        var texture=new RenderTexture(1200,720,24);var pixels=new Texture2D(1200,720,TextureFormat.RGB24,false);camera.targetTexture=texture;
        Click("Forma1");actor.Speed=1;ui.Pause(false);yield return Frames(2);
        float start=Time.unscaledTime,next=0;int count=0,stage=0;var times=new List<float>();var seen=new HashSet<string>();
        while(Time.unscaledTime-start<24)
        {
            float t=Time.unscaledTime-start;
            if(stage==0&&t>=4){Click("Attack");stage++;}
            if(stage==1&&t>=6){Click("Tear");stage++;}
            if(stage==2&&t>=12){Check(actor.HalfShell,"Rasgo termina na forma2 e retorna ao balanço");Click("Attack");stage++;}
            if(stage==3&&t>=14){Click("Swing");stage++;}
            if(stage==4&&t>=18){Click("Next");stage++;}
            if(stage==5&&t>=20){ui.Pause(false);Click("Forma1");stage++;}
            seen.Add(actor.CurrentSprite.name);
            if(t>=next)
            {
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=texture;pixels.ReadPixels(new Rect(0,0,1200,720),0,0);pixels.Apply();RenderTexture.active=null;
                File.WriteAllBytes(directory+"/frame_"+count.ToString("D5")+".png",pixels.EncodeToPNG());times.Add(t);count++;next+=.05f;
            }
            yield return null;
        }
        var concat=new System.Text.StringBuilder("ffconcat version 1.0\n");for(int i=0;i<count;i++){concat.Append("file 'frame_").Append(i.ToString("D5")).Append(".png'\n");concat.Append("duration ").Append((i+1<count?times[i+1]-times[i]:.05f).ToString("F6",System.Globalization.CultureInfo.InvariantCulture)).Append('\n');}File.WriteAllText(directory+"/frames.ffconcat",concat.ToString());
        camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(pixels);texture.Release();Object.Destroy(texture);
        Check(seen.Count==56,"Os56quadros distintos apareceram no PlayerLoop real: "+seen.Count);Check(count>200,"Captura real com "+count+" quadros");
        Check(actor.CompletedLoops>3,"Loops completos e retorno das ações ao balanço");
    }
    private static IEnumerator Snapshot(int width,int height,string name)
    {
        var camera=Camera.main;var canvas=ui.PreviewCanvas;var texture=new RenderTexture(width,height,24);var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=texture;yield return Frames(2);Canvas.ForceUpdateCanvases();camera.Render();
        RenderTexture.active=texture;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();RenderTexture.active=null;
        File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());
        var bounds=actor.GetComponent<SpriteRenderer>().bounds;Vector3 point=camera.WorldToViewportPoint(bounds.center);Check(point.x>.1f&&point.x<.9f,"Ator enquadrado em "+width+"x"+height);
        camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(pixels);texture.Release();Object.Destroy(texture);
    }
    private static UnityEngine.UI.Button FindButton(string name)=>ui.PreviewCanvas.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name==name);
    private static void Click(string name)=>FindButton(name).onClick.Invoke();
    private static void Check(bool value,string message){if(!value)throw new InvalidOperationException("FULL_FRAMES_CHECK_FAILED "+message);checks++;Debug.Log("FULL_FRAMES_CHECK "+message);}
    private static IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    private static IEnumerator Wait(float seconds){double end=EditorApplication.timeSinceStartup+seconds;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static void Tick()
    {
        if(!running)return;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Full frames preview timeout");
            while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}
            Debug.Log("MACAQUINI_FULL_FRAMES_PLAY_OK checks="+checks+" errors="+errors);Finish();
        }
        catch(Exception error){errors++;Debug.LogException(error);Finish();}
    }
    private static void Finish(){running=false;EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;Time.timeScale=1;EditorApplication.isPlaying=false;}
    private static void OnLog(string text,string trace,LogType type){if(trace.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
}
