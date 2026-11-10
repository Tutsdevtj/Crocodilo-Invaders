using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Repeatable real PlayerLoop footage. All achievements and campaign saves are isolated.
[InitializeOnLoad]
public static class AchievementVideoCapture
{
    const string Key="Crocodilo.Achievements.Video",DirectoryPath="Logs/Achievements/VideoCompact";
    static readonly Stack<IEnumerator> Steps=new Stack<IEnumerator>();
    static List<float> times;
    static Camera camera;
    static RenderTexture target;
    static Texture2D pixels;
    static JungleMotionAudioCapture audio;
    static Canvas[] canvases;
    static float start,next;
    static double deadline;
    static bool success;
    static int errors;
    static readonly float[] oldVolumes=new float[3];
    static AchievementVideoCapture()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Begin();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(success&&errors==0?0:1);}
        };
    }
    public static void Run(){Debug.Log("ACH_VIDEO_START_REQUEST");Application.runInBackground=true;SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");EditorApplication.isPlaying=true;}
    public static void RunCompact(){AchievementTools.CompactPopup();Run();}
    static void Begin()
    {
        success=false;errors=0;deadline=EditorApplication.timeSinceStartup+120;Application.runInBackground=true;
        Debug.Log("ACH_VIDEO_PLAYER_STARTED");
        Achievements.UseDirectory(DirectoryPath+"/Save-"+Guid.NewGuid().ToString("N"));CampaignProgress.UseDirectory(DirectoryPath+"/Campaign-"+Guid.NewGuid().ToString("N"));LeaderboardSession.Cancel();
        Steps.Clear();Steps.Push(Scenario());EditorApplication.update+=Tick;
    }
    static IEnumerator Scenario()
    {
        yield return Frames(6);
        Debug.Log("ACH_VIDEO_SCENE_READY frame="+Time.frameCount+" time="+Time.time);
        var player=Object.FindAnyObjectByType<Player>();var waves=Object.FindAnyObjectByType<WaveController>();var spawner=waves.GetComponent<EnemySpawner>();
        player.GetComponent<PlayerInputReader>().enabled=false;player.Health.SetInvulnerable(true);
        yield return Until(()=>waves.Phase==WaveController.RunPhase.Combat,"wave one",40,true);
        Debug.Log("ACH_VIDEO_WAVE_COMBAT budget="+spawner.RemainingToSpawn);
        Enemy last=null;
        double nextStatus=0;
        while(spawner.RemainingToSpawn>0||last==null)
        {
            foreach(var enemy in Object.FindObjectsByType<Enemy>())
            {
                if(enemy.Health.IsDead)continue;
                if(last==null&&spawner.RemainingToSpawn==0){last=enemy;last.GetComponent<Rigidbody2D>().position=new Vector2(1.5f,-.1681f);last.Movement.HoldPosition(true);if(last.Combat!=null)last.Combat.enabled=false;}
                else if(enemy!=last)enemy.Health.TakeDamage(100000);
            }
            if(EditorApplication.timeSinceStartup>nextStatus){Debug.Log("ACH_VIDEO_PREP budget="+spawner.RemainingToSpawn+" alive="+spawner.AliveCount+" frame="+Time.frameCount+" time="+Time.time);nextStatus=EditorApplication.timeSinceStartup+2;}
            yield return Frames(1);
        }
        foreach(var enemy in Object.FindObjectsByType<Enemy>())if(enemy!=last&&!enemy.Health.IsDead)enemy.Health.TakeDamage(100000);
        player.GetComponent<Rigidbody2D>().position=new Vector2(-3,0);
        last.GetComponent<Rigidbody2D>().position=new Vector2(1.5f,-.1681f);last.Movement.HoldPosition(true);
        if(last.Health.PreciseCurrentHealth>1)last.Health.TakeDamage(last.Health.PreciseCurrentHealth-1);
        yield return Frames(5);
        var mixer=GameAudioSettings.Mixer;
        for(int i=0;i<3;i++)mixer.GetFloat(GameAudioSettings.Parameter((AudioChannel)i),out oldVolumes[i]);
        mixer.SetFloat(GameAudioSettings.Parameter(AudioChannel.Master),0);
        mixer.SetFloat(GameAudioSettings.Parameter(AudioChannel.Effects),-2);
        mixer.SetFloat(GameAudioSettings.Parameter(AudioChannel.Music),-23);
        StartRecord();yield return Wait(.8f);
        Debug.Log("ACH_VIDEO_SHOOT");
        if(!player.Combat.TryShoot())throw new Exception("Demo shot rejected");
        yield return Until(()=>Achievements.Store.Get(AchievementCatalog.FirstWave).Unlocked,"native wave-clear achievement",10);
        var rewards=Object.FindAnyObjectByType<CardRewardController>();
        yield return Until(()=>rewards.CanInteract,"native reward ready",10);
        yield return WallWait(.8f);
        if(!rewards.TryChoose(0))throw new Exception("Demo reward choice rejected");
        yield return Until(()=>Time.timeScale>0,"reward closed",10);
        var view=AchievementRuntime.Instance.View;
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.FirstWave,"first achievement popup",10);
        var popup=(RectTransform)view.transform.Find("UnlockNotification");
        if(popup.rect.size!=new Vector2(300,68)||popup.Find("Description").gameObject.activeSelf)throw new Exception("Compact popup layout invalid");
        var title=popup.Find("Name").GetComponent<TMPro.TMP_Text>();title.ForceMeshUpdate();
        if(title.mesh.vertexCount==0||title.isTextOverflowing)throw new Exception("Compact achievement name clipped");
        yield return Until(()=>view.CurrentToastId==AchievementCatalog.CleanWave,"second achievement popup",10);
        yield return Until(()=>view.CurrentToastId==null&&view.PendingCount==0,"popups finish",10);
        yield return Wait(.6f);
        success=Achievements.Store.Get(AchievementCatalog.FirstWave).Unlocked&&view.SoundPlayCount>=2;
    }
    static void StartRecord()
    {
        System.IO.Directory.CreateDirectory(DirectoryPath);camera=Camera.main;
        target=new RenderTexture(1200,720,24);pixels=new Texture2D(1200,720,TextureFormat.RGB24,false);camera.targetTexture=target;camera.aspect=5f/3f;
        canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
        audio=camera.gameObject.AddComponent<JungleMotionAudioCapture>();audio.Begin();times=new List<float>();start=Time.realtimeSinceStartup;next=0;
    }
    static void Record()
    {
        if(times==null||Time.realtimeSinceStartup-start<next)return;
        float timestamp=Time.realtimeSinceStartup-start;
        Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
        pixels.ReadPixels(new Rect(0,0,1200,720),0,0);pixels.Apply();RenderTexture.active=previous;
        File.WriteAllBytes(DirectoryPath+"/frame_"+times.Count.ToString("D5")+".png",pixels.EncodeToPNG());times.Add(timestamp);next=timestamp+1f/20f;
    }
    static void FinishRecord()
    {
        if(times==null)return;
        var text=new StringBuilder("ffconcat version 1.0\n");
        for(int i=0;i<times.Count;i++)text.Append("file 'frame_").Append(i.ToString("D5")).Append(".png'\nduration ").Append((i+1<times.Count?times[i+1]-times[i]:.05f).ToString("F6",CultureInfo.InvariantCulture)).Append('\n');
        text.Append("file 'frame_").Append((times.Count-1).ToString("D5")).Append(".png'\n");
        File.WriteAllText(DirectoryPath+"/frames.ffconcat",text.ToString());audio.Save(DirectoryPath+"/audio.wav");
        File.WriteAllText(DirectoryPath+"/capture.json","{\"frames\":"+times.Count+",\"seconds\":"+(Time.realtimeSinceStartup-start).ToString("F3",CultureInfo.InvariantCulture)+",\"nativeUnlock\":"+success.ToString().ToLowerInvariant()+",\"errors\":"+errors+"}");
        times=null;camera.targetTexture=null;foreach(var canvas in canvases)if(canvas!=null){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
        target.Release();Object.Destroy(target);Object.Destroy(pixels);Object.Destroy(audio);
        for(int i=0;i<3;i++)GameAudioSettings.Mixer.SetFloat(GameAudioSettings.Parameter((AudioChannel)i),oldVolumes[i]);
    }
    static IEnumerator Frames(int n){while(n-->0){int frame=Time.frameCount;while(Time.frameCount==frame)yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
    static IEnumerator WallWait(float seconds){double end=EditorApplication.timeSinceStartup+seconds;while(EditorApplication.timeSinceStartup<end)yield return null;}
    static IEnumerator Until(Func<bool> test,string label,float seconds,bool skip=false)
    {double end=EditorApplication.timeSinceStartup+seconds;while(!test()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException(label);if(skip){var dialogue=Object.FindAnyObjectByType<DialogueRunner>();if(dialogue!=null&&dialogue.IsPlaying)dialogue.Skip();}yield return Frames(1);}}
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Achievement video deadline");Record();
            while(Steps.Count>0){var step=Steps.Peek();if(!step.MoveNext()){Steps.Pop();continue;}if(step.Current is IEnumerator child){Steps.Push(child);continue;}return;}
        }
        catch(Exception exception){errors++;Debug.LogException(exception);}
        FinishRecord();EditorApplication.update-=Tick;Time.timeScale=1;AudioListener.pause=false;LeaderboardSession.Cancel();
        Debug.Log("ACHIEVEMENT_VIDEO_COMPLETE success="+success+" errors="+errors);EditorApplication.isPlaying=false;
    }
}
