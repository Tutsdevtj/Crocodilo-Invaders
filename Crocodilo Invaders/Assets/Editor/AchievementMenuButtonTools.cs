using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class AchievementMenuButtonTools
{
    public const string Artwork="Assets/Art/AchievementsMenu/achievements_medal.png";
    public const string Menu="Assets/Prefabs/UI/MainMenu.prefab";
    [MenuItem("Crocodilo/Achievements/Use illustrated menu medal")]
    public static void Install()
    {
        AssetDatabase.ImportAsset(Artwork);
        var importer=AssetImporter.GetAtPath(Artwork) as TextureImporter;
        if(importer==null)throw new Exception("Achievement medal artwork is missing");
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        var root=PrefabUtility.LoadPrefabContents(Menu);
        try { ApplyToMenu(root);PrefabUtility.SaveAsPrefabAsset(root,Menu); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();Validate();
    }
    public static void ApplyToMenu(GameObject root)
    {
        var achievement=root.transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        var leaderboard=root.transform.Find("LeaderboardButton").GetComponent<UnityEngine.UI.Button>();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Artwork);
        if(sprite==null)throw new Exception("Achievement medal sprite is missing");
        var rect=(RectTransform)achievement.transform;var reference=(RectTransform)leaderboard.transform;
        rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;
        rect.sizeDelta=reference.sizeDelta;rect.anchoredPosition=new Vector2(-reference.anchoredPosition.x,reference.anchoredPosition.y);
        var image=achievement.GetComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Simple;
        image.preserveAspect=true;image.color=Color.white;image.raycastTarget=true;
        foreach(var label in achievement.GetComponentsInChildren<TMP_Text>(true))label.gameObject.SetActive(false);
        achievement.targetGraphic=image;achievement.colors=leaderboard.colors;achievement.transition=leaderboard.transition;
        var group=achievement.GetComponent<CanvasGroup>();if(group==null)group=achievement.gameObject.AddComponent<CanvasGroup>();
        group.alpha=1;group.interactable=group.blocksRaycasts=true;
        var intro=new SerializedObject(root.GetComponent<MenuIntro>());var groups=intro.FindProperty("buttonGroups");
        bool included=false;for(int i=0;i<groups.arraySize;i++)if(groups.GetArrayElementAtIndex(i).objectReferenceValue==group)included=true;
        if(!included){int index=groups.arraySize;groups.arraySize++;groups.GetArrayElementAtIndex(index).objectReferenceValue=group;intro.ApplyModifiedPropertiesWithoutUndo();}
        var play=root.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name=="PlayButton");
        var quit=root.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name=="QuitButton");
        var nav=achievement.navigation;nav.mode=UnityEngine.UI.Navigation.Mode.Explicit;
        nav.selectOnLeft=null;nav.selectOnRight=play;nav.selectOnUp=quit;nav.selectOnDown=play;achievement.navigation=nav;
        nav=leaderboard.navigation;nav.selectOnLeft=play;leaderboard.navigation=nav;
        foreach(var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(button==achievement||button==leaderboard)continue;
            nav=button.navigation;nav.selectOnLeft=achievement;nav.selectOnRight=leaderboard;button.navigation=nav;
        }
    }
    public static void Validate()
    {
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(Menu);
        var achievement=root.transform.Find("AchievementsButton").GetComponent<UnityEngine.UI.Button>();
        var leaderboard=root.transform.Find("LeaderboardButton").GetComponent<UnityEngine.UI.Button>();
        var a=(RectTransform)achievement.transform;var b=(RectTransform)leaderboard.transform;
        if(a.anchorMin!=Vector2.zero||a.anchorMax!=Vector2.zero||a.anchoredPosition.y!=b.anchoredPosition.y||a.anchoredPosition.x<=0)
            throw new Exception("Achievement medal must sit in the lower-left corner, aligned with the leaderboard");
        if(AssetDatabase.GetAssetPath(achievement.GetComponent<UnityEngine.UI.Image>().sprite)!=Artwork||achievement.onClick.GetPersistentMethodName(0)!="ShowAchievements")
            throw new Exception("Achievement artwork or callback is invalid");
        var groups=new SerializedObject(root.GetComponent<MenuIntro>()).FindProperty("buttonGroups");int count=0;
        for(int i=0;i<groups.arraySize;i++)if(groups.GetArrayElementAtIndex(i).objectReferenceValue==achievement.GetComponent<CanvasGroup>())count++;
        if(count!=1)throw new Exception("Achievement medal must join the menu entrance exactly once");
        Debug.Log("ACHIEVEMENT_MENU_MEDAL_OK: illustrated lower-left medal, entrance, focus and callback verified.");
    }
}
