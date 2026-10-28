using UnityEngine;

// Dense gorilla vocals lead the transformation; a short baked echo stays behind the direct roar.
public sealed class MacaquiniJungleVoice : MonoBehaviour
{
    private AudioClip effort, tear, roar;
    private AudioSource[] voices;
    private MacaquiniPhaseTwo phase;
    private Boss boss;
    private bool begun, capPlayed, tearPlayed, roarPlayed;
    private int next;
    public bool Available => effort != null && tear != null && roar != null;
    public int CuesPlayed { get; private set; }
    public int RoarsPlayed { get; private set; }
    public AudioSource[] Voices => voices;
    public bool AnyPlaying { get { if (voices == null) return false; foreach (var voice in voices) if (voice != null && voice.isPlaying) return true; return false; } }
    private void Awake()
    {
        phase = GetComponent<MacaquiniPhaseTwo>(); boss = GetComponent<Boss>();
        const string path = "Audio/MacaquiniGorillaV27/";
        effort = Resources.Load<AudioClip>(path + "gorilla_effort"); tear = Resources.Load<AudioClip>(path + "gorilla_rising"); roar = Resources.Load<AudioClip>(path + "gorilla_power");
        voices = new AudioSource[3];
        for (int i = 0; i < voices.Length; i++)
        {
            var node = new GameObject("JungleMonkeyVoice" + i); node.transform.SetParent(transform, false);
            var voice = node.AddComponent<AudioSource>(); voice.playOnAwake = false; voice.loop = false;
            voice.spatialBlend = 0; voice.dopplerLevel = 0; voice.ignoreListenerPause = false; voice.priority = 48;
            GameAudioSettings.Route(voice, AudioChannel.Effects); voices[i] = voice;
        }
    }
    private bool CanPlay => Available && isActiveAndEnabled && begun && boss.BattleActive && phase.CurrentStage == MacaquiniPhaseTwo.Stage.Tearing;
    public void BeginSequence()
    {
        StopSequence(); begun = true; capPlayed = tearPlayed = roarPlayed = false;
        if (CanPlay) Play(effort, .62f);
    }
    public void CapEffort()
    {
        if (!CanPlay || capPlayed) return; capPlayed = true;
    }
    public void TearEffort()
    {
        if (!CanPlay || tearPlayed) return; tearPlayed = true; Play(tear, .82f);
    }
    public bool PowerRoar()
    {
        if (!CanPlay) return false;
        if (!roarPlayed) { roarPlayed = true; RoarsPlayed++; Play(roar, .96f); }
        return true;
    }
    private void Play(AudioClip clip, float gain)
    {
        var voice = voices[next]; next = (next + 1) % voices.Length;
        voice.Stop(); voice.clip = clip; voice.pitch = 1; voice.volume = gain; voice.Play(); CuesPlayed++;
    }
    public void StopSequence()
    {
        begun = false; next = 0;
        if (voices != null) foreach (var voice in voices) if (voice != null) voice.Stop();
    }
    private void OnDisable() => StopSequence();
}
