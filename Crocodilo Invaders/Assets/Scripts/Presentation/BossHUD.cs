using UnityEngine;

public sealed class BossHUD : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private BossBarFX gusiniBar;
    private Health health;
    private GusiniPhaseTwo phaseTwo;
    private BossBarPhaseTwo phaseTwoBar;

    private void OnEnable()
    {
        if (waves != null)
        {
            waves.BossStarted += Bind;
            waves.RunEnded += Hide;
        }
        if (waves != null && waves.Phase == WaveController.RunPhase.Boss && waves.ActiveBoss != null) Bind(waves.ActiveBoss);
        else Hide(false);
    }

    private void OnDisable()
    {
        if (waves != null)
        {
            waves.BossStarted -= Bind;
            waves.RunEnded -= Hide;
        }
        Hide(false);
    }

    private void Bind(Boss target)
    {
        Unsubscribe();
        if (gusiniBar != null) gusiniBar.gameObject.SetActive(false);
        if (target == null || gusiniBar == null || target.GetComponent<GusiniCombat>() == null || target.GetComponent<Health>().IsDead) return;
        health = target.GetComponent<Health>();
        health.HealthChanged += Refresh;
        gusiniBar.gameObject.SetActive(true);
        phaseTwo = target.GetComponent<GusiniPhaseTwo>();
        phaseTwoBar = gusiniBar.GetComponent<BossBarPhaseTwo>();
        if (phaseTwoBar != null) phaseTwoBar.ResetPhase();
        gusiniBar.Show(health.Fraction);
        if (phaseTwo != null)
        {
            phaseTwo.TransitionStarted += HoldPhaseOne;
            phaseTwo.PhaseTwoRevealed += RevealPhaseTwo;
            if (phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.Transforming)
            {
                HoldPhaseOne();
                if (phaseTwo.RevealTime >= 0f) RevealPhaseTwo();
            }
            else if (phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo) RevealPhaseTwo();
        }
    }

    private void HoldPhaseOne() => gusiniBar.HoldEmpty();
    private void RevealPhaseTwo()
    {
        if (phaseTwoBar != null) phaseTwoBar.Begin(health.Fraction, Mathf.Max(0f, phaseTwo.RevealTime));
    }

    private void Refresh(int current, int maximum)
    {
        if (phaseTwo != null && phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.Transforming) return;
        if (phaseTwoBar != null) phaseTwoBar.SetHealth(health.Fraction);
        gusiniBar.SetHealth(health.Fraction);
    }

    private void Hide(bool completed)
    {
        Unsubscribe();
        if (gusiniBar != null) gusiniBar.gameObject.SetActive(false);
    }

    private void Unsubscribe()
    {
        if (phaseTwo != null)
        {
            phaseTwo.TransitionStarted -= HoldPhaseOne;
            phaseTwo.PhaseTwoRevealed -= RevealPhaseTwo;
        }
        phaseTwo = null; phaseTwoBar = null;
        if (health != null) health.HealthChanged -= Refresh;
        health = null;
    }
}
