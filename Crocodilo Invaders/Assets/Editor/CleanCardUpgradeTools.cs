using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class CleanCardUpgradeTools
{
    public const string Prefab="Assets/Resources/RunUpgrades/RunUpgrades.prefab";
    static TMP_FontAsset font,heading;
    static readonly Color Ink=new Color(.96f,.94f,.81f),Muted=new Color(.76f,.82f,.77f);
    [MenuItem("Crocodilo/UI/Clean cards and install Tab status")]
    public static void Install()
    {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelifySans/PixelifySans Dialogue.asset");
        heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P SDF.asset");
        InstallCards();RemoveOldPauseShortcut();Directory.CreateDirectory(Path.GetDirectoryName(Prefab));AssetDatabase.Refresh();InstallTab();AssetDatabase.SaveAssets();
        Debug.Log("CLEAN_CARDS_INSTALL_OK: focused card comparison; transparent hold-Tab status; no pause or HUD shortcut.");
    }
    static void InstallCards()
    {
        const string file="Assets/Prefabs/UI/CardRewards.prefab";var root=PrefabUtility.LoadPrefabContents(file);
        try
        {
            var all=root.GetComponentsInChildren<Transform>(true);
            foreach(string name in new[]{"Hint","Controls","Totals","HeaderLine"})all.Single(t=>t.name==name).gameObject.SetActive(false);
            var title=all.Single(t=>t.name=="Title").GetComponent<TMP_Text>();title.text="ESCOLHA UMA MELHORIA";title.font=heading;title.fontSize=22;
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="Comparison"))
            {text.font=font;text.fontSize=18;text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,30);}
            var label=all.Single(t=>t.name=="Reroll").GetComponentInChildren<TMP_Text>(true);label.text="TROCAR CARTAS (1)";label.font=font;label.fontSize=19;
            PrefabUtility.SaveAsPrefabAsset(root,file);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void RemoveOldPauseShortcut()
    {
        const string file="Assets/Prefabs/UI/PauseMenu.prefab";var root=PrefabUtility.LoadPrefabContents(file);
        try
        {
            var old=root.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault(b=>b.name=="UpgradesButton");if(old==null)return;
            Object.DestroyImmediate(old.gameObject);
            var names=new[]{"ContinueButton","AchievementsButton","OptionsButton","ReturnButton"};
            var buttons=names.Select(n=>root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==n)).ToArray();
            for(int i=0;i<4;i++)
            {
                var rect=(RectTransform)buttons[i].transform;rect.sizeDelta=new Vector2(420,72);rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,100-i*80);
                var nav=buttons[i].navigation;nav.mode=UnityEngine.UI.Navigation.Mode.Explicit;nav.selectOnUp=buttons[(i+3)%4];nav.selectOnDown=buttons[(i+1)%4];buttons[i].navigation=nav;
            }
            PrefabUtility.SaveAsPrefabAsset(root,file);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void InstallTab()
    {
        bool existing=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab)!=null;
        var root=existing?PrefabUtility.LoadPrefabContents(Prefab):new GameObject("RunUpgrades",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
        try
        {
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=250;
            var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1200,720);scaler.matchWidthOrHeight=.5f;
            var raycaster=root.GetComponent<UnityEngine.UI.GraphicRaycaster>();if(raycaster!=null)raycaster.enabled=false;
            var group=root.GetComponent<CanvasGroup>();if(group==null)group=root.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            var view=root.GetComponent<RunUpgradeView>();if(view==null)view=root.AddComponent<RunUpgradeView>();
            var chip=root.transform.Find("HudUpgrades");if(chip!=null)chip.gameObject.SetActive(false);
            var content=Rect("Content",root.transform);Stretch(content);var dim=content.GetComponent<UnityEngine.UI.Image>();if(dim!=null)dim.enabled=false;
            var panel=Rect("Panel",content);Place(panel,new Vector2(900,380),Vector2.one*.5f,Vector2.zero);Image(panel,new Color(.025f,.055f,.07f,.18f));
            var outline=panel.GetComponent<UnityEngine.UI.Outline>();if(outline==null)outline=panel.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=new Color(.8f,.77f,.51f,.23f);outline.effectDistance=Vector2.one;
            foreach(string name in new[]{"Heading","Count","Close","History"}){var child=panel.Find(name);if(child!=null)child.gameObject.SetActive(false);}
            var portrait=Rect("Portrait",panel);Place(portrait,new Vector2(210,210),new Vector2(0,.5f),new Vector2(24,22));
            var image=Image(portrait,Color.white);image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Portraits/Crocodilo/crocodilo_05_determinado.png");image.preserveAspect=true;
            if(image.sprite==null)throw new Exception("Original Croc portrait required");
            Text("Name",panel,"CROC",18,Ink,new Vector2(210,30),new Vector2(0,.5f),new Vector2(24,-108),true).alignment=TextAlignmentOptions.Center;
            var count=Text("EquippedCount",panel,"",13,Muted,new Vector2(210,26),new Vector2(0,.5f),new Vector2(24,-140));count.alignment=TextAlignmentOptions.Center;
            var divider=Rect("Divider",panel);Place(divider,new Vector2(1,280),new Vector2(0,.5f),new Vector2(250,0));Image(divider,new Color(.8f,.77f,.51f,.23f));
            var stats=Rect("Stats",panel);Place(stats,new Vector2(600,326),new Vector2(0,.5f),new Vector2(274,0));
            foreach(string name in new[]{"AttributeHeading","ValuesHeading","BonusHeading"}){var child=stats.Find(name);if(child!=null)child.gameObject.SetActive(false);}
            var values=new TMP_Text[5];var segments=new UnityEngine.UI.Image[100];
            for(int row=0;row<5;row++)
            {
                var r=Rect("Stat"+row,stats);Place(r,new Vector2(600,58),new Vector2(0,1),new Vector2(0,-row*66));Image(r,new Color(.025f,.055f,.07f,.46f));
                Text("Name",r,UpgradeText.Name((UpgradeKind)row).ToUpperInvariant(),16,Ink,new Vector2(190,24),new Vector2(0,1),Vector2.zero);
                values[row]=Text("Value",r,"",16,Ink,new Vector2(390,24),Vector2.one,Vector2.zero);values[row].alignment=TextAlignmentOptions.Right;
                var bonus=r.Find("Bonus");if(bonus!=null)bonus.gameObject.SetActive(false);
                var bar=Rect("Bar",r);Place(bar,new Vector2(600,20),Vector2.zero,new Vector2(0,3));
                for(int i=0;i<20;i++)
                {
                    var pip=Rect("Pip"+i,bar);Place(pip,new Vector2(26,17),Vector2.zero,new Vector2(i*30,0));segments[row*20+i]=Image(pip,new Color(.68f,.75f,.7f,.14f));
                    var edge=pip.GetComponent<UnityEngine.UI.Outline>();if(edge==null)edge=pip.gameObject.AddComponent<UnityEngine.UI.Outline>();edge.effectColor=new Color(.02f,.035f,.03f,.45f);edge.effectDistance=Vector2.one;
                }
            }
            Set(view,"content",content.gameObject);Set(view,"panel",panel);Set(view,"count",count);SetArray(view,"values",values);SetArray(view,"segments",segments);
            content.gameObject.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,Prefab);
        }
        finally{if(existing)PrefabUtility.UnloadPrefabContents(root);else Object.DestroyImmediate(root);}
    }
    static RectTransform Rect(string name,Transform parent){var found=parent.Find(name);if(found!=null){found.gameObject.SetActive(true);return (RectTransform)found;}var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
    static void Place(RectTransform r,Vector2 size,Vector2 anchor,Vector2 pos){r.anchorMin=r.anchorMax=r.pivot=anchor;r.sizeDelta=size;r.anchoredPosition=pos;r.localScale=Vector3.one;}
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    static UnityEngine.UI.Image Image(RectTransform r,Color color){var image=r.GetComponent<UnityEngine.UI.Image>();if(image==null)image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.enabled=true;image.color=color;image.raycastTarget=false;return image;}
    static TMP_Text Text(string name,Transform parent,string value,float size,Color color,Vector2 dimensions,Vector2 anchor,Vector2 pos,bool title=false)
    {
        var r=Rect(name,parent);Place(r,dimensions,anchor,pos);var text=r.GetComponent<TMP_Text>();if(text==null)text=r.gameObject.AddComponent<TextMeshProUGUI>();text.font=title?heading:font;text.text=value;text.fontSize=size;text.color=color;text.raycastTarget=false;text.alignment=TextAlignmentOptions.MidlineLeft;text.textWrappingMode=TextWrappingModes.NoWrap;text.overflowMode=TextOverflowModes.Ellipsis;return text;
    }
    static void Set(Object target,string name,Object value){var s=new SerializedObject(target);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void SetArray<T>(Object target,string name,T[] value)where T:Object{var s=new SerializedObject(target);var a=s.FindProperty(name);a.arraySize=value.Length;for(int i=0;i<value.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=value[i];s.ApplyModifiedPropertiesWithoutUndo();}
}
