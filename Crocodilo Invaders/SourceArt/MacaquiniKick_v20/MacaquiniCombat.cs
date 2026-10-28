using System;
using System.Collections;
using UnityEngine;

// Owns only this boss's patterns; Boss/Health/WaveController retain lifecycle and scoring.
[RequireComponent(typeof(Boss))]
public sealed class MacaquiniCombat : MonoBehaviour, IBossCombatController
{
    public enum Attack { BananaVolley, BananaFan, Vines, JetStrafe, JetDash, ExhaustGates, Pendulum }
    [SerializeField] private AudioClip pendulumWhip;
    [SerializeField] private bool pendulumEnabled;
    public bool PendulumEnabled=>pendulumEnabled;
    [SerializeField] private Projectile bananaPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Animator animator;
    [SerializeField] private LineRenderer aimGuide;
    [SerializeField] private JungleVineHazard vinePrefab;
    [SerializeField, Min(.1f)] private float warningDuration = .75f;
    [SerializeField, Min(.1f)] private float recoveryDuration = 1.1f;
    [SerializeField, Min(.1f)] private float projectileSpeed = 6f;
    [SerializeField] private Rect vineArena = new Rect(-8.4f, -2.2f, 16.8f, 7.8f);
    private Boss boss;
    private Health target;
    private Collider2D owner;
    private Coroutine fight;
    private Vector2 lockedAim;
    private bool releasePending, clipDone, stopped;
    private Transform threats;
    private Attack? lastAttack;
    private Attack[] bag = { Attack.BananaVolley, Attack.BananaFan, Attack.Vines };
    private bool sweepFromLeft;
    private int bagIndex;
    private bool highSwing;
    private bool phaseTwo;
    public Attack CurrentAttack { get; private set; }
    public Transform ThreatRoot => threats;
    public bool IsTelegraphing { get; private set; }
    public int Releases { get; private set; }
    public bool PerformingAttack { get; private set; }
    public Vector2 AimPoint => lockedAim;
    public Health Target => target;
    public bool PhaseTwo => phaseTwo;
    public MacaquiniPendulumVine Pendulum{get;private set;}
    public void EnterPhaseTwo() { phaseTwo = true; bag=pendulumEnabled?new[]{Attack.Pendulum,Attack.BananaFan,Attack.Vines,Attack.BananaVolley}:new[]{Attack.BananaVolley,Attack.BananaFan,Attack.Vines};bagIndex=0;lastAttack=null; }
    public event Action<Attack> AttackStarted;
    public event Action ShotReleased;

    private void Awake()
    {
        boss = GetComponent<Boss>(); owner = GetComponent<Collider2D>();
        if(GetComponent<MacaquiniFlightPresentation>()!=null){bag[0]=Attack.JetStrafe;bag[1]=Attack.JetDash;bag[2]=Attack.ExhaustGates;}
        if (aimGuide != null) aimGuide.enabled = false;
    }
    public void SetTarget(Health value)
    {
        StopCombat();
        target = value;
        if (!isActiveAndEnabled || target == null || target.IsDead) return;
        stopped = false;
        target.Died += StopCombat;
        bagIndex = 0; lastAttack = null;
        fight = StartCoroutine(Fight());
    }
    private bool Active => !stopped && isActiveAndEnabled && boss.BattleActive && target != null && !target.IsDead;
    private IEnumerator Fight()
    {
        while (Active && !boss.Ready) yield return null;
        yield return new WaitForSeconds(.6f);
        while (Active)
        {
            if (!boss.Ready) { yield return null; continue; }
            yield return Pattern(NextAttack());
            yield return Recover();
        }
        fight = null;
    }
    private Attack NextAttack()
    {
        // The opening teaches all three in order; later bags contain each pattern exactly once.
        if (bagIndex == bag.Length)
        {
            for (int i = bag.Length - 1; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); (bag[i], bag[j]) = (bag[j], bag[i]); }
            if (bag[0] == lastAttack) (bag[0], bag[1]) = (bag[1], bag[0]);
            bagIndex = 0;
        }
        return bag[bagIndex++];
    }
    private IEnumerator Pattern(Attack attack)
    {
        CurrentAttack = attack; lastAttack = attack;
        PerformingAttack = true;
        boss.Movement.HoldPosition(true);
        AttackStarted?.Invoke(attack);
        if(attack==Attack.Pendulum)
        {
            ClearThreats();IsTelegraphing=true;sweepFromLeft=!sweepFromLeft;
            var node=new GameObject("MacaquiniPendulum");node.transform.SetParent(Root(),false);
            var swing=node.AddComponent<MacaquiniPendulumVine>();
            Pendulum=swing;
            swing.Configure(target,GetComponent<MacaquiniFlightPresentation>(),aimGuide.sharedMaterial,vinePrefab.VineSprite,pendulumWhip,sweepFromLeft,boss.Health.Fraction<=.5f);
            while(Active&&swing!=null){IsTelegraphing=!swing.Dangerous;yield return null;}
            IsTelegraphing=false;PerformingAttack=false;yield break;
        }
        if(attack==Attack.JetDash || attack==Attack.ExhaustGates)
        {
            ClearThreats();IsTelegraphing=true;lockedAim=target.transform.position;
            Material material=aimGuide.sharedMaterial;
            if(attack==Attack.JetDash)
            {
                // Commit to one visible diagonal. The player can leave it before ignition.
                Vector2 from=transform.position;
                Vector2 to=ArenaPoint(new Vector2(-5f,Mathf.Clamp(lockedAim.y,-1.8f,2.2f)));
                Heat(from,to,material,.95f,1.15f,.8f);
                yield return new WaitForSeconds(.95f);IsTelegraphing=false;
                if(!Active)yield break;
                boss.Movement.DashTo(to,12f);
                while(Active&&boss.Movement.IsDashing)yield return null;
                yield return new WaitForSeconds(.45f);
            }
            else
            {
                // Engine purge: two narrow columns lock before firing, separated by a broad escape gap.
                float x=Mathf.Clamp(lockedAim.x,-6.8f,3f);
                Heat(new Vector2(x,7.4f),new Vector2(x,-3.4f),material,1.25f,.65f,.7f);
                Heat(new Vector2(x+2.9f,7.4f),new Vector2(x+2.9f,-3.4f),material,1.25f,.65f,.7f);
                yield return new WaitForSeconds(1.25f);IsTelegraphing=false;
                if(!Active)yield break;
                boss.Movement.DashTo(ArenaPoint(new Vector2(4.5f,2.1f)),6f);
                yield return new WaitForSeconds(.85f);
            }
            PerformingAttack=false;yield break;
        }
        if (attack == Attack.Vines)
        {
            // Three horizontal lanes; one entire lane stays empty, with no lingering volley.
            ClearThreats();
            int safe = UnityEngine.Random.Range(0, 3);
            for (int lane = 0; lane < 3; lane++)
            {
                if (lane == safe) continue;
                JungleVineHazard vine = Instantiate(vinePrefab, Root());
                float y = vineArena.yMin + vineArena.height * (lane + .5f) / 3f;
                vine.Configure(new Vector2(vineArena.xMax, y), new Vector2(vineArena.xMin, y), target, 1.1f, .48f);
            }
            IsTelegraphing = true;
            yield return new WaitForSeconds(1.1f);
            IsTelegraphing = false;
            yield return new WaitForSeconds(.6f);
            PerformingAttack = false;
            yield break;
        }
        lockedAim = target.transform.position;
        IsTelegraphing = true;
        if (aimGuide != null)
        {
            aimGuide.positionCount = 2; aimGuide.SetPosition(0, muzzle.position);
            aimGuide.SetPosition(1, lockedAim); aimGuide.enabled = true;
        }
        yield return new WaitForSeconds(warningDuration);
        if (aimGuide != null) aimGuide.enabled = false;
        IsTelegraphing = false;
        if(attack==Attack.JetStrafe)boss.Movement.DashTo(ArenaPoint(new Vector2(3.8f,transform.position.y>1.1f?-.5f:2.1f)),.7f);
        int volleys = attack == Attack.BananaVolley || attack == Attack.JetStrafe ? 3 : 1;
        for (int shot = 0; shot < volleys && Active; shot++)
        {
            releasePending = true; clipDone = false;
            animator.Play("Fire", 0, 0f);
            // Animation events own the release; the deadline only prevents a broken clip hanging combat.
            float deadline = Time.time + 1.5f;
            while (!clipDone && Active && Time.time < deadline) yield return null;
            releasePending = false;
            if (!clipDone && Active) Debug.LogError("Macaquini Fire clip did not complete.", this);
            yield return new WaitForSeconds(.12f);
        }
        PerformingAttack = false;
    }
    private IEnumerator Recover()
    {
        if (!Active) yield break;
        // Alternate anchors so every recovery crosses a visible arc, instead of picking nearby points.
        highSwing = !highSwing;
        Vector2 landing = ArenaPoint(highSwing ? new Vector2(4.5f, 2.1f) : new Vector2(2.6f, -.5f));
        Vector2 crest = ArenaPoint(new Vector2((transform.position.x + landing.x) * .5f, Mathf.Max(transform.position.y, landing.y) + .3f));
        boss.Movement.DashTo(crest, phaseTwo ? 5f : 4.4f);
        while (Active && boss.Movement.IsDashing) yield return null;
        if (!Active) yield break;
        boss.Movement.DashTo(landing, phaseTwo ? 5f : 4.4f);
        while (Active && boss.Movement.IsDashing) yield return null;
        if (!Active) yield break;
        boss.Movement.ReturnToPatrol();
        yield return new WaitForSeconds(phaseTwo ? recoveryDuration * .8f : recoveryDuration);
    }
    private void LateUpdate()
    {
        if (aimGuide == null || !aimGuide.enabled || muzzle == null) return;
        aimGuide.SetPosition(0, muzzle.position); aimGuide.SetPosition(1, lockedAim);
    }
    public void MacaquiniRelease()
    {
        if (!releasePending || !Active || !boss.Ready) return;
        releasePending = false; Releases++;
        ShotReleased?.Invoke();
        Vector2 direction = lockedAim - (Vector2)muzzle.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int count = CurrentAttack == Attack.BananaFan ? (phaseTwo ? 7 : 5) : 1;
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0 : Mathf.Lerp(-32, 32, i / (float)(count - 1));
            Vector2 heading = Quaternion.Euler(0, 0, offset) * direction.normalized;
            Projectile banana = Instantiate(bananaPrefab, muzzle.position, Quaternion.Euler(0, 0, angle + offset - 180f), Root());
            banana.Launch(heading, owner, projectileSpeed);
        }
    }
    public void MacaquiniClipComplete() { clipDone = true; }
    private Transform Root()
    {
        if (threats == null)
        {
            var root = new GameObject("MacaquiniThreats");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, gameObject.scene);
            threats = root.transform;
        }
        return threats;
    }
    private void Heat(Vector2 from,Vector2 to,Material material,float warning,float duration,float width)
    {
        var node=new GameObject("JetpackHeatLane");node.transform.SetParent(Root(),false);
        var hazard=node.AddComponent<MacaquiniHeatHazard>();hazard.Configure(from,to,target,material,warning,duration,width);
        if(Mathf.Abs(from.x-to.x)<.01f)hazard.LaunchFuel(transform.position+Vector3.right*.8f);
    }
    private Vector2 ArenaPoint(Vector2 desired)
    {
        var camera=Camera.main;if(camera==null)return desired;
        float half=camera.orthographicSize*camera.aspect;
        return new Vector2(Mathf.Clamp(desired.x,camera.transform.position.x-half+2.8f,camera.transform.position.x+half-2.1f),Mathf.Clamp(desired.y,camera.transform.position.y-camera.orthographicSize+2.4f,camera.transform.position.y+camera.orthographicSize-2.6f));
    }
    private void ClearThreats()
    {
        Pendulum=null;
        if (threats == null) return;
        threats.gameObject.SetActive(false); Destroy(threats.gameObject); threats = null;
    }
    public void StopCombat()
    {
        stopped = true;
        if (target != null) target.Died -= StopCombat;
        target = null;
        if (fight != null) StopCoroutine(fight);
        fight = null; releasePending = false; clipDone = true; IsTelegraphing = false; PerformingAttack = false;
        if (aimGuide != null) aimGuide.enabled = false;
        if (animator != null && animator.isActiveAndEnabled) animator.Play("Idle", 0, 0f);
        ClearThreats();
    }
    private void OnDisable() => StopCombat();
    public void ForceAttack(Attack attack)
    {
        if (!Active || !boss.Ready || Time.timeScale <= 0) return;
        if(attack==Attack.Pendulum&&(!phaseTwo||!pendulumEnabled))return;
        if (fight != null) StopCoroutine(fight);
        ClearThreats(); releasePending = false;
        if (aimGuide != null) aimGuide.enabled = false;
        fight = StartCoroutine(Forced(attack));
    }
    private IEnumerator Forced(Attack attack)
    {
        yield return Pattern(attack);
        yield return Recover();
        fight = StartCoroutine(Fight());
    }
}
