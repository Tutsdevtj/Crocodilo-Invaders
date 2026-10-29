using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class LakeDistantBirdsTools
{
    public static void Install()
    {
        foreach(string path in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(path);var lake=Object.FindAnyObjectByType<LakeBackdrop>();
            if(lake.GetComponent<LakeDistantBirds>()==null)lake.gameObject.AddComponent<LakeDistantBirds>();
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);Debug.Log("LAKE_DISTANT_BIRDS_V18_INSTALLED");
    }
}
