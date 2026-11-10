using UnityEngine;

// Soft wind under the first stage; the sky scene otherwise has only music and shots.
[DisallowMultipleComponent]
public sealed class SkyAmbience : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float volume = .22f;
    [SerializeField, Min(.01f)] private float fadeSpeed = .3f;
    private AudioSource source;

    private void Start()
    {
        var clip = Resources.Load<AudioClip>("Audio/Ambience/AMB_Sky_Wind_Loop");
        if (clip == null)
        { enabled = false; return; }
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.clip = clip;
        source.volume = 0f;
        GameAudioSettings.Route(source, AudioChannel.Effects);
        source.Play();
    }
    private void Update()
    {
        source.volume = Mathf.MoveTowards(source.volume, volume, Time.unscaledDeltaTime * fadeSpeed);
    }
}
