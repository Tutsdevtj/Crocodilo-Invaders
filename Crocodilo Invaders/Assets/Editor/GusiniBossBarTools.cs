using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GusiniBossBarTools
{
    private const string Prefab = "Assets/Prefabs/UI/BossHUD.prefab";
    private const string Art = "Assets/Art/UI/GusiniBossBarF1/";
    [Serializable] private sealed class Layout { public Piece[] pieces; }
    [Serializable] private sealed class Piece
    {
        public string name;
        public int[] size_ui;
        public int[] anchoredPosition_ui_from_bottomleft;
    }

    [MenuItem("Crocodilo/HUD/Rebuild Gusini F1 bar")]
    public static void Rebuild()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var pieces = JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/GusiniBossBarF1/bossbar_f1_layout.json")).pieces.ToDictionary(p => p.name);
        GameObject root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var old = root.transform.Find("GusiniBarF1");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var bar = Rect("GusiniBarF1", root.transform, new Vector2(-16, 14), new Vector2(476, 100));
            bar.anchorMin = bar.anchorMax = bar.pivot = new Vector2(1, 0);
            var controller = bar.gameObject.AddComponent<BossBarFX>(); var fields = new SerializedObject(controller);
            var motion = Rect("Motion", bar, Vector2.zero, new Vector2(476, 100));
            fields.FindProperty("motion").objectReferenceValue = motion;
            RectTransform Group(string name, string field, string alpha)
            {
                var group = Rect(name, motion, Vector2.zero, new Vector2(476, 100));
                var canvas = group.gameObject.AddComponent<CanvasGroup>(); canvas.interactable = canvas.blocksRaycasts = false;
                fields.FindProperty(field).objectReferenceValue = group; fields.FindProperty(alpha).objectReferenceValue = canvas;
                return group;
            }
            var scarfGroup = Group("Scarf", "scarfGroup", "scarfAlpha");
            var barGroup = Group("Bar", "barGroup", "barAlpha");
            var emblem = Group("Emblem", "emblemGroup", "emblemAlpha");
            emblem.pivot = new Vector2(418f / 476f, .46f); emblem.anchoredPosition = new Vector2(418, 46);
            var nameGroup = Group("Name", "nameGroup", "nameAlpha");
            Image Make(string name, Transform parent, string field = null)
            {
                var p = pieces[name];
                var image = Image(name, parent, Load("Parts/" + name),
                    new Vector2(p.anchoredPosition_ui_from_bottomleft[0], p.anchoredPosition_ui_from_bottomleft[1]), new Vector2(p.size_ui[0], p.size_ui[1]));
                if (name == "hp_fill" || name == "hp_damage_lag" || name == "hp_hit_flash")
                { image.type = UnityEngine.UI.Image.Type.Filled; image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; image.fillOrigin = 1; }
                if (field != null) fields.FindProperty(field).objectReferenceValue = image;
                return image;
            }
            Make("lowhp_glow", barGroup, "lowGlow");
            var tail = Image("ScarfTail", scarfGroup, Load("Parts/orn_scarf_tail_0"), new Vector2(426, 0), new Vector2(50, 28));
            fields.FindProperty("scarf").objectReferenceValue = tail;
            Make("boss_backplate", barGroup); Make("hp_track", barGroup);
            Make("hp_damage_lag", barGroup, "damageLag"); Make("hp_fill", barGroup, "healthFill"); Make("hp_hit_flash", barGroup, "hitFlash");
            var mask = Rect("ShimmerMask", barGroup, new Vector2(32, 34), new Vector2(332, 18)); mask.gameObject.AddComponent<RectMask2D>();
            fields.FindProperty("shimmerMask").objectReferenceValue = mask;
            fields.FindProperty("shimmer").objectReferenceValue = Image("Shimmer", mask, Load("FX/fx_shimmer"), Vector2.zero, new Vector2(18, 18));
            fields.FindProperty("fillEdge").objectReferenceValue = Image("FillEdge", barGroup, Load("FX/fx_fill_edge"), new Vector2(32, 34), new Vector2(4, 18));
            Make("boss_frame", barGroup); Make("orn_beak", barGroup);
            Make("orn_wings", emblem); Make("emblem_roundel", emblem); Make("emblem_alert", emblem, "emblemAlert"); Make("orn_scarf_knot", emblem);
            Make("nameplate", nameGroup); Make("orn_star", nameGroup); Make("name_text", nameGroup);
            var sparks = fields.FindProperty("hitSparks"); sparks.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var image = Image("HitSpark " + i, motion, Load("FX/fx_hit_spark_0"), new Vector2(32, 43), new Vector2(18, 26));
                image.rectTransform.pivot = Vector2.one * .5f; sparks.GetArrayElementAtIndex(i).objectReferenceValue = image;
            }
            var glint = Image("Glint", motion, Load("FX/fx_glint_0"), new Vector2(404, 62), new Vector2(14, 14));
            glint.rectTransform.pivot = Vector2.one * .5f; fields.FindProperty("glint").objectReferenceValue = glint;
            fields.FindProperty("normalFill").objectReferenceValue = Load("Parts/hp_fill");
            fields.FindProperty("criticalFill").objectReferenceValue = Load("Parts/hp_fill_low");
            Frames(fields.FindProperty("scarfFrames"), "Parts/orn_scarf_tail_", 4);
            Frames(fields.FindProperty("hitFrames"), "FX/fx_hit_spark_", 3);
            Frames(fields.FindProperty("glintFrames"), "FX/fx_glint_", 4);
            fields.ApplyModifiedPropertiesWithoutUndo();
            var hud = new SerializedObject(root.GetComponent<BossHUD>()); hud.FindProperty("gusiniBar").objectReferenceValue = controller; hud.ApplyModifiedPropertiesWithoutUndo();
            controller.Show(1f); bar.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets(); Validate();
    }

    [MenuItem("Crocodilo/HUD/Validate Gusini F1 bar")]
    public static void Validate()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab); var hud = new SerializedObject(root.GetComponent<BossHUD>());
        var bar = hud.FindProperty("gusiniBar").objectReferenceValue as BossBarFX;
        Require(bar != null, "Gusini bar binding"); var rect = (RectTransform)bar.transform;
        Require(rect.sizeDelta == new Vector2(476, 100) && rect.anchoredPosition == new Vector2(-16, 14) && rect.anchorMin == new Vector2(1, 0) && rect.pivot == new Vector2(1, 0), "Native bottom-right layout");
        Require(root.transform.Find("BossInfo") == null && bar.GetComponentsInChildren<TMPro.TMP_Text>(true).Length == 0, "Legacy Executor HUD removed");
        var fields = new SerializedObject(bar); var property = fields.GetIterator();
        while (property.NextVisible(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference) Require(property.objectReferenceValue != null, "Missing bar reference: " + property.propertyPath);
        foreach (string field in new[] { "healthFill", "damageLag", "hitFlash" })
        {
            var image = (Image)fields.FindProperty(field).objectReferenceValue;
            Require(image.type == UnityEngine.UI.Image.Type.Filled && image.fillMethod == UnityEngine.UI.Image.FillMethod.Horizontal && image.fillOrigin == 1, "Right-origin continuous fill: " + field);
        }
        Require(fields.FindProperty("hitSparks").arraySize == 3, "Fixed damage FX pool");
        Require(bar.GetComponentsInChildren<Image>(true).All(i => !i.raycastTarget), "HUD does not intercept input");
        Require(Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories).Length == 29, "Only x2 runtime sprites");
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.spritePixelsPerUnit == 100, "Pixel art import: " + path);
        }
        Debug.Log("GUSINI BOSS BAR VALIDATION PASSED: layout, right-origin fills, references, fixed pool and 29 original sprites; no legacy HUD.");
    }

    private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png") ?? throw new InvalidOperationException(name);
    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5;
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
    }
    private static Image Image(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size)
    { var image = Rect(name, parent, position, size).gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false; return image; }
    private static void Frames(SerializedProperty property, string prefix, int count)
    { property.arraySize = count; for (int i = 0; i < count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = Load(prefix + i); }
    private static void Require(bool pass, string message) { if (!pass) throw new InvalidOperationException(message); }
}
