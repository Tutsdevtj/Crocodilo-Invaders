using System;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel { Master, Effects, Music }

// O mixer preserva o volume e os fades locais de cada AudioSource.
public sealed class GameAudioSettings : MonoBehaviour
{
    private static GameAudioSettings instance;
    private readonly float[] levels = new float[3];
    private AudioMixer mixer;
    private bool ready, dirty;
    public static event Action Changed;
    public static AudioMixer Mixer => Instance.mixer;
    public static string PreferenceKey(AudioChannel channel) => "Crocodilo.Audio." + channel + ".v1";
    public static string Parameter(AudioChannel channel) => channel + "Volume";

    private static GameAudioSettings Instance
    {
        get
        {
            if (instance == null) new GameObject("GameAudioSettings").AddComponent<GameAudioSettings>();
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; Changed = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot() { _ = Instance; }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        mixer = Resources.Load<AudioMixer>("Audio/GameAudio");
        for (int i = 0; i < levels.Length; i++)
            levels[i] = Sanitize(PlayerPrefs.GetFloat(PreferenceKey((AudioChannel)i), 1f));
    }

    private void Start() { ready = true; Apply(); }
    public static float Get(AudioChannel channel) => Instance.levels[(int)channel];
    public static float Decibels(float value) => value <= 0f ? -80f : Mathf.Log10(Mathf.Max(.0001f, value)) * 20f;

    public static void Set(AudioChannel channel, float value)
    {
        var settings = Instance;
        value = Sanitize(value);
        if (Mathf.Approximately(settings.levels[(int)channel], value)) return;
        settings.levels[(int)channel] = value;
        settings.dirty = true;
        if (settings.ready) settings.Apply();
        Changed?.Invoke();
    }

    private static float Sanitize(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
    private void Apply()
    {
        if (mixer == null) { Debug.LogError("Configure o mixer Resources/Audio/GameAudio.", this); return; }
        for (int i = 0; i < levels.Length; i++)
            if (!mixer.SetFloat(Parameter((AudioChannel)i), Decibels(levels[i])))
                Debug.LogError("Parâmetro de volume ausente no mixer: " + Parameter((AudioChannel)i), this);
    }

    public static void Save()
    {
        if (instance == null || !instance.dirty) return;
        for (int i = 0; i < instance.levels.Length; i++) PlayerPrefs.SetFloat(PreferenceKey((AudioChannel)i), instance.levels[i]);
        PlayerPrefs.Save();
        instance.dirty = false;
    }

    public static void Route(AudioSource source, AudioChannel channel)
    {
        if (source == null || Instance.mixer == null) return;
        AudioMixerGroup[] groups = Instance.mixer.FindMatchingGroups(channel.ToString());
        if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
    }

    private void OnApplicationPause(bool paused) { if (paused) Save(); }
    private void OnApplicationQuit() => Save();
}
