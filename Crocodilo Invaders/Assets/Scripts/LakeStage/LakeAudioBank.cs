using UnityEngine;

// Every lake sound and its loudness, editable in the Inspector.
public sealed class LakeAudioBank : ScriptableObject
{
    [System.Serializable]
    public sealed class Entry
    {
        public string name;
        public AudioClip clip;
        [Range(0, 1)] public float volume = .4f;
        [Min(0)] public float cooldown = .05f;
        [Range(0, .3f)] public float pitchVariance = .05f;
    }
    public Entry[] sounds;
    public AudioClip ambience, mosquitoBuzz;
    [Range(0, 1)] public float ambienceVolume = .3f;
    [Range(0, 1), Tooltip("Volume do zumbido com três ou mais mosquitos vivos.")] public float mosquitoBuzzVolume = .1f;
}
