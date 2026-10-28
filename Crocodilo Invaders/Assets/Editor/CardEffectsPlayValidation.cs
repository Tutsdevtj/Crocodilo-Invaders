using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Exercita a cena, prefab e coroutines reais; não modifica nem salva a cena.
[InitializeOnLoad]
public static class CardEffectsPlayValidation
{
    private const string Key = "Crocodilo.CardEffects.Validation";
    private const string Output = "Logs/CardEffects";
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static bool running;
    private static int checks, errors;
    private static double deadline;
    static CardEffectsPlayValidation()
    {
        EditorApplication.wantsToQuit += () => !SessionState.GetBool(Key, false);
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false)) Begin();
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key, false))
            {
                SessionState.SetBool(Key, false);
                if (Application.isBatchMode) EditorApplication.Exit(errors == 0 ? 0 : 1);
            }
        };
        if (SessionState.GetBool(Key, false) && EditorApplication.isPlaying) EditorApplication.delayCall += Begin;
    }
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        CardAudioPackageTools.Validate();
        EditorSceneManager.OpenScene("Assets/Scenes/Tests/PlayerMovementTest.unity");
        Find<WaveController>().enabled = false;
        SessionState.SetBool(Key, true);
        EditorApplication.isPlaying = true;
    }
    private static void Begin()
    {
        if (running) return;
        running = true; checks = errors = 0;
        Directory.CreateDirectory(Output);
        Application.logMessageReceived += OnLog;
        deadline = EditorApplication.timeSinceStartup + 180;
        DevelopmentCardSession.Begin();
        DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.FixedRarity;
        DevelopmentCardSession.Rarity = UpgradeRarity.Common;
        steps.Clear(); steps.Push(Scenario());
        EditorApplication.update += Tick;
    }
    private static IEnumerator Scenario()
    {
        yield return Frames(3);
        var waves = Find<WaveController>();
        waves.enabled = true;
        yield return Frames(2);
        var rewards = Find<CardRewardController>();
        var view = Find<CardRewardView>();
        var pause = Find<PauseController>();
        Check(waves.StartCardDevelopment(), "Abre recompensa real no DEV");
        for (int rank = 0; rank < 5; rank++)
        {
            if (rank > 0)
            {
                DevelopmentCardSession.Rarity = (UpgradeRarity)rank;
                Check(rewards.TryReroll(), "Reroll da raridade " + rank);
            }
            yield return Until(() => view.Options.All(o => o.FrontShowing && o.Rarity == (UpgradeRarity)rank), "revelação das três cartas");
            yield return Wait(.12f);
            var feedback = view.Options.Select(o => o.GetComponentInChildren<CardRarityFeedback>(true)).ToArray();
            Check(feedback.All(f => f.IsRevealed && f.HasAura == (rank > 0)), "Aura correta: " + rank);
            if (rank >= 3) yield return Capture(1200, 720, "impact_" + rank);
            yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "Ready");
            Check(view.Options.All(o => o.Rarity == (UpgradeRarity)rank), "Três cartas na raridade " + rank);
            yield return Wait(.3f);
            Check(view.TreasureRarity == (rank >= 3 ? (UpgradeRarity)rank : UpgradeRarity.Common),
                "Tesouro somente em lendárias/míticas: " + rank);
            Check(view.TreasureVoices == (rank >= 3 ? 1 : 0), "Um único loop compartilhado por oferta: " + rank);
            AudioSource treasure = rank == 3 ? view.LegendaryTreasureSource : rank == 4 ? view.MythicTreasureSource : null;
            if (treasure != null)
            {
                Check(treasure.loop && treasure.clip != null && treasure.volume > .1f, "Loop contínuo com volume configurado");
                int initialSample = treasure.timeSamples;
                yield return Wait(.15f);
                Check(treasure.timeSamples != initialSample, "Áudio do tesouro realmente avança");
            }
            if (rank > 0)
                foreach (var option in view.Options)
                {
                    var sound = option.GetComponentInChildren<CardRarityAudio>(true);
                    string expected = new[] { "", "RareReveal", "EpicReveal", "LegendaryReveal", "MythicReveal" }[rank];
                    Check(sound.LastRevealClip != null && sound.LastRevealClip.name == expected,
                        "Som próprio da raridade: " + expected);
                }
            float[] frozen = feedback.Select(f => f.Elapsed).ToArray();
            pause.PauseGame();
            int pausedSample = treasure != null ? treasure.timeSamples : 0;
            // Guardar depois da chamada evita diferença de um frame do editor.
            frozen = feedback.Select(f => f.Elapsed).ToArray();
            yield return Wait(.2f);
            if (treasure != null) Check(Mathf.Abs(treasure.timeSamples - pausedSample) < 1024, "Pausa congela o áudio de tesouro");
            Check(feedback.Select((f, i) => Mathf.Approximately(f.Elapsed, frozen[i])).All(v => v), "Pausa congela efeitos: " + rank);
            Check(!rewards.TryReroll(), "Pausa bloqueia reroll: " + rank);
            pause.ResumeGame();
            yield return Wait(.25f);
            Check(feedback.Select((f, i) => f.Elapsed > frozen[i]).All(v => v), "Efeitos retomam: " + rank);
            yield return Capture(1200, 720, "rarity_" + rank);
            if (rank == 0)
            {
                view.SetPaused(true);
                foreach (var option in view.Options) option.ShowFront(false);
                yield return Capture(1200, 720, "card_backs");
                Check(view.Options.All(o => !o.FrontShowing), "Versos exibidos nos três slots");
                foreach (var option in view.Options) option.ShowFront(true);
                view.SetPaused(false);
            }
            if (rank == 4)
            {
                yield return Capture(800, 600, "three_mythics");
                yield return Capture(1920, 1080, "three_mythics");
                foreach (var option in view.Options)
                {
                    var graphic = option.GetComponentInChildren<CardRarityParticles>(true);
                    Check(!graphic.raycastTarget, "Partículas não bloqueiam cliques");
                    Mesh mesh = graphic.canvasRenderer.GetMesh();
                    Check(mesh != null && mesh.vertexCount > 100 && mesh.vertexCount < 2000,
                        "Malha mítica limitada: " + (mesh != null ? mesh.vertexCount : 0));
                }
            }
        }
        float left = ((RectTransform)view.Options[0].transform).anchoredPosition.x;
        float right = ((RectTransform)view.Options[2].transform).anchoredPosition.x;
        float fit = view.Options[0].transform.localScale.x;
        Check(Mathf.Abs((right - left) / fit - 700f) < .1f, "Espaçamento dos centros: 350 px, adaptado à janela");
        Check(rewards.TryChoose(1), "Escolha real da mítica");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Closed, "escolha finalizada");
        Check(!view.IsVisible && Mathf.Approximately(Time.timeScale, 1f), "Fecha cartas e libera relógio");
        Check(view.TreasureVoices == 0, "Escolha/saída encerra todos os loops de tesouro");
        Check(view.Options.All(o => !o.GetComponentInChildren<CardRarityFeedback>(true).IsRevealed), "Saída limpa todos os efeitos");
        Check(waves.RequestDevelopmentCardReward(), "Nova oferta após escolha");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "nova oferta");
        Check(rewards.TryReroll(), "Reroll continua infinito após nova oferta");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "reroll final");
        // Cobertura exaustiva: as ofertas aleatórias por raridade não garantem ver as 25 artes.
        DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.AllCards;
        DevelopmentCardSession.RestartSequence();
        var visited = new HashSet<string>();
        var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCardCatalog>("Assets/Data/UpgradeCards.asset");
        for (int batch = 0; batch < 9; batch++)
        {
            Check(rewards.TryReroll(), "Ciclo completo, oferta " + batch);
            yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "oferta completa " + batch);
            for (int slot = 0; slot < view.Options.Length; slot++)
            {
                UpgradeCard card = rewards.Offers[slot];
                var face = new SerializedObject(view.Options[slot]).FindProperty("face").objectReferenceValue as UnityEngine.UI.Image;
                Check(face != null && face.sprite == catalog.Artwork(card) && view.Options[slot].FrontShowing,
                    "Arte real completa: " + card.artworkPath);
                visited.Add(card.artworkPath);
            }
            view.SetPaused(true);
            yield return Capture(1200, 720, "all_cards_" + batch);
            view.SetPaused(false);
        }
        var overlapOffers = new[] { catalog.Get(UpgradeKind.AttackDamage, UpgradeRarity.Mythic),
            catalog.Get(UpgradeKind.AttackSpeed, UpgradeRarity.Mythic), catalog.Get(UpgradeKind.MaximumHealth, UpgradeRarity.Common) };
        yield return view.Reroll(overlapOffers, catalog, Find<PlayerRunUpgrades>());
        Check(view.Options.Take(2).All(o => Mathf.Approximately(o.GetComponentInChildren<CardRarityAudio>(true).OfferGain, .5f)),
            "Duas miticas dividem o ganho de revelacao");
        Check(Mathf.Approximately(view.Options[2].GetComponentInChildren<CardRarityAudio>(true).OfferGain, 1f), "Comum preserva ganho na oferta mista");
        yield return Wait(.3f);
        Check(view.TreasureVoices == 1, "Duas miticas compartilham uma unica cintilacao");
        Check(visited.Count == 25, "Todas as 25 cartas exibidas e capturadas no fluxo real");
        DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.FixedRarity;
        DevelopmentCardSession.Rarity = UpgradeRarity.Mythic;
        Check(rewards.TryReroll(), "Oferta mítica antes do teste de limpeza");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "oferta mítica de limpeza");
        yield return Wait(.3f);
        Check(view.TreasureVoices == 1, "Três míticas compartilham um só tesouro");
        Check(view.Options.All(o => Mathf.Approximately(o.GetComponentInChildren<CardRarityAudio>(true).OfferGain, 1f / 3f)),
            "Tres miticas dividem o ganho por tres");
        DevelopmentCardSession.Rarity = UpgradeRarity.Common;
        Check(rewards.TryReroll(), "Troca de míticas para comuns");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "oferta comum de limpeza");
        Check(view.TreasureVoices == 0, "Reroll para comuns remove tesouro anterior");
        DevelopmentCardSession.Rarity = UpgradeRarity.Mythic;
        Check(rewards.TryReroll(), "Abre tesouro antes de sair para o menu");
        yield return Until(() => rewards.State == CardRewardController.RewardState.Ready, "tesouro antes do menu");
        yield return Wait(.3f);
        Check(view.TreasureVoices == 1, "Tesouro ativo antes da troca de cena");
        view.SetPaused(true);
        Check(view.Options.All(o => o.FrontShowing), "Frentes preservadas após reroll");
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainMenu.unity", new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
        yield return Until(() => !DevelopmentCardSession.IsActive, "menu inicial");
        yield return Frames(3);
        Check(!DevelopmentCardSession.IsActive && Mathf.Approximately(Time.timeScale, 1f), "Menu encerra sessão e restaura tempo");
        Check(!Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(s => s.name == "LegendaryTreasure" || s.name == "MythicTreasure"),
            "Menu remove as fontes de tesouro da cena anterior");
    }
    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Card effects validation timed out");
            while (steps.Count > 0)
            {
                IEnumerator top = steps.Peek();
                if (!top.MoveNext()) { steps.Pop(); continue; }
                if (top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            if (errors > 0) throw new InvalidOperationException("Erros durante o Play Mode: " + errors);
            Debug.Log("CARD EFFECTS PLAY VALIDATION PASSED: " + checks + " checks; errors=0");
        }
        catch (Exception error) { errors++; Debug.LogException(error); }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        running = false;
        DevelopmentCardSession.End();
        Time.timeScale = 1f; AudioListener.pause = false;
        EditorApplication.isPlaying = false;
    }
    private static void OnLog(string message, string trace, LogType type)
    {
        // Falha do indexador do Editor 6000.6 ao iniciar em lote; não pertence ao player.
        if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("CARD EFFECTS CHECK FAILED: " + message);
        checks++; Debug.Log("CARD EFFECTS CHECK: " + message);
    }
    private static T Find<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include).Single();
    private static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
    private static IEnumerator Wait(float seconds)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < end) yield return null;
    }
    private static IEnumerator Until(Func<bool> ready, string label)
    {
        double end = EditorApplication.timeSinceStartup + 15;
        while (!ready())
        {
            if (EditorApplication.timeSinceStartup > end) throw new TimeoutException(label);
            yield return null;
        }
    }
    private static IEnumerator Capture(int width, int height, string name)
    {
        Camera camera = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var texture = new RenderTexture(width, height, 24);
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
        camera.targetTexture = texture;
        yield return Frames(2);
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = texture;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(Output + "/" + name + "_" + width + "x" + height + ".png", pixels.EncodeToPNG());
        camera.targetTexture = null;
        foreach (Canvas canvas in canvases) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(pixels); texture.Release(); Object.Destroy(texture);
        yield return Frames(1);
    }
}
