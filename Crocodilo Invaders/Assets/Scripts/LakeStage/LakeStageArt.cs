using UnityEngine;

// Scene reference to everything the lake draws, so nothing is looked up by a path in text.
public sealed class LakeStageArt : ScriptableObject
{
    public LakeTongueAssets tongue;
    public LakeTireArt tire;
    public LakeReworkArt rework;
    public LakeWildlifeAssets wildlife;
    public SapererecaAssets companion;
    public Material waterContact, hudWater;
    public Sprite waterDrop, burp, bossClosedEyes, hudFrame, hudClosedEyes;
}
