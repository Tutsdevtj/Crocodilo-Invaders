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
public static class MacaquiniFighterPlayValidation
{
    private const string Key="Crocodilo.Fighter22.Validation",Output="Docs/ArtPreviews/MacaquiniFighter_v22",Recording="Logs/JungleStage/Fighter22";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static int checks,errors;
    private static double deadline;
    private static Player player;
    private static Boss boss;
    private static MacaquiniCombat combat;
    private static MacaquiniFighter fighter;
    private static Camera camera;
    private static RenderTexture capture;
    private static Texture2D pixels;
    private static JungleMotionAudioCapture audio;
    private static List<float> times;
    private static float recordStart,nextFrame;
    private static bool captureSuspended;
    static MacaquiniFighterPlayValidation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(errors==0?0:1);}
        };
    }
    public static void Run(){EditorSceneManager.OpenScene(MacaquiniFighterV22Tools.Test);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    private static void Begin()
    {
        checks=errors=0;deadline=EditorApplication.timeSinceStartup+240;Application.runInBackground=true;
        Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;steps.Clear();steps.Push(Scenario());EditorApplication.update+=Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(4);LeaderboardSession.Cancel();CampaignProgress.UseDirectory("Logs/JungleStage/Fighter22Save");
        player=Object.FindAnyObjectByType<Player>();player.Health.SetInvulnerable(typeof(MacaquiniFighterPlayValidation),true);player.GetComponent<PlayerInputReader>().enabled=false;
        var waves=Object.FindAnyObjectByType<WaveController>();yield return Until(()=>waves.ActiveBoss!=null,"boss spawn");boss=waves.ActiveBoss;
        yield return Until(()=>boss.GetComponent<MacaquiniPhaseTwo>().CurrentStage==MacaquiniPhaseTwo.Stage.HalfShell&&boss.Ready,"automatic second form");
        combat=boss.GetComponent<MacaquiniCombat>();fighter=boss.GetComponent<MacaquiniFighter>();combat.StopCombat();
        Check(fighter!=null&&fighter.enabled&&combat.PhaseTwo,"Arena entra na segunda forma lutadora automaticamente");
        Check(player.Health.MaxHealth==24&&!player.Health.IsDead,"Treino usa 24 HP, cura e proteção opcionais");
        foreach(string group in new[]{"fighter","helper"})for(int i=0;i<6;i++)
        {
            string path="Assets/Art/JungleStage/Macaquini_v22/"+group+"_"+i.ToString("D2")+".png";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Check(sprite!=null&&sprite.pixelsPerUnit==64&&importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&importer.textureCompression==TextureImporterCompression.Uncompressed,"Pixel art íntegra e sem filtro: "+group+i);
        }
        StartRecord();
        yield return Force(MacaquiniCombat.Attack.FighterJab);
        Check(fighter.CurrentState==MacaquiniFighter.State.Cue&&!fighter.Dangerous,"Soco anuncia antes de causar dano");
        Check(boss.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite.name=="fighter_00","Soco mostra a guarda do corpo completo");
        float fixedTime=Time.time;Vector3 fixedPosition=boss.transform.position;Time.timeScale=0;yield return WallWait(.2f);
        Check(Time.time==fixedTime&&Vector3.Distance(fixedPosition,boss.transform.position)<.001f,"Pausa congela aviso e posição");Time.timeScale=1;
        int hp=player.Health.CurrentHealth;player.Health.SetInvulnerable(typeof(MacaquiniFighterPlayValidation),false);
        yield return Snapshot("jab_warning");yield return Until(()=>fighter.DamageHits>0,"real jab contact");
        Check(player.Health.CurrentHealth==hp-1,"Soco aplica um de dano com colisão real");yield return Wait(.16f);
        Check(player.Health.CurrentHealth==hp-1,"Mesmo soco não aplica dano repetido");player.Health.SetInvulnerable(typeof(MacaquiniFighterPlayValidation),true);
        combat.StopCombat();yield return Frames(3);
        yield return Force(MacaquiniCombat.Attack.FighterJab);hp=player.Health.CurrentHealth;
        var locked=fighter.LockedAim;player.Health.SetInvulnerable(typeof(MacaquiniFighterPlayValidation),false);captureSuspended=true;
        player.Movement.SetMoveInput(Vector2.up);Check(player.TryDash(),"Dash real disponível para escapar da mira travada");
        yield return Until(()=>fighter.StrikesStarted>1,"second jab starts");yield return Wait(.4f);captureSuspended=false;player.Movement.SetMoveInput(Vector2.zero);
        Check(player.Health.CurrentHealth==hp&&Vector2.Distance(locked,fighter.LockedAim)<.001f,"Esquiva muda a posição da nave e o soco não persegue");
        player.Health.SetInvulnerable(typeof(MacaquiniFighterPlayValidation),true);combat.StopCombat();yield return Frames(3);
        yield return Force(MacaquiniCombat.Attack.FighterUppercut);yield return Snapshot("uppercut_cue");
        Check(fighter.CurrentMove==MacaquiniFighter.Move.Uppercut&&boss.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite.name=="fighter_03","Gancho anuncia com punho elevado e postura distinta");
        yield return Until(()=>fighter.Dangerous,"uppercut strike");yield return Snapshot("uppercut_strike");combat.StopCombat();yield return Frames(3);
        yield return Force(MacaquiniCombat.Attack.FighterHammer);yield return Snapshot("hammer_cue");
        Check(boss.transform.Find("CompletePaintedBody").GetComponent<SpriteRenderer>().sprite.name=="fighter_04","Martelo anuncia com as duas mãos acima da cabeça");
        yield return Until(()=>fighter.Dangerous,"hammer strike");yield return Snapshot("hammer_strike");combat.StopCombat();yield return Frames(3);
        var orders=new HashSet<int>();int rounds=fighter.CompletedRounds;int observed=0;
        while(orders.Count<2)
        {
            yield return Force(MacaquiniCombat.Attack.FighterCombo);bool sampled=false;
            while(fighter.Running)
            {
                if(fighter.CurrentState==MacaquiniFighter.State.Strike)
                {orders.Add(fighter.ComboId);observed++;Check(fighter.LastDangerGap>=1.1f,"Intervalo entre ameaças respeita a recarga do dash");if(!sampled){yield return Snapshot("combo_"+orders.Count);sampled=true;}}
                Check(Object.FindObjectsByType<MacaquiniFighterMinion>(FindObjectsSortMode.None).Count(h=>h.Dangerous)<=1,"No máximo um ajudante ataca por vez");
                yield return Wait(.18f);
            }
            combat.StopCombat();yield return Frames(3);
        }
        Check(orders.Count>=2&&observed>=2,"Combos variam a ordem dos golpes em rodadas reais");
        boss.Health.TakeDamage(100);yield return Force(MacaquiniCombat.Attack.FighterHelpers);
        yield return Until(()=>Object.FindObjectsByType<MacaquiniFighterMinion>(FindObjectsSortMode.None).Length==3,"three helpers");yield return Wait(1);yield return Snapshot("three_helpers");
        var helpers=Object.FindObjectsByType<MacaquiniFighterMinion>(FindObjectsSortMode.None);
        Check(helpers.Select(h=>h.Role).Distinct().Count()==3&&helpers.Length==3,"Tropinha tem os três tipos e respeita o limite de três vivos");
        int points=waves.Score.Points,kills=waves.Score.Kills;helpers[0].GetComponent<Health>().TakeDamage(99);yield return Frames(3);
        Check(waves.Score.Points==points+200&&waves.Score.Kills==kills+1,"Ajudante derrotado concede pontuação e um abate uma única vez");
        Check(!fighter.Dangerous,"Boss não ataca junto da rodada dos reforços");
        yield return Wait(1);combat.StopCombat();yield return Frames(3);
        Check(!fighter.Running&&Object.FindObjectsByType<MacaquiniFighterMinion>(FindObjectsSortMode.None).Length==0,"Cancelar combate remove ajudantes e efeitos");
        yield return Force(MacaquiniCombat.Attack.FighterHelpers);yield return Wait(.2f);boss.Health.TakeDamage(999);yield return Frames(3);
        Check(!fighter.Running&&Object.FindObjectsByType<MacaquiniFighterMinion>(FindObjectsSortMode.None).Length==0,"Morte final limpa a tropinha e interrompe os combos");
        StopRecord();
    }
    private static IEnumerator Force(MacaquiniCombat.Attack attack)
    {
        combat.StopCombat();player.Movement.SetMoveInput(Vector2.zero);player.GetComponent<Rigidbody2D>().position=Vector2.zero;
        boss.GetComponent<Rigidbody2D>().position=new Vector2(4,1);boss.Movement.HoldPosition(true);
        combat.SetTarget(player.Health);yield return Frames(2);combat.ForceAttack(attack);yield return Frames(3);
        if(attack==MacaquiniCombat.Attack.FighterJab||attack==MacaquiniCombat.Attack.FighterUppercut||attack==MacaquiniCombat.Attack.FighterHammer)yield return Until(()=>fighter.CurrentState==MacaquiniFighter.State.Cue,"fighter posture cue");
    }
    private static void StartRecord()
    {
        Directory.CreateDirectory(Recording);camera=Camera.main;capture=new RenderTexture(1200,720,24);pixels=new Texture2D(1200,720,TextureFormat.RGB24,false);camera.targetTexture=capture;
        foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay)){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
        times=new List<float>();recordStart=Time.unscaledTime;nextFrame=0;audio=camera.gameObject.AddComponent<JungleMotionAudioCapture>();audio.Begin();
    }
    private static void Record()
    {
        if(times==null||captureSuspended||Time.unscaledTime-recordStart<nextFrame)return;
        camera.Render();RenderTexture.active=capture;pixels.ReadPixels(new Rect(0,0,1200,720),0,0);pixels.Apply();RenderTexture.active=null;
        File.WriteAllBytes(Recording+"/frame_"+times.Count.ToString("D5")+".png",pixels.EncodeToPNG());times.Add(Time.unscaledTime-recordStart);nextFrame=Time.unscaledTime-recordStart+.08f;
    }
    private static void StopRecord()
    {
        if(times==null)return;var text=new System.Text.StringBuilder("ffconcat version 1.0\n");
        for(int i=0;i<times.Count;i++){text.Append("file 'frame_"+i.ToString("D5")+".png'\n");text.Append("duration "+(i+1<times.Count?times[i+1]-times[i]:.08f).ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+"\n");}
        text.Append("file 'frame_"+(times.Count-1).ToString("D5")+".png'\n");File.WriteAllText(Recording+"/frames.ffconcat",text.ToString());audio.Save(Recording+"/audio.wav");times=null;camera.targetTexture=null;capture.Release();Object.Destroy(capture);Object.Destroy(pixels);
    }
    private static IEnumerator Snapshot(string name){yield return Frames(2);Record();File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());}
    private static IEnumerator Wait(float duration){float end=Time.time+duration;while(Time.time<end)yield return null;}
    private static IEnumerator WallWait(float duration){double end=EditorApplication.timeSinceStartup+duration;while(EditorApplication.timeSinceStartup<end)yield return null;}
    private static IEnumerator Frames(int count){while(count-->0)yield return null;}
    private static IEnumerator Until(Func<bool> predicate,string label){double end=EditorApplication.timeSinceStartup+35;while(!predicate()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);yield return null;}}
    private static void Check(bool value,string message){if(!value)throw new InvalidOperationException("FIGHTER_CHECK_FAILED "+message);checks++;Debug.Log("FIGHTER_CHECK "+message);}
    private static void OnLog(string message,string trace,LogType type){if(trace.Contains("UnityEditor.Search.SearchDatabase"))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Fighter deadline");Record();
            while(steps.Count>0){var top=steps.Peek();if(!top.MoveNext()){steps.Pop();continue;}if(top.Current is IEnumerator child){steps.Push(child);continue;}return;}
            Debug.Log("MACAQUINI_FIGHTER_PLAY_OK checks="+checks+" errors="+errors);
        }catch(Exception error){errors++;Debug.LogException(error);}
        StopRecord();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
        File.WriteAllText(Output+"/validation.json","{\"checks\":"+checks+",\"errors\":"+errors+"}");Time.timeScale=1;EditorApplication.isPlaying=false;
    }
}
