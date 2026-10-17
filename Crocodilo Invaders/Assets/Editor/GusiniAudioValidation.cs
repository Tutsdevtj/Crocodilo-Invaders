using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GusiniAudioValidation
{
    public static void Validate()
    {
        var config=AssetDatabase.LoadAssetAtPath<GusiniAudioConfig>("Assets/Data/GusiniAudio.asset");
        Require(config!=null && config.Sounds.Length==20,"20 Gusini sound entries");
        Require(config.Sounds.Select(s => s.id).Distinct().Count()==20,"Unique sound IDs");
        Require(config.Sounds.Sum(s => s.variants.Length)==24,"24 final WAVs");
        Require(config.VoiceBudget<=.85f,"Shared voice headroom");
        foreach(var sound in config.Sounds)
        {
            Require(sound.variants.Length>0 && sound.variants.All(c => c!=null),"Missing audio: "+sound.id);
            foreach(var clip in sound.variants)
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Require(!importer.loadInBackground && importer.defaultSampleSettings.preloadAudioData,"Preloaded audio: "+sound.id);
                Require(importer.defaultSampleSettings.loadType==(sound.loop?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad),"Audio load type: "+sound.id);
                Require(clip.channels==(sound.id==GusiniSound.Flyby?2:1),"Audio channel count: "+sound.id);
            }
        }
        foreach(var id in new[] {GusiniSound.HonkLow,GusiniSound.HonkMid,GusiniSound.HonkHigh})
        {
            var sound=config.Sounds.Single(s => s.id==id);
            Require(sound.enabled==(id==GusiniSound.HonkMid),"Only one honk register enabled: "+id);
            Require(sound.pitchRange==Vector2.one && sound.maxVoices==1,"Single fixed-pitch voice: "+id);
            if(id==GusiniSound.HonkMid)
                Require(sound.volume<=.1f && sound.cooldown>=2f && !sound.loop && sound.variants.All(c => c.length<=.2f),"Brief, quiet honk with repeat protection");
        }
        foreach(var id in new[] {GusiniSound.Command,GusiniSound.Flutter,GusiniSound.YolkWhoosh})
            Require(!config.Sounds.Single(s => s.id==id).enabled,"Optional layer off: "+id);
        var boss=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/Gusini.prefab");
        Require(boss.GetComponent<GusiniAudio>()!=null,"Shared boss audio owner");
        foreach(string name in new[] {"GusiniSquadronGoose","GusiniParachuteEgg"})
            Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/"+name+".prefab").GetComponentsInChildren<AudioSource>(true).Length==0,"No independent voice pileup: "+name);
        Debug.Log("GUSINI AUDIO VALIDATION PASSED: 24 clips/20 effects; preload; one brief squadron honk; optional layers off; shared voice budget.");
    }
    private static void Require(bool pass,string message) { if(!pass) throw new InvalidOperationException(message); }
}
