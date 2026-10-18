using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossHUD : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private GameObject content;
    [SerializeField] private Image healthBar;
    [SerializeField] private Sprite[] healthSprites;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private BossBarFX gusiniBar;
    private Boss boss;
    private Health health;
    private BossCombat legacyCombat;
    private bool showingGusini;
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
        content.SetActive(false);
        if (gusiniBar != null) gusiniBar.gameObject.SetActive(false);
        if (target == null || target.GetComponent<Health>().IsDead) return;
        boss = target;
        health = target.GetComponent<Health>();
        health.HealthChanged += Refresh;
        showingGusini = gusiniBar != null && target.GetComponent<GusiniCombat>() != null;
        if (showingGusini)
        {
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
            return;
        }
        legacyCombat = target.GetComponent<BossCombat>();
        if (legacyCombat != null) legacyCombat.EnrageStarted += ShowEnrage;
        content.SetActive(true);
        nameText.text = boss.DisplayName;
        if (portraitImage != null)
        {
            portraitImage.sprite = boss.Portrait;
            portraitImage.enabled = boss.Portrait != null;
        }
        nameText.color = Color.white;
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void HoldPhaseOne() => gusiniBar.HoldEmpty();
    private void RevealPhaseTwo()
    {
        if (phaseTwoBar != null) phaseTwoBar.Begin(health.Fraction, Mathf.Max(0f, phaseTwo.RevealTime));
    }

    private void Refresh(int current, int maximum)
    {
        if (showingGusini)
        {
            if (phaseTwo != null && phaseTwo.CurrentStage == GusiniPhaseTwo.Stage.Transforming) return;
            if (phaseTwoBar != null) phaseTwoBar.SetHealth(health.Fraction);
            gusiniBar.SetHealth(health.Fraction);
            return;
        }
        int stages = healthSprites.Length - 1;
        int index = Mathf.Clamp(stages - Mathf.CeilToInt(health.Fraction * stages), 0, stages);
        healthBar.sprite = healthSprites[index];
    }

    private void ShowEnrage() => nameText.color = new Color(1f, .5f, .35f);
    private void Hide(bool completed)
    {
        Unsubscribe();
        content.SetActive(false);
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
        if (legacyCombat != null) legacyCombat.EnrageStarted -= ShowEnrage;
        legacyCombat = null;
        health = null;
        boss = null;
        showingGusini = false;
    }
}
