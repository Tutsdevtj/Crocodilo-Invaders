using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

// Hangar/wardrobe MVP: builds the skin catalog and turns the unused "Online" menu slot into "Hangar".
public static class WardrobeTools
{
    public const string Catalog = "Assets/Resources/Wardrobe/SkinCatalog.asset";
    public const string Wordmark = "Assets/Art/MainMenu/Hangar.png";
    const string Menu = "Assets/Prefabs/UI/MainMenu.prefab";
    const string OriginalController = "Assets/Animations/Player/Player.controller";
    const string OriginalIdle = "Assets/Animations/Player/PlayerIdle.anim";
    const string CrocsIdle = CrocsSkinTools.Animations + "/PlayerCrocsIdle.anim";

    [MenuItem("Crocodilo/Wardrobe/Install Hangar (catalog + menu button)")]
    public static void Install()
    {
        ImportWordmark();
        BuildCatalog();
        InstallMenuButton();
        AssetDatabase.SaveAssets();
        Validate();
    }

    static void ImportWordmark()
    {
        AssetDatabase.ImportAsset(Wordmark);
        var importer = AssetImporter.GetAtPath(Wordmark) as TextureImporter;
        if (importer == null) throw new Exception("Run Tools/Art/prepare_hangar_wordmark_v01.cjs first: " + Wordmark);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
        importer.alphaIsTransparency = true;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    static void BuildCatalog()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Catalog));
        var catalog = AssetDatabase.LoadAssetAtPath<SkinCatalog>(Catalog);
        if (catalog == null) { catalog = ScriptableObject.CreateInstance<SkinCatalog>(); AssetDatabase.CreateAsset(catalog, Catalog); }
        var skins = new List<SkinDefinition>
        {
            Skin("original", "Bombardiro Original", "O avião de sempre do B. Crocodilo: confiável, barulhento e cheio de remendo.",
                OriginalController, OriginalIdle),
            Skin("crocs", "Bombardiro Crocs", "Crocs a jato: asas, turbina no calcanhar e capacete de borracha com jibbitz de guerra nos furos. Mesmo desempenho, muito mais estilo.",
                CrocsSkinTools.Controller, CrocsIdle),
            new SkinDefinition
            {
                id = "prototype", displayName = "Protótipo Secreto", slot = SkinSlot.Ship, comingSoon = true,
                description = "Uma pintura nova ainda está na prancheta do mecânico.",
                unlockAchievement = AchievementCatalog.Gusini
            },
        };
        var serialized = new SerializedObject(catalog);
        serialized.Update();
        var array = serialized.FindProperty("skins");
        array.arraySize = skins.Count;
        for (int i = 0; i < skins.Count; i++)
        {
            var s = skins[i]; var e = array.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("id").stringValue = s.id;
            e.FindPropertyRelative("displayName").stringValue = s.displayName;
            e.FindPropertyRelative("description").stringValue = s.description;
            e.FindPropertyRelative("slot").enumValueIndex = (int)s.slot;
            e.FindPropertyRelative("controller").objectReferenceValue = s.controller;
            e.FindPropertyRelative("unlockAchievement").stringValue = s.unlockAchievement ?? "";
            e.FindPropertyRelative("comingSoon").boolValue = s.comingSoon;
            var frames = e.FindPropertyRelative("previewFrames"); var durations = e.FindPropertyRelative("previewDurations");
            frames.arraySize = s.previewFrames.Length; durations.arraySize = s.previewDurations.Length;
            for (int f = 0; f < s.previewFrames.Length; f++)
            {
                frames.GetArrayElementAtIndex(f).objectReferenceValue = s.previewFrames[f];
                durations.GetArrayElementAtIndex(f).floatValue = s.previewDurations[f];
            }
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
    }

    static SkinDefinition Skin(string id, string name, string description, string controllerPath, string idlePath)
    {
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
        if (controller == null || clip == null) throw new Exception("Missing skin animation for " + id);
        var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).First(b => b.propertyName == "m_Sprite");
        var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        var frames = new List<Sprite>(); var durations = new List<float>();
        for (int i = 0; i < keys.Length; i++)
        {
            float end = i + 1 < keys.Length ? keys[i + 1].time : clip.length;
            if (end - keys[i].time <= 0f) continue;
            frames.Add((Sprite)keys[i].value); durations.Add(end - keys[i].time);
        }
        return new SkinDefinition
        {
            id = id, displayName = name, description = description, slot = SkinSlot.Ship, controller = controller,
            previewFrames = frames.ToArray(), previewDurations = durations.ToArray(), unlockAchievement = ""
        };
    }

    static void InstallMenuButton()
    {
        var root = PrefabUtility.LoadPrefabContents(Menu);
        try
        {
            var buttonTransform = Find(root.transform, "HangarButton") ?? Find(root.transform, "OnlineButton");
            if (buttonTransform == null) throw new Exception("Menu slot for the Hangar not found");
            buttonTransform.name = "HangarButton";
            var label = buttonTransform.Find("Hangar") ?? buttonTransform.Find("Online");
            label.name = "Hangar";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Wordmark);
            var image = label.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite; image.preserveAspect = true;
            ((RectTransform)label).sizeDelta = new Vector2(56f * sprite.rect.width / sprite.rect.height, 56f);
            var button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
            var controller = root.GetComponentInChildren<MainMenuController>(true);
            while (button.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(button.onClick, 0);
            UnityEventTools.AddPersistentListener(button.onClick, controller.ShowHangar);
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("hangarButton").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Menu);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static Transform Find(Transform parent, string name)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    [MenuItem("Crocodilo/Wardrobe/Validate Hangar")]
    public static void Validate()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<SkinCatalog>(Catalog);
        if (catalog == null || Resources.Load<SkinCatalog>(SkinCatalog.ResourcePath) != catalog) throw new Exception("Skin catalog must load from Resources");
        if (catalog.Count(SkinSlot.Ship) != 3) throw new Exception("Expected three ship entries");
        var ids = new HashSet<string>();
        foreach (var s in catalog.Skins)
        {
            if (string.IsNullOrWhiteSpace(s.id) || !ids.Add(s.id)) throw new Exception("Skin ids must be unique");
            if (!string.IsNullOrEmpty(s.unlockAchievement) && AchievementCatalog.Get(s.unlockAchievement) == null)
                throw new Exception("Unknown unlock achievement: " + s.unlockAchievement);
            if (!s.comingSoon && (s.controller == null || s.previewFrames.Length == 0 || s.previewFrames.Length != s.previewDurations.Length))
                throw new Exception("Equippable skin needs a controller and preview: " + s.id);
            if (s.previewFrames.Any(f => f == null) || s.previewDurations.Any(d => d <= 0f)) throw new Exception("Invalid preview: " + s.id);
        }
        var original = catalog.Default(SkinSlot.Ship);
        if (original == null || original.id != "original" || AssetDatabase.GetAssetPath(original.controller) != OriginalController)
            throw new Exception("Default ship must be the original airplane");
        var crocs = catalog.Get("crocs");
        if (AssetDatabase.GetAssetPath(crocs.controller) != CrocsSkinTools.Controller ||
            ((AnimatorOverrideController)crocs.controller).runtimeAnimatorController != original.controller)
            throw new Exception("Crocs must override the original controller");
        // The prefab itself keeps the original; the skin is applied only at runtime by the player's choice.
        var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (AssetDatabase.GetAssetPath(player.GetComponent<Animator>().runtimeAnimatorController) != OriginalController)
            throw new Exception("Player prefab must keep the original controller");
        var menu = AssetDatabase.LoadAssetAtPath<GameObject>(Menu);
        var hangar = Find(menu.transform, "HangarButton");
        if (hangar == null || Find(menu.transform, "OnlineButton") != null) throw new Exception("Hangar button must replace the Online slot");
        var button = hangar.GetComponent<UnityEngine.UI.Button>();
        if (button.onClick.GetPersistentEventCount() != 1 || button.onClick.GetPersistentMethodName(0) != "ShowHangar")
            throw new Exception("Hangar button callback is invalid");
        var controller = menu.GetComponentInChildren<MainMenuController>(true);
        if (new SerializedObject(controller).FindProperty("hangarButton").objectReferenceValue != button) throw new Exception("Hangar focus reference missing");
        if (AssetDatabase.GetAssetPath(hangar.Find("Hangar").GetComponent<UnityEngine.UI.Image>().sprite) != Wordmark) throw new Exception("Hangar wordmark missing");
        Debug.Log("WARDROBE_OK: three ship entries (original default, Crocs equippable, locked teaser); menu Hangar button; prefab keeps original airplane.");
    }
}
