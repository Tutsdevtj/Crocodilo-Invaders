using System;
using UnityEngine;

public enum GusiniSound
{
    Engine, Cannon, Cock, Whistle, Command, Flyby, HonkLow, HonkMid, HonkHigh,
    HatchLatch, HatchOpen, EggDrop, HatchClose, Deploy, Flutter, Crack1, Crack2, Burst, YolkWhoosh, Hit
}
public enum GusiniSoundGroup { Ambient, Mechanism, Flyby, Chorus, Damage, Telegraph, Danger }

[CreateAssetMenu(menuName="Crocodilo/Gusini Audio")]
public sealed class GusiniAudioConfig : ScriptableObject
{
    [Serializable]
    public sealed class Sound
    {
        public GusiniSound id;
        public GusiniSoundGroup group;
        public AudioClip[] variants;
        [Range(0f,1f)] public float volume=.3f;
        public Vector2 pitchRange=Vector2.one;
        [Min(1)] public int maxVoices=1;
        [Min(0f)] public float cooldown;
        public bool loop;
        public bool enabled=true;
    }
    [SerializeField] private Sound[] sounds;
    [SerializeField, Range(.1f,.95f)] private float voiceBudget=.82f;
    public Sound[] Sounds => sounds;
    public float VoiceBudget => voiceBudget;
}
