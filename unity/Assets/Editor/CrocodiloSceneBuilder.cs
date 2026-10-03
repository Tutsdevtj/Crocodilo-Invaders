using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Executado uma vez no editor/CLI. O resultado é salvo, não reconstruído em Play.
public static class CrocodiloSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/JogoEditavel.unity";
    private const string PrefabRoot = "Assets/Prefabs";
    private const string GeneratedRoot = "Assets/Art/Generated";
    private static Font font;
    private static Sprite solid;

    [MenuItem("Crocodilo/Criar ou abrir cena editável")]
    public static void CreateEditableScene()
    {
        if (File.Exists(ScenePath))
        {
            if (Application.isBatchMode) { Debug.Log("EDITABLE_SCENE_ALREADY_EXISTS"); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
            return;
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(PrefabRoot);
        Directory.CreateDirectory(GeneratedRoot);
        AssetDatabase.Refresh();
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art" }))
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 100
                || importer.spritePivot != new Vector2(0, 1))) importer.SaveAndReimport();
        }
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        solid = CreateSolid();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Crocodilo Invaders — Configuração");
        var view = root.AddComponent<CrocodiloSceneView>();
        var game = root.AddComponent<CrocodiloGame>();
        game.sceneView = view;

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(6, -3.6f, -10);
        view.gameCamera = cameraObject.GetComponent<Camera>();
        view.gameCamera.orthographic = true;
        view.gameCamera.orthographicSize = 3.6f;
        view.gameCamera.clearFlags = CameraClearFlags.SolidColor;
        view.gameCamera.backgroundColor = new Color(0.025f, 0.12f, 0.25f);

        Transform world = Group("Fase", root.transform);
        view.worldRoot = world.gameObject;
        Transform environment = Group("Cenário", world);
        view.background = Place(MakeVisual("Fundo", Frames("background"), -100), environment, 0, 0);
        view.ground = Place(MakeVisual("Chão", Frames("ground"), -70), environment, 0, 530);
        view.ground.Renderer.drawMode = SpriteDrawMode.Tiled;
        view.ground.Renderer.size = new Vector2(24, 1.94f);
        view.sun = Place(MakeVisual("Sol", Frames("sun"), -90, frameSeconds: 0.08f), environment, 510, 70);
        CrocodiloVisual cloudPrefab = SavePrefab(MakeVisual("Nuvem", Frames("cloud"), -80, speed: 666.6667f));
        view.clouds = new[] { Place(cloudPrefab, environment, 400, 15), Place(cloudPrefab, environment, 750, 55) };

        Transform actors = Group("Personagens", world);
        Transform spawns = Group("Pontos de nascimento", world);
        CrocodiloVisual playerPrefab = SavePrefab(MakeVisual("Jogador", Frames("character"), 20, new Rect(0, 0, 100, 83), health: 30));
        view.player = Place(playerPrefab, actors, 100, 319);
        CrocodiloVisual enemyPrefab = SavePrefab(MakeVisual("Inimigo", Frames("enemy1"), 20, new Rect(4, 2, 65, 62), frameSeconds: 0.14f));
        view.enemies = new[] { Place(enemyPrefab, actors, 899, 225), Place(enemyPrefab, actors, 850, 400) };
        for (int i = 0; i < view.enemies.Length; ++i)
        {
            view.enemies[i].name = "Inimigo " + (i + 1);
            view.enemies[i].spawnPoint = Marker("Spawn Inimigo " + (i + 1), spawns, i == 0 ? 1400 : 1700, i == 0 ? 350 : 400);
            view.enemies[i].respawnY = i == 0 ? new Vector2(300, 500) : new Vector2(250, 500);
        }
        CrocodiloVisual bossPrefab = SavePrefab(MakeVisual("Chefe", Frames("newboss"), 20, new Rect(150, 80, 230, 250), health: 108, frameSeconds: 0.08f));
        view.boss = Place(bossPrefab, actors, 750, 90);
        view.boss.spawnPoint = Marker("Spawn Chefe", spawns, 1700, 90);
        view.bossWingHitboxes = new[] { AddCollider(Group("Colisão asa 1", view.boss.transform).gameObject, new Rect(45, 160, 50, 50)),
            AddCollider(Group("Colisão asa 2", view.boss.transform).gameObject, new Rect(150, 160, 50, 50)) };

        Transform shots = Group("Projéteis", world);
        view.transientRoot = Group("Em voo e efeitos", shots);
        view.normalShot = Place(SavePrefab(MakeVisual("Tiro normal", Frames("projectile"), 25, new Rect(0, 0, 25, 25), speed: 1666.6667f)), shots, 200, 350);
        view.specialShot = Place(SavePrefab(MakeVisual("Tiro especial", Frames("Sprojectile_effect"), 25, new Rect(2, 16, 124, 41), speed: 1666.6667f, damage: 4, frameSeconds: 0.01f)), shots, 300, 350);
        view.enemyBulletPrefab = SavePrefab(MakeVisual("Tiro inimigo", Frames("enemy_projectile"), 25, new Rect(0, 0, 25, 25), speed: 666.6667f));
        CrocodiloVisual missilePrefab = SavePrefab(MakeVisual("Míssil do chefe", Frames("boss_missile"), 35, new Rect(24, 46, 82, 19), speed: 1000, frameSeconds: 0.12f));
        view.missiles = new CrocodiloVisual[5];
        for (int i = 0; i < view.missiles.Length; ++i) view.missiles[i] = Place(missilePrefab, shots, 800, 100 + i * 80);
        Transform attacks = Group("Lasers do chefe", world);
        CrocodiloLaserView horizontalPrefab = MakeLaserPrefab(false), verticalPrefab = MakeLaserPrefab(true);
        view.lasers = new CrocodiloLaserView[4];
        for (int i = 0; i < view.lasers.Length; ++i)
        {
            var prefab = i < 2 ? horizontalPrefab : verticalPrefab;
            view.lasers[i] = ((GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, attacks)).GetComponent<CrocodiloLaserView>();
            view.lasers[i].name += " " + (i % 2 + 1);
            view.lasers[i].previewPosition = i == 0 ? 190 : i == 1 ? 475 : i == 2 ? 200 : 450;
        }
        view.powerUp = Place(SavePrefab(MakeVisual("Caixa de vida", Frames("health_box"), 25, new Rect(0, 0, 62, 69), frameSeconds: 0.08f)), world, 600, 300);
        view.borderAlert = Place(MakeVisual("Alerta de limite", Frames("alert"), 50, frameSeconds: 0.09f), world, 700, 100);
        view.effectPrefabs = new[] { "hit_effect", "sp_hit_effect", "enemy_death", "coin", "hp_up", "boss_death_effect" }
            .Select(key => new CrocodiloSceneView.EffectPrefab { animation = key, prefab = SavePrefab(MakeVisual("Efeito " + key, Frames(key), 40,
                frameSeconds: key == "boss_death_effect" ? 0.15f : key == "sp_hit_effect" ? 0.16f : key == "coin" || key == "hit_effect" ? 0.04f : key == "hp_up" ? 0.08f : 0.05f)) }).ToArray();

        CreateUI(root.transform, game, view);
        view.musicSource = Group("Música", root.transform).gameObject.AddComponent<AudioSource>();
        view.musicSource.playOnAwake = false; view.musicSource.loop = true; view.musicSource.volume = 0.65f;
        view.effectsSource = Group("Efeitos sonoros", root.transform).gameObject.AddComponent<AudioSource>();
        view.effectsSource.playOnAwake = false; view.effectsSource.volume = 0.8f;
        view.audioClips = Directory.GetFiles("Assets/Resources/Audio", "*.mp3").OrderBy(path => path)
            .Select(path => new CrocodiloSceneView.Sound { key = Path.GetFileNameWithoutExtension(path), clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path.Replace('\\', '/')) }).ToArray();
        view.FitCamera();
        view.ApplyEditorPreview();
        RecordOverrides(root);
        Directory.CreateDirectory("Assets/Scenes");
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Não foi possível salvar a cena editável.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("EDITABLE_SCENE_CREATED: " + ScenePath + "; objetos=" + UnityEngine.Object.FindObjectsOfType<Transform>(true).Length);
    }

    private static void RecordOverrides(GameObject root)
    {
        foreach (Component component in root.GetComponentsInChildren<Component>(true))
            if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }

    private static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
    private static Transform Marker(string name, Transform parent, float x, float y)
    {
        Transform marker = Group(name, parent);
        marker.position = new Vector3(x / 100, -y / 100, 0);
        return marker;
    }
    private static CrocodiloVisual MakeVisual(string name, Sprite[] frames, int order, Rect? collider = null,
        float speed = 333.33334f, int health = 4, int damage = 1, float frameSeconds = 0.1f)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        var visual = go.AddComponent<CrocodiloVisual>();
        visual.frames = frames;
        visual.frameSeconds = frameSeconds; visual.moveSpeed = speed; visual.maxHealth = health; visual.damage = damage;
        visual.Renderer.sprite = frames[0]; visual.Renderer.sortingOrder = order;
        if (collider.HasValue) visual.hitbox = AddCollider(go, collider.Value);
        return visual;
    }
    private static BoxCollider2D AddCollider(GameObject go, Rect pixels)
    {
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.offset = new Vector2(pixels.center.x / 100, -pixels.center.y / 100);
        collider.size = pixels.size / 100;
        return collider;
    }
    private static CrocodiloVisual SavePrefab(CrocodiloVisual visual)
    {
        string path = PrefabRoot + "/" + visual.name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(visual.gameObject, path).GetComponent<CrocodiloVisual>();
        UnityEngine.Object.DestroyImmediate(visual.gameObject);
        return prefab;
    }
    private static CrocodiloVisual Place(CrocodiloVisual visual, Transform parent, float x, float y)
    {
        if (EditorUtility.IsPersistent(visual)) visual = ((GameObject)PrefabUtility.InstantiatePrefab(visual.gameObject, parent)).GetComponent<CrocodiloVisual>();
        else visual.transform.SetParent(parent, false);
        visual.GamePosition = new Vector2(x, y);
        return visual;
    }
    private static Sprite[] Frames(string key)
    {
        string staticPath = "Assets/Resources/Art/" + key + ".png";
        if (File.Exists(staticPath)) return new[] { AssetDatabase.LoadAssetAtPath<Sprite>(staticPath) };
        return Directory.GetFiles("Assets/Resources/Art/Anim/" + key, "*.png").OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\', '/'))).ToArray();
    }
    private static Sprite CreateSolid()
    {
        string path = GeneratedRoot + "/Branco.asset";
        var texture = new Texture2D(2, 2) { name = "Branco", filterMode = FilterMode.Point };
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white }); texture.Apply();
        AssetDatabase.CreateAsset(texture, path);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0, 1), 100);
        sprite.name = "Branco";
        AssetDatabase.AddObjectToAsset(sprite, texture);
        return sprite;
    }
    private static CrocodiloLaserView MakeLaserPrefab(bool vertical)
    {
        string key = vertical ? "laser_beam_vertical" : "laser_beam";
        Sprite[] stripFrames = Frames(key).Select(frame =>
        {
            Texture2D texture = frame.texture;
            Rect rect = new Rect(0, 0, vertical ? texture.width : Mathf.Floor(texture.width * 0.7f), vertical ? Mathf.Floor(texture.height * 0.7f) : texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0, 1), 100, 0, SpriteMeshType.FullRect);
            sprite.name = key + "_" + frame.name;
            AssetDatabase.CreateAsset(sprite, GeneratedRoot + "/" + sprite.name + ".asset");
            return sprite;
        }).ToArray();
        var root = new GameObject(vertical ? "Laser vertical" : "Laser horizontal");
        var laser = root.AddComponent<CrocodiloLaserView>();
        laser.vertical = vertical;
        laser.beam = Place(MakeVisual("Feixe", stripFrames, 30, frameSeconds: 0.05f), root.transform, 0, 0);
        laser.trace = Place(MakeVisual("Linha de aviso", Frames(vertical ? "laser_trace_vertical" : "laser_trace"), 30), root.transform, 0, 0);
        laser.warningArea = Place(MakeVisual("Faixa de dano avisada", new[] { solid }, 29), root.transform, 0, 0);
        laser.warningArea.Renderer.color = new Color(0.7f, 0.2f, 1, 0.28f);
        laser.Show(0, 200, new Rect(0, 0, 1200, 720), 0);
        var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabRoot + "/" + root.name + ".prefab").GetComponent<CrocodiloLaserView>();
        UnityEngine.Object.DestroyImmediate(root);
        return saved;
    }

    private static RectTransform RectObject(string name, Transform parent, Rect rect)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(rect.x, -rect.y); rt.sizeDelta = rect.size;
        return rt;
    }
    private static RectTransform Stretch(string name, Transform parent)
    {
        RectTransform rt = RectObject(name, parent, new Rect());
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }
    private static RectTransform Panel(string name, Transform canvas, Color? backgroundColor = null)
    {
        RectTransform root = Stretch(name, canvas);
        if (backgroundColor.HasValue) { Image image = root.gameObject.AddComponent<Image>(); image.color = backgroundColor.Value; image.raycastTarget = false; }
        RectTransform content = RectObject("Layout 1200 x 720", root, new Rect(0, 0, 1200, 720));
        content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f); content.anchoredPosition = Vector2.zero;
        return content;
    }
    private static Image ImageAt(string name, string asset, Transform parent, Rect rect)
    {
        Image image = RectObject(name, parent, rect).gameObject.AddComponent<Image>();
        image.sprite = Frames(asset)[0]; image.raycastTarget = false;
        return image;
    }
    private static Image ImageNative(string name, string asset, Transform parent, float x, float y)
    {
        Sprite sprite = Frames(asset)[0];
        return ImageAt(name, asset, parent, new Rect(x, y, sprite.rect.width, sprite.rect.height));
    }
    private static Text Label(string name, string text, Transform parent, Rect rect, int size = 16, bool centered = false)
    {
        Text label = RectObject(name, parent, rect).gameObject.AddComponent<Text>();
        label.font = font; label.text = text; label.fontSize = size; label.color = Color.white;
        label.fontStyle = FontStyle.Bold; label.raycastTarget = false;
        label.alignment = centered ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow; label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }
    private static Button ButtonAt(string name, string text, Transform parent, Rect rect, UnityEngine.Events.UnityAction action, int size = 42)
    {
        RectTransform rt = RectObject(name, parent, rect);
        Image image = rt.gameObject.AddComponent<Image>(); image.color = Color.white;
        Button button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.64f, 0.35f, 0.025f); colors.highlightedColor = colors.selectedColor = new Color(0.82f, 0.47f, 0.04f);
        colors.pressedColor = new Color(0.45f, 0.25f, 0.015f); button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        Text label = Label("Texto", text, rt, new Rect(0, 0, rect.width, rect.height), size, true);
        label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static void CreateUI(Transform parent, CrocodiloGame game, CrocodiloSceneView view)
    {
        var canvasObject = new GameObject("Canvas — Menu e HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(parent, false); eventSystem.GetComponent<EventSystem>().sendNavigationEvents = false;
        CreateOriginalMenu(canvas.transform, game, view);
        RectTransform about = Panel("Sobre", canvas.transform, Color.black); view.aboutPanel = about.parent.gameObject;
        ImageAt("Arte Sobre", "sobre", about, new Rect(0, 0, 1200, 720));
        ButtonAt("Voltar", "VOLTAR AO MENU", about, new Rect(850, 670, 330, 40), game.BackToMenu, 18);

        RectTransform hud = Panel("HUD", canvas.transform); view.hudPanel = hud.parent.gameObject;
        ImageNative("Retrato jogador", "portrait", hud, 20, 600);
        view.playerHealthImage = ImageNative("Barra de vida jogador", "hp_bar", hud, 140, 650);
        view.chargeImage = ImageNative("Barra do especial", "special-4", hud, 140, 680);
        Label("Nome jogador", "B. Crocodilo", hud, new Rect(165, 626, 230, 24));
        view.pointsText = Label("Pontos", "Pontos: 0", hud, new Rect(450, 655, 270, 25));
        view.killsText = Label("Inimigos", "Inimigos: 0", hud, new Rect(450, 682, 270, 25));
        RectTransform bossHud = Stretch("HUD do chefe", hud); view.bossHud = bossHud.gameObject;
        ImageNative("Retrato chefe", "b_portrait", bossHud, 1070, 600);
        view.bossHealthImage = ImageNative("Barra de vida chefe", "hp_boss", bossHud, 745, 650);
        Label("Nome chefe", "Executor V-9", bossHud, new Rect(967, 625, 200, 25));
        view.playerHealthSprites = new[] { "hp_bar", "hp-1", "hp-2", "hp-3", "hp-4", "hp-5", "hp-6", "hp-7", "hp-8" }.Select(key => Frames(key)[0]).ToArray();
        view.chargeSprites = new[] { "special", "special-1", "special-2", "special-3", "special-4" }.Select(key => Frames(key)[0]).ToArray();
        view.bossHealthSprites = Enumerable.Range(0, 13).Select(i => Frames(i == 0 ? "hp_boss" : "hp_boss" + i)[0]).ToArray();

        RectTransform over = Panel("Derrota", canvas.transform); view.gameOverPanel = over.parent.gameObject;
        var shade = RectObject("Painel escuro", over, new Rect(260, 190, 680, 300)).gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, 0.75f);
        Label("Título", "FIM DE JOGO", over, new Rect(260, 220, 680, 80), 42, true);
        view.gameOverScore = Label("Resultado", "Pontos: 0    Inimigos: 0", over, new Rect(350, 320, 500, 25), 16, true);
        ButtonAt("Voltar", "VOLTAR AO MENU", over, new Rect(405, 390, 390, 45), game.BackToMenu, 18);
        RectTransform victory = Panel("Vitória", canvas.transform, Color.black); view.victoryPanel = victory.parent.gameObject;
        ImageAt("Arte vitória", "win_menu", victory, new Rect(0, 0, 1200, 720));
        view.victoryPoints = Label("Pontos finais", "0", victory, new Rect(620, 275, 160, 44), 30);
        view.victoryKills = Label("Inimigos finais", "0", victory, new Rect(735, 344, 60, 44), 30);
        Label("Rank", "SS", victory, new Rect(668, 532, 110, 60), 42, true);
        ButtonAt("Voltar", "VOLTAR AO MENU", victory, new Rect(850, 670, 330, 40), game.BackToMenu, 18);
        CreatePausePanel(canvas.transform, game, view);
    }

    // Recortes da arte existente: não há uma terceira imagem/botão com "Sair".
    private static RawImage MenuArt(string name, Transform parent, Rect source, Material material)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/mainMenu.png");
        Rect target = new Rect(source.x * 1200 / texture.width, source.y * 720 / texture.height,
            source.width * 1200 / texture.width, source.height * 720 / texture.height);
        RawImage image = RectObject(name, parent, target).gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.uvRect = new Rect(source.x / texture.width, 1 - source.yMax / texture.height,
            source.width / texture.width, source.height / texture.height);
        image.material = material;
        image.raycastTarget = false;
        return image;
    }

    private static void CreateOriginalMenu(Transform canvas, CrocodiloGame game, CrocodiloSceneView view)
    {
        RectTransform menu = Panel("Menu", canvas);
        view.menuPanel = menu.parent.gameObject;
        Image background = Stretch("Fundo original", menu.parent).gameObject.AddComponent<Image>();
        background.sprite = Frames("background")[0]; background.raycastTarget = false;
        background.transform.SetAsFirstSibling();
        string materialPath = GeneratedRoot + "/Arte menu original.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Crocodilo/Arte menu original"));
            AssetDatabase.CreateAsset(material, materialPath);
        }
        MenuArt("Título original", menu, new Rect(230, 60, 970, 390), material);
        RawImage play = MenuArt("Jogar", menu, new Rect(410, 472, 585, 150), material);
        RawImage about = MenuArt("Sobre", menu, new Rect(410, 631, 585, 150), material);
        view.menuButtons = new[] { ArtButton(play, game.PlayFromMenu), ArtButton(about, game.AboutFromMenu) };
        view.menuArrow = ImageNative("Seta de seleção", "seta", menu, 787, 347).rectTransform;
        string[] controls = { "Setas direcionais para se mover", "Pressione ESPAÇO para atacar", "Pressione C para ataque especial", "ESC para pausar o jogo" };
        for (int i = 0; i < controls.Length; ++i) Label("Controle " + (i + 1), controls[i], menu, new Rect(20, 605 + i * 25, 650, 24), 14);
    }

    private static Button ArtButton(RawImage image, UnityEngine.Events.UnityAction action)
    {
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = colors.highlightedColor = colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f); button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static void CreatePausePanel(Transform canvas, CrocodiloGame game, CrocodiloSceneView view)
    {
        RectTransform pause = Panel("Pausa", canvas, new Color(0, 0, 0, 0.65f));
        view.pausePanel = pause.parent.gameObject;
        // Intercepta cliques na fase enquanto a pausa está aberta.
        view.pausePanel.GetComponent<Image>().raycastTarget = true;
        Label("Título", "PAUSADO", pause, new Rect(300, 190, 600, 80), 42, true);
        view.continueButton = ButtonAt("Continuar", "CONTINUAR", pause, new Rect(405, 320, 390, 65), game.ResumeGame, 24);
        view.pauseMenuButton = ButtonAt("Voltar ao menu", "VOLTAR AO MENU", pause, new Rect(405, 410, 390, 65), game.BackToMenu, 24);
        Label("Atalho", "ESC ou ENTER para continuar", pause, new Rect(300, 505, 600, 30), 18, true);
    }

    [MenuItem("Crocodilo/Restaurar menu original e adicionar pausa")]
    public static void RestoreMenuAndPause()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var view = UnityEngine.Object.FindObjectOfType<CrocodiloSceneView>();
        var game = view.GetComponent<CrocodiloGame>();
        Transform canvas = view.menuPanel.transform.parent;
        UnityEngine.Object.DestroyImmediate(view.menuPanel);
        if (view.pausePanel != null) UnityEngine.Object.DestroyImmediate(view.pausePanel);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateOriginalMenu(canvas, game, view);
        CreatePausePanel(canvas, game, view);
        view.ApplyEditorPreview();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("ORIGINAL_MENU_AND_PAUSE_SAVED");
    }

    [MenuItem("Crocodilo/Prévia no editor/Fase")]
    public static void PreviewGameplay() { SetPreview(CrocodiloSceneView.Preview.Fase); }
    [MenuItem("Crocodilo/Prévia no editor/Menu")]
    public static void PreviewMenu() { SetPreview(CrocodiloSceneView.Preview.Menu); }
    [MenuItem("Crocodilo/Prévia no editor/Chefe")]
    public static void PreviewBoss() { SetPreview(CrocodiloSceneView.Preview.Chefe); }
    [MenuItem("Crocodilo/Prévia no editor/Pausa")]
    public static void PreviewPause() { SetPreview(CrocodiloSceneView.Preview.Pausa); }
    private static void SetPreview(CrocodiloSceneView.Preview preview)
    {
        if (Application.isPlaying) return;
        var view = UnityEngine.Object.FindObjectOfType<CrocodiloSceneView>();
        if (view == null) return;
        Undo.RecordObject(view, "Alterar prévia"); view.editorPreview = preview; view.ApplyEditorPreview();
        EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.in2DMode = true;
            if (preview == CrocodiloSceneView.Preview.Menu)
            {
                Selection.activeGameObject = view.menuPanel;
                SceneView.lastActiveSceneView.FrameSelected();
            }
            else SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(6, -3.6f, 0), new Vector3(12, 7.2f, 1)), false);
        }
    }
}
