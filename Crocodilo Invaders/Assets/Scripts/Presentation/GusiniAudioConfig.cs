using System;
using UnityEngine;

public enum GusiniSound
{
    Engine, Cannon, Cock, Whistle, Command, Flyby, HonkLow, HonkMid, HonkHigh,
    HatchLatch, HatchOpen, EggDrop, HatchClose, Deploy, Flutter, Crack1, Crack2, Burst, YolkWhoosh, Hit,
    // Acrescentar somente ao fim: os 20 IDs da F1 já estão serializados.
    P2FinalHit, P2EngineStall, P2FirstPulse, P2CorruptionSurge, P2MutationPeak, P2RevealCry,
    P2EnergyCharge, P2EnergyFire, P2EnergyPop,
    P2ParachutePop, P2EggPulse, P2EggCrack, P2EggBurst,
    P2CorruptCall, P2EdgeMark, P2HonkLow, P2HonkMid, P2HonkHigh, P2DiverWarning, P2DiveWhoosh, P2GoosePoof,
    P2ModuleCrack, P2TentacleEmerge, P2LaneWarning, P2Lash, P2TipCrack, P2ResistCry, P2WeakpointOpen, P2WeakpointClose
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
