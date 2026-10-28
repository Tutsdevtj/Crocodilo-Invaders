using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MacaquiniScaleV16Tools
{
    [Serializable] private class Cel {public string form,path;public float ppu;public Vector2 pivot,muzzle,vine;}
    [Serializable] private class Layout {public Cel[] entries;}
    public static void Install()
    {
        var entries=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/MacaquiniScale_v16/registration.json")).entries;
        var root=PrefabUtility.LoadPrefabContents(JungleStageTools.BossPath);
        try
        {
            var fields=new SerializedObject(root.GetComponent<MacaquiniFlightPresentation>());
            foreach(var group in entries.GroupBy(c=>c.form))
            {
                var sprites=fields.FindProperty(group.Key=="transform"?"transformation":group.Key);
                var muzzles=group.Key=="flight"?fields.FindProperty("flightMuzzles"):group.Key=="swinging"?fields.FindProperty("swingMuzzles"):null;
                var vines=group.Key=="transform"?fields.FindProperty("transformationVines"):group.Key=="swinging"?fields.FindProperty("swingVines"):null;
                int i=0;
                foreach(var cel in group)
                {
                    var imp=(TextureImporter)AssetImporter.GetAtPath(cel.path);imp.spritePixelsPerUnit=cel.ppu;
                    var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;
                    settings.spritePivot=new Vector2(cel.pivot.x/512,1-cel.pivot.y/512);imp.SetTextureSettings(settings);imp.SaveAndReimport();
                    sprites.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(cel.path);
                    if(muzzles!=null)muzzles.GetArrayElementAtIndex(i).vector2Value=cel.muzzle;
                    if(vines!=null)vines.GetArrayElementAtIndex(i).vector2Value=cel.vine;
                    i++;
                }
            }
            fields.FindProperty("uniformSpriteScale").boolValue=true;fields.ApplyModifiedPropertiesWithoutUndo();
            root.transform.Find("CompletePaintedBody").localScale=Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(root,JungleStageTools.BossPath);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();Validate();
    }
    public static void Validate()
    {
        var entries=JsonUtility.FromJson<Layout>(File.ReadAllText("SourceArt/MacaquiniScale_v16/registration.json")).entries;
        foreach(var c in entries)if(Mathf.Abs(AssetDatabase.LoadAssetAtPath<Sprite>(c.path).pixelsPerUnit-80)>.001f)throw new InvalidOperationException(c.path);
        if(entries.Length!=98)throw new InvalidOperationException("Incomplete animation registration");
        Debug.Log("MACAQUINI_SCALE_V16_INSTALL_OK 98 full-body sprites at 80 PPU, common foot anchor, fixed body scale");
    }
    public static void Build()
    {
        string output=Path.GetFullPath("Builds/Windows-Macaquini-Escala-v16/MacaquiniSelva.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var scenes=new[]{MacaquiniJetpackTools.Test}.Concat(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)).Distinct().ToArray();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        Debug.Log("MACAQUINI_SCALE_V16_BUILD "+report.summary.result+" "+report.summary.totalSize);
        if(Application.isBatchMode)EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
    }
}
