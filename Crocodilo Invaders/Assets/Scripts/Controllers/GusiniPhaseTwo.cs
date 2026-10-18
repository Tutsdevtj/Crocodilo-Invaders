using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// A F1 mantém seu controlador. Este componente assume só após o esgotamento da F1.
[RequireComponent(typeof(Boss), typeof(GusiniCombat))]
public sealed class GusiniPhaseTwo : MonoBehaviour
{
    // Defeated: o parasita morre e GusiniDefeat assume a apresentação; o boss não some.
    public enum Stage { PhaseOne, Transforming, PhaseTwo, Stopped, Defeated }
    public enum Attack { Energy, Egg, Squadron, Tentacles }
    [SerializeField] private GusiniPhaseTwoAssets assets;
    [SerializeField] private RuntimeAnimatorController phaseTwoController;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(1)] private int phaseTwoHealth = 320;
    [SerializeField, Min(0)] private float recovery = .8f;
    [SerializeField, Min(1)] private int eggsPerAttack = 3;
    [SerializeField, Min(0f)] private float eggSpacingPixels = 32f;
    [SerializeField, Min(0f)] private float patrolSpeedMultiplier = 1.5f;
    private Boss boss;
    private Health target;
    private Animator animator;
    private GusiniAudio audio;
    private Transform threats;
    private bool clipDone, stopped, firstEnergy = true;
    private string playing;
    private int energyIndex, safeLane, bagIndex;
    private readonly Attack[] bag = { Attack.Energy, Attack.Egg, Attack.Squadron };
    private Vector3 visualOrigin;
    private float visualClock, shakeUntil;
    public Stage CurrentStage { get; private set; }
    public Attack CurrentAttack { get; private set; }
    public bool WeakpointOpen { get; private set; }
    public float TransformationTime { get; private set; }
    public float RevealTime { get; private set; } = -1f;
    public Transform ThreatRoot => threats;
    public event Action TransitionStarted;
    // Evento nativo do clip (690 ms): a pose de falsa derrota, antes do primeiro sinal da corrupção.
    public event Action ApparentDefeatHeld;
    // Esgotamento da F2, antes de Died. Sem ouvinte, a derrota antiga (fade) continua valendo.
    public event Action PhaseTwoDefeated;
    public event Action PhaseTwoRevealed;
    public event Action PhaseTwoStarted;
    public event Action<Attack> AttackStarted;
    public event Action<string, GameObject> Released;
    private bool Fighting => !stopped && boss.BattleActive && target != null && !target.IsDead;

    private void Awake()
    {
        boss = GetComponent<Boss>(); animator = GetComponent<Animator>(); audio = GetComponent<GusiniAudio>();
        visualOrigin = visual.transform.localPosition;
    }
    private void OnEnable()
    {
        GetComponent<Health>().Depleted += OnDepleted;
        boss.BattleStopped += Stop;
    }
    private void OnDisable()
    {
        GetComponent<Health>().Depleted -= OnDepleted;
        boss.BattleStopped -= Stop;
        Stop();
    }

    private void OnDepleted()
    {
        if (CurrentStage == Stage.PhaseTwo && !stopped && PhaseTwoDefeated != null)
        {
            ClearCombat();
            CurrentStage = Stage.Defeated;
            PhaseTwoDefeated.Invoke();
            return;
        }
        if (CurrentStage != Stage.PhaseOne || !boss.BattleActive || assets == null || phaseTwoController == null) return;
        target = GetComponent<GusiniCombat>().Target;
        if (target == null || target.IsDead) return;
        CurrentStage = Stage.Transforming;
        boss.SetTransitioning(true);
        GetComponent<GusiniCombat>().StopCombat(); // cancela releases e remove toda ameaça da F1
        GetComponent<GusiniPresentation>().enabled = false;
        animator.runtimeAnimatorController = phaseTwoController;
        animator.speed = 1f; // tempos nativos da F2, sem herdar os 2x da F1
        target.Died += Stop;
        TransformationTime = 0f; RevealTime = -1f;
        GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .045f, .13f);
        TransitionStarted?.Invoke();
        boss.Health.RestoreNextPhase(phaseTwoHealth);
        if (audio != null) audio.BeginPhaseTwo();
        StartCoroutine(TransformAndFight());
    }

    private IEnumerator TransformAndFight()
    {
        yield return PlayClip("P2Transform");
        if (!Fighting) yield break;
        CurrentStage = Stage.PhaseTwo;
        boss.Movement.SetPatrolSpeedMultiplier(patrolSpeedMultiplier);
        boss.SetTransitioning(false);
        PhaseTwoStarted?.Invoke();
        while (Fighting)
        {
            if (bagIndex == 3)
            {
                for (int i = 2; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    (bag[i], bag[j]) = (bag[j], bag[i]);
                }
                bagIndex = 0;
            }
            CurrentAttack = bag[bagIndex++];
            yield return Perform(CurrentAttack);
            if (bagIndex == 3 && Fighting) yield return Perform(Attack.Tentacles);
        }
    }

    private IEnumerator Perform(Attack attack)
    {
        if (!Fighting) yield break;
        CurrentAttack = attack;
        AttackStarted?.Invoke(attack);
        boss.Movement.HoldPosition(true);
        if (attack == Attack.Energy)
        {
            energyIndex = 0;
            yield return PlayClip(firstEnergy ? "P2EnergyFirst" : "P2Energy");
            firstEnergy = false;
        }
        else if (attack == Attack.Egg) yield return PlayClip("P2Egg");
        else if (attack == Attack.Squadron) yield return PlayClip("P2Call");
        else
        {
            yield return PlayClip("P2Start");
            int[] safeOrder = { 1, 0, 2 };
            for (int strike = 0; strike < 3 && Fighting; strike++)
            {
                safeLane = safeOrder[strike];
                CreateLanes(true);
                PlayLoop("P2Windup");
                yield return new WaitForSeconds(.9f);
                yield return PlayClip("P2Lash");
                // A animação do corpo termina antes do tentáculo de faixa recolher.
                yield return new WaitForSeconds(.56f);
            }
            if (Fighting)
            {
                yield return PlayClip("P2Resist");
                weakpointSoundPlayed = false;
                PlayLoop("P2Weak");
                WeakpointOpen = true;
                boss.Health.IncomingDamageMultiplier = 2f;
                yield return new WaitForSeconds(2f);
                WeakpointOpen = false;
                boss.Health.IncomingDamageMultiplier = 1f;
                yield return PlayClip("P2Close");
            }
        }
        if (!Fighting) yield break;
        PlayLoop("P2Idle");
        boss.Movement.HoldPosition(false);
        yield return new WaitForSeconds(recovery);
    }

    private IEnumerator PlayClip(string state)
    {
        if (!Fighting) yield break;
        playing = state; clipDone = false;
        animator.Play(state, 0, 0f);
        while (!clipDone && Fighting) yield return null;
    }
    private void PlayLoop(string state) { playing = state; animator.Play(state, 0, 0f); }

    // Animation Events em milissegundos, criados a partir dos JSONs fornecidos.
    public void OnPhaseTwoEvent(string value)
    {
        if (!Fighting) return;
        if (value == "clip_end") { clipDone = true; return; }
        if (CurrentStage == Stage.Transforming)
        {
            if (audio != null)
            {
                switch (value)
                {
                    case "final_hit_frame": audio.Play(GusiniSound.P2FinalHit, transform.position); break;
                    case "start_stagger_frame": audio.Play(GusiniSound.P2EngineStall, transform.position); break;
                    case "first_sign_frame": audio.Play(GusiniSound.P2FirstPulse, transform.position); break;
                    case "corruption_start_frame": audio.Play(GusiniSound.P2CorruptionSurge, transform.position); break;
                    case "transformation_peak_frame": audio.Play(GusiniSound.P2MutationPeak, transform.position); break;
                    case "return_pose_frame": audio.Play(GusiniSound.P2RevealCry, transform.position); break;
                }
            }
            if (value == "start_stagger_frame")
            {
                var pickups = FindObjectsByType<HealthPickupSpawner>(FindObjectsSortMode.None);
                foreach (var pickup in pickups)
                    if (pickup.gameObject.scene == gameObject.scene) { pickup.TrySpawnForBossTransition(); break; }
            }
            if (value == "apparent_defeat_hold") ApparentDefeatHeld?.Invoke();
            if (value == "phase2_reveal_frame")
            {
                RevealTime = 0f; PhaseTwoRevealed?.Invoke();
            }
            if (value == "transformation_peak_frame")
            {
                shakeUntil = Time.time + .25f;
                GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .14f, .25f);
            }
            return;
        }
        if (playing.StartsWith("P2Energy", StringComparison.Ordinal) && value == "telegraph_start" && audio != null)
            audio.Play(GusiniSound.P2EnergyCharge, transform.position);
        else if (playing == "P2Start" && value == "telegraph_start" && audio != null)
            audio.Play(GusiniSound.P2ModuleCrack, transform.position);
        else if (playing == "P2Start" && value == "tentacles_emerge" && audio != null)
            audio.Play(GusiniSound.P2TentacleEmerge, transform.position);
        else if (playing == "P2Resist" && value == "resist_start" && audio != null)
            audio.Play(GusiniSound.P2ResistCry, transform.position);
        else if (playing == "P2Weak" && value == "weakpoint_open" && audio != null)
        {
            // O loop visual F4/F5 repete; o som pertence à abertura, uma única vez.
            if (!weakpointSoundPlayed) { weakpointSoundPlayed = true; audio.Play(GusiniSound.P2WeakpointOpen, transform.position); }
        }
        else if (playing == "P2Close" && value == "weakpoint_close" && audio != null)
            audio.Play(GusiniSound.P2WeakpointClose, transform.position);
        else if (playing.StartsWith("P2Energy", StringComparison.Ordinal) && value.StartsWith("fire_", StringComparison.Ordinal))
            SpawnBall();
        else if (playing == "P2Egg" && value == "egg_drop") SpawnEgg();
        else if (playing == "P2Call" && value == "edge_warning") SpawnSquadron(false);
        else if (playing == "P2Call" && value == "squadron_spawn") SpawnSquadron(true);
        else if (playing == "P2Egg" && value == "hatch_open" && audio != null) audio.Play(GusiniSound.HatchOpen, transform.position);
        else if (playing == "P2Call" && value == "call_start" && audio != null) audio.Play(GusiniSound.P2CorruptCall, transform.position);
        else if (playing == "P2Lash" && value == "lash_hit") CreateLanes(false);
    }

    private void EnsureThreats()
    {
        if (threats != null) return;
        var root = new GameObject("Gusini F2 threats");
        SceneManager.MoveGameObjectToScene(root, gameObject.scene);
        threats = root.transform;
    }
    private Vector3 AtPixel(float x, float y) => visual.transform.TransformPoint(new Vector3(x / 100f, y / 100f));
    private float PixelScale => Mathf.Abs(visual.transform.lossyScale.x) / 100f;
    private void SpawnBall()
    {
        EnsureThreats();
        var ball = GusiniEnergyBall.Create(assets, threats, AtPixel(-138f, -24f), PixelScale, energyIndex++, audio);
        Released?.Invoke("energy", ball.gameObject);
        if (audio != null) audio.Play(GusiniSound.P2EnergyFire, ball.transform.position);
        shakeUntil = Time.time + .08f;
    }
    private void SpawnEgg()
    {
        EnsureThreats();
        Vector3 origin = AtPixel(-26f, -34f);
        var burstPoints = new Vector3[eggsPerAttack];
        for (int i = 0; i < eggsPerAttack; i++)
        {
            Vector3 position = origin + Vector3.left * (i * eggSpacingPixels * PixelScale);
            burstPoints[i] = ChooseEggBurstPoint(burstPoints, i);
            float travelDuration = .65f + i * .55f + UnityEngine.Random.Range(0f, .12f);
            var egg = GusiniCorruptedEgg.Create(assets, threats, position, PixelScale, audio, visual.sortingOrder, burstPoints[i], travelDuration);
            egg.Burst += () => Released?.Invoke("egg_burst", egg.gameObject);
            Released?.Invoke("egg", egg.gameObject);
        }
        if (audio != null) audio.Play(GusiniSound.EggDrop, origin);
    }
    private Vector3 ChooseEggBurstPoint(Vector3[] chosen, int count)
    {
        Rect arena = Arena;
        float margin = Mathf.Min(.8f, Mathf.Min(arena.width, arena.height) * .15f);
        float separation = Mathf.Min(2.2f, Mathf.Min(arena.width, arena.height) * .3f);
        Vector3 best = Vector3.zero;
        float bestDistance = -1f;
        for (int attempt = 0; attempt < 64; attempt++)
        {
            Vector3 candidate = new Vector3(UnityEngine.Random.Range(arena.xMin + margin, arena.xMax - margin),
                UnityEngine.Random.Range(arena.yMin + margin, arena.yMax - margin), 0f);
            float nearest = float.PositiveInfinity;
            for (int j = 0; j < count; j++) nearest = Mathf.Min(nearest, Vector3.Distance(candidate, chosen[j]));
            if (nearest >= separation) return candidate;
            if (nearest > bestDistance) { best = candidate; bestDistance = nearest; }
        }
        return best;
    }
    private void SpawnSquadron(bool spawn)
    {
        EnsureThreats();
        if (!spawn)
        {
            if (audio != null) audio.Play(GusiniSound.P2EdgeMark, transform.position);
            squadronCenter = Mathf.Lerp(Arena.yMin + Arena.height * .3f, Arena.yMax - Arena.height * .3f, UnityEngine.Random.value);
            for (int i = 0; i < 5; i++)
            {
                Vector2 slot = Formation(i);
                var marker = GusiniPhaseTwoEffect.Create(assets.Frames("fx_edge_warning_", 2), new[] { .12f, .12f }, threats,
                    new Vector3(Arena.xMax - .2f, squadronCenter + slot.y * PixelScale, 0), PixelScale, true,
                    .28f + (80f + slot.x) / GusiniCorruptedGoose.FormationSpeedPixels);
                Released?.Invoke("warning", marker.gameObject);
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 slot = Formation(i);
                Vector3 pos = new Vector3(Arena.xMax + (80f + slot.x) * PixelScale, squadronCenter + slot.y * PixelScale, 0);
                var goose = GusiniCorruptedGoose.Create(assets, threats, pos, PixelScale, i, target,
                    i == 1 || i == 4, Arena, audio);
                Released?.Invoke("goose", goose.gameObject);
            }
        }
    }
    private float squadronCenter;
    private bool weakpointSoundPlayed;
    private static Vector2 Formation(int i) => i == 0 ? Vector2.zero : new Vector2(i < 3 ? 40 : 80, (i % 2 == 0 ? 1 : -1) * (i < 3 ? 26 : 52));
    private void CreateLanes(bool warning)
    {
        EnsureThreats();
        Rect arena = Arena;
        if (audio != null) audio.Play(warning ? GusiniSound.P2LaneWarning : GusiniSound.P2Lash, transform.position);
        bool firstLane = true;
        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == safeLane) continue;
            float y = arena.yMin + arena.height * ((lane + .5f) / 3f);
            var tentacle = GusiniTentacleLane.Create(assets, threats, arena, y, warning, audio, firstLane);
            firstLane = false;
            Released?.Invoke(warning ? "lane_warning" : "lane", tentacle.gameObject);
        }
    }
    public Rect Arena
    {
        get
        {
            Camera camera = Camera.main;
            if (camera == null) return new Rect(-8f, -3f, 16f, 9.6f);
            Vector3 low = camera.ViewportToWorldPoint(new Vector3(0, 0, -camera.transform.position.z));
            Vector3 high = camera.ViewportToWorldPoint(new Vector3(1, 1, -camera.transform.position.z));
            return Rect.MinMaxRect(low.x, low.y, high.x, high.y);
        }
    }
    private void Update()
    {
        if (CurrentStage == Stage.Transforming)
        {
            TransformationTime += Time.deltaTime;
            if (RevealTime >= 0f) RevealTime += Time.deltaTime;
        }
    }
    private void LateUpdate()
    {
        if (CurrentStage == Stage.Stopped && boss.Health.IsDead)
        {
            Color color = visual.color;
            color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime / boss.DefeatDuration);
            visual.color = color;
            return;
        }
        if (CurrentStage != Stage.PhaseTwo && CurrentStage != Stage.Transforming) return;
        visualClock += Time.deltaTime;
        float bob = CurrentStage == Stage.PhaseTwo ? Mathf.Sin(visualClock * 2f) * .025f : 0f;
        float shake = Time.time < shakeUntil ? Mathf.Sin(visualClock * 160f) * .025f : 0f;
        visual.transform.localPosition = visualOrigin + new Vector3(shake, bob, 0f);
    }
    public void Stop()
    {
        if (CurrentStage == Stage.PhaseOne || CurrentStage == Stage.Defeated) return;
        ClearCombat();
        CurrentStage = Stage.Stopped;
        // Sem uma animação de morte fornecida, conserva a pose reconhecível e faz fade.
        if (animator != null && animator.runtimeAnimatorController == phaseTwoController) animator.speed = 0f;
    }

    private void ClearCombat()
    {
        stopped = true;
        StopAllCoroutines();
        if (target != null) target.Died -= Stop;
        target = null;
        WeakpointOpen = false;
        boss.Movement.SetPatrolSpeedMultiplier(1f);
        boss.Health.IncomingDamageMultiplier = 1f;
        if (audio != null) audio.StopAll();
        if (threats != null) { threats.gameObject.SetActive(false); Destroy(threats.gameObject); threats = null; }
        visual.transform.localPosition = visualOrigin;
    }
}
