using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CrocodiloLeaderboardSetup
{
    [Serializable] private class Art { public string name; public int width, height; public int[] bbox; }
    [Serializable] private class ArtList { public Art[] items; }
    private static TMP_FontAsset font;
    private const string MenuPrefab = "Assets/Prefabs/UI/MainMenu.prefab";

    [MenuItem("Crocodilo/Update Start Button")]
    public static void UpdateStartButton()
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var art = JsonUtility.FromJson<ArtList>(File.ReadAllText("Assets/Art/Leaderboard/ImportRects.json")).items.First(item => item.name == "Start");
        ImportArt(art, factories);
        const string path = "Assets/Prefabs/UI/PlayerName.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var button = root.GetComponentsInChildren<Button>(true).First(item => item.name == "StartButton");
            var image = button.GetComponent<Image>();
            image.sprite = Sprite("Start");
            image.preserveAspect = true;
            image.color = Color.white;
            button.targetGraphic = image;
            var rect = (RectTransform)button.transform;
            rect.sizeDelta = new Vector2(77f * image.sprite.rect.width / image.sprite.rect.height, 77);
            // The lettering belongs to the supplied artwork, so no text overlay is needed.
            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true)) Object.DestroyImmediate(label.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("START BUTTON UPDATE PASSED");
    }

    [MenuItem("Crocodilo/Setup Leaderboard")]
    public static void Setup()
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        foreach (var art in JsonUtility.FromJson<ArtList>(File.ReadAllText("Assets/Art/Leaderboard/ImportRects.json")).items) ImportArt(art, factories);
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset");
        if (font == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf");
            font = TMP_FontAsset.CreateFontAsset(source, 32, 5, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            font.name = "PressStart2P SDF";
            AssetDatabase.CreateAsset(font, "Assets/Fonts/PressStart2P/PressStart2P SDF.asset");
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (Texture2D atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÁÀÂÃÉÊÍÓÔÕÚÜÇáàâãéêíóôõúüç .,:;!?/-_<>×•");
            EditorUtility.SetDirty(font);
        }
        var leaderboard = CreateLeaderboard();
        var name = CreateNameView();
        var menu = PrefabUtility.LoadPrefabContents(MenuPrefab);
        try
        {
            var controller = menu.GetComponent<MainMenuController>();
            Transform old = menu.transform.Find("LeaderboardButton");
            if (old != null)
            {
                var existing = new SerializedObject(menu.GetComponent<MenuIntro>());
                var list = existing.FindProperty("buttonGroups");
                for (int i = list.arraySize - 1; i >= 0; i--)
                {
                    var value = list.GetArrayElementAtIndex(i).objectReferenceValue as CanvasGroup;
                    if (value == null || value.gameObject == old.gameObject)
                    {
                        list.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        list.DeleteArrayElementAtIndex(i);
                    }
                }
                existing.ApplyModifiedPropertiesWithoutUndo();
                Object.DestroyImmediate(old.gameObject);
            }
            Button button = ArtButton("LeaderboardButton", menu.transform, "MenuButton", new Vector2(180, 182), Vector2.zero);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-35, 28);
            UnityEventTools.AddPersistentListener(button.onClick, controller.ShowLeaderboard);
            var group = button.gameObject.AddComponent<CanvasGroup>();
            var intro = menu.GetComponent<MenuIntro>();
            var introData = new SerializedObject(intro);
            var groups = introData.FindProperty("buttonGroups");
            groups.arraySize++;
            groups.GetArrayElementAtIndex(groups.arraySize - 1).objectReferenceValue = group;
            introData.ApplyModifiedPropertiesWithoutUndo();
            var play = menu.GetComponentsInChildren<Button>(true).First(b => b.name == "PlayButton");
            var quit = menu.GetComponentsInChildren<Button>(true).First(b => b.name == "QuitButton");
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = quit;
            navigation.selectOnLeft = play;
            navigation.selectOnDown = play;
            button.navigation = navigation;
            foreach (var original in menu.GetComponentsInChildren<Button>(true).Where(b => b != button))
            {
                navigation = original.navigation;
                navigation.selectOnRight = button;
                original.navigation = navigation;
            }
            Ref(controller, "namePrefab", name);
            Ref(controller, "leaderboardPrefab", leaderboard);
            Ref(controller, "playButton", play);
            Ref(controller, "leaderboardButton", button);
            PrefabUtility.SaveAsPrefabAsset(menu, MenuPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(menu); }
        var wavesRoot = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI/WavesUI.prefab");
        try
        {
            var wavesView = wavesRoot.GetComponent<WavesUI>();
            var wavesData = new SerializedObject(wavesView);
            Transform resultPanel = ((GameObject)wavesData.FindProperty("startPanel").objectReferenceValue).transform;
            Transform oldReturn = resultPanel.Find("ReturnToMenuButton");
            if (oldReturn != null) Object.DestroyImmediate(oldReturn.gameObject);
            var backToMenu = ArtButton("ReturnToMenuButton", resultPanel, "Back", new Vector2(200, 68), new Vector2(0, -65));
            Ref(wavesView, "returnButton", backToMenu);
            PrefabUtility.SaveAsPrefabAsset(wavesRoot, "Assets/Prefabs/UI/WavesUI.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(wavesRoot); }
        // Refresh the existing instance in the scene; all earlier scene objects are preserved.
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("LEADERBOARD SETUP PASSED");
    }

    private static LeaderboardView CreateLeaderboard()
    {
        GameObject root = CanvasRoot("Leaderboard", out Transform content);
        var view = root.AddComponent<LeaderboardView>();
        Transform panel = Rect("Panel", content, new Vector2(760, 963), new Vector2(0, -10));
        Image(panel, "Panel", new Vector2(760, 963), Vector2.zero);
        Image(panel, "Mascot", new Vector2(295, 260), new Vector2(0, 360));
        Image(panel, "Title", new Vector2(620, 72), new Vector2(0, 200));
        Image(panel, "Divider", new Vector2(420, 43), new Vector2(0, 140));
        Image(panel, "Header", new Vector2(650, 62), new Vector2(0, 82));
        Image(panel, "Position", new Vector2(64, 23), new Vector2(-272, 82));
        Image(panel, "Player", new Vector2(145, 24), new Vector2(-72, 82));
        Image(panel, "Points", new Vector2(114, 22), new Vector2(247, 82));
        TMP_Text[] positions = new TMP_Text[7], names = new TMP_Text[7], scores = new TMP_Text[7];
        for (int i = 0; i < 7; i++)
        {
            float y = 24 - i * 51;
            positions[i] = Text("Position" + i, panel, "", new Vector2(70, 46), new Vector2(-275, y), 30);
            names[i] = Text("Name" + i, panel, "", new Vector2(350, 46), new Vector2(-41, y), 22);
            names[i].alignment = TextAlignmentOptions.MidlineLeft;
            scores[i] = Text("Score" + i, panel, "", new Vector2(190, 46), new Vector2(229, y), 24);
            scores[i].alignment = TextAlignmentOptions.MidlineRight;
            Image(panel, "Separator", new Vector2(650, 2), new Vector2(0, y - 26));
        }
        TMP_Text empty = Text("EmptyMessage", panel, "AINDA SEM PONTUAÇÕES\n\nJOGUE PARA ENTRAR\nNO RANKING", new Vector2(610, 200), new Vector2(0, -132), 20);
        Button back = ArtButton("BackButton", panel, "Back", new Vector2(237, 82), new Vector2(-202, -384));
        Button next = ArtButton("NextButton", panel, "Next", new Vector2(245, 79), new Vector2(198, -384));
        Button previous = LabelButton("PreviousPage", panel, "<", new Vector2(50, 42), new Vector2(-48, -349));
        var pagination = Text("Pagination", panel, "1 / 1", new Vector2(150, 32), new Vector2(0, -389), 17);
        var status = Text("Status", panel, "RANKING LOCAL", new Vector2(650, 40), new Vector2(0, -447), 11);
        Button close = LabelButton("CloseButton", panel, "X", new Vector2(42, 42), new Vector2(310, 402));
        // VOLTAR always returns to the menu; page navigation has its own small arrow.
        UnityEventTools.AddPersistentListener(back.onClick, view.Close);
        Ref(view, "content", content.gameObject);
        Refs(view, "positions", positions); Refs(view, "names", names); Refs(view, "scores", scores);
        Ref(view, "pagination", pagination); Ref(view, "emptyMessage", empty); Ref(view, "status", status);
        Ref(view, "previous", previous); Ref(view, "next", next); Ref(view, "close", close);
        Link(back, previous, next, close);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/UI/Leaderboard.prefab");
        Object.DestroyImmediate(root);
        return prefab.GetComponent<LeaderboardView>();
    }

    private static PlayerNameView CreateNameView()
    {
        GameObject root = CanvasRoot("PlayerName", out Transform content);
        var view = root.AddComponent<PlayerNameView>();
        Transform panel = Rect("Panel", content, new Vector2(850, 440), Vector2.zero);
        Image(panel, "Panel", new Vector2(850, 440), Vector2.zero);
        Text("Title", panel, "NOME DO JOGADOR", new Vector2(740, 60), new Vector2(0, 128), 29);
        Transform inputRoot = Rect("PlayerNameInput", panel, new Vector2(670, 75), new Vector2(0, 24));
        var inputBackground = inputRoot.gameObject.AddComponent<Image>();
        inputBackground.sprite = Sprite("Header");
        var input = inputRoot.gameObject.AddComponent<TMP_InputField>();
        var viewport = Rect("TextViewport", inputRoot, new Vector2(620, 56), Vector2.zero);
        viewport.gameObject.AddComponent<RectMask2D>();
        var text = Text("InputText", viewport, "", new Vector2(610, 54), Vector2.zero, 24);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        var placeholder = Text("Placeholder", viewport, "SEU NOME", new Vector2(610, 54), Vector2.zero, 24);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.color = new Color(.45f, .64f, .72f);
        input.targetGraphic = inputBackground;
        input.textViewport = (RectTransform)viewport;
        input.textComponent = (TextMeshProUGUI)text;
        input.placeholder = placeholder;
        input.fontAsset = font;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterValidation = TMP_InputField.CharacterValidation.None;
        input.caretColor = new Color(1f, .83f, .12f);
        input.customCaretColor = true;
        var message = Text("Message", panel, "", new Vector2(750, 55), new Vector2(0, -43), 12);
        var back = ArtButton("CancelButton", panel, "Back", new Vector2(225, 77), new Vector2(-184, -133));
        var start = ArtButton("StartButton", panel, "Start", new Vector2(245, 77), new Vector2(182, -133));
        var startImage = start.GetComponent<Image>();
        startImage.preserveAspect = true;
        ((RectTransform)start.transform).sizeDelta = new Vector2(77f * startImage.sprite.rect.width / startImage.sprite.rect.height, 77);
        Ref(view, "content", content.gameObject); Ref(view, "input", input); Ref(view, "message", message);
        Ref(view, "startButton", start); Ref(view, "cancelButton", back);
        Link(back, start);
        var nav = input.navigation; nav.mode = Navigation.Mode.Explicit; nav.selectOnDown = start; input.navigation = nav;
        nav = start.navigation; nav.selectOnUp = input; start.navigation = nav;
        nav = back.navigation; nav.selectOnUp = input; back.navigation = nav;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/UI/PlayerName.prefab");
        Object.DestroyImmediate(root);
        return prefab.GetComponent<PlayerNameView>();
    }

    private static GameObject CanvasRoot(string name, out Transform content)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 80;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        // Fit the entire portrait panel even in narrow windows.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        content = Rect("Content", root.transform, Vector2.zero, Vector2.zero);
        var rect = (RectTransform)content; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        var shade = content.gameObject.AddComponent<Image>();
        shade.color = new Color(.005f, .025f, .055f, .94f);
        return root;
    }

    private static void ImportArt(Art art, SpriteDataProviderFactories factories)
    {
        string path = "Assets/Art/Leaderboard/" + art.name + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var existing = provider.GetSpriteRects().FirstOrDefault(rect => rect.name == art.name);
        var id = existing != null ? existing.spriteID : GUID.Generate();
        provider.SetSpriteRects(new[] { new SpriteRect { name = art.name, alignment = SpriteAlignment.Center, spriteID = id,
            pivot = new Vector2(.5f, .5f), rect = new Rect(art.bbox[0], art.height - art.bbox[3], art.bbox[2] - art.bbox[0], art.bbox[3] - art.bbox[1]) } });
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(art.name, id) });
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Sprite Sprite(string name) => AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Leaderboard/" + name + ".png").OfType<Sprite>().First();
    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        return rect;
    }
    private static Image Image(Transform parent, string art, Vector2 size, Vector2 position)
    {
        var image = Rect(art, parent, size, position).gameObject.AddComponent<Image>();
        image.sprite = Sprite(art); image.raycastTarget = false;
        return image;
    }
    private static TMP_Text Text(string name, Transform parent, string value, Vector2 size, Vector2 position, int fontSize)
    {
        var text = Rect(name, parent, size, position).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.text = value; text.fontSize = fontSize;
        text.enableAutoSizing = true; text.fontSizeMin = Mathf.Min(11, fontSize); text.fontSizeMax = fontSize;
        text.alignment = TextAlignmentOptions.Center; text.color = new Color(.97f, .96f, .9f);
        text.richText = false; text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }
    private static Button ArtButton(string name, Transform parent, string art, Vector2 size, Vector2 position)
    {
        Image image = Image(parent, art, size, position); image.name = name; image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(1f, .88f, .55f); colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(.7f, .8f, .9f); colors.disabledColor = new Color(.4f, .4f, .4f, .5f);
        button.colors = colors;
        return button;
    }
    private static Button LabelButton(string name, Transform parent, string label, Vector2 size, Vector2 position)
    {
        var button = ArtButton(name, parent, "Header", size, position);
        Text("Label", button.transform, label, size - new Vector2(24, 14), Vector2.zero, label.Length > 2 ? 23 : 18);
        return button;
    }
    private static void Link(params Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var nav = buttons[i].navigation; nav.mode = Navigation.Mode.Explicit;
            nav.selectOnLeft = nav.selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length];
            nav.selectOnRight = nav.selectOnDown = buttons[(i + 1) % buttons.Length];
            buttons[i].navigation = nav;
        }
    }
    private static void Ref(Object target, string field, Object value)
    {
        var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Refs(Object target, string field, Object[] values)
    {
        var data = new SerializedObject(target); var list = data.FindProperty(field); list.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        data.ApplyModifiedPropertiesWithoutUndo();
    }
}
