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
public static class JungleStagePlayValidation
{
    private const string Key = "Crocodilo.Jungle.Validation";
    private const string RegressionKey = "Crocodilo.Jungle.GusiniRegression";
    private const string Output = "Docs/ArtPreviews/JungleStage/Gameplay";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static bool running;
    private static int checks, errors;
    private static double deadline;
    static JungleStagePlayValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(RegressionKey, false)) Application.runInBackground = true;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(RegressionKey, false))
            {
                SessionState.SetBool(RegressionKey, false);
                var field = typeof(GusiniPhaseTwoPlayValidation).GetField("errors", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                EditorApplication.Exit((int)field.GetValue(null) == 0 ? 0 : 1);
            }
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false)) Begin();
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key, false))
            {
                SessionState.SetBool(Key, false);
                if (Application.isBatchMode || Environment.GetCommandLineArgs().Contains("-jungle-test-exit")) EditorApplication.Exit(errors == 0 ? 0 : 1);
            }
        };
        if (SessionState.GetBool(Key, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }
    public static void RunGusiniRegression()
    {
        SessionState.SetBool(RegressionKey, true);
        GusiniPhaseTwoPlayValidation.Run();
    }
    [MenuItem("Crocodilo/Fase 2/Validate jungle in Play Mode")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        JungleStageTools.Validate();
        EditorSceneManager.OpenScene(JungleStageTools.Stage);
        JungleStageTools.Find<WaveController>(UnityEngine.SceneManagement.SceneManager.GetActiveScene()).enabled = false;
        JungleStageTools.Find<ChapterFlow>(UnityEngine.SceneManagement.SceneManager.GetActiveScene()).enabled = false;
        SessionState.SetBool(Key + ".Showcase", false); SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    public static void Showcase()
    {
        EditorSceneManager.OpenScene(JungleStageTools.Test);
        SessionState.SetBool(Key + ".Showcase", true); SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
    }
    private static void Begin()
    {
        if (running) return;
        running = true; checks = errors = 0; deadline = EditorApplication.timeSinceStartup + 300;
        Application.runInBackground = true;
        Directory.CreateDirectory(Output);
        Application.logMessageReceived += OnLog;
        steps.Clear(); steps.Push(SessionState.GetBool(Key + ".Showcase", false) ? MotionScenario() : Scenario()); EditorApplication.update += Tick;
    }
    private static IEnumerator MotionScenario()
    {
        LeaderboardSession.Cancel(); CampaignProgress.UseDirectory("Logs/JungleStage/IsolatedSave");
        var player = Object.FindAnyObjectByType<Player>(); player.Health.SetInvulnerable(typeof(JungleStagePlayValidation), true);
        var waves = Object.FindAnyObjectByType<WaveController>();
        yield return Until(() => waves.ActiveBoss != null && waves.ActiveBoss.Ready, "entrada para prévia");
        var boss = waves.ActiveBoss; boss.Health.SetInvulnerable(typeof(JungleStagePlayValidation), true);
        var combat = boss.GetComponent<MacaquiniCombat>(); var rig = boss.transform.Find("Visual");
        var background = Object.FindAnyObjectByType<JungleBackdrop>();
        Check(background.VineCount == 8, "Oito cipós em planos independentes");
        Check(background.ParallaxLayerCount == 4, "Quatro faixas transparentes além da mata profunda e cipós");
        var drawings = boss.GetComponent<MacaquiniPresentation>();
        var seenDrawings = new HashSet<int>();
        var farTile = background.ParallaxRoot.Find("RemoteTrunks/Segment0");
        var middleTile = background.ParallaxRoot.Find("MiddleTrunks/Segment0");
        float farStart = farTile.position.x, middleStart = middleTile.position.x;
        float until = Time.time + 1f;
        while (Time.time < until) { seenDrawings.Add(drawings.CurrentDrawing); yield return null; }
        Check(drawings.DrawingCount == 4, "Quatro desenhos próprios da mão: alcance e fechamento da pegada");
        var jointMeshes = boss.GetComponentsInChildren<SpriteRenderer>().Where(r => r.sprite != null && (r.sprite.name.EndsWith("_rounded") || r.name.Contains("Overlap"))).ToArray();
        Check(jointMeshes.Length == 8 && jointMeshes.All(r => r.sprite.vertices.Length > 4), "Oito recortes arredondados preservam a malha em execução");
        float farTravel = Mathf.Abs(farTile.position.x - farStart), middleTravel = Mathf.Abs(middleTile.position.x - middleStart);
        Check(farTravel > .08f && middleTravel > farTravel * 2f, "Parallax usa velocidades distintas com deslocamento contínuo");
        Quaternion pose = rig.localRotation; float clock = background.WindClock;
        yield return Wait(.3f); Check(Quaternion.Angle(pose, rig.localRotation) > .2f && background.WindClock > clock, "Boss e fundo têm movimento real");
        var pause = Object.FindAnyObjectByType<PauseController>(); pause.PauseGame(); yield return Frames(2);
        pose = rig.localRotation; clock = background.WindClock; yield return Wait(.2f);
        Check(Quaternion.Angle(pose, rig.localRotation) < .001f && Mathf.Approximately(clock, background.WindClock), "Pausa congela rig e vento");
        Vector3 pausedTile = middleTile.position; int pausedDrawing = drawings.CurrentDrawing;
        yield return Wait(.2f);
        Check(middleTile.position == pausedTile && drawings.CurrentDrawing == pausedDrawing, "Pausa congela faixas e desenhos do boss");
        pause.ResumeGame(); yield return Frames(2);
        yield return Wait(.4f);
        Check(rig.Find("braco_distante/Weapon/Muzzle") != null && rig.Find("braco_proximo/SupportHand") != null, "Arma e cipó presos às mãos");
        combat.ForceAttack(MacaquiniCombat.Attack.BananaVolley);
        yield return Capture(1200, 720, "closed_shell");
        yield return RecordMotion();
        yield return Capture(1200, 720, "motion_boss");
        yield return Capture(800, 600, "motion_boss");
        yield return Capture(1920, 1080, "motion_boss");
    }
    private static IEnumerator RecordMotion()
    {
        string directory = "Logs/JungleStage/MotionFrames"; Directory.CreateDirectory(directory);
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
        var texture = new RenderTexture(1200, 720, 24); camera.targetTexture = texture;
        var pixels = new Texture2D(1200, 720, TextureFormat.RGB24, false);
        var recorder = camera.gameObject.AddComponent<JungleMotionAudioCapture>();
        yield return Frames(2); recorder.Begin();
        float start = Time.unscaledTime, next = 0; int count = 0, stagedAttack = 0;
        var motionCombat = Object.FindAnyObjectByType<MacaquiniCombat>();
        var times = new List<float>();
        var usedDrawings = new HashSet<int>();
        float worstLoadedGrip = 0, worstWeaponGrip = 0, worstPeelGrip = 0, maxTearProgress = 0, minElbow = 360, maxElbow = 0, loadedTravel = 0;
        Vector3 previousGrip = Vector3.zero; bool previouslyLoaded = false;
        float minBossY = motionCombat.transform.position.y, maxBossY = minBossY;
        bool startedTear = false, protectedHalfShell = false;
        while (Time.unscaledTime - start < 22)
        {
            float elapsed = Time.unscaledTime - start;
            var anatomy = motionCombat.GetComponent<MacaquiniPresentation>();
            usedDrawings.Add(anatomy.CurrentDrawing);
            worstWeaponGrip = Mathf.Max(worstWeaponGrip, anatomy.WeaponGripError);
            worstPeelGrip = Mathf.Max(worstPeelGrip, anatomy.PeelGripError);
            maxTearProgress = Mathf.Max(maxTearProgress, anatomy.TearProgress);
            minElbow = Mathf.Min(minElbow, anatomy.SupportElbowAngle); maxElbow = Mathf.Max(maxElbow, anatomy.SupportElbowAngle);
            if (anatomy.GripLoaded && !motionCombat.GetComponent<Boss>().Transitioning)
            {
                worstLoadedGrip = Mathf.Max(worstLoadedGrip, anatomy.SupportGripError);
                if (previouslyLoaded) loadedTravel = Mathf.Max(loadedTravel, Vector3.Distance(previousGrip, anatomy.GripPoint));
            }
            previouslyLoaded = anatomy.GripLoaded && !motionCombat.GetComponent<Boss>().Transitioning; previousGrip = anatomy.GripPoint;
            if (!startedTear && elapsed >= 7.5f)
            {
                var motionBoss = motionCombat.GetComponent<Boss>(); motionBoss.Health.SetInvulnerable(typeof(JungleStagePlayValidation), false);
                motionBoss.Health.TakeDamage(99999); startedTear = true;
            }
            if (!protectedHalfShell && motionCombat.GetComponent<MacaquiniPhaseTwo>().CurrentStage == MacaquiniPhaseTwo.Stage.HalfShell)
            {
                motionCombat.GetComponent<Health>().SetInvulnerable(typeof(JungleStagePlayValidation), true); protectedHalfShell = true;
            }
            minBossY = Mathf.Min(minBossY, motionCombat.transform.position.y); maxBossY = Mathf.Max(maxBossY, motionCombat.transform.position.y);
            if (elapsed >= 4.5f + stagedAttack * 4.5f && stagedAttack < 3 && motionCombat.GetComponent<Boss>().Ready)
            {
                motionCombat.ForceAttack(stagedAttack == 0 ? MacaquiniCombat.Attack.BananaFan : stagedAttack == 1 ? MacaquiniCombat.Attack.Vines : MacaquiniCombat.Attack.BananaVolley);
                stagedAttack++;
            }
            if (elapsed >= next)
            {
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, 1200, 720), 0, 0); pixels.Apply(); RenderTexture.active = null;
                File.WriteAllBytes(directory + "/frame_" + count.ToString("D5") + ".png", pixels.EncodeToPNG());
                times.Add(elapsed); count++; next += .05f;
            }
            yield return null;
        }
        recorder.Save("Logs/JungleStage/motion_audio.wav"); Object.Destroy(recorder);
        var concat = new System.Text.StringBuilder("ffconcat version 1.0\n");
        for (int i = 0; i < count; i++) { concat.Append("file 'frame_").Append(i.ToString("D5")).Append(".png'\n"); concat.Append("duration ").Append((i + 1 < count ? times[i + 1] - times[i] : .05f).ToString("F6", System.Globalization.CultureInfo.InvariantCulture)).Append('\n'); }
        File.WriteAllText(directory + "/frames.ffconcat", concat.ToString());
        camera.targetTexture = null; foreach (Canvas canvas in canvases) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(pixels); texture.Release(); Object.Destroy(texture);
        Check(count > 100, "Prévia capturada do PlayerLoop real: " + count + " quadros");
        Check(protectedHalfShell && motionCombat.GetComponent<MacaquiniPresentation>().HalfShell, "Captura mostra a casca fechada, o rasgo e o macaco parcialmente libertado");
        Check(usedDrawings.Count == 4, "As quatro fases de abertura/fechamento da mão aparecem na luta");
        Check(worstLoadedGrip < .015f && loadedTravel < .015f, "Mão carregada permanece fixa no cipó sem desencaixe: " + worstLoadedGrip.ToString("F4"));
        Check(worstWeaponGrip < .001f, "Banana original mantém a pegada em todos os quadros: " + worstWeaponGrip.ToString("F4"));
        Check(worstPeelGrip < .02f, "Mão acompanha a própria peça da casca durante o rasgo: " + worstPeelGrip.ToString("F4"));
        Check(maxTearProgress > .8f, "Clip nativo anima a abertura real da casca: " + maxTearProgress.ToString("F2"));
        Check(maxElbow - minElbow > 15, "Cotovelo articula de verdade com a sustentação do corpo: " + (maxElbow - minElbow).ToString("F1") + " graus");
        Check(maxBossY - minBossY > 2.5f, "Boss percorre arco vertical amplo: " + (maxBossY - minBossY).ToString("F2") + " unidades");
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(3);
        LeaderboardSession.Cancel();
        CampaignProgress.UseDirectory("Logs/JungleStage/IsolatedSave");
        var waves = Object.FindAnyObjectByType<WaveController>();
        var player = Object.FindAnyObjectByType<Player>();
        var spawner = waves.GetComponent<EnemySpawner>();
        var rewards = Object.FindAnyObjectByType<CardRewardController>();
        var pause = Object.FindAnyObjectByType<PauseController>();
        player.Health.SetInvulnerable(typeof(JungleStagePlayValidation), true);
        Check(waves.BossPrefab.GetComponent<MacaquiniCombat>() != null, "Boss próprio configurado");
        Check(waves.CombatWaveCount == 9 && waves.WaveCount == 10, "Nove hordas mais boss");
        Check(!CampaignProgress.Has("chapter2_completed"), "Progresso isolado antes da conclusão");
        Time.timeScale = 6f;
        waves.enabled = true; waves.StartRun();
        bool sawBat = false, sawWasp = false;
        for (int wave = 1; wave <= 9; wave++)
        {
            int expected = wave;
            yield return Until(() => waves.CurrentWave == expected && waves.Phase == WaveController.RunPhase.Combat, "combate da horda " + wave);
            while (waves.Phase == WaveController.RunPhase.Combat)
            {
                foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                {
                    if (enemy.Health.IsDead) continue;
                    sawBat |= enemy.name.Contains("JungleBat"); sawWasp |= enemy.name.Contains("JungleWasp");
                    enemy.Health.TakeDamage(9999);
                }
                yield return null;
            }
            Check(waves.AwaitingReward, "Carta real após horda " + wave);
            yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "cartas prontas " + wave);
            Check(rewards.TryChoose(0), "Escolha de melhoria " + wave);
            yield return Until(() => !waves.AwaitingReward, "liberação da próxima horda");
        }
        Check(sawBat && sawWasp, "Os dois bichos passaram pelo EnemySpawner real");
        Check(waves.Score.Kills == 164, "164 bichos contabilizados uma vez");
        Check(waves.Score.Points == 140200, "Abates e nove bônus de horda corretos");
        Time.timeScale = 1;
        yield return Until(() => waves.ActiveBoss != null && waves.ActiveBoss.Ready, "entrada do boss");
        var boss = waves.ActiveBoss; var combat = boss.GetComponent<MacaquiniCombat>();
        Check(!boss.Health.IsInvulnerable, "Hitbox e vulnerabilidade liberadas após entrada");
        var bossHud = Object.FindAnyObjectByType<BossHUD>(); var bar = bossHud.ActiveBar;
        Check(bar != null && bar.gameObject.activeInHierarchy && bar.name == "MacaquiniBar", "Macaquini usa sua barra exclusiva da selva");
        Check(bar.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i => i.name == "name_text" && i.sprite.name == "hud_title"), "Nome pixel art correto na HUD");
        Check(bar.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i => i.name == "emblem_roundel" && i.sprite.name == "hud_portrait"), "Medalhão usa o retrato do Macaquini");
        combat.ForceAttack(MacaquiniCombat.Attack.BananaVolley);
        yield return Until(() => combat.IsTelegraphing, "aviso de mira");
        int before = combat.Releases;
        pause.PauseGame(); yield return Frames(2);
        float held = Time.time;
        yield return Wait(.3f);
        Check(pause.IsPaused && Mathf.Approximately(Time.time, held) && combat.Releases == before, "Pausa congela aviso e eventos");
        pause.ResumeGame();
        yield return Until(() => combat.Releases >= before + 3, "três releases da salva");
        Check(combat.Releases == before + 3, "Três releases, sem duplicação");
        combat.MacaquiniRelease(); Check(combat.Releases == before + 3, "Animation event duplicado rejeitado");
        combat.ForceAttack(MacaquiniCombat.Attack.BananaFan);
        before = combat.Releases;
        yield return Until(() => combat.Releases > before, "release do leque");
        var bananas = combat.ThreatRoot.GetComponentsInChildren<Projectile>();
        Check(bananas.Length == 5, "Leque com cinco projéteis separados");
        var velocities = bananas.Select(b => b.GetComponent<Rigidbody2D>().linearVelocity).ToArray();
        Check(velocities.All(v => Mathf.Abs(v.magnitude - 6f) < .01f) && velocities.Select(v => Mathf.Round(v.y * 100)).Distinct().Count() == 5, "Velocidade fixa e direções distintas");
        yield return Capture(1200, 720, "macaquini_fan");
        combat.ForceAttack(MacaquiniCombat.Attack.Vines);
        yield return Frames(2);
        var vines = combat.ThreatRoot.GetComponentsInChildren<JungleVineHazard>();
        Check(vines.Length == 2 && vines.All(v => !v.Dangerous), "Duas faixas avisadas e uma faixa inteira livre");
        var line = vines[0].GetComponent<LineRenderer>(); float laneY = line.GetPosition(0).y;
        Check(Mathf.Abs(laneY - vines[1].GetComponent<LineRenderer>().GetPosition(0).y) > 2.5f, "Faixas separadas deixam corredor largo");
        yield return Capture(1200, 720, "vine_warning");
        pause.PauseGame(); yield return Frames(2); yield return Wait(.25f);
        Check(vines.All(v => !v.Dangerous), "Pausa mantém cipós inofensivos no aviso");
        pause.ResumeGame();
        player.GetComponent<Rigidbody2D>().position = new Vector2(-3f, laneY);
        player.Health.SetInvulnerable(typeof(JungleStagePlayValidation), false);
        float hp = player.Health.PreciseCurrentHealth;
        yield return Until(() => vines.Any(v => v.Dangerous), "cipó ativo");
        yield return Frames(3);
        Check(Mathf.Abs(player.Health.PreciseCurrentHealth - (hp - 2f)) < .01f, "Cipó aplica dois de dano físico uma vez");
        yield return Wait(.1f);
        Check(Mathf.Abs(player.Health.PreciseCurrentHealth - (hp - 2f)) < .01f, "Sobreposição não repete dano do mesmo golpe");
        player.Health.SetInvulnerable(typeof(JungleStagePlayValidation), true);
        combat.ForceAttack(MacaquiniCombat.Attack.BananaFan);
        yield return Capture(800, 600, "macaquini");
        combat.ForceAttack(MacaquiniCombat.Attack.Vines);
        yield return Capture(1920, 1080, "macaquini");
        int completed = 0; waves.ChapterCompleted += () => completed++;
        boss.Health.TakeDamage(99999);
        var macaPhase = boss.GetComponent<MacaquiniPhaseTwo>();
        Check(macaPhase.CurrentStage == MacaquiniPhaseTwo.Stage.Tearing && !boss.Health.IsDead && boss.Transitioning, "Primeiro esgotamento inicia rasgo sem matar o macaco");
        Check(waves.Score.Kills == 164 && waves.Score.Points == 140200, "Primeira forma não concede abate nem pontos de boss");
        Check(combat.ThreatRoot == null && !combat.PerformingAttack, "Rasgo cancela ataques e limpa as ameaças");
        Check(!boss.Health.TakeDamage(99999), "Transformação protege contra dano excedente");
        pause.PauseGame(); yield return Frames(2);
        var animator = boss.GetComponent<Animator>(); float tearTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        yield return Wait(.2f);
        Check(Mathf.Abs(animator.GetCurrentAnimatorStateInfo(0).normalizedTime - tearTime) < .001f, "Pausa congela animação do rasgo");
        pause.ResumeGame();
        yield return Until(() => macaPhase.CurrentStage == MacaquiniPhaseTwo.Stage.HalfShell, "metade da casca rasgada");
        Check(boss.GetComponent<MacaquiniPresentation>().HalfShell && boss.Health.MaxHealth == 180, "Segunda forma mostra macaco e conserva metade da banana");
        yield return Until(() => boss.Ready, "segunda forma pronta");
        combat.ForceAttack(MacaquiniCombat.Attack.BananaFan);
        int halfReleases = combat.Releases;
        yield return Until(() => combat.Releases > halfReleases, "leque da segunda forma");
        Check(combat.ThreatRoot.GetComponentsInChildren<Projectile>().Length == 7, "Segunda forma usa leque de sete bananas com o mesmo aviso");
        yield return Capture(1200, 720, "half_shell");
        boss.Health.TakeDamage(99999);
        Check(combat.ThreatRoot == null && !combat.IsTelegraphing, "Morte limpa ameaças imediatamente");
        Check(waves.Score.Kills == 165 && waves.Score.Points == 180200, "Boss concede 40000 pontos e um abate");
        yield return Until(() => waves.Phase == WaveController.RunPhase.ChapterCompleted, "conclusão do capítulo 2");
        Check(completed == 1, "Conclusão emitida uma vez");
        Check(!CampaignProgress.Has("gusini_rescued") && !CampaignProgress.Has("chapter1_completed"), "Teste não grava marcos do Gusini");
        var chapter = Object.FindAnyObjectByType<ChapterFlow>().Chapter;
        Check(chapter.ChapterId == "chapter2" && chapter.CompletionFlags.SequenceEqual(new[] { "chapter2_completed" }), "Dados de progresso exclusivos do capítulo 2");
        yield return TestCancellation(player);
        yield return TestCreatures(spawner, player);
        var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Assets/Scenes/MainMenu.unity");
        while (!operation.isDone) yield return null;
        yield return Frames(3);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        var menuGroup = (CanvasGroup)new SerializedObject(menu).FindProperty("menuGroup").objectReferenceValue;
        yield return Until(() => menuGroup.interactable && menuGroup.alpha > .99f, "abertura completa do menu");
        var jungleButton = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.name == "JungleMission");
        Check(jungleButton != null, "Entrada da fase 2 disponível no menu");
        yield return Capture(1200, 720, "menu_phase2");
        jungleButton.onClick.Invoke(); yield return Frames(2);
        Check(Object.FindAnyObjectByType<PlayerNameView>().IsVisible, "Botão da selva abre o fluxo normal de nome");
        var selection = typeof(MainMenuController).GetField("selectedGameplayScene", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Check((string)selection.GetValue(menu) == MainMenuController.JungleStageScene, "Nome e loading usam o capítulo escolhido");
        Object.FindAnyObjectByType<PlayerNameView>().Close(); menu.PlayGame();
        Check((string)selection.GetValue(menu) == "Assets/Scenes/Tests/PlayerMovementTest.unity", "Jogar original continua selecionando o Gusini");
        Object.FindAnyObjectByType<PlayerNameView>().Close();
    }
    private static IEnumerator TestCreatures(EnemySpawner spawner, Player player)
    {
        var batPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/JungleBat.prefab").GetComponent<Enemy>();
        var waspPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/JungleWasp.prefab").GetComponent<Enemy>();
        var bat = spawner.SpawnReinforcements(batPrefab, 1, 6).Single();
        var wasp = spawner.SpawnReinforcements(waspPrefab, 1, 6).Single();
        Time.timeScale = 2;
        yield return Until(() => bat.Combat.IsTelegraphing, "aviso do morcego");
        yield return Frames(2);
        Check(bat.GetComponentInChildren<LineRenderer>().enabled, "Morcego mostra mira antes do leque");
        yield return Until(() => Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length >= 3, "leque real do morcego");
        Check(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Any(p => p.TargetsLayer(player.gameObject.layer)), "Morcego dispara contra a layer do jogador");
        yield return Until(() => wasp.Combat.IsTelegraphing, "aviso da vespa");
        yield return Frames(2);
        Check(wasp.GetComponentInChildren<LineRenderer>().enabled, "Vespa mostra trajetória antes da investida");
        yield return Until(() => wasp.Movement.IsDashing, "investida real da vespa");
        // Editor ticks can precede FixedUpdate: wait for the actual physics velocity.
        yield return Until(() => wasp.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 7f, "velocidade física da vespa após FixedUpdate");
        Check(wasp.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 7f, "Vespa executa investida física");
        yield return Capture(1200, 720, "jungle_creatures");
        spawner.StopCombat();
        Check(!bat.Combat.enabled && !wasp.Combat.enabled, "Fim de combate desliga os ataques dos bichos");
        yield return Frames(2);
        Check(!bat.GetComponentInChildren<LineRenderer>().enabled && !wasp.GetComponentInChildren<LineRenderer>().enabled, "Fim de combate apaga os avisos");
        Object.Destroy(bat.gameObject); Object.Destroy(wasp.gameObject);
        foreach (var projectile in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Object.Destroy(projectile.gameObject);
        Time.timeScale = 1;
        yield return Frames(2);
    }
    private static IEnumerator TestCancellation(Player player)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(JungleStageTools.BossPath);
        var targetRoot = new GameObject("IsolatedTarget"); targetRoot.layer = LayerMask.NameToLayer("Player");
        targetRoot.transform.position = new Vector3(-6, 2);
        var target = targetRoot.AddComponent<Health>();
        var boss = Object.Instantiate(prefab, new Vector3(5.2f, 2f), Quaternion.identity).GetComponent<Boss>();
        boss.BeginBattle(target);
        yield return Until(() => boss.Ready, "boss isolado pronto");
        var combat = boss.GetComponent<MacaquiniCombat>(); combat.ForceAttack(MacaquiniCombat.Attack.Vines);
        yield return Frames(2);
        Check(combat.ThreatRoot != null, "Ameaças isoladas criadas");
        target.TakeDamage(9999);
        Check(combat.ThreatRoot == null, "Morte do alvo cancela ameaças");
        Object.Destroy(boss.gameObject); Object.Destroy(targetRoot); yield return Frames(2);
        boss = Object.Instantiate(prefab, new Vector3(5.2f, 2f), Quaternion.identity).GetComponent<Boss>(); boss.BeginBattle(player.Health);
        yield return Until(() => boss.Ready, "segundo boss isolado pronto");
        combat = boss.GetComponent<MacaquiniCombat>(); combat.ForceAttack(MacaquiniCombat.Attack.Vines); yield return Frames(2);
        boss.gameObject.SetActive(false);
        Check(combat.ThreatRoot == null, "Disable cancela ameaças e coroutines");
        Object.Destroy(boss.gameObject);
        yield return Frames(2);
        Check(Object.FindObjectsByType<JungleVineHazard>(FindObjectsSortMode.None).Length == 0, "Nenhum cipó órfão");
    }
    private static void Tick()
    {
        EditorApplication.QueuePlayerLoopUpdate();
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Jungle validation deadline");
            while (steps.Count > 0)
            {
                var step = steps.Peek(); if (!step.MoveNext()) { steps.Pop(); continue; }
                if (step.Current is IEnumerator nested) { steps.Push(nested); continue; } return;
            }
            if (errors > 0) throw new InvalidOperationException("Unexpected runtime errors: " + errors);
            Debug.Log("JUNGLE_PLAY_OK checks=" + checks + " errors=0");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
        Directory.CreateDirectory("Logs/JungleStage");
        File.WriteAllText("Logs/JungleStage/result.json", "{\"checks\":" + checks + ",\"errors\":" + errors + "}");
        running = false; Time.timeScale = 1; AudioListener.pause = false; EditorApplication.isPlaying = false;
    }
    private static void OnLog(string message, string trace, LogType type)
    {
        if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("JUNGLE_CHECK_FAILED " + message);
        checks++; Debug.Log("JUNGLE_CHECK " + message);
    }
    private static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
    private static IEnumerator Wait(float seconds) { double end = EditorApplication.timeSinceStartup + seconds; while (EditorApplication.timeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> condition, string label)
    {
        double end = EditorApplication.timeSinceStartup + 30;
        while (!condition())
        {
            if (EditorApplication.timeSinceStartup > end)
            {
                var waves = Object.FindAnyObjectByType<WaveController>(); var rewards = Object.FindAnyObjectByType<CardRewardController>(); var pause = Object.FindAnyObjectByType<PauseController>();
                throw new TimeoutException(label + "; phase=" + waves?.Phase + "; cards=" + rewards?.State + "; paused=" + pause?.IsPaused + "; timeScale=" + Time.timeScale);
            }
            yield return null;
        }
    }
    private static IEnumerator Capture(int width, int height, string name)
    {
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var texture = new RenderTexture(width, height, 24);
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
        camera.targetTexture = texture; yield return Frames(2); Canvas.ForceUpdateCanvases(); camera.Render();
        var jungle = Object.FindAnyObjectByType<JungleBackdrop>();
        if (jungle != null && jungle.ParallaxRoot != null)
        {
            float halfWidth = camera.orthographicSize * camera.aspect;
            foreach (var layer in jungle.ParallaxRoot.GetComponentsInChildren<ScrollingLayer>())
            {
                var tiles = layer.GetComponentsInChildren<SpriteRenderer>().OrderBy(s => s.bounds.min.x).ToArray();
                bool covered = tiles.First().bounds.min.x <= camera.transform.position.x - halfWidth + .01f
                    && tiles.Last().bounds.max.x >= camera.transform.position.x + halfWidth - .01f;
                for (int i = 1; i < tiles.Length; i++) covered &= tiles[i].bounds.min.x <= tiles[i - 1].bounds.max.x + .01f;
                Check(covered, "Faixa contínua " + layer.name + " em " + width + "x" + height);
            }
        }
        RenderTexture.active = texture; var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply(); RenderTexture.active = null;
        var boss = Object.FindAnyObjectByType<MacaquiniCombat>();
        if (boss != null && boss.GetComponent<Health>().IsDead == false)
        {
            // Compare against the same frame without the boss: catches opaque scenery drawn on top.
            var sprites = boss.GetComponentsInChildren<SpriteRenderer>().Where(s => s.enabled).ToArray();
            var original = pixels.GetPixels32();
            foreach (var sprite in sprites) sprite.enabled = false;
            camera.Render(); RenderTexture.active = texture;
            var without = new Texture2D(width, height, TextureFormat.RGB24, false);
            without.ReadPixels(new Rect(0, 0, width, height), 0, 0); without.Apply(); RenderTexture.active = null;
            var hidden = without.GetPixels32(); int different = 0;
            for (int i = 0; i < hidden.Length; i++) if (!hidden[i].Equals(original[i])) different++;
            foreach (var sprite in sprites) sprite.enabled = true;
            Object.Destroy(without);
            Check(different > 100, "Boss visível na captura " + width + "x" + height);
        }
        File.WriteAllBytes(Output + "/" + name + "_" + width + "x" + height + ".png", pixels.EncodeToPNG());
        camera.targetTexture = null;
        foreach (Canvas canvas in canvases) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(pixels); texture.Release(); Object.Destroy(texture); yield return Frames(1);
    }
}
