using UnityEngine;
using UnityEngine.U2D.IK;

// Anatomical two-bone arms: a loaded hand stays on its world-space vine grip.
[DefaultExecutionOrder(80)]
public sealed class MacaquiniPresentation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] parts;
    [SerializeField] private Transform visualRig, farArm, nearArm, weapon, muzzle, supportHand;
    [SerializeField] private Transform gunForearm, supportForearm, gunPalm, supportPalm;
    [SerializeField] private Transform gunShoulderCover, supportShoulderCover;
    [SerializeField] private SpriteRenderer actionSprite;
    [SerializeField] private SpriteRenderer transformationView;
    [SerializeField] private SpriteRenderer peelLeft, peelRight;
    [SerializeField, HideInInspector] private float tearProgress;
    [SerializeField] private Sprite closedCore, halfCore;
    [SerializeField] private Vector2[] boneAxes, shoulders, halfShoulders;
    [SerializeField] private float limbScale = 1.3f;
    [SerializeField] private Sprite[] gripFrames;
    [SerializeField] private Sprite hudTitle, hudPortrait, hudPortraitHalf;
    [SerializeField] private LineRenderer supportVine;
    [SerializeField] private Material effectMaterial;
    [SerializeField] private SpriteRenderer charge;
    [SerializeField] private Sprite[] chargeFrames;
    [SerializeField] private AudioClip launchSound, chargeSound, whipSound, growlSound, hurtSound;
    private Health health;
    private Boss boss;
    private MacaquiniCombat combat;
    private AudioSource voice;
    private Rigidbody2D body;
    private MaterialPropertyBlock properties;
    private LineRenderer aura, muzzleRing, flash;
    private SpriteRenderer weaponView, supportView, gunView;
    private Vector3 basePosition, grip, oldGrip, nextGrip;
    private Vector3 peelLeftOrigin, peelRightOrigin, tearGunStart;
    private float clock, flashUntil, recoil, energy, transfer = -1f;
    private bool arrived, whipPlayed, gripReady, transforming;
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
    public const float HandScale = 1.35f;
    private const float PalmContact = .13f * HandScale;
    private float bodyRoll, bodyRollVelocity, aimAngle, aimVelocity, tearStartRoll;
    private bool aimReady;
    private int aimFrame = -1;
    private readonly float[] limbLengths = new float[2];
    private readonly Vector3[] limbPositions = new Vector3[3];
    private float[] limbAngles = new float[2];
    private static readonly float[] WeaponRecoilPixels = { 0, 0, 0, 0, 4, 8, 8, 6, 4, 2, 1, 0 };
    public Sprite HudTitle => hudTitle;
    public Sprite HudPortrait => hudPortrait;
    public Sprite HudPortraitHalf => hudPortraitHalf;
    public int CurrentDrawing { get; private set; }
    public int DrawingCount => gripFrames == null ? 0 : gripFrames.Length;
    public bool GripLoaded => transfer < 0;
    public float SupportGripError { get; private set; }
    public float WeaponGripError { get; private set; }
    public float SupportElbowAngle { get; private set; }
    public Vector3 GripPoint => grip;
    public Transform Weapon => weapon;
    public float PeelGripError { get; private set; }
    public float TearProgress => tearProgress;
    private Vector3 Shoulder(int index)
    {
        float blend = HalfShell ? 1 : transforming ? Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.25f, .42f, tearProgress)) : 0;
        return actionSprite.transform.TransformPoint(Vector2.Lerp(shoulders[index], halfShoulders[index], blend));
    }
#if UNITY_EDITOR
    public void PreviewPose()
    {
        body = GetComponent<Rigidbody2D>(); combat = GetComponent<MacaquiniCombat>();
        weaponView = weapon.GetComponent<SpriteRenderer>(); supportView = supportPalm.GetComponent<SpriteRenderer>(); gunView = gunPalm.GetComponent<SpriteRenderer>();
        PoseSupport(); PoseWeapon(); supportVine.enabled = false;
    }
#endif
    public bool HalfShell { get; private set; }
    public void BeginTransformation()
    {
        transforming = true; tearProgress = 0;
        if (GetComponent<MacaquiniFlightPresentation>() != null) { Play(growlSound, .8f); return; }
        tearStartRoll = visualRig.localEulerAngles.z;
        tearGunStart = gunPalm.position + gunPalm.rotation * (Vector3.up * PalmContact * transform.lossyScale.x);
        foreach (var part in parts) { part.enabled = true; part.SetPropertyBlock(null); }
        weaponView.enabled = false; transformationView.enabled = false; charge.enabled = false;
        gunView.sortingOrder = 23;
        peelLeft.enabled = peelRight.enabled = true; peelLeft.color = peelRight.color = Color.white;
        aura.enabled = muzzleRing.enabled = flash.enabled = false;
        visualRig.localPosition = basePosition;
        Play(growlSound, .8f);
    }
    public void EndTransformation(bool half)
    {
        if (!transforming) return;
        transforming = false; HalfShell = half;
        if (GetComponent<MacaquiniFlightPresentation>() != null) return;
        bodyRoll = visualRig.localEulerAngles.z; bodyRollVelocity = 0;
        transformationView.enabled = false;
        peelLeft.enabled = peelRight.enabled = false;
        gunView.sortingOrder = 17;
        actionSprite.sprite = half ? halfCore : closedCore;
        foreach (var part in parts) part.enabled = true;
    }
    private void Awake()
    {
        health = GetComponent<Health>(); boss = GetComponent<Boss>(); combat = GetComponent<MacaquiniCombat>();
        voice = GetComponent<AudioSource>(); body = GetComponent<Rigidbody2D>(); GameAudioSettings.Route(voice, AudioChannel.Effects);
        properties = new MaterialPropertyBlock(); basePosition = visualRig.localPosition;
        peelLeftOrigin = peelLeft.transform.localPosition; peelRightOrigin = peelRight.transform.localPosition;
        weaponView = weapon.GetComponent<SpriteRenderer>(); supportView = supportPalm.GetComponent<SpriteRenderer>(); gunView = gunPalm.GetComponent<SpriteRenderer>();
        aura = Effect("ShellEnergy", visualRig, 8, .018f); aura.loop = true;
        muzzleRing = Effect("MuzzleShock", muzzle, 25, .025f); muzzleRing.loop = true;
        flash = Effect("MuzzleFlash", muzzle, 26, .045f);
        if (GetComponent<MacaquiniFlightPresentation>() != null) visualRig.gameObject.SetActive(false);
    }
    private LineRenderer Effect(string name, Transform parent, int order, float width)
    {
        var node = new GameObject(name); node.transform.SetParent(parent, false);
        var line = node.AddComponent<LineRenderer>(); line.sharedMaterial = effectMaterial; line.useWorldSpace = false;
        line.sortingOrder = order; line.startWidth = line.endWidth = width; return line;
    }
    private void OnEnable() { health.Damaged += OnDamage; health.Died += OnDeath; combat.AttackStarted += OnAttack; combat.ShotReleased += OnShot; boss.BattleStopped += OnBattleStopped; }
    private void OnDisable()
    {
        health.Damaged -= OnDamage; health.Died -= OnDeath; combat.AttackStarted -= OnAttack; combat.ShotReleased -= OnShot; boss.BattleStopped -= OnBattleStopped;
        if (voice != null) voice.Stop();
        foreach (var part in parts) if (part != null) part.SetPropertyBlock(null);
    }
    private void Play(AudioClip clip, float gain) { if (voice != null && clip != null) voice.PlayOneShot(clip, gain); }
    private void OnAttack(MacaquiniCombat.Attack attack) { whipPlayed = false; Play(chargeSound, .43f); }
    private void OnShot()
    {
        recoil = 1;
        // Animator has selected the original release frame. Align its measured grip before spawning.
        if (GetComponent<MacaquiniFlightPresentation>() == null) PoseWeapon();
        Play(launchSound, .65f); GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .045f, .12f);
    }
    private void OnDamage() { flashUntil = Time.time + .14f; Play(hurtSound, .28f); }
    private void OnDeath() { OnBattleStopped(); supportVine.enabled = false; Play(growlSound, .55f); }
    private void OnBattleStopped()
    {
        if (voice != null) voice.Stop();
        if (charge != null) charge.enabled = false;
        if (aura != null) aura.enabled = muzzleRing.enabled = flash.enabled = false;
        if (supportVine != null) supportVine.enabled = false;
    }
    private void LateUpdate()
    {
        if (GetComponent<MacaquiniFlightPresentation>() != null) return;
        if (health.IsDead || !boss.BattleActive) return;
        clock += Time.deltaTime; recoil = Mathf.MoveTowards(recoil, 0, Time.deltaTime * 6);
        if (transforming) { PoseTear(); return; }
        if (!arrived && boss.Ready) { arrived = true; Play(growlSound, .8f); GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .07f, .18f); }
        float desiredRoll = Mathf.Sin(clock * 2.1f) * 3f - Mathf.Clamp(body.linearVelocity.x * 1.7f, -12, 12) + recoil * 2;
        bodyRoll = Mathf.SmoothDampAngle(bodyRoll, desiredRoll, ref bodyRollVelocity, .14f, 100, Time.deltaTime);
        float roll = bodyRoll;
        visualRig.localRotation = Quaternion.Euler(0, 0, roll);
        Vector3 pivot = Vector3.up * 1.1f;
        Vector3 bob = Vector3.up * (Mathf.Sin(clock * 2.8f) * .035f);
        if (Time.time < flashUntil) bob.x = Mathf.Sin(clock * 140) * .025f;
        visualRig.localPosition = basePosition + pivot - visualRig.localRotation * pivot + bob;
        PoseSupport(); PoseWeapon();
        foreach (var part in parts)
        {
            part.GetPropertyBlock(properties); properties.SetFloat(FlashAmount, Mathf.Clamp01((flashUntil - Time.time) / .14f)); part.SetPropertyBlock(properties);
        }
        bool vines = combat.PerformingAttack && combat.CurrentAttack == MacaquiniCombat.Attack.Vines;
        bool aiming = combat.PerformingAttack && !vines;
        energy = Mathf.MoveTowards(energy, combat.IsTelegraphing ? 1 : 0, Time.deltaTime * 3);
        Circle(aura, new Vector3(0, 1.55f), .64f + energy * .08f, new Color(.35f, 1, .16f, energy * .2f));
        Circle(muzzleRing, Vector3.zero, .08f + (1 - recoil) * .32f, new Color(.55f, 1, .18f, recoil * .8f));
        flash.enabled = recoil > .35f; flash.positionCount = 5;
        flash.SetPositions(new[] { new Vector3(-.14f, 0), new Vector3(-.04f, .08f), new Vector3(.05f, 0), new Vector3(-.04f, -.08f), new Vector3(-.14f, 0) });
        flash.startColor = flash.endColor = new Color(.8f, 1, .25f, recoil);
        charge.enabled = combat.IsTelegraphing && aiming;
        if (chargeFrames.Length > 0) charge.sprite = chargeFrames[(int)(clock * 16) % chargeFrames.Length];
        charge.transform.localScale = Vector3.one * (.2f + energy * .25f); charge.color = new Color(.55f, 1, .25f, .7f);
        if (vines && !combat.IsTelegraphing && !whipPlayed) { whipPlayed = true; Play(whipSound, .6f); GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .08f, .16f); }
    }
    private void PoseSupport()
    {
        Vector3 shoulder = Shoulder(1);
        supportShoulderCover.SetPositionAndRotation(shoulder, visualRig.rotation);
        float reach = (boneAxes[2].magnitude + boneAxes[3].magnitude) * transform.lossyScale.x * limbScale;
        float minimumReach = Mathf.Abs(boneAxes[2].magnitude - boneAxes[3].magnitude) * transform.lossyScale.x * limbScale;
        if (!gripReady) { grip = shoulder + Vector3.up * (reach * .72f); grip.y = Mathf.Min(grip.y, 7.6f); gripReady = true; }
        Vector3 wrist = grip - Vector3.up * PalmContact * transform.lossyScale.x;
        // Release before the arm can be pulled apart. The next contact is anticipated from physical velocity.
        float wristDistance = Vector3.Distance(shoulder, wrist);
        if (transfer < 0 && (wristDistance > reach * .88f || wristDistance < minimumReach + .2f))
        {
            transfer = 0; oldGrip = grip;
            Vector3 predictedShoulder = shoulder + (Vector3)body.linearVelocity * .40f;
            nextGrip = predictedShoulder + Vector3.up * (reach * .63f);
            nextGrip.y = Mathf.Min(nextGrip.y, 7.6f);
        }
        if (transfer >= 0)
        {
            transfer += Time.deltaTime;
            if (transfer < .1f) { CurrentDrawing = transfer < .05f ? 2 : 1; grip = oldGrip; }
            else if (transfer < .44f)
            {
                CurrentDrawing = 0;
                float u = Mathf.Clamp01((transfer - .1f) / .34f);
                u = u * u * u * (u * (u * 6 - 15) + 10);
                Vector3 controlA = oldGrip + new Vector3(reach * .38f, reach * .13f);
                Vector3 controlB = nextGrip + new Vector3(reach * .30f, -reach * .08f);
                grip = Mathf.Pow(1-u,3) * oldGrip + 3*(1-u)*(1-u)*u * controlA + 3*(1-u)*u*u * controlB + u*u*u * nextGrip;
            }
            else
            {
                grip = nextGrip; CurrentDrawing = transfer < .50f ? 1 : transfer < .55f ? 2 : 3;
                if (transfer >= .58f) transfer = -1;
            }
        }
        else CurrentDrawing = 3;
        supportView.sprite = gripFrames[CurrentDrawing]; supportPalm.rotation = Quaternion.identity;
        Vector3 targetWrist = grip - Vector3.up * PalmContact * transform.lossyScale.x;
        Vector3 end = SolveArm(nearArm, supportForearm, shoulder, targetWrist, boneAxes[2], boneAxes[3], -1);
        supportPalm.position = end; supportHand.position = end + Vector3.up * PalmContact * transform.lossyScale.x;
        SupportGripError = Vector3.Distance(supportHand.position, grip);
        SupportElbowAngle = Quaternion.Angle(nearArm.rotation, supportForearm.rotation);
        Vector3 vineEnd = transfer >= .1f && transfer < .44f ? nextGrip : grip;
        supportVine.enabled = true; supportVine.positionCount = 16;
        Vector3 top = new Vector3(vineEnd.x + .25f, 8.2f, 0);
        for (int i = 0; i < 16; i++)
        {
            float t = i / 15f; Vector3 p = Vector3.Lerp(top, vineEnd, t);
            p.x += Mathf.Sin(t * Mathf.PI) * Mathf.Sin(clock * 1.4f) * .09f;
            supportVine.SetPosition(i, p);
        }
    }
    private void PoseWeapon()
    {
        if (weaponView == null || boneAxes == null || boneAxes.Length < 4) return;
        Vector3 shoulder = Shoulder(0);
        gunShoulderCover.SetPositionAndRotation(shoulder, visualRig.rotation);
        bool aiming = combat.PerformingAttack && combat.CurrentAttack != MacaquiniCombat.Attack.Vines;
        Vector2 aimDirection = combat.AimPoint - (Vector2)shoulder;
        float aim = aiming ? Mathf.Clamp(Mathf.DeltaAngle(180, Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg), -32, 32) : -8 + Mathf.Sin(clock * 2) * 5;
        if (!aimReady || !Application.isPlaying) { aimAngle = aim; aimReady = true; }
        else if (aimFrame != Time.frameCount) aimAngle = Mathf.SmoothDampAngle(aimAngle, aim, ref aimVelocity, .1f, 240, Time.deltaTime);
        aimFrame = Time.frameCount; aim = aimAngle;
        Quaternion rotation = Quaternion.Euler(0, 0, aim);
        float reach = (boneAxes[0].magnitude + boneAxes[1].magnitude) * limbScale * .68f;
        Vector3 handGrip = shoulder + rotation * new Vector3(-reach + recoil * .06f, -.1f, 0) * transform.lossyScale.x;
        gunPalm.rotation = Quaternion.Euler(0, 0, 90 + aim);
        Vector3 wristTarget = handGrip - gunPalm.rotation * (Vector3.up * PalmContact * transform.lossyScale.x);
        gunPalm.position = SolveArm(farArm, gunForearm, shoulder, wristTarget, boneAxes[0], boneAxes[1], 1);
        gunView.sprite = gripFrames[3]; gunView.flipX = true;
        Vector3 actualGrip = gunPalm.position + gunPalm.rotation * (Vector3.up * PalmContact * transform.lossyScale.x);
        int frame = 0;
        if (weaponView.sprite != null) int.TryParse(weaponView.sprite.name.Replace("weapon_", ""), out frame);
        float offset = WeaponRecoilPixels[Mathf.Clamp(frame, 0, WeaponRecoilPixels.Length - 1)];
        weapon.rotation = rotation;
        Vector3 handOffset = new Vector3(offset / 100, 0);
        weapon.position = actualGrip - weapon.TransformVector(handOffset);
        // Original v04 metadata: hand(86+offset,60), mouth(32+offset,39), pivot(86,60).
        muzzle.localPosition = new Vector3((32 + offset - 86) / 100, (60 - 39) / 100f, 0);
        WeaponGripError = Vector3.Distance(weapon.TransformPoint(handOffset), actualGrip);
    }
    private Vector3 SolveArm(Transform upper, Transform lower, Vector3 start, Vector3 target, Vector2 upperAxis, Vector2 lowerAxis, float bend)
    {
        float scale = transform.lossyScale.x * limbScale, a = upperAxis.magnitude * scale, b = lowerAxis.magnitude * scale;
        Vector2 delta = target - start; float distance = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + .005f, a + b - .005f);
        Vector2 direction = delta.sqrMagnitude > .0001f ? delta.normalized : Vector2.up;
        Vector2 endpoint = (Vector2)start + direction * distance;
        limbLengths[0] = a; limbLengths[1] = b; limbPositions[0] = start;
        Limb.Solve(endpoint, limbLengths, limbPositions, ref limbAngles);
        Vector2 elbow = (Vector2)start + (Vector2)(Quaternion.Euler(0, 0, Angle(direction) + bend * limbAngles[0]) * Vector3.right) * a;
        upper.position = start; lower.position = elbow;
        upper.rotation = Quaternion.Euler(0, 0, Angle(elbow - (Vector2)start) - Angle(upperAxis));
        lower.rotation = Quaternion.Euler(0, 0, Angle(endpoint - elbow) - Angle(lowerAxis));
        return endpoint;
    }
    private void PoseTear()
    {
        float t = Mathf.Clamp01(tearProgress), pull = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.33f, .82f, t));
        float release = Mathf.InverseLerp(.86f, 1, t);
        float roll = -pull * 5 + Mathf.Sin(t * 75) * Mathf.Sin(t * Mathf.PI) * 1.8f;
        roll = Mathf.LerpAngle(tearStartRoll, roll, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .15f, t)));
        visualRig.localRotation = Quaternion.Euler(0, 0, roll);
        Vector3 pivot = Vector3.up * 1.1f;
        visualRig.localPosition = basePosition + pivot - visualRig.localRotation * pivot;
        actionSprite.sprite = t >= .33f ? halfCore : closedCore;
        peelLeft.transform.localPosition = peelLeftOrigin + new Vector3(-.4f * pull, -.6f * release, 0);
        peelRight.transform.localPosition = peelRightOrigin + new Vector3(.25f * pull, -.4f * release, 0);
        peelLeft.transform.localRotation = Quaternion.Euler(0, 0, pull * 68);
        peelRight.transform.localRotation = Quaternion.Euler(0, 0, -pull * 46);
        peelLeft.color = peelRight.color = new Color(1, 1, 1, 1 - release);
        PoseSupport();
        // Fingers close on an actual point of the peel; the arm follows that piece as it tears away.
        Vector3 contact = peelLeft.transform.TransformPoint(new Vector3(-.28f, 1.73f, 0));
        Vector3 handTarget = Vector3.Lerp(tearGunStart, contact, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.03f, .25f, t)));
        if (release > 0) handTarget = Vector3.Lerp(contact, tearGunStart, release);
        gunPalm.rotation = peelLeft.transform.rotation;
        Vector3 shoulder = Shoulder(0);
        gunShoulderCover.SetPositionAndRotation(shoulder, visualRig.rotation);
        Vector3 wrist = handTarget - gunPalm.rotation * (Vector3.up * PalmContact * transform.lossyScale.x);
        gunPalm.position = SolveArm(farArm, gunForearm, shoulder, wrist, boneAxes[0], boneAxes[1], 1);
        int drawing = t < .15f ? 0 : t < .24f ? 1 : t < .3f ? 2 : release > .2f ? 0 : 3;
        gunView.sprite = gripFrames[drawing];
        PeelGripError = t >= .3f && release <= 0 ? Vector3.Distance(gunPalm.position + gunPalm.rotation * (Vector3.up * PalmContact * transform.lossyScale.x), contact) : 0;
    }
    private static float Angle(Vector2 value) => Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;
    private static void Circle(LineRenderer line, Vector3 center, float radius, Color color)
    {
        line.enabled = color.a > .01f; line.positionCount = 32; line.startColor = line.endColor = color;
        for (int i = 0; i < 32; i++) { float a = i * Mathf.PI * 2 / 32; line.SetPosition(i, center + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
    }
}
