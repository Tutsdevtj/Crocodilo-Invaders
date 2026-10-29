using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LakeLilyPadStageTools
{
    public static void Rebuild(){LakeLilyPadImport.Install();Install();}
    public static void Install()
    {
        AssetDatabase.Refresh();
        var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/LakeStage/LilyPadsV16/lilypads.png").OfType<Sprite>().ToArray();
        string[] names={"lilypad_small_notched","lilypad_large_rim","lilypad_nibbled","lilypad_pink_bud","lilypad_pink_bloom","lilypad_double_cluster"};
        if(sprites.Length!=6)throw new System.InvalidOperationException("Import lily pad sprites first");
        foreach(string path in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(path);var lake=Object.FindAnyObjectByType<LakeBackdrop>();
            var pads=lake.GetComponent<LakeLilyPads>();if(pads==null)pads=lake.gameObject.AddComponent<LakeLilyPads>();
            var data=new SerializedObject(pads);var array=data.FindProperty("sprites");array.arraySize=6;
            for(int i=0;i<6;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=sprites.Single(s=>s.name==names[i]);
            data.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);Debug.Log("LAKE_LILYPADS_V16_STAGE_INSTALLED");
    }
}
