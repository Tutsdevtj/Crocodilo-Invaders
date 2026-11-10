using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AchievementTools
{
    public const string Folder = "Assets/Resources/Achievements";
    public const string Prefab = Folder + "/Achievements.prefab";
    static readonly Color Navy = new Color(.045f, .085f, .12f), Ink = new Color(.94f, .92f, .80f), Gold = new Color(.79f, .65f, .34f), Muted = new Color(.58f, .68f, .69f);
    static TMP_FontAsset bodyFont, titleFont;
    static Sprite plate, pixel;

    [MenuItem("Crocodilo/Achievements/Install")]
    public static void Install()
    {
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelifySans/PixelifySans Dialogue.asset");
        titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset");
        if (bodyFont == null || titleFont == null) throw new Exception("Missing project fonts");
        CreatePlate();
        const string pixelPath = Folder + "/pixel.png";
        if (!File.Exists(pixelPath))
        {
            var texture = new Texture2D(1,1,TextureFormat.RGBA32,false); texture.SetPixel(0,0,Color.white); texture.Apply();
            File.WriteAllBytes(pixelPath,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        }
        AssetDatabase.ImportAsset(pixelPath);
        var pixelImporter=(TextureImporter)AssetImporter.GetAtPath(pixelPath);pixelImporter.textureType=TextureImporterType.Sprite;
        pixelImporter.filterMode=FilterMode.Point;pixelImporter.mipmapEnabled=false;pixelImporter.textureCompression=TextureImporterCompression.Uncompressed;pixelImporter.SaveAndReimport();
        pixel=AssetDatabase.LoadAssetAtPath<Sprite>(pixelPath);
        string soundPath = Folder + "/achievement_unlock.wav";
        var importer = AssetImporter.GetAtPath(soundPath) as AudioImporter;
        if (importer == null) throw new Exception("Generate achievement_unlock.wav first");
        var sample = importer.defaultSampleSettings; sample.loadType = AudioClipLoadType.DecompressOnLoad;
        sample.compressionFormat = AudioCompressionFormat.PCM; sample.preloadAudioData = true;
        importer.defaultSampleSettings = sample; importer.forceToMono = true; importer.SaveAndReimport();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null) CreatePrefab();
        var feature = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            SetArray(feature.GetComponent<AchievementView>(), "icons", LoadIcons());
            // Pixelify's 22px line is 26.4px high; give titles enough vertical room.
            foreach(var row in feature.GetComponentsInChildren<AchievementRow>(true))
            {
                var title=row.transform.Find("Name").GetComponent<TMP_Text>();
                title.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,32);
                var pos=title.rectTransform.anchoredPosition;pos.y=-4;title.rectTransform.anchoredPosition=pos;
            }
            ConfigureCompactToast(feature);
            PrefabUtility.SaveAsPrefabAsset(feature, Prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(feature); }
        InstallMenu(); InstallPause(); AssetDatabase.SaveAssets(); Validate();
    }
    [MenuItem("Crocodilo/Achievements/Compact popup")]
    public static void CompactPopup()
    {
        var feature=PrefabUtility.LoadPrefabContents(Prefab);
        try { ConfigureCompactToast(feature); PrefabUtility.SaveAsPrefabAsset(feature,Prefab); }
        finally { PrefabUtility.UnloadPrefabContents(feature); }
        AssetDatabase.SaveAssets();
        Debug.Log("ACHIEVEMENT_COMPACT_POPUP_OK: 300x68, 40px icon, flat background, two text lines.");
    }
    static void ConfigureCompactToast(GameObject feature)
    {
        var rect=(RectTransform)feature.transform.Find("UnlockNotification");
        Position(rect,new Vector2(300,68),new Vector2(1,1),new Vector2(-12,-12));
        var background=rect.GetComponent<UnityEngine.UI.Image>();
        background.sprite=null;background.type=UnityEngine.UI.Image.Type.Simple;
        background.color=new Color(.095f,.115f,.14f,.98f);background.raycastTarget=false;
        var icon=rect.Find("Icon").GetComponent<UnityEngine.UI.Image>();
        Position(icon.rectTransform,new Vector2(40,40),new Vector2(0,.5f),new Vector2(12,0));
        var label=rect.Find("UnlockedLabel").GetComponent<TMP_Text>();
        Position(label.rectTransform,new Vector2(224,18),new Vector2(0,1),new Vector2(64,-9));
        label.text="Conquista desbloqueada";label.fontSize=12;label.color=new Color(.61f,.67f,.72f);
        var title=rect.Find("Name").GetComponent<TMP_Text>();
        Position(title.rectTransform,new Vector2(224,28),new Vector2(0,1),new Vector2(64,-27));
        title.fontSize=18;title.color=new Color(.94f,.95f,.96f);
        rect.Find("Description").gameObject.SetActive(false);
    }
    static void CreatePlate()
    {
        const string file = Folder + "/military_plate.png";
        if (!File.Exists(file))
        {
            var texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++)
            {
                int edge = Mathf.Min(x, y, 23 - x, 23 - y);
                bool clipped = (x < 3 && y < 3) || (x > 20 && y < 3) || (x < 3 && y > 20) || (x > 20 && y > 20);
                Color c = clipped ? Color.clear : edge == 0 ? new Color(.035f,.045f,.06f) : edge == 1 ? Gold : edge == 2 ? new Color(.24f,.28f,.25f) : Navy;
                if ((x == 5 || x == 18) && (y == 5 || y == 18)) c = new Color(.71f,.72f,.57f);
                texture.SetPixel(x,y,c);
            }
            texture.Apply(); File.WriteAllBytes(file, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        }
        AssetDatabase.ImportAsset(file);
        var importer = (TextureImporter)AssetImporter.GetAtPath(file);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(7,7,7,7); importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        plate = AssetDatabase.LoadAssetAtPath<Sprite>(file);
    }
    static void CreatePrefab()
    {
        var root = new GameObject("Achievements", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 3500; canvas.pixelPerfect = true;
        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200,720); scaler.matchWidthOrHeight = .5f;
        root.AddComponent<AchievementRuntime>(); root.AddComponent<AchievementTracker>();
        var view = root.AddComponent<AchievementView>(); var source = root.AddComponent<AudioSource>(); source.playOnAwake = false;
        try
        {
            var panel = Rect("Panel", root.transform); Stretch(panel,0,0,0,0);
            Image(panel, new Color(.015f,.028f,.045f,.94f), true);
            var frame = Box("MedalBoard", panel, new Vector2(1000,620), Vector2.one*.5f, Vector2.zero, true);
            Stretch(frame,44,34,44,34);
            Text("Eyebrow",frame,"REGISTRO DE SERVIÇO",14,Muted,new Vector2(500,24),new Vector2(0,1),new Vector2(28,-22));
            Text("Title",frame,"CONQUISTAS",22,Ink,new Vector2(640,36),new Vector2(0,1),new Vector2(28,-50),true);
            var summary = Text("Summary",frame,"0 / 10 CONQUISTAS DESBLOQUEADAS",18,Ink,new Vector2(620,26),new Vector2(0,1),new Vector2(28,-94));
            var close = Button("Close",frame,"VOLTAR",new Vector2(150,44),new Vector2(1,1),new Vector2(-24,-24));
            var meter = Meter("Completion",frame,new Vector2(0,1),new Vector2(28,-126),new Vector2(440,5));
            var filters = new UnityEngine.UI.Button[3];
            for(int i=0;i<3;i++)filters[i]=Button("Filter"+i,frame,new[]{"TODAS","DESBLOQUEADAS","PENDENTES"}[i],new Vector2(i==1?210:150,34),new Vector2(0,1),new Vector2(28+(i==0?0:i==1?162:384),-143),15);
            var list = Rect("Rows",frame); Stretch(list,28,188,28,84);
            var layout = list.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 10; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            var rows = new AchievementRow[4];
            for(int i=0;i<4;i++) rows[i]=CreateRow(list,i);
            var empty = Text("Empty",list,"",23,Muted,new Vector2(600,120),Vector2.one*.5f,Vector2.zero);
            var ignore = empty.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); ignore.ignoreLayout=true;
            empty.alignment=TextAlignmentOptions.Center;
            var prev = Button("Previous",frame,"< ANTERIOR",new Vector2(170,38),new Vector2(0,0),new Vector2(28,30),16);
            var next = Button("Next",frame,"PRÓXIMA >",new Vector2(170,38),new Vector2(1,0),new Vector2(-28,30),16);
            var pagination=Text("Pagination",frame,"PÁGINA 1 / 3",16,Ink,new Vector2(240,24),new Vector2(.5f,0),new Vector2(0,42)); pagination.alignment=TextAlignmentOptions.Center;
            var status=Text("SaveStatus",frame,"PROGRESSO SALVO NESTE COMPUTADOR",12,Muted,new Vector2(580,18),new Vector2(.5f,0),new Vector2(0,14));status.alignment=TextAlignmentOptions.Center;
            var toast=Box("UnlockNotification",root.transform,new Vector2(390,104),new Vector2(1,0),new Vector2(-20,20),false);
            var toastGroup=toast.gameObject.AddComponent<CanvasGroup>(); toastGroup.alpha=0;toastGroup.blocksRaycasts=false;
            var icon=Icon("Icon",toast,new Vector2(64,64),new Vector2(0,.5f),new Vector2(16,0));
            Text("UnlockedLabel",toast,"CONQUISTA DESBLOQUEADA",13,Gold,new Vector2(284,20),new Vector2(0,1),new Vector2(90,-13));
            var toastTitle=Text("Name",toast,"",22,Ink,new Vector2(280,26),new Vector2(0,1),new Vector2(90,-35));
            var toastDescription=Text("Description",toast,"",16,Muted,new Vector2(280,34),new Vector2(0,1),new Vector2(90,-63));
            Set(view,"panel",panel.gameObject); SetArray(view,"rows",rows); SetArray(view,"icons",LoadIcons());
            Set(view,"closeButton",close);Set(view,"previousButton",prev);Set(view,"nextButton",next);SetArray(view,"filterButtons",filters);
            Set(view,"completionFill",meter);Set(view,"summary",summary);Set(view,"pagination",pagination);Set(view,"empty",empty);Set(view,"saveStatus",status);
            Set(view,"toast",toast);Set(view,"toastGroup",toastGroup);Set(view,"toastIcon",icon);Set(view,"toastTitle",toastTitle);Set(view,"toastDescription",toastDescription);
            Set(view,"audioSource",source);Set(view,"unlockSound",AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/achievement_unlock.wav"));
            PrefabUtility.SaveAsPrefabAsset(root,Prefab);
        }
        finally { Object.DestroyImmediate(root); }
    }
    static AchievementRow CreateRow(Transform parent,int index)
    {
        var rect=Rect("MedalRow"+index,parent);rect.sizeDelta=new Vector2(900,82);
        Image(rect,new Color(.07f,.12f,.16f));
        var element=rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.minHeight=element.preferredHeight=82;
        var accent=Rect("Accent",rect);Stretch(accent,0,0,0,0);accent.anchorMax=new Vector2(0,1);accent.sizeDelta=new Vector2(3,0);accent.anchoredPosition=new Vector2(1.5f,0);
        var accentImage=Image(accent,Muted);
        var icon=Icon("Icon",rect,new Vector2(62,62),new Vector2(0,.5f),new Vector2(14,0));
        var secret=Text("Secret",rect,"?",38,Muted,new Vector2(62,62),new Vector2(0,.5f),new Vector2(14,0),true);secret.alignment=TextAlignmentOptions.Center;
        var title=Text("Name",rect,"",22,Ink,new Vector2(610,26),new Vector2(0,1),new Vector2(94,-9));
        var desc=Text("Description",rect,"",18,Muted,new Vector2(650,25),new Vector2(0,1),new Vector2(94,-35));
        var status=Text("Status",rect,"",12,Muted,new Vector2(530,18),new Vector2(0,1),new Vector2(94,-60));
        var progress=Text("Progress",rect,"",14,Ink,new Vector2(135,22),new Vector2(1,.5f),new Vector2(-16,8));progress.alignment=TextAlignmentOptions.Right;
        var fill=Meter("ProgressBar",rect,new Vector2(1,.5f),new Vector2(-16,-15),new Vector2(125,5));
        // Text stretches up to the progress column, preserving readability at 800x600.
        foreach(var text in new[]{title,desc,status}){var r=text.rectTransform;r.anchorMax=new Vector2(1,1);r.offsetMax=new Vector2(-165,r.offsetMax.y);}
        var row=rect.gameObject.AddComponent<AchievementRow>();
        Set(row,"icon",icon);Set(row,"accent",accentImage);Set(row,"progressFill",fill);Set(row,"title",title);Set(row,"description",desc);Set(row,"progress",progress);Set(row,"status",status);Set(row,"secretMark",secret.gameObject);Set(row,"progressRoot",fill.transform.parent.gameObject);
        return row;
    }
    static Sprite[] LoadIcons()
    {
        return AchievementCatalog.All.Select(definition =>
        {
            string path=AchievementIconTools.Icons+"/"+definition.Id+".png";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)throw new Exception("Missing achievement-specific icon: "+path);
            return sprite;
        }).ToArray();
    }
    static void InstallMenu()
    {
        const string file="Assets/Prefabs/UI/MainMenu.prefab";var root=PrefabUtility.LoadPrefabContents(file);
        try
        {
            var controller=root.GetComponent<MainMenuController>();
            var existing=root.transform.Find("AchievementsButton");
            var button=existing!=null?existing.GetComponent<UnityEngine.UI.Button>():Button("AchievementsButton",root.transform,"CONQUISTAS",new Vector2(240,52),Vector2.one,new Vector2(-24,-24),18);
            AchievementMenuButtonTools.ApplyToMenu(root);
            button.onClick.RemoveAllListeners();
            for(int i=button.onClick.GetPersistentEventCount()-1;i>=0;i--)UnityEventTools.RemovePersistentListener(button.onClick,i);
            UnityEventTools.AddPersistentListener(button.onClick,controller.ShowAchievements);
            Set(controller,"achievementsButton",button);PrefabUtility.SaveAsPrefabAsset(root,file);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void InstallPause()
    {
        const string file="Assets/Prefabs/UI/PauseMenu.prefab";var root=PrefabUtility.LoadPrefabContents(file);
        try
        {
            var view=root.GetComponent<PauseMenuView>();var serialized=new SerializedObject(view);
            var page=((GameObject)serialized.FindProperty("mainPage").objectReferenceValue).transform;
            var existing=page.Find("AchievementsButton");
            var button=existing!=null?existing.GetComponent<UnityEngine.UI.Button>():Button("AchievementsButton",page,"CONQUISTAS",new Vector2(420,72),Vector2.one*.5f,new Vector2(0,20),22);
            for(int i=button.onClick.GetPersistentEventCount()-1;i>=0;i--)UnityEventTools.RemovePersistentListener(button.onClick,i);
            UnityEventTools.AddPersistentListener(button.onClick,view.ShowAchievements);
            foreach(var pair in new[]{("ContinueButton",100f),("OptionsButton",-60f),("ReturnButton",-140f)})
            {var found=root.GetComponentsInChildren<RectTransform>(true).First(r=>r.name==pair.Item1);var p=found.anchoredPosition;p.y=pair.Item2;found.anchoredPosition=p;}
            PrefabUtility.SaveAsPrefabAsset(root,file);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static RectTransform Rect(string name,Transform parent)
    {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
    static void Position(RectTransform r,Vector2 size,Vector2 anchor,Vector2 pos)
    {r.anchorMin=r.anchorMax=r.pivot=anchor;r.sizeDelta=size;r.anchoredPosition=pos;}
    static void Stretch(RectTransform r,float left,float top,float right,float bottom)
    {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=Vector2.one*.5f;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
    static UnityEngine.UI.Image Image(RectTransform r,Color color,bool raycast=false)
    {var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=raycast;return i;}
    static RectTransform Box(string name,Transform parent,Vector2 size,Vector2 anchor,Vector2 pos,bool raycast)
    {var r=Rect(name,parent);Position(r,size,anchor,pos);var i=Image(r,Color.white,raycast);i.sprite=plate;i.type=UnityEngine.UI.Image.Type.Sliced;return r;}
    static TMP_Text Text(string name,Transform parent,string value,float size,Color color,Vector2 dimensions,Vector2 anchor,Vector2 pos,bool heading=false)
    {var r=Rect(name,parent);Position(r,dimensions,anchor,pos);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=heading?titleFont:bodyFont;t.fontSize=size;t.color=color;t.text=value;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;t.overflowMode=TextOverflowModes.Ellipsis;return t;}
    static UnityEngine.UI.Image Icon(string name,Transform parent,Vector2 size,Vector2 anchor,Vector2 pos)
    {var r=Rect(name,parent);Position(r,size,anchor,pos);var i=Image(r,Color.white);i.preserveAspect=true;return i;}
    static UnityEngine.UI.Button Button(string name,Transform parent,string label,Vector2 size,Vector2 anchor,Vector2 pos,float fontSize=18)
    {
        var r=Box(name,parent,size,anchor,pos,true);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();
        var c=b.colors;c.highlightedColor=new Color(1f,.91f,.64f);c.selectedColor=new Color(1f,.91f,.64f);c.pressedColor=new Color(.62f,.73f,.58f);b.colors=c;
        var text=Text("Label",r,label,fontSize,Ink,size,Vector2.one*.5f,Vector2.zero);Stretch(text.rectTransform,10,4,10,4);text.alignment=TextAlignmentOptions.Center;
        return b;
    }
    static UnityEngine.UI.Image Meter(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size)
    {var r=Rect(name,parent);Position(r,size,anchor,pos);Image(r,new Color(.16f,.22f,.25f));var f=Rect("Fill",r);Stretch(f,0,0,0,0);var i=Image(f,Gold);i.sprite=pixel;i.type=UnityEngine.UI.Image.Type.Filled;i.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;i.fillAmount=0;return i;}
    static void Set(Object target,string property,Object value)
    {var so=new SerializedObject(target);so.FindProperty(property).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static void SetArray<T>(Object target,string property,T[] values) where T:Object
    {var so=new SerializedObject(target);var p=so.FindProperty(property);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
    public static void Validate()
    {
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        if(root==null||root.GetComponent<AchievementRuntime>()==null||root.GetComponent<AchievementView>()==null||root.GetComponent<AchievementTracker>()==null)throw new Exception("Missing achievement runtime");
        if(root.GetComponentsInChildren<AchievementRow>(true).Length!=4)throw new Exception("Expected four paginated rows");
        var icons=new SerializedObject(root.GetComponent<AchievementView>()).FindProperty("icons");
        foreach(var definition in AchievementCatalog.All)
        {
            bool found=false;
            for(int i=0;i<icons.arraySize;i++)found|=icons.GetArrayElementAtIndex(i).objectReferenceValue?.name==definition.Id;
            if(!found)throw new Exception("Missing icon named "+definition.Id);
        }
        foreach(var component in root.GetComponentsInChildren<Component>(true))if(component==null)throw new Exception("Missing script");
        Debug.Log("ACHIEVEMENTS_INSTALL_OK: persistent UI, menu/pause buttons, audio and gameplay tracker.");
    }
}
