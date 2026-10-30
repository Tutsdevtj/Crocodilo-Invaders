using UnityEngine;

public sealed class LakeJourneyArt : ScriptableObject
{
    public Sprite[] enemies;
    public Sprite[] props;
    public Sprite Frame(int kind,int pose) => enemies[Mathf.Clamp(kind,0,3)*4+Mathf.Clamp(pose,0,3)];
}
