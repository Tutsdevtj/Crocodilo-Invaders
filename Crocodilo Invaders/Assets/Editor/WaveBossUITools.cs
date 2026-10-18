using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Anúncio de ondas e alerta PERIGO! GOOSINI a partir do pacote Wave_Boss_UI (SourceArt/WaveBossUI).
// Install é mutação idempotente (importação, dados, WavesUI.prefab e tema do Gusini); Validate só confere.
public static class WaveBossUITools
{
    private const string WaveArt = "Assets/Art/UI/WaveBanner/";
    private const string BossArt = "Assets/Art/UI/BossAlert/";
    private const string Manifest = "SourceArt/WaveBossUI/wave_boss_ui.json";
    private const string Sync = "SourceArt/GoosiniBossAlert/sync_timestamps.json";
    private const string StylePath = "Assets/Data/UI/WaveBannerStyle.asset";
    private const string ThemePath = "Assets/Data/UI/GoosiniBossAlert.asset";
    private const string AlertClip = "Assets/Audio/Boss/BossAlert/goosini_boss_alert_final.wav";
    private const string WavesPrefab = "Assets/Prefabs/UI/WavesUI.prefab";
    private const string GusiniPrefab = "Assets/Prefabs/Boss/Gusini.prefab";
    private const string Mixer = "Assets/Resources/Audio/GameAudio.mixer";
    private const int ParticlePool = 20;
    private static readonly string[] Scenes =
        { "Assets/Scenes/Tests/PlayerMovementTest.unity", "Assets/Scenes/Tests/StageOneWaves.unity", "Assets/Scenes/Tests/GusiniBossTest.unity" };
    private static readonly string[] Lines = { "s_0", "s_1", "m_0", "m_1", "l_0", "l_1" };
    private static int checks;

    [MenuItem("Crocodilo/UI/Install wave and boss announcements")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        ConfigureSprites();
        ConfigureAudio();
        var json = Json();
        WaveBannerStyle style = BuildStyle(json);
        BossAlertTheme theme = BuildTheme(json);
        BuildPrefab(style, json);
        var boss = AssetDatabase.LoadAssetAtPath<Boss>(GusiniPrefab);
        var data = new SerializedObject(boss);
        data.FindProperty("alertTheme").objectReferenceValue = theme;
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(boss);
        AssetDatabase.SaveAssets();
        Debug.Log("WAVE BOSS UI INSTALLED");
        Validate();
    }

    [MenuItem("Crocodilo/UI/Validate wave and boss announcements")]
    public static void Validate()
    {
        checks = 0;
        var json = Json();
        foreach (string path in SpritePaths())
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Require(importer != null && importer.textureType == TextureImporterType.Sprite && importer.spritePixelsPerUnit == 50f
                && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
                && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency, "Pixel import " + path);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            Require(settings.spriteMeshType == SpriteMeshType.FullRect && settings.spriteAlignment == (int)SpriteAlignment.TopLeft, "Top-left pivot " + path);
        }
        Require(SpritePaths().Count() == 74, "74 runtime sprites (wave 41, boss common 13, Goosini 20)");
        Require(!Directory.GetFiles("Assets/Art/UI/WaveBanner", "*", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles("Assets/Art/UI/BossAlert", "*", SearchOption.AllDirectories))
            .Any(f => f.EndsWith(".gif") || f.EndsWith(".py") || f.EndsWith(".json") || f.Contains("pop2") || f.Contains("atlas") || f.Contains("full_236")),
            "Previews, tools, atlas, pop2 and the assembled band stay out of runtime art");

        var audio = (AudioImporter)AssetImporter.GetAtPath(AlertClip);
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AlertClip);
        Require(clip != null && clip.channels == 2 && clip.frequency == 48000 && Mathf.Abs(clip.length - 2f) < .001f, "Final alert WAV is 2.000 s stereo 48 kHz");
        Require(!audio.forceToMono && audio.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad
            && audio.defaultSampleSettings.compressionFormat == AudioCompressionFormat.PCM && audio.defaultSampleSettings.preloadAudioData, "Alert WAV: PCM, decompress on load, preloaded");
        Require(!Directory.GetFiles("Assets", "goosini_boss_alert_candidate*", SearchOption.AllDirectories).Any()
            && !Directory.GetFiles("Assets", "*Comparacao*", SearchOption.AllDirectories).Any(), "Candidates and comparison previews are not imported");

        var style = AssetDatabase.LoadAssetAtPath<WaveBannerStyle>(StylePath);
        Require(style != null, "Wave banner style asset");
        ValidateStyle(style, json);
        var theme = AssetDatabase.LoadAssetAtPath<BossAlertTheme>(ThemePath);
        Require(theme != null && theme.title != null && theme.titleFlash != null && theme.frontAttachment != null && theme.topAccessory != null
            && theme.trailFrames.Length == 4 && theme.trailFrames.All(s => s != null) && theme.particles.Length == 4
            && theme.particles.All(p => p.normal != null && p.dissolve1 != null && p.dissolve2 != null), "Goosini theme pieces");
        Require(theme.alertSound == clip && Mathf.Approximately(theme.alertVolume, .7f), "Goosini theme uses the final WAV at 0.70");
        var events = Obj(Json(Sync))["events"] as List<object>;
        float peak = Num(events.Cast<Dictionary<string, object>>().First(e => (string)e["event"] == "impact_peak")["time_ms"]);
        Require(Mathf.Abs(theme.alertSoundOffsetMs + peak - 200f) <= 10f, $"Audio impact peak ({theme.alertSoundOffsetMs + peak:0} ms) meets the visual slam at 200 ms (±10 ms)");
        Require(AssetDatabase.LoadAssetAtPath<Boss>(GusiniPrefab).AlertTheme == theme, "Gusini prefab owns the Goosini alert theme");

        GameObject root = PrefabUtility.LoadPrefabContents(WavesPrefab);
        try
        {
            var ui = root.GetComponent<WavesUI>();
            var announcer = root.GetComponentInChildren<WaveAnnouncer>(true);
            Require(ui != null && announcer != null && ui.Announcer == announcer, "WavesUI drives the WaveAnnouncer");
            Require(root.transform.Find("WaveBanner") == null, "Legacy TMP WaveBanner removed");
            Require(announcer.GetComponentsInChildren<TMP_Text>(true).Length == 0, "Announcements use only the approved PNGs");
            Require(new SerializedObject(ui).FindProperty("bannerText") == null && new SerializedObject(ui).FindProperty("enterDuration") == null, "Legacy banner fields removed");
            Require(root.GetComponentsInChildren<Transform>(true).All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "No missing scripts in WavesUI");
            foreach (MonoBehaviour behaviour in new MonoBehaviour[] { announcer, announcer.GetComponentInChildren<WaveBanner>(true),
                         announcer.GetComponentInChildren<BossAlertBanner>(true), announcer.GetComponentInChildren<BannerParticleLayer>(true) })
                RequireReferences(behaviour);
            foreach (Image image in announcer.GetComponentsInChildren<Image>(true))
                Require(!image.raycastTarget && image.rectTransform.pivot == new Vector2(0, 1), "Non-interactive top-left image " + image.name);
            var audioSource = announcer.GetComponentInChildren<BossAlertBanner>(true).GetComponent<AudioSource>();
            Require(audioSource != null && !audioSource.loop && !audioSource.playOnAwake && audioSource.spatialBlend == 0f
                && audioSource.outputAudioMixerGroup != null && audioSource.outputAudioMixerGroup.name == "Effects", "2D alert source routed to Effects");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        if (!Application.isBatchMode && EditorSceneManager.GetActiveScene().isDirty) Debug.LogWarning("Skipping scene checks: the open scene has unsaved changes.");
        else foreach (string path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var uis = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<WavesUI>(true)).ToArray();
                Require(uis.Length == 1 && new SerializedObject(uis[0]).FindProperty("waves").objectReferenceValue != null && uis[0].Announcer != null, "WavesUI wired in " + path);
                Require(scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                    .All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "No missing scripts in " + path);
            }
            finally { if (EditorSceneManager.sceneCount > 1) EditorSceneManager.CloseScene(scene, true); }
        }
        Debug.Log($"WAVE BOSS UI VALIDATION PASSED: {checks} checks.");
    }

    // Exemplos do manifesto (1–10, 11, 15, 99) recalculados pelas mesmas regras do runtime.
    private static void ValidateStyle(WaveBannerStyle style, Dictionary<string, object> json)
    {
        var wave = Obj(json["wave"]);
        var digits = Obj(wave["digits"]);
        for (int d = 0; d < 10; d++)
        {
            var spec = Obj(digits[d.ToString()]);
            var digit = style.digits[d];
            Require(digit.normal != null && digit.pop1 != null && digit.flash != null
                && digit.normal.name == "wave_digit_" + d && digit.pop1.name == $"wave_digit_{d}_pop1" && digit.flash.name == $"wave_digit_{d}_flash", "Digit sprites " + d);
            Require(digit.firstX == Int(spec["first_x"]) && digit.barEndOffset == Int(spec["bar_end_offset"]) && digit.burstOffset == Vec(spec["burst_offset"])
                && digit.pop1Offset == Vec(Obj(spec["variant_offset"])["pop1"]) && digit.flashOffset == Vec(Obj(spec["variant_offset"])["flash"])
                && digit.normal.rect.width == Int(spec["w"]) && digit.normal.rect.height == Int(spec["h"]), "Digit metrics " + d);
        }
        var kerning = Obj(wave["kerning_advance"]);
        Require(kerning.Count == 100 && kerning.All(p => style.kerning[(p.Key[0] - '0') * 10 + p.Key[1] - '0'] == Int(p.Value)), "Kerning table (100 pairs)");
        var values = new int[WaveBannerStyle.MaxDigits];
        var xs = new int[WaveBannerStyle.MaxDigits];
        foreach (var example in Obj(wave["examples"]))
        {
            var spec = Obj(example.Value);
            var layout = style.Compose(int.Parse(example.Key), values, xs);
            var expectedX = ((List<object>)spec["digit_x"]).Select(Int).ToArray();
            var bbox = ((List<object>)spec["bbox_x"]).Select(Int).ToArray();
            var burst = Vec(spec["burst"]);
            Require(layout.barLength == Int(spec["bar_length_L"]) && xs.Take(layout.digitCount).SequenceEqual(expectedX)
                && layout.burstX == burst.x && layout.burstY == burst.y && layout.bboxMin == bbox[0] && layout.bboxMax == bbox[1]
                && Mathf.Approximately(layout.CenterX, Num(spec["center_x"])) && layout.speedlinesX == Int(spec["speedlines_x"]),
                $"ONDA {example.Key}: L {layout.barLength}, x [{string.Join(",", xs.Take(layout.digitCount))}], centro {layout.CenterX}");
        }
        Require(Keys(Obj(wave["timing_ms"])["keyframes"]).SequenceEqual(style.motion), "Wave motion matches the approved 1050 ms timeline");
        Require(style.Duration == Int(Obj(wave["timing_ms"])["total"]), "Wave total 1050 ms");
        Require(style.barHead != null && style.barMid != null && style.barEnd != null && style.word != null && style.burst != null
            && style.speedlines.Length == 6 && style.speedlines.All(s => s != null), "Wave pieces");
        var boss = Obj(json["boss"]);
        var banner = AssetDatabase.LoadAssetAtPath<GameObject>(WavesPrefab).GetComponentInChildren<BossAlertBanner>(true);
        if (banner == null) return;
        var data = new SerializedObject(banner);
        var timing = Obj(boss["timing_ms"]);
        var motion = Enumerable.Range(0, data.FindProperty("motion").arraySize).Select(i => KeyFrom(data.FindProperty("motion").GetArrayElementAtIndex(i))).ToArray();
        Require(Keys(timing["keyframes"]).SequenceEqual(motion), "Boss motion matches the approved 1300 ms timeline");
        var steps = (List<object>)timing["alert_timeline"];
        var timeline = data.FindProperty("alertTimeline");
        Require(timeline.arraySize == steps.Count && steps.Select((s, i) => (Obj(s), timeline.GetArrayElementAtIndex(i))).All(p =>
            Num(((List<object>)p.Item1["t"])[0]) == p.Item2.FindPropertyRelative("startMs").floatValue
            && string.Equals((string)p.Item1["state"], ((BossAlertBanner.AlertState)p.Item2.FindPropertyRelative("state").enumValueIndex).ToString(), StringComparison.OrdinalIgnoreCase)),
            "PERIGO! timeline: flash → amber → red, two blinks then fixed");
        Require(Num(Obj(json["sequence_wave10_to_goosini"])["total_ms"]) == style.Duration + data.FindProperty("transitionMs").floatValue + banner.Duration,
            "ONDA 10 → feathers → GOOSINI totals 2500 ms");
    }

    private static void ConfigureSprites()
    {
        foreach (string path in SpritePaths())
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 50f; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.TopLeft;
            settings.spritePivot = new Vector2(0f, 1f);
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
    }

    private static void ConfigureAudio()
    {
        var importer = (AudioImporter)AssetImporter.GetAtPath(AlertClip);
        importer.forceToMono = false;
        importer.loadInBackground = false;
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.preloadAudioData = true;
        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();
    }

    private static WaveBannerStyle BuildStyle(Dictionary<string, object> json)
    {
        var style = LoadOrCreate<WaveBannerStyle>(StylePath);
        var wave = Obj(json["wave"]);
        var pieces = Obj(wave["pieces"]);
        var bar = Obj(pieces["bar"]);
        style.barHead = Sprite(WaveArt + "wave_bar_head.png");
        style.barMid = Sprite(WaveArt + "wave_bar_mid.png");
        style.barEnd = Sprite(WaveArt + "wave_bar_end.png");
        style.word = Sprite(WaveArt + "wave_word_onda.png");
        style.burst = Sprite(WaveArt + "wave_burst.png");
        style.speedlines = Lines.Select(l => Sprite(WaveArt + "wave_speedlines_" + l + ".png")).ToArray();
        style.barPosition = new Vector2Int(Int(bar["x"]), Int(bar["y"]));
        style.barHeadWidth = Int(bar["head_w"]);
        style.barEndBack = Int(bar["end_back"]);
        style.minBarLength = Int(bar["L_min"]);
        style.wordPosition = Vec(pieces["word_onda"]);
        style.digitsTop = Int(pieces["digits_top_y"]);
        style.screenTop = 236 - Int(wave["banner_height"]);
        var digits = Obj(wave["digits"]);
        style.digits = Enumerable.Range(0, 10).Select(d =>
        {
            var spec = Obj(digits[d.ToString()]);
            var variants = Obj(spec["variant_offset"]);
            return new WaveBannerStyle.Digit
            {
                normal = Sprite($"{WaveArt}Digits/wave_digit_{d}.png"),
                pop1 = Sprite($"{WaveArt}Digits/wave_digit_{d}_pop1.png"),
                flash = Sprite($"{WaveArt}Digits/wave_digit_{d}_flash.png"),
                firstX = Int(spec["first_x"]), barEndOffset = Int(spec["bar_end_offset"]), burstOffset = Vec(spec["burst_offset"]),
                pop1Offset = Vec(variants["pop1"]), flashOffset = Vec(variants["flash"]),
            };
        }).ToArray();
        style.kerning = new int[100];
        foreach (var pair in Obj(wave["kerning_advance"])) style.kerning[(pair.Key[0] - '0') * 10 + pair.Key[1] - '0'] = Int(pair.Value);
        style.motion = Keys(Obj(wave["timing_ms"])["keyframes"]).ToArray();
        EditorUtility.SetDirty(style);
        return style;
    }

    private static BossAlertTheme BuildTheme(Dictionary<string, object> json)
    {
        var theme = LoadOrCreate<BossAlertTheme>(ThemePath);
        var boss = Obj(json["boss"]);
        var goosini = Obj(boss["goosini"]);
        var band = Obj(Obj(boss["generic"])["band"]);
        string art = BossArt + "Goosini/";
        theme.title = Sprite(art + "goosini_title.png");
        theme.titleFlash = Sprite(art + "goosini_title_flash.png");
        theme.titlePosition = Vec(goosini["title"]);
        theme.bandLength = Int(band["length_goosini"]);
        theme.bandHead = theme.bandMid = theme.bandEnd = null;
        theme.frontAttachment = Sprite(art + "goosini_beak_tip.png");
        theme.frontPosition = Vec(goosini["beak_tip"]);
        theme.topAccessory = Sprite(art + "goosini_feather_long.png");
        theme.topPosition = Vec(goosini["feather_long"]);
        var scarf = Obj(goosini["scarf"]);
        theme.trailFrames = Enumerable.Range(0, Int(scarf["frames"])).Select(i => Sprite($"{art}goosini_scarf_{i}.png")).ToArray();
        theme.trailFrameMs = Num(scarf["frame_ms"]);
        theme.trailOffset = new Vector2Int(Int(scarf["x"]) - Int(band["x"]) - theme.bandLength, Int(scarf["y"]) - Int(band["y"]));
        theme.particles = Enumerable.Range(0, 4).Select(i => new BossAlertTheme.Particle
        {
            normal = Sprite($"{art}Feathers/feather_particle_{i}.png"),
            dissolve1 = Sprite($"{art}Feathers/feather_particle_{i}_dissolve_1.png"),
            dissolve2 = Sprite($"{art}Feathers/feather_particle_{i}_dissolve_2.png"),
        }).ToArray();
        theme.transitionParticles = true;
        theme.alertSound = AssetDatabase.LoadAssetAtPath<AudioClip>(AlertClip);
        theme.alertVolume = .7f;
        // Banner começa em 1200 ms da sequência e bate em +200; o pico grave do WAV está em 349 ms.
        // Começar em −150 (1050 ms, durante a transição) põe o pico em +199, dentro da incerteza medida (±10 ms).
        theme.alertSoundOffsetMs = -150f;
        EditorUtility.SetDirty(theme);
        return theme;
    }

    private static void BuildPrefab(WaveBannerStyle style, Dictionary<string, object> json)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(WavesPrefab);
        try
        {
            var ui = root.GetComponent<WavesUI>();
            int index = root.transform.childCount;
            foreach (string legacy in new[] { "WaveBanner", "Announcements" })
            {
                Transform old = root.transform.Find(legacy);
                if (old == null) continue;
                index = Mathf.Min(index, old.GetSiblingIndex());
                Object.DestroyImmediate(old.gameObject);
            }
            var announcements = Stretch("Announcements", root.transform);
            announcements.SetSiblingIndex(Mathf.Min(index, root.transform.childCount - 1));
            var announcer = announcements.gameObject.AddComponent<WaveAnnouncer>();

            var waveObject = Stretch("Wave", announcements);
            var wave = waveObject.gameObject.AddComponent<WaveBanner>();
            var motion = TopCenter("Motion", waveObject);
            var waveData = new SerializedObject(wave);
            waveData.FindProperty("style").objectReferenceValue = style;
            waveData.FindProperty("root").objectReferenceValue = motion;
            Set(waveData, "speedlines", Pixel("Speedlines", motion));
            Set(waveData, "barHead", Pixel("BarHead", motion));
            Set(waveData, "barMid", Pixel("BarMid", motion));
            Set(waveData, "barEnd", Pixel("BarEnd", motion));
            Set(waveData, "word", Pixel("Onda", motion));
            var digitsRoot = Corner("Digits", motion);
            waveData.FindProperty("digitsRoot").objectReferenceValue = digitsRoot;
            var digitImages = waveData.FindProperty("digitImages");
            digitImages.arraySize = 3;
            for (int i = 0; i < 3; i++) digitImages.GetArrayElementAtIndex(i).objectReferenceValue = Pixel("Digit" + i, digitsRoot);
            Set(waveData, "burst", Pixel("Burst", motion));
            waveData.ApplyModifiedPropertiesWithoutUndo();

            var bossObject = Stretch("BossAlert", announcements);
            var source = bossObject.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = false; source.spatialBlend = 0f; source.priority = 32; source.volume = .7f;
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(Mixer);
            source.outputAudioMixerGroup = mixer != null ? mixer.FindMatchingGroups("Effects").FirstOrDefault() : null;
            var banner = bossObject.gameObject.AddComponent<BossAlertBanner>();
            var bossData = new SerializedObject(banner);
            var bossSpeedlines = Free("Speedlines", bossObject);
            var particlesRect = Stretch("Particles", bossObject);
            var layer = particlesRect.gameObject.AddComponent<BannerParticleLayer>();
            var layerData = new SerializedObject(layer);
            var pool = layerData.FindProperty("pool");
            pool.arraySize = ParticlePool;
            for (int i = 0; i < ParticlePool; i++) pool.GetArrayElementAtIndex(i).objectReferenceValue = Free("Feather" + i, particlesRect);
            var particles = Obj(Obj(Obj(json["boss"])["timing_ms"])["particles"]);
            layerData.FindProperty("lifeMs").floatValue = Num(particles["life_ms"]);
            layerData.FindProperty("frameMs").floatValue = Num(particles["frame_ms"]);
            layerData.FindProperty("dissolve1At").floatValue = Num(particles["dissolve_1_at"]);
            layerData.FindProperty("dissolve2At").floatValue = Num(particles["dissolve_2_at"]);
            var drift = (List<object>)particles["drift_px_per_ms"];
            layerData.FindProperty("driftPerMs").vector2Value = new Vector2(Num(drift[0]), Num(drift[1]));
            layerData.ApplyModifiedPropertiesWithoutUndo();
            var transition = bossData.FindProperty("transitionFeathers");
            transition.arraySize = 4;
            for (int i = 0; i < 4; i++) transition.GetArrayElementAtIndex(i).objectReferenceValue = Free("Transition" + i, particlesRect);
            var bannerRoot = TopCenter("Banner", bossObject);
            var boss = Obj(json["boss"]);
            var generic = Obj(boss["generic"]);
            var canvas = ((List<object>)boss["canvas"]).Select(Int).ToArray();
            bannerRoot.sizeDelta = new Vector2(canvas[0], canvas[1]) * 2f;
            bossData.FindProperty("root").objectReferenceValue = bannerRoot;
            Set(bossData, "speedlines", bossSpeedlines);
            Set(bossData, "trail", Pixel("Trail", bannerRoot));
            Set(bossData, "bandHead", Pixel("BandHead", bannerRoot));
            Set(bossData, "bandMid", Pixel("BandMid", bannerRoot));
            Set(bossData, "bandEnd", Pixel("BandEnd", bannerRoot));
            Set(bossData, "front", Pixel("Front", bannerRoot));
            Set(bossData, "top", Pixel("Top", bannerRoot));
            Set(bossData, "title", Pixel("Title", bannerRoot));
            Set(bossData, "alert", Pixel("Alert", bannerRoot));
            Set(bossData, "impact", Pixel("Impact", bannerRoot));
            bossData.FindProperty("particles").objectReferenceValue = layer;
            bossData.FindProperty("alertAudio").objectReferenceValue = source;
            string common = BossArt + "Common/";
            bossData.FindProperty("defaultBandHead").objectReferenceValue = Sprite(common + "boss_band_head.png");
            bossData.FindProperty("defaultBandMid").objectReferenceValue = Sprite(common + "boss_band_mid.png");
            bossData.FindProperty("defaultBandEnd").objectReferenceValue = Sprite(common + "boss_band_end.png");
            bossData.FindProperty("alertRed").objectReferenceValue = Sprite(common + "boss_alert_perigo_red.png");
            bossData.FindProperty("alertAmber").objectReferenceValue = Sprite(common + "boss_alert_perigo_amber.png");
            bossData.FindProperty("alertFlash").objectReferenceValue = Sprite(common + "boss_alert_perigo_flash.png");
            bossData.FindProperty("impactBurst").objectReferenceValue = Sprite(common + "boss_impact_burst.png");
            var frames = bossData.FindProperty("speedlineFrames");
            frames.arraySize = 6;
            for (int i = 0; i < 6; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = Sprite(common + "boss_speedlines_" + Lines[i] + ".png");
            var bandSpec = Obj(generic["band"]);
            bossData.FindProperty("canvasSize").vector2IntValue = new Vector2Int(canvas[0], canvas[1]);
            bossData.FindProperty("bandPosition").vector2IntValue = new Vector2Int(Int(bandSpec["x"]), Int(bandSpec["y"]));
            bossData.FindProperty("bandHeadWidth").intValue = Int(bandSpec["head_w"]);
            bossData.FindProperty("bandEndWidth").intValue = Int(bandSpec["end_w"]);
            bossData.FindProperty("alertPosition").vector2IntValue = Vec(generic["alert"]);
            bossData.FindProperty("impactPosition").vector2IntValue = Vec(generic["impact_burst"]);
            bossData.FindProperty("speedlinesPosition").vector2IntValue = Vec(generic["speedlines"]);
            var timing = Obj(boss["timing_ms"]);
            bossData.FindProperty("screenCenterY").floatValue = Num(timing["screen_center_y"]);
            var keys = Keys(timing["keyframes"]).ToArray();
            var motionProperty = bossData.FindProperty("motion");
            motionProperty.arraySize = keys.Length;
            for (int i = 0; i < keys.Length; i++) KeyTo(motionProperty.GetArrayElementAtIndex(i), keys[i]);
            var steps = (List<object>)timing["alert_timeline"];
            var timeline = bossData.FindProperty("alertTimeline");
            timeline.arraySize = steps.Count;
            for (int i = 0; i < steps.Count; i++)
            {
                var step = Obj(steps[i]);
                timeline.GetArrayElementAtIndex(i).FindPropertyRelative("startMs").floatValue = Num(((List<object>)step["t"])[0]);
                timeline.GetArrayElementAtIndex(i).FindPropertyRelative("state").enumValueIndex =
                    (int)Enum.Parse(typeof(BossAlertBanner.AlertState), (string)step["state"], true);
            }
            var sequence = (List<object>)Obj(json["sequence_wave10_to_goosini"])["steps"];
            bossData.FindProperty("transitionMs").floatValue = Num(Obj(sequence[2])["t"]) - Num(Obj(sequence[1])["t"]);
            bossData.ApplyModifiedPropertiesWithoutUndo();

            var announcerData = new SerializedObject(announcer);
            announcerData.FindProperty("waveBanner").objectReferenceValue = wave;
            announcerData.FindProperty("bossBanner").objectReferenceValue = banner;
            announcerData.ApplyModifiedPropertiesWithoutUndo();
            var uiData = new SerializedObject(ui);
            uiData.FindProperty("announcer").objectReferenceValue = announcer;
            uiData.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, WavesPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static IEnumerable<string> SpritePaths() =>
        Directory.GetFiles(WaveArt, "*.png", SearchOption.AllDirectories).Concat(Directory.GetFiles(BossArt, "*.png", SearchOption.AllDirectories))
            .Select(p => p.Replace('\\', '/')).OrderBy(p => p, StringComparer.Ordinal);

    private static IEnumerable<BannerKeyframe> Keys(object keyframes) => ((List<object>)keyframes).Select(Obj).Select(k => new BannerKeyframe(
        Num(k["t"]), Num(k["to_t"]), Num(k["x"]), Num(k["to_x"]),
        (string)k["ease"] == "out_cubic" ? BannerKeyframe.Ease.OutCubic : (string)k["ease"] == "in_cubic" ? BannerKeyframe.Ease.InCubic : BannerKeyframe.Ease.Linear));

    private static BannerKeyframe KeyFrom(SerializedProperty p) => new BannerKeyframe(p.FindPropertyRelative("startMs").floatValue, p.FindPropertyRelative("endMs").floatValue,
        p.FindPropertyRelative("fromX").floatValue, p.FindPropertyRelative("toX").floatValue, (BannerKeyframe.Ease)p.FindPropertyRelative("ease").enumValueIndex);

    private static void KeyTo(SerializedProperty p, BannerKeyframe key)
    {
        p.FindPropertyRelative("startMs").floatValue = key.startMs; p.FindPropertyRelative("endMs").floatValue = key.endMs;
        p.FindPropertyRelative("fromX").floatValue = key.fromX; p.FindPropertyRelative("toX").floatValue = key.toX;
        p.FindPropertyRelative("ease").enumValueIndex = (int)key.ease;
    }

    private static RectTransform Stretch(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    // Âncora no centro superior do Canvas: X_banner = 600 + x mesmo quando a proporção da tela muda.
    private static RectTransform TopCenter(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f); rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero; rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private static RectTransform Corner(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero; rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private static Image Pixel(string name, Transform parent)
    {
        var rect = Corner(name, parent);
        rect.gameObject.AddComponent<CanvasRenderer>();
        var image = rect.gameObject.AddComponent<Image>();
        image.raycastTarget = false; image.maskable = false; image.enabled = false;
        return image;
    }

    // Peça solta (partícula, speedline do boss): posicionada em px de tela a partir do centro superior.
    private static Image Free(string name, Transform parent)
    {
        var image = Pixel(name, parent);
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(.5f, 1f);
        return image;
    }

    private static void Set(SerializedObject data, string field, Object value) => data.FindProperty(field).objectReferenceValue = value;

    private static void RequireReferences(MonoBehaviour behaviour)
    {
        Require(behaviour != null, "Announcement component present");
        var property = new SerializedObject(behaviour).GetIterator();
        while (property.NextVisible(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.name != "m_Script")
                Require(property.objectReferenceValue != null, $"{behaviour.GetType().Name}.{property.propertyPath} assigned");
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static Sprite Sprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Missing sprite " + path);
        return sprite;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("WAVE BOSS UI CHECK FAILED: " + message);
        checks++;
    }

    private static Dictionary<string, object> Json(string path = Manifest) => Obj(MiniJson.Parse(File.ReadAllText(path, Encoding.UTF8)));
    private static Dictionary<string, object> Obj(object value) => (Dictionary<string, object>)value;
    private static float Num(object value) => (float)(double)value;
    private static int Int(object value) => (int)Math.Round((double)value);
    private static Vector2Int Vec(object value)
    {
        if (value is Dictionary<string, object> map) return new Vector2Int(Int(map["x"]), Int(map["y"]));
        var list = (List<object>)value;
        return new Vector2Int(Int(list[0]), Int(list[1]));
    }

    // Leitor JSON mínimo: o manifesto usa objetos com chaves numéricas ("0".."9", "00".."99").
    private static class MiniJson
    {
        public static object Parse(string text) { int i = 0; object value = Value(text, ref i); return value; }

        private static object Value(string s, ref int i)
        {
            Skip(s, ref i);
            char c = s[i];
            if (c == '{')
            {
                var map = new Dictionary<string, object>(); i++;
                for (Skip(s, ref i); s[i] != '}'; Skip(s, ref i))
                {
                    string key = (string)Value(s, ref i); Skip(s, ref i); i++;
                    map[key] = Value(s, ref i); Skip(s, ref i);
                    if (s[i] == ',') i++;
                }
                i++; return map;
            }
            if (c == '[')
            {
                var list = new List<object>(); i++;
                for (Skip(s, ref i); s[i] != ']'; Skip(s, ref i))
                {
                    list.Add(Value(s, ref i)); Skip(s, ref i);
                    if (s[i] == ',') i++;
                }
                i++; return list;
            }
            if (c == '"')
            {
                var text = new StringBuilder(); i++;
                while (s[i] != '"')
                {
                    if (s[i] == '\\')
                    {
                        char e = s[++i];
                        if (e == 'u') { text.Append((char)Convert.ToInt32(s.Substring(i + 1, 4), 16)); i += 4; }
                        else text.Append(e == 'n' ? '\n' : e == 't' ? '\t' : e == 'r' ? '\r' : e == 'b' ? '\b' : e == 'f' ? '\f' : e);
                    }
                    else text.Append(s[i]);
                    i++;
                }
                i++; return text.ToString();
            }
            if (s.Substring(i).StartsWith("true")) { i += 4; return true; }
            if (s.Substring(i).StartsWith("false")) { i += 5; return false; }
            if (s.Substring(i).StartsWith("null")) { i += 4; return null; }
            int start = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
        }

        private static void Skip(string s, ref int i) { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }
    }
}
