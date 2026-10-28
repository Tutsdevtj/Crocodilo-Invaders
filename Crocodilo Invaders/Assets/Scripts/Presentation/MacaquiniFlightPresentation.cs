using UnityEngine;

// Complete painted poses own anatomy; separate effects only animate propulsion and damage.
[DefaultExecutionOrder(100)]
public sealed class MacaquiniFlightPresentation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer view;
    [SerializeField] private Sprite[] flight, transformation, swinging, shooting;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private Material effectsMaterial;
    [SerializeField] private Material[] particleMaterials;
    [SerializeField] private AudioClip engineLoop;
    [SerializeField] private Vector2[] flightMuzzles, swingMuzzles, shootMuzzles;
    [SerializeField] private bool naturalMotion;
    [SerializeField] private Vector2[] swingVines, jetNozzles;
    [SerializeField, Min(1)] private float motionFramesPerSecond=20;
    [SerializeField] private float closedFramesPerSecond;
    [SerializeField] private bool chronologicalTransformation;
    [SerializeField] private bool uniformSpriteScale;
    [SerializeField] private float[] transformationStarts;
    [SerializeField] private Vector2[] transformationVines;
    [SerializeField] private int transformationVineStart=-1;
    private Boss boss;
    private Health health;
    private MacaquiniCombat combat;
    private MacaquiniPhaseTwo phase;
    private Rigidbody2D body;
    private SpriteRenderer[] flames;
    private Sprite[][] flameFrames;
    private ParticleSystem smoke, sparks;
    private LineRenderer[] cracks;
    private LineRenderer vine;
    private LineRenderer muzzleFlash;
    private AudioSource motor;
    private float clock, roll, rollVelocity, firedAt=-10, flashUntil;
    private float motionPhase,recoilOffset,recoilVelocity;
    private float visualTearProgress;
    private bool wasTransforming;
    private AnimationCurve smokeGrowthCurve;
    public bool HalfShell => phase.CurrentStage==MacaquiniPhaseTwo.Stage.HalfShell;
    public bool Transforming => phase.CurrentStage==MacaquiniPhaseTwo.Stage.Tearing;
    public bool JetpackAttached => phase.CurrentStage==MacaquiniPhaseTwo.Stage.ClosedShell || Transforming&&TearProgress<.625f;
    public float TearProgress => chronologicalTransformation&&Transforming?Mathf.Max(visualTearProgress,GetComponent<MacaquiniPresentation>().TearProgress):GetComponent<MacaquiniPresentation>().TearProgress;
    public float DamageLevel {get;private set;}
    public float SmokeRate {get;private set;}
    public int Frame {get;private set;}
    public float MotionPhase => motionPhase;
    public int MotionPeriod => Mathf.Max(1,2*(HalfShell?swinging.Length:flight.Length)-2);
    public int MotionDrawingCount => HalfShell?swinging.Length:flight.Length;
    public bool NaturalMotion => naturalMotion;
    public bool ChronologicalTransformation=>chronologicalTransformation;
    public bool UniformSpriteScale=>uniformSpriteScale;
    public int TransformationDrawingCount=>transformation.Length;
    public bool BodyVisible=>view.enabled&&view.gameObject.activeInHierarchy&&view.sprite!=null&&view.color.a>.999f;
    public Sprite CurrentSprite => view.sprite;
    public bool VineVisible => HalfShell || Transforming&&(chronologicalTransformation?Frame>=transformationVineStart&&transformationVineStart>=0:TearProgress>=.90f);
    public Vector3 MuzzlePosition => shotOrigin.position;
    public Vector3 VineGripPosition=>view.transform.TransformPoint(HalfShell&&swingVines!=null&&swingVines.Length>Frame?(Vector3)swingVines[Frame]:new Vector3(.2f,3f));
    private void Awake()
    {
        boss=GetComponent<Boss>();health=GetComponent<Health>();combat=GetComponent<MacaquiniCombat>();
        phase=GetComponent<MacaquiniPhaseTwo>();body=GetComponent<Rigidbody2D>();
        smokeGrowthCurve=AnimationCurve.Linear(0,.3f,1,1.4f);
        flames=new SpriteRenderer[2];flameFrames=new Sprite[2][];
        for(int engine=0;engine<2;engine++){
            var texture=(Texture2D)particleMaterials[engine].mainTexture;float width=texture.width/4f;flameFrames[engine]=new Sprite[4];
            for(int frame=0;frame<4;frame++)flameFrames[engine][frame]=Sprite.Create(texture,new Rect(frame*width,0,width,texture.height),new Vector2(.5f,1),256,0,SpriteMeshType.FullRect);
            var jet=new GameObject(engine==0?"LeftExhaust":"RightExhaust");jet.transform.SetParent(view.transform,false);
            jet.transform.localPosition=jetNozzles!=null&&jetNozzles.Length==2?(Vector3)jetNozzles[engine]:engine==0?new Vector3(.69f,.68f):new Vector3(.90f,.81f);jet.transform.localRotation=Quaternion.Euler(0,0,22);
            flames[engine]=jet.AddComponent<SpriteRenderer>();flames[engine].sortingOrder=34;flames[engine].sprite=flameFrames[engine][0];flames[engine].enabled=false;
        }
        smoke=Particles("MotorSmoke",new Vector3(.43f,1.55f),36,2);
        sparks=Particles("MotorSparks",new Vector3(.58f,1.45f),37,3);
        cracks=new[]{Line("EngineCrackA",.025f),Line("EngineCrackB",.02f)};
        cracks[0].positionCount=4;cracks[0].SetPositions(new[]{new Vector3(.30f,1.9f),new Vector3(.42f,1.70f),new Vector3(.35f,1.63f),new Vector3(.52f,1.4f)});
        cracks[1].positionCount=4;cracks[1].SetPositions(new[]{new Vector3(.61f,1.5f),new Vector3(.75f,1.27f),new Vector3(.68f,1.2f),new Vector3(.85f,.98f)});
        vine=Line("CanopyVineExtension",.075f);vine.sortingOrder=29;vine.positionCount=18;
        muzzleFlash=Line("PaintedGunFlash",.065f);muzzleFlash.transform.SetParent(shotOrigin,false);muzzleFlash.sortingOrder=45;muzzleFlash.positionCount=5;
        muzzleFlash.SetPositions(new[]{new Vector3(0,-.1f),new Vector3(-.22f,.04f),new Vector3(.01f,.13f),new Vector3(-.12f,.02f),Vector3.zero});
        var sound=new GameObject("JetpackMotor");sound.transform.SetParent(transform,false);motor=sound.AddComponent<AudioSource>();
        motor.clip=engineLoop;motor.loop=true;motor.playOnAwake=false;GameAudioSettings.Route(motor,AudioChannel.Effects);
        view.sprite=flight[0];UpdateMuzzle();
    }
    private ParticleSystem Particles(string name,Vector3 position,int order,int materialIndex)
    {
        var node=new GameObject(name);node.transform.SetParent(view.transform,false);node.transform.localPosition=position;
        var ps=node.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=2;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=120;main.startLifetime=.24f;main.startSize=.09f;main.startSpeed=1;
        var emission=ps.emission;emission.rateOverTime=0;
        var shape=ps.shape;shape.enabled=false;
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterials[materialIndex];renderer.sortingOrder=order;renderer.renderMode=ParticleSystemRenderMode.Billboard;
        var sheet=ps.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=4;sheet.numTilesY=1;sheet.animation=ParticleSystemAnimationType.WholeSheet;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,.999f));sheet.cycleCount=1;
        var fade=ps.colorOverLifetime;fade.enabled=true;
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.2f,0),new GradientAlphaKey(.85f,.12f),new GradientAlphaKey(.55f,.65f),new GradientAlphaKey(0,1)});fade.color=gradient;
        ps.Play();return ps;
    }
    public void AttachDiscardedSmoke(Transform pack)
    {
        // Keep the trail with the falling motor; its world-space particles drift after emission ends.
        var trail=Particles("DiscardedMotorSmoke",Vector3.zero,33,2);
        trail.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        trail.transform.SetParent(pack,false);trail.transform.localPosition=Vector3.up*.4f;
        var main=trail.main;main.loop=false;main.duration=1.8f;main.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.1f);main.startSize=new ParticleSystem.MinMaxCurve(.2f,.55f);main.startSpeed=.6f;
        var emission=trail.emission;emission.rateOverTime=16;trail.Play();
    }
    private LineRenderer Line(string name,float width)
    {
        var node=new GameObject(name);node.transform.SetParent(view.transform,false);
        var line=node.AddComponent<LineRenderer>();line.sharedMaterial=effectsMaterial;line.useWorldSpace=false;line.sortingOrder=35;line.startWidth=line.endWidth=width;line.enabled=false;return line;
    }
    private void OnEnable(){GetComponent<MacaquiniCombat>().ShotReleased+=OnShot;GetComponent<Health>().Damaged+=OnDamage;GetComponent<Boss>().BattleStopped+=Stop;}
    private void OnDisable(){GetComponent<MacaquiniCombat>().ShotReleased-=OnShot;GetComponent<Health>().Damaged-=OnDamage;GetComponent<Boss>().BattleStopped-=Stop;Stop();}
    private void OnDamage(){flashUntil=Time.time+.10f;if(!Transforming&&!HalfShell)DamageLevel=Mathf.Max(DamageLevel,1-health.Fraction);}
    public void BeginTearSequence()
    {
        if(!chronologicalTransformation)return;
        visualTearProgress=0;Frame=0;wasTransforming=true;view.sprite=transformation[0];view.color=Color.white;
    }
    public void AdvanceTearMilestone(float progress)
    {
        if(chronologicalTransformation&&Transforming)visualTearProgress=Mathf.Max(visualTearProgress,Mathf.Clamp01(progress));
    }
    private void OnShot(){firedAt=Time.time;if(HalfShell&&!naturalMotion){Frame=0;view.sprite=shooting[0];}UpdateMuzzle();}
    private void UpdateMuzzle()
    {
        // Mouth of the painted organic banana gun, relative to the registered pelvis.
        Vector3 local;
        if(HalfShell&&!naturalMotion&&Time.time-firedAt<.5f)local=shootMuzzles[Mathf.Clamp(Frame,0,shootMuzzles.Length-1)];
        else if(HalfShell)local=swingMuzzles[Mathf.Clamp(Frame,0,swingMuzzles.Length-1)];
        else local=flightMuzzles[Mathf.Clamp(Frame,0,flightMuzzles.Length-1)];
        shotOrigin.position=view.transform.TransformPoint(local);
    }
    private void LateUpdate()
    {
        if(!boss.BattleActive||health.IsDead)return;
        if(Time.timeScale<=0)return;
        clock+=Time.deltaTime;
        var kick=GetComponent<MacaquiniSwingKick>();
        if(kick!=null&&kick.Running){kick.Present(vine);muzzleFlash.enabled=false;return;}
        Vector2 velocity=body.linearVelocity;
        float desired=naturalMotion?(HalfShell?Mathf.Sin(clock*1.35f)*1.4f:Mathf.Clamp(-velocity.x*1.6f+velocity.y*.8f,-13,13)+Mathf.Sin(clock*1.8f)*.8f):HalfShell?Mathf.Sin(clock*2)*3:Mathf.Clamp(-velocity.x*2.6f+velocity.y*1.2f,-17,17)+Mathf.Sin(clock*3)*2;
        if(HalfShell&&combat.Pendulum!=null)desired-=combat.Pendulum.Angle*Mathf.Rad2Deg*.035f;
        if(Transforming)desired=chronologicalTransformation?Mathf.Sin(TearProgress*Mathf.PI*8)*Mathf.Sin(TearProgress*Mathf.PI)*.65f:Mathf.Sin(TearProgress*60)*Mathf.Sin(TearProgress*Mathf.PI)*2;
        roll=Mathf.SmoothDampAngle(roll,desired,ref rollVelocity,naturalMotion?.24f:.16f,120,Time.deltaTime);
        view.transform.localRotation=Quaternion.Euler(0,0,roll);
        view.transform.localPosition=Vector3.up*(naturalMotion?Mathf.Sin(clock*1.8f)*.025f:Mathf.Sin(clock*3)*.055f);
        float recoilTarget=Time.time-firedAt<.055f?.07f:0;
        recoilOffset=Mathf.SmoothDamp(recoilOffset,recoilTarget,ref recoilVelocity,recoilTarget>recoilOffset?.045f:.11f,10,Time.deltaTime);
        view.transform.localPosition+=Vector3.right*(naturalMotion?recoilOffset:Mathf.Max(0,1-(Time.time-firedAt)/.18f)*.07f);
        if(!HalfShell&&!Transforming)DamageLevel=Mathf.Max(DamageLevel,1-health.Fraction);
        if(Transforming){
            if(chronologicalTransformation){visualTearProgress=Mathf.Clamp01(Mathf.Max(visualTearProgress,GetComponent<MacaquiniPresentation>().TearProgress));int drawing=0;while(drawing+1<transformationStarts.Length&&transformationStarts[drawing+1]<=visualTearProgress)drawing++;Frame=Mathf.Max(wasTransforming?Frame:0,drawing);}
            else Frame=Mathf.Clamp((int)(TearProgress*transformation.Length),0,transformation.Length-1);
            view.sprite=transformation[Frame];
        }
        else if(naturalMotion){if(wasTransforming&&chronologicalTransformation&&HalfShell)motionPhase=0;else{float fps=!HalfShell&&closedFramesPerSecond>0?closedFramesPerSecond:motionFramesPerSecond;motionPhase=Mathf.Repeat(motionPhase+Time.deltaTime*fps,MotionPeriod);}Frame=PingPongDrawing(motionPhase,MotionDrawingCount);view.sprite=HalfShell?swinging[Frame]:flight[Frame];}
        else if(HalfShell){float age=Time.time-firedAt;if(age<.5f){Frame=Mathf.Min(shooting.Length-1,(int)(age*shooting.Length/.5f));view.sprite=shooting[Frame];}else{Frame=PingPongDrawing(clock*8,swinging.Length);view.sprite=swinging[Frame];}}
        else{Frame=PingPongDrawing(clock*(velocity.magnitude>5?14:10),flight.Length);view.sprite=flight[Frame];}
        view.color=!(chronologicalTransformation&&Transforming)&&Time.time<flashUntil?new Color(1,.78f,.65f):Color.white;
        wasTransforming=Transforming;
        UpdateMuzzle();Propulsion(velocity.magnitude);
        muzzleFlash.enabled=Time.time-firedAt<.12f;muzzleFlash.startColor=muzzleFlash.endColor=new Color(1,.9f,.24f);
        // Extend only the vine painted into the second form to the moving canopy.
        vine.enabled=HalfShell||chronologicalTransformation&&VineVisible;
        if(vine.enabled){Vector3 start=Transforming?(Vector3)transformationVines[Frame]:naturalMotion&&swingVines!=null&&swingVines.Length==swinging.Length?(Vector3)swingVines[Frame]:new Vector3(.18f,3.13f);Vector3 top=vine.transform.InverseTransformPoint(new Vector3(transform.position.x+.4f,8.2f));for(int i=0;i<18;i++){float t=i/17f;Vector3 p=Vector3.Lerp(start,top,t);p.x+=Mathf.Sin(t*Mathf.PI)*Mathf.Sin(clock*2)*.08f;vine.SetPosition(i,p);}vine.startColor=vine.endColor=new Color(.19f,.42f,.08f);}
    }
    public static int PingPongDrawing(float phase,int count){if(count<=1)return 0;int period=count*2-2,p=Mathf.FloorToInt(Mathf.Repeat(phase,period));return p<count?p:period-p;}
    private void Propulsion(float speed)
    {
        bool attached=JetpackAttached;
        float failure=Transforming?1:DamageLevel;
        float thrust=attached?(combat.PerformingAttack?1.2f:.8f)+speed*.07f:0;
        bool sputter=failure>.33f&&Mathf.Sin(clock*(17+failure*13))>1-failure*.65f;
        if(sputter)thrust*=.16f;
        for(int i=0;i<flames.Length;i++){
            var jet=flames[i];jet.enabled=attached;jet.sprite=flameFrames[i][(int)(clock*24+i*1.7f)%4];
            float flutter=1+Mathf.Sin(clock*67+i*2)*.09f;
            jet.transform.localScale=new Vector3(.30f+thrust*.12f,(.55f+thrust*.35f)*flutter,1);
            jet.color=new Color(1,1,1,sputter?.25f:1);
        }
        SmokeRate=attached?Mathf.InverseLerp(.22f,1,failure)*36:0;
        var smain=smoke.main;smain.startLifetime=new ParticleSystem.MinMaxCurve(.7f,1.2f);smain.startSize=new ParticleSystem.MinMaxCurve(.22f,.42f+failure*.4f);smain.startColor=Color.white;
        var se=smoke.emission;se.rateOverTime=SmokeRate*.5f;var sv=smoke.velocityOverLifetime;sv.enabled=true;sv.space=ParticleSystemSimulationSpace.World;sv.x=new ParticleSystem.MinMaxCurve(-.32f,.45f);sv.y=new ParticleSystem.MinMaxCurve(1.05f,1.35f);sv.z=new ParticleSystem.MinMaxCurve(0,0);
        var ss=smoke.sizeOverLifetime;ss.enabled=true;ss.size=new ParticleSystem.MinMaxCurve(1,smokeGrowthCurve);
        var ke=sparks.emission;ke.rateOverTime=attached&&failure>.5f?(failure-.5f)*28:0;
        var km=sparks.main;km.startColor=Color.white;km.startLifetime=.35f;km.startSize=.2f;km.startSpeed=2.5f;km.gravityModifier=.4f;
        var sparkShape=sparks.shape;sparkShape.enabled=true;sparkShape.shapeType=ParticleSystemShapeType.Sphere;sparkShape.radius=.08f;
        for(int i=0;i<cracks.Length;i++){cracks[i].enabled=attached&&failure>(i==0?.35f:.65f);cracks[i].startColor=cracks[i].endColor=new Color(1,.22f,.03f,.6f+.4f*Mathf.Sin(clock*21));}
        if(attached){if(!motor.isPlaying&&engineLoop!=null)motor.Play();motor.pitch=.8f+thrust*.24f-failure*.12f;motor.volume=sputter?.025f:.12f+thrust*.035f;}else motor.Stop();
    }
    private void Stop()
    {
        if(motor!=null)motor.Stop();
        SmokeRate=0;
        if(flames!=null)foreach(var p in flames)p.enabled=false;
        if(smoke!=null)smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(sparks!=null)sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(cracks!=null)foreach(var c in cracks)c.enabled=false;
        if(vine!=null)vine.enabled=false;
        if(muzzleFlash!=null)muzzleFlash.enabled=false;
    }
    private void OnDestroy(){if(flameFrames!=null)foreach(var frames in flameFrames)foreach(var frame in frames)if(frame!=null)Destroy(frame);}
}
