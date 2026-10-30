using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class DeveloperMenuView : MonoBehaviour
{
    private MainMenuController owner;
    private GameObject root, window;
    private UnityEngine.UI.Button mode, rarity, wave, launch;
    private TMP_Text status;
    private TMP_Text description;
    private GameObject categories, cards, bosses, phases;
    private UnityEngine.UI.Button firstCategory, firstBoss, firstPhase;
    private int page, selectedBoss;
    public bool IsVisible => window != null && window.activeSelf;

    public void Initialize(MainMenuController menu, TMP_FontAsset font)
    {
        owner = menu;
        root = DevelopmentUI.Canvas("DeveloperMenuCanvas");
        var tab = DevelopmentUI.Button("DeveloperTab", root.transform, font, "DEV", Open, 32f);
        DevelopmentUI.Corner(tab, Vector2.one, new Vector2(-16f, -16f), new Vector2(68f, 32f));
        tab.GetComponent<UnityEngine.UI.Image>().color = new Color(.055f, .075f, .06f, .55f);
        tab.GetComponentInChildren<TMP_Text>().color = new Color(.55f, .61f, .51f, .65f);
        var shade = DevelopmentUI.Rect("DeveloperWindow", root.transform, Vector2.zero, Vector2.zero, Vector2.zero);
        shade.anchorMax = Vector2.one;
        shade.offsetMin = shade.offsetMax = Vector2.zero;
        shade.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, .8f);
        window = shade.gameObject;
        RectTransform panel = DevelopmentUI.Panel("DeveloperPanel", shade, new Vector2(640f, 540f), new Vector2(.5f, .5f), Vector2.zero);
        DevelopmentUI.Text("Heading", panel, font, "MODO DESENVOLVEDOR", 24f, 34f);
        description = DevelopmentUI.Text("Description", panel, font, "", 17f, 52f);
        categories = Group("Categories", panel);
        firstCategory = DevelopmentUI.Button("Cards", categories.transform, font, "CARTAS  >", () => ShowPage(1));
        DevelopmentUI.Button("FinalBoss", categories.transform, font, "BOSS FINAL  >", () => ShowPage(2));
        DevelopmentUI.Button("Base", categories.transform, font, "BASE", LaunchBase);
        cards = Group("CardsPage", panel);
        mode = DevelopmentUI.Button("OfferMode", cards.transform, font, "", CycleMode);
        rarity = DevelopmentUI.Button("Rarity", cards.transform, font, "", CycleRarity);
        wave = DevelopmentUI.Button("Wave", cards.transform, font, "", CycleWave);
        launch = DevelopmentUI.Button("StartCardTest", cards.transform, font, "INICIAR TESTE DE CARTAS", Launch, 48f);
        bosses = Group("BossesPage", panel);
        firstBoss = DevelopmentUI.Button("Boss1", bosses.transform, font, "BOSS 1 · GUSINI  >", () => SelectBoss(1));
        DevelopmentUI.Button("Boss2", bosses.transform, font, "BOSS 2 · MACAQUINI  >", () => SelectBoss(2));
        DevelopmentUI.Button("LakeBoss", bosses.transform, font, "SAPENEU CURURU · LAGO", () => { if (owner.BeginDevelopmentBoss(3, 1)) window.SetActive(false); });
        phases = Group("PhasesPage", panel);
        firstPhase = DevelopmentUI.Button("Phase1", phases.transform, font, "FASE 1", () => LaunchBoss(1));
        DevelopmentUI.Button("Phase2", phases.transform, font, "FASE 2", () => LaunchBoss(2));
        DevelopmentUI.Button("Close", panel, font, "VOLTAR  [ESC]", Back, 40f);
        status = DevelopmentUI.Text("Status", panel, font, "F8 abre este painel. Testes não entram no ranking.", 14f, 34f);
        status.color = DevelopmentUI.Muted;
        window.SetActive(false);
        Refresh();
    }

    private static GameObject Group(string name, Transform parent)
    {
        var group = DevelopmentUI.Rect(name, parent, Vector2.zero, new Vector2(.5f, .5f), Vector2.zero);
        var layout = group.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return group.gameObject;
    }

    private void ShowPage(int value)
    {
        page = value;
        categories.SetActive(page == 0); cards.SetActive(page == 1);
        bosses.SetActive(page == 2); phases.SetActive(page == 3);
        description.text = page == 0 ? "DEV · ESCOLHA UM TIPO DE TESTE"
            : page == 1 ? "DEV / CARTAS\nPartida real, mesma tela, animações e bônus."
            : page == 2 ? "DEV / BOSS FINAL\nEscolha o boss."
            : "DEV / BOSS FINAL / " + (selectedBoss == 1 ? "BOSS 1 · GUSINI" : "BOSS 2 · MACAQUINI");
        status.text = "F8 fecha este painel. Testes não entram no ranking.";
        var focus = page == 0 ? firstCategory : page == 1 ? mode : page == 2 ? firstBoss : firstPhase;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(focus.gameObject);
    }

    private void SelectBoss(int value) { selectedBoss = value; ShowPage(3); }
    public void Back() { if (page == 0) Close(); else ShowPage(page == 3 ? 2 : 0); }
    private void LaunchBoss(int phase)
    {
        if (owner.BeginDevelopmentBoss(selectedBoss, phase)) window.SetActive(false);
        else status.text = "Não foi possível iniciar o boss. Confira as cenas do build.";
    }
    private void LaunchBase()
    {
        if (owner.BeginDevelopmentBase()) window.SetActive(false);
        else status.text = "Não foi possível abrir a base. Confira as cenas do build.";
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
        {
            if (IsVisible) Close(); else Open();
        }
    }

    public void Open()
    {
        if (IsVisible || owner == null || !owner.TryOpenDeveloperMenu()) return;
        Refresh();
        window.SetActive(true);
        ShowPage(0);
    }

    public void Close()
    {
        if (!IsVisible) return;
        window.SetActive(false);
        owner.CloseDeveloperMenu();
    }

    private void Launch()
    {
        if (owner.BeginDevelopmentCards()) window.SetActive(false);
        else status.text = "Não foi possível iniciar. Confira a cena e o loading.";
    }
    private void CycleMode() { DevelopmentCardSession.Mode = (DevelopmentCardSession.OfferMode)(((int)DevelopmentCardSession.Mode + 1) % 3); Refresh(); }
    private void CycleRarity() { DevelopmentCardSession.Rarity = (UpgradeRarity)(((int)DevelopmentCardSession.Rarity + 1) % 5); Refresh(); }
    private void CycleWave() { DevelopmentCardSession.CompletedWave = DevelopmentCardSession.CompletedWave == 1 ? 5 : DevelopmentCardSession.CompletedWave == 5 ? 9 : 1; Refresh(); }
    private void Refresh()
    {
        DevelopmentUI.Label(mode, "GERAÇÃO: " + DevelopmentCardSession.ModeLabel);
        DevelopmentUI.Label(rarity, "RARIDADE: " + DevelopmentCardSession.RarityLabel);
        rarity.interactable = DevelopmentCardSession.Mode == DevelopmentCardSession.OfferMode.FixedRarity;
        DevelopmentUI.Label(wave, "PROGRESSO: APÓS A ONDA " + DevelopmentCardSession.CompletedWave);
    }
    private void OnDestroy() { if (root != null) Destroy(root); }
}
