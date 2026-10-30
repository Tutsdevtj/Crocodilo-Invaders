using System.Collections.Generic;
using UnityEngine;

// Coordinates only lake enemies; WaveController and the card controller still own the run.
[DefaultExecutionOrder(-20)]
public sealed class LakeJourneyDirector : MonoBehaviour
{
    private WaveController waves;
    private EnemySpawner spawner;
    private LakeBackdrop lake;
    private Player player;
    private LakeJourneyEnemy striking;
    private readonly HashSet<LakeJourneyEnemy> preparing=new HashSet<LakeJourneyEnemy>();
    private readonly Queue<LakeJourneyEnemy> preparationOrder=new Queue<LakeJourneyEnemy>();
    private Transform threats;
    private float clock,nextStrike,nextDamage;
    private int ordinal;
    public int Wave => waves!=null?waves.CurrentWave:0;
    public int Section => Wave<=3?0:Wave<=6?1:2;
    public bool CombatActive => waves!=null && waves.Phase==WaveController.RunPhase.Combat && player!=null && !player.Health.IsDead;
    public bool PiranhasEnabled => Wave>=5 || (waves!=null && waves.IsBossWave(Wave));
    public bool Calm => waves!=null && waves.IsBossWave(Wave) && waves.Phase==WaveController.RunPhase.Announcing;
    public float Clock => clock;
    public float WaterY => lake.WaterY;
    public Health Target => player!=null?player.Health:null;
    public Transform Threats => threats;
    public Material WaterMaterial => lake.WaterFXMaterial;
    public Sprite DropSprite { get; private set; }
    public int Attacks { get; private set; }
    public int Hits { get; private set; }
    public int Spawned { get; private set; }
    public int ActivePreparations => preparing.Count;
    public LakeJourneyEnemy Striking => striking;
    public int ActiveDrops => threats!=null?threats.GetComponentsInChildren<LakeJourneyDrop>().Length:0;
    public Rect Arena
    {
        get
        {
            var camera=lake.GameplayCamera;float h=camera.orthographicSize*2;
            return new Rect(camera.transform.position.x-h*camera.aspect*.5f,camera.transform.position.y-h*.5f,h*camera.aspect,h);
        }
    }
    private void Awake()
    {
        lake=GetComponent<LakeBackdrop>();waves=FindAnyObjectByType<WaveController>();spawner=waves!=null?waves.GetComponent<EnemySpawner>():null;
        player=FindAnyObjectByType<Player>();
        DropSprite=Resources.Load<Sprite>("LakeJourneyV22/water-drop");
        threats=new GameObject("LakeJourneyThreats").transform;threats.SetParent(transform,false);
    }
    private void OnEnable()
    {
        if(spawner!=null)spawner.EnemySpawned+=SpawnedEnemy;
        if(waves!=null){waves.WaveAnnounced+=Announced;waves.RunEnded+=Ended;}
    }
    private void Update()
    {
        clock+=Time.deltaTime;
        if(!CombatActive && threats!=null && threats.childCount>0)ClearThreats();
    }
    private void SpawnedEnemy(Enemy enemy)
    {
        var actor=enemy.GetComponent<LakeJourneyEnemy>();if(actor==null)return;
        actor.Join(this,ordinal++);Spawned++;
    }
    private void Announced(int wave,int total){ordinal=0;preparing.Clear();preparationOrder.Clear();striking=null;nextStrike=clock+.9f;ClearThreats();}
    public bool Prepare(LakeJourneyEnemy enemy)
    {
        if(!CombatActive || enemy==null)return false;
        if(preparing.Contains(enemy))return true;
        if(preparing.Count>=2)return false;
        preparing.Add(enemy);preparationOrder.Enqueue(enemy);return true;
    }
    public bool Strike(LakeJourneyEnemy enemy)
    {
        while(preparationOrder.Count>0 && (preparationOrder.Peek()==null || !preparing.Contains(preparationOrder.Peek())))preparationOrder.Dequeue();
        if(!CombatActive || !preparing.Contains(enemy) || preparationOrder.Count==0 || preparationOrder.Peek()!=enemy || striking!=null || clock<nextStrike || ActiveDrops>0)return false;
        preparationOrder.Dequeue();striking=enemy;preparing.Remove(enemy);nextStrike=clock+1.18f;Attacks++;return true;
    }
    public void Release(LakeJourneyEnemy enemy)
    {
        preparing.Remove(enemy);if(striking==enemy){striking=null;nextStrike=Mathf.Max(nextStrike,clock+.25f);}
    }
    public bool Hit()
    {
        if(!CombatActive || clock<nextDamage)return false;
        nextDamage=clock+.7f;if(!player.Health.TakeDamage(1))return false;Hits++;return true;
    }
    private void ClearThreats()
    {
        if(threats==null)return;
        foreach(Transform child in threats){child.gameObject.SetActive(false);Destroy(child.gameObject);}
    }
    private void Ended(bool won){striking=null;preparing.Clear();preparationOrder.Clear();ClearThreats();}
    private void OnDisable()
    {
        if(spawner!=null)spawner.EnemySpawned-=SpawnedEnemy;
        if(waves!=null){waves.WaveAnnounced-=Announced;waves.RunEnded-=Ended;}
        Ended(false);
    }
}
