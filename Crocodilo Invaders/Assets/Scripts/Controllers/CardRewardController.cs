using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CardRewardController : MonoBehaviour
{
    public enum RewardState { Closed, Revealing, Ready, Choosing }
    [SerializeField] private WaveController waves;
    [SerializeField] private Player player;
    [SerializeField] private PauseController pause;
    [SerializeField] private UpgradeCardCatalog catalog;
    [SerializeField] private CardRewardView view;
    private PlayerRunUpgrades upgrades;
    private PlayerInputReader playerInput;
    private readonly System.Random random = new System.Random();
    private UpgradeCard[] offers;
    private bool ownsFreeze;
    private bool inputWasEnabled;
    private float previousTimeScale;
    public RewardState State { get; private set; }
    public IReadOnlyList<UpgradeCard> Offers => offers;
    public int RerollsRemaining { get; private set; }
    public bool HasUnlimitedRerolls => DevelopmentCardSession.IsActive;
    public float RewardProgress { get; private set; }
    public bool CanInteract => State == RewardState.Ready && (pause == null || !pause.IsPaused);

    private void Awake()
    {
        upgrades = player != null ? player.GetComponent<PlayerRunUpgrades>() : null;
        playerInput = player != null ? player.GetComponent<PlayerInputReader>() : null;
        if (waves != null && upgrades != null && view != null && catalog != null && catalog.IsValid) return;
        Debug.LogError("Configure player, catálogo, ondas e apresentação no CardRewardController.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        waves.WaveCleared += OnWaveCleared;
        waves.RunEnded += OnRunEnded;
        view.ChoiceRequested += OnChoice;
        view.RerollRequested += OnReroll;
        if (pause != null) { pause.PauseChanged += OnPauseChanged; pause.MenuReturnRequested += OnMenuReturn; }
    }

    private void OnWaveCleared(int wave, int points)
    {
        if (State != RewardState.Closed || !waves.RequestWaveReward()) return;
        previousTimeScale = Time.timeScale;
        ownsFreeze = true;
        inputWasEnabled = playerInput != null && playerInput.enabled;
        if (playerInput != null) playerInput.enabled = false;
        Time.timeScale = 0f;
        if (pause != null) pause.AllowPauseWhileFrozen(true);
        RerollsRemaining = 1;
        int rewardCount = Mathf.Min(waves.CombatWaveCount, waves.WaveCount - 1);
        RewardProgress = rewardCount > 1 ? Mathf.Clamp01((wave - 1f) / (rewardCount - 1)) : 0f;
        offers = DevelopmentCardSession.RollOffers(catalog, random, RewardProgress);
        State = RewardState.Revealing;
        StartCoroutine(Reveal(false));
    }

    private IEnumerator Reveal(bool reroll)
    {
        if (reroll) yield return view.Reroll(offers, catalog, upgrades);
        else yield return view.Open(waves.CurrentWave, offers, catalog, upgrades);
        State = RewardState.Ready;
        RefreshInteraction();
    }

    public bool TryReroll()
    {
        if (!CanInteract || (!HasUnlimitedRerolls && RerollsRemaining <= 0)) return false;
        if (!HasUnlimitedRerolls) RerollsRemaining--;
        State = RewardState.Revealing;
        RefreshInteraction();
        offers = DevelopmentCardSession.RollOffers(catalog, random, RewardProgress);
        StartCoroutine(Reveal(true));
        return true;
    }

    public bool TryChoose(int index)
    {
        if (!CanInteract || offers == null || index < 0 || index >= offers.Length) return false;
        State = RewardState.Choosing;
        RefreshInteraction();
        if (!upgrades.Apply(offers[index]))
        {
            State = RewardState.Ready;
            RefreshInteraction();
            return false;
        }
        StartCoroutine(FinishChoice(index));
        return true;
    }

    private IEnumerator FinishChoice(int index)
    {
        yield return view.Choose(index, upgrades);
        State = RewardState.Closed;
        RestoreFreeze();
        waves.CompleteWaveReward();
    }

    private void Update()
    {
        if (!CanInteract || Keyboard.current == null) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard.rKey.wasPressedThisFrame) TryReroll();
        else if (keyboard.digit1Key.wasPressedThisFrame) TryChoose(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) TryChoose(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) TryChoose(2);
    }

    private void OnChoice(int index) => TryChoose(index);
    private void OnReroll() => TryReroll();
    private void OnPauseChanged(bool paused)
    {
        view.SetPaused(paused);
        RefreshInteraction();
    }
    private void RefreshInteraction() => view.SetInteraction(CanInteract, RerollsRemaining);
    private void OnMenuReturn() => previousTimeScale = 1f;

    private void RestoreFreeze()
    {
        if (!ownsFreeze) return;
        ownsFreeze = false;
        if (pause != null)
        {
            pause.AllowPauseWhileFrozen(false);
            // A pausa pode estar sobre a escolha quando a cena é fechada.
            if (pause.IsPaused) pause.ResumeGame();
        }
        Time.timeScale = previousTimeScale;
        if (playerInput != null && player != null && player.Health != null && !player.Health.IsDead)
            playerInput.enabled = inputWasEnabled;
    }

    private void OnRunEnded(bool completed) => Cancel();
    private void Cancel()
    {
        StopAllCoroutines();
        State = RewardState.Closed;
        if (view != null) view.HideImmediately();
        RestoreFreeze();
        if (waves != null) waves.CompleteWaveReward();
    }

    private void OnDisable()
    {
        if (waves != null) { waves.WaveCleared -= OnWaveCleared; waves.RunEnded -= OnRunEnded; }
        if (view != null) { view.ChoiceRequested -= OnChoice; view.RerollRequested -= OnReroll; }
        if (pause != null) { pause.PauseChanged -= OnPauseChanged; pause.MenuReturnRequested -= OnMenuReturn; }
        // O relógio precisa ser liberado mesmo se as ondas/UI já foram destruídas.
        Cancel();
    }
}
