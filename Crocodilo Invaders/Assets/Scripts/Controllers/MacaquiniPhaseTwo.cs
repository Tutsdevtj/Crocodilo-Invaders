using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Boss), typeof(MacaquiniCombat))]
public sealed class MacaquiniPhaseTwo : MonoBehaviour
{
    public enum Stage { ClosedShell, Tearing, HalfShell, Stopped }
    [SerializeField] private int secondFormHealth = 180;
    private Boss boss;
    private MacaquiniCombat combat;
    private MacaquiniPresentation presentation;
    private Health target;
    private Animator animator;
    private Coroutine sequence;
    private bool clipDone;
    public Stage CurrentStage { get; private set; }
    public event Action TransitionStarted, SecondFormStarted;
    private void Awake()
    {
        boss = GetComponent<Boss>(); combat = GetComponent<MacaquiniCombat>();
        presentation = GetComponent<MacaquiniPresentation>(); animator = GetComponent<Animator>();
    }
    private void OnEnable() { GetComponent<Health>().Depleted += OnDepleted; boss.BattleStopped += Stop; }
    private void OnDisable() { GetComponent<Health>().Depleted -= OnDepleted; boss.BattleStopped -= Stop; Stop(); }
    private void OnDepleted()
    {
        if (CurrentStage != Stage.ClosedShell || !boss.BattleActive) return;
        target = combat.Target;
        if (target == null || target.IsDead) return;
        CurrentStage = Stage.Tearing; boss.SetTransitioning(true); combat.StopCombat();
        TransitionStarted?.Invoke();
        boss.Health.RestoreNextPhase(secondFormHealth);
        target.Died += OnTargetDied;
        GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .055f, .2f);
        presentation.BeginTransformation();GetComponent<MacaquiniFlightPresentation>()?.BeginTearSequence();clipDone = false; animator.Play("Tear", 0, 0);
        sequence = StartCoroutine(Transform());
    }
    private IEnumerator Transform()
    {
        float elapsed = 0;
        var flight=GetComponent<MacaquiniFlightPresentation>();
        var body=GetComponent<Rigidbody2D>();Vector2 start=body.position;
        while (!clipDone && target != null && !target.IsDead && elapsed < 9f)
        {
            elapsed += Time.deltaTime;
            // Once the broken pack leaves, gravity pulls him down until the vine catches his weight.
            // Movement is held and hitboxes are disabled throughout this authored transition.
            if(flight!=null){float progress=flight.TearProgress;float fall=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.625f,.90f,progress));float catchRebound=Mathf.Sin(Mathf.InverseLerp(.90f,1,progress)*Mathf.PI)*.07f;body.position=start+Vector2.down*(fall*.65f-catchRebound);}
            yield return null;
        }
        if (!clipDone) { if (target != null && !target.IsDead) Debug.LogError("Macaquini tear animation did not complete.", this); Stop(); yield break; }
        CurrentStage = Stage.HalfShell;
        presentation.EndTransformation(true); animator.Play("Idle", 0, 0);
        boss.SetTransitioning(false); boss.Movement.ReturnToPatrol(); combat.EnterPhaseTwo(); combat.SetTarget(target);
        SecondFormStarted?.Invoke(); sequence = null;
    }
    // Native final-frame event: the shell and broken jetpack are discarded before the vine is caught.
    public void MacaquiniTearComplete()
    {
        clipDone = true; GusiniPhaseTwoCameraFeedback.Pulse(Camera.main, .075f, .18f);
    }
    // Stopping just the coroutine leaves presentation/audio/debris alive. Use the battle shutdown path.
    private void OnTargetDied() => boss.StopBattle();
    private void Stop()
    {
        if (target != null) target.Died -= OnTargetDied;
        target = null;
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
        if (CurrentStage == Stage.Tearing)
        {
            animator.Play("Idle", 0, 0); presentation.EndTransformation(false);
            boss.SetTransitioning(false);
        }
        CurrentStage = Stage.Stopped;
    }
}
