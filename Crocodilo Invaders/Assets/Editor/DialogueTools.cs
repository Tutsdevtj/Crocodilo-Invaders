using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Instala diálogos, retratos, música do Gusini e liga as cenas narrativas sem editar YAML à mão.
public static class DialogueTools
{
    private const string UiArt = "Assets/Art/UI/Dialogue/";
    private const string PortraitArt = "Assets/Art/Portraits/";
    private const string FontSource = "Assets/Fonts/PixelifySans/PixelifySans-VariableFont_wght.ttf";
    public const string FontAsset = "Assets/Fonts/PixelifySans/PixelifySans Dialogue.asset";
    private const string NameFont = "Assets/Fonts/PressStart2P/PressStart2P SDF.asset";
    private const string DataFolder = "Assets/Data/Dialogue/";
    public const string CastAsset = DataFolder + "DialogueCast.asset";
    public const string RunIntroAsset = DataFolder + "RunIntro.asset";
    public const string BossIntroAsset = DataFolder + "GusiniBossIntro.asset";
    public const string TransitionAsset = DataFolder + "GusiniPhaseTransition.asset";
    public const string FinaleAsset = DataFolder + "GusiniFinale.asset";
    public const string ChapterAsset = "Assets/Data/Flow/Chapter1.asset";
    public const string Prefab = "Assets/Prefabs/UI/Cinematics.prefab";
    private const string BossPrefab = "Assets/Prefabs/Boss/Gusini.prefab";
    private const string Blip = "Assets/Audio/Dialogue/SFX_Dialogo_TextBlip_05.wav";
    private const string MusicOne = "Assets/Audio/Music/MUS_Gusini_Phase1_Loop.wav";
    private const string MusicTwo = "Assets/Audio/Music/MUS_Gusini_Phase2_Loop.wav";
    public static readonly string[] Scenes = { "Assets/Scenes/Tests/PlayerMovementTest.unity", "Assets/Scenes/Tests/GusiniBossTest.unity" };
    private const string Characters = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        + "ÀÁÂÃÄÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜàáâãäçèéêëìíîïñòóôõöùúûüªº°¡¿«»·…—–“”‘’";
    private static readonly Color Cream = new Color32(246, 236, 204, 255);

    private struct UiSprite { public string name; public Vector4 border; public Vector2 pivot; }
    private static readonly UiSprite[] UiSprites =
    {
        new UiSprite { name = "dialogue_panel_backplate", border = new Vector4(14, 12, 14, 12), pivot = new Vector2(.5f, 0f) },
        new UiSprite { name = "dialogue_panel_frame", border = new Vector4(14, 12, 14, 12), pivot = new Vector2(.5f, 0f) },
        new UiSprite { name = "dialogue_highlight", border = new Vector4(14, 12, 14, 12), pivot = new Vector2(.5f, 0f) },
        new UiSprite { name = "dialogue_nameplate", border = new Vector4(10, 0, 7, 0), pivot = new Vector2(0f, .5f) },
        new UiSprite { name = "dialogue_nameplate_accent", border = new Vector4(10, 0, 7, 0), pivot = new Vector2(0f, .5f) },
        new UiSprite { name = "dialogue_radio_idle", pivot = new Vector2(1f, 0f) },
        new UiSprite { name = "dialogue_radio_active_0", pivot = new Vector2(1f, 0f) },
        new UiSprite { name = "dialogue_radio_active_1", pivot = new Vector2(1f, 0f) },
        new UiSprite { name = "dialogue_radio_active_2", pivot = new Vector2(1f, 0f) },
        new UiSprite { name = "dialogue_continue_0", pivot = new Vector2(.5f, .5f) },
        new UiSprite { name = "dialogue_continue_1", pivot = new Vector2(.5f, .5f) },
        new UiSprite { name = "dialogue_continue_2", pivot = new Vector2(.5f, .5f) },
    };
    private static readonly (string speaker, string id, string file)[] Portraits =
    {
        ("crocodilo", "neutral", "Crocodilo/crocodilo_master_neutral.png"),
        ("crocodilo", "alert", "Crocodilo/crocodilo_01_surpreso_indignado.png"),
        ("crocodilo", "serious", "Crocodilo/crocodilo_02_serio_autoritario.png"),
        ("crocodilo", "worried", "Crocodilo/crocodilo_03_confuso_preocupado.png"),
        ("crocodilo", "sincere", "Crocodilo/crocodilo_04_sincero_magoado.png"),
        ("crocodilo", "determined", "Crocodilo/crocodilo_05_determinado.png"),
        ("gusini", "smug", "GusiniPhase1/gusini_p1_smug.png"),
        ("gusini", "resentful", "GusiniPhase1/gusini_p1_resentful.png"),
        ("gusini", "power", "GusiniPhase1/gusini_p1_power.png"),
        ("gusini", "vulnerable", "GusiniPhase1/gusini_p1_vulnerable.png"),
        ("gusini", "hostile", "GusiniPhase1/gusini_p1_hostile.png"),
        ("gusini_corrupted", "corrupted_furious", "GusiniPhase2/gusini_p2_corrupted_furious.png"),
    };

    [MenuItem("Crocodilo/Dialogue/Install cinematics")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        ConfigureImports();
        CreateFont();
        GusiniDefeatTools.Install();
        CreateData();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null) BuildPrefab();
        ApplyLayout();
        EnsureChapterTransition();
        InstallMusic();
        foreach (string scene in Scenes) InstallScene(scene, scene.EndsWith("PlayerMovementTest.unity", StringComparison.Ordinal));
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void ConfigureImports()
    {
        foreach (UiSprite spec in UiSprites) ConfigureSprite(UiArt + spec.name + ".png", 50, spec.pivot, spec.border);
        foreach (var portrait in Portraits) ConfigureSprite(PortraitArt + portrait.file, 100, new Vector2(.5f, 0f), Vector4.zero);
        var blip = (AudioImporter)AssetImporter.GetAtPath(Blip);
        var blipSettings = blip.defaultSampleSettings;
        blipSettings.loadType = AudioClipLoadType.DecompressOnLoad;
        blipSettings.compressionFormat = AudioCompressionFormat.PCM;
        blipSettings.preloadAudioData = true;
        blip.defaultSampleSettings = blipSettings;
        blip.forceToMono = true;
        blip.SaveAndReimport();
        foreach (string path in new[] { MusicOne, MusicTwo })
        {
            var music = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = music.defaultSampleSettings;
            // Faixas longas: streaming Vorbis, sem reter dezenas de MB decodificados.
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = .7f;
            settings.preloadAudioData = false;
            music.defaultSampleSettings = settings;
            music.loadInBackground = true;
            music.forceToMono = false;
            music.SaveAndReimport();
        }
    }

    private static void ConfigureSprite(string path, float pixelsPerUnit, Vector2 pivot, Vector4 border)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new FileNotFoundException(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 512;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        settings.spriteBorder = border;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static void CreateFont()
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAsset) != null) return;
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontSource);
        // 22 px = 2 px por pixel da fonte (grade de 1/11 em). Raster hinted, sem SDF borrado.
        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, 22, 2, GlyphRenderMode.RASTER_HINTED, 512, 512, AtlasPopulationMode.Dynamic, false);
        asset.name = "PixelifySans Dialogue";
        AssetDatabase.CreateAsset(asset, FontAsset);
        asset.atlasTexture.name = "PixelifySans Dialogue Atlas";
        asset.material.name = "PixelifySans Dialogue Material";
        AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        if (!asset.TryAddCharacters(Characters, out string missing)) Debug.LogWarning("Pixelify sem glifos: " + missing);
        asset.atlasTexture.filterMode = FilterMode.Point;
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(asset.atlasTexture);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    // Cria somente o que falta: falas e expressões editadas depois no Inspector são preservadas.
    private static void CreateData()
    {
        Directory.CreateDirectory(DataFolder);
        AssetDatabase.Refresh();
        var cast = AssetDatabase.LoadAssetAtPath<DialogueCast>(CastAsset);
        if (cast == null)
        {
            cast = ScriptableObject.CreateInstance<DialogueCast>();
            AssetDatabase.CreateAsset(cast, CastAsset);
            var fields = new SerializedObject(cast);
            var speakers = fields.FindProperty("speakers");
            speakers.arraySize = 3;
            Speaker(speakers.GetArrayElementAtIndex(0), "crocodilo", "B. Crocodilo", new Color32(150, 190, 90, 255), DialogueCast.Side.Left, .92f);
            Speaker(speakers.GetArrayElementAtIndex(1), "gusini", "Gusini", new Color32(226, 70, 60, 255), DialogueCast.Side.Right, 1.08f);
            Speaker(speakers.GetArrayElementAtIndex(2), "gusini_corrupted", "Gusini Corrompido", new Color32(124, 236, 72, 255), DialogueCast.Side.Right, .86f);
            fields.ApplyModifiedPropertiesWithoutUndo();
        }
        AudioClip pulse = PhaseTwoClip(GusiniSound.P2FirstPulse);
        Sequence(RunIntroAsset, cast, .9f, .25f, new[]
        {
            L("crocodilo", "alert", "Central, aqui é Croc. Tem aeronave nossa caindo por todo lado. Que diabos tá acontecendo?!", .7f, DialogueSequence.PauseStyle.RadioStatic),
            L("crocodilo", "worried", "Central...? Tá me ouvindo?", 1.1f, DialogueSequence.PauseStyle.RadioStatic),
            L("crocodilo", "determined", "Merda... Quem ainda estiver voando, aguenta firme. Tô chegando.", .8f),
        });
        Sequence(BossIntroAsset, cast, .5f, .4f, new[]
        {
            L("crocodilo", "alert", "Gusini...? Aqueles aviões eram dos nossos."),
            L("gusini", "smug", "Olha só. A estrela do esquadrão resolveu aparecer."),
            L("crocodilo", "serious", "Baixa esse canhão."),
            L("gusini", "resentful", "Sempre dando ordens. Como naquela tempestade."),
            L("crocodilo", "serious", "Eu voltei pra te buscar.", .25f),
            L("gusini", "resentful", "Eu sei. Todo mundo fez questão de lembrar."),
            L("crocodilo", "sincere", "Eu fiz aquilo porque você era meu amigo.", .2f),
            L("gusini", "power", "E agora eu não preciso mais ser salvo."),
            L("crocodilo", "worried", "Se é você mesmo aí dentro, para com isso.", .3f),
            L("gusini", "hostile", "Então vem me parar, Croc."),
        });
        var outburst = L("gusini", "hostile", "NÃO PRECISO DE VOCÊ!", .45f);
        outburst.effect = DialogueSequence.LineEffect.Interference;
        outburst.glimpseSpeakerId = "gusini_corrupted";
        outburst.glimpseExpressionId = "corrupted_furious";
        outburst.startSound = pulse;
        outburst.startSoundVolume = .55f;
        Sequence(TransitionAsset, cast, .9f, .15f, new[]
        {
            L("gusini", "smug", "Heh... vai me buscar de novo?"),
            L("crocodilo", "sincere", "Se precisar, quantas vezes for."),
            L("gusini", "vulnerable", "Eu... achei que estava no controle."),
            L("crocodilo", "serious", "Então larga isso. Vem comigo."),
            L("gusini", "vulnerable", "Croc... ele não me deixa...", .2f),
            outburst,
        });
        EnsureRadioSpeaker(cast);
        // Encerramento do capítulo 1: Cena A (Gusini voltou), B (Central, beat "radio") e C (novo começo, beat "farewell").
        var radioOpen = L("radio", "", "Atenção... atenção a todas as aeronaves sobreviventes. Aqui é a Central de Emergência.", 1f, DialogueSequence.PauseStyle.RadioStatic);
        radioOpen.effect = DialogueSequence.LineEffect.Interference;
        radioOpen.beat = "radio";
        var farewell = L("gusini", "vulnerable", "Croc... depois do que eu fiz, será que vão me deixar entrar lá?", .9f, DialogueSequence.PauseStyle.RadioStatic);
        farewell.beat = "farewell";
        Sequence(FinaleAsset, cast, 1.3f, .6f, new[]
        {
            L("crocodilo", "worried", "Gusini?! Gus, me responde!"),
            L("gusini", "vulnerable", "Agh... Croc? Que merda aconteceu com a minha nave?", .7f),
            L("crocodilo", "neutral", "A parte boa é que tu voltou. A ruim é que tua nave não pode dizer o mesmo."),
            L("gusini", "vulnerable", "Eu... ataquei os nossos, né?"),
            L("crocodilo", "serious", "Atacou. Mas aquele bicho tava na tua cabeça."),
            L("gusini", "smug", "Heh... tu me salvou de novo."),
            L("crocodilo", "neutral", "Não acostuma."),
            L("gusini", "vulnerable", "...Obrigado, irmão.", .3f),
            radioOpen,
            L("crocodilo", "alert", "Central?! Crocodilo na escuta. Tem mais alguém aí?"),
            L("radio", "", "A base principal foi destruída. Perdemos a torre de controle e grande parte da frota."),
            L("crocodilo", "worried", "Quantos conseguiram escapar?"),
            L("radio", "", "Ainda estamos contando. Os sobreviventes estão se reunindo numa pista improvisada."),
            L("radio", "", "Transmitindo coordenadas do novo ponto de encontro. Precisamos de toda ajuda disponível."),
            L("crocodilo", "determined", "Coordenadas recebidas. Estamos indo."),
            farewell,
            L("crocodilo", "neutral", "Como piloto? Não sei. Como mecânico, talvez."),
            L("gusini", "resentful", "Talvez?! Eu sou o melhor mecânico desse esquadrão!"),
            L("crocodilo", "neutral", "Então prova. Essa lata tua tá caindo aos pedaços."),
            L("gusini", "smug", "Heh... tá bom, parceiro. Vamos pra casa."),
        });
        Directory.CreateDirectory(Path.GetDirectoryName(ChapterAsset));
        AssetDatabase.Refresh();
        if (AssetDatabase.LoadAssetAtPath<ChapterDefinition>(ChapterAsset) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ChapterDefinition>(), ChapterAsset);
    }

    // RÁDIO: transmissões sem retrato, reutilizáveis em outras missões. Acrescentado a elencos já existentes.
    private static void EnsureRadioSpeaker(DialogueCast cast)
    {
        var fields = new SerializedObject(cast);
        var speakers = fields.FindProperty("speakers");
        if (cast.Find("radio") == null)
        {
            speakers.arraySize++;
            Speaker(speakers.GetArrayElementAtIndex(speakers.arraySize - 1), "radio", "RÁDIO", new Color32(200, 200, 200, 255), DialogueCast.Side.None, .82f);
        }
        // Press Start 2P desenha o acento do Á com 1 px; a Pixelify mantém "RÁDIO" legível na placa.
        for (int i = 0; i < speakers.arraySize; i++)
        {
            var speaker = speakers.GetArrayElementAtIndex(i);
            if (speaker.FindPropertyRelative("id").stringValue != "radio" || speaker.FindPropertyRelative("nameFont").objectReferenceValue != null) continue;
            speaker.FindPropertyRelative("nameFont").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAsset);
            speaker.FindPropertyRelative("nameFontSize").floatValue = 22f;
        }
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Speaker(SerializedProperty speaker, string id, string name, Color accent, DialogueCast.Side side, float pitch)
    {
        speaker.FindPropertyRelative("id").stringValue = id;
        speaker.FindPropertyRelative("displayName").stringValue = name;
        speaker.FindPropertyRelative("accent").colorValue = accent;
        speaker.FindPropertyRelative("side").enumValueIndex = (int)side;
        speaker.FindPropertyRelative("flipX").boolValue = false;
        speaker.FindPropertyRelative("typingPitch").floatValue = pitch;
        var expressions = speaker.FindPropertyRelative("expressions");
        var owned = Portraits.Where(p => p.speaker == id).ToArray();
        expressions.arraySize = owned.Length;
        for (int i = 0; i < owned.Length; i++)
        {
            var expression = expressions.GetArrayElementAtIndex(i);
            expression.FindPropertyRelative("id").stringValue = owned[i].id;
            expression.FindPropertyRelative("portrait").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitArt + owned[i].file);
        }
    }

    private static DialogueSequence.Line L(string speaker, string expression, string text, float pauseBefore = 0f,
        DialogueSequence.PauseStyle style = DialogueSequence.PauseStyle.Silent)
        => new DialogueSequence.Line { speakerId = speaker, expressionId = expression, text = text, pauseBefore = pauseBefore, pauseStyle = style };

    private static void Sequence(string path, DialogueCast cast, float openDelay, float closeDelay, DialogueSequence.Line[] lines)
    {
        if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null) return;
        var sequence = ScriptableObject.CreateInstance<DialogueSequence>();
        AssetDatabase.CreateAsset(sequence, path);
        var fields = new SerializedObject(sequence);
        fields.FindProperty("cast").objectReferenceValue = cast;
        fields.FindProperty("openDelay").floatValue = openDelay;
        fields.FindProperty("closeDelay").floatValue = closeDelay;
        var array = fields.FindProperty("lines");
        array.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = array.GetArrayElementAtIndex(i);
            DialogueSequence.Line source = lines[i];
            line.FindPropertyRelative("speakerId").stringValue = source.speakerId;
            line.FindPropertyRelative("speakerName").stringValue = string.Empty;
            line.FindPropertyRelative("text").stringValue = source.text;
            line.FindPropertyRelative("expressionId").stringValue = source.expressionId;
            line.FindPropertyRelative("pauseBefore").floatValue = source.pauseBefore;
            line.FindPropertyRelative("pauseStyle").enumValueIndex = (int)source.pauseStyle;
            line.FindPropertyRelative("pauseAfter").floatValue = source.pauseAfter;
            line.FindPropertyRelative("soundOverride").objectReferenceValue = source.soundOverride;
            line.FindPropertyRelative("startSound").objectReferenceValue = source.startSound;
            line.FindPropertyRelative("startSoundVolume").floatValue = source.startSoundVolume;
            line.FindPropertyRelative("effect").enumValueIndex = (int)source.effect;
            line.FindPropertyRelative("glimpseSpeakerId").stringValue = source.glimpseSpeakerId ?? string.Empty;
            line.FindPropertyRelative("glimpseExpressionId").stringValue = source.glimpseExpressionId ?? string.Empty;
            line.FindPropertyRelative("beat").stringValue = source.beat ?? string.Empty;
        }
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AudioClip PhaseTwoClip(GusiniSound id)
    {
        var config = AssetDatabase.LoadAssetAtPath<GusiniAudioConfig>("Assets/Data/GusiniPhaseTwoAudio.asset");
        var sound = config != null ? config.Sounds.FirstOrDefault(s => s.id == id) : null;
        return sound?.variants != null && sound.variants.Length > 0 ? sound.variants[0] : null;
    }

    private static Sprite Ui(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(UiArt + name + ".png");

    private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = 5;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        return rect;
    }

    private static RectTransform Stretch(string name, Transform parent, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
        rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        return rect;
    }

    private static Image Image(RectTransform rect, Sprite sprite, bool sliced)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI Text(RectTransform rect, TMP_FontAsset font, float size, TextAlignmentOptions alignment, TextWrappingModes wrapping)
    {
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = Cream;
        text.alignment = alignment;
        text.textWrappingMode = wrapping;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.richText = true;
        text.fontFeatures = new List<OTL_FeatureTag>();
        return text;
    }

    private static void BuildPrefab()
    {
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAsset);
        var names = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NameFont);
        var root = new GameObject("Cinematics", typeof(RectTransform));
        try
        {
            root.layer = 5;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Acima de HUD (100) e cartas (200); abaixo da pausa (300) e das opções (400).
            canvas.sortingOrder = 250;
            canvas.pixelPerfect = false;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1200f, 720f);
            scaler.matchWidthOrHeight = .5f;
            scaler.referencePixelsPerUnit = 100f;
            root.AddComponent<GraphicRaycaster>();

            RectTransform content = Stretch("Content", root.transform, Vector2.zero, Vector2.zero);
            var group = content.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            // Bloqueia cliques na UI de gameplay; nenhum botão por baixo recebe o clique da fala.
            var blocker = Image(Stretch("InputBlocker", content, Vector2.zero, Vector2.zero), null, false);
            blocker.color = new Color(0f, 0f, 0f, .12f);
            blocker.raycastTarget = true;
            Image left = Image(Rect("PortraitLeft", content, Vector2.zero, Vector2.zero, new Vector2(.5f, 0f), new Vector2(160f, 0f), new Vector2(320f, 320f)), null, false);
            Image right = Image(Rect("PortraitRight", content, Vector2.right, Vector2.right, new Vector2(.5f, 0f), new Vector2(-160f, 0f), new Vector2(320f, 320f)), null, false);
            left.preserveAspect = right.preserveAspect = true;

            RectTransform ui = Rect("DialogueUI", content, new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, 32f), new Vector2(720f, 140f));
            RectTransform panel = Stretch("Panel", ui, Vector2.zero, Vector2.zero);
            Image(Stretch("Backplate", panel, Vector2.zero, Vector2.zero), Ui("dialogue_panel_backplate"), true);
            Image(Stretch("Frame", panel, Vector2.zero, Vector2.zero), Ui("dialogue_panel_frame"), true);
            Image highlight = Image(Stretch("Highlight", panel, Vector2.zero, Vector2.zero), Ui("dialogue_highlight"), true);
            highlight.color = new Color(1f, 1f, 1f, 0f);
            var bodyText = Text(Stretch("Body", panel, new Vector2(30f, 24f), new Vector2(-30f, -26f)), body, 22f, TextAlignmentOptions.TopLeft, TextWrappingModes.Normal);
            bodyText.lineSpacing = 16f;
            bodyText.text = string.Empty;
            Image continueIcon = Image(Rect("ContinueIcon", panel, Vector2.right, Vector2.right, Vector2.right, new Vector2(-22f, 18f), new Vector2(22f, 20f)), Ui("dialogue_continue_0"), false);
            continueIcon.enabled = false;

            RectTransform plate = Rect("Nameplate", ui, Vector2.up, Vector2.up, new Vector2(0f, .5f), new Vector2(24f, 2f), new Vector2(160f, 30f));
            Image(Stretch("Plate", plate, Vector2.zero, Vector2.zero), Ui("dialogue_nameplate"), true);
            Image accent = Image(Stretch("Accent", plate, Vector2.zero, Vector2.zero), Ui("dialogue_nameplate_accent"), true);
            var nameText = Text(Stretch("NameText", plate, new Vector2(22f, 6f), new Vector2(-20f, -8f)), names, 16f, TextAlignmentOptions.TopLeft, TextWrappingModes.NoWrap);
            nameText.text = string.Empty;
            Image radio = Image(Rect("Radio", ui, Vector2.one, Vector2.one, Vector2.right, new Vector2(-24f, -16f), new Vector2(68f, 34f)), Ui("dialogue_radio_idle"), false);

            // Topo central: livre de retratos, painel, HUDs e contador de onda.
            var skip = Text(Rect("SkipHint", content, new Vector2(.5f, 1f), new Vector2(.5f, 1f), new Vector2(.5f, 1f), new Vector2(0f, -16f), new Vector2(480f, 20f)),
                names, 16f, TextAlignmentOptions.Top, TextWrappingModes.NoWrap);
            skip.text = "<mark=#12161CC8> SEGURE TAB: PULAR </mark>";
            skip.alpha = .7f;
            content.gameObject.SetActive(false);

            var blips = root.AddComponent<DialogueBlipAudio>();
            var blipFields = new SerializedObject(blips);
            blipFields.FindProperty("blip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Blip);
            blipFields.ApplyModifiedPropertiesWithoutUndo();
            var runner = root.AddComponent<DialogueRunner>();
            var fields = new SerializedObject(runner);
            fields.FindProperty("content").objectReferenceValue = content.gameObject;
            fields.FindProperty("group").objectReferenceValue = group;
            fields.FindProperty("panelMotion").objectReferenceValue = ui;
            fields.FindProperty("body").objectReferenceValue = bodyText;
            fields.FindProperty("nameplate").objectReferenceValue = plate;
            fields.FindProperty("nameplateAccent").objectReferenceValue = accent;
            fields.FindProperty("nameText").objectReferenceValue = nameText;
            fields.FindProperty("radio").objectReferenceValue = radio;
            fields.FindProperty("continueIcon").objectReferenceValue = continueIcon;
            fields.FindProperty("highlight").objectReferenceValue = highlight;
            fields.FindProperty("leftPortrait").objectReferenceValue = left;
            fields.FindProperty("rightPortrait").objectReferenceValue = right;
            fields.FindProperty("skipHint").objectReferenceValue = skip;
            fields.FindProperty("radioIdle").objectReferenceValue = Ui("dialogue_radio_idle");
            SetArray(fields.FindProperty("radioActive"), Ui("dialogue_radio_active_0"), Ui("dialogue_radio_active_1"), Ui("dialogue_radio_active_2"));
            SetArray(fields.FindProperty("continueFrames"), Ui("dialogue_continue_0"), Ui("dialogue_continue_1"), Ui("dialogue_continue_2"));
            fields.ApplyModifiedPropertiesWithoutUndo();
            var cutscenes = root.AddComponent<RunCutscenes>();
            var scenes = new SerializedObject(cutscenes);
            scenes.FindProperty("dialogue").objectReferenceValue = runner;
            scenes.FindProperty("runIntro").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(RunIntroAsset);
            scenes.FindProperty("bossIntro").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(BossIntroAsset);
            scenes.FindProperty("phaseTransition").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(TransitionAsset);
            scenes.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { Object.DestroyImmediate(root); }
    }

    // Retratos ancorados ao centro do painel (720 px): 480 px de cada lado = 120 px da borda em 1200x720.
    // A sobreposição com o painel fica igual em 4:3 e 16:9. Margem lateral do texto: antes 30.
    private const float PortraitOffset = 480f, TextSideMargin = 78f;

    // Ajustes no prefab existente (dica de skip, ordem e posição dos retratos, margens): preserva os fileIDs usados pelos overrides das cenas.
    private static void ApplyLayout()
    {
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var rect = (RectTransform)root.transform.Find("Content/SkipHint");
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -16f);
            rect.sizeDelta = new Vector2(480f, 20f);
            var text = rect.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Top;
            text.text = "<mark=#12161CC8> SEGURE TAB: PULAR </mark>";
            // Retratos desenham por cima do quadro de diálogo (irmãos posteriores ficam na frente).
            Transform content = root.transform.Find("Content");
            foreach (string portrait in new[] { "PortraitLeft", "PortraitRight" })
                content.Find(portrait).SetSiblingIndex(content.Find("DialogueUI").GetSiblingIndex() + 1);
            rect.SetAsLastSibling();
            // Com os retratos na frente, eles vão mais para as bordas e o texto recua para não ficar sob o focinho/bico.
            foreach (string portrait in new[] { "PortraitLeft", "PortraitRight" })
            {
                var slot = (RectTransform)content.Find(portrait);
                slot.anchorMin = slot.anchorMax = new Vector2(.5f, 0f);
                slot.anchoredPosition = new Vector2(portrait == "PortraitLeft" ? -PortraitOffset : PortraitOffset, 0f);
            }
            var body = (RectTransform)content.Find("DialogueUI/Panel/Body");
            body.offsetMin = new Vector2(TextSideMargin, 24f);
            body.offsetMax = new Vector2(-TextSideMargin, -26f);
            ((RectTransform)content.Find("DialogueUI/Panel/ContinueIcon")).anchoredPosition = new Vector2(-TextSideMargin + 14f, 18f);
            var runner = new SerializedObject(root.GetComponent<DialogueRunner>());
            runner.FindProperty("plateInset").floatValue = TextSideMargin - 14f;
            runner.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    // Overlay de fade e serviço de troca de cena; o aviso de desenvolvimento só aparece sem a cena de destino.
    private static void EnsureChapterTransition()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAsset);
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            Transform existing = root.transform.Find("TransitionOverlay");
            RectTransform overlay = existing as RectTransform;
            if (overlay == null)
            {
                overlay = Stretch("TransitionOverlay", root.transform, Vector2.zero, Vector2.zero);
                var canvas = overlay.gameObject.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                // Acima do diálogo (250), abaixo da pausa (300): o menu de pausa continua acessível no aviso.
                canvas.sortingOrder = 260;
                overlay.gameObject.AddComponent<GraphicRaycaster>();
                overlay.gameObject.AddComponent<CanvasGroup>();
                var black = Image(Stretch("Black", overlay, Vector2.zero, Vector2.zero), null, false);
                black.color = Color.black;
                black.raycastTarget = true;
                var message = Text(Rect("Message", overlay, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 14f), new Vector2(1000f, 60f)),
                    font, 22f, TextAlignmentOptions.Center, TextWrappingModes.Normal);
                message.text = string.Empty;
                var hint = Text(Rect("Hint", overlay, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -36f), new Vector2(1000f, 30f)),
                    font, 22f, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
                hint.text = "Esc: menu de pausa";
                overlay.gameObject.SetActive(false);
            }
            var transition = root.GetComponent<SceneTransition>() ?? root.AddComponent<SceneTransition>();
            var fields = new SerializedObject(transition);
            fields.FindProperty("overlay").objectReferenceValue = overlay.GetComponent<CanvasGroup>();
            fields.FindProperty("message").objectReferenceValue = overlay.Find("Message").GetComponent<TMP_Text>();
            fields.FindProperty("hint").objectReferenceValue = overlay.Find("Hint").GetComponent<TMP_Text>();
            fields.FindProperty("dialogue").objectReferenceValue = root.GetComponent<DialogueRunner>();
            fields.ApplyModifiedPropertiesWithoutUndo();
            var flow = root.GetComponent<ChapterFlow>() ?? root.AddComponent<ChapterFlow>();
            var flowFields = new SerializedObject(flow);
            flowFields.FindProperty("chapter").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ChapterDefinition>(ChapterAsset);
            flowFields.FindProperty("transition").objectReferenceValue = transition;
            flowFields.ApplyModifiedPropertiesWithoutUndo();
            var cutscenes = new SerializedObject(root.GetComponent<RunCutscenes>());
            cutscenes.FindProperty("bossFinale").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(FinaleAsset);
            cutscenes.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void SetArray(SerializedProperty array, params Object[] values)
    {
        array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void InstallMusic()
    {
        var root = PrefabUtility.LoadPrefabContents(BossPrefab);
        try
        {
            var music = root.GetComponent<GusiniMusic>() ?? root.AddComponent<GusiniMusic>();
            var fields = new SerializedObject(music);
            fields.FindProperty("phaseOneMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicOne);
            fields.FindProperty("phaseTwoMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicTwo);
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, BossPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void InstallScene(string path, bool playRunIntro)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var waves = Object.FindObjectsByType<WaveController>(FindObjectsInactive.Include).Single();
        var pause = Object.FindObjectsByType<PauseController>(FindObjectsInactive.Include).Single();
        var player = new SerializedObject(waves).FindProperty("player").objectReferenceValue;
        var cutscenes = Object.FindObjectsByType<RunCutscenes>(FindObjectsInactive.Include).FirstOrDefault();
        if (cutscenes == null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), scene);
            cutscenes = instance.GetComponent<RunCutscenes>();
        }
        var fields = new SerializedObject(cutscenes);
        fields.FindProperty("waves").objectReferenceValue = waves;
        fields.FindProperty("player").objectReferenceValue = player;
        fields.FindProperty("pause").objectReferenceValue = pause;
        fields.FindProperty("playRunIntro").boolValue = playRunIntro;
        fields.ApplyModifiedPropertiesWithoutUndo();
        var runner = new SerializedObject(cutscenes.GetComponent<DialogueRunner>());
        runner.FindProperty("pause").objectReferenceValue = pause;
        runner.ApplyModifiedPropertiesWithoutUndo();
        var waveFields = new SerializedObject(waves);
        waveFields.FindProperty("cutscenes").objectReferenceValue = cutscenes;
        waveFields.ApplyModifiedPropertiesWithoutUndo();
        var flow = new SerializedObject(cutscenes.GetComponent<ChapterFlow>());
        flow.FindProperty("waves").objectReferenceValue = waves;
        flow.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Crocodilo/Dialogue/Validate cinematics")]
    public static void Validate()
    {
        foreach (UiSprite spec in UiSprites) RequirePixelSprite(UiArt + spec.name + ".png", 50f, spec.border);
        foreach (var portrait in Portraits)
        {
            Sprite sprite = RequirePixelSprite(PortraitArt + portrait.file, 100f, Vector4.zero);
            Require(sprite.rect.size == new Vector2(160f, 160f) && sprite.pivot == new Vector2(80f, 0f), "Portrait 160x160 pivot base " + portrait.file);
        }
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAsset);
        Require(font != null && font.atlasRenderMode == GlyphRenderMode.RASTER_HINTED && font.atlasTexture.filterMode == FilterMode.Point, "Pixelify raster font");
        var cast = AssetDatabase.LoadAssetAtPath<DialogueCast>(CastAsset);
        Require(cast != null && cast.Speakers.Length >= 3, "Dialogue cast");
        foreach (var portrait in Portraits)
            Require(DialogueCast.HasExpression(cast.Find(portrait.speaker), portrait.id), "Expression " + portrait.speaker + "/" + portrait.id);
        var corrupted = cast.Find("gusini_corrupted");
        Require(corrupted.expressions.Length == 1 && corrupted.expressions[0].id == "corrupted_furious", "Phase 2 uses only corrupted_furious");
        foreach (string path in new[] { RunIntroAsset, BossIntroAsset, TransitionAsset, FinaleAsset })
        {
            var sequence = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
            Require(sequence != null, "Sequence " + path);
            if (!sequence.Validate(out string error)) Fail(path + ": " + error);
            foreach (var line in sequence.Lines)
                Require(font.HasCharacters(line.text, out List<char> missing) || Fail("Glifos ausentes: " + new string(missing.ToArray())), "Glyphs " + line.text);
        }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        Require(prefab != null && prefab.GetComponent<DialogueRunner>() != null && prefab.GetComponent<RunCutscenes>() != null, "Cinematics prefab");
        Require(prefab.GetComponent<Canvas>().sortingOrder == 250 && prefab.GetComponent<CanvasScaler>().referenceResolution == new Vector2(1200f, 720f), "Cinematics canvas");
        RequireReferences(prefab.GetComponent<DialogueRunner>(), "pause");
        RequireReferences(prefab.GetComponent<RunCutscenes>(), "waves", "player", "pause");
        RequireReferences(prefab.GetComponent<DialogueBlipAudio>());
        RequireReferences(prefab.GetComponent<SceneTransition>());
        RequireReferences(prefab.GetComponent<ChapterFlow>(), "waves");
        Require(prefab.transform.Find("TransitionOverlay").GetComponent<Canvas>().sortingOrder == 260, "Transition overlay below pause");
        var radio = cast.Find("radio");
        Require(radio != null && radio.side == DialogueCast.Side.None && radio.displayName == "RÁDIO", "RÁDIO speaker without portrait");
        var finale = AssetDatabase.LoadAssetAtPath<DialogueSequence>(FinaleAsset);
        Require(finale.Count == 20 && finale.Lines.Count(l => l.speakerId == "radio") == 4, "Finale script: 20 lines, 4 radio");
        Require(finale.Lines.All(l => l.speakerId != "gusini_corrupted"), "Finale never uses the corrupted portrait");
        Require(finale.Lines.Any(l => l.beat == "radio") && finale.Lines.Any(l => l.beat == "farewell"), "Finale beats");
        var chapter = AssetDatabase.LoadAssetAtPath<ChapterDefinition>(ChapterAsset);
        Require(chapter != null && chapter.NextScene == "BaseImprovisada" && chapter.CompletionFlags.Contains("gusini_rescued") && chapter.CompletionFlags.Contains("chapter1_completed"), "Chapter 1 destination and flags");
        Require(font.HasCharacters(chapter.UnavailableMessage, out List<char> missingMessage) || Fail("Glifos ausentes: " + new string(missingMessage.ToArray())), "Fallback message glyphs");
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefab);
        Require(boss.GetComponent<GusiniMusic>() != null, "Gusini music component");
        RequireReferences(boss.GetComponent<GusiniMusic>());
        foreach (string path in new[] { MusicOne, MusicTwo })
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            Require(importer.defaultSampleSettings.loadType == AudioClipLoadType.Streaming, "Music streaming " + path);
            Require(AssetDatabase.LoadAssetAtPath<AudioClip>(path).length > 60f, "Music length " + path);
        }
        Require(AssetDatabase.LoadAssetAtPath<AudioClip>(Blip).length < .1f, "Text blip is the 50 ms master");
        Debug.Log("DIALOGUE CINEMATICS VALIDATION PASSED: imports, glyphs, cast, three sequences, prefab, music.");
    }

    public static void ValidateScenes()
    {
        foreach (string path in Scenes)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var cutscenes = Object.FindObjectsByType<RunCutscenes>(FindObjectsInactive.Include).Single();
            RequireReferences(cutscenes);
            RequireReferences(cutscenes.GetComponent<DialogueRunner>());
            RequireReferences(cutscenes.GetComponent<ChapterFlow>());
            var waves = Object.FindObjectsByType<WaveController>(FindObjectsInactive.Include).Single();
            Require(new SerializedObject(waves).FindProperty("cutscenes").objectReferenceValue == cutscenes, "WaveController cutscenes " + path);
        }
        Debug.Log("DIALOGUE SCENE WIRING PASSED.");
    }

    private static Sprite RequirePixelSprite(string path, float pixelsPerUnit, Vector4 border)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        Require(sprite != null && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
            && importer.textureCompression == TextureImporterCompression.Uncompressed && Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit), "Pixel import " + path);
        Require(sprite.border == border, "9-slice " + path);
        return sprite;
    }

    private static void RequireReferences(Object component, params string[] sceneOnly)
    {
        var fields = new SerializedObject(component);
        var property = fields.GetIterator();
        for (bool enter = true; property.NextVisible(enter); enter = false)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script" || sceneOnly.Contains(property.name)) continue;
            Require(property.objectReferenceValue != null, component.GetType().Name + "." + property.propertyPath);
        }
        if (component is Component behaviour && behaviour.GetComponent<DialogueRunner>() is DialogueRunner runner && component == runner)
            Require(new SerializedObject(runner).FindProperty("radioActive").arraySize == 3, "Radio frames");
    }

    private static bool Fail(string message) => throw new InvalidOperationException(message);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("DIALOGUE CHECK FAILED: " + message);
    }
}
