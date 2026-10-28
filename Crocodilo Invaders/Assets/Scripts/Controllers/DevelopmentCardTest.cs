using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class DevelopmentCardTest : MonoBehaviour
{
    private WaveController waves;
    private CardRewardController rewards;
    private CardRewardView rewardView;
    private PlayerRunUpgrades upgrades;
    private PauseController pause;
    private string menuScene;
    private GameObject root, panel;
    private UnityEngine.UI.Button generate, reset, mode, rarity, wave;
    private TMP_Text status;
    private bool leaving;

    public void Initialize(WaveController target, string menu)
    {
        waves = target;
        menuScene = menu;
        rewards = FindAnyObjectByType<CardRewardController>();
        rewardView = FindAnyObjectByType<CardRewardView>();
        upgrades = FindAnyObjectByType<PlayerRunUpgrades>();
        pause = FindAnyObjectByType<PauseController>();
        if (rewards == null || !rewards.isActiveAndEnabled || upgrades == null)
        {
            Debug.LogError("Teste de cartas requer os controladores reais de recompensa e upgrades.", this);
            DevelopmentCardSession.End();
            return;
        }
        TMP_FontAsset font = rewardView?.GetComponentInChildren<TMP_Text>(true)?.font;
        BuildPanel(font);
        if (!waves.StartCardDevelopment())
        {
            status.text = "Não foi possível abrir a recompensa. Volte ao menu.";
            panel.SetActive(true);
        }
    }

    private void BuildPanel(TMP_FontAsset font)
    {
        root = DevelopmentUI.Canvas("DevelopmentCardTestCanvas");
        var tab = DevelopmentUI.Button("DevelopmentControls", root.transform, font, "DEV · CARTAS  [F8]", Toggle, 32f);
        DevelopmentUI.Corner(tab, Vector2.one, new Vector2(-12f, -12f), new Vector2(200f, 32f));
        RectTransform content = DevelopmentUI.Panel("CardTestControls", root.transform, new Vector2(380f, 440f), Vector2.one, new Vector2(-12f, -54f));
        panel = content.gameObject;
        DevelopmentUI.Text("Heading", content, font, "TESTE DE CARTAS", 20f, 28f);
        mode = DevelopmentUI.Button("OfferMode", content, font, "", CycleMode, 38f);
        rarity = DevelopmentUI.Button("Rarity", content, font, "", CycleRarity, 38f);
        wave = DevelopmentUI.Button("Wave", content, font, "", CycleWave, 38f);
        generate = DevelopmentUI.Button("Generate", content, font, "GERAR NOVA OFERTA", () => { Generate(); });
        reset = DevelopmentUI.Button("ResetBonuses", content, font, "ZERAR BÔNUS E CICLO", ResetBonuses, 38f);
        DevelopmentUI.Button("ReturnToMenu", content, font, "VOLTAR AO MENU", ReturnToMenu, 38f);
        status = DevelopmentUI.Text("Status", content, font, "", 14f, 48f);
        status.color = DevelopmentUI.Muted;
        panel.SetActive(false);
        Refresh();
    }

    private void Update()
    {
        if (root == null || leaving) return;
        if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame) Toggle();
        Refresh();
    }

    private void Toggle()
    {
        if (panel == null || pause != null && pause.IsPaused) return;
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(mode.gameObject);
        else if (EventSystem.current != null)
        {
            GameObject selected = rewards.CanInteract && rewardView != null && rewardView.Options.Length > 0
                ? rewardView.Options[Mathf.Min(1, rewardView.Options.Length - 1)].Button.gameObject : null;
            EventSystem.current.SetSelectedGameObject(selected);
        }
    }

    public bool Generate()
    {
        if (!CanGenerate || !waves.RequestDevelopmentCardReward()) return false;
        panel.SetActive(false);
        return true;
    }

    public bool CanGenerate => !leaving && rewards != null && rewards.State == CardRewardController.RewardState.Closed
        && waves != null && waves.Phase == WaveController.RunPhase.Intermission && (pause == null || !pause.IsPaused);

    private void ResetBonuses()
    {
        if (!CanGenerate) return;
        upgrades.ResetForRun();
        DevelopmentCardSession.RestartSequence();
    }
    private void CycleMode() { DevelopmentCardSession.Mode = (DevelopmentCardSession.OfferMode)(((int)DevelopmentCardSession.Mode + 1) % 3); Refresh(); }
    private void CycleRarity() { DevelopmentCardSession.Rarity = (UpgradeRarity)(((int)DevelopmentCardSession.Rarity + 1) % 5); Refresh(); }
    private void CycleWave() { DevelopmentCardSession.CompletedWave = DevelopmentCardSession.CompletedWave == 1 ? 5 : DevelopmentCardSession.CompletedWave == 5 ? 9 : 1; Refresh(); }

    private void Refresh()
    {
        if (mode == null) return;
        bool editable = rewards != null && (rewards.State == CardRewardController.RewardState.Closed || rewards.CanInteract)
            && (pause == null || !pause.IsPaused);
        mode.interactable = wave.interactable = editable;
        DevelopmentUI.Label(mode, DevelopmentCardSession.ModeLabel);
        DevelopmentUI.Label(rarity, "RARIDADE: " + DevelopmentCardSession.RarityLabel);
        rarity.interactable = editable && DevelopmentCardSession.Mode == DevelopmentCardSession.OfferMode.FixedRarity;
        DevelopmentUI.Label(wave, "APÓS A ONDA " + DevelopmentCardSession.CompletedWave);
        generate.interactable = reset.interactable = CanGenerate;
        status.text = rewards == null ? "Controlador de cartas ausente." : rewards.State == CardRewardController.RewardState.Closed
            ? $"Bônus aplicados: {upgrades.AcquiredCount}. Gere a próxima oferta."
            : "Escolha uma carta na tela normal. Rerolls infinitos [R].";
    }

    public void ReturnToMenu()
    {
        if (leaving || SceneUtility.GetBuildIndexByScenePath(menuScene) < 0) return;
        leaving = true;
        if (pause != null && pause.IsPaused) pause.ResumeGame();
        LeaderboardSession.Cancel();
        SceneManager.LoadSceneAsync(menuScene, LoadSceneMode.Single);
    }
    private void OnDestroy() { if (root != null) Destroy(root); }
}
