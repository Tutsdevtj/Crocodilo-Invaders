using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class PlayerDashPlayValidation
{
    private const string Session = "Crocodilo.Dash.Validation";
    private static readonly Stack<IEnumerator> stack = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    private static float oldFixed;
    private static Camera camera;
    private static Player player;
    private static PlayerDash dash;
    private static PlayerDashVFX vfx;
    private static Rigidbody2D body;
    private static InputSettings.BackgroundBehavior originalBackground;
    private static InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
    [Serializable] private class WallSpec { public string name; public Vector3 position, scale; public Quaternion rotation; public Vector2 size, offset; public int layer; }
    [Serializable] private class WallSet { public WallSpec[] walls; }
    static PlayerDashPlayValidation() { EditorApplication.playModeStateChanged += Mode; }
    [MenuItem("Crocodilo/Player/Validate dash in Play Mode")]
    public static void Run() => Start(false);
    public static void RunVisual() => Start(true);
    private static void Start(bool visualOnly)
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        PlayerDashTools.Validate();
        var sourceScene = EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity",OpenSceneMode.Additive);
        var arena = sourceScene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="ArenaBounds");
        var walls = arena.GetComponentsInChildren<BoxCollider2D>().Select(w=>new WallSpec { name=w.name,position=w.transform.position,rotation=w.transform.rotation,scale=w.transform.lossyScale,size=w.size,offset=w.offset,layer=w.gameObject.layer }).ToArray();
        SessionState.SetString(Session+".Walls",JsonUtility.ToJson(new WallSet {walls=walls}));
        EditorSceneManager.CloseScene(sourceScene,true);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Session+".VisualOnly",visualOnly);
        SessionState.SetBool(Session, true); EditorApplication.isPlaying = true;
    }
    private static void Mode(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Session, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            checks = errors = 0; oldFixed = Time.fixedDeltaTime; deadline = EditorApplication.timeSinceStartup + 240;
            originalBackground=InputSystem.settings.backgroundBehavior; originalEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Application.logMessageReceived += Log; stack.Clear(); stack.Push(Scenario()); EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(Session, false);
            if (Application.isBatchMode) EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }
    private static void Log(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Dash validation timed out");
            for (int budget = 0; budget < 100; budget++)
            {
                if (stack.Count == 0) { Debug.Log("PLAYER DASH PLAY VALIDATION PASSED: " + checks + " assertions."); Finish(); return; }
                var current = stack.Peek();
                if (!current.MoveNext()) { stack.Pop(); continue; }
                if (current.Current is IEnumerator nested) { stack.Push(nested); continue; }
                return;
            }
        }
        catch (Exception error) { Debug.LogException(error); errors++; Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        Time.captureFramerate = 0; Time.timeScale = 1f; Time.fixedDeltaTime = oldFixed; AudioListener.pause = false;
        InputSystem.settings.backgroundBehavior=originalBackground; InputSystem.settings.editorInputBehaviorInPlayMode=originalEditorInput;
        EditorApplication.isPlaying = false;
    }
    private static void Check(bool value, string text)
    {
        if (!value) throw new InvalidOperationException("DASH CHECK FAILED: " + text);
        Debug.Log("DASH CHECK " + (++checks) + ": " + text);
    }
    private static IEnumerator Wait(float seconds)
    {
        float until = Time.time + seconds;
        while (Time.time < until) yield return null;
    }
    private static IEnumerator Frames(int frames)
    {
        int until = Time.frameCount + frames; while (Time.frameCount < until) yield return null;
    }
    private static void Spawn()
    {
        player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), Vector3.zero, Quaternion.identity).GetComponent<Player>();
        player.GetComponent<PlayerInputReader>().enabled = false;
        body = player.GetComponent<Rigidbody2D>(); dash = player.Dash; vfx = player.GetComponent<PlayerDashVFX>();
        player.Movement.SetMoveSpeed(5f);
    }
    private static IEnumerator Fresh()
    {
        if (player != null) Object.Destroy(player.gameObject);
        yield return Frames(2); Spawn(); yield return Frames(1);
    }
    private static IEnumerator Perform(Vector2 direction, float expected, string label)
    {
        body.position = Vector2.zero; body.linearVelocity = Vector2.zero;
        player.SetMoveInput(direction);
        Check(player.TryDash(), "Dash accepted " + label);
        player.SetMoveInput(Vector2.zero);
        float until = Time.time + .6f;
        while (dash.IsDashing && Time.time < until) yield return null;
        Check(!dash.IsDashing && Vector2.Distance(body.position, direction.normalized * expected) < .012f,
            label + " exact free distance (actual " + body.position + ")");
        yield return Wait(dash.CooldownRemaining + .05f);
    }
    private static IEnumerator Scenario()
    {
        Time.captureFramerate = 60;
        var cameraRoot = new GameObject("Dash validation camera"); cameraRoot.tag = "MainCamera";
        camera = cameraRoot.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 3f; camera.aspect = 5f / 3f;
        camera.transform.position = new Vector3(0, 0, -10); camera.backgroundColor = new Color(.28f,.56f,.77f); cameraRoot.AddComponent<AudioListener>();
        Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/BattleSky.prefab"));
        Spawn(); yield return Frames(2);
        var audioFields = new SerializedObject(vfx);
        var dashClip = audioFields.FindProperty("dashSound").objectReferenceValue as AudioClip;
        var dashSource = (AudioSource)typeof(PlayerDashVFX).GetField("audioSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vfx);
        Check(dashClip == AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Player/SFX_Player_Dash.wav") && dashClip != null, "Provided dash audio wired on real prefab");
        Check(dashSource != null && !dashSource.isPlaying && !dashSource.playOnAwake && dashSource.spatialBlend == 0f, "Dash source is silent before input and uses 2D playback");
        Check(dashSource.outputAudioMixerGroup != null && dashSource.outputAudioMixerGroup.name == "Effects", "Dash sound respects Effects mixer");
        Check(player.TryDash() && dashSource.isPlaying, "Successful dash starts provided sound");
        Check(!player.TryDash(), "Cooldown rejects duplicate audio trigger");
        vfx.enabled = false;
        Check(!dashSource.isPlaying, "Disable cleans up dash audio");
        yield return Fresh();
        Vector2[] directions = { Vector2.right, new Vector2(1,1), Vector2.up, new Vector2(-1,1), Vector2.left, new Vector2(-1,-1), Vector2.down, new Vector2(1,-1) };
        if (!SessionState.GetBool(Session+".VisualOnly",false))
        {
        foreach (float step in new[] { 1f/30f, .02f, 1f/120f })
        {
            Time.fixedDeltaTime = step;
            foreach (int fps in new[] { 15, 30, 60, 144 })
            {
                Time.captureFramerate = fps;
                foreach (Vector2 direction in directions) yield return Perform(direction, 2.2f, fps + "FPS / physics " + step + " / " + direction);
            }
        }
        Time.fixedDeltaTime = .02f; Time.captureFramerate = 60;
        var fields = new SerializedObject(dash);
        fields.FindProperty("speedCurve").animationCurveValue = AnimationCurve.Linear(0, .1f, 1, 5f); fields.ApplyModifiedPropertiesWithoutUndo();
        yield return Perform(Vector2.right,2.2f,"custom accelerating curve normalized");
        fields.Update(); fields.FindProperty("speedCurve").animationCurveValue = AnimationCurve.Linear(0,0,1,0); fields.ApplyModifiedPropertiesWithoutUndo();
        yield return Perform(Vector2.one,2.2f,"zero-area curve falls back safely");
        yield return Fresh();
        dash.SetUpgradeMultipliers(1.5f,.5f,2f);
        Check(Mathf.Abs(dash.Cooldown - .55f) < .001f && dash.InvulnerabilityDuration == .16f, "Upgrade cooldown and capped i-frames");
        yield return Perform(Vector2.one,3.3f,"distance upgrade");
        yield return Fresh();
        yield return Perform(new Vector2(.4f,1f),2.2f,"analog vector remains unquantized");
        // Impulso real no início e desaceleração no fim, observado na física.
        player.SetMoveInput(Vector2.right); player.TryDash(); player.SetMoveInput(Vector2.zero);
        float peak = 0f, tail = 100f;
        while (dash.IsDashing) { peak = Mathf.Max(peak,body.linearVelocity.magnitude); if (body.position.x > 1.9f) tail = Mathf.Min(tail,body.linearVelocity.magnitude); yield return null; }
        Check(peak > 20f && tail < peak * .5f, "Strong initial impulse, short slowing tail");
        yield return Fresh();
        object shield = new object(); player.Health.SetInvulnerable(shield,true);
        player.TryDash(); yield return Wait(.2f);
        Check(player.Health.IsInvulnerable && !dash.HasInvulnerability && !vfx.OutlineVisible, "Dash expiry preserves another shield and removes only dash indicator");
        player.Health.SetInvulnerable(shield,false);
        Check(!player.Health.IsInvulnerable, "Independent shield releases normally");
        yield return Fresh();
        int starts=0, ends=0, cancels=0; float protectionEnd=0;
        dash.DashStarted += _ => starts++; dash.DashEnded += () => ends++; dash.DashCancelled += () => cancels++; dash.InvulnerabilityEnded += () => protectionEnd=Time.time;
        float started=Time.time; player.TryDash();
        Check(!player.TryDash() && starts==1, "Cooldown request cannot duplicate VFX/start events");
        yield return Wait(.14f);
        Check(!player.Health.IsInvulnerable && !vfx.OutlineVisible && Mathf.Abs(protectionEnd-started-.13f)<.04f, "Real 130ms protection expiry independent of VFX tail");
        yield return Wait(.12f);
        Check(ends==1 && cancels==0, "Natural finish emits only DashEnded");
        yield return Wait(dash.CooldownRemaining+.02f);
        player.TryDash(); yield return Wait(.04f); dash.CancelDash();
        Check(cancels==1 && !dash.IsDashing && !player.Health.IsInvulnerable && dash.CooldownRemaining>0, "Cancellation clears movement/protection while retaining cooldown");
        yield return Wait(.3f);
        Check(vfx.ActiveAfterimages==0 && !vfx.StreakVisible && !vfx.WorldRoot.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled), "Cancelled VFX finish without stuck sprites");
        yield return Fresh();
        player.TryDash(); yield return Wait(.04f);
        Vector2 paused = body.position; int ghosts=vfx.ActiveAfterimages;
        Time.timeScale=0f; yield return Frames(2);
        double real=EditorApplication.timeSinceStartup+.15;
        while(EditorApplication.timeSinceStartup<real) yield return null;
        Check(Vector2.Distance(paused,body.position)<.001f && ghosts==vfx.ActiveAfterimages && dash.IsDashing && vfx.OutlineVisible, "Pause freezes physics, protection and all FX clocks");
        Time.timeScale=1; player.SetMoveInput(Vector2.zero); yield return Wait(.4f);
        Check(Mathf.Abs(body.position.x-2.2f)<.012f && vfx.ActiveAfterimages==0 && !vfx.StreakVisible, "Resume completes exact distance and residual cleanup");
        yield return Fresh();
        player.TryDash(); yield return Wait(.04f); player.SetGameplayActive(false); yield return Frames(1);
        Check(!dash.IsDashing && !vfx.OutlineVisible && !vfx.StreakVisible && vfx.ActiveAfterimages==0, "Cutscene/gameplay block clears presentation immediately");
        player.SetGameplayActive(true); yield return Wait(1.2f);
        player.TryDash(); yield return Wait(.04f); player.gameObject.SetActive(false);
        Check(vfx.ActiveAfterimages==0 && !vfx.StreakVisible && !vfx.OutlineVisible, "Player disable clears pool and outline");
        player.gameObject.SetActive(true);
        yield return Fresh();
        player.TryDash(); yield return Wait(.135f); player.Health.TakeDamage(1000); yield return Frames(1);
        Check(player.Health.IsDead && !dash.IsDashing && vfx.ActiveAfterimages==0 && !vfx.StreakVisible && !vfx.OutlineVisible, "Death during final dash segment clears all effects");
        // Bordas/obstáculo fino, inclusive distância melhorada e passo de 30Hz.
        yield return Fresh();
        var obstacle = new GameObject("Thin solid obstacle"); obstacle.transform.position = new Vector3(.65f,0,0);
        obstacle.AddComponent<BoxCollider2D>().size = new Vector2(.03f,8f);
        dash.SetUpgradeMultipliers(2f,1f,1f); Time.fixedDeltaTime=1f/30f;
        player.TryDash(); yield return Wait(.3f);
        Check(body.position.x < .65f, "Upgraded dash cannot tunnel through thin solid obstacle (x="+body.position.x+")");
        Object.Destroy(obstacle); Time.fixedDeltaTime=.02f;
        // Mesmas quatro paredes da arena, sem alterar cena de produção.
        var bounds = new GameObject("Copies of real arena walls");
        foreach(var spec in JsonUtility.FromJson<WallSet>(SessionState.GetString(Session+".Walls", "")).walls)
        {
            var wallRoot=new GameObject(spec.name);wallRoot.layer=spec.layer;wallRoot.transform.SetParent(bounds.transform);
            wallRoot.transform.SetPositionAndRotation(spec.position,spec.rotation);wallRoot.transform.localScale=spec.scale;
            var collider=wallRoot.AddComponent<BoxCollider2D>();collider.size=spec.size;collider.offset=spec.offset;
        }
        foreach (var wall in bounds.GetComponentsInChildren<BoxCollider2D>())
        {
            yield return Fresh();
            Vector2 direction = wall.size.x > wall.size.y ? (wall.transform.position.y > 0 ? Vector2.up : Vector2.down) : (wall.transform.position.x > 0 ? Vector2.right : Vector2.left);
            body.position = (Vector2)wall.bounds.center - direction * 1.2f;
            player.SetMoveInput(direction); player.TryDash(); player.SetMoveInput(Vector2.zero); yield return Wait(.3f);
            Check(Vector2.Dot(body.position - (Vector2)wall.bounds.center,direction)<0f,"Dash remains inside arena wall "+wall.name);
        }
        Object.Destroy(bounds);
        yield return Fresh();
        // Contato esquivado consome o projétil, inclusive após acabar a proteção.
        var projectile = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/EnemyProjectile.prefab"),Vector3.zero,Quaternion.identity).GetComponent<Projectile>();
        player.TryDash(); projectile.Launch(Vector2.zero,null,0f); yield return Wait(.2f);
        Check(projectile==null && player.Health.CurrentHealth==30, "Dodged projectile is consumed with no delayed damage");
        yield return Fresh();
        var laser = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/BossLaser.prefab")).GetComponent<BossLaser>();
        player.TryDash(); laser.Begin(Vector2.zero,false,20f,0f,.5f); yield return Wait(.3f);
        Check(player.Health.CurrentHealth==30, "Dodged laser never charges damage after i-frames expire");
        Object.Destroy(laser.gameObject);
        yield return Fresh();
        int fired=0; player.Combat.ShotFired += ()=>fired++;
        player.Combat.TryShoot(); player.TryDash(); yield return Wait(.3f);
        Check(fired==1, "Dash preserves native shot animation/release");
        }
        yield return Fresh();
        // Inputs nativos: ambos os Shifts, sem repetição ao segurar.
        var keyboard = InputSystem.AddDevice<Keyboard>();
        player.GetComponent<PlayerInputReader>().enabled=true;
        int inputStarts=0, heldShots=0; dash.DashStarted += _=>inputStarts++; player.Combat.ShotFired += ()=>heldShots++;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.UpArrow,Key.LeftShift,Key.Space)); yield return Wait(1.3f);
        Check(inputStarts==1 && dash.Direction==Vector2.up, "Left Shift and held key produce one upward dash (starts="+inputStarts+", direction="+dash.Direction+")");
        Check(heldShots>=2,"Holding Space preserves continuous native fire during and after dash");
        InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return Frames(2);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.RightArrow,Key.RightShift)); yield return Wait(.2f);
        Check(inputStarts==2 && dash.Direction==Vector2.right, "Right Shift uses existing action and direction");
        InputSystem.RemoveDevice(keyboard); player.GetComponent<PlayerInputReader>().enabled=false;
        // Capturas de gameplay com tiros inimigos acima dos efeitos.
        yield return Fresh();
        bool[] expectedFlipX={false,false,false,true,true,true,false,false};
        bool[] expectedFlipY={false,false,false,false,false,true,false,true};
        float[] expectedAngles={0,0,90,0,0,0,270,0};
        for(int orientation=0;orientation<directions.Length;orientation++)
        {
            Vector2 direction=directions[orientation];
            var bullets=new List<GameObject>();
            for(int shot=0;shot<5;shot++)
            {
                var bullet=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/GusiniBullet.prefab"),new Vector3(3f,-1.4f+shot*.7f,0),Quaternion.identity);
                bullet.GetComponent<Projectile>().Launch(new Vector2(-1f,(shot-2)*.12f),null,4f);bullets.Add(bullet);
            }
            body.position = Vector2.zero; player.SetMoveInput(direction); player.TryDash(); player.SetMoveInput(Vector2.zero);
            var spark=vfx.WorldRoot.GetComponentsInChildren<SpriteRenderer>().First(s=>s.name=="Ignition 0");
            Vector3 origin=spark.transform.position;
            yield return Wait(.06f);
            Check(vfx.StreakVisible && vfx.ActiveAfterimages>=2 && vfx.OutlineVisible,"VFX choreography at peak "+direction);
            var trail=player.GetComponentsInChildren<SpriteRenderer>().Single(s=>s.name=="Dash speed streak");
            Check(Vector3.Distance(trail.transform.position,vfx.VisualCenter)<.01f,"Trail pivot follows visual center "+direction);
            Check(trail.flipX==expectedFlipX[orientation] && trail.flipY==expectedFlipY[orientation] && Mathf.Abs(Mathf.DeltaAngle(trail.transform.localEulerAngles.z,expectedAngles[orientation]))<.1f && trail.sprite.name.Contains((orientation%2==1)?"_d_":"_h_"),"Manifest variant/flip/90-degree rotation "+direction);
            Check(Vector3.Distance(origin,spark.transform.position)<.001f,"Ignition stays at its world spawn "+direction);
            Capture("dash_"+direction.x+"_"+direction.y);
            Check(bullets.All(b=>b.GetComponentInChildren<SpriteRenderer>().sortingOrder>player.GetComponentInChildren<SpriteRenderer>().sortingOrder+1),"Enemy fan remains above every dash effect");
            foreach(var bullet in bullets)Object.Destroy(bullet);yield return Wait(1.2f);
        }
        int poolCount=vfx.WorldRoot.GetComponentsInChildren<Transform>(true).Length;
        for(int i=0;i<8;i++) yield return Perform(directions[i],2.2f,"pool reuse "+i);
        Check(poolCount==vfx.WorldRoot.GetComponentsInChildren<Transform>(true).Length,"Afterimage/puff pool stays fixed across repeated dashes");
        Transform owned=vfx.WorldRoot; Object.Destroy(player.gameObject); yield return Frames(2);
        Check(owned==null,"Destroying player destroys all world-space FX ownership");
        var activeScene=SceneManager.GetActiveScene(); var temporary=SceneManager.CreateScene("Dash unload validation"); SceneManager.SetActiveScene(temporary);
        Spawn(); player.TryDash(); yield return Wait(.04f); owned=vfx.WorldRoot;
        SceneManager.SetActiveScene(activeScene); var unload=SceneManager.UnloadSceneAsync(temporary); while(!unload.isDone) yield return null;
        Check(player==null && owned==null,"Scene unload removes player and world VFX even when destruction order changes");
        Check(errors==0,"No Unity errors during native dash simulation");
    }
    private static void Capture(string name)
    {
        Directory.CreateDirectory("Logs/DashVFX");
        var texture = new RenderTexture(1200,720,24); camera.targetTexture=texture; camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=texture;
        var pixels=new Texture2D(1200,720,TextureFormat.RGB24,false); pixels.ReadPixels(new Rect(0,0,1200,720),0,0); pixels.Apply();
        File.WriteAllBytes("Logs/DashVFX/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=null; RenderTexture.active=previous; Object.Destroy(pixels); Object.Destroy(texture);
    }
}
