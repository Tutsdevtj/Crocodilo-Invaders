using UnityEditor;
using UnityEngine;

public static class LakeTongueFieldTools
{
    public static void Apply()
    {
        var root=PrefabUtility.LoadPrefabContents(LakeStageTools.BossPath);
        try
        {
            var serialized=new SerializedObject(root.GetComponent<LakeTongueAttack>());
            var points=serialized.FindProperty("stationPositions");points.arraySize=5;
            Vector2[] positions={new Vector2(.13f,.66f),new Vector2(.27f,.48f),new Vector2(.36f,.58f),new Vector2(.46f,.44f),new Vector2(.51f,.51f)};
            for(int i=0;i<positions.Length;i++)points.GetArrayElementAtIndex(i).vector2Value=positions[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,LakeStageTools.BossPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();Debug.Log("LAKE_FLY_FIELD_V10_APPLIED");
    }
}
