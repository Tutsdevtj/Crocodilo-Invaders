using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerHUDTools
{
    private const string Prefab = "Assets/Prefabs/UI/PlayerHUD.prefab";
    private const string Art = "Assets/Art/UI/PlayerHUDv2/";
    [Serializable] private sealed class Layout { public Piece[] pieces; }
    [Serializable] private sealed class Piece
    {
        public string name;
        public int[] size_ui;
        public int[] unity_anchoredPosition_bottomleft;
    }

    [MenuItem("Crocodilo/HUD/Rebuild Player cockpit")]
    public static void Rebuild()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/PlayerHUDv2/hud_v2_layout.json"));
        var pieces = layout.pieces.ToDictionary(p => p.name);
        GameObject root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var panel = root.transform.Find("PlayerPanel");
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                var child = panel.GetChild(i);
                if (child.name != "PointsText" && child.name != "KillsText") UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            var cockpitRect = Rect("Cockpit", panel, new Vector2(16, 16), new Vector2(364, 112));
            var cockpit = cockpitRect.gameObject.AddComponent<PlayerHUDCockpit>();
            var fields = new SerializedObject(cockpit);
            Image Make(string name, string field = null)
            {
                Piece piece = pieces[name];
                string folder = name.StartsWith("fx_") ? "FX/" : "Parts/";
                Image image = Image(name, cockpitRect, Load(folder + name),
                    new Vector2(piece.unity_anchoredPosition_bottomleft[0], piece.unity_anchoredPosition_bottomleft[1]),
                    new Vector2(piece.size_ui[0], piece.size_ui[1]));
                if (piece.name.Contains("fill") || name == "hp_damage_lag" || name == "hp_heal_flash") Filled(image);
                if (field != null) fields.FindProperty(field).objectReferenceValue = image;
                return image;
            }
            void Frames(string field, string folder, string prefix, int count)
                => Sprites(fields.FindProperty(field), Enumerable.Range(0, count).Select(i => Load(folder + prefix + i)).ToArray());
            void SpriteField(string field, string name) => fields.FindProperty(field).objectReferenceValue = Load("Parts/" + name);

            Make("hud_plate"); Make("hud_name"); Make("hp_track");
            Make("hp_damage_lag", "damageLag"); Make("hp_fill", "healthFill"); Make("hp_heal_flash", "healFlash");
            Make("hp_housing"); Make("hp_emblem", "emblem"); Make("hp_dial_crack", "dialCrack"); Make("hp_lowhealth_glow", "lowHealthGlow");
            SpriteField("healthNormal", "hp_fill"); SpriteField("healthLow", "hp_fill_low");
            SpriteField("emblemNormal", "hp_emblem"); SpriteField("emblemLow", "hp_emblem_low");
            Make("sp_track"); Make("sp_fill_low", "specialFill");
            var mask = Rect("LiquidMask", cockpitRect, new Vector2(84, 18), new Vector2(206, 14));
            mask.gameObject.AddComponent<RectMask2D>(); fields.FindProperty("liquidMask").objectReferenceValue = mask;
            var wave = Image("Wave", mask, Load("Parts/sp_wave_0"), new Vector2(0, 10), new Vector2(206, 4));
            fields.FindProperty("wave").objectReferenceValue = wave;
            Images(fields.FindProperty("bubbles"), Enumerable.Range(0, 3).Select(i => Image("Bubble " + i, mask, Load("Parts/sp_bubble_0"), Vector2.zero, new Vector2(8, 6))).ToArray());
            Make("sp_refill_flash", "refillFlash"); Make("sp_glass"); Make("sp_housing"); Make("sp_turbine");
            fields.FindProperty("fan").objectReferenceValue = Image("Fan", cockpitRect, Load("Parts/sp_fan_0"), new Vector2(296, 12), new Vector2(10, 26));
            fields.FindProperty("lamp").objectReferenceValue = Image("Lamp", cockpitRect, Load("Parts/sp_lamp_low"), new Vector2(320, 36), new Vector2(6, 4));
            Make("sp_full_glow", "fullGlow");
            fields.FindProperty("exhaust").objectReferenceValue = Image("Exhaust", cockpitRect, Load("FX/fx_flame_0"), new Vector2(356, 12), new Vector2(48, 26));
            fields.FindProperty("activation").objectReferenceValue = Image("Activation", cockpitRect, Load("FX/fx_activate_0"), new Vector2(340, 8), new Vector2(34, 34));
            Images(fields.FindProperty("sparks"), Enumerable.Range(0, 9).Select(i => Image("Spark " + i, cockpitRect, Load("FX/fx_spark_dot"), new Vector2(356, 26), new Vector2(2, 2))).ToArray());
            Sprites(fields.FindProperty("chargeSprites"), new[] { Load("Parts/sp_fill_low"), Load("Parts/sp_fill_mid"), Load("Parts/sp_fill_full") });
            Sprites(fields.FindProperty("lampSprites"), new[] { Load("Parts/sp_lamp_low"), Load("Parts/sp_lamp_mid"), Load("Parts/sp_lamp_full") });
            Sprites(fields.FindProperty("sparkSprites"), new[] { Load("FX/fx_spark_dot"), Load("FX/fx_spark_small"), Load("FX/fx_spark_cross"), Load("FX/fx_spark_streak") });
            Frames("waveFrames", "Parts/", "sp_wave_", 4); Frames("bubbleFrames", "Parts/", "sp_bubble_", 3);
            Frames("fanFrames", "Parts/", "sp_fan_", 3); Frames("flameFrames", "FX/", "fx_flame_", 6);
            Frames("activationFrames", "FX/", "fx_activate_", 3); Frames("shutdownFrames", "FX/", "fx_shutdown_", 4);
            fields.ApplyModifiedPropertiesWithoutUndo();
            var hudFields = new SerializedObject(root.GetComponent<PlayerHUD>());
            hudFields.FindProperty("cockpit").objectReferenceValue = cockpit;
            hudFields.ApplyModifiedPropertiesWithoutUndo();
            cockpit.ResetDisplay(1f, 0f);
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("Crocodilo/HUD/Validate Player cockpit")]
    public static void Validate()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var hud = new SerializedObject(root.GetComponent<PlayerHUD>());
        var cockpit = hud.FindProperty("cockpit").objectReferenceValue as PlayerHUDCockpit;
        Require(cockpit != null, "Cockpit wired to existing PlayerHUD");
        var rect = (RectTransform)cockpit.transform;
        Require(rect.anchoredPosition == new Vector2(16, 16) && rect.sizeDelta == new Vector2(364, 112), "Cockpit native x2 layout");
        Require(cockpit.GetComponent<Mask>() == null && cockpit.GetComponent<RectMask2D>() == null, "Exhaust can extend beyond panel");
        Require(root.GetComponent<CanvasScaler>().referenceResolution == new Vector2(1200, 720), "Existing canvas resolution");
        Require(hud.FindProperty("pointsText").objectReferenceValue != null && hud.FindProperty("killsText").objectReferenceValue != null, "Score text references preserved");
        var fields = new SerializedObject(cockpit); var property = fields.GetIterator();
        while (property.NextVisible(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                Require(property.objectReferenceValue != null, "Cockpit missing reference: " + property.propertyPath);
        Require(fields.FindProperty("sparks").arraySize == 9 && fields.FindProperty("bubbles").arraySize == 3, "Fixed FX pools");
        foreach (string field in new[] { "healthFill", "damageLag", "healFlash", "specialFill", "refillFlash" })
        {
            var image = (Image)fields.FindProperty(field).objectReferenceValue;
            Require(image.type == UnityEngine.UI.Image.Type.Filled && image.fillMethod == UnityEngine.UI.Image.FillMethod.Horizontal && image.fillOrigin == 0, "Continuous fill: " + field);
        }
        Require(cockpit.GetComponentsInChildren<Image>(true).All(i => !i.raycastTarget), "HUD does not intercept input");
        Require(Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories).Length == 51, "Only native x2 runtime art imported");
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.spritePixelsPerUnit == 100, "Pixel art import: " + path);
        }
        Debug.Log("PLAYER HUD V2 VALIDATION PASSED: native layout, continuous fills, references, fixed FX pools and 51 pixel-art sprites.");
    }

    private static Sprite Load(string relative) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + relative + ".png") ?? throw new InvalidOperationException(relative);
    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = 5; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        return rect;
    }
    private static Image Image(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size)
    {
        var image = Rect(name, parent, position, size).gameObject.AddComponent<Image>();
        image.sprite = sprite; image.raycastTarget = false; return image;
    }
    private static void Filled(Image image)
    {
        image.type = UnityEngine.UI.Image.Type.Filled; image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
    }
    private static void Images(SerializedProperty property, Image[] images)
    {
        property.arraySize = images.Length;
        for (int i = 0; i < images.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
    }
    private static void Sprites(SerializedProperty property, Sprite[] sprites)
    {
        property.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }
    private static void Require(bool pass, string message) { if (!pass) throw new InvalidOperationException(message); }
}
