using System;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-40)]
public sealed class LakeBossEntrance : MonoBehaviour
{
    public enum Stage { Hidden, Round, Warning, Reveal, Arrival, Settle, Finished }
    public const float WarningDuration=1.05f,RevealDuration=2.3f,ArrivalDuration=1.7f,SettleDuration=.5f;
    [SerializeField] private Sprite lily;
    [SerializeField] private AudioClip waterSound,croakSound;
    private WaveController waves;
    private Player player;
    private LakeBackdrop lake;
    private Boss boss;
    private Rigidbody2D body;
    private SpriteRenderer bossVisual;
    private GameObject flyField;
    private Vector2 berth;
    private Action announced;
    private Canvas canvas;
    private CanvasGroup group;
    private RectTransform card,heroRect,warningRect;
    private UnityEngine.UI.Image dim,topBar,bottomBar,hero;
    private TMP_Text warning,warningDetail,arrivalText;
    private LakeEntranceWater water;
    private AudioSource audioSource;
    private float timer,clock;
    private bool playerWasActive,ownsPlayer;
    public Stage CurrentStage { get; private set; }
    public bool IsPlaying => CurrentStage!=Stage.Hidden && CurrentStage!=Stage.Finished;
    public float Clock => clock;
    public float StageTime => timer;
    public int Entrances { get; private set; }
    public Canvas PresentationCanvas => canvas;
    private void Awake()
    {
        waves=GetComponent<WaveController>();lake=FindAnyObjectByType<LakeBackdrop>();player=FindAnyObjectByType<Player>();
        audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;audioSource.ignoreListenerPause=false;
        GameAudioSettings.Route(audioSource,AudioChannel.Effects);
    }
    private void OnEnable(){if(waves!=null){waves.BossStarted+=OnBoss;waves.RunEnded+=OnEnd;}}
    private void OnDisable(){if(waves!=null){waves.BossStarted-=OnBoss;waves.RunEnded-=OnEnd;}Cancel();}
    public void Play(WaveAnnouncer announcer,int round,Action completed)
    {
        Cancel();Build();Entrances++;clock=timer=0;announced=completed;
        playerWasActive=player!=null && player.Movement.enabled;ownsPlayer=true;
        if(player!=null){player.Health.SetInvulnerable(this,true);player.SetGameplayActive(false);}
        group.alpha=0;canvas.gameObject.SetActive(true);SetStage(Stage.Round);
        announcer.Play(round,null,()=>{if(CurrentStage==Stage.Round){SetStage(Stage.Warning);PlaySound(waterSound,.55f);}});
    }
    private void SetStage(Stage next){CurrentStage=next;timer=0;}
    private void Update()
    {
        if(!IsPlaying)return;float dt=Time.deltaTime;if(dt<=0)return;timer+=dt;clock+=dt;
        if(player==null || player.Health.IsDead || waves.Phase==WaveController.RunPhase.Failed){Cancel();return;}
        if(CurrentStage==Stage.Warning && timer>=WarningDuration)SetStage(Stage.Reveal);
        else if(CurrentStage==Stage.Reveal && timer>=RevealDuration)
        {
            SetStage(Stage.Arrival);Action done=announced;announced=null;done?.Invoke();
        }
        else if(CurrentStage==Stage.Arrival && boss!=null && timer>=ArrivalDuration)
        {
            body.position=berth;LakeWaterFX.Create(transform,new Vector2(berth.x,lake.WaterY),1.8f);
            LakeSurfaceRipple.Create(transform,new Vector2(berth.x,lake.WaterY),1.2f,lake.WaterFXMaterial);
            SetStage(Stage.Settle);PlaySound(croakSound,.65f);
        }
        else if(CurrentStage==Stage.Settle && timer>=SettleDuration)Finish();
        Paint();
    }
    private void FixedUpdate()
    {
        if(CurrentStage!=Stage.Arrival || body==null)return;
        float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(timer/ArrivalDuration));
        body.MovePosition(new Vector2(berth.x,Mathf.Lerp(lake.WaterY-2.1f,berth.y,t)));
    }
    private void OnBoss(Boss target)
    {
        if(CurrentStage!=Stage.Arrival || target.GetComponent<SapoDoPneuCombat>()==null)return;
        boss=target;boss.HoldCombat(true);boss.Movement.enabled=false;body=boss.GetComponent<Rigidbody2D>();
        berth=new Vector2(boss.GetComponent<SapoDoPneuCombat>().Arena.xMin+boss.GetComponent<SapoDoPneuCombat>().Arena.width*.74f,lake.WaterY+SapoDoPneuCombat.FloatHeight);
        body.position=new Vector2(berth.x,lake.WaterY-2.1f);body.linearVelocity=Vector2.zero;
        bossVisual=boss.GetComponentInChildren<SpriteRenderer>();
        var first=boss.GetComponent<LakeTongueAttack>()?.Fly(0);flyField=first!=null?first.transform.parent.gameObject:null;
        if(flyField!=null)flyField.SetActive(false);
        timer=0;LakeSurfaceRipple.Create(transform,new Vector2(berth.x,lake.WaterY),.8f,lake.WaterFXMaterial);
    }
    private void Finish()
    {
        if(boss!=null && !boss.Health.IsDead)
        {
            boss.Movement.CompleteScriptedArrival(berth);boss.HoldCombat(false);
            if(flyField!=null)flyField.SetActive(true);
        }
        ReleasePlayer();SetStage(Stage.Finished);if(canvas!=null)canvas.gameObject.SetActive(false);
    }
    public void Cancel(bool releaseAnnouncement=true)
    {
        Action done=announced;announced=null;
        if(boss!=null && !boss.Health.IsDead)
        {
            if(boss.BattleActive && player!=null && !player.Health.IsDead && body!=null)boss.Movement.CompleteScriptedArrival(berth);
            boss.HoldCombat(false);if(flyField!=null)flyField.SetActive(true);
        }
        ReleasePlayer();CurrentStage=Stage.Hidden;if(canvas!=null)canvas.gameObject.SetActive(false);
        if(audioSource!=null)audioSource.Stop();if(releaseAnnouncement)done?.Invoke();boss=null;body=null;flyField=null;
    }
    private void ReleasePlayer()
    {
        if(!ownsPlayer)return;ownsPlayer=false;
        if(player!=null){player.Health.SetInvulnerable(this,false);if(playerWasActive && !player.Health.IsDead && waves!=null && waves.Phase!=WaveController.RunPhase.Failed)player.SetGameplayActive(true);}
    }
    private void OnEnd(bool won){Cancel();}
    private void PlaySound(AudioClip clip,float volume){if(clip!=null && audioSource!=null)audioSource.PlayOneShot(clip,volume);}
    private void Paint()
    {
        if(canvas==null)return;
        bool alert=CurrentStage==Stage.Warning,reveal=CurrentStage==Stage.Reveal;
        group.alpha=CurrentStage==Stage.Round?0:1;
        warningRect.gameObject.SetActive(alert);card.gameObject.SetActive(reveal);
        arrivalText.enabled=CurrentStage==Stage.Arrival || CurrentStage==Stage.Settle;
        dim.color=new Color(.015f,.065f,.07f,alert?.22f:reveal?.4f:0);
        float bars=CurrentStage==Stage.Round?0:CurrentStage==Stage.Arrival||CurrentStage==Stage.Settle?1-Mathf.Clamp01(timer/.5f):1;
        topBar.rectTransform.sizeDelta=new Vector2(0,36*bars);bottomBar.rectTransform.sizeDelta=new Vector2(0,36*bars);
        if(alert){warning.color=Color.Lerp(new Color(1,.78f,.3f),new Color(.94f,.95f,.68f),.5f+.5f*Mathf.Sin(timer*9));warningRect.localScale=Vector3.one*(1+Mathf.Sin(timer*10)*.012f);}
        if(reveal)
        {
            float enter=1-Mathf.Pow(1-Mathf.Clamp01(timer/.32f),3),exit=Mathf.Clamp01((timer-(RevealDuration-.3f))/.3f);
            card.anchoredPosition=new Vector2(1100*(1-enter)-1200*exit*exit,0);
            heroRect.anchoredPosition=new Vector2(330+45*(1-enter),16+Mathf.Sin(timer*2.3f)*3);water.Present(clock);
        }
    }
    private void Build()
    {
        if(canvas!=null)return;
        var root=new GameObject("SapeneuCururuEntrance",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
        root.transform.SetParent(transform,false);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=240;
        var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1200,720);scaler.matchWidthOrHeight=.5f;
        group=root.AddComponent<CanvasGroup>();group.blocksRaycasts=group.interactable=false;
        dim=Image("LakeDusk",root.transform,Vector2.zero,Vector2.zero,Color.clear);dim.rectTransform.anchorMin=Vector2.zero;dim.rectTransform.anchorMax=Vector2.one;
        topBar=Image("TopLetterbox",root.transform,Vector2.zero,Vector2.zero,new Color(.015f,.04f,.045f));topBar.rectTransform.anchorMin=new Vector2(0,1);topBar.rectTransform.anchorMax=Vector2.one;topBar.rectTransform.pivot=new Vector2(.5f,1);
        bottomBar=Image("BottomLetterbox",root.transform,Vector2.zero,Vector2.zero,new Color(.015f,.04f,.045f));bottomBar.rectTransform.anchorMin=Vector2.zero;bottomBar.rectTransform.anchorMax=new Vector2(1,0);bottomBar.rectTransform.pivot=new Vector2(.5f,0);
        var font=waves.BossPrefab.GetComponent<SapoDoPneuCombat>().HudFont;
        warningRect=Rect("LakeWarning",root.transform,new Vector2(1000,160),Vector2.zero);
        warning=Text("Warning","PERIGO NO LAGO",font,warningRect,new Vector2(1000,70),new Vector2(0,20),38,new Color(1,.8f,.3f));
        warningDetail=Text("WarningDetail","ALGUMA COISA ESTA SE MEXENDO...",font,warningRect,new Vector2(1000,40),new Vector2(0,-40),15,new Color(.7f,.88f,.78f));
        card=Rect("SapeneuReveal",root.transform,new Vector2(1060,300),Vector2.zero);
        var ribbon=Rect("MovingLakeRibbon",card,new Vector2(1060,270),new Vector2(0,-5));ribbon.pivot=Vector2.zero;ribbon.anchoredPosition=new Vector2(-530,-140);water=ribbon.gameObject.AddComponent<LakeEntranceWater>();
        Text("LakeLocation","LAGO DO PNEU",font,card,new Vector2(600,36),new Vector2(-170,87),16,new Color(.53f,.75f,.6f));
        Text("BossTitle","SAPENEU CURURU",font,card,new Vector2(660,75),new Vector2(-165,25),36,new Color(.98f,.94f,.66f));
        Text("BossSubtitle","O DONO DESTE BREJO",font,card,new Vector2(600,42),new Vector2(-170,-37),18,new Color(.63f,.87f,.79f));
        Text("BossChallenge","NINGUEM PASSA PELO MEU LAGO!",font,card,new Vector2(650,36),new Vector2(-165,-84),12,new Color(.56f,.72f,.64f));
        hero=Image("SapeneuPortrait",card,new Vector2(330,16),new Vector2(360,340),Color.white);hero.sprite=waves.BossPrefab.GetComponentInChildren<SpriteRenderer>().sprite;hero.preserveAspect=true;heroRect=hero.rectTransform;
        if(lily!=null){var leaf=Image("FloatingLily",card,new Vector2(-405,-116),new Vector2(180,26),Color.white);leaf.sprite=lily;leaf.preserveAspect=true;}
        arrivalText=Text("EmergenceTitle","SAPENEU CURURU",font,root.transform,new Vector2(1000,60),new Vector2(0,230),30,new Color(.98f,.94f,.66f));
        root.SetActive(false);
    }
    private static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 at)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=at;return r;
    }
    private static UnityEngine.UI.Image Image(string name,Transform parent,Vector2 at,Vector2 size,Color color)
    {var image=Rect(name,parent,size,at).gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;}
    private static TMP_Text Text(string name,string text,TMP_FontAsset font,Transform parent,Vector2 size,Vector2 at,float height,Color color)
    {
        var label=Rect(name,parent,size,at).gameObject.AddComponent<TextMeshProUGUI>();label.text=text;label.font=font;label.fontSize=height;label.color=color;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
        label.textWrappingMode=TextWrappingModes.NoWrap;label.enableAutoSizing=true;label.fontSizeMin=height-2;label.fontSizeMax=height;return label;
    }
}
