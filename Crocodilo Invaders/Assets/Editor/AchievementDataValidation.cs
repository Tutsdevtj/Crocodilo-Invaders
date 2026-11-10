using System;
using System.IO;
using UnityEngine;

public static class AchievementDataValidation
{
    public static int Run(string directory)
    {
        Directory.CreateDirectory(directory);int checks=0;
        void Check(bool value,string label){if(!value)throw new Exception("ACHIEVEMENT_DATA_FAILED "+label);checks++;}
        var store=new AchievementStore(Path.Combine(directory,"normal"));
        Check(store.UnlockedCount==0,"fresh store");
        Check(!store.Get(AchievementCatalog.LastTooth).Unlocked,"hidden initially locked");
        Check(store.Set(AchievementCatalog.Dodge,4,out bool unlocked)&&!unlocked,"partial progress");
        Check(!store.Set(AchievementCatalog.Dodge,2,out _),"progress cannot regress");
        Check(!store.Add("invalid",1,out _),"unknown ID ignored");
        for(int i=0;i<6;i++)Check(store.Add(AchievementCatalog.Dodge,1,out unlocked)&&unlocked==(i==5),"threshold "+i);
        long timestamp=store.Get(AchievementCatalog.Dodge).unlockedUtc;
        Check(timestamp>0&&!store.Add(AchievementCatalog.Dodge,1,out _),"no duplicate unlock");
        var snapshot=store.Get(AchievementCatalog.Dodge);snapshot.progress=0;
        Check(store.Get(AchievementCatalog.Dodge).progress==10,"read snapshots cannot mutate save");
        for(int i=0;i<5;i++)
        {
            Check(store.CollectCardKind((UpgradeKind)i,out unlocked)&&unlocked==(i==4),"unique card type "+i);
            Check(!store.CollectCardKind((UpgradeKind)i,out _),"duplicate type "+i);
        }
        var reload=new AchievementStore(Path.Combine(directory,"normal"));
        Check(reload.Get(AchievementCatalog.Dodge).unlockedUtc==timestamp&&reload.Get(AchievementCatalog.Cards).progress==5,"persistent counters and dates");
        Check(reload.Set(AchievementCatalog.LastTooth,1,out unlocked)&&unlocked,"hidden unlock persists");
        Check(new AchievementStore(Path.Combine(directory,"normal")).Get(AchievementCatalog.LastTooth).Unlocked,"hidden reload");
        File.Copy(reload.FilePath,reload.FilePath+".bak",true);File.WriteAllText(reload.FilePath,"{invalid");
        var recovery=new AchievementStore(Path.Combine(directory,"normal"));
        Check(recovery.Get(AchievementCatalog.LastTooth).Unlocked,"corrupt primary recovers backup");
        Check(recovery.Set(AchievementCatalog.FirstWave,1,out _),"recovered save writable");
        var futureDir=Path.Combine(directory,"future");Directory.CreateDirectory(futureDir);
        string future="{\"version\":99,\"entries\":[]}";File.WriteAllText(Path.Combine(futureDir,"achievements.json"),future);
        var futureStore=new AchievementStore(futureDir);
        Check(!futureStore.Set(AchievementCatalog.FirstWave,1,out _)&&File.ReadAllText(futureStore.FilePath)==future,"future version protected");
        var blocked=Path.Combine(directory,"blocked");File.WriteAllText(blocked,"file, not a directory");
        var failed=new AchievementStore(blocked);
        Check(failed.Set(AchievementCatalog.FirstWave,1,out unlocked)&&unlocked&&failed.HasUnsavedChanges&&failed.Error!=null,"disk failure keeps the award pending");
        failed.RetrySave();Check(failed.HasUnsavedChanges,"retry while the disk still fails stays pending");
        File.Delete(blocked);failed.RetrySave();
        Check(!failed.HasUnsavedChanges&&new AchievementStore(blocked).Get(AchievementCatalog.FirstWave).Unlocked,"retry persists the pending award");
        var tunedDir=Path.Combine(directory,"tuned");Directory.CreateDirectory(tunedDir);
        File.WriteAllText(Path.Combine(tunedDir,"achievements.json"),"{\"version\":1,\"entries\":[{\"id\":\"sport_mode\",\"progress\":7,\"mask\":0,\"unlockedUtc\":100},{\"id\":\"first_wave\",\"progress\":1,\"mask\":0,\"unlockedUtc\":200}]}");
        var tuned=new AchievementStore(tunedDir);
        Check(tuned.Get(AchievementCatalog.Dodge).Unlocked&&tuned.Get(AchievementCatalog.Dodge).progress==10&&tuned.Get(AchievementCatalog.FirstWave).Unlocked,"changed goal keeps unlocks and the rest of the save");
        File.WriteAllText(Path.Combine(tunedDir,"achievements.json"),"{\"version\":1,\"entries\":[{\"id\":\"sport_mode\",\"progress\":15,\"mask\":0,\"unlockedUtc\":0}]}");
        var lowered=new AchievementStore(tunedDir);
        Check(!lowered.Get(AchievementCatalog.Dodge).Unlocked&&lowered.Get(AchievementCatalog.Dodge).progress==9,"lowered goal stays one step from unlocking");
        Check(AchievementCatalog.All.Count==10&&AchievementCatalog.Released.Count==8,"two achievements wait for a public route");
        Check(AchievementCatalog.Get(AchievementCatalog.Macaquini).Released==false&&AchievementCatalog.Get(AchievementCatalog.Sapeneu).Released==false,"Macaquini and Sapeneu stay out of the list");
        Debug.Log("ACHIEVEMENTS_DATA_OK checks="+checks);return checks;
    }
}
