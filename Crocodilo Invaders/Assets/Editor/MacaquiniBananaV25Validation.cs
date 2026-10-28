using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class MacaquiniBananaV25Validation
{
    private const string Key = "Crocodilo.Banana25.Validation";
    private const string Output = "Docs/ArtPreviews/MacaquiniBanana_v25";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static int checks, errors;
    private static double deadline;
    private static Player player;
    private static Boss boss;
    private static MacaquiniCombat combat;
    private static MacaquiniPhaseOneHelpers director;
    static MacaquiniBananaV25Validation()
    {
        EditorApplication.playModeStateChanged += state => {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Begin();
            if (state == PlayModeStateChange.EnteredEditMode) {
                SessionState.SetBool(Key, false);
                bool build = SessionState.GetBool(Key + ".Build", false); SessionState.SetBool(Key + ".Build", false);
                if (errors == 0 && build) EditorApplication.delayCall += MacaquiniBananaV25Tools.Build;
                else EditorApplication.Exit(errors == 0 ? 0 : 1);
            }
        };
    }
    public static void Run()
    {
        MacaquiniBananaV25Tools.Import(); EditorSceneManager.OpenScene(MacaquiniJetpackTools.Test);
        SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    public static void RunAndBuild() { SessionState.SetBool(Key + ".Build", true); Run(); }
    private static void Begin()
    {
        checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 240;
        Application.runInBackground = true; Directory.CreateDirectory(Output);
        Application.logMessageReceived += OnLog; steps.Clear(); steps.Push(Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(4); LeaderboardSession.Cancel(); CampaignProgress.UseDirectory("Logs/JungleStage/Banana25Save");
        player = Object.FindAnyObjectByType<Player>(); player.GetComponent<PlayerInputReader>().enabled = false;
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true);
        var waves = Object.FindAnyObjectByType<WaveController>();
        yield return Until(() => waves.ActiveBoss != null && waves.ActiveBoss.Ready, "boss ready");
        boss = waves.ActiveBoss; combat = boss.GetComponent<MacaquiniCombat>(); director = boss.GetComponent<MacaquiniPhaseOneHelpers>();
        Check(director != null && !combat.PhaseTwo, "Integração automática na primeira forma");
        Check(director.LivingHelpers == 0, "Abertura ensina o boss sozinho");
        foreach (string species in new[] { "bat", "moth" }) for (int i = 0; i < 8; i++)
        {
            var sprite = Resources.Load<Sprite>("MacaquiniHelpersV24/" + species + "_" + i);
            Check(sprite != null && sprite.pixelsPerUnit == 64 && sprite.texture.filterMode == FilterMode.Point, "Sprite nativo " + species + i);
        }
        yield return AttackScenario();
        combat.StopCombat(); yield return Frames(3); combat.SetTarget(player.Health);
        int naturalWave = director.WavesSpawned;
        yield return Until(() => director.WavesSpawned > naturalWave, "natural reinforcement wave");
        Check(director.LivingHelpers == 2, "Primeira onda natural traz dois morcegos");
        yield return ObserveWings(Helpers()[0], .65f, "Morcego troca os oito desenhos reais de asas durante o voo");
        yield return Wait(1); yield return Snapshot("fase1-com-ajudantes");
        combat.StopCombat(); yield return Frames(3);
        Check(Object.FindObjectsByType<MacaquiniJungleHelper>(FindObjectsSortMode.None).Length == 0, "Stop limpa ajudantes imediatamente");
        StartIsolated(); director.SpawnWave();
        yield return Until(() => Helpers().Any(h => h.CurrentStage == MacaquiniJungleHelper.Stage.Warning), "bat warning");
        var bat = Helpers().First(h => h.CurrentStage == MacaquiniJungleHelper.Stage.Warning);
        Check(Helpers().Count(h => h.Committed) == 1, "Somente um bote comprometido por vez");
        Vector2 locked = bat.LockedAim; Vector3 position = bat.transform.position;
        float time = Time.time; int wingFrame = bat.AnimationFrame; var frozenDrawing = bat.CurrentDrawing;
        Time.timeScale = 0; yield return WallWait(.25f);
        Check(Time.time == time && bat.transform.position == position && bat.AnimationFrame == wingFrame && bat.CurrentDrawing == frozenDrawing, "Pausa congela aviso, posição e desenho das asas"); Time.timeScale = 1;
        yield return Snapshot("morcego-aviso");
        player.GetComponent<Rigidbody2D>().position = locked;
        int hp = player.Health.CurrentHealth; player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), false);
        yield return Until(() => player.Health.CurrentHealth < hp, "real bat contact"); yield return Wait(.18f);
        Check(player.Health.CurrentHealth == hp - 1, "Bote aplica dano uma vez com contato real");
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true);
        Check(Vector2.Distance(bat.LockedAim, locked) < .001f, "Bote mantém mira travada");
        director.Cancel(); yield return Frames(3); StartIsolated(); director.SpawnWave();
        var victim = Helpers()[0]; int points = waves.Score.Points, kills = waves.Score.Kills;
        victim.GetComponent<Health>().TakeDamage(99); victim.GetComponent<Health>().TakeDamage(99);
        Check(waves.Score.Points == points + 150 && waves.Score.Kills == kills + 1, "Abate concede pontos uma única vez");
        yield return Wait(.3f);
        // Let the surviving bat finish before requesting the mixed wave.
        foreach (var h in Helpers()) h.GetComponent<Health>().TakeDamage(99);
        yield return Wait(.3f); director.SpawnWave();
        Check(director.LivingHelpers == 3 && Helpers().Count(h => h.Spitter) == 1, "Onda mista tem dois morcegos e uma mariposa");
        var moth = Helpers().First(h => h.Spitter);
        Check(Helpers().All(h => h.CurrentDrawing != null && h.CurrentDrawing.name.StartsWith(h.Spitter ? "moth_" : "bat_")), "Toda a tropa usa voadores; sapo removido do combate");
        Check(director.ThreatRoot.GetComponentsInChildren<LineRenderer>().All(l => !l.name.Contains("Vine")), "Mariposa paira sem cipó");
        yield return ObserveWings(moth, .45f, "Mariposa troca oito quadros de asas em voo real");
        int count = director.LivingHelpers; director.SpawnWave(); Check(director.LivingHelpers == count, "Sem reposição nem excesso de três ajudantes");
        yield return Until(() => Object.FindAnyObjectByType<MacaquiniSporeBubble>() != null, "moth releases spores");
        yield return Snapshot("mariposa-esporos");
        var bubble = Object.FindAnyObjectByType<MacaquiniSporeBubble>();
        var shotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerProjectile.prefab");
        // Resolve the player's actual projectile prefab by its target mask, independent of naming.
        if (shotPrefab == null) shotPrefab = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" })
            .Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))
            .First(p => p.GetComponent<Projectile>() != null && p.GetComponent<Projectile>().TargetsLayer(LayerMask.NameToLayer("Enemy")));
        var shot = Object.Instantiate(shotPrefab, bubble.transform.position + Vector3.left * .5f, Quaternion.identity).GetComponent<Projectile>();
        shot.Launch(Vector2.right, null, 8);
        yield return Until(() => bubble == null, "player shot cancels bubble");
        Check(Object.FindObjectsByType<MacaquiniSporeBubble>(FindObjectsSortMode.None).Length == 0, "Tiro real destrói a bolha antes da explosão");
        director.Spit(new Vector2(3, 2));
        player.GetComponent<Rigidbody2D>().position = new Vector2(-4, -1);
        yield return Until(() => Object.FindAnyObjectByType<MacaquiniSporeBubble>().Bursting, "four way burst");
        yield return Wait(.15f); yield return Snapshot("bolha-quatro-sementes");
        Check(director.ThreatRoot.GetComponentsInChildren<LineRenderer>().Count(l => l.name.StartsWith("SporeSeed") && l.enabled) == 4, "Bolha solta quatro sementes separadas");
        var seed = director.ThreatRoot.GetComponentsInChildren<LineRenderer>().First(l => l.name == "SporeSeed0");
        player.GetComponent<Rigidbody2D>().position = seed.GetPosition(0) + Vector3.up * .15f;
        hp = player.Health.CurrentHealth; player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), false);
        yield return Until(() => player.Health.CurrentHealth < hp, "real seed contact"); yield return Wait(.1f);
        Check(player.Health.CurrentHealth == hp - 1, "Semente causa um de dano com contato real");
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true);
        director.Cancel(); yield return Frames(3);
        Check(Object.FindAnyObjectByType<MacaquiniSporeBubble>() == null, "Cancelamento remove bolhas e sementes");
        // Real boss dash sweeps across a helper held inside its visible lane.
        combat.SetTarget(player.Health); combat.ForceAttack(MacaquiniCombat.Attack.JetDash);
        director.SpawnWave(); var friendly = Helpers()[0];
        yield return Until(() => boss.Movement.IsDashing && !combat.IsTelegraphing, "boss dash ignition");
        friendly.enabled = false;
        friendly.GetComponent<Rigidbody2D>().position = boss.transform.position;
        int friendlyBefore = director.FriendlyHits;
        yield return Until(() => director.FriendlyHits > friendlyBefore, "friendly fire sweep");
        Check(friendly == null || !friendly.Alive, "Rasante do boss elimina o próprio ajudante");
        combat.StopCombat(); yield return Frames(3); combat.SetTarget(player.Health); director.SpawnWave();
        boss.Health.TakeDamage(999);
        yield return Frames(4);
        Check(director.LivingHelpers == 0 && Object.FindAnyObjectByType<MacaquiniJungleHelper>() == null, "Transformação limpa todos os ajudantes da primeira forma");
        yield return Until(() => combat.PhaseTwo && boss.Ready, "second form");
        Check(boss.GetComponent<MacaquiniFighter>() != null && director.LivingHelpers == 0, "Segunda forma preservada sem ajudantes da primeira");
        combat.StopCombat(); StartIsolated(); director.SpawnWave();
        Check(director.LivingHelpers == 0, "Primeira tropa não nasce na segunda forma");
        boss.StopBattle(); yield return Frames(3);
        Check(Object.FindAnyObjectByType<MacaquiniSporeBubble>() == null, "Fim da batalha sem ameaças órfãs");
    }
    private static IEnumerator AttackScenario()
    {
        yield return ResetAttack(MacaquiniCombat.Attack.JetStrafe);
        int shots = combat.Releases; float began = Time.time;
        yield return Until(() => combat.Releases > shots, "first aimed fruit");
        Vector2 firstAim = combat.LastReleaseAim; Check(combat.LastReleaseCount == 1, "Rajada abre com tiro direto");
        player.GetComponent<Rigidbody2D>().position = new Vector2(-2, 2);
        yield return Until(() => combat.Releases > shots + 1, "second aimed fruit");
        Check(Vector2.Distance(firstAim, combat.LastReleaseAim) > 1, "Boss renova a mira entre disparos sem perseguir a banana em voo");
        yield return Until(() => combat.Releases == shots + 3, "third native release");
        Check(combat.LastReleaseCount == 3, "Terceiro disparo finaliza com leque curto de três bananas");
        Check(Time.time - began < 3.1f, "Rajada tem cadência mais ágil com três eventos nativos");
        yield return Snapshot("rajada-final");
        yield return ResetAttack(MacaquiniCombat.Attack.JetDash);
        var dash = Object.FindAnyObjectByType<MacaquiniDashAttack>();
        Check(dash != null && !dash.Dangerous && combat.IsTelegraphing, "Rasante anuncia sem causar dano no caminho");
        Check(Object.FindAnyObjectByType<MacaquiniHeatHazard>() == null, "Rasante não cria faixa vermelha de calor");
        var marks = dash.GetComponentsInChildren<SpriteRenderer>();
        Check(marks.Any(m => m.name == "DashWarningPixel") && marks.Where(m => m.name == "DashWarningPixel").All(m => m.color.g > .7f), "Aviso usa pequenos marcadores dourados");
        Vector2 point = Vector2.Lerp(dash.From, dash.Destination, .65f); player.GetComponent<Rigidbody2D>().position = point;
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), false); int hp = player.Health.CurrentHealth;
        yield return Wait(.2f); Check(player.Health.CurrentHealth == hp, "Ficar na rota durante o aviso não dá dano");
        yield return Snapshot("rasante-aviso");
        yield return Until(() => dash.Dangerous, "physical dash launch"); yield return Wait(.08f);
        Check(player.Health.CurrentHealth == hp, "Rasante não acerta à distância quando o boss ainda não chegou");
        yield return Until(() => player.Health.CurrentHealth < hp, "physical boss contact"); yield return Wait(.15f);
        Check(player.Health.CurrentHealth == hp - 1, "Contato real do corpo causa um dano único");
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true);
        yield return Snapshot("rasante-vento");
        yield return ResetAttack(MacaquiniCombat.Attack.ExhaustGates);
        var fruits = Object.FindObjectsByType<MacaquiniFallingBanana>();
        Check(fruits.Length == 3 && fruits.Select(f => f.LaneX).Distinct().Count() == 3, "Chuva lança três frutas em faixas separadas");
        Check(Object.FindAnyObjectByType<MacaquiniHeatHazard>() == null, "Chuva não usa colunas vermelhas");
        float lane = fruits.OrderBy(f => f.LaneX).ElementAt(1).LaneX;
        player.GetComponent<Rigidbody2D>().position = new Vector2(lane, 0); hp = player.Health.CurrentHealth;
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), false);
        yield return Wait(.4f); Check(player.Health.CurrentHealth == hp, "Frutas são inofensivas durante o lançamento e aviso");
        var positions = fruits.Select(f => f.transform.position).ToArray(); Time.timeScale = 0; yield return WallWait(.2f);
        Check(fruits.Select((f, i) => Vector3.Distance(f.transform.position, positions[i]) < .001f).All(v => v), "Pausa congela as frutas e os avisos"); Time.timeScale = 1;
        yield return Snapshot("chuva-lancamento");
        yield return Until(() => fruits.Any(f => f != null && f.Falling), "first falling fruit");
        Check(fruits.Any(f => f != null && !f.Falling), "Quedas são escalonadas em vez de simultâneas");
        yield return Snapshot("chuva-de-bananas");
        yield return Until(() => player.Health.CurrentHealth < hp, "falling banana physical contact"); yield return Wait(.15f);
        Check(player.Health.CurrentHealth == hp - 1, "Uma fruta aplica um dano quando alcança a nave");
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true);
        combat.StopCombat(); yield return Frames(3);
        Check(Object.FindAnyObjectByType<MacaquiniFallingBanana>() == null && Object.FindAnyObjectByType<MacaquiniDashAttack>() == null, "Cancelar limpa frutas, marcadores e vento");
        Check(Mathf.Approximately(boss.GetComponent<Animator>().speed, 1), "Cancelar restaura a velocidade de animação para a transformação");
    }
    private static IEnumerator ResetAttack(MacaquiniCombat.Attack attack)
    {
        combat.StopCombat(); yield return Frames(3);
        player.Health.SetInvulnerable(typeof(MacaquiniBananaV25Validation), true); player.GetComponent<Rigidbody2D>().position = Vector2.zero;
        boss.GetComponent<Rigidbody2D>().position = new Vector2(4, 1); boss.Movement.HoldPosition(true);
        combat.SetTarget(player.Health); combat.ForceAttack(attack); yield return Frames(3);
    }
    private static MacaquiniJungleHelper[] Helpers() => Object.FindObjectsByType<MacaquiniJungleHelper>(FindObjectsSortMode.None).Where(h => h.Alive).ToArray();
    private static IEnumerator ObserveWings(MacaquiniJungleHelper helper, float seconds, string label)
    {
        var seen = new HashSet<Sprite>(); float end = Time.time + seconds;
        while (Time.time < end) { if (helper != null) seen.Add(helper.CurrentDrawing); yield return null; }
        Check(helper != null && helper.WingFrameCount == 8 && seen.Count == 8 && !seen.Contains(null), label);
    }
    private static void StartIsolated()
    {
        boss.Movement.HoldPosition(true); player.Movement.SetMoveInput(Vector2.zero); player.GetComponent<Rigidbody2D>().position = Vector2.zero;
        director.Begin(player.Health, AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BossTelegraph.mat"));
    }
    private static IEnumerator Snapshot(string name)
    {
        yield return Frames(2); var camera = Camera.main; var texture = new RenderTexture(1200, 720, 24);
        var overlays = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
        var previous = camera.targetTexture; var active = RenderTexture.active; camera.targetTexture = texture;
        camera.Render(); RenderTexture.active = texture; var pixels = new Texture2D(1200, 720, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, 1200, 720), 0, 0); pixels.Apply();
        File.WriteAllBytes(Output + "/" + name + ".png", pixels.EncodeToPNG());
        camera.targetTexture = previous; RenderTexture.active = active; texture.Release(); Object.Destroy(texture); Object.Destroy(pixels);
        foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static IEnumerator Frames(int count) { while (count-- > 0) yield return null; }
    private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    private static IEnumerator WallWait(float seconds) { double end = EditorApplication.timeSinceStartup + seconds; while (EditorApplication.timeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label) { double end = EditorApplication.timeSinceStartup + 35; while (!predicate()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label); yield return null; } }
    private static void Check(bool value, string label) { if (!value) throw new InvalidOperationException("BANANA25_FAIL " + label); checks++; Debug.Log("BANANA25_CHECK " + label); }
    private static void OnLog(string message, string trace, LogType type) { if (trace.Contains("UnityEditor.Search.SearchDatabase")) return; if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Banana25 deadline");
            while (steps.Count > 0) { var current = steps.Peek(); if (!current.MoveNext()) { steps.Pop(); continue; } if (current.Current is IEnumerator child) { steps.Push(child); continue; } return; }
            Debug.Log("MACAQUINI_BANANA25_PLAY_OK checks=" + checks + " errors=" + errors);
        } catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog; Time.timeScale = 1;
        File.WriteAllText(Output + "/validation.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        EditorApplication.isPlaying = false;
    }
}
