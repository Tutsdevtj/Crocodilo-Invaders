using UnityEngine;

public sealed class BossHUD : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private BossBarFX gusiniBar;
    [SerializeField] private BossBarFX macaquiniBar;
    private Health health;
    private GusiniPhaseTwo phaseTwo;
    private BossBarPhaseTwo phaseTwoBar;
    private BossBarFX otherBar, activeBar;
    private MacaquiniPhaseTwo macaquiniPhase;
    public BossBarFX ActiveBar => activeBar;
    private void OnEnable()
    {
        if (waves != null) { waves.BossStarted += Bind; waves.RunEnded += Hide; }
        if (waves != null && waves.Phase == WaveController.RunPhase.Boss && waves.ActiveBoss != null) Bind(waves.ActiveBoss);
        else Hide(false);
    }
    private void OnDisable()
    {
        if (waves != null) { waves.BossStarted -= Bind; waves.RunEnded -= Hide; }
        Hide(false);
    }
    private void Bind(Boss target)
    {
        Unsubscribe();
        if (gusiniBar != null) gusiniBar.gameObject.SetActive(false);
        if (otherBar != null) otherBar.gameObject.SetActive(false);
        activeBar = null;
        if (target == null || target.Health.IsDead || gusiniBar == null) return;
        health = target.Health; health.HealthChanged += Refresh; health.Died += OnBossDied;
        bool gusini = target.GetComponent<GusiniCombat>() != null;
        if (!gusini)
        {
            if (otherBar == null)
            {
                otherBar = macaquiniBar != null ? macaquiniBar : Instantiate(gusiniBar, gusiniBar.transform.parent);
                otherBar.name = "MacaquiniBar";
            }
            activeBar = otherBar;
        }
        else activeBar = gusiniBar;
        activeBar.gameObject.SetActive(true);
        var phaseSkin = activeBar.GetComponent<BossBarPhaseTwo>();
        if (phaseSkin != null) { phaseSkin.ResetPhase(); phaseSkin.enabled = gusini; }
        if (!gusini)
        {
            var macaquini = target.GetComponent<MacaquiniPresentation>();
            if (macaquini != null) activeBar.SetIdentity(macaquini.HudTitle, macaquini.HudPortrait);
        }
        activeBar.Show(health.Fraction);
        if (activeBar.JungleView != null) activeBar.JungleView.Bind(target.GetComponent<MacaquiniFlightPresentation>());
        macaquiniPhase = target.GetComponent<MacaquiniPhaseTwo>();
        if (macaquiniPhase != null)
        {
            macaquiniPhase.TransitionStarted += HoldMacaquini;
            macaquiniPhase.SecondFormStarted += RevealMacaquini;
            if (macaquiniPhase.CurrentStage == MacaquiniPhaseTwo.Stage.Tearing) HoldMacaquini();
            else if (macaquiniPhase.CurrentStage == MacaquiniPhaseTwo.Stage.HalfShell) RevealMacaquini();
        }
        phaseTwo = gusini ? target.GetComponent<GusiniPhaseTwo>() : null;
        phaseTwoBar = gusini ? phaseSkin : null;
        if (phaseTwo == null) return;
        phaseTwo.TransitionStarted += HoldPhaseOne; phaseTwo.PhaseTwoRevealed += RevealPhaseTwo;
        if (phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.Transforming)
        {
            HoldPhaseOne(); if (phaseTwo.RevealTime >= 0f) RevealPhaseTwo();
        }
        else if (phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.PhaseTwo) RevealPhaseTwo();
    }
    private void HoldPhaseOne() => gusiniBar.HoldEmpty();
    private void HoldMacaquini() { if (activeBar != null) activeBar.HoldEmpty(); }
    private void RevealMacaquini()
    {
        if (activeBar == null) return;
        var portrait = macaquiniPhase.GetComponent<MacaquiniPresentation>();
        activeBar.SetIdentity(portrait.HudTitle, portrait.HudPortraitHalf);
        activeBar.BeginPhaseTwoCharge(health.Fraction);
    }
    private void RevealPhaseTwo()
    {
        if (phaseTwoBar != null) phaseTwoBar.Begin(health.Fraction, Mathf.Max(0f, phaseTwo.RevealTime));
    }
    private void Refresh(int current, int maximum)
    {
        if (phaseTwo != null && phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.Transforming) return;
        if (phaseTwoBar != null) phaseTwoBar.SetHealth(health.Fraction);
        if (activeBar != null) activeBar.SetHealth(health.Fraction);
    }
    private void OnBossDied()
    {
        // Chapter endings do not emit the legacy RunEnded event.
        if (activeBar != null && activeBar.JungleView != null) Hide(false);
    }
    private void Hide(bool completed)
    {
        Unsubscribe();
        if (gusiniBar != null) gusiniBar.gameObject.SetActive(false);
        if (otherBar != null) otherBar.gameObject.SetActive(false);
        activeBar = null;
    }
    private void Unsubscribe()
    {
        if (macaquiniPhase != null) { macaquiniPhase.TransitionStarted -= HoldMacaquini; macaquiniPhase.SecondFormStarted -= RevealMacaquini; }
        macaquiniPhase = null;
        if (phaseTwo != null) { phaseTwo.TransitionStarted -= HoldPhaseOne; phaseTwo.PhaseTwoRevealed -= RevealPhaseTwo; }
        phaseTwo = null; phaseTwoBar = null;
        if (health != null) { health.HealthChanged -= Refresh; health.Died -= OnBossDied; }
        health = null;
    }
}
