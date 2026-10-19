using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Isola o cenário com uma sonda física; a suíte HubPlayValidation testa o personagem/câmera reais.
// Os controladores da câmera ficam suspensos apenas durante esta inspeção manual do mapa.
[InitializeOnLoad]
public static class BaseImprovisadaMapPlayValidation
{
    private const string Session = "Crocodilo.BaseMap.Validation";
    private const string Output = "Logs/BaseImprovisadaMap";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static bool running;
    private static double deadline;
    private static int checks, errors;
    private static readonly List<string> report = new List<string>();

    static BaseImprovisadaMapPlayValidation()
    {
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Session, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }

    [MenuItem("Crocodilo/Base/Validate BaseImprovisada map in Play Mode")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BaseImprovisadaMapTools.Validate();
        EditorSceneManager.OpenScene(BaseImprovisadaMapTools.ScenePath, OpenSceneMode.Single);
        SessionState.SetBool(Session, true);
        EditorApplication.isPlaying = true;
    }

    private static IEnumerator Scenario()
    {
        yield return Frames(5);
        Camera camera = Camera.main;
        if (camera.GetComponent<HubCameraController2D>() is var follow && follow != null) follow.enabled = false;
        if (camera.GetComponent<UnityEngine.U2D.PixelPerfectCamera>() is var perfect && perfect != null) perfect.enabled = false;
        camera.orthographicSize = 4.5f;
        // The map probe must land on the ground, rather than on the HUB character
        // occupying the same spawn. Real character collisions have their own suite.
        foreach (var player in Object.FindObjectsByType<HubCharacterMotor2D>()) player.gameObject.SetActive(false);
        Check(camera != null && Object.FindObjectsByType<Camera>().Length == 1, "Scene opens in Play Mode with its single camera");
        Vector3 cameraStart = camera.transform.position;
        var layers = Object.FindObjectsByType<ParallaxLayer2D>().ToDictionary(l => l.name);
        var panorama = layers["BG0_Panorama"].GetComponentInChildren<SpriteRenderer>();
        Transform ruins = layers["BG3_Ruins"].transform, near = layers["BG4_Near"].transform, front = layers["Foreground"].transform;
        var hangar = GameObject.Find("HubHangar").transform;
        float aspectHalf = 10.5f; // 21:9 com tamanho 4,5 cobre também 16:9 e 4:3

        // Parallax: deslocamentos relativos ao mundo
        float p0 = panorama.transform.position.x, r0 = ruins.position.x, n0 = near.position.x, f0 = front.position.x, h0 = hangar.position.x;
        yield return MoveCamera(camera, cameraStart.x + 40f);
        float pS = (40f - (panorama.transform.position.x - p0)) / 40f, rS = (40f - (ruins.position.x - r0)) / 40f;
        float nS = (40f - (near.position.x - n0)) / 40f, fS = (40f - (front.position.x - f0)) / 40f;
        report.Add($"Screen speed relative to the world: BG0 {pS:0.00}, BG3 {rS:0.00}, BG4 {nS:0.00}, world 1.00, foreground {fS:0.00}");
        Check(Mathf.Approximately(hangar.position.x, h0), "World objects stay put when the camera moves");
        Check(Mathf.Abs(pS - .03f) < .005f && Mathf.Abs(rS - .3f) < .005f && Mathf.Abs(nS - .6f) < .005f && Mathf.Abs(fS - 1.25f) < .005f,
            "Background layers move slower than the world (0.03 / 0.30 / 0.60), foreground faster (1.25)");

        // Cobertura: panorama e chão cobrem toda a vista em todo o trajeto possível da câmera
        bool covered = true;
        float worst = float.MaxValue;
        for (float x = BaseImprovisadaMapTools.PlayableLeft + 8f; x <= BaseImprovisadaMapTools.PlayableRight - 8f; x += 2f)
        {
            yield return MoveCamera(camera, x);
            Bounds b = panorama.bounds;
            float margin = Mathf.Min(x - aspectHalf - b.min.x, b.max.x - (x + aspectHalf));
            worst = Mathf.Min(worst, margin);
            covered &= margin >= 0f && b.min.y <= camera.transform.position.y - 4.5f && b.max.y >= camera.transform.position.y + 4.5f
                && x - aspectHalf >= BaseImprovisadaMapTools.VisualLeft && x + aspectHalf <= BaseImprovisadaMapTools.VisualRight;
        }
        report.Add($"Panorama horizontal margin at 21:9 across the camera range: {worst:0.00} units");
        Check(covered, "No empty sky or ground edge anywhere along the camera range (21:9)");

        // Capturas reais de cada zona
        Directory.CreateDirectory(Output);
        var shots = new (string name, float x)[] { ("A_entrada", 0f), ("B_enfermaria", 9.5f), ("B_C_patio", 19.5f), ("C_hangar", 29.5f), ("C_oficina", 38.5f),
            ("D_acampamento", 50f), ("E_comando", 63f), ("F_radio", 77f), ("G_expansao", 89f) };
        foreach (var shot in shots)
        {
            yield return MoveCamera(camera, shot.x);
            yield return Capture(camera, 1200, 720, shot.name);
        }
        yield return MoveCamera(camera, 29.5f);
        yield return Capture(camera, 1920, 1080, "C_hangar");
        yield return Capture(camera, 800, 600, "C_hangar");
        yield return MoveCamera(camera, cameraStart.x);
        camera.transform.position = cameraStart;
        yield return Frames(2);
        Check(Mathf.Approximately(panorama.transform.position.x, p0), "Parallax returns to its authored position with the camera");

        // Luzes: um único gerenciador e variação discreta
        var flicker = Object.FindObjectsByType<BaseLightFlicker>().Single();
        var halos = Object.FindObjectsByType<SpriteRenderer>().Where(r => r.sortingLayerName == "AmbientEffects").ToArray();
        float a0 = halos.Sum(h => h.color.a);
        yield return Seconds(.6f);
        float a1 = halos.Sum(h => h.color.a);
        Check(halos.Length >= 30 && Mathf.Abs(a1 - a0) > 0f && halos.All(h => h.color.a <= .45f), $"{halos.Length} amber halos, subtle flicker from one manager");

        // Física: sonda do tamanho do Croc pelo chão contínuo e contra os bloqueios
        var spawn = GameObject.Find("PlayerSpawn").transform;
        var probe = new GameObject("ValidationProbe");
        var body = probe.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        probe.AddComponent<BoxCollider2D>().size = new Vector2(.5f, .88f);
        probe.transform.position = new Vector3(spawn.position.x, 2f, 0f);
        yield return Seconds(1.2f);
        Check(Mathf.Abs(probe.transform.position.y - .44f) < .03f, $"Spawn rests on the ground (centre y {probe.transform.position.y:0.000})");
        float minY = float.MaxValue;
        yield return Drive(body, 6f, 25f, y => minY = Mathf.Min(minY, y));
        float gateStop = probe.transform.position.x;
        report.Add($"Probe walked right from {spawn.position.x:0.0} and stopped at x {gateStop:0.00} (expansion gate {BaseImprovisadaMapTools.ExpansionGateX})");
        Check(gateStop > 75f && gateStop < BaseImprovisadaMapTools.ExpansionGateX, "Entrance, infirmary, hangar, camp, command and radio fronts are walkable; expansion gate blocks");
        Check(minY > .4f, $"Never sinks through the ground (min centre y {minY:0.000})");
        var expansion = Object.FindObjectsByType<BaseSector>().Single(s => s.SectorId == "expansion");
        expansion.SetState(BaseSector.State.Open);
        yield return Frames(2);
        yield return Drive(body, 6f, 8f, y => minY = Mathf.Min(minY, y));
        float end = probe.transform.position.x;
        Check(end > 97.5f && end < BaseImprovisadaMapTools.PlayableRight, $"Opening the sector by state only clears the gate; the map end still holds (x {end:0.00})");
        expansion.SetState(BaseSector.State.Locked);
        yield return Frames(2);
        Check(GameObject.Find("ExpansionArea").transform.Find("Gate").gameObject.activeSelf, "Locking again restores gate visuals and collider");
        probe.transform.position = new Vector3(spawn.position.x, .5f, 0f);
        body.linearVelocity = Vector2.zero;
        yield return Frames(3);
        yield return Drive(body, -6f, 4f, y => minY = Mathf.Min(minY, y));
        Check(probe.transform.position.x < -4.5f && probe.transform.position.x > BaseImprovisadaMapTools.PlayableLeft, $"Left boundary holds (x {probe.transform.position.x:0.00})");
        Check(minY > .4f, "Floor continuous in both directions");
        Object.Destroy(probe);
        yield return Frames(2);
    }

    private static IEnumerator Drive(Rigidbody2D body, float speed, float seconds, Action<float> sample)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        float lastX = body.position.x, stuckSince = Time.time;
        while (EditorApplication.timeSinceStartup < until)
        {
            body.linearVelocity = new Vector2(speed, body.linearVelocity.y);
            sample(body.position.y);
            if (Mathf.Abs(body.position.x - lastX) > .01f) { lastX = body.position.x; stuckSince = Time.time; }
            else if (Time.time - stuckSince > .6f) break;
            yield return Frames(1);
        }
        body.linearVelocity = Vector2.zero;
    }

    private static IEnumerator MoveCamera(Camera camera, float x)
    {
        var p = camera.transform.position;
        camera.transform.position = new Vector3(x, p.y, p.z);
        yield return Frames(2);
    }

    private static IEnumerator Capture(Camera camera, int width, int height, string name)
    {
        var texture = new RenderTexture(width, height, 24);
        camera.targetTexture = texture;
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = texture;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        pixels.Apply();
        RenderTexture.active = previous;
        camera.targetTexture = null;
        File.WriteAllBytes($"{Output}/{name}_{width}x{height}.png", pixels.EncodeToPNG());
        Object.Destroy(pixels);
        texture.Release();
        yield return Frames(1);
    }

    private static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Session, false)) Begin();
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Session, false))
        {
            SessionState.SetBool(Session, false);
            if (Application.isBatchMode) EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }

    private static void Begin()
    {
        if (running) return;
        running = true;
        checks = errors = 0;
        report.Clear();
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 240;
        steps.Clear();
        steps.Push(Scenario());
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Base map play validation timed out");
            while (steps.Count > 0)
            {
                IEnumerator top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            if (errors > 0) throw new InvalidOperationException(errors + " runtime errors/exceptions were logged");
            foreach (string line in report) Debug.Log("BASE MAP REPORT: " + line);
            Debug.Log("BASE MAP PLAY VALIDATION PASSED: " + checks + " assertions.");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        running = false;
        EditorApplication.isPlaying = false;
    }

    private static void OnLog(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("BASE MAP CHECK FAILED: " + message);
        checks++; Debug.Log("BASE MAP CHECK " + checks + ": " + message);
    }

    private static IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int frame = Time.frameCount;
            while (Time.frameCount == frame) yield return null;
        }
    }

    private static IEnumerator Seconds(float seconds)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < until) yield return null;
    }
}
