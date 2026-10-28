using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniHudV15Tools
{
    public static void Install()
    {
        const string path="Assets/Prefabs/UI/BossHUD.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var bar=root.transform.Find("MacaquiniBar");
            if(bar==null)throw new InvalidOperationException("Missing existing jungle HUD");
            foreach(string name in new[]{"JungleFrameOne","JungleFrameTwo"})
            {
                var frame=bar.Find(name);if(frame==null)throw new InvalidOperationException(name);
                if(frame.GetComponent<MacaquiniFrameAperture>()==null)frame.gameObject.AddComponent<MacaquiniFrameAperture>();
            }
            var window=bar.Find("PortraitWindow") as RectTransform;
            if(window==null){window=new GameObject("PortraitWindow",typeof(RectTransform),typeof(UnityEngine.UI.RectMask2D)).GetComponent<RectTransform>();window.gameObject.layer=5;window.SetParent(bar,false);}
            window.anchorMin=window.anchorMax=window.pivot=Vector2.zero;window.anchoredPosition=new Vector2(413,63);window.sizeDelta=new Vector2(56,52);
            foreach(string name in new[]{"emblem_roundel","EnragedPortrait"})
            {
                var portrait=bar.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i=>i.name==name).rectTransform;
                portrait.SetParent(window,false);portrait.anchoredPosition=new Vector2(-23,-26);
            }
            window.SetSiblingIndex(0);
            void Meter(string name,float y,float height)
            {
                var image=bar.Find(name).GetComponent<UnityEngine.UI.Image>();
                image.rectTransform.anchoredPosition=new Vector2(57,y);image.rectTransform.sizeDelta=new Vector2(337,height);
                image.type=UnityEngine.UI.Image.Type.Filled;image.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;image.fillOrigin=0;
            }
            Meter("hp_fill",57,18);Meter("hp_damage_lag",57,18);Meter("SapHighlight",72,2);Meter("JungleAccent",57,2);
            bar.Find("hp_damage_lag").GetComponent<UnityEngine.UI.Image>().color=new Color(.29f,.32f,.16f,.45f);
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();
        Debug.Log("MACAQUINI_HUD_V15_INSTALL_OK portrait behind apertured frame; inset left-to-right continuous meter");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-HUD-v15/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_HUD_V15_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
