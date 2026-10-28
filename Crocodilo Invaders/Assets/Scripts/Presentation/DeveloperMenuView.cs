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
        DevelopmentUI.Text("Description", panel, font, "TESTE DE CARTAS\nPartida real, mesma tela, animações e bônus.", 17f, 52f);
        mode = DevelopmentUI.Button("OfferMode", panel, font, "", CycleMode);
        rarity = DevelopmentUI.Button("Rarity", panel, font, "", CycleRarity);
        wave = DevelopmentUI.Button("Wave", panel, font, "", CycleWave);
        launch = DevelopmentUI.Button("StartCardTest", panel, font, "INICIAR TESTE DE CARTAS", Launch, 48f);
        DevelopmentUI.Button("Close", panel, font, "VOLTAR  [ESC]", Close, 40f);
        status = DevelopmentUI.Text("Status", panel, font, "F8 abre este painel. Testes não entram no ranking.", 14f, 34f);
        status.color = DevelopmentUI.Muted;
        window.SetActive(false);
        Refresh();
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
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(launch.gameObject);
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
