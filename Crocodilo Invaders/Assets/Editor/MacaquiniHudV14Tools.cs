using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MacaquiniHudV14Tools
{
    public const string Art = "Assets/Art/UI/MacaquiniHUD_v14/";
    private const string Prefab = "Assets/Prefabs/UI/BossHUD.prefab";
    public static void Install()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png"))
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100; imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.alphaIsTransparency = true; imp.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings(); imp.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; imp.SetTextureSettings(settings); imp.SaveAndReimport();
        }
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var previous = root.transform.Find("MacaquiniBar");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var rect = Rect("MacaquiniBar", root.transform, new Vector2(-16,14), new Vector2(500,167));
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1,0);
            var skin = rect.gameObject.AddComponent<MacaquiniBossBar>();
            var bar = rect.gameObject.AddComponent<BossBarFX>();
            var canvas = rect.gameObject.AddComponent<CanvasGroup>(); canvas.blocksRaycasts = canvas.interactable = false;
            var fields = new SerializedObject(skin);
            fields.FindProperty("entrance").objectReferenceValue = canvas;
            void Bind(string field, UnityEngine.Object obj) => fields.FindProperty(field).objectReferenceValue = obj;
            Image Make(string name, Sprite sprite, Vector2 pos, Vector2 size) => Image(name,rect,sprite,pos,size);
            Make("JungleFrameOne",Load("jungle_frame"),Vector2.zero,new Vector2(500,167));
            Bind("frameTwo",Make("JungleFrameTwo",Load("jungle_frame_half"),Vector2.zero,new Vector2(500,167)));
            Image Fill(string name, string field, Color color, float y = 56, float height = 20)
            {
                var image = Make(name, null, new Vector2(55,y), new Vector2(332,height));
                image.type = UnityEngine.UI.Image.Type.Filled; image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
                image.fillOrigin = 1; image.color = color; Bind(field,image); return image;
            }
            // Unity's white sprite supplies a rectangular continuous meter inside the painted track.
            var white = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Fill("hp_damage_lag","lag",new Color(.95f,.68f,.22f)).sprite = white;
            Fill("hp_fill","fill",new Color(.33f,.85f,.16f)).sprite = white;
            Fill("SapHighlight","sheen",Color.white,70,3).sprite = white;
            Bind("accent",Make("JungleAccent",white,new Vector2(65,51),new Vector2(312,1)));
            Make("name_text",Load("hud_title"),new Vector2(65,104),new Vector2(240,28));
            Bind("portraitOne",Make("emblem_roundel",Load("hud_portrait"),new Vector2(395,40),new Vector2(104,104)));
            Bind("portraitTwo",Make("EnragedPortrait",Load("hud_portrait_half"),new Vector2(395,40),new Vector2(104,104)));
            var label = Rect("FormLabel",rect,new Vector2(65,22),new Vector2(310,18)).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset");
            label.fontSize = 9; label.color = new Color(.91f,.86f,.59f); label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Left; label.text = "I  ·  CASCA INFECTADA";
            Bind("formLabel",label); fields.ApplyModifiedPropertiesWithoutUndo();
            var hud = new SerializedObject(root.GetComponent<BossHUD>());
            hud.FindProperty("macaquiniBar").objectReferenceValue = bar; hud.ApplyModifiedPropertiesWithoutUndo();
            rect.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root,Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets(); Validate();
    }
    public static void Validate()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var hud = new SerializedObject(root.GetComponent<BossHUD>());
        var bar = hud.FindProperty("macaquiniBar").objectReferenceValue as BossBarFX;
        Require(bar != null && bar.name == "MacaquiniBar", "Dedicated bar binding");
        var view = bar.GetComponent<MacaquiniBossBar>(); Require(view != null, "Dedicated jungle presenter");
        Require(view.FirstPortrait != view.SecondPortrait && view.PortraitSize == 104, "Distinct enlarged portraits");
        Require(root.transform.Find("GusiniBarF1").GetComponent<BossBarPhaseTwo>() != null,"Original Gusini preserved");
        Require(bar.GetComponentsInChildren<Image>(true).All(i=>!i.raycastTarget),"No input interception");
        var fields = new SerializedObject(view); var prop = fields.GetIterator();
        while(prop.NextVisible(true)) if(prop.propertyType == SerializedPropertyType.ObjectReference) Require(prop.objectReferenceValue != null,prop.propertyPath);
        Debug.Log("MACAQUINI_HUD_V14_INSTALL_OK distinct jungle forms, 104px portraits, continuous health");
    }
    public static void Build()
    {
        string output = Path.GetFullPath("Builds/Windows-Macaquini-HUD-v14/MacaquiniSelva.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes = new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes, locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_HUD_V14_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode) EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
    private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png") ?? throw new InvalidOperationException(name);
    private static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var r = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.gameObject.layer=5;
        r.SetParent(parent,false); r.anchorMin=r.anchorMax=r.pivot=Vector2.zero; r.anchoredPosition=pos; r.sizeDelta=size; return r;
    }
    private static Image Image(string name,Transform parent,Sprite sprite,Vector2 pos,Vector2 size)
    {var i=Rect(name,parent,pos,size).gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;return i;}
    private static void Require(bool condition,string message) {if(!condition)throw new InvalidOperationException("HUD_V14 "+message);}
}
