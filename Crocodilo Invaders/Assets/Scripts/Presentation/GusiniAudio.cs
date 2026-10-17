using System;
using UnityEngine;

// Um único dono do mix do boss: vozes limitadas e caudas fora do ovo.
[RequireComponent(typeof(Boss))]
public sealed class GusiniAudio : MonoBehaviour
{
    [SerializeField] private GusiniAudioConfig config;
    [SerializeField, Range(4,16)] private int poolSize=10;
    private sealed class Voice
    {
        public AudioSource source;
        public GusiniAudioConfig.Sound sound;
        public float until, gain;
        public Transform loopOwner;
    }
    private Voice[] voices;
    private GusiniAudioConfig.Sound[] sounds;
    private float[] nextAllowed;
    private int[] nextVariant;
    private Boss boss;
    private GusiniCombat combat;
    private bool stopped=true;
    private float whistleEndsAt, squadronCueAt=float.PositiveInfinity;
    private Vector3 squadronCuePosition;
    private readonly System.Random variation=new System.Random();
    public event Action<GusiniSound,AudioClip,float> SoundPlayed;
    public GusiniAudioConfig Config => config;
    public bool IsRunning => !stopped && isActiveAndEnabled && boss.BattleActive && !boss.Health.IsDead && combat.isActiveAndEnabled;
    public int ActiveVoiceCount
    {
        get { int count=0; if(voices!=null) foreach(var v in voices) if(Active(v)) count++; return count; }
    }

    private void Awake()
    {
        boss=GetComponent<Boss>(); combat=GetComponent<GusiniCombat>();
        int count=Enum.GetValues(typeof(GusiniSound)).Length;
        sounds=new GusiniAudioConfig.Sound[count]; nextAllowed=new float[count]; nextVariant=new int[count];
        if(config!=null) foreach(var sound in config.Sounds) sounds[(int)sound.id]=sound;
        voices=new Voice[poolSize];
        for(int i=0;i<voices.Length;i++)
        {
            var child=new GameObject("Gusini voice "+i); child.transform.SetParent(transform,false);
            var source=child.AddComponent<AudioSource>();
            source.playOnAwake=false; source.spatialBlend=0; source.ignoreListenerPause=false;
            GameAudioSettings.Route(source,AudioChannel.Effects);
            voices[i]=new Voice { source=source };
        }
    }

    public void BeginBattle()
    {
        stopped=false;
        Play(GusiniSound.Engine,transform.position,transform);
    }
    private bool Active(Voice v) => v.sound!=null && (v.sound.loop || Time.time<v.until);
    private void Update()
    {
        if(stopped) return;
        if(!IsRunning) { StopAll(); return; }
        foreach(var v in voices)
            if(v.sound!=null && (!Active(v) || (v.sound.loop && (v.loopOwner==null || !v.loopOwner.gameObject.activeInHierarchy)))) Clear(v);
        if(Time.time>=squadronCueAt)
        {
            squadronCueAt=float.PositiveInfinity;
            Play(GusiniSound.HonkMid,squadronCuePosition);
        }
        ApplyMix();
    }

    public bool Play(GusiniSound id,Vector3 position,Transform loopOwner=null)
    {
        if(!IsRunning || config==null) return false;
        var sound=sounds[(int)id];
        if(sound==null || !sound.enabled || sound.variants==null || sound.variants.Length==0) return false;
        // Dois ovos no mesmo instante compartilham o ataque sonoro, sem somar dois transientes.
        if(id==GusiniSound.Burst)
            foreach(var v in voices)
                if(Active(v) && v.sound.id==id && v.until-Time.time>v.source.clip.length/Mathf.Abs(v.source.pitch)-.035f)
                { v.gain=Mathf.Min(sound.volume*1.12f,1f); ApplyMix(); return false; }
        if(Time.time<nextAllowed[(int)id]) return false;
        Voice free=null, lowest=null; int playing=0;
        foreach(var v in voices)
        {
            if(!Active(v)) { if(free==null) free=v; continue; }
            if(v.sound.id==id) playing++;
            if(!v.sound.loop && (lowest==null || v.sound.group<lowest.sound.group)) lowest=v;
        }
        if(playing>=sound.maxVoices) return false;
        if(free==null)
        {
            if(lowest==null || lowest.sound.group>=sound.group || lowest.sound.loop) return false;
            free=lowest;
        }
        int variant=nextVariant[(int)id]++ % sound.variants.Length;
        AudioClip clip=sound.variants[variant];
        if(clip==null) return false;
        Clear(free);
        float pitch=Mathf.Lerp(sound.pitchRange.x,sound.pitchRange.y,(float)variation.NextDouble());
        free.sound=sound; free.gain=sound.volume; free.loopOwner=loopOwner;
        free.until=Time.time+clip.length/Mathf.Max(.1f,Mathf.Abs(pitch));
        free.source.clip=clip; free.source.pitch=pitch; free.source.loop=sound.loop;
        free.source.panStereo=Mathf.Clamp(position.x/10f,-1f,1f)*.12f;
        free.source.priority=Mathf.Clamp(160-(int)sound.group*18,0,256);
        nextAllowed[(int)id]=Time.time+sound.cooldown;
        if(id==GusiniSound.Whistle) whistleEndsAt=free.until;
        ApplyMix(); free.source.Play();
        SoundPlayed?.Invoke(id,clip,free.source.volume);
        return true;
    }

    private void ApplyMix()
    {
        if(config==null) return;
        bool danger=false;
        foreach(var v in voices) if(Active(v) && v.sound.group==GusiniSoundGroup.Danger) danger=true;
        float sum=0;
        foreach(var v in voices)
        {
            if(!Active(v)) continue;
            float duck=danger && (v.sound.group==GusiniSoundGroup.Chorus || v.sound.group==GusiniSoundGroup.Ambient || v.sound.group==GusiniSoundGroup.Flyby) ? .65f : 1f;
            v.source.volume=v.gain*duck; sum+=v.source.volume;
        }
        // Orçamento linear conservador: mesmo transientes coincidentes mantêm headroom do Gusini.
        if(sum<=config.VoiceBudget) return;
        float scale=config.VoiceBudget/sum;
        foreach(var v in voices) if(Active(v)) v.source.volume*=scale;
    }

    public void CueSquadron(Vector3 position)
    {
        if(!IsRunning || config==null || !float.IsPositiveInfinity(squadronCueAt)) return;
        // Um só efeito curto por ataque inteiro; os três grupos não têm vozes próprias.
        squadronCuePosition=position;
        squadronCueAt=Mathf.Max(Time.time,whistleEndsAt+.03f);
        Play(GusiniSound.Flyby,position);
    }
    public void StopOwnerLoop(Transform owner)
    {
        foreach(var v in voices) if(v.sound!=null && v.sound.loop && v.loopOwner==owner) Clear(v);
    }
    public void StopAll()
    {
        stopped=true; squadronCueAt=float.PositiveInfinity; whistleEndsAt=0;
        if(voices==null) return;
        foreach(var v in voices) Clear(v);
    }
    private static void Clear(Voice v)
    {
        if(v.source!=null) { v.source.Stop(); v.source.clip=null; }
        v.sound=null; v.loopOwner=null;
    }
    private void OnDisable() => StopAll();
}
