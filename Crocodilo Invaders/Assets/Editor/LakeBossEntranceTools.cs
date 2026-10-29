using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LakeBossEntranceTools
{
    public static void Install()
    {
        AssetDatabase.Refresh();
        var leaf=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/LakeStage/LilyPadsV16/lilypads.png").OfType<Sprite>().First(s=>s.name=="lilypad_small_notched");
        foreach(string path in new[]{LakeStageTools.Stage,LakeStageTools.Test})
        {
            var scene=EditorSceneManager.OpenScene(path);var waves=Object.FindAnyObjectByType<WaveController>();
            var intro=waves.GetComponent<LakeBossEntrance>();if(intro==null)intro=waves.gameObject.AddComponent<LakeBossEntrance>();
            JungleStageTools.Set(intro,"lily",leaf);
            JungleStageTools.Set(intro,"waterSound",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/LakeEntranceV17/water-swell.wav"));
            JungleStageTools.Set(intro,"croakSound",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/LakeEntranceV17/cururu-croak.wav"));
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LakeStageTools.Stage);Debug.Log("LAKE_BOSS_ENTRANCE_V17_INSTALLED");
    }
}
