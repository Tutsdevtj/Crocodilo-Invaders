using System;
using UnityEngine;

// Lake presentation uses the same runner as Gusini; combat and chapter flow keep ownership.
public sealed class LakeStoryController : MonoBehaviour
{
    public enum Scene { None, Opening, Encounter, Fury, Finale }
    [SerializeField] private DialogueRunner dialogue;
    [SerializeField] private Player player;
    [SerializeField] private PauseController pause;
    [SerializeField] private WaveController waves;
    [SerializeField] private LakeStoryWorld world;
    [SerializeField] private DialogueSequence opening,encounter,fury,finale;
    private Boss boss;
    private SapoDoPneuCombat combat;
    private Action completed;
    private bool introPlayed,encounterPlayed,furyPlayed,finalePlayed,ownedPlayer,playerWasActive;
    public Scene Playing { get; private set; }
    public DialogueRunner Dialogue => dialogue;
    public LakeStoryWorld World => world;
    public int ScenesCompleted { get; private set; }
    public event Action<Scene> SceneStarted;
    private void OnEnable()
    {
        if(dialogue!=null)dialogue.LineStarted+=OnLine;
        if(waves!=null)waves.RunEnded+=OnEnd;
        if(pause!=null)pause.MenuReturnRequested+=Abort;
    }
    private void OnDisable()
    {
        if(dialogue!=null)dialogue.LineStarted-=OnLine;
        if(waves!=null)waves.RunEnded-=OnEnd;
        if(pause!=null)pause.MenuReturnRequested-=Abort;
        if(combat!=null)combat.RageStarted-=OnRage;
        Abort();
    }
    public bool PlayIntro(Action done)
    {
        if(introPlayed)return false;introPlayed=true;
        return Begin(Scene.Opening,opening,done);
    }
    public void BindBoss(Boss target)
    {
        if(combat!=null)combat.RageStarted-=OnRage;
        boss=target;combat=target!=null?target.GetComponent<SapoDoPneuCombat>():null;
        if(combat==null)return;
        boss.RetainAfterDefeat();combat.RageStarted+=OnRage;
    }
    public void PlayBossIntro(Boss target)
    {
        if(encounterPlayed || target==null || !target.BattleActive)return;
        if(boss!=target)BindBoss(target);
        encounterPlayed=true;Begin(Scene.Encounter,encounter,null);
    }
    private void OnRage()
    {
        if(furyPlayed || boss==null || boss.Health.IsDead)return;
        furyPlayed=true;Begin(Scene.Fury,fury,null);
    }
    public bool PlayFinale(Boss target,Action done)
    {
        if(finalePlayed || target==null || !target.RetainedAfterDefeat)return false;
        finalePlayed=true;boss=target;
        world?.ShowFinale(target);
        return Begin(Scene.Finale,finale,done);
    }
    private bool Begin(Scene scene,DialogueSequence sequence,Action done)
    {
        if(Playing!=Scene.None || dialogue==null || sequence==null)return false;
        Playing=scene;completed=done;
        playerWasActive=player!=null && player.Movement.enabled;ownedPlayer=true;
        if(player!=null){player.Health.SetInvulnerable(this,true);player.SetGameplayActive(false);}
        if(boss!=null && !boss.Health.IsDead){boss.HoldCombat(true);boss.Health.SetInvulnerable(this,true);}
        if(!dialogue.Play(sequence,Finish)){Release(true);Playing=Scene.None;completed=null;return false;}
        SceneStarted?.Invoke(scene);return true;
    }
    private void OnLine(int index,DialogueSequence.Line line)
    {
        if(Playing==Scene.Fury && line.beat=="join")combat?.Companion?.Enter();
        if(Playing==Scene.Finale && line.beat=="channel")world?.OpenChannel(false);
    }
    private void Finish()
    {
        Scene ending=Playing;Playing=Scene.None;ScenesCompleted++;
        if(ending==Scene.Fury){combat?.Companion?.Enter();combat?.FinishStoryRage();}
        if(ending==Scene.Finale)world?.OpenChannel(true);
        Action done=completed;completed=null;
        Release(ending!=Scene.Finale);
        done?.Invoke();
    }
    private void Release(bool restore)
    {
        if(boss!=null){boss.Health.SetInvulnerable(this,false);if(!boss.Health.IsDead)boss.HoldCombat(false);}
        if(ownedPlayer && player!=null)
        {
            player.Health.SetInvulnerable(this,false);
            if(restore && playerWasActive && !player.Health.IsDead && waves!=null && waves.Phase!=WaveController.RunPhase.Failed)player.SetGameplayActive(true);
        }
        ownedPlayer=false;
    }
    public void Abort()
    {
        Playing=Scene.None;completed=null;
        if(dialogue!=null)dialogue.Cancel();
        Release(false);
    }
    private void OnEnd(bool won)=>Abort();
}
