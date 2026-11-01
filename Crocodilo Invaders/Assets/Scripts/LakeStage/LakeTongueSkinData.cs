using UnityEngine;

public sealed class LakeTongueSkinData : ScriptableObject
{
    public Sprite drawing;
    public Material material;
    public float rootUv=.84f, tipUv=.24f;
    public float height=.38f, rootLength=.32f, tipLength=.48f;
}
